using FluentValidation;
using MediatR;
using NestHub.Domain.Analytics;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Analytics.Commands.RecordAnalyticsEvent;

public sealed record RecordAnalyticsEventCommand(Guid VendorId, AnalyticsActionType ActionType) : IRequest<Unit>;

public sealed class RecordAnalyticsEventCommandValidator : AbstractValidator<RecordAnalyticsEventCommand>
{
    public RecordAnalyticsEventCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.ActionType).IsInEnum();
    }
}

public sealed class RecordAnalyticsEventCommandHandler : IRequestHandler<RecordAnalyticsEventCommand, Unit>
{
    private readonly IAnalyticsLogRepository _analyticsLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordAnalyticsEventCommandHandler(IAnalyticsLogRepository analyticsLogRepository, IUnitOfWork unitOfWork)
    {
        _analyticsLogRepository = analyticsLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RecordAnalyticsEventCommand request, CancellationToken cancellationToken)
    {
        var log = AnalyticsLog.Record(new VendorId(request.VendorId), request.ActionType);
        _analyticsLogRepository.Add(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
