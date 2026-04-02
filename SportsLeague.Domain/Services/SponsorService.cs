using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;

    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ITournamentRepository tournamentRepository)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync() => await _sponsorRepository.GetAllAsync();

    public async Task<Sponsor?> GetByIdAsync(int id) => await _sponsorRepository.GetByIdAsync(id);

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        if (await _sponsorRepository.GetsByNameAsync(sponsor.Name))
            throw new InvalidOperationException("Ya existe un patrocinador con este nombre.");

        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException("El formato del email de contacto no es válido.");

        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existing = await _sponsorRepository.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

        if (existing.Name != sponsor.Name && await _sponsorRepository.GetsByNameAsync(sponsor.Name))
            throw new InvalidOperationException("Ya existe un patrocinador con este nombre.");

        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException("El formato del email de contacto no es válido.");

        existing.Name = sponsor.Name;
        existing.ContactEmail = sponsor.ContactEmail;
        existing.Phone = sponsor.Phone;
        existing.WebsiteUrl = sponsor.WebsiteUrl;
        existing.Category = sponsor.Category;

        await _sponsorRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _sponsorRepository.ExistsAsync(id))
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

        await _sponsorRepository.DeleteAsync(id);
    }

    public async Task RegisterSponsorToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
    {
        if (contractAmount <= 0)
            throw new InvalidOperationException("El ContractAmount debe ser mayor a 0.");

        if (!await _sponsorRepository.ExistsAsync(sponsorId))
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

        if (!await _tournamentRepository.ExistsAsync(tournamentId))
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

        var existingLink = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existingLink != null)
            throw new InvalidOperationException("Este sponsor ya está vinculado a este torneo.");

        var tournamentSponsor = new TournamentSponsor
        {
            SponsorId = sponsorId,
            TournamentId = tournamentId,
            ContractAmount = contractAmount,
            JoinedAt = DateTime.UtcNow
        };

        await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
    }

    public async Task RemoveSponsorFromTournamentAsync(int sponsorId, int tournamentId)
    {
        var existingLink = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existingLink == null)
            throw new KeyNotFoundException("El sponsor no está vinculado a este torneo.");

        await _tournamentSponsorRepository.DeleteAsync(existingLink.Id);
    }

    public async Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int sponsorId)
    {
        if (!await _sponsorRepository.ExistsAsync(sponsorId))
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

        var links = await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
        return links.Select(ts => ts.Tournament);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
        catch
        {
            return false;
        }
    }
}