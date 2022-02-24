namespace RipeDatabaseObjects
{
    /// <summary>
    /// ROUTE Object
    /// https://www.ripe.net/manage-ips-and-asns/db/support/documentation/ripe-database-documentation/rpsl-object-types/4-2-descriptions-of-primary-objects/4-2-5-description-of-the-route-object
    /// </summary>
    public class Route : RipeObject
    {
        public override string GetKey(bool allowSpaces)
        {
            return this["route"];
        }

        public override string GetRipeObjectType()
        {
            return "route";
        }
    }
}