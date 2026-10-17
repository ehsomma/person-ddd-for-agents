using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Persistence.Abstractions;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Mediator.Abstractions;

namespace BuildingBlocks.Application.Cqrs;

/// <summary>
/// Base class for handlers of a <see cref="ICommand{TResponse}"/>. Exactly one handler must exist per
/// <typeparamref name="TCommand"/>/<typeparamref name="TResponse"/> pair.
/// </summary>
/// <typeparam name="TCommand">Command type handled.</typeparam>
/// <typeparam name="TResponse">Result type produced by handling the command.</typeparam>
public abstract class CommandHandler<TCommand, TResponse> : RequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    #region Declarations

    /// <summary>Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1401:Fields should be private",
        Justification = "I prefer to use it as field just in protected fields in base classes like Repository base class.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "I prefer to use it as field just in protected fields in base classes like Repository base class.")]
    //// ReSharper disable once InconsistentNaming
    protected readonly IUnitOfWork _unitOfWork; // From Persistence (not Projection).

    /// <summary>Publishes the domain events raised by the aggregates handled by the command.</summary>
    private readonly IDomainEventPublisher _domainEventPublisher;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandler{TCommand, TResponse}"/> class.
    /// </summary>
    /// <param name="domainEventPublisher">Publishes the domain events raised by the aggregates handled by the command.</param>
    /// <param name="unitOfWork">Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    protected CommandHandler(
        IDomainEventPublisher domainEventPublisher,
        IUnitOfWork unitOfWork)
    {
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    #endregion

    #region Protected methods

    /// <summary>
    /// Executes the specified <paramref name="operation"/> inside a database transaction: commits if it
    /// succeeds, and rolls back and rethrows the exception if it fails.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Publish the domain events after this call, so the handlers only react to changes that are already persisted.
    /// </para>
    /// <para>
    /// The <paramref name="cancellationToken"/> is checked before opening the transaction, and the
    /// <paramref name="operation"/> must also receive it (e.g. inside the lambda). A cancellation during the
    /// operation rolls back, but the commit is never cancelled: if the operation succeeds, it's always committed.
    /// </para>
    /// </remarks>
    /// <param name="operation">The persistence operation to execute (e.g. <c>() => _repository.InsertAsync(entity, cancellationToken)</c>).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the transactional operation.</returns>
    /// <exception cref="OperationCanceledException">When the <paramref name="cancellationToken"/> is cancelled before the commit.</exception>
    protected async Task ExecuteInTransaction(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Before opening the transaction: if it's already cancelled, there's nothing to roll back.
        cancellationToken.ThrowIfCancellationRequested();

        _unitOfWork.BeginTransaction();
        try
        {
            await operation();
            _unitOfWork.Commit();
        }
        catch
        {
            _unitOfWork.Rollback();

            throw;
        }
    }

    /// <summary>
    /// Publishes the specified domain events, one after the other and in order (see
    /// <see cref="IDomainEventPublisher.Publish(IEnumerable{IDomainEvent}, CancellationToken)"/>).
    /// </summary>
    /// <remarks>
    /// Call it after committing the changes (e.g. with the events pulled from the aggregate), so the
    /// handlers only react to changes that are already persisted. It doesn't receive a <see cref="CancellationToken"/>
    /// on purpose: the changes are already committed, so the events are never cancelled.
    /// </remarks>
    /// <param name="domainEvents">The domain events to publish (e.g. <c>aggregate.PullDomainEvents()</c>).</param>
    /// <returns>A task that represents the publish operation.</returns>
    protected Task PublishDomainEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        // NOTE: CancellationToken.None and not the request token: if the client disconnects after the commit,
        // the events can't be left half published.
        Task publishTask = _domainEventPublisher.Publish(domainEvents, CancellationToken.None);

        return publishTask;
    }

    #endregion
}

/// <summary>
/// Base class for handlers of a <see cref="ICommand"/> that produces no result. Exactly one handler must
/// exist per <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">Command type handled.</typeparam>
#pragma warning disable SA1402
public abstract class CommandHandler<TCommand> : RequestHandler<TCommand>
    where TCommand : ICommand
{
    #region Declarations

    /// <summary>Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1401:Fields should be private",
        Justification = "I prefer to use it as field just in protected fields in base classes like Repository base class.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "I prefer to use it as field just in protected fields in base classes like Repository base class.")]
    //// ReSharper disable once InconsistentNaming
    protected readonly IUnitOfWork _unitOfWork; // From Persistence (not Projection).

    /// <summary>Publishes the domain events raised by the aggregates handled by the command.</summary>
    private readonly IDomainEventPublisher _domainEventPublisher;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandler{TCommand}"/> class.
    /// </summary>
    /// <param name="domainEventPublisher">Publishes the domain events raised by the aggregates handled by the command.</param>
    /// <param name="unitOfWork">Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    protected CommandHandler(
        IDomainEventPublisher domainEventPublisher,
        IUnitOfWork unitOfWork)
    {
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    #endregion

    #region Protected methods

    /// <inheritdoc cref="CommandHandler{TCommand, TResponse}.ExecuteInTransaction(Func{Task}, CancellationToken)"/>
    protected async Task ExecuteInTransaction(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Before opening the transaction: if it's already cancelled, there's nothing to roll back.
        cancellationToken.ThrowIfCancellationRequested();

        _unitOfWork.BeginTransaction();
        try
        {
            await operation();
            _unitOfWork.Commit();
        }
        catch
        {
            _unitOfWork.Rollback();

            throw;
        }
    }

    /// <inheritdoc cref="CommandHandler{TCommand, TResponse}.PublishDomainEvents(IEnumerable{IDomainEvent})"/>
    protected Task PublishDomainEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        // NOTE: CancellationToken.None and not the request token: if the client disconnects after the commit,
        // the events can't be left half published.
        Task publishTask = _domainEventPublisher.Publish(domainEvents, CancellationToken.None);

        return publishTask;
    }

    #endregion
}
#pragma warning restore SA1402
