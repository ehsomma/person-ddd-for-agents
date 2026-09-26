using System.Reflection;

namespace Records.Shared.Messaging;

/// <summary>
/// Represents the metadata of a message: its identity and its place in the chain of messages that
/// caused it (correlation/causation).
/// </summary>
public class MessageMetadata
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageMetadata"/> class for a root message (one not
    /// caused by another message), so its <see cref="CorrelationId"/> and <see cref="CausationId"/> are
    /// its own <see cref="MessageId"/>.
    /// </summary>
    /// <param name="contentId"><inheritdoc cref="ContentId" path="/summary"/></param>
    public MessageMetadata(string contentId)
        : this(Guid.Empty, Guid.Empty, contentId)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageMetadata"/> class for a message caused by
    /// another one: it keeps the causation message's <see cref="CorrelationId"/> and
    /// <see cref="ContentId"/>, and its <see cref="CausationId"/> is the causation message's
    /// <see cref="MessageId"/>.
    /// </summary>
    /// <param name="causationMetadata">The <see cref="MessageMetadata"/> of the message that caused this one.</param>
    /// <exception cref="ArgumentNullException"><paramref name="causationMetadata"/> is <see langword="null"/>.</exception>
    public MessageMetadata(MessageMetadata causationMetadata)
        : this(
            EnsureNotNull(causationMetadata).CorrelationId,
            causationMetadata.MessageId,
            causationMetadata.ContentId)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageMetadata"/> class.
    /// </summary>
    /// <param name="correlationId">The id of the first message of the chain, or <see cref="Guid.Empty"/> for a root message.</param>
    /// <param name="causationId">The id of the message that caused this one, or <see cref="Guid.Empty"/> for a root message.</param>
    /// <param name="contentId"><inheritdoc cref="ContentId" path="/summary"/></param>
    private MessageMetadata(Guid correlationId, Guid causationId, string contentId)
    {
        MessageId = Guid.NewGuid();

        // Mensaje raiz: no hay mensaje previo en la cadena, asi que el mismo es su correlation y causation.
        if (correlationId == Guid.Empty)
        {
            correlationId = MessageId;
        }

        if (causationId == Guid.Empty)
        {
            causationId = correlationId;
        }

        CorrelationId = correlationId;
        CausationId = causationId;
        CreatedOnUtc = DateTime.UtcNow;
        Host = Assembly.GetEntryAssembly()?.GetName().Name ?? "(unresolved)";
        Version = "1";
        ContentId = contentId;
    }

    #endregion

    #region Properties

    /// <summary>The unique id of the message.</summary>
    public Guid MessageId { get; init; }

    /// <summary>The id of the first message of the chain that caused the occurrence of this message.</summary>
    public Guid CorrelationId { get; init; }

    /// <summary>The id of the message that caused the occurrence of this message.</summary>
    public Guid CausationId { get; init; }

    /// <summary>When the message was created (UTC).</summary>
    public DateTime CreatedOnUtc { get; init; }

    /// <summary>The name of the assembly (application) where the message was created.</summary>
    public string Host { get; init; }

    /// <summary>The version of the message format.</summary>
    public string Version { get; init; }

    /// <summary>The id of the object in the content of the message (e.g. the aggregate id of a domain event).</summary>
    /// <remarks>
    /// Avoids navigating through the message content to know which object it refers to.
    /// NOTE: the id could be a string, Guid, int, etc., so it is exposed as a string.
    /// </remarks>
    public string ContentId { get; init; }

    #endregion

    #region Private methods

    /// <summary>
    /// Validates the argument before chaining to the private constructor (a constructor initializer
    /// cannot run statements before calling <c>this(...)</c>).
    /// </summary>
    private static MessageMetadata EnsureNotNull(MessageMetadata causationMetadata)
    {
        ArgumentNullException.ThrowIfNull(causationMetadata);
        return causationMetadata;
    }

    #endregion
}
