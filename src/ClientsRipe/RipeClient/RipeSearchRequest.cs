using RestSharp;

namespace ClientsRipe
{
    public class RipeSearchRequest : IRipeSearchRequest
    {
        public RipeSearchRequest() { }

        public RipeSearchRequest(string queryString)
        {
            QueryString = queryString;
        }

        public RipeSearchRequest(string queryString, TypeFilter filter)
        {
            QueryString = queryString;
            Filter = filter;
        }
        
        public string QueryString { get; set; }
        public TypeFilter Filter { get; set; } = 0;
        public RipeSearchRequestFlags Flags { get; set; }
        public string[] Sources { get; set; } = new[] {"ripe"};
        public void AddFilter(TypeFilter filter)
        {
            Filter |= filter;
        }

        public RestRequest GetRequest()
        {
             var request = new RestRequest("search", Method.Get);

             request.AddParameter("query-string", QueryString);

             foreach (var source in Sources)
             {
                 request.AddParameter("source", source);
             }
            
             if (Filter.HasFlag(TypeFilter.Route)) 
                request.AddParameter("type-filter", "route");

             if (Filter.HasFlag(TypeFilter.Route6))
                 request.AddParameter("type-filter", "route6");
             
             if (Filter.HasFlag(TypeFilter.Inetnum))
                 request.AddParameter("type-filter", "inetnum");
             
             if (Filter.HasFlag(TypeFilter.Inetnum6))
                 request.AddParameter("type-filter", "inetnum6");
             
             if (Filter.HasFlag(TypeFilter.Autnum))
                 request.AddParameter("type-filter", "aut-num");

             if (Flags == RipeSearchRequestFlags.AllMore)
             {
                 request.AddParameter("flags", "M");
             }

             if (Flags == RipeSearchRequestFlags.OneMore)
             {
                 request.AddParameter("flags", "m");
             }

             if (Flags == RipeSearchRequestFlags.AllLess)
             {
                 request.AddParameter("flags", "L");
             }
             
             if (Flags == RipeSearchRequestFlags.OneLess)
             {
                 request.AddParameter("flags", "l");
             }
             
             //TODO: Setup all types

            return request;
        }
    }
}
