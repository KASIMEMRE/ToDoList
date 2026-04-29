using ToDoList.Models;

namespace ToDoList.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Todo> Todos { get; }
        IGenericRepository<User> Users { get; }

        Task<int> SaveChangesAsync();

    }
}
