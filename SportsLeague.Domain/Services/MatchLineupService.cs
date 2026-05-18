using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchLineupRepository _matchLineupRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly MatchValidationHelper _validationHelper;

    public MatchLineupService(
        IMatchLineupRepository matchLineupRepository,
        IMatchRepository matchRepository,
        MatchValidationHelper validationHelper)
    {
        _matchLineupRepository = matchLineupRepository;
        _matchRepository = matchRepository;
        _validationHelper = validationHelper;
    }

    public async Task<MatchLineup> AddLineupAsync(int matchId, MatchLineup matchLineup)
    {
        // V1: El partido debe existir
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        // V6: El partido debe estar en estado Scheduled
        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Solo se pueden registrar alineaciones para partidos programados (Scheduled)");

        // V2 & V3: El jugador debe existir y pertenecer a uno de los equipos
        var player = await _validationHelper.ValidatePlayerInMatchAsync(matchLineup.PlayerId, match);

        // V4: El jugador no puede estar registrado dos veces
        bool exists = await _matchLineupRepository.ExistsByMatchAndPlayerAsync(matchId, matchLineup.PlayerId);
        if (exists)
            throw new InvalidOperationException($"El jugador con ID {matchLineup.PlayerId} ya está registrado en la alineación de este partido");

        // V5: Máximo 11 titulares por equipo
        if (matchLineup.IsStarter)
        {
            var teamLineup = await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, player.TeamId);
            int startersCount = teamLineup.Count(ml => ml.IsStarter);
            if (startersCount == 11)
                throw new InvalidOperationException($"El equipo ya tiene el máximo de 11 titulares permitidos");
        }

        matchLineup.MatchId = matchId;
        return await _matchLineupRepository.CreateAsync(matchLineup);
    }

    public async Task<IEnumerable<MatchLineup>> GetMatchLineupAsync(int matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _matchLineupRepository.GetByMatchAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetMatchLineupByTeamAsync(int matchId, int teamId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
    }

    public async Task RemoveLineupAsync(int matchId, int id)
    {
        var lineup = await _matchLineupRepository.GetByIdAsync(id);
        if (lineup == null || lineup.MatchId != matchId)
            throw new KeyNotFoundException($"No se encontró el registro de alineación con ID {id} para este partido");

        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match != null && match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("No se pueden modificar alineaciones de un partido que ya no está programado");

        await _matchLineupRepository.DeleteAsync(lineup.Id);
    }
}
