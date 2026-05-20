using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWebApp.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Название категории")]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<Book>? Books { get; set; }
}