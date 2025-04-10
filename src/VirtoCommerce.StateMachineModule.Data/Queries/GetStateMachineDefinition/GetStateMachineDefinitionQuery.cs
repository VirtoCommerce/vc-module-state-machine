using System.ComponentModel.DataAnnotations;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class GetStateMachineDefinitionQuery : IQuery<StateMachineDefinition>
{
    [Required]
    public string StateMachineDefinitionId { get; set; }
    public string Locale { get; set; }
}
