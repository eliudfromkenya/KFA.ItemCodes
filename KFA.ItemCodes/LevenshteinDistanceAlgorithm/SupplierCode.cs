using KFA.ItemCodes.LevenshteinDistanceAlgorithm;
using ReactiveUI;

namespace LevenshteinDistanceAlgorithm;
public class SupplierCode : ReactiveObject
{
    private string? email;
    private string? telephone;
    private string? address;
    private string? code;
    private string? name;
    private string? originalName;
    private Branch? itemGroup;

    public string? Code { get => code; set => this.RaiseAndSetIfChanged(ref code, value); }
    public string? Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }
    public string? OriginalName { get => originalName; set => this.RaiseAndSetIfChanged(ref originalName, value); }
    public string? Email { get => email; set =>  this.RaiseAndSetIfChanged(ref email, value); }
    public string? Telephone { get => telephone; set =>  this.RaiseAndSetIfChanged(ref telephone, value); }
    public string? Address { get => address; set =>  this.RaiseAndSetIfChanged(ref address, value); }
    public Branch? Branch { get => itemGroup; set =>  this.RaiseAndSetIfChanged(ref itemGroup, value); }
}