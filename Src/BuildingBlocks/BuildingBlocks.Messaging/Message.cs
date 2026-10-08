namespace BuildingBlocks.Messaging;

/*
Analogía
========

Analogía: La carta
En el código: PersonCreatedEvent: el contenido, lo que
pasó en el negocio.
────────────────────────────────────────
Analogía: El sobre (genérico)
En el código: Message<TContent> (BuildingBlocks.Messaging): envuelve la carta, sin importar por qué
correo va a viajar
────────────────────────────────────────
Analogía: El sobre del correo interno
En el código: DomainMessage<TEvent>: un Message<TEvent> con la estampilla (INotification) del correo
interno (el mediator)
────────────────────────────────────────
Analogía: Lo escrito en el sobre
En el código: MessageMetadata: número de seguimiento
(MessageId), número de trámite (CorrelationId), "en
respuesta a la carta X" (CausationId) y de qué
expediente se trata (ContentId, el id del agregado)
────────────────────────────────────────
Analogía: La estampilla
En el código: INotification: lo que hace que el correo la acepte
────────────────────────────────────────
Analogía: El correo
En el código: IPublisher / Mediator
────────────────────────────────────────
Analogía: Los destinatarios
En el código: Los DomainEventHandler<TEvent>

Algunos matices que la hacen más precisa:
1. La carta no lleva estampilla. Es justo la decisión de diseño: IDomainEvent no implementa
   INotification. El que escribe la carta (el dominio) no sabe nada del correo. El franqueo lo pone,
   quien la manda, que es la capa de Application.

2. Es más un sobre prefranqueado que una estampilla pegada. INotification no se agrega al momento de
   enviar: es parte del tipo, porque DomainMessage<TEvent> ya "viene impreso" con franqueo. Además es
   una interfaz marcadora vacía, igual que una estampilla, que no dice nada del contenido. Solo habilita
   el envío.

3. Es una circular, no una carta con un destinatario. El correo la entrega a todos los suscriptos a
   ese tipo de carta, que pueden ser 0, 1 o N handlers. Si no hay ninguno, no se devuelve al remitente:
   simplemente nadie la lee. Esa es la diferencia con un IRequest, que es una carta certificada con un
   único destinatario obligatorio y respuesta.
*/

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
