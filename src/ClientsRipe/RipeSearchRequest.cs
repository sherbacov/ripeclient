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


    public enum RipeSearchRequestFlags
    {
        /// <summary>
        /// None flags
        /// </summary>
        None,
        /// <summary>
        /// -M (or --all-more) - requests that the server should return all the sub-ranges
        /// that are completely contained within the reference range. When there are
        /// hierarchies of sub-ranges, all levels of the hierarchies will be returned down
        /// to the smallest sub-range in each hierarchy. Many objects can be returned.
        /// </summary>
        AllMore,
        /// <summary>
        /// -m (or --one-more) - requests that the server should return all the sub-ranges
        /// that are completely contained within the reference range. When there are
        /// hierarchies of sub-ranges, only the top level of the hierarchies will be returned.
        /// These are usually called one level more specific ranges. Many objects can be returned.
        /// </summary>
        OneMore,
        
        /// <summary>
        /// -L (or --all-less) - requests that the server returns the object with an exact match
        /// range, if it exists, and all objects with ranges that are bigger than the reference
        /// range and that fully contain, or encompass, it. Many objects can be returned.
        /// </summary>
        AllLess,
        /// <summary>
        /// -l (or --one-less) - requests that the server does not return the object with an exact
        /// match range, if it exists. It returns only the object with the smallest range that is
        /// bigger than the reference range and that fully contains, or encompasses, it. This is
        /// usually referred to as the one level less specific range. Only one object can be
        /// returned. Less Specific Range Queries For Referenced IRT Objects
        /// </summary>
        OneLess,
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
