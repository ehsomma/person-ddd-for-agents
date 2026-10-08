namespace BuildingBlocks.Messaging;

/// <summary>
/// Represents a message: the data to be delivered to a consumer or handler (the <see cref="Content"/>,
/// e.g. a domain or integration event) together with its <see cref="MessageMetadata"/>.
/// </summary>
/// <remarks>
/// It knows nothing about how it is delivered (mediator, broker, Outbox): each transport defines its own
/// subclass when needed (e.g. <c>DomainMessage&lt;TEvent&gt;</c> adds the mediator's <c>INotification</c>).
/// </remarks>
/// <typeparam name="TContent">The type of the content of the message.</typeparam>
public class Message<TContent>
    where TContent : notnull
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Message{TContent}"/> class.
    /// </summary>
    /// <param name="metadata"><inheritdoc cref="Metadata" path="/summary"/></param>
    /// <param name="content"><inheritdoc cref="Content" path="/summary"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="metadata"/> or <paramref name="content"/> is <see langword="null"/>.</exception>
    public Message(MessageMetadata metadata, TContent content)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(content);

        Metadata = metadata;
        Content = content;
    }

    #endregion

    #region Properties

    /// <summary>The metadata of the message.</summary>
    public MessageMetadata Metadata { get; }

    /// <summary>The content of the message (e.g. a domain event).</summary>
    public TContent Content { get; }

    #endregion
}
