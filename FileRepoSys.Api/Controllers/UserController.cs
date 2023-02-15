using AutoMapper;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models.UserModels;
using FileRepoSys.Api.Repository.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace FileRepoSys.Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        //private readonly IDistributedCache _distributedCache;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, IMapper mapper, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }
        
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<UserDto>> Get(string id, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetOneUser(Guid.Parse(id), cancellationToken);
                var userDto = _mapper.Map<User, UserDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("something wrong");
            }
            
        }

        [HttpPut]
        [Route("update/profile")]
        public async Task<IActionResult> Put([FromBody] UserProfileUpdateViewModel viewModel, CancellationToken cancellationToken)
        {
            try
            {
                User user = new()
                {
                    Id = viewModel.Id,
                    UserName = viewModel.UserName
                };
                var flag = await _userRepository.UpdateUserName(user, cancellationToken);
                if (flag == 0)
                {
                    return BadRequest("update fail");
                }

                await _distributedCache.RemoveAsync($"user-{viewModel.Id}");//remove cache
                return Ok("update success");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("something wrong");
            }
            
        }

        [HttpPut]
        [Route("update/security")]
        public async Task<IActionResult> Put([FromBody] UserSecurityUpdateViewModel viewModel, CancellationToken cancellationToken)
        {
            try
            {
                User user = new()
                {
                    Id = viewModel.Id,
                    Password = viewModel.NewPassword
                };
                var flag = await _userRepository.UpdateUserPassword(viewModel.OldPassword, user, cancellationToken);
                if (flag == 0)
                {
                    return BadRequest("密码错误");
                }
                await _distributedCache.RemoveAsync($"user-{viewModel.Id}");//remove cache
                return Ok("修改成功");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest();
            }
        }
    }
}
