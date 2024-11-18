namespace WarehouseApi.Repositories
{
    /// <summary>
    /// Generic interface for repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
