using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.BLL.Services;

public class ExampleService : IExampleService
{
    private readonly ApplicationDbContext _context;

    public ExampleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync<T>(int id) where T : class
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task CreateAsync<T>(T entity) where T : class
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync<T>(T entity) where T : class
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync<T>(int id) where T : class
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity is null)
            return false;

        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
