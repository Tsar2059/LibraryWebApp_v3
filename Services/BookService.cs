using Microsoft.EntityFrameworkCore;
using LibraryWebApp.Data;
using LibraryWebApp.Models;
using LibraryWebApp.Validators;
using FluentValidation;

namespace LibraryWebApp.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<BookService> _logger;
    private readonly BookValidator _validator = new();

    public BookService(LibraryDbContext context, ILogger<BookService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Book> AddBookAsync(Book book, List<int> authorIds, IFormFile? bookFile = null)
    {
        var validationResult = await _validator.ValidateAsync(book);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors.First().ErrorMessage);
        }

        // Сохраняем файл, если он был загружен
        if (bookFile != null)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "books");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(bookFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bookFile.CopyToAsync(stream);
            }

            book.FilePath = uniqueFileName;
            book.FileName = bookFile.FileName;
            book.FileType = Path.GetExtension(bookFile.FileName).ToLower();
        }

        book.AvailableCopies = book.TotalCopies;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        foreach (var authorId in authorIds)
        {
            _context.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("Добавлена книга {Title}", book.Title);
        return book;
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.Category)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
    {
        return await _context.Authors.ToListAsync();
    }

    public async Task<Author?> GetAuthorByIdAsync(int id)
    {
        return await _context.Authors
            .Include(a => a.BookAuthors)
            .ThenInclude(ba => ba.Book)
            .ThenInclude(b => b.Category)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<byte[]?> GetBookFileAsync(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null || string.IsNullOrEmpty(book.FilePath))
            return null;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "books", book.FilePath);
        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return false;

        // Удаляем файл книги, если он есть
        if (!string.IsNullOrEmpty(book.FilePath))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "books", book.FilePath);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Удалена книга {Title}", book.Title);
        return true;
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var author = await _context.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null) return false;

        if (!string.IsNullOrEmpty(author.PhotoPath))
        {
            var fileName = Path.GetFileName(author.PhotoPath);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "authors", fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllBooksAsync();

        return await _context.Books
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.Category)
            .Where(b => b.Title.Contains(searchTerm) ||
                        b.BookAuthors.Any(ba => ba.Author.Name.Contains(searchTerm)))
            .ToListAsync();
    }
}