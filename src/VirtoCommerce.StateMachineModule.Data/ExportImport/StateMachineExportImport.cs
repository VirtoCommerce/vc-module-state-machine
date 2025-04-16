using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Queries;

namespace VirtoCommerce.StateMachineModule.Data.ExportImport;
public class StateMachineExportImport
{
    private readonly IStateMachineDefinitionSearchService _stateMachineDefinitionsSearchService;
    private readonly IStateMachineDefinitionService _stateMachineDefinitionsCrudService;
    private readonly IStateMachineInstanceSearchService _stateMachineInstancesSearchService;
    private readonly IStateMachineInstanceService _stateMachineInstancesCrudService;
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;
    private readonly IStateMachineLocalizationCrudService _stateMachineLocalizationCrudService;
    private readonly JsonSerializer _jsonSerializer;

    private readonly int _batchSize = 50;

    public StateMachineExportImport(
        IStateMachineDefinitionSearchService stateMachineDefinitionsSearchService,
        IStateMachineDefinitionService stateMachineDefinitionsCrudService,
        IStateMachineInstanceSearchService stateMachineInstancesSearchService,
        IStateMachineInstanceService stateMachineInstancesCrudService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService,
        IStateMachineLocalizationCrudService stateMachineLocalizationCrudService,
        JsonSerializer jsonSerializer
        )
    {
        _stateMachineDefinitionsSearchService = stateMachineDefinitionsSearchService;
        _stateMachineDefinitionsCrudService = stateMachineDefinitionsCrudService;
        _stateMachineInstancesSearchService = stateMachineInstancesSearchService;
        _stateMachineInstancesCrudService = stateMachineInstancesCrudService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
        _stateMachineLocalizationCrudService = stateMachineLocalizationCrudService;
        _jsonSerializer = jsonSerializer;
    }

    public virtual async Task DoExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
        progressCallback(progressInfo);

        using (var sw = new StreamWriter(outStream))
        using (var writer = new JsonTextWriter(sw))
        {
            await writer.WriteStartObjectAsync();

            #region Export StateMachineDefinitions

            progressInfo.Description = "StateMachineDefinitions exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("StateMachineDefinitions");
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _stateMachineDefinitionsSearchService.SearchAsync(new SearchStateMachineDefinitionsQuery { Skip = skip, Take = take });
                return (GenericSearchResult<StateMachineDefinition>)searchResult;
            }
            , (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} state machine definitions have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);

            #endregion

            #region Export StateMachineInstances

            progressInfo.Description = "StateMachineInstances exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("StateMachineInstances");
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _stateMachineInstancesSearchService.SearchAsync(new SearchStateMachineInstancesQuery { Skip = skip, Take = take });
                return (GenericSearchResult<StateMachineInstance>)searchResult;
            }
            , (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} state machine instances have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);

            #endregion

            #region Export StateMachineLocalizations

            progressInfo.Description = "StateMachineLocalizations exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("StateMachineLocalizations");
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _stateMachineLocalizationSearchService.SearchAsync(new SearchStateMachineLocalizationCriteria { Skip = skip, Take = take }, false);
                return (GenericSearchResult<StateMachineLocalization>)searchResult;
            }
            , (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} state machine localizations have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);

            #endregion

            await writer.WriteEndObjectAsync();
            await writer.FlushAsync();
        }
    }

    public virtual async Task DoImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var progressInfo = new ExportImportProgressInfo();

        using (var streamReader = new StreamReader(inputStream))
        using (var reader = new JsonTextReader(streamReader))
        {
            while (await reader.ReadAsync())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                switch (reader.Value?.ToString())
                {

                    case "StateMachineDefinitions":
                        await reader.DeserializeArrayWithPagingAsync<StateMachineDefinition>(_jsonSerializer, _batchSize, items => _stateMachineDefinitionsCrudService.SaveChangesAsync(items), processedCount =>
                        {
                            progressInfo.Description = $"{processedCount} state machine definitions have been imported";
                            progressCallback(progressInfo);
                        }, cancellationToken);
                        break;
                    case "StateMachineInstances":
                        await reader.DeserializeArrayWithPagingAsync<StateMachineInstance>(_jsonSerializer, _batchSize, items => _stateMachineInstancesCrudService.SaveChangesAsync(items), processedCount =>
                        {
                            progressInfo.Description = $"{processedCount} state machine instances have been imported";
                            progressCallback(progressInfo);
                        }, cancellationToken);
                        break;
                    case "StateMachineLocalizations":
                        await reader.DeserializeArrayWithPagingAsync<StateMachineLocalization>(_jsonSerializer, _batchSize, items => _stateMachineLocalizationCrudService.SaveChangesAsync(items), processedCount =>
                        {
                            progressInfo.Description = $"{processedCount} state machine localization have been imported";
                            progressCallback(progressInfo);
                        }, cancellationToken);
                        break;

                    default:
                        continue;
                }
            }
        }
    }
}
