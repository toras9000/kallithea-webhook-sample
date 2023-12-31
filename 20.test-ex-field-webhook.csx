#r "nuget: Lestaly, 0.51.0"
#r "nuget: KallitheaApiClient, 0.7.0-lib.23"
#nullable enable
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script (v1.5 or lator).
// You can install .NET SDK 8.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

var settings = new
{
    // Service URL
    ServiceUrl = new Uri(@"http://localhost:8800/"),

    // API key
    ApiKey = "1111222233334444555566667777888899990000",

    // Webhook URL
    WebhookAddress = "http://sample-host-gateway:8801/webhook-accept",

    // kallithea working base dir
    ServerWorkDir = "work",

    // local working base dir
    LocalWorkDir = ThisSource.RelativeDirectory("work"),
};

await Paved.RunAsync(async () =>
{
    using var client = new CustomKallitheaClient(new Uri(settings.ServiceUrl, "/_admin/api"), settings.ApiKey);

    // create repogroup if not exist
    var repogroups = await client.GetRepoGroupsAsync();
    if (repogroups.All(i => i.group_name != settings.ServerWorkDir))
    {
        await client.CreateRepoGroupAsync(new(settings.ServerWorkDir));
    }

    // create repository
    Console.WriteLine("Create repository");
    var repoName = $"test-{DateTime.Now:yyyyMMdd-HHmmss}";
    var repoPath = $"{settings.ServerWorkDir}/{repoName}";
    await client.CreateRepoAsync(new(repoPath, repo_type: RepoType.git));
    var repo = await client.GetRepoAsync(new(repoPath));

    // add extra field
    Console.WriteLine("Add extra fields");
    await client.CreateRepoExtraFieldAsync(new(repoPath, "webhook-push-url", field_value: settings.WebhookAddress));
    await client.CreateRepoExtraFieldAsync(new(repoPath, "webhook-push-format", field_value: "changes"));       // "changes" or "revs" or not set
    await client.CreateRepoExtraFieldAsync(new(repoPath, "webhook-push-branches", field_value: "main master")); // space separated

    // clone repository
    Console.WriteLine("Clone repository");
    var repoUrl = new Uri($"http://admin:admin123@{settings.ServiceUrl.Authority}/{repoPath}");
    var cloneDir = settings.LocalWorkDir.RelativeDirectory(repoName);
    await "git".args("clone", repoUrl.AbsoluteUri, cloneDir.FullName).silent().result().success();

    // commit
    for (var i = 0; i < 8; i++)
    {
        Console.WriteLine($"Create commit {i}");
        await cloneDir.RelativeFile("aaa.txt").WriteAllTextAsync($"{i}");
        await "git".args("-C", cloneDir.FullName, "add", ".").silent().result().success();
        await "git".args("-C", cloneDir.FullName, "commit", "-m", $"commit {i}").silent().result().success();
    }

    // push
    Console.WriteLine("push");
    await "git".args("-C", cloneDir.FullName, "push").silent().result().success();
});

record ExtraField(string key, string label, string desc, string type, string value);
record CreateExtraFieldArgs(string repoid, string field_key, string? field_label = null, string? field_desc = null, string? field_value = null);
record CreateExtraFieldResult(string msg, ExtraField extra_field);

class CustomKallitheaClient : SimpleKallitheaClient
{
    public CustomKallitheaClient(Uri apiEntry, string? apiKey) : base(new ExtraKallitheaClient(apiEntry, apiKey)) { }
    public Task<CreateExtraFieldResult> CreateRepoExtraFieldAsync(CreateExtraFieldArgs args, string? id = null, CancellationToken cancelToken = default)
        => ((ExtraKallitheaClient)this.Client).CreateRepoExtraFieldAsync(args, id ?? TakeId(), cancelToken).UnwrapResponse();

    public class ExtraKallitheaClient : KallitheaClient
    {
        public ExtraKallitheaClient(Uri apiEntry, string? apiKey = null, Func<HttpClient>? clientFactory = null) : base(apiEntry, apiKey, clientFactory) { }
        public Task<ApiResponse<CreateExtraFieldResult>> CreateRepoExtraFieldAsync(CreateExtraFieldArgs args, string? id = null, CancellationToken cancelToken = default)
            => CreateContext(id, "create_repo_extra_field", args).PostAsync<CreateExtraFieldResult>(cancelToken);
    }
}