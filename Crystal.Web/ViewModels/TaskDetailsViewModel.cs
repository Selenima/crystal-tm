using Crystal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Crystal.Web.ViewModels;

public class TaskDetailsViewModel
{
    public TaskItem Task { get; set; } = new();
    public List<SelectListItem> AllowedTransitions { get; set; } = [];
}
