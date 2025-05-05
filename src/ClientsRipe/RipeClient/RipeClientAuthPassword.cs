using System.Threading.Tasks;

namespace ClientsRipe
{
    public class RipeClientAuthPassword : IRipeClientAuth
    {
        private readonly string _password;

        public RipeClientAuthPassword(string password)
        {
            _password = password;
        }
        
        public Task<string> GetSecret()
        {
            return Task.FromResult(_password);
        }
    }
}