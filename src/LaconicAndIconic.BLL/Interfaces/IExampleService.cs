namespace LaconicAndIconic.BLL.Interfaces;

public interface IExampleService
{
    Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
    Task<T?> GetByIdAsync<T>(int id) where T : class;
    Task CreateAsync<T>(T entity) where T : class;
    Task UpdateAsync<T>(T entity) where T : class;
    Task<bool> DeleteAsync<T>(int id) where T : class;
}
