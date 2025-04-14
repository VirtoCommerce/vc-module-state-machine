using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Commands.UpdateStateMachineLocalization;
public class UpdateStateMachineLocalizationCommandHandler : ICommandHandler<UpdateStateMachineLocalizationCommand>
{
    private readonly IStateMachineLocalizationCrudService _stateMachineLocalizationCrudService;
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;

    public UpdateStateMachineLocalizationCommandHandler(
        IStateMachineLocalizationCrudService stateMachineLocalizationCrudService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService
        )
    {
        _stateMachineLocalizationCrudService = stateMachineLocalizationCrudService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
    }

    public virtual async Task Handle(UpdateStateMachineLocalizationCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Localizations == null || request.Localizations.Length == 0)
        {
            throw new ArgumentNullException(nameof(request.Localizations));
        }

        var localizations = request.Localizations;
        var definitionIds = localizations.Select(x => x.DefinitionId).Distinct().ToArray();
        var existedLocalizationSearchCriteria = new SearchStateMachineLocalizationCriteria { DefinitionIds = definitionIds };
        var existedLocalizationSearchResults = (await _stateMachineLocalizationSearchService.SearchAsync(existedLocalizationSearchCriteria, false)).Results;
        var localizationsToSave = new List<StateMachineLocalization>();

        foreach (var localization in localizations)
        {
            var existedLocalization = existedLocalizationSearchResults.FirstOrDefault(x => x.DefinitionId == localization.DefinitionId && x.Item == localization.Item && x.Locale == localization.Locale);
            if (existedLocalization != null)
            {
                existedLocalization.Value = localization.Value;
                localizationsToSave.Add(existedLocalization);
            }
            else
            {
                localizationsToSave.Add(localization);
            }
        }

        await _stateMachineLocalizationCrudService.SaveChangesAsync(localizationsToSave);
    }
}
