using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _matchLineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(IMatchLineupService matchLineupService, IMapper mapper)
    {
        _matchLineupService = matchLineupService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> AddLineup(int matchId, [FromBody] MatchLineupRequestDto dto)
    {
        try
        {
            var matchLineup = _mapper.Map<MatchLineup>(dto);
            var createdLineup = await _matchLineupService.AddLineupAsync(matchId, matchLineup);
            var responseDto = _mapper.Map<MatchLineupResponseDTO>(createdLineup);
            
            return CreatedAtAction(nameof(AddLineup), new { matchId, id = createdLineup.Id }, responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMatchLineup(int matchId)
    {
        try
        {
            var lineups = await _matchLineupService.GetMatchLineupAsync(matchId);
            var responseDtos = _mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineups);
            return Ok(responseDtos);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    [HttpGet("team/{teamId}")]
    public async Task<IActionResult> GetMatchLineupByTeam(int matchId, int teamId)
    {
        try
        {
            var lineups = await _matchLineupService.GetMatchLineupByTeamAsync(matchId, teamId);
            var responseDtos = _mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineups);
            return Ok(responseDtos);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveLineup(int matchId, int id)
    {
        try
        {
            await _matchLineupService.RemoveLineupAsync(matchId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }
}
