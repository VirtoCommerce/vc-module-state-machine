using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Models;
public class StateMachineLocalizationEntity : AuditableEntity, IDataEntity<StateMachineLocalizationEntity, StateMachineLocalization>
{
    [Required]
    [StringLength(128)]
    public string DefinitionId { get; set; }

    [Required]
    [StringLength(128)]
    public string Item { get; set; }

    [Required]
    [StringLength(32)]
    public string Locale { get; set; }

    [Required]
    public string Value { get; set; }

    public virtual StateMachineLocalization ToModel(StateMachineLocalization model)
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

        model.DefinitionId = DefinitionId;
        model.Item = Item;
        model.Locale = Locale;
        model.Value = Value;

        return model;
    }

    public virtual StateMachineLocalizationEntity FromModel(StateMachineLocalization model, PrimaryKeyResolvingMap pkMap)
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

        DefinitionId = model.DefinitionId;
        Item = model.Item;
        Locale = model.Locale;
        Value = model.Value;

        return this;
    }

    public virtual void Patch(StateMachineLocalizationEntity target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        target.DefinitionId = DefinitionId;
        target.Item = Item;
        target.Locale = Locale;
        target.Value = Value;
    }
}
