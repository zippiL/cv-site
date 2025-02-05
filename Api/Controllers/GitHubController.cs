using Microsoft.AspNetCore.Mvc;
using Service; // ודא שאתה כולל את ה-namespaces הנכונים
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;


[Route("api/[controller]")]
[ApiController]
public class GitHubController : ControllerBase
{
    private readonly GitHubService _gitHubService;
    private readonly string _userName;
    private readonly string _token;
    private readonly IConfiguration _configuration;

public GitHubController(IOptions<GitHubIntegrationOptions> options, IMemoryCache cache)
    {
        _gitHubService = new GitHubService(options, cache);
    }



    [HttpGet("portfolio")]
    public async Task<ActionResult<List<RepositoryInfo>>> GetPortfolio()
    {
        var portfolio = await _gitHubService.GetPortfolio();
        return Ok(portfolio);
    }

    [HttpGet("search-repositories")]
    public async Task<ActionResult<List<Repository>>> SearchRepositories(string repoName = null, string language = null, string username = null)
    {
        var repositories = await _gitHubService.SearchRepositories(repoName, language, username);
        return Ok(repositories);
    }

}
