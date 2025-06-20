using FilmMatch.Application.Features.Users.Commands.RegisterUser;
using FilmMatch.Application.Features.Users.Queries.LoginUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FilmMatch.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FilmMatch.Domain.Constants;
using System.Security.Claims;
using FilmMatch.Application.Features.Users.Queries.GetUsernames;
using FilmMatch.Application.Features.Users.UserToAdmin;
using FilmMatch.Domain.Entities;
using FilmMatch.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using FilmMatch.Application.Features.Users.Queries.GetUsernameById;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Controllers ;

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IDbContext _dbContext;
        private readonly ILogger<UserController> _logger;

        public UserController(IMediator mediator, IUserService userService, UserManager<IdentityUser<Guid>> userManager, RoleManager<IdentityRole<Guid>> roleManager, IDbContext dbContext, ILogger<UserController> logger)
        {
            _mediator = mediator;
            _userService = userService;
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand request)
        {
            _logger.LogInformation("POST /User/register: {@Request}", request);
            var result = await _mediator.Send(new RegisterUserCommand()
            {
                Password = request.Password,
                Email = request.Email,
                Name = request.Name
            });
            _logger.LogInformation("User registered: {Email}", request.Email);
            return Ok(result);
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserQuery request)
        {
            _logger.LogInformation("POST /User/login: {@Request}", request);
            var ok = await _mediator.Send(new LoginUserQuery()
            {
                Email = request.Email,
                Password = request.Password
            });
            _logger.LogInformation("User login attempt: {Email}", request.Email);
            return Ok(ok);
        }

        [HttpGet("GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            _logger.LogInformation("GET /User/GetCurrentUser");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("GetCurrentUser: Unauthorized");
                return Unauthorized();
            }
            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
            {
                _logger.LogWarning("GetCurrentUser: Identity user not found {UserId}", userId);
                return NotFound();
            }
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);
            if (user == null)
            {
                _logger.LogWarning("GetCurrentUser: User not found {IdentityUserId}", identityUser.Id);
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(identityUser);
            _logger.LogInformation("GetCurrentUser: Success for {UserId}", userId);
            return Ok(new
            {
                user.Id,
                user.Name,
                user.HasSubscription,
                Roles = roles
            });
        }

        // ГОВНО
        [HttpGet("GetAllUsers")]
        [Authorize(Roles = $"{RoleConstants.God},{RoleConstants.Admin}")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("GET /User/GetAllUsers");
            var users = _userManager.Users.ToList();
            var userDtos = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new
                {
                    user.Id,
                    user.Email,
                    Roles = roles
                });
            }

            _logger.LogInformation("Returned {Count} users", userDtos.Count);
            return Ok(userDtos);
        }

        [HttpPost("MakeAdmin/{userId}")]
        [Authorize(Roles = RoleConstants.God)]
        public async Task<IActionResult> MakeAdmin(string userId)
        {
            _logger.LogInformation("POST /User/MakeAdmin/{UserId}", userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("MakeAdmin: User not found {UserId}", userId);
                return NotFound();
            }

            var result = await _userManager.AddToRoleAsync(user, RoleConstants.Admin);
            if (!result.Succeeded)
            {
                _logger.LogWarning("MakeAdmin: Failed to add admin role {UserId}", userId);
                return BadRequest(result.Errors);
            }
            _logger.LogInformation("User promoted to admin: {UserId}", userId);
            return Ok();
        }

        [HttpPost("BlockUser/{userId}")]
        [Authorize(Roles = $"{RoleConstants.God},{RoleConstants.Admin}")]
        public async Task<IActionResult> BlockUser(string userId)
        {
            _logger.LogInformation("POST /User/BlockUser/{UserId}", userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("BlockUser: User not found {UserId}", userId);
                return NotFound();
            }

            var result = await _userManager.AddToRoleAsync(user, RoleConstants.Blocked);
            if (!result.Succeeded)
            {
                _logger.LogWarning("BlockUser: Failed to block user {UserId}", userId);
                return BadRequest(result.Errors);
            }
            _logger.LogInformation("User blocked: {UserId}", userId);
            return Ok();
        }

        [HttpPost("UnblockUser/{userId}")]
        [Authorize(Roles = $"{RoleConstants.God},{RoleConstants.Admin}")]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            _logger.LogInformation("POST /User/UnblockUser/{UserId}", userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("UnblockUser: User not found {UserId}", userId);
                return NotFound();
            }

            var result = await _userManager.RemoveFromRoleAsync(user, RoleConstants.Blocked);
            if (!result.Succeeded)
            {
                _logger.LogWarning("UnblockUser: Failed to unblock user {UserId}", userId);
                return BadRequest(result.Errors);
            }
            _logger.LogInformation("User unblocked: {UserId}", userId);
            return Ok();
        }
        
        [HttpGet("usernames")]
        [Authorize]
        public async Task<IActionResult> GetUsernames([FromQuery]string? query)
        {
            _logger.LogInformation("GET /User/usernames: query={Query}", query);
            var result = await _mediator.Send(new GetUsernamesQuery(query));
            _logger.LogInformation("Returned usernames: {@Result}", result);
            return Ok(result);
        }

        [HttpPut("UserToAdmin")]
        [Authorize(Roles = RoleConstants.God)]
        public async Task<IActionResult> UserToAdmin(Guid userId)
        {
            _logger.LogInformation("PUT /User/UserToAdmin: {UserId}", userId);
            var result = await _mediator.Send(new UserToAdminCommand(userId));
            _logger.LogInformation("User promoted to admin: {UserId}", userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("UsernameById/{userId}")]
        public async Task<IActionResult> GetUsernameById(Guid userId)
        {
            _logger.LogInformation("GET /User/UsernameById/{UserId}", userId);
            var result = await _mediator.Send(new GetUsernameByIdQuery(userId));
            if (result == null)
            {
                _logger.LogWarning("GetUsernameById: User not found {UserId}", userId);
                return NotFound();
            }
            _logger.LogInformation("Returned username for {UserId}", userId);
            return Ok(result);
        }
    }