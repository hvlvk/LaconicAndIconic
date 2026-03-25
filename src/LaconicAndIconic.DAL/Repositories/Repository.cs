using LaconicAndIconic.DAL.Data;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.DAL.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _context.Set<T>().ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
        => await _context.Set<T>().FindAsync(id);

    public async Task AddAsync(T entity)
        => await _context.Set<T>().AddAsync(entity);

    public void Update(T entity)
        => _context.Set<T>().Update(entity);

    public void Remove(T entity)
        => _context.Set<T>().Remove(entity);

    public async Task<bool> ExistsAsync(int id)
        => await _context.Set<T>().FindAsync(id) is not null;

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
