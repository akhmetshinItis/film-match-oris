using MediatR;
using FilmMatch.Application.Interfaces;
using FilmMatch.Domain.Entities;
using FilmMatch.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Application.Features.Categories.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
    {
        private readonly IDbContext _dbContext;
        private readonly IS3Service _s3Service;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;
        public CreateCategoryCommandHandler(IDbContext dbContext, IS3Service s3Service, ILogger<CreateCategoryCommandHandler> logger)
        {
            _dbContext = dbContext;
            _s3Service = s3Service;
            _logger = logger;
        }
        public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CreateCategoryCommand: {@Request}", request);
            if (request.Image == null || request.Image.Length == 0)
            {
                _logger.LogWarning("Image is required for category creation");
                throw new ArgumentException("Image is required");
            }
            var url = await _s3Service.UploadAsync(request.Image, cancellationToken);
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ImageUrl = url,
                CreatedDate = DateTime.UtcNow,
            };
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Category created: {Id}", category.Id);
            return category.Id;
        }
    }
} 