using System.Threading.Tasks;

namespace ClientsRipe
{
    public interface IRipeClientAuth
    {
        Task<string> GetSecret();
    }
}