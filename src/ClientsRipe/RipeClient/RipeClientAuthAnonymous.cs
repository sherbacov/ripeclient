using System.Threading.Tasks;

namespace ClientsRipe
{
    public class RipeClientAuthAnonymous : IRipeClientAuth
    {
        public Task<string> GetSecret()
        {
            return Task.FromResult("");
        }
    } 
}