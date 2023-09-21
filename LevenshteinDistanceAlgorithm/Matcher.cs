// using MoreLinq;
using FuzzySharp;
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
                    if ((col.Distributor?.Trim()?.Length > 3)
                        && !col.Name.ToUpper().Contains(col.Distributor))
                    {
                        if (!Regex.IsMatch(col.Name, @"\(.*[a-zA-Z].*\)"))
                        {
                            col.Name = $"{col.Name} ({col.Distributor})";
                            col.OriginalName = $"{col.OriginalName} ({col.Distributor})";
                        }
                    }
                    else if (col.Code.StartsWith("08") && !col.Name.ToUpper().Contains("UNGA"))
                    {
                        if (!Regex.IsMatch(col.Name, @"\(.*[a-zA-Z].*\)"))
                        {
                            col.Name = $"{col.Name} (UNGA)";
                            col.OriginalName = $"{col.OriginalName} (UNGA)";
                        }
                    }
                    else if (col.Code.StartsWith("15") && !col.Name.ToUpper().Contains("COOPER"))
                    {
                        if (!Regex.IsMatch(col.Name, @"\(.*[a-zA-Z].*\)"))
                        {
                            col.OriginalName = $"{col.OriginalName} (COOPER)";
                            col.Name = $"{col.Name} (COOPER)";
                        }
                    }
                    else if (col.Code.StartsWith("17") && !col.Name.ToUpper().Contains("TWIGA"))
                    {
                        if (!Regex.IsMatch(col.Name, @"\(.*[a-zA-Z].*\)"))
                        {
                            col.OriginalName = $"{col.OriginalName} (TWIGA)";
                            col.Name = $"{col.Name} (TWIGA)";
                        }
                    }
                }
                catch { }               

                var (name, unitOfMeasure, groupName, harmonizedName, harmonizedGroupName) = CheckCodesName(col.Name);
                try
                {
                    name = CheckHarmonizedName(name, col.Code);
                }
                catch { }

                var match = Regex.Match(name ?? "", @"\(.*[a-zA-Z].*\) *$");
                if (match.Success && (col?.OriginalName?.Contains(match.Value) ?? false))
                {
                    col.OriginalName = $"{col.OriginalName} {match.Value}";
                }
                col.MeasureUnit = unitOfMeasure;
               
                if (string.IsNullOrWhiteSpace(col.ItemGroup))
                    col.GroupName = groupName;
                col.HarmonizedGroupName=harmonizedGroupName;
                col.HarmonizedName=harmonizedName;
            });
        }

        public static (string? name, string? unitOfMeasure, string? groupName, string? harmonizedName, string? harmonizedGroupName) CheckCodesName(string? name, string? distributor = null, string? itemCode = "")
        {
           try
                {
                if (!string.IsNullOrWhiteSpace(distributor)
                && !int.TryParse(distributor ?? "", out int _))
                    if ((name??"").Split(' ').Any(c => (distributor??"").Split(' ').Any(k => k.Contains(c) || c.Contains(k))))
                    {
                        name = $"{name} ({distributor})";

                    }
                }
                catch (Exception ex) { Functions.Notify("Error 1 " + ex.ToString()); }
				
				try
                {
					name = name?.Replace(" x", " X");
                    if(Regex.IsMatch(name??"", " X *[0-9]+ *[a-zA-Z]+ *$"))
                    {
                       var value = Regex.Match(name ?? "", "X *[0-9]+ *[a-zA-Z]+ *$").Value;
                       name = name?.Replace(value, value[1..]);
                    }
                }
                catch (Exception ex) { Functions.Notify("Error 1 " + ex.ToString()); }

                try
                {
                    var pattern = @"\(.*[A-Z]+.*\)";
                    if (Regex.Match(name ?? "", pattern).Success)
                    {
                        var value = Regex.Match(name ?? "", pattern).Value;
                        name = $"{value[1..^1]} {name?.Replace(value, "")}";
                    }
                    name = name?.Replace(")", " ").Replace("(", " ");
                }
                catch (Exception ex) { Functions.Notify("Error 2A " + ex.ToString()); }

                try
                {
                    var pattern = @"\{.*[A-Z]+.*\}";
                    if (Regex.Match(name ?? "", pattern).Success)
                    {
                        var value = Regex.Match(name ?? "", pattern).Value;
                        name = $"{value[1..^1]} {name?.Replace(value, "")}";
                    }
                    name = name?.Replace("}", " ").Replace("{", " ");
                }
                catch (Exception ex) { Functions.Notify("Error 2A " + ex.ToString()); }

                try
                {
                    var pattern = @"\[.*[A-Z]+.*\]";
                    if (Regex.Match(name ?? "", pattern).Success)
                    {
                        var value = Regex.Match(name ?? "", pattern).Value;
                        name = $"{value[1..^1]} {name?.Replace(value, "")}";
                    }
                    name = name?.Replace("]", " ").Replace("[", " ");
                }
                catch (Exception ex) { Functions.Notify("Error 2A " + ex.ToString()); }
                name = name?.ToUpper();
                try
                {
                    var pattern = @"\"".*[A-Z]+.*\""";
                    if (Regex.Match(name ?? "", pattern).Success)
                    {
                        var value = Regex.Match(name ?? "", pattern).Value;
                        name = $"{value[1..^1]} {name?.Replace(value, "")}";
                    }
                }
                catch (Exception ex) { Functions.Notify("Error 2B " + ex.ToString()); }
                try
                {
                    name = CheckName(name);
                }
                catch (Exception ex) { Functions.Notify("Error 2B " + ex.ToString()); }
                try
                {
                    name = CheckHarmonizedName(name, itemCode);
                }
                catch (Exception ex) { Functions.Notify("Error 2B " + ex.ToString()); } 

            string? unitOfMeasure = null, groupName = null, harmonizedName=null, harmonizedGroupName = null;
                try
                {
                    const string pattern = @"([\d]+\.?[\d]* *[a-zA-Z]{1,5} *$)|([\d]* *x *[\d]+ *[a-zA-Z]{1,5} *$)|([\d]* *\* *[\d]+ *[a-zA-Z]{1,5} *$)|([\d]+ *[a-zA-Z]{1,5} *x *[\d]* *$)|([\d]+ *[a-zA-Z]{1,5} *\* *[\d]* *$)";

                    if (Regex.Match(name ?? "", pattern).Success)
                        unitOfMeasure = Regex.Match(name ?? "", pattern).Value?.Replace("  ", " ")?.Trim();
                    groupName = name?.Trim();
                    if (unitOfMeasure?.Length > 1)
                        groupName = name?.Replace(unitOfMeasure ?? "", "")?.Trim();
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
                        { "JOJEMI",new[]{"JOJ","JOJE", "JJ"} },
                        {"SIMLAW", new[]{ "SIM" } },
                        {"ULTRAVETIS",new []{"ULTRA","ULT","ULTRAVET","ULTVET","ULT VET"} },
                        {"GRIFFATON", new[]{"GRI", "GRIFF","GRIF", } },
                        {"HYGRO",new []{"HYGROTECH","HGRO"} },
                        {"E.A", new[]{"EA", "EASTAFRICA","EAST AFR", "E.AFR"} },
                        {"ROYAL", new[]{"RYL", "ROYL","RL"} },
                        {"SEED-CO", new[]{"SEED", "SEED CO","SEEDCO","SC"} }
                    };
                   
                    foreach (var item in replaces)
                    {
                        try
                        {
                            name = String.Join(" ", name?.Split(' ').Select(x =>
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
                        if (Regex.Match(unitOfMeasure ?? "", pattern).Success)
                        {
                            var value = Regex.Match(unitOfMeasure ?? "", pattern).Value;
                            var replacer = objs.FirstOrDefault(m => m == value.ToUpper());
                            if (!string.IsNullOrWhiteSpace(replacer) && !(value == item.Key))
                                unitOfMeasure = unitOfMeasure?.Replace(value, item.Key);
                            if ($"{item.Key}S" == value.ToUpper() && !(value == item.Key))
                                unitOfMeasure = unitOfMeasure?.Replace(value, item.Key);
                        }
                    }
                }
                catch (Exception ex) { Functions.Notify("Error 4 " + ex.ToString()); }
                try
                {
                    harmonizedName = $@"{string.Join(" ", Regex.Matches(groupName ?? "", "[0-9a-zA-Z]+")
                   .Select(v => v.Value?.ToUpper()?.Trim())
                   .Distinct().OrderBy(c => c))} {unitOfMeasure?.Trim()}".Replace(" ", "");
                }
                catch (Exception ex) { Functions.Notify("Error 5 " + ex.ToString()); }
                try
                {
                    harmonizedGroupName = string.Join(" ", Regex.Matches(groupName ?? "", "[0-9a-zA-Z]+")
                   .Select(v => v.Value?.ToUpper()?.Trim())
                   .Distinct().OrderBy(c => c)).Replace(" ", "");
                }
                catch (Exception ex) { Functions.Notify("Error 5 " + ex.ToString()); }

                try
                {
                    harmonizedGroupName = CheckHarmonizedName(harmonizedGroupName);
                    harmonizedName = CheckHarmonizedName(harmonizedName);
                }
                catch (Exception ex) { Functions.Notify("Error 5 " + ex.ToString()); }

            return (name, unitOfMeasure, groupName, harmonizedName, harmonizedGroupName);
        }

        public static string? CheckHarmonizedName(string? name, string? itemCode = null)
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

            if (name.Contains("ROCK SALT") && !name.Contains("MAGADI SODA"))
                name = name.Replace("ROCK SALT", "MAGADI SODA");

            if (name.Contains("CALIFORNIA W ") && !name.Contains("CALIFORNIA WONDER "))
                name = name.Replace("CALIFORNIA W ", "CALIFORNIA WONDER ");

            if (name.Contains("CALIFONIA") && !name.Contains("CALIFORNIA"))
                name = name.Replace("CALIFONIA", "CALIFORNIA");
            if (name.Contains("WONDERG") && !name.Contains("WONDER G"))
                name = name.Replace("WONDERG", "WONDER G");
            if (name.Contains("CROPW") && !name.Contains("CROP W"))
                name = name.Replace("CROPW", "CROP W");
            if (name.Contains("C WONDER") && !name.Contains("CALIFORNIA WONDER"))
                name = name.Replace("C WONDER", "CALIFORNIA WONDER");
            if (name.Contains("C.WONDER") && !name.Contains("CALIFORNIA WONDER"))
                name = name.Replace("C.WONDER", "CALIFORNIA WONDER");
            if (name.Contains("C/WONDER") && !name.Contains("CALIFORNIA WONDER"))
                name = name.Replace("C/WONDER", "CALIFORNIA WONDER");

           // if (name?.Contains("G")??false  || (itemCode?.StartsWith("42") ?? false))
            {
                if (name.Contains("C W ") && !name.Contains("CALIFORNIA WONDER "))
                    name = name.Replace("C W ", " CALIFORNIA WONDER ");
                if (name.Contains("C.W ") && !name.Contains("CALIFORNIA WONDER "))
                    name = name.Replace("C.W ", " CALIFORNIA WONDER ");
                if (name.Contains("C/W ") && !name.Contains("CALIFORNIA WONDER "))
                    name = name.Replace("C/W ", " CALIFORNIA WONDER ");
            }
            //if (true || (itemCode?.StartsWith("07") ?? false))
            {
                if (name.Contains("HYBRID"))
                    name = name.Replace("HYBRID", "H");
                if (name.Contains("HYBREED"))
                    name = name.Replace("HYBREED", "H");
                if (name.Contains("MAIZE SEED") && !name.Contains("SEED-CO"))
                    name = name.Replace("MAIZE SEED", "H.S.M");
            }
            return name.Replace("  ", " ").Replace("LTD.", "").Replace("LTD", "");
        }

        public static string? CheckName(string? name)
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
            var ma = (short)(Fuzz.PartialRatio(s ?? "", t ?? ""));
            return (short)(ma * -1);
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

            //return allItemsCodes
            //    .Select(m =>
            //    {
            //        var mn = LaveteshinDistanceAlgorithm(m, tt);
            //        if (m.MeasureUnit != tt.MeasureUnit)
            //            mn += 3;
            //        return new { Obj = m, Measure = mn };
            //    }).OrderBy(v => v.Measure)
            //    .Take(matchCount)
            //    .Select(m => (m.Obj, m.Measure)).ToList();

            return allItemsCodes
              .Select(m =>
              {
                  var mn =  LaveteshinDistanceAlgorithm(m, tt);
                  if (m.MeasureUnit != tt.MeasureUnit)
                      mn += 3;
                  return new { Obj = m, Measure = mn };
              }).OrderBy(v => v.Measure)
              .Take(matchCount)
              .Select(m => (m.Obj, m.Measure)).ToList();
        }
    }
}