using FluentAssertions;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("9876543210")]
    [InlineData("+919876543210")]
    public void Create_WithValidNumber_Succeeds(string value)
    {
        var phoneNumber = PhoneNumber.Create(value);

        phoneNumber.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc123")]
    [InlineData("123")]
    public void Create_WithInvalidNumber_Throws(string value)
    {
        var act = () => PhoneNumber.Create(value);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoPhoneNumbers_WithSameValue_AreEqual()
    {
        var first = PhoneNumber.Create("9876543210");
        var second = PhoneNumber.Create("9876543210");

        first.Should().Be(second);
    }
}
