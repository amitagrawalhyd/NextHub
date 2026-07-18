using MediatR;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.SearchVendors;

public sealed record SearchVendorsQuery(string? Query, string? Category) : IRequest<IReadOnlyList<VendorDto>>;

public sealed class SearchVendorsQueryHandler : IRequestHandler<SearchVendorsQuery, IReadOnlyList<VendorDto>>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IAiService _aiService;

    public SearchVendorsQueryHandler(IVendorRepository vendorRepository, IAiService aiService)
    {
        _vendorRepository = vendorRepository;
        _aiService = aiService;
    }

    public async Task<IReadOnlyList<VendorDto>> Handle(SearchVendorsQuery request, CancellationToken cancellationToken)
    {
        var candidates = string.IsNullOrWhiteSpace(request.Category)
            ? await _vendorRepository.GetAllApprovedAsync(cancellationToken)
            : await _vendorRepository.GetByCategoryAsync(request.Category, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Query))
            return candidates.Select(v => v.ToDto()).ToList();

        var searchable = candidates
            .Select(v => new SearchableVendor(
                v.Id.Value,
                v.BusinessName,
                v.Bio ?? string.Empty,
                v.Services.Select(s => s.Title).ToList(),
                v.Services.Select(s => s.Category).Distinct().ToList()))
            .ToList();

        var rankedIds = _aiService.RankVendorsBySearchRelevance(request.Query, searchable);
        var vendorsById = candidates.ToDictionary(v => v.Id, v => v);

        return rankedIds
            .Select(id => new VendorId(id))
            .Where(vendorsById.ContainsKey)
            .Select(id => vendorsById[id].ToDto())
            .ToList();
    }
}
