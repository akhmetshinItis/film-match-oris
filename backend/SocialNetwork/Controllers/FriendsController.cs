using FilmMatch.Application.Features.Friends.GetAllPossibleFriends;
using FilmMatch.Application.Features.Friends.GetAllUserFriends;
using FilmMatch.Application.Features.Friends.DeleteFriend;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FriendsController> _logger;
        public FriendsController(IMediator mediator, ILogger<FriendsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("AllUserFriends")]
        public async Task<IActionResult> GetAllUserFriends()
        {
            _logger.LogInformation("GET /Friends/AllUserFriends");
            var result = await _mediator.Send(new GetAllUserFriendsQuery());
            _logger.LogInformation("Returned {Count} user friends", result.Friends.Count);
            return Ok(result);
        }

        [HttpGet("AllPossibleFriends")]
        public async Task<IActionResult> GetAllPossibleFriends()
        {
            _logger.LogInformation("GET /Friends/AllPossibleFriends");
            var result = await _mediator.Send(new GetAllPossibleFriendsQuery());
            _logger.LogInformation("Returned {Count} possible friends", result.Users.Count);
            return Ok(result);
        }

        [HttpDelete("DeleteFriend/{friendId}")]
        public async Task<IActionResult> DeleteFriend(Guid friendId)
        {
            _logger.LogInformation("DELETE /Friends/DeleteFriend/{FriendId}", friendId);
            var result = await _mediator.Send(new DeleteFriendCommand(friendId));
            if (!result)
            {
                _logger.LogWarning("DeleteFriend: friendship not found with {FriendId}", friendId);
                return NotFound();
            }
            _logger.LogInformation("Friendship deleted with {FriendId}", friendId);
            return NoContent();
        }
    }
}