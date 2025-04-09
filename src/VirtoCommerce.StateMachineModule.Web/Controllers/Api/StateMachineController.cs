using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.StateMachineModule.Core;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Data.Commands;
using VirtoCommerce.StateMachineModule.Data.Queries;
using VirtoCommerce.StateMachineModule.Data.Validators;

namespace VirtoCommerce.StateMachineModule.Web.Controllers.Api
{
    [ApiController]
    [Route("api/statemachine")]
    public class StateMachineController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuthorizationService _authorizationService;

        public StateMachineController(IMediator mediator, IAuthorizationService authorizationService)
        {
            _mediator = mediator;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Route("definitions/search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<SearchStateMachineDefinitionResult>> Search([FromBody] SearchStateMachineDefinitionsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [Route("definitions/new")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<StateMachineDefinition>> CreateNewDefinition([FromBody] CreateStateMachineDefinitionCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        [Route("definitions/{definitionId}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<StateMachineDefinition>> GetStateMachineById([FromRoute] string definitionId)
        {
            var query = new GetStateMachineDefinitionQuery { StateMachineDefinitionId = definitionId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [Route("definitions/validate")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<StateMachineDefinition>> ValidateDefinition([FromBody] StateMachineDefinition definition)
        {
            var validator = new StateMachineValidator();
            await validator.ValidateAndThrowAsync(definition);
            return Ok();
        }

        [HttpGet]
        [Route("definitions/allstates")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<StateMachineStateShort[]>> GetAllStates([FromQuery] string entityType)
        {
            var query = ExType<GetStateMachineDefinitionStatesQuery>.New();
            query.EntityType = entityType;
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpPost]
        [Route("instances/search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<SearchStateMachineInstanceResult>> SearchInstance([FromBody] SearchStateMachineInstancesQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        [Route("instances/{instanceId}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<StateMachineInstance>> GetStateMachineInstanceById([FromRoute] string instanceId)
        {
            var query = new GetStateMachineInstanceQuery { StateMachineInstanceId = instanceId, User = User };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [Route("instances/{definitionId}/new")]
        //TODO: Replace IHasDynamicProperties to some other base interface that has Disrciminator property to be able receive all vc  models as argument
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<StateMachineInstance>> CreateNewInstance([FromRoute] string definitionId, [FromQuery] string instanceId, [FromBody] IHasDynamicProperties entity)
        {
            var command = new CreateStateMachineInstanceCommand { StateMachineDefinitionId = definitionId, StateMachineInstanceId = instanceId, Entity = entity };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost]
        [Route("firetrigger")]
        [Authorize(ModuleConstants.Security.Permissions.Fire)]
        public async Task<ActionResult<StateMachineInstance>> FireTrigger([FromBody] FireStateMachineTriggerCommand command)
        {
            command.User = User;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
