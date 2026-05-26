using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

public class TaskComment
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public int UserId { get; set; }

    [Required]
    [StringLength(400)]
    [Display(Name = "Комментарий")]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TaskItem? TaskItem { get; set; }
    public User? User { get; set; }
}
