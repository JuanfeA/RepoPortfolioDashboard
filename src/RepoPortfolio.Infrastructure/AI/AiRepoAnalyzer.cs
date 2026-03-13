using Microsoft.Extensions.Caching.Memory;
using RepoPortfolio.Core.Interfaces;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Infrastructure.AI;

/// <summary>
/// Factory pattern for creating the appropriate AI analyzer.
/// Implements IRepoAnalyzer with caching and batch support.
/// </summary>
public class AiRepoAnalyzer : IRepoAnalyzer
{
    private readonly AiAnalyzerBase _analyzer;
    private readonly AiAnalyzerOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ISdlcStore? _store;

    public AiRepoAnalyzer(AiAnalyzerOptions options, IMemoryCache? cache = null, ISdlcStore? store = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions());
        _store = store;
        _analyzer = CreateAnalyzer(options);
    }

    /// <summary>
    /// Factory method - Strategy pattern for selecting provider.
    /// </summary>
    private static AiAnalyzerBase CreateAnalyzer(AiAnalyzerOptions options) => options.Provider switch
    {
        AiProvider.OpenAI or AiProvider.AzureOpenAI or AiProvider.LocalLLM 
            => new OpenAiAnalyzer(options),
        AiProvider.Anthropic 
            => new AnthropicAnalyzer(options),
        _ 
            => new NullAnalyzer()
    };

    public bool IsAvailable => _analyzer.IsAvailable;

    public async Task<RepoInsights> AnalyzeAsync(Repository repo, CancellationToken ct = default)
    {
        // Check cache first
        var cached = await GetCachedInsightsAsync(repo.Id, ct);
        if (cached != null && !cached.IsStale)
            return cached;

        // Perform analysis
        RepoInsights insights;
        if (_analyzer is NullAnalyzer nullAnalyzer)
        {
            // Use rule-based analysis when AI not available
            insights = await nullAnalyzer.AnalyzeAsync(repo, ct);
        }
        else
        {
            insights = await _analyzer.AnalyzeAsync(repo, ct);
        }

        // Cache result
        var cacheKey = GetCacheKey(repo.Id);
        _cache.Set(cacheKey, insights, TimeSpan.FromHours(_options.CacheExpirationHours));

        // Persist if store available
        if (_store != null)
        {
            await _store.SaveInsightsAsync(insights, ct);
        }

        return insights;
    }

    public async Task<IReadOnlyList<RepoInsights>> AnalyzeBatchAsync(
        IEnumerable<Repository> repos, 
        CancellationToken ct = default)
    {
        var repoList = repos.ToList();
        var results = new List<RepoInsights>();

        if (!_options.EnableBatchAnalysis || !IsAvailable)
        {
            // Sequential fallback
            foreach (var repo in repoList)
            {
                ct.ThrowIfCancellationRequested();
                results.Add(await AnalyzeAsync(repo, ct));
            }
            return results;
        }

        // Parallel batch with semaphore limiting
        var semaphore = new SemaphoreSlim(_options.MaxConcurrentAnalyses);
        var tasks = repoList.Select(async repo =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                return await AnalyzeAsync(repo, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        results.AddRange(await Task.WhenAll(tasks));
        return results;
    }

    public async Task<RepoInsights?> GetCachedInsightsAsync(Guid repositoryId, CancellationToken ct = default)
    {
        var cacheKey = GetCacheKey(repositoryId);
        
        // Check memory cache
        if (_cache.TryGetValue(cacheKey, out RepoInsights? cached))
            return cached;

        // Check persistent store
        if (_store != null)
        {
            var persisted = await _store.GetLatestInsightsAsync(repositoryId, ct);
            if (persisted != null)
            {
                // Warm up memory cache
                _cache.Set(cacheKey, persisted, TimeSpan.FromHours(_options.CacheExpirationHours));
                return persisted;
            }
        }

        return null;
    }

    private static string GetCacheKey(Guid repoId) => $"insights:{repoId}";
}

/// <summary>
/// Builder for AiRepoAnalyzer with fluent configuration.
/// </summary>
public class AiRepoAnalyzerBuilder
{
    private readonly AiAnalyzerOptions _options = new();
    private IMemoryCache? _cache;
    private ISdlcStore? _store;

    public AiRepoAnalyzerBuilder UseOpenAI(string apiKey, string model = "gpt-4o-mini")
    {
        _options.Provider = AiProvider.OpenAI;
        _options.ApiKey = apiKey;
        _options.Model = model;
        return this;
    }

    public AiRepoAnalyzerBuilder UseAnthropic(string apiKey, string model = "claude-3-sonnet-20240229")
    {
        _options.Provider = AiProvider.Anthropic;
        _options.ApiKey = apiKey;
        _options.Model = model;
        return this;
    }

    public AiRepoAnalyzerBuilder UseAzureOpenAI(string apiKey, string endpoint, string deployment)
    {
        _options.Provider = AiProvider.AzureOpenAI;
        _options.ApiKey = apiKey;
        _options.BaseUrl = $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version=2024-02-15-preview";
        _options.Model = deployment;
        return this;
    }

    public AiRepoAnalyzerBuilder UseLocalLLM(string baseUrl, string model = "llama2")
    {
        _options.Provider = AiProvider.LocalLLM;
        _options.ApiKey = "not-required";
        _options.BaseUrl = baseUrl;
        _options.Model = model;
        return this;
    }

    public AiRepoAnalyzerBuilder WithCache(IMemoryCache cache)
    {
        _cache = cache;
        return this;
    }

    public AiRepoAnalyzerBuilder WithStore(ISdlcStore store)
    {
        _store = store;
        return this;
    }

    public AiRepoAnalyzerBuilder WithCacheExpiration(int hours)
    {
        _options.CacheExpirationHours = hours;
        return this;
    }

    public AiRepoAnalyzerBuilder WithBatchConfig(bool enabled = true, int maxConcurrent = 3)
    {
        _options.EnableBatchAnalysis = enabled;
        _options.MaxConcurrentAnalyses = maxConcurrent;
        return this;
    }

    public AiRepoAnalyzer Build() => new(_options, _cache, _store);
}
