// using MoreLinq;
using KFA.ItemCodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LevenshteinDistanceAlgorithm
{
    internal static class Matcher
    {
        public static void CheckCodes(ref List<ItemCode> codes)
        {
            codes.ForEach(col =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(col.Distributor)
                    && !int.TryParse(col.Distributor ?? "", out int _))
                        col.Name = $"{col.Distributor} {col.Name}";
                }
                catch (Exception ex) { Functions.Notify("Error 1 " + ex.ToString()); }
				
				try
                {
					col.Name = col.Name?.Replace("x", " ")?.Replace("X", " ");
                }
                catch (Exception ex) { Functions.Notify("Error 1 " + ex.ToString()); }

                try
                {
                    var pattern = @"\(.*[A-Z]+.*\)";
                    if (Regex.Match(col.Name ?? "", pattern).Success)
                    {
                        var value = Regex.Match(col.Name ?? "", pattern).Value;
                        col.Name = $"{value[1..^1]} {col.Name?.Replace(value, "")}";
                    }
                    col.Name = col?.Name?.Replace(")", " ").Replace("(", " ");
                }
                catch (Exception ex) { Functions.Notify("Error 2A " + ex.ToString()); }

                try
                {
                    var pattern = @"\{.*[A-Z]+.*\}";
                    if (Regex.Match(col.Name ?? "", pattern).Success)
                    {
                        var value = Regex.Match(col.Name ?? "", pattern).Value;
                        col.Name = $"{value[1..^1]} {col.Name?.Replace(value, "")}";
                    }
                    col.Name = col?.Name?.Replace("}", " ").Replace("{", " ");
                }
                catch (Exception ex) { Functions.Notify("Error 2A " + ex.ToString()); }

                try
                {
                    var pattern = @"\[.*[A-Z]+.*\]";
                    if (Regex.Match(col.Name ?? "", pattern).Success)
                    {
                        var value = Regex.Match(col.Name ?? "", pattern).Value;
                        col.Name = $"{value[1..^1]} {col.Name?.Replace(value, "")}";
                    }
                    col.Name = col?.Name?.Replace("]", " ").Replace("[", " ");
                }
                catch (Exception ex) { Functions.Notify("Error 2A " + ex.ToString()); }
                col.Name = col.Name?.ToUpper();
                try
                {
                    var pattern = @"\"".*[A-Z]+.*\""";
                    if (Regex.Match(col.Name ?? "", pattern).Success)
                    {
                        var value = Regex.Match(col.Name ?? "", pattern).Value;
                        col.Name = $"{value[1..^1]} {col.Name?.Replace(value, "")}";
                    }
                }
                catch (Exception ex) { Functions.Notify("Error 2B " + ex.ToString()); }
                try
                {
                    col.Name = CheckName(col.Name);
                }
                catch (Exception ex) { Functions.Notify("Error 2B " + ex.ToString()); }
                try
                {
                    const string pattern = @"([\d]+\.?[\d]* *[a-zA-Z]{1,5} *$)|([\d]* *x *[\d]+ *[a-zA-Z]{1,5} *$)|([\d]* *\* *[\d]+ *[a-zA-Z]{1,5} *$)|([\d]+ *[a-zA-Z]{1,5} *x *[\d]* *$)|([\d]+ *[a-zA-Z]{1,5} *\* *[\d]* *$)";

                    if (Regex.Match(col.Name ?? "", pattern).Success)
                        col.MeasureUnit = Regex.Match(col.Name ?? "", pattern).Value?.Replace("  ", " ")?.Trim();
                    col.GroupName = col.Name?.Trim();
                    if (col.MeasureUnit?.Length > 1)
                        col.GroupName = col.Name?.Replace(col.MeasureUnit ?? "", "")?.Trim();
                }
                catch (Exception ex) { Functions.Notify("Error 3 " + ex.ToString()); }
                try
                {
                    var replaces = new Dictionary<string, string[]>
                    {
                        { "KG",new[]{"KGS", "KILO", "KILOGRAM"} },
                        {"GM", new[]{ "GRAM","GS","G","GRM" } },
                        {"MM",new []{"MILLIMETER"} },
                        {"M", new[]{"METER", "MT","MTR"} },
                        {"ML",new []{"MILLITER","MLT"} },
                        {"L", new[]{"LITER", "LT","LTR"} }
                    };

                    var companyReplaces = new Dictionary<string, string[]>
                    {
                        { "JOJEMI",new[]{"JOJ"} },
                        {"SIMLAW", new[]{ "SIM" } },
                        {"ULTRAVETIS",new []{"ULTRA","ULT"} },
                        {"GRIFFATON", new[]{"GRI", "GRIFF","GRIF", } },
                        {"HYGRO",new []{"HYGROTECH","HGRO"} },
                        {"L", new[]{"LITER", "LT","LTR"} }
                    };
                    //fert, cabb, D/ , S/Loaf, EGG P/BLACK BEAUTY,EGG PLANT B/B , EGG PLANT BEAUTY,L/MASH,L/STOCK,M/SALVE,M SALVE,H/PROS,H/PHOS,H/PHO, L/STOCK,D/LICK,S/LICK,HI PHOS,R/CREOLE,S/D LICK,M/MAKER,S  D/LICK,S/D LICK,S/LICK,D/MEAL,SUGARLOAF,D/LICK, D.LICK, D/LICK,M/MAKER,M. MAKER,TOMATOE,HIGHPHOS,H/PHOSPOROUS,HI-PHOS,S/BABY,H/PHOSPHOROUS,
                    //start with "
                    //replace name double space
                    // LTD. from distributor before matching
                    foreach (var item in replaces)
                    {
                        try
                        {
                            col.Name = String.Join(" ", col?.Name?.Split(' ').Select(x =>
                             {
                                 foreach (var item in companyReplaces)
                                     foreach (var obj in item.Value)
                                         if (obj == x?.ToUpper())
                                             return item.Key;
                                 return x;
                             }) ?? Array.Empty<string>());
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        var objs = item.Value.SelectMany(n => new[] { n, $"{n}S" }).ToList();
                        var pattern = $"[a-zA-Z]+";
                        if (Regex.Match(col.MeasureUnit ?? "", pattern).Success)
                        {
                            var value = Regex.Match(col.MeasureUnit ?? "", pattern).Value;
                            var replacer = objs.FirstOrDefault(m => m == value.ToUpper());
                            if (!string.IsNullOrWhiteSpace(replacer) && !(value == item.Key))
                                col.MeasureUnit = col.MeasureUnit?.Replace(value, item.Key);
                            if ($"{item.Key}S" == value.ToUpper() && !(value == item.Key))
                                col.MeasureUnit = col.MeasureUnit?.Replace(value, item.Key);
                        }
                    }
                }
                catch (Exception ex) { Functions.Notify("Error 4 " + ex.ToString()); }
                try
                {
                    col.HarmonizedName = $@"{string.Join(" ", Regex.Matches(col.GroupName ?? "", "[0-9a-zA-Z]+")
                   .Select(v => v.Value?.ToUpper()?.Trim())
                   .Distinct().OrderBy(c => c))} {col.MeasureUnit?.Trim()}".Replace(" ", "");
                }
                catch (Exception ex) { Functions.Notify("Error 5 " + ex.ToString()); }
                try
                {
                    col.HarmonizedGroupName = string.Join(" ", Regex.Matches(col.GroupName ?? "", "[0-9a-zA-Z]+")
                   .Select(v => v.Value?.ToUpper()?.Trim())
                   .Distinct().OrderBy(c => c)).Replace(" ", "");
                }
                catch (Exception ex) { Functions.Notify("Error 5 " + ex.ToString()); }

                try
                {
                    col.HarmonizedGroupName = CheckHarmonizedName(col.HarmonizedGroupName);
                    col.HarmonizedName = CheckHarmonizedName(col.HarmonizedName);
                }
                catch (Exception ex) { Functions.Notify("Error 5 " + ex.ToString()); }
            });
        }

        private static string? CheckHarmonizedName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            if (name.Contains("S/LICK") && !name.Contains("SUPER LICK"))
                name = name.Replace("S/LICK", "SUPER LICK");
            if (name.Contains(" HIGH PHOS ") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("HIGH PHOS", "HIGH PHOSPHOROUS");
            if (name.Contains(" H/P ") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("H/P", "HIGH PHOSPHOROUS");
            if (name.Contains(" H-PHOS ") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("S/D LICK", "SUPER DAIRY LICK");
            if (name.Contains("S/D LICK") && !name.Contains("SUPER DAIRY LICK"))
                name = name.Replace("S/D LICK", "SUPER DAIRY LICK");
            if (name.Contains("S  D/LICK") && !name.Contains("SUPER DAIRY LICK"))
                name = name.Replace("S  D/LICK", "SUPER DAIRY LICK");
            if (name.Contains("S/D LICK") && !name.Contains("SUPER DAIRY LICK"))
                name = name.Replace("S/D LICK", "SUPER DAIRY LICK");
            if (name.Contains("FORDHOOK") && !name.Contains("FORD HOOK"))
                name = name.Replace("FORDHOOK", "FORD HOOK");
            if (name.Contains("NIGHTSHADE") && !name.Contains("NIGHT SHADE"))
                name = name.Replace("NIGHTSHADE", "NIGHT SHADE");
            if (name.Contains("SUGARBABY") && !name.Contains("SUGARBABY"))
                name = name.Replace("SUGARBABY", "SUGAR BABY");
            if (name.Contains("SPINACH F/HOOK") && !name.Contains("SPINACH FORD HOOK"))
                name = name.Replace("SPINACH F/H ", "SPINACH FORD HOOK ");
            if (name.Contains("SPINACH F/H ") && !name.Contains("SPINACH FORD HOOK "))
                name = name.Replace("SPINACH F GIANT", "SPINACH FORD HOOK");
            if (name.Contains("SPINACH F GIANT") && !name.Contains("SPINACH FORD HOOK GIANT"))
                name = name.Replace("SPINACH F/HOOK", "SPINACH FORD HOOK GIANT");

            if (name.Contains("CALIFORNIA W ") && !name.Contains("CALIFORNIA WONDER "))
                name = name.Replace("CALIFORNIA W ", "CALIFORNIA WONDER ");

            return name.Replace("  ", " ").Replace("LTD.", "").Replace("LTD", "");
        }

        private static string? CheckName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            name = name.Replace("PHOSPHORUS", "PHOSPHOROUS");
            if (name.Contains("CABB.") && !name.Contains("CABBAGE"))
                name = name.Replace("CABB.", "CABBAGE");
            if (name.Contains("CABB") && !name.Contains("CABBAGE"))
                name = name.Replace("CABB", "CABBAGE");
            if (name.Contains("FERT.") && !name.Contains("FERTILIZER"))
                name = name.Replace("FERT.", "FERTILIZER");
            if (name.Contains("FERT") && !name.Contains("FERTILIZER"))
                name = name.Replace("FERT", "FERTILIZER");
            if (name.Contains("D/") && !name.Contains("DAIRY "))
                name = name.Replace("D/", "DAIRY ");
            if (name.Contains("D /") && !name.Contains("DAIRY "))
                name = name.Replace("D /", "DAIRY ");
            if (name.Contains("H/YIELD") && !name.Contains("HIGH YIELD"))
                name = name.Replace("H/YIELD", "HIGH YIELD");
            if (name.Contains("H/Y") && !name.Contains("HIGH YIELD"))
                name = name.Replace("H/Y", "HIGH YIELD");
            if (name.Contains("S/LOAF") && !name.Contains("SUGAR LOAF"))
                name = name.Replace("S/LOAF", "SUGAR LOAF");
            if (name.Contains("SUGARLOAF") && !name.Contains("SUGAR LOAF"))
                name = name.Replace("SUGARLOAF", "SUGAR LOAF");
            if (name.Contains("EGG P/BLACK BEAUTY"))
                name = name.Replace("EGG P/BLACK BEAUTY", "EGG PLANT BLACK BEAUTY");
            if (name.Contains("EGG PLANT B/B"))
                name = name.Replace("EGG PLANT B/B", "EGG PLANT BLACK BEAUTY");
            if (name.Contains("EGG PLANT BEAUTY"))
                name = name.Replace("EGG PLANT BEAUTY", "EGG PLANT BLACK BEAUTY");
            if (name.Contains("L/MASH") && !name.Contains("LAYERS MASH"))
                name = name.Replace("L/MASH", "LAYERS MASH");
            if (name.Contains("L/STOCK") && !name.Contains("LIVESTOCK"))
                name = name.Replace("L/STOCK", "LIVESTOCK");
            if (name.Contains("L/STOCK") && !name.Contains("LIVESTOCK"))
                name = name.Replace("L/STOCK", "LIVESTOCK");
            if (name.Contains("M/SALVE") && !name.Contains("MILKING SALVE"))
                name = name.Replace("M/SALVE", "MILKING SALVE");
            if (name.Contains("M SALVE") && !name.Contains("MILKING SALVE"))
                name = name.Replace("M SALVE", "MILKING SALVE");
            if (name.Contains("MILK SALVE") && !name.Contains("MILKING SALVE"))
                name = name.Replace("MILK SALVE", "MILKING SALVE");
            if (name.Contains("H/PROS") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("H/PROS", "HIGH PHOSPHOROUS");
            if (name.Contains("H/PHO") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("H/PHO", "HIGH PHOSPHOROUS");
            if (name.Contains("HI PHOS") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("HI PHOS", "HIGH PHOSPHOROUS");
            if (name.Contains("HIGHPHOS") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("HIGHPHOS", "HIGH PHOSPHOROUS");
            if (name.Contains(" PHOSP") && !name.Contains(" PHOSPH"))
                name = name.Replace(" PHOSP", " PHOSPHOROUS");
            if (name.Contains("H/PHOSPOROUS") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("H/PHOSPOROUS", "HIGH PHOSPHOROUS");
            if (name.Contains("HI-PHOS") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("HI-PHOS", "HIGH PHOSPHOROUS");
            if (name.Contains("R/CREOLE") && !name.Contains("RED CREOLE"))
                name = name.Replace("R/CREOLE", "RED CREOLE");
            if (name.Contains("M/MAKER") && !name.Contains("MONEY MAKER"))
                name = name.Replace("M/MAKER", "MONEY MAKER");
            if (name.Contains("M. MAKER") && !name.Contains("MONEY MAKER"))
                name = name.Replace("M. MAKER", "MONEY MAKER");
            if (name.Contains("S/BABY") && !name.Contains("SUGAR BABY"))
                name = name.Replace("S/BABY", "SUGAR BABY");
            if (name.Contains("TOMATOE") && !name.Contains("TOMATO"))
                name = name.Replace("TOMATOE", "TOMATO");
            if (name.Contains("H/PHOSPHOROUS") && !name.Contains("HIGH PHOSPHOROUS"))
                name = name.Replace("H/PHOSPHOROUS", "HIGH PHOSPHOROUS");
            if (name.Contains("DUODIP") && !name.Contains("DUO DIP"))
                name = name.Replace("DUODIP", "DUO DIP");
            if (name.Contains("AGROLEAF") && !name.Contains("AGRO LEAF"))
                name = name.Replace("AGROLEAF", "AGRO LEAF");
            if (name.Contains("STOCK L.") && !name.Contains("STOCK LICK"))
                name = name.Replace("STOCK L.", "STOCK LICK");
            if (name.Contains("S. L.") && !name.Contains("STOCK LICK"))
                name = name.Replace("S. L.", "STOCK LICK");
            if (name.Contains("STOCKLICK") && !name.Contains("STOCK LICK"))
                name = name.Replace("STOCKLICK", "STOCK LICK");
            if (name.Contains("DARKRED") && !name.Contains("DARK RED"))
                name = name.Replace("DARKRED", "DARK RED");

            if (name.Contains("DUODIP") && !name.Contains("DUO DIP"))
                name = name.Replace("DUODIP", "DUO DIP");

            if (name.Contains("VETRO ") && !name.Contains("VETPRO "))
                name = name.Replace("VETRO ", "VETRO ");

            if (name.Contains("D.LICK") && !name.Contains("DAIRY LICK"))
                name = name.Replace("D.LICK", "DAIRY LICK");

            if (name.Contains("FERTILIZERILITY"))
                name = name.Replace("FERTILIZERILITY", "FERTILITY");

            name = name.Replace("PHOSPHOROUSS", "PHOSPHOROUS");
            return name.Replace("  ", " ").Replace("LTD.", "").Replace("LTD", "");
        }

        public static short LaveteshinDistanceAlgorithm(ItemCode code, ItemCode code2)
        {
            short level = LaveteshinDistanceAlgorithmBody(code.HarmonizedName ?? "", code2.HarmonizedName ?? "");
            if (string.IsNullOrWhiteSpace(code.MeasureUnit)
                || string.IsNullOrWhiteSpace(code.MeasureUnit))
                level += 2;
            else if (code.MeasureUnit != code2.MeasureUnit)
                level += 5;
            return level;
        }

        public static short LaveteshinDistanceAlgorithmBody(string s, string t)
        {
            s = s.ToUpper();
            t = t.ToUpper();

            int n = s.Length, m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
                return (short)m;
            if (m == 0)
                return (short)n;

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return (short)(d[n, m] + 1);
        }

        internal static List<ItemCodeMatch> MatchItemCode(List<ItemCode> nyahururuItemCodes, List<ItemCode> allItemsCodes)
        {
            List<ItemCodeMatch> itemCodeMatches = new();
            nyahururuItemCodes.ForEach(tt =>
            {
                var newCode = MatchCodes(tt, allItemsCodes, 1)?.FirstOrDefault();
                itemCodeMatches.Add(new ItemCodeMatch
                {
                    OriginalCode = tt,
                    MatchedCode = newCode?.Code,
                    MatchStrength = (short)newCode?.Measure
                });
            });
            return itemCodeMatches;
        }

        public static List<(ItemCode Code, short Measure)> MatchCodes(ItemCode tt, List<ItemCode> allItemsCodes, int matchCount = 1)
        {
            var obj = allItemsCodes.FirstOrDefault(m => m.HarmonizedName == tt.HarmonizedName);

            if (obj != null)
                return new() { (obj, 0) };

            return allItemsCodes
                .Select(m =>
                {
                    var mn = LaveteshinDistanceAlgorithm(m, tt);
                    if (m.MeasureUnit != tt.MeasureUnit)
                        mn += 3;
                    return new { Obj = m, Measure = mn };
                }).OrderBy(v => v.Measure)
                .Take(matchCount)
                .Select(m => (m.Obj, m.Measure)).ToList();
        }
    }
}