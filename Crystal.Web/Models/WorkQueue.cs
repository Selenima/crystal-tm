using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

public class WorkQueue
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Название очереди")]
    public string Name { get; set; } = string.Empty;

    [StringLength(400)]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
