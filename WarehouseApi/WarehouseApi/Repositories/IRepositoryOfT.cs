namespace WarehouseApi.Repositories
{
    public interface IRepository<T>
    {
        Task<T?> Get(int id);
        Task<IEnumerable<T>> GetAll();
        void Delete(T entity);
        void Update(T entity);
        void Add(T entity);
        Task SaveChanges();
    }
}
