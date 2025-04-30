using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using static VirtoCommerce.StateMachineModule.Core.ModuleConstants;

namespace VirtoCommerce.StateMachineModule.Data.Queries.GetStateMachineSettings;
public class GetStateMachineSettingsQueryHandler : IQueryHandler<GetStateMachineSettingsQuery, StateMachineSettings>
{
    private readonly ISettingsManager _settingsManager;

    public GetStateMachineSettingsQueryHandler(
        ISettingsManager settingsManager
        )
    {
        _settingsManager = settingsManager;
    }

    public virtual async Task<StateMachineSettings> Handle(GetStateMachineSettingsQuery request, CancellationToken cancellationToken)
    {
        var languageSettings = await _settingsManager.GetObjectSettingAsync(Settings.General.StateMachineLanguages.Name);

        var result = ExType<StateMachineSettings>.New();
        result.Languages = languageSettings.AllowedValues?.Select(x => x.ToString()).OrderBy(x => x).ToArray() ?? [languageSettings.DefaultValue?.ToString()];

        return result;
    }
}
