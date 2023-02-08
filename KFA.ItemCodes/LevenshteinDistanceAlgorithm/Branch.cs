using ReactiveUI;

namespace KFA.ItemCodes.LevenshteinDistanceAlgorithm
{
    public class Branch : ReactiveObject
    {
        private string? groupId;
        private string? groupName;
        private string? prefix;

        public string? Code { get => groupId; set => this.RaiseAndSetIfChanged(ref groupId, value); }
        public string? BranchName { get => groupName; set => this.RaiseAndSetIfChanged(ref groupName, value); }
        public string? Prefix { get => prefix; set => this.RaiseAndSetIfChanged(ref prefix, value); }
    }
}