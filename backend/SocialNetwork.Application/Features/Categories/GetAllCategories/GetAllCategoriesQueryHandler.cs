using MediatR;
using FilmMatch.Application.Interfaces;
using FilmMatch.Application.Contracts.Responses.Categories.GetAllCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Application.Features.Categories.GetAllCategories
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, GetAllCategoriesResponse>
    {
        private readonly IDbContext _dbContext;
        private readonly ILogger<GetAllCategoriesQueryHandler> _logger;
        public GetAllCategoriesQueryHandler(IDbContext dbContext, ILogger<GetAllCategoriesQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<GetAllCategoriesResponse> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetAllCategoriesQuery");
            var categories = await _dbContext.Categories
                .Select(c => new GetAllCategoriesDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl,
                })
                .ToListAsync(cancellationToken);
            _logger.LogInformation("Returned {Count} categories", categories.Count);
            return new GetAllCategoriesResponse { Categories = categories };
        }
    }
} 