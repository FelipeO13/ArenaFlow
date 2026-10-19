using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Models;

namespace BoxeGymManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BoxingClass> BoxingClasses => Set<BoxingClass>();
    public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de relacionamentos
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Plan)
            .WithMany(p => p.Students)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Student)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttendanceLog>()
            .HasOne(a => a.Student)
            .WithMany(s => s.AttendanceLogs)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override int SaveChanges()
    {
        SanitizeStrings();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SanitizeStrings();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SanitizeStrings()
    {
        foreach (var entry in ChangeTracker.Entries<Student>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.RegistrationCode ??= string.Empty;
                entry.Entity.FullName ??= string.Empty;
                entry.Entity.CPF ??= string.Empty;
                entry.Entity.Phone ??= string.Empty;
                entry.Entity.Email ??= string.Empty;
                entry.Entity.Address ??= string.Empty;
                entry.Entity.EmergencyContactName ??= string.Empty;
                entry.Entity.EmergencyContactPhone ??= string.Empty;
                entry.Entity.WeightCategory ??= string.Empty;
                entry.Entity.MedicalNotes ??= string.Empty;
                entry.Entity.StatusReason ??= string.Empty;
                if (string.IsNullOrWhiteSpace(entry.Entity.Stance)) entry.Entity.Stance = "Destro (Orthodox)";
            }
        }

        foreach (var entry in ChangeTracker.Entries<Plan>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.Name ??= string.Empty;
                entry.Entity.Description ??= string.Empty;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Payment>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.ReferenceMonthYear ??= string.Empty;
                entry.Entity.ReceiptCode ??= string.Empty;
                entry.Entity.Notes ??= string.Empty;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BoxingClass>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.Title ??= string.Empty;
                entry.Entity.CoachName ??= string.Empty;
                entry.Entity.LevelRequirement ??= "Todos os níveis";
                entry.Entity.RingOrRoom ??= "Ringue Principal";
            }
        }

        foreach (var entry in ChangeTracker.Entries<AttendanceLog>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.Message ??= "Entrada autorizada";
            }
        }
    }

    public static void SeedData(AppDbContext context)
    {
        if (context.Plans.Any()) return; // Já foi semeado

        // 1. Planos
        var planFit = new Plan
        {
            Name = "Boxe Fitness & Aeróbico",
            Description = "Foco em queima calórica, coordenação e fundamentos sem contato direto.",
            Price = 149.90m,
            BillingCycle = PlanBillingCycle.Mensal,
            DaysPerWeek = 2,
            IncludesSparring = false,
            IsActive = true
        };

        var planNobreArte = new Plan
        {
            Name = "Nobre Arte Livre (Diário)",
            Description = "Acesso livre a todos os horários de treino de boxe, saco de pancadas e musculação funcional.",
            Price = 229.90m,
            BillingCycle = PlanBillingCycle.Mensal,
            DaysPerWeek = 0, // Livre
            IncludesSparring = true,
            IsActive = true
        };

        var planCompeticao = new Plan
        {
            Name = "Boxe Sparring & Competição",
            Description = "Treinos táticos, preparação física avançada de atletas e sessões com luva de sparring.",
            Price = 289.90m,
            BillingCycle = PlanBillingCycle.Mensal,
            DaysPerWeek = 0, // Livre
            IncludesSparring = true,
            IsActive = true
        };

        var planTrimestral = new Plan
        {
            Name = "Plano Trimestral Gold",
            Description = "Plano trimestral com desconto exclusivo e passe livre completo.",
            Price = 589.00m,
            BillingCycle = PlanBillingCycle.Trimestral,
            DaysPerWeek = 0,
            IncludesSparring = true,
            IsActive = true
        };

        context.Plans.AddRange(planFit, planNobreArte, planCompeticao, planTrimestral);
        context.SaveChanges();

        // 2. Horários de Aulas
        var classes = new List<BoxingClass>
        {
            new() { Title = "Fundamentos de Boxe & Guarda", CoachName = "Mestre Popó", DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(8, 0, 0), MaxCapacity = 16, LevelRequirement = "Iniciante", RingOrRoom = "Ringue 1" },
            new() { Title = "Saco de Pancadas & Manopla", CoachName = "Prof. Rogério", DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(18, 30, 0), EndTime = new TimeSpan(19, 30, 0), MaxCapacity = 14, LevelRequirement = "Todos os níveis", RingOrRoom = "Área de Sacos" },
            new() { Title = "Sparring Técnico Supervisionado", CoachName = "Mestre Popó", DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(20, 30, 0), MaxCapacity = 10, LevelRequirement = "Intermediário / Sparring", RingOrRoom = "Ringue Principal" },
            new() { Title = "Boxe Funcional & Cardio", CoachName = "Prof. Bruna", DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 0, 0), MaxCapacity = 18, LevelRequirement = "Todos os níveis", RingOrRoom = "Tatame / Espaço Livre" },
            new() { Title = "Combinações & Esquiva", CoachName = "Prof. Rogério", DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(19, 0, 0), MaxCapacity = 14, LevelRequirement = "Intermediário", RingOrRoom = "Ringue 1" },
            new() { Title = "Treino Livre de Ringue", CoachName = "Monitor Plantonista", DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(18, 30, 0), MaxCapacity = 12, LevelRequirement = "Todos com autorização", RingOrRoom = "Ringue Principal" },
            new() { Title = "Clínica de Sparring & Tática", CoachName = "Mestre Popó", DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(20, 30, 0), MaxCapacity = 12, LevelRequirement = "Avançado / Atletas", RingOrRoom = "Ringue Principal" },
            new() { Title = "Aulão de Boxe de Sábado", CoachName = "Equipe Nobre Arte", DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 30, 0), MaxCapacity = 25, LevelRequirement = "Todos os níveis", RingOrRoom = "Academia Completa" }
        };
        context.BoxingClasses.AddRange(classes);
        context.SaveChanges();

        // 3. Alunos de Exemplo
        var student1 = new Student
        {
            RegistrationCode = "BX-1001",
            FullName = "Lucas 'Canhoto' Andrade",
            CPF = "123.456.789-01",
            BirthDate = new DateTime(1998, 5, 14),
            Phone = "(11) 98765-4321",
            Email = "lucas.andrade@boxe.com",
            Address = "Rua dos Campeões, 140 - São Paulo, SP",
            EmergencyContactName = "Mariana Andrade (Irmã)",
            EmergencyContactPhone = "(11) 97123-4567",
            WeightKg = 69.5m,
            WeightCategory = "Peso Meio-Médio (até 69kg)",
            ExperienceLevel = BoxingLevel.Sparring,
            Stance = "Canhoto (Southpaw)",
            MedicalNotes = "Atestado válido entregue. Sem restrições.",
            HasMedicalCertificate = true,
            PlanId = planNobreArte.Id,
            PlanStartDate = DateTime.Today.AddMonths(-2),
            PlanEndDate = DateTime.Today.AddDays(12),
            IsActive = true
        };

        var student2 = new Student
        {
            RegistrationCode = "BX-1002",
            FullName = "Beatriz Helena Santos",
            CPF = "234.567.890-12",
            BirthDate = new DateTime(2001, 9, 23),
            Phone = "(11) 99123-8877",
            Email = "beatriz.santos@email.com",
            Address = "Av. do Esporte, 850 - São Paulo, SP",
            EmergencyContactName = "Roberto Santos (Pai)",
            EmergencyContactPhone = "(11) 98822-3344",
            WeightKg = 57.0m,
            WeightCategory = "Peso Pena (até 57kg)",
            ExperienceLevel = BoxingLevel.CompetidorProfissional,
            Stance = "Destro (Orthodox)",
            MedicalNotes = "Exame médico cardiovascular regularizado.",
            HasMedicalCertificate = true,
            PlanId = planCompeticao.Id,
            PlanStartDate = DateTime.Today.AddMonths(-1),
            PlanEndDate = DateTime.Today.AddDays(25),
            IsActive = true
        };

        var student3 = new Student
        {
            RegistrationCode = "BX-1003",
            FullName = "Marcos Vinicius de Souza",
            CPF = "345.678.901-23",
            BirthDate = new DateTime(1992, 11, 2),
            Phone = "(11) 97654-3210",
            Email = "marcos.souza@email.com",
            Address = "Rua Augusta, 400 - São Paulo, SP",
            EmergencyContactName = "Fernanda Souza (Esposa)",
            EmergencyContactPhone = "(11) 99988-7766",
            WeightKg = 81.2m,
            WeightCategory = "Peso Meio-Pesado (até 81kg)",
            ExperienceLevel = BoxingLevel.Iniciante,
            Stance = "Destro (Orthodox)",
            MedicalNotes = "Condromalácia patelar grau 1 no joelho esquerdo. Evitar saltos de corda prolongados.",
            HasMedicalCertificate = true,
            PlanId = planFit.Id,
            PlanStartDate = DateTime.Today.AddMonths(-3),
            PlanEndDate = DateTime.Today.AddDays(-5), // Venceu há 5 dias!
            IsActive = true
        };

        context.Students.AddRange(student1, student2, student3);
        context.SaveChanges();

        // 4. Pagamentos
        var payments = new List<Payment>
        {
            new()
            {
                StudentId = student1.Id,
                Amount = 229.90m,
                DueDate = DateTime.Today.AddDays(-18),
                PaidDate = DateTime.Today.AddDays(-18),
                Status = PaymentStatus.Pago,
                Method = PaymentMethod.Pix,
                ReferenceMonthYear = $"{DateTime.Today.Month:D2}/{DateTime.Today.Year}",
                ReceiptCode = "REC-202610-101",
                Notes = "Pago via QR Code Pix na recepção"
            },
            new()
            {
                StudentId = student2.Id,
                Amount = 289.90m,
                DueDate = DateTime.Today.AddDays(25),
                PaidDate = DateTime.Today.AddDays(-5),
                Status = PaymentStatus.Pago,
                Method = PaymentMethod.CartaoCredito,
                ReferenceMonthYear = $"{DateTime.Today.Month:D2}/{DateTime.Today.Year}",
                ReceiptCode = "REC-202610-102",
                Notes = "Cartão de Crédito - 1x"
            },
            new()
            {
                StudentId = student3.Id,
                Amount = 149.90m,
                DueDate = DateTime.Today.AddDays(-5),
                PaidDate = null,
                Status = PaymentStatus.Atrasado,
                Method = PaymentMethod.Pix,
                ReferenceMonthYear = $"{DateTime.Today.Month:D2}/{DateTime.Today.Year}",
                ReceiptCode = "REC-202610-103",
                Notes = "Aguardando envio do comprovante Pix"
            }
        };
        context.Payments.AddRange(payments);
        context.SaveChanges();

        // 5. Histórico de Entradas / Saídas
        var attendances = new List<AttendanceLog>
        {
            new()
            {
                StudentId = student1.Id,
                EntryTime = DateTime.Today.AddHours(7).AddMinutes(12),
                ExitTime = DateTime.Today.AddHours(8).AddMinutes(35),
                AccessStatus = AccessStatus.Autorizado,
                Message = "Entrada liberada - Plano regular"
            },
            new()
            {
                StudentId = student2.Id,
                EntryTime = DateTime.Now.AddHours(-1).AddMinutes(-15),
                ExitTime = null, // Treinando no momento!
                AccessStatus = AccessStatus.Autorizado,
                Message = "Entrada liberada - Atleta Competição"
            }
        };
        context.AttendanceLogs.AddRange(attendances);
        context.SaveChanges();
    }
}
