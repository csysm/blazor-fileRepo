using AutoMapper;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models.UserModels;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Services.Contract;
using FileRepoSys.Api.Uow.Contract;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserController> _logger;
        //private readonly IDistributedCache _distributedCache;

        public UserController(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<UserDto>> Get([FromServices] IMapper _mapper,Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetOneUser(userId, cancellationToken);
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
                await _userRepository.UpdateUser(user, user => user.UserName);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
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
        public async Task<IActionResult> Put([FromServices] IHashHelper md5Helper, [FromBody] UserSecurityUpdateViewModel viewModel, CancellationToken cancellationToken)
        {
            User user = await _userRepository.GetOneUser(viewModel.Id);
            if (md5Helper.MD5Encrypt32(viewModel.OldPassword) != user.Password)
            {
                return Unauthorized("原密码错误");
            }

            try
            {
                user.Password = md5Helper.MD5Encrypt32(viewModel.NewPassword);
                await _userRepository.UpdateUser(user, user => user.Password);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
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
