namespace Crystal.Web.Models;

public class UserProjectAccess
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProjectEntityId { get; set; }

    public User? User { get; set; }
    public ProjectEntity? ProjectEntity { get; set; }
}
