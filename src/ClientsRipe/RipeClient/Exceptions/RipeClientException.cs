using System;

namespace ClientsRipe
{
    public class RipeClientException : Exception
    {
        public RipeClientException(string message) : base(message)
        {
        }
    }
}