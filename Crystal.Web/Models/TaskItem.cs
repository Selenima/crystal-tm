using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

// главная сущность задачи, тут хранится название статус проект исполнитель и доп поля
public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Название")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите проект.")]
    [Display(Name = "Проект")]
    public int ProjectEntityId { get; set; }

    [Display(Name = "Очередь")]
    public int? WorkQueueId { get; set; }

    [Display(Name = "Исполнитель")]
    public int? AssignedToId { get; set; }

    [Display(Name = "Создатель")]
    public int CreatedById { get; set; }

    [Display(Name = "Статус")]
    public int TaskStatusEntityId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дедлайн")]
    public DateTime? Deadline { get; set; }

    [Required]
    [Display(Name = "Приоритет")]
    public TaskPriority Priority { get; set; }

    public ProjectEntity? ProjectEntity { get; set; }
    public WorkQueue? WorkQueue { get; set; }
    public User? AssignedTo { get; set; }
    public User? CreatedBy { get; set; }
    public TaskStatusEntity? TaskStatusEntity { get; set; }
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskFieldValue> FieldValues { get; set; } = new List<TaskFieldValue>();
}
