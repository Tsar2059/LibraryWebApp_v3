using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWebApp.Models;

public class Loan
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int ReaderId { get; set; }

    [Required]
    [Display(Name = "Дата выдачи")]
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Display(Name = "Срок возврата")]
    public DateTime DueDate { get; set; }

    [Display(Name = "Дата возврата")]
    public DateTime? ReturnDate { get; set; }

    [StringLength(500)]
    [Display(Name = "Примечания")]
    public string? Notes { get; set; }

    // Навигационные свойства
    [ForeignKey(nameof(BookId))]
    public virtual Book? Book { get; set; }

    [ForeignKey(nameof(ReaderId))]
    public virtual Reader? Reader { get; set; }
}