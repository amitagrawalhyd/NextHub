using FluentAssertions;
using NestHub.Domain.Exceptions;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.UnitTests.ValueObjects;

public class RatingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Create_WithValueWithinRange_Succeeds(int value)
    {
        var rating = Rating.Create(value);

        rating.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithValueOutsideRange_ThrowsInvalidRatingException(int value)
    {
        var act = () => Rating.Create(value);

        act.Should().Throw<InvalidRatingException>();
    }
}
