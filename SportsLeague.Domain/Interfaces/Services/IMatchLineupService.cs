using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Services;

public interface IMatchLineupService
{
    Task<MatchLineup> AddLineupAsync(int matchId, MatchLineup matchLineup);
    Task<IEnumerable<MatchLineup>> GetMatchLineupAsync(int matchId);
    Task<IEnumerable<MatchLineup>> GetMatchLineupByTeamAsync(int matchId, int teamId);
    Task RemoveLineupAsync(int matchId, int id);
}
