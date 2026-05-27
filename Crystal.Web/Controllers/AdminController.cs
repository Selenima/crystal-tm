using Crystal.Web.Data;
using Crystal.Web.Models;
using Crystal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
// тут редактируются пользователи проекты очереди статусы переходы и поля
public class AdminController(ApplicationDbContext context) : Controller
{
    // хэшер нужен чтобы новый пароль хранить как хеш а не текстом
    private readonly PasswordHasher<User> _passwordHasher = new();

    // главная страница админки собирает все справочники в одну модель
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            Users = await context.Users
                .Include(x => x.WorkQueue)
                .Include(x => x.ProjectAccesses)
                .ThenInclude(x => x.ProjectEntity)
                .OrderBy(x => x.LastName)
                .ToListAsync(),
            Projects = await context.Projects.OrderBy(x => x.Name).ToListAsync(),
            Queues = await context.WorkQueues.Include(x => x.Users).OrderBy(x => x.Name).ToListAsync(),
            Statuses = await context.TaskStatuses.OrderBy(x => x.Id).ToListAsync(),
            Transitions = await context.TaskStatusTransitions
                .Include(x => x.FromTaskStatusEntity)
                .Include(x => x.ToTaskStatusEntity)
                .OrderBy(x => x.FromTaskStatusEntity!.Name)
                .ThenBy(x => x.ToTaskStatusEntity!.Name)
                .ToListAsync(),
            FieldDefinitions = await context.TaskFieldDefinitions.OrderByDescending(x => x.IsBase).ThenBy(x => x.Name).ToListAsync()
        };

        return View(model);
    }
    
    [HttpGet]
    // get форма пользователя. без id создаем нового с id редактируем старого
    public async Task<IActionResult> UserForm(int? id)
    {
        var model = new UserEditViewModel();
        if (id.HasValue)
        {
            var user = await context.Users
                .Include(x => x.ProjectAccesses)
                .FirstOrDefaultAsync(x => x.Id == id.Value);
            if (user == null)
            {
                return NotFound();
            }

            model.Id = user.Id;
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.Email = user.Email;
            model.Position = user.Position;
            model.Department = user.Department;
            model.IsAdmin = user.IsAdmin;
            model.WorkQueueId = user.WorkQueueId;
            model.SelectedProjectIds = user.ProjectAccesses.Select(x => x.ProjectEntityId).ToList();
        }

        model.Queues = await GetQueueOptions();
        model.Projects = await GetProjectOptions();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // форма пользователя сохраняет данные пароль очередь и доступы к проектам
    public async Task<IActionResult> UserForm(UserEditViewModel model)
    {
        if (model.Id == 0 && string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Для нового пользователя нужен пароль.");
        }

        if (!ModelState.IsValid)
        {
            model.Queues = await GetQueueOptions();
            model.Projects = await GetProjectOptions();
            return View(model);
        }

        User user;
        if (model.Id == 0)
        {
            user = new User();
            context.Users.Add(user);
        }
        else
        {
            user = await context.Users.FindAsync(model.Id) ?? throw new InvalidOperationException("User not found.");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.Position = model.Position;
        user.Department = model.Department;
        user.IsAdmin = model.IsAdmin;
        user.WorkQueueId = model.WorkQueueId;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
        }

        await context.SaveChangesAsync();

        var currentAccesses = await context.UserProjectAccesses
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        context.UserProjectAccesses.RemoveRange(currentAccesses);

        foreach (var projectId in model.SelectedProjectIds.Distinct())
        {
            context.UserProjectAccesses.Add(new UserProjectAccess
            {
                UserId = user.Id,
                ProjectEntityId = projectId
            });
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // удаляет пользователя если он найден
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // форма проекта для создания или редактирования
    public async Task<IActionResult> ProjectForm(int? id)
    {
        if (!id.HasValue)
        {
            return View(new ProjectEntity());
        }

        var project = await context.Projects.FindAsync(id.Value);
        return project == null ? NotFound() : View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // сохраняет проект и проверяет что форма валидная
    public async Task<IActionResult> ProjectForm(ProjectEntity model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Id == 0)
        {
            context.Projects.Add(model);
        }
        else
        {
            context.Update(model);
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // удаляет проект
    public async Task<IActionResult> DeleteProject(int id)
    {
        var item = await context.Projects.FindAsync(id);
        if (item != null)
        {
            context.Projects.Remove(item);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // форма очереди, плюс подтягивает пользователей и задачи этой очереди
    public async Task<IActionResult> QueueForm(int? id)
    {
        var model = new QueueEditViewModel();
        if (id.HasValue)
        {
            var queue = await context.WorkQueues.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id.Value);
            if (queue == null)
            {
                return NotFound();
            }

            model.Id = queue.Id;
            model.Name = queue.Name;
            model.Description = queue.Description;
            model.SelectedUserIds = queue.Users.Select(x => x.Id).ToList();
        }

        model.AllUsers = await context.Users
            .OrderBy(x => x.FirstName)
            .Select(x => new SelectListItem(x.FullName, x.Id.ToString()))
            .ToListAsync();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // сохраняет очередь и переносит выбранных пользователей и задачи
    public async Task<IActionResult> QueueForm(QueueEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllUsers = await context.Users
                .OrderBy(x => x.FirstName)
                .Select(x => new SelectListItem(x.FullName, x.Id.ToString()))
                .ToListAsync();
            return View(model);
        }

        WorkQueue queue;
        if (model.Id == 0)
        {
            queue = new WorkQueue();
            context.WorkQueues.Add(queue);
        }
        else
        {
            queue = await context.WorkQueues.FindAsync(model.Id) ?? throw new InvalidOperationException("Queue not found.");
        }

        queue.Name = model.Name;
        queue.Description = model.Description;
        await context.SaveChangesAsync();

        var users = await context.Users.ToListAsync();
        foreach (var user in users)
        {
            if (model.SelectedUserIds.Contains(user.Id))
            {
                user.WorkQueueId = queue.Id;
            }
            else if (user.WorkQueueId == queue.Id)
            {
                user.WorkQueueId = null;
            }
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // удаляет очередь, связанные пользователи и задачи остаются без очереди
    public async Task<IActionResult> DeleteQueue(int id)
    {
        var item = await context.WorkQueues.FindAsync(id);
        if (item != null)
        {
            context.WorkQueues.Remove(item);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // форма статуса задачи
    public async Task<IActionResult> StatusForm(int? id)
    {
        if (!id.HasValue)
        {
            return View(new TaskStatusEntity());
        }

        var item = await context.TaskStatuses.FindAsync(id.Value);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // сохраняет название и цвет статуса
    public async Task<IActionResult> StatusForm(TaskStatusEntity model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Id == 0)
        {
            context.TaskStatuses.Add(model);
        }
        else
        {
            context.Update(model);
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // форма правила перехода между статусами
    public async Task<IActionResult> TransitionForm(int? id)
    {
        var model = new StatusTransitionEditViewModel();
        if (id.HasValue)
        {
            var transition = await context.TaskStatusTransitions.FindAsync(id.Value);
            if (transition == null)
            {
                return NotFound();
            }

            model.Id = transition.Id;
            model.FromTaskStatusEntityId = transition.FromTaskStatusEntityId;
            model.ToTaskStatusEntityId = transition.ToTaskStatusEntityId;
        }

        model.Statuses = await GetStatusOptions();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // сохраняет правило
    public async Task<IActionResult> TransitionForm(StatusTransitionEditViewModel model)
    {
        if (model.FromTaskStatusEntityId == model.ToTaskStatusEntityId)
        {
            ModelState.AddModelError(string.Empty, "Начальный и конечный статус должны отличаться.");
        }

        var duplicateExists = await context.TaskStatusTransitions.AnyAsync(x =>
            x.Id != model.Id &&
            x.FromTaskStatusEntityId == model.FromTaskStatusEntityId &&
            x.ToTaskStatusEntityId == model.ToTaskStatusEntityId);

        if (duplicateExists)
        {
            ModelState.AddModelError(string.Empty, "Такой переход уже существует.");
        }

        if (!ModelState.IsValid)
        {
            model.Statuses = await GetStatusOptions();
            return View(model);
        }

        TaskStatusTransition transition;
        if (model.Id == 0)
        {
            transition = new TaskStatusTransition();
            context.TaskStatusTransitions.Add(transition);
        }
        else
        {
            transition = await context.TaskStatusTransitions.FindAsync(model.Id) ?? throw new InvalidOperationException("Transition not found.");
        }

        transition.FromTaskStatusEntityId = model.FromTaskStatusEntityId;
        transition.ToTaskStatusEntityId = model.ToTaskStatusEntityId;

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // удаляет правило перехода статуса
    public async Task<IActionResult> DeleteTransition(int id)
    {
        var item = await context.TaskStatusTransitions.FindAsync(id);
        if (item != null)
        {
            context.TaskStatusTransitions.Remove(item);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // удаляет статус если он найден
    public async Task<IActionResult> DeleteStatus(int id)
    {
        var item = await context.TaskStatuses.FindAsync(id);
        if (item != null)
        {
            context.TaskStatuses.Remove(item);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // форма дополнительного поля задачи
    public async Task<IActionResult> FieldForm(int? id)
    {
        if (!id.HasValue)
        {
            return View(new TaskFieldDefinition());
        }

        var item = await context.TaskFieldDefinitions.FindAsync(id.Value);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // сохраняет настройку дополнительного поля
    public async Task<IActionResult> FieldForm(TaskFieldDefinition model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Id == 0)
        {
            context.TaskFieldDefinitions.Add(model);
        }
        else
        {
            context.Update(model);
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // удаляет дополнительное поле и его значения
    public async Task<IActionResult> DeleteField(int id)
    {
        var item = await context.TaskFieldDefinitions.FindAsync(id);
        if (item != null)
        {
            context.TaskFieldDefinitions.Remove(item);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // собирает очереди для дропдауна
    private async Task<List<SelectListItem>> GetQueueOptions()
    {
        return await context.WorkQueues
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }

    // собирает проекты для чекбоксов доступа
    private async Task<List<SelectListItem>> GetProjectOptions()
    {
        return await context.Projects
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }

    // собирает статусы для формы переходов
    private async Task<List<SelectListItem>> GetStatusOptions()
    {
        return await context.TaskStatuses
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync();
    }
}
