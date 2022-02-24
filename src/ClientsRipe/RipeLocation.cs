namespace ClientsRipe
{
    public interface IRipeLocation
    {
        public string Url { get; set; }
    }

    public class RipeNonSecureLocation : IRipeLocation
    {
        public string Url { get; set; } = "http://rest.db.ripe.net/";
    }

    public class RipeSecureLocation : IRipeLocation
    {
        public string Url { get; set; } = "https://rest.db.ripe.net/";
    }
}