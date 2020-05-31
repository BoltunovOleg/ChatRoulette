using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var git = new GitHubClient(new ProductHeaderValue("Bugtracker"));
            git.Credentials = new Credentials("BoltunovOleg", "PNGBMPpngbmp4510");
            var issues = git.Issue.GetAllForRepository("BoltunovOleg", "ChatRoulette").GetAwaiter().GetResult();
            foreach (var issue in issues)
            {
                if (issue.Body.StartsWith("UserId: 505"))
                {
                        git.Issue.Lock("BoltunovOleg", "ChatRoulette", issue.Number).GetAwaiter().GetResult();
                }
            }
        }
    }
}
