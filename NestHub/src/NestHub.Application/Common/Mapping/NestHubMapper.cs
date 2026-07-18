using NestHub.Application.Reviews.Dtos;
using NestHub.Application.Residents.Dtos;
using NestHub.Application.SosRequests.Dtos;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Residents;
using NestHub.Domain.Reviews;
using NestHub.Domain.Societies;
using NestHub.Domain.SosRequests;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;
using Riok.Mapperly.Abstractions;

namespace NestHub.Application.Common.Mapping;

[Mapper]
public static partial class NestHubMapper
{
    [MapperIgnoreSource(nameof(Society.DomainEvents))]
    public static partial SocietyDto ToDto(this Society society);

    [MapperIgnoreSource(nameof(Resident.DomainEvents))]
    public static partial ResidentDto ToDto(this Resident resident);

    public static partial ServiceDto ToDto(this Service service);

    [MapperIgnoreSource(nameof(Vendor.DomainEvents))]
    public static partial VendorDto ToDto(this Vendor vendor);

    [MapperIgnoreSource(nameof(Review.DomainEvents))]
    public static partial ReviewDto ToDto(this Review review);

    public static partial SosClaimDto ToDto(this SosClaim sosClaim);

    [MapperIgnoreSource(nameof(SosRequest.DomainEvents))]
    public static partial SosRequestDto ToDto(this SosRequest sosRequest);

    private static Guid Map(SocietyId id) => id.Value;
    private static Guid Map(UserId id) => id.Value;
    private static Guid Map(VendorId id) => id.Value;
    private static Guid Map(ServiceId id) => id.Value;
    private static Guid Map(ResidentId id) => id.Value;
    private static Guid Map(ReviewId id) => id.Value;
    private static Guid Map(SosRequestId id) => id.Value;
    private static Guid Map(SosClaimId id) => id.Value;

    private static string Map(PhoneNumber phoneNumber) => phoneNumber.Value;

    private static string Map(PricingInfo pricing) => pricing.ToString();

    private static int Map(Rating rating) => rating.Value;
}
