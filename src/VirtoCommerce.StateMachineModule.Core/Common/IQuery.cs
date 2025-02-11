using MediatR;

namespace VirtoCommerce.StateMachineModule.Core.Common;
public interface IQuery<out TResult> : IRequest<TResult>
{
}
