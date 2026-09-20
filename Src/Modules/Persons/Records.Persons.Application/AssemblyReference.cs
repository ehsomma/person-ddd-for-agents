namespace Records.Persons.Application;

/// <summary>
/// Tipo marcador sin logica, usado solo para obtener una referencia a este assembly
/// (<c>typeof(AssemblyReference).Assembly</c>) al registrar los handlers de CQRS, sin acoplarse
/// a ningun command/query handler concreto.
/// </summary>
public sealed class AssemblyReference
{
}
