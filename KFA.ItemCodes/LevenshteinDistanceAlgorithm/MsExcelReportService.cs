using OfficeOpenXml;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
//using MoreLinq;
using System.Text.RegularExpressions;

namespace LevenshteinDistanceAlgorithm;

public class MsExcelReportService
{
    private int currentRow = 3;

    public string GenerateMatchReport(ExcelPackage excelPackage, List<ItemCodeMatch> itemsMatched, string mainFolder, string branch)
    {
        currentRow = 3;
        var matchCodes = $"{branch} Matches";
        var sheet = excelPackage.Workbook.Worksheets.Add(matchCodes);

        sheet.Cells["A1:G1"].Merge = true;
        sheet.Cells["A1"].Value = matchCodes;
        sheet.Row(1).Height = 20;
        sheet.Row(1).Style.Font.Size = 20;
        sheet.Row(1).Style.Font.Color.SetColor(Color.Purple);
        sheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(1).Style.Font.Bold = true;

        sheet.Row(currentRow).Height = 20;
        sheet.Row(currentRow).Style.Font.Size = 12;
        sheet.Row(currentRow).Style.Font.Color.SetColor(Color.DarkGray);
        sheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(currentRow).Style.Font.Bold = true;

        var range = sheet.Cells[currentRow, 1, currentRow, 3];
        range.Style.Font.Color.SetColor(Color.RebeccaPurple);
        range.Style.Font.UnderLine = true;
        range.Style.Font.Size = 14;
        range.Merge = true;
        range.Value = $"{branch} Item Codes";

        range = sheet.Cells[currentRow, 4, currentRow, 7];
        range.Style.Font.Color.SetColor(Color.RebeccaPurple);
        range.Style.Font.UnderLine = true;
        range.Style.Font.Size = 14;
        range.Merge = true;
        range.Value = "Matched Item Codes";
        currentRow++;

        sheet.Cells[currentRow, 1].Value = "Item Code";
        sheet.Cells[currentRow, 2].Value = "Description";
        sheet.Cells[currentRow, 3].Value = "Quantity";

        sheet.Cells[currentRow, 4].Value = "Item Code";
        sheet.Cells[currentRow, 5].Value = "Description";
        sheet.Cells[currentRow, 6].Value = "Match Strength";
        sheet.Cells[currentRow, 7].Value = "Already Verified";

        sheet.Cells[currentRow, 8].Value = "IUOM";
        sheet.Cells[currentRow, 9].Value = "Buying_Price";
        sheet.Cells[currentRow, 10].Value = "Sale_Price";
        sheet.Cells[currentRow, 11].Value = "Closing";

        sheet.Cells[currentRow, 4, currentRow, 7].Style.Font.Color.SetColor(Color.RebeccaPurple);
        sheet.Cells[currentRow, 1, currentRow, 7].Style.Font.UnderLine = true;
        currentRow++;

        itemsMatched.OrderBy(c => c.MatchStrength)
            .ToList().ForEach(col =>
        {
            if (!col?.MatchedCode?.IsVerified ?? false)
                sheet.Cells[currentRow, 1, currentRow, 7].Style.Fill.SetBackground(Color.Lavender);
            sheet.Cells[currentRow, 1].Value = col?.OriginalCode?.Code;
            sheet.Cells[currentRow, 2].Value = col?.OriginalCode?.Name;
            sheet.Cells[currentRow, 3].Value = col?.OriginalCode?.Quantity;

            if (col?.MatchedCode != null)
            {
                sheet.Cells[currentRow, 4].Value = col?.MatchedCode?.Code;
                sheet.Cells[currentRow, 5].Value = col?.MatchedCode?.Name;
                sheet.Cells[currentRow, 6].Value = col?.MatchStrength;
                sheet.Cells[currentRow, 7].Value = (col?.MatchedCode.IsVerified ?? false) ? "Yes" : "No";
                sheet.Cells[currentRow, 4].Style.Font.Bold = true;

                if (!string.IsNullOrWhiteSpace(col?.OriginalCode?.Narration))
                {
                    var decimals = col.OriginalCode
                    .Narration.Split(",")
                    .Select(m => decimal.TryParse(m, out decimal mnx) ? mnx : 0).ToArray();
                    if(decimals.Length > 3)
                    {
                        sheet.Cells[currentRow, 8].Value = col.OriginalCode
                    .Narration.Split(",").FirstOrDefault();
                        sheet.Cells[currentRow, 9].Value = decimals[1];
                        sheet.Cells[currentRow, 10].Value = decimals[2];
                        sheet.Cells[currentRow, 11].Value = decimals[3];
                    }
                }
            }
            range = sheet.Cells[currentRow, 4, currentRow, 6];
            range.Style.Font.Color.SetColor(Color.DarkRed);
            currentRow++;
        });

        var file = Path.Combine(mainFolder, $"{branch}.xlsx");
        //excelPackage.SaveAs(new FileInfo(file), "Zanas2022");
        excelPackage.SaveAs(new FileInfo(file));
        return file;
    }



    public string GenerateGroupsCodeReport(ExcelPackage excelPackage, List<ItemCode> items, string mainFolder, string branch)
    {
        currentRow = 3;
        var matchCodes = $"Master Item Code Groups";
        var sheet = excelPackage.Workbook.Worksheets.Add(matchCodes);

        sheet.Cells["A1:G1"].Merge = true;
        sheet.Cells["A1"].Value = matchCodes;
        sheet.Row(1).Height = 20;
        sheet.Row(1).Style.Font.Size = 20;
        sheet.Row(1).Style.Font.Color.SetColor(Color.Purple);
        sheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(1).Style.Font.Bold = true;

        sheet.Row(currentRow).Height = 20;
        sheet.Row(currentRow).Style.Font.Size = 12;
        sheet.Row(currentRow).Style.Font.Color.SetColor(Color.DarkGray);
        sheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(currentRow).Style.Font.Bold = true;

        var range = sheet.Cells[currentRow, 1, currentRow, 6];
        range.Style.Font.Color.SetColor(Color.RebeccaPurple);
        range.Style.Font.UnderLine = true;
        range.Style.Font.Size = 14;
        range.Merge = true;
        range.Value = matchCodes;
        currentRow++;

        sheet.Cells[currentRow, 1].Value = "Item Code";
        sheet.Cells[currentRow, 2].Value = "Description";
        sheet.Cells[currentRow, 3].Value = "Quantity";
        sheet.Cells[currentRow, 4].Value = "Unit Of Sale";
        sheet.Cells[currentRow, 5].Value = "Item Group Name";
        sheet.Cells[currentRow, 6].Value = "Is Duplicate";

        sheet.Cells[currentRow, 4, currentRow, 6].Style.Font.Color.SetColor(Color.RebeccaPurple);
        sheet.Cells[currentRow, 1, currentRow, 6].Style.Font.UnderLine = true;
        currentRow++;

        items
            .GroupBy(v => v.HarmonizedGroupName)
            .Where(c => c.Count() > 1)            
            .OrderBy(c => c.FirstOrDefault()?.GroupName)
            .ToList().ForEach(grp =>
            {
                range = sheet.Cells[currentRow, 1, currentRow, 6];
                range.Merge = true;
                range.Value = grp.First().GroupName;
                range.Style.Font.Color.SetColor(Color.DarkRed);
                currentRow++;

                List<string> doneCols = new();

                foreach (var col in grp
                   .OrderBy(c => c.Quantity)
                   .ThenBy(c => c.MeasureUnit)
                   .ThenBy(x => x.Name))
                {
                    if (!col?.IsVerified ?? false)
                        sheet.Cells[currentRow, 1, currentRow, 5].Style.Fill.SetBackground(Color.Lavender);
                    sheet.Cells[currentRow, 1].Value = col?.Code;
                    sheet.Cells[currentRow, 2].Value = col?.Name;
                    sheet.Cells[currentRow, 3].Value = col?.Quantity;
                    sheet.Cells[currentRow, 4].Value = col?.MeasureUnit;
                    sheet.Cells[currentRow, 5].Value = col?.GroupName;
                    sheet.Cells[currentRow, 6].Value = doneCols.Contains(col?.Name ?? "") ? "Yes" : "No";
                    doneCols.Add(col?.Name ?? "");

                    currentRow++;
                }
                currentRow += 3;
            });

        var file = Path.Combine(mainFolder, $"{branch}.xlsx");
        //excelPackage.SaveAs(new FileInfo(file), "Zanas2022");
        excelPackage.SaveAs(new FileInfo(file));
        return file;
    }


    public string GenerateDulplicatedItemCodes2Report(ExcelPackage excelPackage, List<ItemCode> items, string mainFolder, string branch)
    {
        currentRow = 3;
        var matchCodes = $"Duplicated Item Codes";
        var sheet = excelPackage.Workbook.Worksheets.Add(matchCodes);

        sheet.Cells["A1:G1"].Merge = true;
        sheet.Cells["A1"].Value = matchCodes;
        sheet.Row(1).Height = 20;
        sheet.Row(1).Style.Font.Size = 20;
        sheet.Row(1).Style.Font.Color.SetColor(Color.Purple);
        sheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(1).Style.Font.Bold = true;

        sheet.Row(currentRow).Height = 20;
        sheet.Row(currentRow).Style.Font.Size = 12;
        sheet.Row(currentRow).Style.Font.Color.SetColor(Color.DarkGray);
        sheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(currentRow).Style.Font.Bold = true;

        var range = sheet.Cells[currentRow, 1, currentRow, 5];
        range.Style.Font.Color.SetColor(Color.RebeccaPurple);
        range.Style.Font.UnderLine = true;
        range.Style.Font.Size = 14;
        range.Merge = true;
        range.Value = matchCodes;
        currentRow++;

        sheet.Cells[currentRow, 1].Value = "Item Code";
        sheet.Cells[currentRow, 2].Value = "Description";
        sheet.Cells[currentRow, 3].Value = "Quantity";
        sheet.Cells[currentRow, 4].Value = "Unit Of Sale";
        sheet.Cells[currentRow, 5].Value = "Is Duplicate";

        sheet.Cells[currentRow, 4, currentRow, 5].Style.Font.Color.SetColor(Color.RebeccaPurple);
        sheet.Cells[currentRow, 1, currentRow, 5].Style.Font.UnderLine = true;
        currentRow++;

        var itemGroups = (new[] { "Nyahururu Branch Eliud.xlsx", "Nyahururu Branch seroney.xlsx" })
             .SelectMany(x =>
             {
                 using var nyahururuDifferenceFile = new ExcelPackage(new FileInfo(Path.Combine(mainFolder, x)));

                 using var sheet = nyahururuDifferenceFile.Workbook
                             .Worksheets["Duplicated Item Codes"];
                 var cells = (from cell in sheet.Cells["A:A"]
                             select cell).ToArray();

                 var objs = cells.Where(cell => CustomValidations.IsValidItemCode((int.TryParse(cell?.Value?.ToString() ?? "", out int nc) ? nc : 0).ToString("000000")))
                   .Select(cell => new
                   {
                       ItemCode = sheet.Cells[cell.Start.Row, 1].Value?.ToString(),
                       ItemName = sheet.Cells[cell.Start.Row, 2].Value?.ToString(),
                       Quantity = decimal.TryParse(sheet.Cells[cell.Start.Row, 3].Value?.ToString(), out decimal mx) ? mx : -1,
                       Unit = sheet.Cells[cell.Start.Row, 4].Value?.ToString(),
                       IsSelectedByDefault = sheet.Cells[cell.Start.Row, 5].Value?.ToString() == "No",
                       IsSelectedByOne = sheet.Cells[cell.Start.Row, 8].Value?.ToString()?.Trim() == "1",
                       NewItemCode = sheet.Cells[cell.Start.Row, 9].Value?.ToString(),
                       HasNewItemCode = CustomValidations.IsValidItemCode((int.TryParse(sheet.Cells[cell.Start.Row, 9].Value?.ToString() ?? "", out int nc) ? nc : 0).ToString("000000"))
                   }).ToArray();
                 return objs;
             }).GroupBy(v => v.ItemName)
             .Select(c =>
             {
                 string? newItemCode = null;
                 var hasNewItemCode = c.Any(m => m.HasNewItemCode);
                 newItemCode = c.FirstOrDefault(c => c.HasNewItemCode || c.NewItemCode?.Length > 4 )?.NewItemCode;

                 var codes = c.Select(m => m.ItemCode?.Trim()).ToArray();

                 var selectedCode = c.First().ItemCode;
                 if (!string.IsNullOrWhiteSpace(newItemCode))
                     selectedCode = c.First(v => v.NewItemCode == newItemCode).ItemCode;
                 else if (c.Any(n => n.IsSelectedByOne))
                     selectedCode = c.First(n => n.IsSelectedByOne).ItemCode;
                 else if (c.Any(n => n.IsSelectedByDefault))
                     selectedCode = c.First(n => n.IsSelectedByDefault).ItemCode;

                 string unit = "KG", value = c.First().Unit ?? "";
                  const string unitMeasurePattern = @"^ *[\d]+\.?[\d]* *[a-zA-Z]{1,5} *$";
                 if(Regex.Match(value, unitMeasurePattern).Success)
                 {
                     value = Regex.Match(value, @"^ *[\d]+\.?[\d]* *").Value;
                     unit = c.First().Unit.Replace(value, "").ToUpper().Trim();
                 }
                 return new
                 {
                     HasNewItemCode = hasNewItemCode,
                     NewItemCode = newItemCode,
                     ItemCodes = codes,
                     Name=c.Key,
                     SelectedItemCode = selectedCode,
                     UnitOfMeasure = unit
                 };
             }).ToList();

        
        using var unitsOfMeasureSheet = excelPackage.Workbook.Worksheets.Add("Units Of Measure");
        int measureCount = 1;
        itemGroups.Select(v => v.UnitOfMeasure)
            .Distinct().OrderBy(m => m)
            .ToList().ForEach(x =>
            {
                try
                {
                    unitsOfMeasureSheet.Cells[$"A{measureCount++}"].Value = x;
                }
                catch { }
            });

        using var masterSheet = excelPackage.Workbook.Worksheets[0];
        var itemRows = masterSheet.Cells["A:A"]
            .Select(x => new
            {
                Value = x.Value?.ToString()?.Trim(),
                x.Start.Row
            }).Where(m => m.Value != null &&
               CustomValidations.IsValidItemCode(m.Value)).ToArray();

        var itemInvalidRows = masterSheet.Cells["A:A"]
           .Select(x => new
           {
               Value = x.Value?.ToString()?.Trim(),
               x.Start.Row
           }).Where(m => m.Value != null &&
              !CustomValidations.IsValidItemCode(m.Value)).ToArray();

        foreach (var itemRow in itemRows)
        {
            try
            {
                var itm = items.FirstOrDefault(c => itemRow.Value?.ToString()?.Trim() == c.Code);
                if (itm != null)
                {
                    masterSheet.Cells[$"A{itemRow.Row}"].Value = itemRow.Value?.ToString()?.Trim();
                    masterSheet.Cells[$"C{itemRow.Row}"].Value = itm.Name;
                }
            }
            catch { }
        }

        var maxRow = masterSheet.Dimension.Rows;
        foreach (var itemRow in itemInvalidRows)
        {
            try
            {
                if(itemRow.Row > 3)
                {
                    range = masterSheet.Cells[itemRow.Row, 1, itemRow.Row, 150];
                    range.Style.Font.Color.SetColor(Color.DarkRed);
                    range.Style.Fill.SetBackground(Color.HotPink);
                    range.Style.Font.Bold = true;

                    range = masterSheet.Cells[itemRow.Row, 1, itemRow.Row, 1];
                    range.Style.Font.Color.SetColor(Color.RebeccaPurple);
                    range.Style.Font.UnderLine = true;
                    range.Style.Font.Size = 14;

                    masterSheet.Cells[$"CI{itemRow.Row}"].Value = "DELXX";
                }
            }
            catch { }
        }

        foreach (var item in items.OrderBy(m => m.Code))
        {
            try
            {
                var itm = itemRows.FirstOrDefault(c => c.Value == item.Code);
                if (itm != null)
                    continue;

                if (!CustomValidations.IsValidItemCode(item.Code??""))
                    continue;

                var closeItemRow = itemRows.First();
                for (int i = 6 - 1; i >= 1; i--)
                {
                    try
                    {
                        var rows = itemRows.Where(m => m.Value[..i] == item.Code?[..i]).ToArray();
                        if (rows.Any())
                        {
                            closeItemRow = rows.First();
                            break;
                        }
                    }
                    catch { }
                }
                if (itm == null)
                {
                    maxRow++;
                    masterSheet.Cells[closeItemRow.Row, 1, closeItemRow.Row, 150]
                        .Copy(masterSheet.Cells[maxRow, 1, maxRow, 150]);
                    masterSheet.Cells[$"A{maxRow}"].Value = item.Code;
                    masterSheet.Cells[$"C{maxRow}"].Value = item.Name;
                    masterSheet.Cells[$"M{maxRow}"].Value = "";//supplier
                    masterSheet.Cells[$"CF{maxRow}"].Value = "";//selected item code
                    masterSheet.Cells[$"AS{maxRow}"].Value = "KG";
                    masterSheet.Cells[$"CI{maxRow}"].Value = "ADDX";
                    range = masterSheet.Cells[maxRow, 1, maxRow, 150];
                    range.Style.Font.Color.SetColor(Color.RebeccaPurple);
                    range.Style.Fill.SetBackground(Color.LightGreen);
                }
            }
            catch { }
        }

        itemRows = masterSheet.Cells["A:A"]
            .Select(x => new
            {
                Value = x.Value?.ToString()?.Trim(),
                x.Start.Row
            }).Where(m => m.Value != null &&
               CustomValidations.IsValidItemCode(m.Value)).ToArray();

        items.GroupBy(c => c.HarmonizedName).ToList().ForEach(x =>
        {
            var itm = itemGroups.FirstOrDefault(c => x.Any(op => op.Name == c.Name));

            foreach (var item in x)
            {
                var currentCodes = x.Select(c => c.Code).ToArray();
                var itemRow = itemRows.FirstOrDefault(x => x.Value == item.Code);

                if (itemRow != null)
                {
                    if (x.Count() == 1)
                    {
                        try
                        {
                            masterSheet.Cells[$"CF{itemRow.Row}"].Value = "1";
                        }
                        catch { }
                    }
                    var selectedCode = itm?.SelectedItemCode;

                    //masterSheet.Cells[$"A{itemRow.Row}"].Value = item.Code;
                    masterSheet.Cells[$"C{itemRow.Row}"].Value = item.Name;
                    masterSheet.Cells[$"CF{itemRow.Row}"].Value = selectedCode;
                    masterSheet.Cells[$"AS{itemRow.Row}"].Value = itm?.UnitOfMeasure ?? "KG";

                    if (!string.IsNullOrWhiteSpace(selectedCode) && x.Any(m => m.Code == selectedCode))
                    {
                        if (selectedCode == item.Code)
                        {
                            masterSheet.Cells[$"CF{itemRow.Row}"].Value = "1";
                            if (masterSheet.Cells[$"CI{itemRow.Row}"].Value.ToString() == "DEL")
                                masterSheet.Cells[$"CI{itemRow.Row}"].Value = "UPDX";

                            if (itm?.NewItemCode?.Trim()?.Length > 4)
                            {
                                var code = itm?.NewItemCode?.Trim();
                                if (int.TryParse(code, out int mm))
                                    code = mm.ToString("000000");
                                if (!itemRows.Any(v => v.Value == code))
                                {
                                    masterSheet.Cells[$"CF{itemRow.Row}"].Value = $"ShortChanged: ({masterSheet.Cells[$"A{itemRow.Row}"].Value})";
                                    masterSheet.Cells[$"A{itemRow.Row}"].Value = code;
                                    range = masterSheet.Cells[itemRow.Row, 1, itemRow.Row, 150];
                                    range.Style.Font.Color.SetColor(Color.DarkBlue);
                                    range.Style.Fill.SetBackground(Color.GreenYellow);
                                    range = masterSheet.Cells[itemRow.Row, 1, itemRow.Row, 1];
                                    range.Style.Font.UnderLine = true;
                                    range.Style.Font.Size = 14;
                                }
                            }
                        }
                        else
                        {
                            if (masterSheet.Cells[$"CI{itemRow.Row}"].Value.ToString() != "DEL")
                                masterSheet.Cells[$"CI{itemRow.Row}"].Value = "DELX";
                            range = masterSheet.Cells[itemRow.Row, 1, itemRow.Row, 150];
                            range.Style.Font.Color.SetColor(Color.DarkRed);
                            range.Style.Fill.SetBackground(Color.LightSlateGray);
                        }
                    }
                }
            }
            //individual added units of measures
            //Code macthing as by branch
        });

        items
            .GroupBy(v => v.HarmonizedName)
            .Where(x => x.Count() > 1)
            .OrderByDescending(c => c?.Count())
            .ToList().ForEach(grp =>
            {
                List<string> doneCols = new();
                var itm = itemGroups.FirstOrDefault(c => grp.Any(op => op.Name == c.Name));
                var isFound = grp.Any(m => m.Code == itm?.SelectedItemCode);
                foreach (var col in grp
                   .OrderByDescending(c => c.Quantity)
                   .ThenBy(x => x.MeasureUnit)
                   .ThenBy(x => x.Name))
                {
                    var isDuplicate = doneCols.Contains((col?.HarmonizedName ?? ""));
                    doneCols.Add((col?.HarmonizedName ?? ""));
                    if (isFound)
                        isDuplicate = itm?.SelectedItemCode != col?.Code;

                    if (!col?.IsVerified ?? false)
                        sheet.Cells[currentRow, 1, currentRow, 5].Style.Fill.SetBackground(Color.Lavender);
                    sheet.Cells[currentRow, 1].Value = col?.Code;
                    sheet.Cells[currentRow, 2].Value = col?.Name;
                    sheet.Cells[currentRow, 3].Value = col?.Quantity;
                    sheet.Cells[currentRow, 4].Value = col?.MeasureUnit;
                    sheet.Cells[currentRow, 5].Value = isDuplicate ? "Yes" : "No";

                    if (isDuplicate)
                    {
                        range = sheet.Cells[currentRow, 1, currentRow, 5];
                        range.Style.Font.Color.SetColor(Color.DarkRed);
                    }
                    currentRow++;
                }
                currentRow += 1;
            });

        var file = Path.Combine(mainFolder, $"{branch}.xlsx");
        excelPackage.SaveAs(new FileInfo(file));
        return file;
    }

    public string GenerateDulplicatedItemCodesReport(ExcelPackage excelPackage, List<ItemCode> items, string mainFolder, string branch)
    {
        currentRow = 3;
        var matchCodes = $"Duplicated Item Codes";
        var sheet = excelPackage.Workbook.Worksheets.Add(matchCodes);

        sheet.Cells["A1:G1"].Merge = true;
        sheet.Cells["A1"].Value = matchCodes;
        sheet.Row(1).Height = 20;
        sheet.Row(1).Style.Font.Size = 20;
        sheet.Row(1).Style.Font.Color.SetColor(Color.Purple);
        sheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(1).Style.Font.Bold = true;

        sheet.Row(currentRow).Height = 20;
        sheet.Row(currentRow).Style.Font.Size = 12;
        sheet.Row(currentRow).Style.Font.Color.SetColor(Color.DarkGray);
        sheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(currentRow).Style.Font.Bold = true;

        var range = sheet.Cells[currentRow, 1, currentRow, 5];
        range.Style.Font.Color.SetColor(Color.RebeccaPurple);
        range.Style.Font.UnderLine = true;
        range.Style.Font.Size = 14;
        range.Merge = true;
        range.Value = matchCodes;
        currentRow++;

        sheet.Cells[currentRow, 1].Value = "Item Code";
        sheet.Cells[currentRow, 2].Value = "Description";
        sheet.Cells[currentRow, 3].Value = "Quantity";
        sheet.Cells[currentRow, 4].Value = "Unit Of Sale";
        sheet.Cells[currentRow, 5].Value = "Is Duplicate";

        sheet.Cells[currentRow, 4, currentRow, 5].Style.Font.Color.SetColor(Color.RebeccaPurple);
        sheet.Cells[currentRow, 1, currentRow, 5].Style.Font.UnderLine = true;
        currentRow++;

        items
            .GroupBy(v => v.HarmonizedName)
            .Where(x => x.Count() > 1)
            .OrderByDescending(c => c?.Count())
            .ToList().ForEach(grp =>
            {
                List<string> doneCols = new();

               


                foreach (var col in grp
                   .OrderByDescending(c => c.Quantity)
                   .ThenBy(x => x.MeasureUnit)
                   .ThenBy(x => x.Name))
                {
                    var isDuplicate = doneCols.Contains((col?.HarmonizedName ?? ""));
                     doneCols.Add((col?.HarmonizedName ?? ""));

                    if (!col?.IsVerified ?? false)
                        sheet.Cells[currentRow, 1, currentRow, 5].Style.Fill.SetBackground(Color.Lavender);
                    sheet.Cells[currentRow, 1].Value = col?.Code;
                    sheet.Cells[currentRow, 2].Value = col?.Name;
                    sheet.Cells[currentRow, 3].Value = col?.Quantity;
                    sheet.Cells[currentRow, 4].Value = col?.MeasureUnit;
                    sheet.Cells[currentRow, 5].Value = isDuplicate ? "Yes" : "No";

                    //try
                    //{
                    //    var chkBox = sheet.Drawings.AddCheckBoxControl(col?.Code);
                    //    chkBox.SetPosition(7, currentRow, 1, 1);
                    //    chkBox.LinkedCell = new ExcelAddress("$G$1");
                    //}
                    //catch { }


                    if (isDuplicate)
                    {
                        range = sheet.Cells[currentRow, 1, currentRow, 5];
                        range.Style.Font.Color.SetColor(Color.DarkRed);
                    }
                    currentRow++;
                }
                currentRow += 1;
            });

        var file = Path.Combine(mainFolder, $"{branch}.xlsx");
        excelPackage.SaveAs(new FileInfo(file));
        return file;
    }

    public string GenerateDulplicatedItemCodesOnlyReport(ExcelPackage excelPackage, List<ItemCode> items, string mainFolder, string branch)
    {
        currentRow = 3;
        var matchCodes = $"Duplicated Item Codes Only";
        var sheet = excelPackage.Workbook.Worksheets.Add(matchCodes);

        sheet.Cells["A1:G1"].Merge = true;
        sheet.Cells["A1"].Value = matchCodes;
        sheet.Row(1).Height = 20;
        sheet.Row(1).Style.Font.Size = 20;
        sheet.Row(1).Style.Font.Color.SetColor(Color.Purple);
        sheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(1).Style.Font.Bold = true;

        sheet.Row(currentRow).Height = 20;
        sheet.Row(currentRow).Style.Font.Size = 12;
        sheet.Row(currentRow).Style.Font.Color.SetColor(Color.DarkGray);
        sheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        sheet.Row(currentRow).Style.Font.Bold = true;

        var range = sheet.Cells[currentRow, 1, currentRow, 5];
        range.Style.Font.Color.SetColor(Color.RebeccaPurple);
        range.Style.Font.UnderLine = true;
        range.Style.Font.Size = 14;
        range.Merge = true;
        range.Value = matchCodes;
        currentRow++;

        sheet.Cells[currentRow, 1].Value = "Item Code";
        sheet.Cells[currentRow, 2].Value = "Description";
        sheet.Cells[currentRow, 3].Value = "Quantity";
        sheet.Cells[currentRow, 4].Value = "Unit Of Sale";
        sheet.Cells[currentRow, 5].Value = "Is Duplicate";

        sheet.Cells[currentRow, 4, currentRow, 5].Style.Font.Color.SetColor(Color.RebeccaPurple);
        sheet.Cells[currentRow, 1, currentRow, 5].Style.Font.UnderLine = true;
        currentRow++;

        items
            .GroupBy(v => v.HarmonizedName)
            .OrderBy(c => c.FirstOrDefault()?.HarmonizedName)
            .ToList().ForEach(grp =>
            {
                List<string> doneCols = new();
                foreach (var col in grp
                   .OrderBy(c => c.MeasureUnit)
                   .ThenBy(x => x.Quantity)
                   .ThenBy(x => x.HarmonizedName))
                {
                    var isDuplicate = doneCols.Contains(col?.Name ?? "");

                    if (isDuplicate)
                    {
                        if (!col?.IsVerified ?? false)
                            sheet.Cells[currentRow, 1, currentRow, 5].Style.Fill.SetBackground(Color.Lavender);
                        sheet.Cells[currentRow, 1].Value = col?.Code;
                        sheet.Cells[currentRow, 2].Value = col?.Name;
                        sheet.Cells[currentRow, 3].Value = col?.Quantity;
                        sheet.Cells[currentRow, 4].Value = col?.MeasureUnit;
                        sheet.Cells[currentRow, 5].Value = isDuplicate ? "Yes" : "No";
                        currentRow++;
                    }
                    doneCols.Add(col?.Name ?? "");                    
                }
            });

        var file = Path.Combine(mainFolder, $"{branch}.xlsx");
        excelPackage.SaveAs(new FileInfo(file));
        return file;
    }
}