using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.StateMachineModule.Tests;
[ExcludeFromCodeCoverage]
public static class TestHepler
{
    public static IServiceCollection AddCollection<T>(this IServiceCollection services, T t) where T : class
    {
        return services.AddTransient(provider => t);
    }

    public static T LoadFromJsonFile<T>(string fileName)
    {
        var filePath = Path.Combine(@"../../../TestData", fileName);
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
    }

    public static dynamic LoadArrayFromJsonFile(string fileName)
    {
        var filePath = Path.Combine(@"../../../TestData", fileName);
        return JArray.Parse(File.ReadAllText(filePath));
    }

}
