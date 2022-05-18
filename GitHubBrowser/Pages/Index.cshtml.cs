using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Net.Http.Headers;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace GitHubBrowser.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _env;
        private readonly IHttpClientFactory _httpFactory;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration env, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _env = env;
            _httpFactory = httpFactory;
        }

        [BindProperty]
        public string SearchTerm { get; set; }

        public IEnumerable<GitRepo> Repos { get; set; } = new List<GitRepo>();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var client = _httpFactory.CreateClient();

            var githubUrl = _env["GitHubUrl"];

            var fullUrl = $"{githubUrl}/orgs/{SearchTerm}/repos";

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, fullUrl)
            {
                Headers =
                {
                    { HeaderNames.UserAgent, "dotnet" }
                }
            };

            var httpResponseMessage = await client.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream =
                    await httpResponseMessage.Content.ReadAsStreamAsync();

                Repos = await JsonSerializer.DeserializeAsync<IEnumerable<GitRepo>>(contentStream);
            }

            return Page();
        }
    }
    public class GitRepo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("created")]
        public string Created { get; set; }
    }
}