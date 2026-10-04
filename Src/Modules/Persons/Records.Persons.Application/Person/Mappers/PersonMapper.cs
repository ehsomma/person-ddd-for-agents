using BuildingBlocks.Application.Mappings.Abstractions.Mappers;
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Mappers;

/// <summary>
/// Represents a mapper to map a <see cref="DomainModel.Person"/> to a <see cref="Dto.Person"/>.
/// </summary>
internal sealed class PersonMapper : IDomainMapper<DomainModel.Person, Dto.Person>
{
    #region Public methods

    /// <inheritdoc />
    public Dto.Person FromDomainToDto(DomainModel.Person domainPerson)
    {
        ArgumentNullException.ThrowIfNull(domainPerson);

        DomainModel.Address domainAddress = domainPerson.Address;

        Dto.Person personDto = new()
        {
            Id = domainPerson.Id,
            FullName = domainPerson.FullName.Value,
            Email = domainPerson.Email.Value,
            Phone = domainPerson.Phone?.Value,
            Gender = domainPerson.Gender.Name,
            Birthdate = domainPerson.Birthdate,
            Address = new Dto.Address
            {
                Id = domainAddress.Id,
                StreetLine1 = domainAddress.StreetLine1.Value,
                StreetLine2 = domainAddress.StreetLine2?.Value,
                City = domainAddress.City?.Value,
                State = domainAddress.State?.Value,
                Country = domainAddress.Country?.Value,
                LatLng = domainAddress.LatLng == null
                    ? null
                    : new Dto.LatLng { Lat = domainAddress.LatLng.Lat, Lng = domainAddress.LatLng.Lng },
            },
            PersonalAssets = domainPerson.PersonalAssets
                .Select(domainPersonalAsset => new Dto.PersonalAsset
                {
                    Id = domainPersonalAsset.Id,
                    Description = domainPersonalAsset.Description.Value,
                    Value = domainPersonalAsset.Value.Value,
                })
                .ToList(),
        };

        return personDto;
    }

    #endregion
}
