using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Services;

public class TriggerContextEnrichmentService : ITriggerContextEnrichmentService
{
    public virtual Task EnrichContext(StateMachineTriggerContext context)
    {
        return Task.CompletedTask;
    }
}
