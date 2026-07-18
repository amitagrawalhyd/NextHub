using FluentAssertions;
using Moq;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.SosRequests.Commands.ClaimSosRequest;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.SosRequests;

namespace NestHub.Application.UnitTests.SosRequests;

public class ClaimSosRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_ClaimsRequestAndNotifiesResident()
    {
        var sosRequest = SosRequest.RaiseNew(ResidentId.New(), SocietyId.New(), "Plumbing", "Leaking pipe!");
        var sosRequestRepository = new Mock<ISosRequestRepository>();
        sosRequestRepository.Setup(r => r.GetByIdAsync(sosRequest.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sosRequest);

        var notificationService = new Mock<INotificationService>();
        var vendorId = VendorId.New();

        var handler = new ClaimSosRequestCommandHandler(sosRequestRepository.Object, Mock.Of<IUnitOfWork>(), notificationService.Object);

        var result = await handler.Handle(new ClaimSosRequestCommand(sosRequest.Id.Value, vendorId.Value), CancellationToken.None);

        result.VendorId.Should().Be(vendorId.Value);
        sosRequest.Status.Should().Be(Domain.Enums.SosStatus.Claimed);
        notificationService.Verify(
            n => n.NotifySosRequestClaimedAsync(sosRequest.Id, sosRequest.ResidentId, vendorId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
