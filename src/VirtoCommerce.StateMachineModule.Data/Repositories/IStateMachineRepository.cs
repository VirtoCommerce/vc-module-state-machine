using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Data.Models;

namespace VirtoCommerce.StateMachineModule.Data.Repositories;
public interface IStateMachineRepository : IRepository
{
    IQueryable<StateMachineDefinitionEntity> StateMachineDefinitions { get; }
    IQueryable<StateMachineInstanceEntity> StateMachineInstances { get; }

    Task<StateMachineDefinitionEntity[]> GetStateMachineDefinitionsByIds(string[] ids, string responseGroup = null);
    Task<StateMachineInstanceEntity[]> GetStateMachineInstancesByIds(string[] ids, string responseGroup = null);
}
