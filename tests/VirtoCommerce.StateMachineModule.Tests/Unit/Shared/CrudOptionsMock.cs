using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class CrudOptionsMock : IOptions<CrudOptions>
{
    public CrudOptions Value
    {
        get
        {
            var crudOptions = new CrudOptions();
            crudOptions.MaxResultWindow = 100;
            return crudOptions;
        }
    }
}
