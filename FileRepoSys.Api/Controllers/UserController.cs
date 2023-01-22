using FileRepoSys.Api.Repository.Contract;
using Microsoft.AspNetCore.Mvc;

namespace FileRepoSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        public UserController(ILogger logger IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> User(Guid id)
        {

        }
    }
}
