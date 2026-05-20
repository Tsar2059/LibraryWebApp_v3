using FluentValidation;
using LibraryWebApp.Models;
using System.Text.RegularExpressions;

namespace LibraryWebApp.Validators;

public class BookValidator : AbstractValidator<Book>
{
    private static readonly Regex SqlInjectionPattern = new(
        @"\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|CREATE|TRUNCATE|EXEC|EXECUTE|MERGE|REPLACE|DECLARE|CURSOR|FETCH|INTO|WHERE|FROM|ADD|MODIFY|RENAME|BACKUP|RESTORE|xp_|sp_)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public BookValidator()
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Название книги обязательно")
            .Length(1, 200).WithMessage("Название должно быть от 1 до 200 символов")
            .Must(NotContainSqlInjection).WithMessage("Название содержит запрещённые SQL-команды");

        // ISBN полностью удалён

        RuleFor(b => b.Year)
            .InclusiveBetween(1450, DateTime.UtcNow.Year)
            .WithMessage($"Год издания должен быть между 1450 и {DateTime.UtcNow.Year}");

        RuleFor(b => b.TotalCopies)
            .InclusiveBetween(0, 10000)
            .WithMessage("Количество экземпляров от 0 до 10000");

        RuleFor(b => b.Description)
            .MaximumLength(500).WithMessage("Описание не более 500 символов")
            .Must(NotContainSqlInjection).When(b => !string.IsNullOrEmpty(b.Description))
            .WithMessage("Описание содержит запрещённые SQL-команды");
    }

    private static bool NotContainSqlInjection(string input)
    {
        return string.IsNullOrEmpty(input) || !SqlInjectionPattern.IsMatch(input);
    }
}