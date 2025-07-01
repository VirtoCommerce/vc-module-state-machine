using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.StateMachineModule.Data.Models;
using VirtoCommerce.StateMachineModule.Data.Repositories;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class StateMachineRepositoryMock : IStateMachineRepository
{
    public IQueryable<StateMachineDefinitionEntity> StateMachineDefinitions
    {
        get
        {
            return StateMachineDefinitionEntities.AsAsyncQueryable();
        }
    }

    public List<StateMachineDefinitionEntity> StateMachineDefinitionEntities = new();

    public IQueryable<StateMachineInstanceEntity> StateMachineInstances
    {
        get
        {
            return StateMachineInstanceEntities.AsAsyncQueryable();
        }
    }

    public List<StateMachineInstanceEntity> StateMachineInstanceEntities = new();

    public IQueryable<StateMachineLocalizationEntity> StateMachineLocalizations
    {
        get
        {
            return StateMachineLocalizationEntities.AsAsyncQueryable();
        }
    }

    public List<StateMachineLocalizationEntity> StateMachineLocalizationEntities = new();

    public IQueryable<StateMachineAttributeEntity> StateMachineAttributes
    {
        get
        {
            return StateMachineAttributeEntities.AsAsyncQueryable();
        }
    }

    public List<StateMachineAttributeEntity> StateMachineAttributeEntities = new();

    public IUnitOfWork UnitOfWork => new Mock<IUnitOfWork>().Object;

    public void Add<T>(T item) where T : class
    {
        if (item.GetType() == typeof(StateMachineDefinitionEntity))
        {
            var stateMachineDefinitionEntity = item as StateMachineDefinitionEntity;
            if (string.IsNullOrEmpty(stateMachineDefinitionEntity.Id))
            {
                stateMachineDefinitionEntity.Id = nameof(StateMachineDefinitionEntity) + stateMachineDefinitionEntity.CreatedDate.Ticks.ToString();
            }
            StateMachineDefinitionEntities.Add(stateMachineDefinitionEntity);
        }
        else if (item.GetType() == typeof(StateMachineInstanceEntity))
        {
            var stateMachineInstanceEntity = item as StateMachineInstanceEntity;
            if (string.IsNullOrEmpty(stateMachineInstanceEntity.Id))
            {
                stateMachineInstanceEntity.Id = nameof(StateMachineInstanceEntity) + stateMachineInstanceEntity.CreatedDate.Ticks.ToString();
            }
            StateMachineInstanceEntities.Add(stateMachineInstanceEntity);
        }
        else if (item.GetType() == typeof(StateMachineLocalizationEntity))
        {
            var stateMachineLocalizationEntity = item as StateMachineLocalizationEntity;
            if (string.IsNullOrEmpty(stateMachineLocalizationEntity.Id))
            {
                stateMachineLocalizationEntity.Id = nameof(StateMachineLocalizationEntity) + stateMachineLocalizationEntity.CreatedDate.Ticks.ToString();
            }
            StateMachineLocalizationEntities.Add(stateMachineLocalizationEntity);
        }
    }

    public void Attach<T>(T item) where T : class
    {
        throw new NotImplementedException();
    }

    public void Remove<T>(T item) where T : class
    {
        if (item.GetType() == typeof(StateMachineDefinitionEntity))
        {
            var stateMachineDefinitionEntity = item as StateMachineDefinitionEntity;
            StateMachineDefinitionEntities.Remove(stateMachineDefinitionEntity);
        }
        else if (item.GetType() == typeof(StateMachineInstanceEntity))
        {
            var stateMachineInstanceEntity = item as StateMachineInstanceEntity;
            StateMachineInstanceEntities.Remove(stateMachineInstanceEntity);
        }
        else if (item.GetType() == typeof(StateMachineLocalizationEntity))
        {
            var stateMachineLocalizationEntity = item as StateMachineLocalizationEntity;
            StateMachineLocalizationEntities.Remove(stateMachineLocalizationEntity);
        }
    }

    public void Update<T>(T item) where T : class
    {
        throw new NotImplementedException();
    }
    public void Dispose()
    {
    }

    public Task<StateMachineDefinitionEntity[]> GetStateMachineDefinitionsByIds(string[] ids, string responseGroup = null)
    {
        var result = new List<StateMachineDefinitionEntity>();
        if (!ids.IsNullOrEmpty())
        {
            result = StateMachineDefinitionEntities.Where(x => ids.Contains(x.Id)).ToList();
        }
        return Task.FromResult(result.ToArray());
    }

    public Task<StateMachineDefinitionEntity> GetActiveStateMachineDefinitionByEntityType(string entityType, string responseGroup = null)
    {
        var result = StateMachineDefinitionEntities.FirstOrDefault(x => x.EntityType == entityType && x.IsActive);
        return Task.FromResult(result);
    }

    public Task<StateMachineInstanceEntity[]> GetStateMachineInstancesByIds(string[] ids, string responseGroup = null)
    {
        var result = new List<StateMachineInstanceEntity>();
        if (!ids.IsNullOrEmpty())
        {
            result = StateMachineInstanceEntities.Where(x => ids.Contains(x.Id)).ToList();
        }
        return Task.FromResult(result.ToArray());
    }

    public Task<StateMachineLocalizationEntity[]> GetStateMachineLocalizationsByIds(string[] ids, string responseGroup = null)
    {
        var result = new List<StateMachineLocalizationEntity>();
        if (!ids.IsNullOrEmpty())
        {
            result = StateMachineLocalizationEntities.Where(x => ids.Contains(x.Id)).ToList();
        }
        return Task.FromResult(result.ToArray());
    }

    public Task<StateMachineAttributeEntity[]> GetStateMachineAttributesByIds(string[] ids, string responseGroup = null)
    {
        var result = new List<StateMachineAttributeEntity>();
        if (!ids.IsNullOrEmpty())
        {
            result = StateMachineAttributeEntities.Where(x => ids.Contains(x.Id)).ToList();
        }
        return Task.FromResult(result.ToArray());
    }
}
