using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class StateMachineInstancesSearchServiceStub : IStateMachineInstancesSearchService
{
    public List<StateMachineInstance> StateMachineInstances = new List<StateMachineInstance>
    {
        new StateMachineInstance
        {
            Id = "TestStateMachineInstanceId1",
            EntityType = "TestEntityType1",
        },
        new StateMachineInstance
        {
            Id = "TestStateMachineInstanceId2",
            EntityType = "TestEntityType2",
        },
        new StateMachineInstance
        {
            Id = "TestStateMachineInstanceId3",
            EntityType = "TestEntityType3",
        },
    };

    public Task<SearchStateMachineInstancesResult> SearchAsync(SearchStateMachineInstancesCriteria criteria, bool clone = true)
    {
        var result = new SearchStateMachineInstancesResult();
        result.Results = StateMachineInstances
            .Where(x => criteria.ObjectIds.Contains(x.Id)).ToList();
        result.TotalCount = result.Results.Count;

        return Task.FromResult(result);
    }
}
