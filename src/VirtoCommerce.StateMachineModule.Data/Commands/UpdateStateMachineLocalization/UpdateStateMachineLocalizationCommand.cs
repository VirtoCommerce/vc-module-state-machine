using System.ComponentModel.DataAnnotations;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Commands.UpdateStateMachineLocalization;
public class UpdateStateMachineLocalizationCommand : ICommand
{
    [Required]
    public StateMachineLocalization[] Localizations { get; set; }
}
