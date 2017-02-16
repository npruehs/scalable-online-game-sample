using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using LobbyActor.Interfaces;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        // GET api/login/5
        [HttpGet("{id}")]
        public async Task<string> Get(string id)
        {
            var lobbyActor = ActorProxy.Create<ILobbyActor>(new ActorId(0));
            var count = await lobbyActor.GetCountAsync(new System.Threading.CancellationToken());
            return count.ToString();
        }
    }
}
