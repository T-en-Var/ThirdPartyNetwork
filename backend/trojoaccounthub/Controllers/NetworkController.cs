using Microsoft.AspNetCore.Mvc;
using System.Net;
using trojoaccounthub.Dtos;
using trojoaccounthub.Exceptions;

namespace trojoaccounthub.Controllers
{
    [Route("api/networks")]
    [ApiController]
    public class NetworkController : Controller
    {
        private readonly INetworkService _service;

        public NetworkController(INetworkService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
        public IActionResult GetNetworks()
        {
            ICollection<NetworkDto> networks = _service.GetAll();
            return networks == null
                ? throw new UnableToHttpException("fetch Networks")
                : StatusCode(200, networks);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(NetworkDto))]
        [ProducesResponseType(404)]
        public IActionResult GetNetworkById(int id)
        {
            NetworkDto? network = _service.GetById(id);
            return StatusCode(200, network);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(NetworkDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult CreateNetwork([FromBody] CreateNetworkDto networkCreate)
        {
            NetworkDto? network = _service.Create(networkCreate);
            return network == null 
                ? throw new UnableToHttpException("create Network") 
                : StatusCode(201, network);
        }

        [HttpPatch("/{id}/")]
        public IActionResult Update(int id, UpdateNetworkDto networkUpdate)
        {
            NetworkDto? network = _service.Update(id, networkUpdate);
            return network == null
                ? throw new UnableToHttpException("update Network")
                : StatusCode(200, network);
        }
    }
}
