using FluentAssertions;
using Moq;
using NestHub.Application.Vendors.Commands.RegisterVendor;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.UnitTests.Vendors;

public class RegisterVendorCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_AddsVendorAndSaves()
    {
        var vendorRepository = new Mock<IVendorRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RegisterVendorCommandHandler(vendorRepository.Object, unitOfWork.Object);

        var command = new RegisterVendorCommand(Guid.NewGuid(), "Sharma Plumbing Works", "9876543210", "Reliable plumbing.");

        var result = await handler.Handle(command, CancellationToken.None);

        result.BusinessName.Should().Be("Sharma Plumbing Works");
        result.IsApproved.Should().BeFalse();
        vendorRepository.Verify(r => r.Add(It.Is<Vendor>(v => v.BusinessName == "Sharma Plumbing Works")), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
