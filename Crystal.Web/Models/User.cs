using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Должность")]
    public string Position { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Отдел")]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Display(Name = "Администратор")]
    public bool IsAdmin { get; set; }

    [Display(Name = "Очередь")]
    public int? WorkQueueId { get; set; }

    public WorkQueue? WorkQueue { get; set; }
    public ICollection<UserProjectAccess> ProjectAccesses { get; set; } = new List<UserProjectAccess>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();

    public string FullName => $"{FirstName} {LastName}";
}
