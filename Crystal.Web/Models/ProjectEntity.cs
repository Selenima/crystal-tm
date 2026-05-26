using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

public class ProjectEntity
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Название проекта")]
    public string Name { get; set; } = string.Empty;

    [StringLength(400)]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    public ICollection<UserProjectAccess> UserAccesses { get; set; } = new List<UserProjectAccess>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
