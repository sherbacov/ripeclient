using System.Threading;
using System.Threading.Tasks;
using RipeDatabaseObjects;


namespace ClientsRpki
{
    public interface IRipeDatabaseRoute
    {
        public Task Add(string route, string origin, CancellationToken cancellationToken);
        public Task Remove(string route, string origin, CancellationToken cancellationToken);
        
        public Task Add(RipeRoute route, CancellationToken cancellationToken);
        public Task Remove(RipeRoute route, CancellationToken cancellationToken);
    }
}