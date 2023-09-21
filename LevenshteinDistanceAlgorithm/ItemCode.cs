using ReactiveUI;

namespace LevenshteinDistanceAlgorithm;
public class ItemCode : ReactiveObject
{
    private string? code;
    private string? name;
    private string? originalName;
    private string? itemGroup;
    private string? harmonizedName;
    private string? narration;
    private string? harmonizedGroupName;
    private string? measureUnit;
    private string? groupName;
    private string? distributor;
    private bool? isVerified;
    private decimal? quantity;

    public string? Code { get => code; set =>  this.RaiseAndSetIfChanged(ref code, value); }
    public string? Name { get => name; set =>  this.RaiseAndSetIfChanged(ref name, value); }
    public string? OriginalName { get => originalName; set =>  this.RaiseAndSetIfChanged(ref originalName, value); }
    public string? ItemGroup { get => itemGroup; set =>  this.RaiseAndSetIfChanged(ref itemGroup, value); }
    public string? HarmonizedName { get => harmonizedName; set =>  this.RaiseAndSetIfChanged(ref harmonizedName, value); }
    public string? Narration { get => narration; set =>  this.RaiseAndSetIfChanged(ref narration, value); }
    public string? HarmonizedGroupName { get => harmonizedGroupName; set =>  this.RaiseAndSetIfChanged(ref harmonizedGroupName, value); }
    public string? MeasureUnit { get => measureUnit; set =>  this.RaiseAndSetIfChanged(ref measureUnit, value); }
    public string? GroupName { get => groupName; set =>  this.RaiseAndSetIfChanged(ref groupName, value); }
    public string? Distributor { get => distributor; set =>  this.RaiseAndSetIfChanged(ref distributor, value); }
    public bool? IsVerified { get => isVerified; set =>  this.RaiseAndSetIfChanged(ref isVerified, value); }
    public decimal? Quantity { get => quantity; set =>  this.RaiseAndSetIfChanged(ref quantity, value); }
}