using FilmMatch.Application.Features.FriendRequests.AcceptFriendrequest;
using FilmMatch.Application.Features.FriendRequests.DeclineFriendRequest;
using FilmMatch.Application.Features.FriendRequests.GetAllFriendRequests;
using FilmMatch.Application.Features.Friends.SendFriendRequest;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Controllers
{
    public class FriendRequestsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FriendRequestsController> _logger;
        public FriendRequestsController(IMediator mediator, ILogger<FriendRequestsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpPost("friendRequest")]
        public async Task<IActionResult> SendRequest([FromQuery] Guid receiverId, string? message)
        {
            _logger.LogInformation("POST /FriendRequests/friendRequest: receiverId={ReceiverId}, message={Message}", receiverId, message);
            var result = await _mediator.Send(new SendFriendRequestCommand
            {
                ReceiverId = receiverId,
                Message = message ?? string.Empty
            });
            _logger.LogInformation("Friend request sent to {ReceiverId}", receiverId);
            return Ok(result);
        }
        [HttpGet("allFriendRequests")]
        public async Task<IActionResult> GetAllFriendRequests(GetAllFriendRequestsQuery query)
        {
            _logger.LogInformation("GET /FriendRequests/allFriendRequests");
            var result = await _mediator.Send(query);
            _logger.LogInformation("Returned {Count} friend requests", result.Requests.Count);
            return Ok(result);
        }
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptFriendRequest(AcceptFriendRequestCommand command)
        {
            _logger.LogInformation("POST /FriendRequests/accept: {@Command}", command);
            var result = await _mediator.Send(command);
            _logger.LogInformation("Friend request accepted: {@Result}", result);
            return Ok(result);
        }
        [HttpPost("decline")]
        public async Task<IActionResult> DeclineFriendRequest(DeclineFriendRequestCommand command)
        {
            _logger.LogInformation("POST /FriendRequests/decline: {@Command}", command);
            var result = await _mediator.Send(command);
            _logger.LogInformation("Friend request declined: {@Result}", result);
            return Ok(result);
        }
    }
}