using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Commands.UpdateStateMachineLocalization;
public class UpdateStateMachineLocalizationCommandHandler : ICommandHandler<UpdateStateMachineLocalizationCommand>
{
    private readonly IStateMachineLocalizationCrudService _stateMachineLocalizationCrudService;

    public UpdateStateMachineLocalizationCommandHandler(
        IStateMachineLocalizationCrudService stateMachineLocalizationCrudService
        )
    {
        _stateMachineLocalizationCrudService = stateMachineLocalizationCrudService;
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

        await _stateMachineLocalizationCrudService.SaveChangesAsync(request.Localizations);
    }
}
