using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Commands;
public class UpdateStateMachineAttributeCommandHandler : ICommandHandler<UpdateStateMachineAttributeCommand>
{
    private readonly IStateMachineAttributeCrudService _stateMachineAttributeCrudService;
    private readonly IStateMachineAttributeSearchService _stateMachineAttributeSearchService;

    public UpdateStateMachineAttributeCommandHandler(
        IStateMachineAttributeCrudService stateMachineAttributeCrudService,
        IStateMachineAttributeSearchService stateMachineAttributeSearchService
        )
    {
        _stateMachineAttributeCrudService = stateMachineAttributeCrudService;
        _stateMachineAttributeSearchService = stateMachineAttributeSearchService;
    }

    public virtual async Task Handle(UpdateStateMachineAttributeCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Attributes == null || request.Attributes.Length == 0)
        {
            throw new ArgumentNullException(nameof(request.Attributes));
        }

        var attributes = request.Attributes;
        var definitionIds = attributes.Select(x => x.DefinitionId).Distinct().ToArray();
        var existedAttributeSearchCriteria = new SearchStateMachineAttributeCriteria { DefinitionIds = definitionIds };
        var existedAttributeSearchResults = (await _stateMachineAttributeSearchService.SearchAsync(existedAttributeSearchCriteria, false)).Results;
        var attributesToSave = new List<StateMachineAttribute>();

        foreach (var attribute in attributes)
        {
            var existedAttribute = existedAttributeSearchResults.FirstOrDefault(x => x.DefinitionId == attribute.DefinitionId && x.Item == attribute.Item && x.AttributeKey == attribute.AttributeKey);
            if (existedAttribute != null)
            {
                existedAttribute.Value = attribute.Value;
                attributesToSave.Add(existedAttribute);
            }
            else
            {
                attributesToSave.Add(attribute);
            }
        }

        await _stateMachineAttributeCrudService.SaveChangesAsync(attributesToSave);
    }
}
