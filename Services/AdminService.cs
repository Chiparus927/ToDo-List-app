using ToDoListApp.Database;
using ToDoListApp.Models;

namespace ToDoListApp.Services;

public class AdminService
{
    private readonly UserRepository _userRepository;
    private readonly TaskRepository _taskRepository;

    public AdminService(UserRepository userRepository, TaskRepository taskRepository)
    {
        _userRepository = userRepository;
        _taskRepository = taskRepository;
    }

    public List<UserModel> GetUsers() => _userRepository.GetAllUsers();

    public List<AdminTaskModel> GetAllTasks() => _taskRepository.GetAllTasksForAdmin();

    public void UpdateUserRole(int userId, string role) => _userRepository.UpdateRole(userId, role);

    public void DeleteUser(int userId) => _userRepository.DeleteUser(userId);
}
