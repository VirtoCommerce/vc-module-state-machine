using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Core.Services;

public interface ITriggerContextEnrichmentService
{
    Task EnrichContext(StateMachineTriggerContext context);
}
