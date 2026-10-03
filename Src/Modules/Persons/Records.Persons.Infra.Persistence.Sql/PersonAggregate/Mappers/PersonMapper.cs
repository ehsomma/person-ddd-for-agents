using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Infra.Persistence.Mappings.Abstractions.Mappers;
using Records.Persons.Domain.PersonAggregate.Enumerators;
using Records.Persons.Domain.PersonAggregate.ValueObjects;
using Records.Persons.Domain.Shared.ValueObjects;
using DataModel = Records.Persons.Infra.Persistence.Sql.PersonAggregate.Models; // Using aliases.
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

namespace Records.Persons.Infra.Persistence.Sql.PersonAggregate.Mappers;

/// <summary>
/// Defines a mapper to map a <see cref="DomainModel.Person"/> to a <see cref="DataModel.Person"/>
/// and vice versa.
/// </summary>
public class PersonMapper : IPersistanceMapper<DomainModel.Person, DataModel.Person>
{
    #region Public methods

    /// <inheritdoc />
    public DataModel.Person FromDomainToDataModel(DomainModel.Person domainPerson)
    {
        ArgumentNullException.ThrowIfNull(domainPerson);

        DomainModel.Address domainAddress = domainPerson.Address;

        DataModel.Person dataPerson = new()
        {
            Id = domainPerson.Id,
            FullName = domainPerson.FullName.Value,
            Email = domainPerson.Email.Value,
            Phone = domainPerson.Phone?.Value,
            Gender = domainPerson.Gender.Name,
            Birthdate = domainPerson.Birthdate,
            CreatedOnUtc = domainPerson.CreatedOnUtc,
            UpdatedOnUtc = domainPerson.UpdatedOnUtc,

            // The children (address and personal assets) receive the ID of the person (FK).
            Address = new DataModel.Address
            {
                Id = domainAddress.Id,
                PersonId = domainPerson.Id,
                StreetLine1 = domainAddress.StreetLine1.Value,
                StreetLine2 = domainAddress.StreetLine2?.Value,
                City = domainAddress.City?.Value,
                State = domainAddress.State?.Value,
                Country = domainAddress.Country?.Value,
                Lat = domainAddress.LatLng?.Lat,
                Lng = domainAddress.LatLng?.Lng,
            },
            PersonalAssets = domainPerson.PersonalAssets
                .Select(domainPersonalAsset => new DataModel.PersonalAsset
                {
                    Id = domainPersonalAsset.Id,
                    PersonId = domainPerson.Id,
                    Description = domainPersonalAsset.Description.Value,
                    Value = domainPersonalAsset.Value.Value,
                })
                .ToList(),
        };

        return dataPerson;
    }

    /// <inheritdoc />
    public DomainModel.Person? FromDataModelToDomain(DataModel.Person? dataPerson)
    {
        DomainModel.Person? domainPerson = null;

        if (dataPerson != null)
        {
            // DomainModel.Value.
            DomainModel.Address? domainAddress = null;
            if (dataPerson.Address != null)
            {
                DataModel.Address dataAddress = dataPerson.Address;

                // The address may not have coordinates. With "||" (not "&&") a row with only one of them
                // (corrupt data) reaches LatLng.Build, which throws instead of silently discarding it.
                LatLng? latLng = null;
                if (dataAddress.Lat != null || dataAddress.Lng != null)
                {
                    latLng = LatLng.Build(dataAddress.Lat, dataAddress.Lng);
                }

                domainAddress = DomainModel.Address.Load(
                    dataAddress.Id,
                    StreetLine.Build(dataAddress.StreetLine1),
                    StreetLine2.Build(dataAddress.StreetLine2),
                    City.Build(dataAddress.City),
                    State.Build(dataAddress.State),
                    CountryName.Build(dataAddress.Country),
                    latLng);
            }

            FullName fullName = FullName.Build(dataPerson.FullName);
            Email email = Email.Build(dataPerson.Email);
            PhoneNumber phoneNumber = PhoneNumber.Build(dataPerson.Phone);

            // DomainModel.Person.
            domainPerson = DomainModel.Person.Load(
                dataPerson.Id,
                domainAddress,
                fullName,
                email,
                phoneNumber,
                Gender.FromName(dataPerson.Gender),
                null,
                dataPerson.Birthdate,
                dataPerson.CreatedOnUtc,
                dataPerson.UpdatedOnUtc);

            // DomainModel.PersonalAsset.
            if (dataPerson.PersonalAssets != null)
            {
                foreach (DataModel.PersonalAsset dataPersonalAsset in dataPerson.PersonalAssets)
                {
                    DomainModel.PersonalAsset domainPersonalAsset = DomainModel.PersonalAsset.Load(
                        dataPersonalAsset.Id,
                        PersonalAssetDescription.Build(dataPersonalAsset.Description),
                        Money.Build(dataPersonalAsset.Value));

                    domainPerson.LoadPersonalAsset(domainPersonalAsset);
                }
            }
        }

        return domainPerson;
    }

    #endregion
}
