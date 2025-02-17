using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.StateMachineModule.Data.Models;

namespace VirtoCommerce.StateMachineModule.Data.Repositories;
public class StateMachineRepository : DbContextRepositoryBase<StateMachineDbContext>, IStateMachineRepository
{
    public StateMachineRepository(StateMachineDbContext dbContext)
    : base(dbContext)
    {
    }

    public IQueryable<StateMachineDefinitionEntity> StateMachineDefinitions => DbContext.Set<StateMachineDefinitionEntity>();
    public IQueryable<StateMachineInstanceEntity> StateMachineInstances => DbContext.Set<StateMachineInstanceEntity>();

    public virtual async Task<StateMachineDefinitionEntity[]> GetStateMachineDefinitionsByIds(string[] ids, string responseGroup = null)
    {
        var result = Array.Empty<StateMachineDefinitionEntity>();

        if (!ids.IsNullOrEmpty())
        {
            result = await StateMachineDefinitions.
                Where(x => ids.Contains(x.Id)).ToArrayAsync();
        }
        return result;
    }

    public virtual async Task<StateMachineDefinitionEntity> GetActiveStateMachineDefinitionByEntityType(string entityType, string responseGroup = null)
    {
        return await StateMachineDefinitions
            .FirstOrDefaultAsync(x => x.EntityType == entityType && x.IsActive);
    }

    public virtual async Task<StateMachineInstanceEntity[]> GetStateMachineInstancesByIds(string[] ids, string responseGroup = null)
    {
        var result = Array.Empty<StateMachineInstanceEntity>();

        if (!ids.IsNullOrEmpty())
        {
            result = await StateMachineInstances.Where(x => ids.Contains(x.Id)).Include(x => x.StateMachineDefinition).ToArrayAsync();
        }
        return result;
    }
}
