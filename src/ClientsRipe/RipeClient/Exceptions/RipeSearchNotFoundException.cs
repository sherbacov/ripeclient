using System;

namespace ClientsRipe
{
    public class RipeSearchNotFoundException : Exception
    {
        public RipeSearchNotFoundException() : base() { }
        public RipeSearchNotFoundException(string message) : base(message)  { }
    }
}