using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Challenge
{
    [SQLite.PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool CompletionStatus { get; set; } = false;
    public bool ChallengeAccepted { get; set; } = false;
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int XPReward { get; set; } = 0;
    public string Reward { get; set; } = "";
    public bool HasReward => !string.IsNullOrWhiteSpace(Reward);
}
