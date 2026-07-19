using FluentAssertions;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.UnitTests.ValueObjects;

public class GeoLocationTests
{
    [Fact]
    public void ToPoint_ThenFromPoint_RoundTripsExactly()
    {
        var original = GeoLocation.Create(17.4448, 78.3498);

        var point = original.ToPoint();
        var roundTripped = GeoLocation.FromPoint(point);

        roundTripped.Latitude.Should().Be(original.Latitude);
        roundTripped.Longitude.Should().Be(original.Longitude);
    }

    [Fact]
    public void ToPoint_UsesLongitudeAsXAndLatitudeAsY()
    {
        // NetTopologySuite's Point convention is (X, Y) = (Longitude, Latitude) — the inverse
        // of how this codebase reads/writes coordinates everywhere else. This test guards
        // against ever silently swapping the axes again.
        var location = GeoLocation.Create(17.4448, 78.3498);

        var point = location.ToPoint();

        point.X.Should().Be(78.3498);
        point.Y.Should().Be(17.4448);
    }
}
