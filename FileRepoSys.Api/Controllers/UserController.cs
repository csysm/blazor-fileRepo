using AutoMapper;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models.UserModels;
using FileRepoSys.Api.Repository.Contract;
using Microsoft.AspNetCore.Mvc;

namespace FileRepoSys.Api.Controllers
{
    [Route("user")]
    [ApiController]
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
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneUser(Guid.Parse(id), cancellationToken);
            var userDto = _mapper.Map<User, UserDto>(user);
            return Ok(userDto);
        }

        [HttpGet]
        [Route("{keyword}/{desc}")]
        public async Task<IActionResult> Get(string keyword, int desc, CancellationToken cancellationToken)
        {

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var users = await _userRepository.GetUsersByPage(user => user.UserName.Contains(keyword), 10, 1, cancellationToken, Convert.ToBoolean(desc));
                var userDtos = _mapper.Map<List<User>, List<UserDto>>(users);
                return Ok(userDtos);
            }
            return Ok("no keyword");
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Post(UserAddViewModel viewModel, CancellationToken cancellationToken)
        {
            User newUser = new()
            {
                Email = viewModel.Email,
                Password = viewModel.Password,
                UserName = viewModel.UserName,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            int isSuccess = await _userRepository.AddOneUser(newUser, cancellationToken);
            if (isSuccess == 0)//若邮箱已经被注册
            {
                return Ok("email existed");
            }
            return Ok("sign up success");
        }

        [HttpPut]
        [Route("update/profile")]
        public async Task<IActionResult> Put(UserProfileUpdateViewModel viewModel, CancellationToken cancellationToken)
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
        public async Task<IActionResult> Put(UserSecurityUpdateViewModel viewModel, CancellationToken cancellationToken)
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

        [HttpPut]
        [Route("active")]
        public async Task<IActionResult> Put(string id,CancellationToken cancellationToken)
        {
            var flag= await _userRepository.ActiveUser(Guid.Parse(id), cancellationToken);
            if(flag == 0)
            {
                return NotFound("active fail for no such user");
            }
            return Ok("active success");
        }
    }
}
