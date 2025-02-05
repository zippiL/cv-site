namespace Service;
using Octokit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;



using Microsoft.Extensions.Caching.Memory;

public class GitHubService
{
    private readonly GitHubClient _client;
    private readonly GitHubIntegrationOptions _configuration;
    private readonly string _token;
    private readonly string _userName;
    private readonly IMemoryCache _cache;

    public GitHubService(IOptions<GitHubIntegrationOptions> options, IMemoryCache cache)
    {
        _configuration = options.Value;
        _token = _configuration.Token;
        _client = new GitHubClient(new ProductHeaderValue("YourAppName"))
        {
            Credentials = new Credentials(_token)
        };
        _userName = _configuration.UserName;
        _cache = cache;
    }

    public async Task<List<RepositoryInfo>> GetPortfolio()
    {
        // מפתח הקאש
        string cacheKey = "PortfolioData";

        // בדוק אם הנתונים כבר בקאש
        if (!_cache.TryGetValue(cacheKey, out List<RepositoryInfo> repositoryInfos))
        {
            // אם לא, קבל את הנתונים מה-GitHub
            var repositories = await _client.Repository.GetAllForUser(_userName);
            repositoryInfos = new List<RepositoryInfo>();

            foreach (var repo in repositories)
            {
                var commits = await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name);
                var pullRequests = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
                var lastCommit = commits.FirstOrDefault();

                repositoryInfos.Add(new RepositoryInfo
                {
                    Name = repo.Name,
                    Language = repo.Language,
                    LastCommitDate = lastCommit?.Commit.Committer.Date.DateTime,
                    StarsCount = repo.StargazersCount,
                    PullRequestsCount = pullRequests.Count(),
                    Url = repo.HtmlUrl
                });
            }

            // הגדר את הנתונים בקאש עם תאריך תפוגה (למשל, 60 שניות)
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60));

            _cache.Set(cacheKey, repositoryInfos, cacheEntryOptions);
        }

        return repositoryInfos;
    }




    public async Task<List<Repository>> SearchRepositories(string repoName = null, string language = null, string username = null)
    {
        var request = new SearchRepositoriesRequest(repoName)
        {
            Language = language != null ? (Language)Enum.Parse(typeof(Language), language, true) : null,
            User = username
        };

        var result = await _client.Search.SearchRepo(request);
        return result.Items.ToList();
    }

    

}