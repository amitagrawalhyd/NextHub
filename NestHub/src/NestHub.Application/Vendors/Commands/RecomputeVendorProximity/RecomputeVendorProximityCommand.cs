using FluentValidation;
using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Commands.RecomputeVendorProximity;

/// <summary>
/// The core automation: given a vendor whose location just changed, finds every active society
/// within <see cref="NearbyRadiusKm"/> of the vendor's own pin (excluding whichever society, if
/// any, is already that vendor's explicit InHouse affiliation) and replaces the vendor's
/// auto-computed Nearby coverage rows with that set. Invoked event-driven, via
/// VendorLocationChangedDomainEventHandler — never called directly from a controller/page.
/// </summary>
public sealed record RecomputeVendorProximityCommand(Guid VendorId) : IRequest<Unit>;

public sealed class RecomputeVendorProximityCommandValidator : AbstractValidator<RecomputeVendorProximityCommand>
{
    public RecomputeVendorProximityCommandValidator() => RuleFor(x => x.VendorId).NotEmpty();
}

public sealed class RecomputeVendorProximityCommandHandler : IRequestHandler<RecomputeVendorProximityCommand, Unit>
{
    public const double NearbyRadiusKm = 5.0;

    private readonly IVendorRepository _vendorRepository;
    private readonly ISocietyRepository _societyRepository;
    private readonly IVendorSocietyCoverageRepository _coverageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecomputeVendorProximityCommandHandler(
        IVendorRepository vendorRepository,
        ISocietyRepository societyRepository,
        IVendorSocietyCoverageRepository coverageRepository,
        IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _societyRepository = societyRepository;
        _coverageRepository = coverageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RecomputeVendorProximityCommand request, CancellationToken cancellationToken)
    {
        var vendorId = new VendorId(request.VendorId);
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);

        // Vendor may have been deleted between the event being raised and this handler running
        // (or simply no longer has a location) — either way, no nearby societies apply.
        if (vendor?.GeoLocation is null)
        {
            await _coverageRepository.ReplaceNearbyForVendorAsync(vendorId, Array.Empty<SocietyId>(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        var inHouse = await _coverageRepository.GetInHouseForVendorAsync(vendorId, cancellationToken);
        var nearbySocieties = await _societyRepository.GetWithinRadiusAsync(vendor.GeoLocation, NearbyRadiusKm, cancellationToken);

        var nearbySocietyIds = nearbySocieties
            .Select(s => s.Id)
            .Where(id => inHouse is null || id != inHouse.SocietyId);

        await _coverageRepository.ReplaceNearbyForVendorAsync(vendorId, nearbySocietyIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
