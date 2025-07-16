using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class GetStateMachineInstanceQuery : IQuery<StateMachineInstance>
{
    [Required]
    public string StateMachineInstanceId { get; set; }
    public ClaimsPrincipal User { get; set; }
    public string Locale { get; set; }
}
