namespace BuildingBlocks.Application.Mappings.Abstractions.Mappers;

/// <summary>
/// Defines a mapper to map a TDomain to a TDto.
/// </summary>
/// <remarks>
/// It doesn't map from TDto to TDomain: building a domain model depends on the use case (e.g. Create()
/// registers domain events and Load() doesn't), so that is done in each command handler.
/// </remarks>
/// <typeparam name="TDomain">The type of the domain model object.</typeparam>
/// <typeparam name="TDto">The type of the DTO object.</typeparam>
public interface IDomainMapper<in TDomain, out TDto>
{
    /// <summary>
    /// Maps the specified domain model to a DTO.
    /// </summary>
    /// <param name="domainModel">The domain model to map from.</param>
    /// <returns>A DTO.</returns>
    TDto FromDomainToDto(TDomain domainModel);
}
