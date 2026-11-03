namespace CleanModelContextProtocol.Application.Authors.Queries.GetAuthorById;

using Entities;
using Mediator;

public class GetAuthorByIdQuery : IRequest<Author>
{
    public Guid Id { get; init; }
}
