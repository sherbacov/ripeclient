using System;
using RestSharp;

namespace ClientsRipe
{
    [Flags]
    public enum TypeFilter : short
    {
        None = 0,
        Autnum = 1,
        Inetnum = 2,
        Inetnum6 = 4,
        Route = 8,
        Route6 = 16,
        Person = 32
    }

    public interface IRipeSearchRequest
    {
        public string QueryString { get; set; }
        
        public TypeFilter Filter { get; set; }

        public string[] Sources { get; set; }

        public void AddFilter(TypeFilter filter);

        public RestRequest GetRequest();
    }

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
            
             if (Filter == TypeFilter.Route) 
                request.AddParameter("type-filter", "route");

             if (Filter == TypeFilter.Inetnum) 
                 request.AddParameter("type-filter", "inetnum");
             
             if (Filter == TypeFilter.Autnum)
                 request.AddParameter("type-filter", "aut-num");

             //TODO: Setup all types

            return request;
        }
    }
}
