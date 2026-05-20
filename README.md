# ToDoListApp (C# WinForms + MySQL)

Aplicatie desktop pentru gestionarea sarcinilor zilnice, construita in C# WinForms cu arhitectura pe layere:

- `Forms -> Services -> Repository -> Database`
- Validare input
- Query-uri parametrizate
- UI moderna cu sidebar si `DataGridView`

## Structura proiect

- `Database/`
  - `DbConnection.cs`
  - `UserRepository.cs`
  - `TaskRepository.cs`
  - `schema.sql`
- `Forms/`
  - `LoginForm.cs`
  - `RegisterForm.cs`
  - `DashboardForm.cs`
  - `AddTaskForm.cs`
  - `EditTaskForm.cs`
  - `SettingsForm.cs`
- `Models/`
  - `UserModel.cs`
  - `TaskModel.cs`
  - `CategoryModel.cs`
- `Services/`
  - `AuthService.cs`
  - `TaskService.cs`
- `Utils/`
  - `Validator.cs`
  - `Helpers.cs`
- `Resources/`
  - `Icons/`
  - `Images/`
- `ToDoListApp.Tests/` — teste automate (xUnit)

## Configurare baza de date

1. Creeaza baza MySQL ruland scriptul:
   - `Database/schema.sql`
2. Configureaza conexiunea in `App.config` la cheia `TodoListConnection`:
   - exemplu: `Server=localhost;Database=TodoListDB;Uid=root;Pwd=PAROLA_TA;`

## Rulare

```bash
dotnet restore
dotnet build
dotnet run
```

Aplicatia porneste cu `LoginForm`.

## Testare automata (xUnit)

Proiectul `ToDoListApp.Tests` contine teste unitare pentru `Validator` si `Helpers.HashPassword` (fara UI si fara MySQL).

```bash
dotnet test
```

Pentru a rula doar proiectul de teste: `dotnet test ToDoListApp.Tests/ToDoListApp.Tests.csproj`.
