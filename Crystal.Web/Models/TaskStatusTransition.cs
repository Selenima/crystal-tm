namespace Crystal.Web.Models;

public class TaskStatusTransition
{
    public int Id { get; set; }
    public int FromTaskStatusEntityId { get; set; }
    public int ToTaskStatusEntityId { get; set; }

    public TaskStatusEntity? FromTaskStatusEntity { get; set; }
    public TaskStatusEntity? ToTaskStatusEntity { get; set; }
}
