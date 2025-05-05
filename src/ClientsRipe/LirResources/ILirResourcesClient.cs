using System.Threading;
using System.Threading.Tasks;
using ClientsRipe.LirResources.Models;

namespace ClientsRipe.LirResources;

public interface ILirResourcesClient
{
    public bool Debug { get; set; }

    public Task<LirResourcesReply> GetResources(string apiKey, CancellationToken cancellationToken);
}
