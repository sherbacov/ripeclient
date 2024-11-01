using System;

namespace ClientsRipe
{
    public class RipeClientNotFoundException : Exception
    {
        public RipeClientNotFoundException(string message) : base(message)
        {
        }
    }
}