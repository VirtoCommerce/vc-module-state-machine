using FluentValidation;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Validators;
public class StateMachineValidator : AbstractValidator<StateMachineDefinition>
{
    public StateMachineValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty();
        RuleFor(x => x.States).NotEmpty();
    }
}
