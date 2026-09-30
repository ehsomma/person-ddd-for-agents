using BuildingBlocks.Infra.Persistence.Abstractions;
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

    /// <summary>Mediator library to send and handle commands and queries implementing CQRS.</summary>
    private readonly IMediator _mediator;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandler{TCommand, TResponse}"/> class.
    /// </summary>
    /// <param name="mediator">Implementation of mediator pattern to send and handle commands and queries implementing CQRS.</param>
    /// <param name="unitOfWork">Manage a <see cref="IDbSession"/> to encapsulate a business transaction which can affect the database.</param>
    /// <exception cref="ArgumentNullException">When some argument for the constructor parameters is null.</exception>
    protected CommandHandler(
        IMediator mediator,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
}
#pragma warning restore SA1402
