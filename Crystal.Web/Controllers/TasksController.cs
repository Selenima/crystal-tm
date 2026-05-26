using System.Security.Claims;
using Crystal.Web.Data;
using Crystal.Web.Models;
using Crystal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Web.Controllers;

[Authorize]
public class TasksController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 6;

    public async Task<IActionResult> Index(string? search, int? statusId, TaskPriority? priority, int? projectId, int page = 1)
    {
        return View(await BuildIndexModel(search, statusId, priority, projectId, page));
    }

    public async Task<IActionResult> Create()
    {
        var availableProjectIds = await GetAvailableProjectIds();
        if (availableProjectIds.Count == 0)
        {
            TempData["TaskError"] = "У вас нет доступа ни к одному проекту.";
            return RedirectToAction(nameof(Index));
        }

        var defaultStatusId = await context.TaskStatuses
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();

        return View(await BuildTaskForm(new TaskEditViewModel
        {
            Task = new TaskItem
            {
                Deadline = DateTime.Today.AddDays(7),
                Priority = TaskPriority.Medium,
                TaskStatusEntityId = defaultStatusId
            }
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskEditViewModel model)
    {
        if (!await HasProjectAccess(model.Task.ProjectEntityId))
        {
            return Forbid();
        }

        await ValidateFields(model);
        if (!ModelState.IsValid)
        {
            return View(await BuildTaskForm(model));
        }

        model.Task.CreatedById = GetCurrentUserId();
        model.Task.TaskStatusEntityId = await context.TaskStatuses
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();

        context.Tasks.Add(model.Task);
        await context.SaveChangesAsync();
        await SaveFieldValues(model.Task.Id, model.Fields);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var task = await context.Tasks
            .Include(x => x.FieldValues)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (task == null || !await HasProjectAccess(task.ProjectEntityId))
        {
            return NotFound();
        }

        var model = new TaskEditViewModel
        {
            Id = task.Id,
            CreatedById = task.CreatedById,
            Task = task
        };

        return View(await BuildTaskForm(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskEditViewModel model)
    {
        if (id != model.Task.Id)
        {
            return NotFound();
        }

        var existingTask = await context.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (existingTask == null || !await HasProjectAccess(existingTask.ProjectEntityId) || !await HasProjectAccess(model.Task.ProjectEntityId))
        {
            return NotFound();
        }

        await ValidateFields(model);
        if (!ModelState.IsValid)
        {
            return View(await BuildTaskForm(model));
        }

        model.Task.CreatedById = model.CreatedById ?? GetCurrentUserId();
        model.Task.TaskStatusEntityId = existingTask.TaskStatusEntityId;
        context.Update(model.Task);
        await context.SaveChangesAsync();
        await SaveFieldValues(model.Task.Id, model.Fields);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await context.Tasks
            .Include(x => x.ProjectEntity)
            .Include(x => x.WorkQueue)
            .Include(x => x.AssignedTo)
            .Include(x => x.CreatedBy)
            .Include(x => x.TaskStatusEntity)
            .Include(x => x.Comments).ThenInclude(x => x.User)
            .Include(x => x.FieldValues).ThenInclude(x => x.TaskFieldDefinition)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (task == null || !await HasProjectAccess(task.ProjectEntityId))
        {
            return NotFound();
        }

        var allowedTransitions = await context.TaskStatusTransitions
            .Where(x => x.FromTaskStatusEntityId == task.TaskStatusEntityId)
            .Include(x => x.ToTaskStatusEntity)
            .OrderBy(x => x.ToTaskStatusEntity!.Name)
            .Select(x => new SelectListItem(x.ToTaskStatusEntity!.Name, x.ToTaskStatusEntityId.ToString()))
            .ToListAsync();

        return View(new TaskDetailsViewModel
        {
            Task = task,
            AllowedTransitions = allowedTransitions
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task != null && await HasProjectAccess(task.ProjectEntityId))
        {
            context.Tasks.Remove(task);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Filter(string? search, int? statusId, TaskPriority? priority, int? projectId, int page = 1)
    {
        return PartialView("_TaskTable", await BuildIndexModel(search, statusId, priority, projectId, page));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, int statusId)
    {
        var task = await context.Tasks.FirstOrDefaultAsync(x => x.Id == id);
        if (task == null || !await HasProjectAccess(task.ProjectEntityId))
        {
            return NotFound();
        }

        var transitionExists = await context.TaskStatusTransitions.AnyAsync(x =>
            x.FromTaskStatusEntityId == task.TaskStatusEntityId &&
            x.ToTaskStatusEntityId == statusId);

        if (!transitionExists)
        {
            TempData["StatusError"] = "Переход в выбранный статус запрещен.";
            return RedirectToAction(nameof(Details), new { id });
        }

        task.TaskStatusEntityId = statusId;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Comments(int id)
    {
        var taskProjectId = await context.Tasks
            .Where(x => x.Id == id)
            .Select(x => (int?)x.ProjectEntityId)
            .FirstOrDefaultAsync();

        if (!taskProjectId.HasValue || !await HasProjectAccess(taskProjectId.Value))
        {
            return NotFound();
        }

        var comments = await context.TaskComments
            .Where(x => x.TaskItemId == id)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return PartialView("_CommentsList", comments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int taskId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            Response.StatusCode = 400;
            return Content("Комментарий обязателен.");
        }

        var taskProjectId = await context.Tasks
            .Where(x => x.Id == taskId)
            .Select(x => (int?)x.ProjectEntityId)
            .FirstOrDefaultAsync();

        if (!taskProjectId.HasValue || !await HasProjectAccess(taskProjectId.Value))
        {
            return NotFound();
        }

        context.TaskComments.Add(new TaskComment
        {
            TaskItemId = taskId,
            UserId = GetCurrentUserId(),
            Comment = comment.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var comments = await context.TaskComments
            .Where(x => x.TaskItemId == taskId)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return PartialView("_CommentsList", comments);
    }

    private async Task<TaskIndexViewModel> BuildIndexModel(string? search, int? statusId, TaskPriority? priority, int? projectId, int page)
    {
        var availableProjectIds = await GetAvailableProjectIds();

        var query = context.Tasks
            .Include(x => x.ProjectEntity)
            .Include(x => x.AssignedTo)
            .Include(x => x.TaskStatusEntity)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
        {
            query = query.Where(x => availableProjectIds.Contains(x.ProjectEntityId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Title.Contains(search) || (x.Description ?? string.Empty).Contains(search));
        }

        if (statusId.HasValue)
        {
            query = query.Where(x => x.TaskStatusEntityId == statusId.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(x => x.Priority == priority.Value);
        }

        if (projectId.HasValue)
        {
            query = query.Where(x => x.ProjectEntityId == projectId.Value);
        }

        var totalItems = await query.CountAsync();
        var tasks = await query
            .OrderBy(x => x.TaskStatusEntityId)
            .ThenBy(x => x.Deadline)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return new TaskIndexViewModel
        {
            Tasks = tasks,
            Search = search,
            StatusId = statusId,
            Priority = priority,
            ProjectId = projectId,
            Page = page,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize)),
            Statuses = await context.TaskStatuses
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToListAsync(),
            Projects = await context.Projects
                .Where(x => User.IsInRole("Admin") || availableProjectIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToListAsync()
        };
    }

    private async Task<TaskEditViewModel> BuildTaskForm(TaskEditViewModel model)
    {
        var availableProjectIds = await GetAvailableProjectIds();

        var definitions = await context.TaskFieldDefinitions
            .OrderByDescending(x => x.IsBase)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var existingValues = model.Task.FieldValues.ToDictionary(x => x.TaskFieldDefinitionId, x => x.Value);
        if (model.Fields.Count == 0)
        {
            model.Fields = definitions.Select(x => new TaskFieldInputViewModel
            {
                DefinitionId = x.Id,
                Name = x.Name,
                FieldType = x.FieldType,
                IsRequired = x.IsRequired,
                IsBase = x.IsBase,
                Value = existingValues.GetValueOrDefault(x.Id)
            }).ToList();
        }

        model.Users = await context.Users
            .OrderBy(x => x.FirstName)
            .Select(x => new SelectListItem(x.FullName, x.Id.ToString()))
            .ToListAsync();

        model.Projects = await context.Projects
            .Where(x => User.IsInRole("Admin") || availableProjectIds.Contains(x.Id))
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();

        model.Queues = await context.WorkQueues
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();

        return model;
    }

    private Task ValidateFields(TaskEditViewModel model)
    {
        foreach (var field in model.Fields.Where(x => x.IsRequired && string.IsNullOrWhiteSpace(x.Value)))
        {
            ModelState.AddModelError(string.Empty, $"Поле \"{field.Name}\" обязательно.");
        }

        return Task.CompletedTask;
    }

    private async Task SaveFieldValues(int taskId, IEnumerable<TaskFieldInputViewModel> fields)
    {
        var currentValues = await context.TaskFieldValues.Where(x => x.TaskItemId == taskId).ToListAsync();
        context.TaskFieldValues.RemoveRange(currentValues);

        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.Value))
            {
                continue;
            }

            context.TaskFieldValues.Add(new TaskFieldValue
            {
                TaskItemId = taskId,
                TaskFieldDefinitionId = field.DefinitionId,
                Value = field.Value.Trim()
            });
        }

        await context.SaveChangesAsync();
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : 0;
    }

    private async Task<List<int>> GetAvailableProjectIds()
    {
        if (User.IsInRole("Admin"))
        {
            return await context.Projects.Select(x => x.Id).ToListAsync();
        }

        var userId = GetCurrentUserId();
        return await context.UserProjectAccesses
            .Where(x => x.UserId == userId)
            .Select(x => x.ProjectEntityId)
            .ToListAsync();
    }

    private async Task<bool> HasProjectAccess(int projectId)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var userId = GetCurrentUserId();
        return await context.UserProjectAccesses.AnyAsync(x => x.UserId == userId && x.ProjectEntityId == projectId);
    }
}
