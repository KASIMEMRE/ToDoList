using ToDoList.Data;
using ToDoList.Models;

namespace ToDoList.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IGenericRepository<Todo> Todos { get; private set; }
        public IGenericRepository<User> Users { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            
            Todos = new GenericRepository<Todo>(_context);
            Users = new GenericRepository<User>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
