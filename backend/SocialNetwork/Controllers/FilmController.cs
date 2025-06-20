using System.ComponentModel.DataAnnotations;
using FilmMatch.Application.Features.Films.BookmarkFilm;
using FilmMatch.Application.Features.Films.CreateFilm;
using FilmMatch.Application.Features.Films.DislikeFilm;
using FilmMatch.Application.Features.Films.GetRecommendations;
using FilmMatch.Application.Features.Films.LikeFilm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmMatch.Domain.Constants;
using FilmMatch.Domain.Entities;
using FilmMatch.Application.Interfaces;
using MediatR;
using FilmMatch.Application.Features.Films.GetAllFilms;
using FilmMatch.Application.Features.Categories.GetAllCategories;
using FilmMatch.Application.Contracts.Responses.Categories.GetAllCategories;
using FilmMatch.Application.Features.Films.DeleteFilm;
using FilmMatch.Application.Features.Films.UpdateFilm;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class FilmController : ControllerBase
    {
        private readonly IDbContext _dbContext;
        private readonly IMediator _mediator;
        private readonly ILogger<FilmController> _logger;

        public FilmController(IDbContext dbContext, IMediator mediator, ILogger<FilmController> logger)
        {
            _dbContext = dbContext;
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("GET /Film");
            var films = await _dbContext.Films
                .Include(f => f.Category)
                .ToListAsync();
            _logger.LogInformation("Returned {Count} films", films.Count);
            return Ok(films);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("GET /Film/{Id}", id);
            var film = await _dbContext.Films
                .Include(f => f.Category)
                .Where(f => f.Id == id)
                .Select(f => new {
                    f.Id,
                    f.Title,
                    f.ReleaseDate,
                    f.ImageUrl,
                    f.LongDescription,
                    f.ShortDescription,
                    Category = f.Category == null ? null : new {
                        f.Category.Id,
                        f.Category.Name
                    }
                })
                .FirstOrDefaultAsync();

            if (film == null)
            {
                _logger.LogWarning("Film not found: {Id}", id);
                return NotFound();
            }

            return Ok(film);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = $"{RoleConstants.God},{RoleConstants.Admin}")]
        public async Task<IActionResult> Create([FromForm] CreateFilmCommand command)
        {
            _logger.LogInformation("POST /Film: {@Command}", command);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateFilm: invalid model state");
                return BadRequest(ModelState);
            }
            await _mediator.Send(command);
            _logger.LogInformation("Film created: {Title}", command.Title);
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateFilmCommand command)
        {
            _logger.LogInformation("PUT /Film/{Id}: {@Command}", id, command);
            if (id != command.Id)
            {
                _logger.LogWarning("Update failed: id mismatch {Id} != {CommandId}", id, command.Id);
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateFilm: invalid model state");
                return BadRequest(ModelState);
            }
            var result = await _mediator.Send(command);
            if (!result)
            {
                _logger.LogWarning("Update failed: film not found {Id}", id);
                return NotFound();
            }
            _logger.LogInformation("Film updated: {Id}", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.God},{RoleConstants.Admin}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("DELETE /Film/{Id}", id);
            var result = await _mediator.Send(new DeleteFilmCommand(id));
            if (!result)
            {
                _logger.LogWarning("Delete failed: film not found {Id}", id);
                return NotFound();
            }
            _logger.LogInformation("Film deleted: {Id}", id);
            return NoContent();
        }

        [HttpGet("GetAllFilms")]
        [Authorize]
        public async Task<IActionResult> GetAllFilms([FromQuery] Guid? categoryId = null, [FromQuery] string? search = null)
        {
            _logger.LogInformation("GET /Film/GetAllFilms: categoryId={CategoryId}, search={Search}", categoryId, search);
            var result = await _mediator.Send(new GetAllFilmsQuery(categoryId, search));
            _logger.LogInformation("Returned {Count} films", result.Films.Count);
            return Ok(result.Films);
        }

        [HttpPost("Like/{filmId}")]
        public async Task<IActionResult> ToggleLike(Guid filmId)
        {
            _logger.LogInformation("POST /Film/Like/{FilmId}", filmId);
            var result = await _mediator.Send(new ToggleLikeFilmCommand(filmId));
            _logger.LogInformation("ToggleLike result: {@Result}", result);
            return Ok(result);
        }

        [HttpGet("AllLikedFilms")]
        public async Task<IActionResult> GetAllLikedFilms()
        {
            _logger.LogInformation("GET /Film/AllLikedFilms");
            var result = await _mediator.Send(new GetLikedFilmsQuery());
            _logger.LogInformation("Returned {Count} liked films", result.Films.Count);
            return Ok(result);
        }

        [HttpPost("Dislike/{filmId}")]
        public async Task<IActionResult> ToggleDislike(Guid filmId)
        {
            _logger.LogInformation("POST /Film/Dislike/{FilmId}", filmId);
            var result = await _mediator.Send(new ToggleDislikeFilmCommand(filmId));
            _logger.LogInformation("ToggleDislike result: {@Result}", result);
            return Ok(result);
        }

        [HttpGet("AllDislikedFilms")]
        public async Task<IActionResult> GetAllDislikedFilms([FromQuery] Guid? userId = null)
        {
            _logger.LogInformation("GET /Film/AllDislikedFilms: userId={UserId}", userId);
            var result = await _mediator.Send(new GetDislikedFilmsQuery(userId));
            _logger.LogInformation("Returned {Count} disliked films", result.Films.Count);
            return Ok(result);
        }

        [HttpPost("Bookmark/{filmId}")]
        public async Task<IActionResult> BookmarkFilm(Guid filmId)
        {
            _logger.LogInformation("POST /Film/Bookmark/{FilmId}", filmId);
            var result = await _mediator.Send(new BookmarkFilmCommand(filmId));
            _logger.LogInformation("Bookmark result: {@Result}", result);
            if (!result.IsSuccessed)
                return Ok(result.Message);
            return Ok();
        }

        [HttpDelete("Bookmark/{filmId}")]
        public async Task<IActionResult> UnBookmarkFilm(Guid filmId)
        {
            _logger.LogInformation("DELETE /Film/Bookmark/{FilmId}", filmId);
            var result = await _mediator.Send(new UnbookmarkFilmCommand(filmId));
            _logger.LogInformation("UnBookmark result: {@Result}", result);
            if (!result.IsSuccessed)
                return Ok(result.Message);
            return Ok();
        }
        
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            _logger.LogInformation("GET /Film/recommendations");
            var result = await _mediator.Send(new GetRecommendationsQuery());
            _logger.LogInformation("Returned recommendations: {@Result}", result);
            return Ok(result);
        }

        [HttpGet("Bookmarked")]
        public async Task<IActionResult> GetBookmarkedFilms()
        {
            _logger.LogInformation("GET /Film/Bookmarked");
            var result = await _mediator.Send(new GetBookmarkedFilmsQuery());
            _logger.LogInformation("Returned {Count} bookmarked films", result.Films.Count);
            return Ok(result.Films);
        }
    }
} 