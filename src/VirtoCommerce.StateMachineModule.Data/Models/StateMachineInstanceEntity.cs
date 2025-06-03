using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Models;
public class StateMachineInstanceEntity : AuditableEntity, IDataEntity<StateMachineInstanceEntity, StateMachineInstance>
{
    [Required]
    [StringLength(128)]
    public string EntityId { get; set; }

    [Required]
    [StringLength(128)]
    public string EntityType { get; set; }

    [StringLength(128)]
    [Required]
    public string StateMachineId { get; set; }

    [StringLength(128)]
    public string State { get; set; }

    public StateMachineDefinitionEntity StateMachineDefinition { get; set; }

    [Required]
    public bool IsStopped { get; set; }

    public virtual StateMachineInstance ToModel(StateMachineInstance model)
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

        model.EntityId = EntityId;
        model.EntityType = EntityType;

        if (StateMachineDefinition != null)
        {
            model.Configure(StateMachineDefinition.ToModel(ExType<StateMachineDefinition>.New()), State);
        }
        model.IsStopped = IsStopped;

        return model;
    }

    public virtual StateMachineInstanceEntity FromModel(StateMachineInstance model, PrimaryKeyResolvingMap pkMap)
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

        EntityId = model.EntityId;
        EntityType = model.EntityType;
        StateMachineId = model.StateMachineDefinitionId;
        State = model.CurrentStateName;
        IsStopped = model.IsStopped;

        return this;
    }

    public virtual void Patch(StateMachineInstanceEntity target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        target.State = State;
        target.IsStopped = IsStopped;
    }
}
