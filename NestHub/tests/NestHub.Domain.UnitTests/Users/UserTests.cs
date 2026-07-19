using FluentAssertions;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Users;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.UnitTests.Users;

public class UserTests
{
    [Fact]
    public void AssignToSociety_SetsSocietyId()
    {
        var user = User.Register(PhoneNumber.Create("9876543210"), null, "hash", UserType.Admin);
        var societyId = SocietyId.New();

        user.AssignToSociety(societyId);

        user.SocietyId.Should().Be(societyId);
    }

    [Fact]
    public void AssignToSociety_WithNull_MakesUserCentralAdmin()
    {
        var user = User.Register(PhoneNumber.Create("9876543210"), null, "hash", UserType.Admin, SocietyId.New());

        user.AssignToSociety(null);

        user.SocietyId.Should().BeNull();
    }
}
