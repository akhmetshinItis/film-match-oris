using MediatR;
using FilmMatch.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Application.Features.Categories.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly IDbContext _dbContext;
        private readonly ILogger<DeleteCategoryCommandHandler> _logger;
        public DeleteCategoryCommandHandler(IDbContext dbContext, ILogger<DeleteCategoryCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DeleteCategoryCommand: {Id}", request.Id);
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("DeleteCategory: Category not found {Id}", request.Id);
                return false;
            }
            category.IsDeleted = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Category deleted: {Id}", category.Id);
            return true;
        }
    }
} 