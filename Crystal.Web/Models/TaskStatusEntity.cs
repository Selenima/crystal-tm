using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

public class TaskStatusEntity
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    [Display(Name = "Статус")]
    public string Name { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Цвет")]
    public string ColorClass { get; set; } = "secondary";

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskStatusTransition> AvailableFromTransitions { get; set; } = new List<TaskStatusTransition>();
    public ICollection<TaskStatusTransition> AvailableToTransitions { get; set; } = new List<TaskStatusTransition>();
}
