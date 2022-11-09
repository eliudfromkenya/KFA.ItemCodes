using ReactiveUI;

namespace KFA.ItemCodes.LevenshteinDistanceAlgorithm
{
    public class ItemGroup : ReactiveObject
    {
        private string? groupId;
        private string? groupName;
        private bool isEnabled;

        public string? GroupId { get => groupId; set => this.RaiseAndSetIfChanged(ref groupId, value); }
        public string? GroupName { get => groupName; set => this.RaiseAndSetIfChanged(ref groupName, value); }
        public bool IsEnabled { get => isEnabled; set => this.RaiseAndSetIfChanged(ref isEnabled, value); }
    }
}