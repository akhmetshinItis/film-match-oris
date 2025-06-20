using FilmMatch.Application.Contracts.Responses.Categories.GetAllCategories;
using FilmMatch.Application.Features.Categories.GetAllCategories;
using FilmMatch.Application.Features.Categories.CreateCategory;
using FilmMatch.Application.Features.Categories.UpdateCategory;
using FilmMatch.Application.Features.Categories.DeleteCategory;
using FilmMatch.Application.Interfaces;
using FilmMatch.Domain.Constants;
using FilmMatch.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(IMediator mediator, ILogger<CategoryController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpGet("GetCategory")]
        public async Task<ActionResult<IEnumerable<GetAllCategoriesDto>>> GetAllCategories()
        {
            _logger.LogInformation("GET /Category/GetCategory");
            var result = await _mediator.Send(new GetAllCategoriesQuery());
            _logger.LogInformation("Returned {Count} categories", result.Categories.Count);
            return Ok(result.Categories);
        }
        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.God},{RoleConstants.Admin}")]
        public async Task<IActionResult> Create([FromForm] CreateCategoryCommand command)
        {
            _logger.LogInformation("POST /Category: {@Command}", command);
            var id = await _mediator.Send(command);
            _logger.LogInformation("Category created with id {Id}", id);
            return Ok(id);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = $"{RoleConstants.God},{RoleConstants.Admin}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateCategoryCommand command)
        {
            _logger.LogInformation("PUT /Category/{Id}: {@Command}", id, command);
            if (id != command.Id)
            {
                _logger.LogWarning("Update failed: id mismatch {Id} != {CommandId}", id, command.Id);
                return BadRequest();
            }
            var result = await _mediator.Send(command);
            if (!result)
            {
                _logger.LogWarning("Update failed: category not found {Id}", id);
                return NotFound();
            }
            _logger.LogInformation("Category updated: {Id}", id);
            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.God},{RoleConstants.Admin}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("DELETE /Category/{Id}", id);
            var result = await _mediator.Send(new DeleteCategoryCommand(id));
            if (!result)
            {
                _logger.LogWarning("Delete failed: category not found {Id}", id);
                return NotFound();
            }
            _logger.LogInformation("Category deleted: {Id}", id);
            return NoContent();
        }
    }

}

