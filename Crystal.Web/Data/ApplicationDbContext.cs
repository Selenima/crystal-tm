using Crystal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<UserProjectAccess> UserProjectAccesses => Set<UserProjectAccess>();
    public DbSet<WorkQueue> WorkQueues => Set<WorkQueue>();
    public DbSet<TaskStatusEntity> TaskStatuses => Set<TaskStatusEntity>();
    public DbSet<TaskStatusTransition> TaskStatusTransitions => Set<TaskStatusTransition>();
    public DbSet<TaskFieldDefinition> TaskFieldDefinitions => Set<TaskFieldDefinition>();
    public DbSet<TaskFieldValue> TaskFieldValues => Set<TaskFieldValue>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<TaskStatusEntity>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<TaskFieldDefinition>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<UserProjectAccess>()
            .HasIndex(x => new { x.UserId, x.ProjectEntityId })
            .IsUnique();
        modelBuilder.Entity<TaskStatusTransition>()
            .HasIndex(x => new { x.FromTaskStatusEntityId, x.ToTaskStatusEntityId })
            .IsUnique();
        modelBuilder.Entity<TaskFieldValue>()
            .HasIndex(x => new { x.TaskItemId, x.TaskFieldDefinitionId })
            .IsUnique();

        modelBuilder.Entity<UserProjectAccess>()
            .HasOne(x => x.User)
            .WithMany(x => x.ProjectAccesses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProjectAccess>()
            .HasOne(x => x.ProjectEntity)
            .WithMany(x => x.UserAccesses)
            .HasForeignKey(x => x.ProjectEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(x => x.WorkQueue)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.WorkQueueId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TaskItem>()
            .HasOne(x => x.ProjectEntity)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ProjectEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(x => x.WorkQueue)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.WorkQueueId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TaskItem>()
            .HasOne(x => x.AssignedTo)
            .WithMany(x => x.AssignedTasks)
            .HasForeignKey(x => x.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TaskItem>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedTasks)
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(x => x.TaskStatusEntity)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.TaskStatusEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskStatusTransition>()
            .HasOne(x => x.FromTaskStatusEntity)
            .WithMany(x => x.AvailableFromTransitions)
            .HasForeignKey(x => x.FromTaskStatusEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskStatusTransition>()
            .HasOne(x => x.ToTaskStatusEntity)
            .WithMany(x => x.AvailableToTransitions)
            .HasForeignKey(x => x.ToTaskStatusEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskComment>()
            .HasOne(x => x.TaskItem)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskComment>()
            .HasOne(x => x.User)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskFieldValue>()
            .HasOne(x => x.TaskItem)
            .WithMany(x => x.FieldValues)
            .HasForeignKey(x => x.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskFieldValue>()
            .HasOne(x => x.TaskFieldDefinition)
            .WithMany(x => x.Values)
            .HasForeignKey(x => x.TaskFieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectEntity>().HasData(
            new ProjectEntity { Id = 1, Name = "Internal Portal", Description = "Внутренний проект для сотрудников." },
            new ProjectEntity { Id = 2, Name = "Demo Release", Description = "Учебная демонстрация для сдачи." }
        );

        modelBuilder.Entity<WorkQueue>().HasData(
            new WorkQueue { Id = 1, Name = "Backend Queue", Description = "Разработчики backend." },
            new WorkQueue { Id = 2, Name = "QA Queue", Description = "Команда тестирования." }
        );

        modelBuilder.Entity<TaskStatusEntity>().HasData(
            new TaskStatusEntity { Id = 1, Name = "New", ColorClass = "secondary" },
            new TaskStatusEntity { Id = 2, Name = "In Progress", ColorClass = "primary" },
            new TaskStatusEntity { Id = 3, Name = "Done", ColorClass = "success" }
        );

        modelBuilder.Entity<TaskStatusTransition>().HasData(
            new TaskStatusTransition { Id = 1, FromTaskStatusEntityId = 1, ToTaskStatusEntityId = 2 },
            new TaskStatusTransition { Id = 2, FromTaskStatusEntityId = 2, ToTaskStatusEntityId = 3 },
            new TaskStatusTransition { Id = 3, FromTaskStatusEntityId = 2, ToTaskStatusEntityId = 1 }
        );

        modelBuilder.Entity<TaskFieldDefinition>().HasData(
            new TaskFieldDefinition { Id = 1, Name = "Estimate", FieldType = TaskFieldType.Number, IsRequired = true, IsBase = true },
            new TaskFieldDefinition { Id = 2, Name = "Due Note", FieldType = TaskFieldType.Text, IsRequired = false, IsBase = true },
            new TaskFieldDefinition { Id = 3, Name = "Customer", FieldType = TaskFieldType.Text, IsRequired = false, IsBase = false }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "Crystal",
                Email = "admin@crystal.local",
                Position = "Administrator",
                Department = "Management",
                PasswordHash = SeederPassword.Hash("admin123"),
                IsAdmin = true,
                WorkQueueId = 1
            },
            new User
            {
                Id = 2,
                FirstName = "Anna",
                LastName = "Smirnova",
                Email = "anna@crystal.local",
                Position = "Backend Developer",
                Department = "Development",
                PasswordHash = SeederPassword.Hash("anna123"),
                IsAdmin = false,
                WorkQueueId = 1
            },
            new User
            {
                Id = 3,
                FirstName = "Oleg",
                LastName = "Sidorov",
                Email = "oleg@crystal.local",
                Position = "QA Engineer",
                Department = "QA",
                PasswordHash = SeederPassword.Hash("oleg123"),
                IsAdmin = false,
                WorkQueueId = 2
            }
        );

        modelBuilder.Entity<UserProjectAccess>().HasData(
            new UserProjectAccess { Id = 1, UserId = 1, ProjectEntityId = 1 },
            new UserProjectAccess { Id = 2, UserId = 1, ProjectEntityId = 2 },
            new UserProjectAccess { Id = 3, UserId = 2, ProjectEntityId = 2 },
            new UserProjectAccess { Id = 4, UserId = 3, ProjectEntityId = 1 }
        );

        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = 1,
                Title = "Подготовить структуру проекта",
                Description = "Создать базовую MVC структуру и подключить PostgreSQL.",
                ProjectEntityId = 2,
                WorkQueueId = 1,
                CreatedById = 1,
                AssignedToId = 2,
                TaskStatusEntityId = 2,
                Deadline = new DateTime(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc),
                Priority = TaskPriority.High
            },
            new TaskItem
            {
                Id = 2,
                Title = "Составить тест-кейсы",
                Description = "Подготовить краткий список проверок для релиза.",
                ProjectEntityId = 1,
                WorkQueueId = 2,
                CreatedById = 1,
                AssignedToId = 3,
                TaskStatusEntityId = 1,
                Deadline = new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc),
                Priority = TaskPriority.Medium
            }
        );

        modelBuilder.Entity<TaskComment>().HasData(
            new TaskComment { Id = 1, TaskItemId = 1, UserId = 1, Comment = "Главное - не усложнять решение.", CreatedAt = new DateTime(2026, 4, 15, 10, 0, 0, DateTimeKind.Utc) },
            new TaskComment { Id = 2, TaskItemId = 1, UserId = 2, Comment = "Начала перенос на PostgreSQL.", CreatedAt = new DateTime(2026, 4, 15, 11, 15, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<TaskFieldValue>().HasData(
            new TaskFieldValue { Id = 1, TaskItemId = 1, TaskFieldDefinitionId = 1, Value = "8" },
            new TaskFieldValue { Id = 2, TaskItemId = 1, TaskFieldDefinitionId = 2, Value = "Нужно закончить до презентации" },
            new TaskFieldValue { Id = 3, TaskItemId = 2, TaskFieldDefinitionId = 3, Value = "Учебная группа" }
        );
    }
}
