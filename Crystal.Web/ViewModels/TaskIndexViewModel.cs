using Crystal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Crystal.Web.ViewModels;

public class TaskIndexViewModel
{
    public IEnumerable<TaskItem> Tasks { get; set; } = [];
    public string? Search { get; set; }
    public int? StatusId { get; set; }
    public TaskPriority? Priority { get; set; }
    public int? ProjectId { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public List<SelectListItem> Statuses { get; set; } = [];
    public List<SelectListItem> Projects { get; set; } = [];
}
