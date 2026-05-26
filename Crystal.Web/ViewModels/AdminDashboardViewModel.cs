using Crystal.Web.Models;

namespace Crystal.Web.ViewModels;

public class AdminDashboardViewModel
{
    public List<User> Users { get; set; } = [];
    public List<ProjectEntity> Projects { get; set; } = [];
    public List<WorkQueue> Queues { get; set; } = [];
    public List<TaskStatusEntity> Statuses { get; set; } = [];
    public List<TaskStatusTransition> Transitions { get; set; } = [];
    public List<TaskFieldDefinition> FieldDefinitions { get; set; } = [];
}
