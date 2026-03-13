using Microsoft.AspNetCore.Mvc;
using RepoPortfolio.Application.Services;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Api.Controllers;

/// <summary>
/// API endpoints for repository portfolio management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly PortfolioService _portfolio;

    public PortfolioController(PortfolioService portfolio)
    {
        _portfolio = portfolio;
    }

    /// <summary>
    /// Get portfolio overview with all repositories and scores.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PortfolioOverview), StatusCodes.Status200OK)]
    public async Task<ActionResult<PortfolioOverview>> GetOverview(CancellationToken ct)
    {
        var overview = await _portfolio.GetOverviewAsync(ct);
        return Ok(overview);
    }

    /// <summary>
    /// Get detailed information for a specific repository.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RepositoryDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepositoryDetail>> GetRepository(Guid id, CancellationToken ct)
    {
        var detail = await _portfolio.GetRepositoryDetailAsync(id, ct);
        if (detail == null)
            return NotFound();
        return Ok(detail);
    }

    /// <summary>
    /// Sync repositories from GitHub.
    /// </summary>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(SyncResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<SyncResult>> Sync([FromBody] SyncRequest request, CancellationToken ct)
    {
        var result = await _portfolio.SyncRepositoriesAsync(request.Owner, ct);
        return Ok(result);
    }

    /// <summary>
    /// Recalculate scores for all repositories.
    /// </summary>
    [HttpPost("recalculate")]
    [ProducesResponseType(typeof(RecalculateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecalculateResponse>> Recalculate(CancellationToken ct)
    {
        var count = await _portfolio.RecalculateAllScoresAsync(ct);
        return Ok(new RecalculateResponse { RecalculatedCount = count });
    }
}

/// <summary>
/// Request to sync repositories.
/// </summary>
public class SyncRequest
{
    /// <summary>GitHub username or organization to sync.</summary>
    public required string Owner { get; set; }
}

/// <summary>
/// Response from recalculate operation.
/// </summary>
public class RecalculateResponse
{
    /// <summary>Number of repositories recalculated.</summary>
    public int RecalculatedCount { get; set; }
}
