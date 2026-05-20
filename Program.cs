using Microsoft.EntityFrameworkCore;
using LibraryWebApp.Data;
using LibraryWebApp.Services;
using LibraryWebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Регистрируем DbContext как Scoped
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрируем DbContextFactory как Scoped
builder.Services.AddDbContextFactory<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

// Регистрируем сервисы
builder.Services.AddScoped<IBookService, BookService>();

// Регистрируем AuthService как Singleton (ВАЖНО: ДО builder.Build())
builder.Services.AddSingleton<AuthService>();

// Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Применяем миграции при запуске
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Эндпоинт для чтения книги онлайн
app.MapGet("/books/read/{id:int}", async (int id, IBookService bookService) =>
{
    var book = await bookService.GetBookByIdAsync(id);
    if (book == null)
        return Results.NotFound("❌ Книга не найдена");

    if (string.IsNullOrEmpty(book.FilePath))
        return Results.NotFound("📭 Файл книги отсутствует в базе данных");

    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "books", book.FilePath);
    
    if (!File.Exists(filePath))
        return Results.NotFound("📭 Файл книги не найден на сервере. Пожалуйста, загрузите файл заново.");

    var fileBytes = await File.ReadAllBytesAsync(filePath);
    var contentType = book.FileType?.ToLower() switch
    {
        ".pdf" => "application/pdf",
        ".epub" => "application/epub+zip",
        ".fb2" => "application/fb2",
        _ => "application/octet-stream"
    };

    return Results.File(fileBytes, contentType);
});

// Эндпоинт для скачивания книги
app.MapGet("/books/download/{id:int}", async (int id, IBookService bookService) =>
{
    var book = await bookService.GetBookByIdAsync(id);
    if (book == null)
        return Results.NotFound("❌ Книга не найдена");

    if (string.IsNullOrEmpty(book.FilePath))
        return Results.NotFound("📭 Файл книги отсутствует в базе данных");

    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "books", book.FilePath);
    
    if (!File.Exists(filePath))
        return Results.NotFound("📭 Файл книги не найден на сервере. Пожалуйста, загрузите файл заново.");

    var fileBytes = await File.ReadAllBytesAsync(filePath);
    return Results.File(fileBytes, "application/octet-stream", book.FileName ?? "book.pdf");
});

app.Run();