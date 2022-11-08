using LevenshteinDistanceAlgorithm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFA.ItemCodes.Classes
{
    internal static class SearchService
    {
        internal static  ObservableCollection<ItemCode> SearchItemCode(string text, ObservableCollection<ItemCode> data,  bool advancedSearch)
        {
            if (advancedSearch)
                return AdvancedSearchItemCode(text, data);

             static bool search(string searcher, string searchee)
            {
                searcher = searcher.Trim();
                if (searcher.StartsWith("*"))
                {
                    return searchee?.EndsWith(searcher.Replace("*", "")) ?? false;
                }
                else if (searcher.EndsWith("*"))
                {
                    return searchee?.StartsWith(searcher.Replace("*", "")) ?? false;
                }
                else if (searcher.Contains("*"))
                {
                    var tt = searcher.Split('*');
                    return (searchee?.StartsWith(tt.First()) ?? false) && (searchee?.EndsWith(tt.Last()) ?? false);
                }
                else
                {
                    return searchee?.Contains(searcher) ?? false;
                }
            }
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return data;
                else
                {
                    var texts = text?.ToLower()?.Split(' ')
                        .Select(c => c?.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToArray();
                    if (!(texts?.Any()??false))
                        return data;
                    else
                        return new(data.Where(c => texts
                               .All(x => search(x, c.Code?.ToLower()?.Replace("-", "")) || search(x, c.Distributor?.ToLower()) || search(x, c.Code?.ToLower()) || search(x, c.Name?.ToLower()) || search(x, c.Narration?.ToLower()))));
                }
            }
            catch (Exception ex)
            {
                Functions.NotifyError("Error Loading the page", "Loading Error", ex);
            }
            return data;
        }

        private static ObservableCollection<ItemCode> AdvancedSearchItemCode(string text, ObservableCollection<ItemCode> data)
        {
            return new ObservableCollection<ItemCode>(ItemChecker.SearchItemByName(text, data.ToList()));
        }
    }
}
