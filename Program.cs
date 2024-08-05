using Octokit;
using System.Collections.Concurrent;

var owner = "lodash";
var repo = "lodash";
var sha = "main";
var lettersRange = Enumerable.Range('a', 26);

// Get content
var client = new GitHubClient(new ProductHeaderValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName));
var treeResponse = await client.Git.Tree.GetRecursive(owner, repo, sha);
var files = treeResponse.Tree.Where(item => item.Path.EndsWith(".js") || item.Path.EndsWith(".ts")).ToList();

// Count letters frequency
var letterFrequencies = new ConcurrentDictionary<char, int>();

foreach (var item in lettersRange)
{
    letterFrequencies[(char)item] = 0;
}

var tasks = files.Select(async file =>
{
    var fileContents = await client.Repository.Content.GetAllContentsByRef(owner, repo, file.Path, sha);

    var content = fileContents[0].Content.ToLower();

    foreach (var letter in content)
    {
        if (!char.IsLetter(letter))
        {
            continue;
        }

        if (letterFrequencies.ContainsKey(letter))
        {
            letterFrequencies[letter]++;
        }
    }
});

 await Task.WhenAll(tasks);

// Sort by frequency in decreasing order
var sortedFrequencies = letterFrequencies.OrderByDescending(kv => kv.Value);

// Display results
Console.WriteLine($"Letter Frequencies in {owner}/{repo} repository (JavaScript/TypeScript Files):");
foreach (var (letter, frequency) in sortedFrequencies)
{
    Console.WriteLine($"{letter}: {frequency}");
}