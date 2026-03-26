using System.Linq.Expressions;
using LaconicAndIconic.DAL.Data;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.DAL.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected ApplicationDbContext Context { get; }

    public Repository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await Context.Set<T>().ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
        => await Context.Set<T>().FindAsync(id);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = Context.Set<T>();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
        => await Context.Set<T>().AddAsync(entity);

    public void Update(T entity)
        => Context.Set<T>().Update(entity);

    public void Remove(T entity)
        => Context.Set<T>().Remove(entity);

    public async Task<bool> ExistsAsync(int id)
        => await Context.Set<T>().FindAsync(id) is not null;

    public async Task SaveChangesAsync()
        => await Context.SaveChangesAsync();
}
