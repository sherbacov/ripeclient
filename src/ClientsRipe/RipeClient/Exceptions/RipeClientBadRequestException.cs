using System;
using RipeDatabaseObjects;

namespace ClientsRipe
{
    public class RipeClientBadRequestException : Exception {
    
        public RipeClientBadRequestException(string message) : base(message)
        {
        }

        public ErrorMessages ErrorMessages { get; set; }
        
        public RipeClientBadRequestException(ErrorMessages errorMessages)
        {
            ErrorMessages = errorMessages;
        }
    }
}