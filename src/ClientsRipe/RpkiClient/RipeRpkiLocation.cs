namespace ClientsRpki
{
    public interface IRipeRpkiLocation
    {
        public string Url { get; set; }
    }

    public class RipeRpkiTestLocation : IRipeRpkiLocation
    {
        public string Url { get; set; } = "https://localcert.ripe.net/api/rpki";
    }

    public class RipeRpkiProductionLocation : IRipeRpkiLocation
    {
        public string Url { get; set; } = "https://my.ripe.net/api/rpki";
    }
}
