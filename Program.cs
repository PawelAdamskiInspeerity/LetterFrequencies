using Octokit;
using System.Collections.Concurrent;

var owner = "lodash";
var repo = "lodash";
var sha = "main";
var token = "";
var lettersRange = Enumerable.Range('a', 26);
var maxDegreeOfParallelism = 30;

// Get GitHubClient
var client = new GitHubClient(new ProductHeaderValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName));

if (!string.IsNullOrWhiteSpace(token))
{
    client.Credentials = new Credentials(token);
}

// Get files
var treeResponse = await client.Git.Tree.GetRecursive(owner, repo, sha);
var files = treeResponse.Tree.Where(item => item.Path.EndsWith(".js") || item.Path.EndsWith(".ts")).ToList();

// Count letters frequency
var letterFrequencies = new ConcurrentDictionary<char, int>();

await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, async (file, _) =>
{
    var fileContents = await client.Repository.Content.GetAllContentsByRef(owner, repo, file.Path, sha);
    var letters = fileContents[0].Content.ToLower();

    foreach (var letter in letters)
    {
        if (lettersRange.Contains(letter))
        {
            letterFrequencies.AddOrUpdate(letter, 1, (_, oldValue) => oldValue + 1);
        }
    };
});

// Sort by frequency in decreasing order
var sortedFrequencies = letterFrequencies.OrderByDescending(x => x.Value);

// Display results
Console.WriteLine($"Letter Frequencies in {owner}/{repo} repository (JavaScript/TypeScript Files):");

foreach (var (letter, frequency) in sortedFrequencies)
{
    Console.WriteLine($"{letter}: {frequency}");
}