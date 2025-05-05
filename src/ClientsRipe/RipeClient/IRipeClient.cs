using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RipeDatabaseObjects;

namespace ClientsRipe
{
    public interface IRipeClient
    {
        public bool Debug { get; set; } 
            
        public IEnumerable<DatabaseObject> SearchSync(IRipeSearchRequest query);
        
        /// <summary>
        /// Searching object at RIPE database
        /// </summary>
        /// <param name="query">Searching object. Like "91.194.10.0/24"</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Founded objects from RIPE Database</returns>
        public Task<IEnumerable<DatabaseObject>> Search(IRipeSearchRequest query, CancellationToken cancellationToken);

        public Task<DatabaseObject> GetObjectByKey(string key, string objectType, string source, CancellationToken cancellationToken);
        
        /// <summary>
        /// Add object to RIPE Database
        /// </summary>
        /// <param name="obj">Object to add</param>
        /// <returns>Raw reply from RIPE</returns>
        public Task<string> AddObject(RipeObject obj, CancellationToken cancellationToken);
        public Task<RipeObjects> UpdateObject(RipeObject obj, CancellationToken cancellationToken);
        public Task RemoveObject(RipeObject obj, CancellationToken cancellationToken);
    }
}