using KFA.ItemCodes;
using LevenshteinDistanceAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class ItemChecker
{
    internal static List<(string itemFrom, string itemTo, int count)> SearchItemForward(string itemCodeToSearch, List<LevenshteinDistanceAlgorithm.ItemCode> allItemsCodes)
    {
        List<(string itemFrom, string itemTo, int count)> objs = new();

        try
        {
            var ans = itemCodeToSearch;
            if (ans?.ToUpper()?.StartsWith("Q") ?? false) return objs;

            if (int.TryParse(ans, out int obj))
            {
                int itemFrom = -1;
                for (int i = obj; i < 460000; i++)
                {
                    var code = i.ToString("000000");
                    if (!allItemsCodes.Any(n => n.Code == code))
                    {
                        if (itemFrom == -1)
                            itemFrom = i;
                    }
                    else
                    {
                        if (itemFrom != -1)
                        {
                            objs.Add((itemFrom.ToString("000000"), (i - 1).ToString("000000"), i - itemFrom - 1));
                            itemFrom = -1;
                            if (objs.Count >= 30)
                                return objs;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Functions.NotifyError(ex);
        }
        return objs;
    }

    internal static List<(string itemFrom, string itemTo, int count)> SearchItemBackward(string itemCodeToSearch, List<LevenshteinDistanceAlgorithm.ItemCode> allItemsCodes)
    {
        List<(string itemFrom, string itemTo, int count)> objs = new();

        try
        {
            var ans = itemCodeToSearch;
            if (ans?.ToUpper()?.StartsWith("Q") ?? false) return objs;

            if (int.TryParse(ans, out int obj))
            {
                int itemTo = -1;
                for (int i = obj - 1; i >= 10001; i--)
                {
                    var code = i.ToString("000000");
                    if (!allItemsCodes.Any(n => n.Code == code))
                    {
                        if (itemTo == -1)
                            itemTo = i;
                    }
                    else
                    {
                        if (itemTo != -1)
                        {
                            objs.Add(((i + 1).ToString("000000"), itemTo.ToString("000000"), itemTo - (i + 1)));
                            itemTo = -1;
                            if (objs.Count >= 30)
                                return objs;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Functions.NotifyError(ex);
        }
        return objs;
    }

    internal static List<LevenshteinDistanceAlgorithm.ItemCode> SearchItemByName(string itemCodeToSearch, List<LevenshteinDistanceAlgorithm.ItemCode> allItemsCodes)
    {
        try
        {
            var ans = itemCodeToSearch?.ToUpper();

            if (ans?.StartsWith("Q") ?? false) return new();

            var body = new StringBuilder();

            var items = allItemsCodes.Where(v => v.Code == ans).ToList();
            items.AddRange(allItemsCodes
                .OrderBy(n => Matcher.LaveteshinDistanceAlgorithmBody(n.HarmonizedName ?? "", ans ?? ""))
                .Take(40));
            return items;
        }
        catch (Exception ex)
        {
            Functions.NotifyError(ex);
        }
        return new();
    }
}