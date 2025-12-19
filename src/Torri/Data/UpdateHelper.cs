using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Torri.Data;

public interface IUpdateHelper<TEntity>
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    IUpdateHelper<TEntity> UpdateProperty<TValue>(Expression<Func<TEntity, TValue>> propertyExpression, TValue value);
}

public class UpdateHelper<TEntity>(EntityEntry<TEntity> entry, TorriContext context) : IUpdateHelper<TEntity>
    where TEntity : class
{
    public IUpdateHelper<TEntity> UpdateProperty<TValue>(Expression<Func<TEntity, TValue>> propertyExpression, TValue value)
    {
        var prop = entry.Property(propertyExpression);
        prop.CurrentValue = value;
        prop.IsModified = true;
        return this;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}