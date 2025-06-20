using System.ComponentModel.DataAnnotations;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class GetStateMachineDefinitionStatesQuery : IQuery<StateMachineStateShort[]>
{
    [Required]
    public string EntityType { get; set; }
    public string Locale { get; set; }
}
