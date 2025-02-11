using MediatR;

namespace VirtoCommerce.StateMachineModule.Core.Common;
public interface ICommand<out TResult> : IRequest<TResult>
{
}

public interface ICommand : IRequest
{
}
