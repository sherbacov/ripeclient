namespace ClientsRipe
{
    public class RipeNonSecureLocation : IRipeLocation
    {
        public string Url { get; set; } = "http://rest.db.ripe.net/";
    }
}