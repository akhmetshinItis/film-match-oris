using MediatR;
using FilmMatch.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using FilmMatch.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Application.Features.Categories.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly IDbContext _dbContext;
        private readonly IS3Service _s3Service;
        private readonly ILogger<UpdateCategoryCommandHandler> _logger;
        public UpdateCategoryCommandHandler(IDbContext dbContext, IS3Service s3Service, ILogger<UpdateCategoryCommandHandler> logger)
        {
            _dbContext = dbContext;
            _s3Service = s3Service;
            _logger = logger;
        }
        public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling UpdateCategoryCommand: {@Request}", request);
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("UpdateCategory: Category not found {Id}", request.Id);
                return false;
            }
            category.Name = request.Name;
            if (request.Image != null && request.Image.Length > 0)
            {
                var url = await _s3Service.UploadAsync(request.Image, cancellationToken);
                category.ImageUrl = url;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Category updated: {Id}", category.Id);
            return true;
        }
    }
} 