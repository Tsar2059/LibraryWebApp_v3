using System.ComponentModel.DataAnnotations;

namespace LibraryWebApp.Models;

public class Author
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Имя автора обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 100 символов")]
    [Display(Name = "Имя автора")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Биография не более 1000 символов")]
    [Display(Name = "Биография")]
    public string? Biography { get; set; }

    [Display(Name = "Фото автора")]
    public string? PhotoPath { get; set; }

    // Связь N:N с книгами
    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}