using FluentAssertions;
using NestHub.Domain.Common;
using NestHub.Domain.Residents;

namespace NestHub.Domain.UnitTests.Residents;

public class ResidentTests
{
    [Fact]
    public void UpdateDetails_UpdatesNameBlockFlatAndSociety()
    {
        var resident = Resident.Create(UserId.New(), SocietyId.New(), "A", "101", "Priya Rao");
        var newSocietyId = SocietyId.New();

        resident.UpdateDetails("Priya R. Rao", "B", "202", newSocietyId);

        resident.Name.Should().Be("Priya R. Rao");
        resident.BlockNumber.Should().Be("B");
        resident.FlatNumber.Should().Be("202");
        resident.SocietyId.Should().Be(newSocietyId);
    }

    [Fact]
    public void UpdateDetails_WithBlankName_ThrowsArgumentException()
    {
        var resident = Resident.Create(UserId.New(), SocietyId.New(), "A", "101", "Priya Rao");

        var act = () => resident.UpdateDetails(" ", "B", "202", resident.SocietyId);

        act.Should().Throw<ArgumentException>();
    }
}
