using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure SQLite Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=boxegym.db"));

var app = builder.Build();

// Garantir criação do banco e dados iniciais
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN Status INTEGER NOT NULL DEFAULT 0;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN StatusReason TEXT NOT NULL DEFAULT '';"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN StatusChangedAt TEXT NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN PhotoPath TEXT NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN Modality INTEGER NOT NULL DEFAULT 0;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN CNH TEXT NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN GuardianName TEXT NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN GuardianPhone TEXT NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN GuardianAddress TEXT NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN HeightCm INTEGER NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN ReachCm INTEGER NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Students ADD COLUMN ModalityNotes TEXT NULL;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("UPDATE Students SET Status = 2 WHERE IsActive = 0 AND Status = 0;"); } catch { }
    try { dbContext.Database.ExecuteSqlRaw("UPDATE AttendanceLogs SET ExitTime = EntryTime WHERE AccessStatus IN (2, 3) AND ExitTime IS NULL;"); } catch { }
    AppDbContext.SeedData(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
