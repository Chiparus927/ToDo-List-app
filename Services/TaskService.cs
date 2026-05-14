using ToDoListApp.Database;
using ToDoListApp.Models;

namespace ToDoListApp.Services;

public class TaskService
{
    private readonly TaskRepository _taskRepository;

    public TaskService(TaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public List<CategoryModel> GetCategories() => _taskRepository.GetCategories();

    public List<TaskModel> GetTasks(int userId, string? statusFilter, string? search, int? categoryId)
        => _taskRepository.GetTasks(userId, statusFilter, search, categoryId);

    public int AddTask(TaskModel task) => _taskRepository.AddTask(task);

    public void UpdateTask(TaskModel task) => _taskRepository.UpdateTask(task);

    public void DeleteTask(int taskId, int userId) => _taskRepository.DeleteTask(taskId, userId);
}
