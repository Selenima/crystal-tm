using System.Security.Claims;
using System.Globalization;
using Crystal.Web.Data;
using Crystal.Web.Models;
using Crystal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Web.Controllers;

[Authorize]
// контроллер задач
public class TasksController(ApplicationDbContext context) : Controller
{
    // сколько задач показываем на одной странице
    private const int PageSize = 6;

    // главная страница задач, собирает фильтры и отдает полную view
    public async Task<IActionResult> Index(string? search, int? statusId, TaskPriority? priority, int? projectId, int page = 1)
    {
        return View(await BuildIndexModel(search, statusId, priority, projectId, page));
    }

    // отдает пустую форму задачи с дефолтным статусом и дедлайном
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
                Deadline = DateTime.UtcNow.Date.AddDays(7),
                Priority = TaskPriority.Medium,
                TaskStatusEntityId = defaultStatusId
            }
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // проверяет доступ к проекту и сохраняет задачу
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
        NormalizeTaskDates(model.Task);

        context.Tasks.Add(model.Task);
        await context.SaveChangesAsync();
        await SaveFieldValues(model.Task.Id, model.Fields);

        return RedirectToAction(nameof(Index));
    }

    // загружает задачу и ее доп поля для формы
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
    // обновляет задачу, но статус оставляет старым чтобы его меняли через переходы
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
        NormalizeTaskDates(model.Task);
        context.Update(model.Task);
        await context.SaveChangesAsync();
        await SaveFieldValues(model.Task.Id, model.Fields);

        return RedirectToAction(nameof(Index));
    }

    // показывает полную карточку задачи с комментариями и разрешенными статусами
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
    // удаление задачи, срабатывает только если есть доступ к проекту
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
    // ajax фильтр возвращает только таблицу, без всего layout
    public async Task<IActionResult> Filter(string? search, int? statusId, TaskPriority? priority, int? projectId, int page = 1)
    {
        return PartialView("_TaskTable", await BuildIndexModel(search, statusId, priority, projectId, page));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // смена статуса проверяет что такой переход вообще разрешен
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
    // возвращает список комментариев для частичного обновления на странице
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
    // добавляет комментарий и сразу возвращает обновленный partial
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

    // собирает модель списка задач с фильтрами пагинацией и select списками
    private async Task<TaskIndexViewModel> BuildIndexModel(string? search, int? statusId, TaskPriority? priority, int? projectId, int page)
    {
        var availableProjectIds = await GetAvailableProjectIds();

        // query постепенно дополняется фильтрами, запрос в базу уйдет только на ToListAsync
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

    // подготавливает форму задачи, добавляет списки пользователей проектов очередей и доп поля
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
                Value = FormatFieldValueForInput(x.FieldType, existingValues.GetValueOrDefault(x.Id))
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

    // ручная проверка динамических полей, потому что их нельзя заранее прописать атрибутами
    private Task ValidateFields(TaskEditViewModel model)
    {
        foreach (var field in model.Fields.Where(x => x.IsRequired && string.IsNullOrWhiteSpace(x.Value)))
        {
            ModelState.AddModelError(string.Empty, $"Поле \"{field.Name}\" обязательно.");
        }

        foreach (var field in model.Fields.Where(x => x.FieldType == TaskFieldType.Date && !string.IsNullOrWhiteSpace(x.Value)))
        {
            if (!DateTime.TryParseExact(field.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                ModelState.AddModelError(string.Empty, $"РџРѕР»Рµ \"{field.Name}\" РґРѕР»Р¶РЅРѕ Р±С‹С‚СЊ РґР°С‚РѕР№.");
            }
        }

        return Task.CompletedTask;
    }

    // сохраняет доп поля просто удаляя старые значения и записывая новые
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
                Value = NormalizeFieldValue(field)
            });
        }

        await context.SaveChangesAsync();
    }

    // достает id текущего пользователя из claims cookie
    private static void NormalizeTaskDates(TaskItem task)
    {
        if (task.Deadline.HasValue)
        {
            task.Deadline = DateTime.SpecifyKind(task.Deadline.Value.Date, DateTimeKind.Utc);
        }
    }

    private static string NormalizeFieldValue(TaskFieldInputViewModel field)
    {
        var value = field.Value?.Trim() ?? string.Empty;
        if (field.FieldType != TaskFieldType.Date)
        {
            return value;
        }

        return DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string? FormatFieldValueForInput(TaskFieldType fieldType, string? value)
    {
        if (fieldType != TaskFieldType.Date || string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : value;
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : 0;
    }

    // возвращает проекты доступные пользователю, админ видит все
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

    // проверяет доступ к одному проекту перед показом или изменением данных
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
