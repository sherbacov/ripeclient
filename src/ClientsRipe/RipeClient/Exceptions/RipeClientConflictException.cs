using System;

namespace ClientsRipe
{
    public class RipeClientConflictException: Exception
    {
        public RipeClientConflictException(string content)
        {
            Content = content;
        }
        
        public string Content { get; }
    }
}