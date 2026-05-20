using LibraryWebApp.Models;

namespace LibraryWebApp.Services;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> AddBookAsync(Book book, List<int> authorIds, IFormFile? bookFile = null);
    Task<IEnumerable<Author>> GetAllAuthorsAsync();
    Task<Author?> GetAuthorByIdAsync(int id);
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
    Task<byte[]?> GetBookFileAsync(int bookId);
    Task<bool> DeleteBookAsync(int id);
}