using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWebApp.Models;

public class Book
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Название от 1 до 200 символов")]
    [Display(Name = "Название книги")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Год издания обязателен")]
    [Range(1450, 2100, ErrorMessage = "Год издания от 1450 до текущего")]
    [Display(Name = "Год издания")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Количество экземпляров обязательно")]
    [Range(0, 10000, ErrorMessage = "Количество от 0 до 10000")]
    [Display(Name = "Количество экземпляров")]
    public int TotalCopies { get; set; }

    [Display(Name = "Доступно экземпляров")]
    public int AvailableCopies { get; set; }

    [StringLength(500, ErrorMessage = "Описание не более 500 символов")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    // Поля для файла книги
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }

    // Внешний ключ для категории
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }

    // Связь N:N с авторами
    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}