
using DojoCourse.Module.Indexes;
using DojoCourse.Module.Models;
using YesSql;

namespace DojoCourse.Module.Services;

public record GameRatingSummary(
    int Count,
    double? Average,
    int? YourScore
);

public class GameRatingService
{
    private readonly ISession _session;

    public GameRatingService(ISession session)
    {
        _session = session;
    }

    // Közösségi átlag, szavazatszám és saját értékelés.
    public async Task<GameRatingSummary> GetSummaryAsync(
        string gameId,
        string? userId)
    {
        var ratings = (await _session
            .Query<GameRatingRecord, GameRatingIndex>(
                index => index.GameContentItemId == gameId)
            .ListAsync())
            .ToList();

        int count = ratings.Count;

        double? average = count == 0
            ? null
            : ratings.Average(r => (double)r.Score);

        int? yourScore = null;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            yourScore = ratings
                .FirstOrDefault(r => r.UserId == userId)
                ?.Score;
        }

        return new GameRatingSummary(
            count,
            average,
            yourScore
        );
    }

    // Új értékelés mentése vagy meglévő módosítása.
    public async Task SaveOrUpdateAsync(
        string gameId,
        string userId,
        int score)
    {
        if (string.IsNullOrWhiteSpace(gameId) ||
            string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "Game and user identifiers are required.");
        }

        if (score < 1 || score > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                "Rating must be between 1 and 10.");
        }

        var existingRating = await _session
            .Query<GameRatingRecord, GameRatingIndex>(
                index => index.GameContentItemId == gameId
                    && index.UserId == userId)
            .FirstOrDefaultAsync();

        if (existingRating == null)
        {
            existingRating = new GameRatingRecord
            {
                GameContentItemId = gameId,
                UserId = userId,
                Score = score,
                UpdatedUtc = DateTime.UtcNow
            };
        }
        else
        {
            existingRating.Score = score;
            existingRating.UpdatedUtc = DateTime.UtcNow;
        }

        await _session.SaveAsync(existingRating);

        // A dokumentum és a hozzá tartozó index mentése.
        await _session.SaveChangesAsync();
    }
}
