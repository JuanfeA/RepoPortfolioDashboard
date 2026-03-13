using Microsoft.AspNetCore.Mvc;
using RepoPortfolio.Application.Services;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Api.Controllers;

/// <summary>
/// Endpoints for repos to report their status (CI/CD integration).
/// </summary>
[ApiController]
[Route("api/repos")]
public class ReportsController : ControllerBase
{
    private readonly PortfolioService _portfolio;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(PortfolioService portfolio, ILogger<ReportsController> logger)
    {
        _portfolio = portfolio;
        _logger = logger;
    }

    /// <summary>
    /// Receive a status report from a repository's CI/CD pipeline.
    /// </summary>
    [HttpPost("report")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportResponse>> ReceiveReport(
        [FromBody] ReportRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Repository))
        {
            return BadRequest(new { error = "Repository name is required" });
        }

        _logger.LogInformation(
            "Received report from {Repo} - Event: {Event}, SHA: {Sha}", 
            request.Repository, 
            request.Event, 
            request.Sha?[..7]);

        // Convert request to domain model
        var report = new RepoStatusReport
        {
            RepositoryFullName = request.Repository,
            CommitSha = request.Sha,
            Branch = request.Branch,
            EventType = request.Event,
            Commits30Days = request.Metrics?.Commits30d,
            OpenIssues = request.Metrics?.OpenIssues,
            OpenPrs = request.Metrics?.OpenPrs,
            HasReadme = request.Metrics?.HasReadme,
            HasLicense = request.Metrics?.HasLicense,
            HasTests = request.Metrics?.HasTests,
            HasCi = request.Metrics?.HasCi,
            ReportedPhase = ParsePhase(request.Sdlc?.Phase),
            ReportedMaturity = request.Sdlc?.Maturity,
            ReportedAt = request.Timestamp ?? DateTime.UtcNow,
            ReporterVersion = request.ReporterVersion
        };

        // Process the report (update repo data)
        var result = await _portfolio.ProcessReportAsync(report, ct);

        return Ok(new ReportResponse
        {
            Received = true,
            Repository = request.Repository,
            UpdatedScore = result.NewScore,
            Message = result.Message
        });
    }

    /// <summary>
    /// Get the latest reports for a repository.
    /// </summary>
    [HttpGet("{owner}/{repo}/reports")]
    [ProducesResponseType(typeof(IEnumerable<RepoStatusReport>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RepoStatusReport>>> GetReports(
        string owner,
        string repo,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var fullName = $"{owner}/{repo}";
        var reports = await _portfolio.GetReportsAsync(fullName, limit, ct);
        return Ok(reports);
    }

    /// <summary>
    /// Get the health summary for a repository.
    /// </summary>
    [HttpGet("{owner}/{repo}/health")]
    [ProducesResponseType(typeof(RepoHealthSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepoHealthSummary>> GetHealth(
        string owner,
        string repo,
        CancellationToken ct)
    {
        var fullName = $"{owner}/{repo}";
        var summary = await _portfolio.GetHealthSummaryAsync(fullName, ct);
        
        if (summary == null)
            return NotFound(new { error = $"Repository {fullName} not found" });
            
        return Ok(summary);
    }

    /// <summary>
    /// Request LLM analysis for a repository.
    /// </summary>
    [HttpPost("{owner}/{repo}/analyze")]
    [ProducesResponseType(typeof(RepoInsights), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<RepoInsights>> RequestAnalysis(
        string owner,
        string repo,
        [FromQuery] bool force = false,
        CancellationToken ct = default)
    {
        var fullName = $"{owner}/{repo}";
        
        try
        {
            var insights = await _portfolio.AnalyzeRepositoryAsync(fullName, force, ct);
            
            if (insights == null)
                return NotFound(new { error = $"Repository {fullName} not found" });
                
            return Ok(insights);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not configured"))
        {
            return StatusCode(503, new { error = "LLM analysis not configured", details = ex.Message });
        }
    }

    private static SdlcPhase? ParsePhase(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase)) return null;
        
        return phase.ToUpperInvariant() switch
        {
            "IDEA" or "IDEATION" => SdlcPhase.Ideation,
            "PLAN" or "PLANNING" => SdlcPhase.Planning,
            "DEV" or "DEVELOPMENT" => SdlcPhase.Development,
            "TEST" or "TESTING" => SdlcPhase.Testing,
            "REL" or "RELEASE" => SdlcPhase.Release,
            "MAINT" or "MAINTENANCE" => SdlcPhase.Maintenance,
            "DEPR" or "DEPRECATED" => SdlcPhase.Deprecated,
            _ => SdlcPhase.Unknown
        };
    }
}

#region Request/Response DTOs

public class ReportRequest
{
    public required string Repository { get; set; }
    public string? Sha { get; set; }
    public string? Branch { get; set; }
    public string? Event { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? ReporterVersion { get; set; }
    public ReportMetrics? Metrics { get; set; }
    public ReportSdlc? Sdlc { get; set; }
}

public class ReportMetrics
{
    public int? Commits30d { get; set; }
    public int? OpenIssues { get; set; }
    public int? OpenPrs { get; set; }
    public bool? HasReadme { get; set; }
    public bool? HasLicense { get; set; }
    public bool? HasTests { get; set; }
    public bool? HasCi { get; set; }
    public double? TestCoverage { get; set; }
}

public class ReportSdlc
{
    public string? Phase { get; set; }
    public int? Maturity { get; set; }
}

public class ReportResponse
{
    public bool Received { get; set; }
    public required string Repository { get; set; }
    public double? UpdatedScore { get; set; }
    public string? Message { get; set; }
}

#endregion
