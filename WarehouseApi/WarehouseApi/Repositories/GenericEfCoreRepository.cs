using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;

namespace WarehouseApi.Repositories
{
    public class GenericEfCoreRepository<T> : IRepository<T> where T : class
    {
        private readonly WarehouseContext _context;
        private DbSet<T> _dbSet;

        public GenericEfCoreRepository(WarehouseContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<T?> Get(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
