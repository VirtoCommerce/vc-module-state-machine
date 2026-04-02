using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;

[ExcludeFromCodeCoverage]
public class TriggerContextEnrichmentServiceStub : ITriggerContextEnrichmentService
{
    public Task EnrichContext(StateMachineTriggerContext context)
    {
        return Task.CompletedTask;
    }
}
