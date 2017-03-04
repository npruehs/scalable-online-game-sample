using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace LobbyActor.Interfaces
{
    public interface ILobbyActor : IActor
    {
        Task<LoginResponse> LoginAsync(string playerId, CancellationToken cancellationToken);
    }
}
