using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.JsonConverters;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Models;
public class StateMachineDefinitionEntity : AuditableEntity, IDataEntity<StateMachineDefinitionEntity, StateMachineDefinition>
{
    [Required]
    [StringLength(512)]
    public string Name { get; set; }

    [StringLength(128)]
    public string EntityType { get; set; }

    public bool IsActive { get; set; }

    [StringLength(32)]
    public string Version { get; set; }

    [Required]
    public string StatesSerialized { get; set; }

    public virtual StateMachineDefinition ToModel(StateMachineDefinition model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.Name = Name;
        model.EntityType = EntityType;
        model.IsActive = IsActive;
        model.Version = Version;
        model.States = JsonConvert.DeserializeObject<StateMachineState[]>(StatesSerialized, new PolymorphJsonConverter());

        return model;
    }

    public virtual StateMachineDefinitionEntity FromModel(StateMachineDefinition model, PrimaryKeyResolvingMap pkMap)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        Name = model.Name;
        EntityType = model.EntityType;
        IsActive = model.IsActive;
        Version = model.Version;

        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
        settings.Converters.Add(new ConditionJsonConverter(doNotSerializeAvailCondition: true));
        StatesSerialized = JsonConvert.SerializeObject(model.States, settings);

        return this;
    }


    public virtual void Patch(StateMachineDefinitionEntity target)
    {
        target.Name = Name;
        target.EntityType = EntityType;
        target.IsActive = IsActive;
        target.Version = Version;
        target.StatesSerialized = StatesSerialized;
    }
}
