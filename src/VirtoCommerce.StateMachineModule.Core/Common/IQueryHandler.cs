using MediatR;

namespace VirtoCommerce.StateMachineModule.Core.Common;
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
}
