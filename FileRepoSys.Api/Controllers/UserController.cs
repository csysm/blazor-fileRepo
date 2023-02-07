using AutoMapper;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models.UserModels;
using FileRepoSys.Api.Repository.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileRepoSys.Api.Controllers
{
    [Route("user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }
        
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<UserDto>> Get(string id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneUser(Guid.Parse(id), cancellationToken);
            var userDto = _mapper.Map<User, UserDto>(user);
            return Ok(userDto);
        }

        [HttpPut]
        [Route("update/profile")]
        public async Task<IActionResult> Put([FromBody] UserProfileUpdateViewModel viewModel, CancellationToken cancellationToken)
        {
            User user = new()
            {
                Id = viewModel.Id,
                UserName = viewModel.UserName
            };
            var flag = await _userRepository.UpdateUserName(user, cancellationToken);
            if (flag == 0)
            {
                return Ok("update fail");
            }
            return Ok("update success");
        }

        [HttpPut]
        [Route("update/security")]
        public async Task<IActionResult> Put([FromBody] UserSecurityUpdateViewModel viewModel, CancellationToken cancellationToken)
        {
            User user = new()
            {
                Id=viewModel.Id,
                Password = viewModel.NewPassword
            };
            var flag = await _userRepository.UpdateUserPassword(viewModel.OldPassword, user, cancellationToken);
            if (flag == 0)
            {
                return Ok("wrong password");
            }
            return Ok("update success");
        }
    }
}
