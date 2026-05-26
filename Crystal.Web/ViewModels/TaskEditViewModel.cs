using Crystal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Crystal.Web.ViewModels;

public class TaskEditViewModel
{
    public int Id { get; set; }
    public int? CreatedById { get; set; }

    public TaskItem Task { get; set; } = new();
    public List<TaskFieldInputViewModel> Fields { get; set; } = [];
    public List<SelectListItem> Users { get; set; } = [];
    public List<SelectListItem> Projects { get; set; } = [];
    public List<SelectListItem> Queues { get; set; } = [];
}
