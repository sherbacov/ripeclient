using RestSharp;

namespace ClientsRipe
{
    public interface IRipeSearchRequest
    {
        public string QueryString { get; set; }
        
        public TypeFilter Filter { get; set; }

        public string[] Sources { get; set; }

        public void AddFilter(TypeFilter filter);

        public RestRequest GetRequest();
    }
}
