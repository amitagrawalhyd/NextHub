using FluentAssertions;
using Moq;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Users.Commands.Login;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.Users;
using NestHub.Domain.ValueObjects;

namespace NestHub.Application.UnitTests.Users;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var user = User.Register(PhoneNumber.Create("9876543210"), null, "hashed-password", UserType.Resident);
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.GetByPhoneNumberAsync(It.IsAny<PhoneNumber>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(p => p.Verify("wrong-password", "hashed-password")).Returns(false);

        var handler = new LoginCommandHandler(userRepository.Object, passwordHasher.Object, Mock.Of<IJwtTokenGenerator>());

        var act = () => handler.Handle(new LoginCommand("9876543210", "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAuthResultWithToken()
    {
        var user = User.Register(PhoneNumber.Create("9876543210"), null, "hashed-password", UserType.Resident);
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.GetByPhoneNumberAsync(It.IsAny<PhoneNumber>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(p => p.Verify("correct-password", "hashed-password")).Returns(true);

        var tokenGenerator = new Mock<IJwtTokenGenerator>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        tokenGenerator.Setup(t => t.Generate(user.Id, UserType.Resident)).Returns(new GeneratedToken("jwt-token", expiresAt));

        var handler = new LoginCommandHandler(userRepository.Object, passwordHasher.Object, tokenGenerator.Object);

        var result = await handler.Handle(new LoginCommand("9876543210", "correct-password"), CancellationToken.None);

        result.Token.Should().Be("jwt-token");
        result.UserId.Should().Be(user.Id.Value);
        result.UserType.Should().Be(nameof(UserType.Resident));
    }
}
