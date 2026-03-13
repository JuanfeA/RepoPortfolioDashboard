namespace RepoPortfolio.Infrastructure.AI;

/// <summary>
/// Configuration options for AI repository analyzer.
/// </summary>
public class AiAnalyzerOptions
{
    /// <summary>
    /// The AI provider to use (OpenAI, Anthropic, LocalLLM).
    /// </summary>
    public AiProvider Provider { get; set; } = AiProvider.None;
    
    /// <summary>
    /// API key for the AI service.
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Model name (e.g., "gpt-4o", "claude-3-sonnet").
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";
    
    /// <summary>
    /// Base URL for custom endpoints (e.g., Azure OpenAI or local LLM).
    /// </summary>
    public string? BaseUrl { get; set; }
    
    /// <summary>
    /// Maximum tokens for analysis response.
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
    
    /// <summary>
    /// Temperature for response generation (0.0-1.0).
    /// </summary>
    public double Temperature { get; set; } = 0.3;
    
    /// <summary>
    /// Cache expiration in hours.
    /// </summary>
    public int CacheExpirationHours { get; set; } = 168; // 7 days
    
    /// <summary>
    /// Enable parallel batch analysis.
    /// </summary>
    public bool EnableBatchAnalysis { get; set; } = true;
    
    /// <summary>
    /// Maximum concurrent analyses in batch mode.
    /// </summary>
    public int MaxConcurrentAnalyses { get; set; } = 3;
    
    /// <summary>
    /// Whether AI analysis is properly configured.
    /// </summary>
    public bool IsConfigured => Provider != AiProvider.None && !string.IsNullOrWhiteSpace(ApiKey);
}

/// <summary>
/// Supported AI providers.
/// </summary>
public enum AiProvider
{
    None = 0,
    OpenAI = 1,
    Anthropic = 2,
    AzureOpenAI = 3,
    LocalLLM = 4
}
