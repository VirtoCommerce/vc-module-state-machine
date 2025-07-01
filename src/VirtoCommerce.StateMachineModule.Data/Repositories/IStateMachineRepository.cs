using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Data.Models;

namespace VirtoCommerce.StateMachineModule.Data.Repositories;
public interface IStateMachineRepository : IRepository
{
    IQueryable<StateMachineDefinitionEntity> StateMachineDefinitions { get; }
    IQueryable<StateMachineInstanceEntity> StateMachineInstances { get; }
    IQueryable<StateMachineLocalizationEntity> StateMachineLocalizations { get; }
    IQueryable<StateMachineAttributeEntity> StateMachineAttributes { get; }

    Task<StateMachineDefinitionEntity[]> GetStateMachineDefinitionsByIds(string[] ids, string responseGroup = null);
    Task<StateMachineDefinitionEntity> GetActiveStateMachineDefinitionByEntityType(string entityType, string responseGroup = null);
    Task<StateMachineInstanceEntity[]> GetStateMachineInstancesByIds(string[] ids, string responseGroup = null);
    Task<StateMachineLocalizationEntity[]> GetStateMachineLocalizationsByIds(string[] ids, string responseGroup = null);
    Task<StateMachineAttributeEntity[]> GetStateMachineAttributesByIds(string[] ids, string responseGroup = null);
}
