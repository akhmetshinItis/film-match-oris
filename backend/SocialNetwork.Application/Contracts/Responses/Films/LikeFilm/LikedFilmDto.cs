using System;

namespace FilmMatch.Application.Contracts.Responses.Films.LikeFilm
{
    public class LikedFilmDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? LongDescription { get; set; }
        public string? ShortDescription { get; set; }
        public LikedCategoryDto? Category { get; set; }
    }
} 