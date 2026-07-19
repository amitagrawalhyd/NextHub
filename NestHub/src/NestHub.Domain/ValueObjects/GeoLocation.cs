using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace NestHub.Domain.ValueObjects;

public sealed record GeoLocation
{
    private const int Wgs84Srid = 4326;
    private static readonly GeometryFactory Factory = NtsGeometryServices.Instance.CreateGeometryFactory(Wgs84Srid);

    public double Latitude { get; }
    public double Longitude { get; }

    private GeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be between -180 and 180.");

        return new GeoLocation(latitude, longitude);
    }

    /// <summary>
    /// NetTopologySuite's Point convention is (X, Y) = (Longitude, Latitude) — the inverse of
    /// how everyone reads/writes coordinates in this codebase. Centralizing the axis swap here
    /// means it only has to be gotten right once.
    /// </summary>
    public Point ToPoint() => Factory.CreatePoint(new Coordinate(Longitude, Latitude));

    public static GeoLocation FromPoint(Point point) => Create(point.Y, point.X);
}
