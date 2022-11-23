using System;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using System.Threading.Tasks;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.ViewModels;
using System.Linq;
using AvaloniaEdit.Utils;
using LevenshteinDistanceAlgorithm;
using System.Collections.Generic;
using OfficeOpenXml;
using Avalonia.Media;
using System.IO;

namespace KFA.ItemCodes.Views
{
    public partial class MoreCommandsPage :Window
    {
        internal string Supplier;
        internal static bool isUpdate = false;
        static internal string ItemCode, ItemName, ItemGroup;
        public ICommand DuplicatedCommand { get; }
        public ICommand HarmonizeCommand { get; }
        public ICommand TransferStocksCommand { get; }
        public ICommand CloseCommand { get; }

        public MoreCommandsPage()
        {
            InitializeComponent();
            DataContext = this;
             
            this.FindControl<Button>("BtnClose").Click += (dd, ff) => this.Close();
            this.FindControl<TextBlock>("TxbHeader").Text = isUpdate ? "MORE SETTINGS" : "MORE SETTING";


            this.FindControl<Button>("BtnDuplicatedItems").Command = DuplicatedCommand = ReactiveCommand.CreateFromTask(async tt => await DuplicatedItems(tt));
            this.FindControl<Button>("BtnTransferStocks").Command = TransferStocksCommand = ReactiveCommand.CreateFromTask(async tt => await  TransferStocks(tt));
            this.FindControl<Button>("BtnHarmonize").Command = HarmonizeCommand = ReactiveCommand.CreateFromTask(async tt => await Harmonize(tt));            
        }
        
        async Task DuplicatedItems(object obj)
        {
            try
            {
                using var excelPackage = new ExcelPackage();
                var repo = new MsExcelReportService();
                repo.GenerateDulplicatedItemCodes2Report(excelPackage, MainWindowViewModel.models.ToList(), @"C:\Users\Eliud\Desktop\Excel Working Files", "all Branches");
                MessageFound("done");
            }
            catch (Exception ex)
            {
                ErrorFound(ex);
            }


        }

        private void MessageFound(string message)
        {
            Functions.RunOnMain(() =>
            {
                this.FindControl<TextBlock>("TxbError").Text = message;
                this.FindControl<TextBlock>("TxbError").Foreground = Brushes.Blue;
            });
        }

        async Task TransferStocks(object obj)
        {
            try
            {
                using var kosgeiFile = new ExcelPackage(new FileInfo(Path.Combine(@"C:\Users\Eliud\Desktop\Excel Working Files", "all Branches.xlsx")));
                using var sheetKosgei = kosgeiFile.Workbook
                                        .Worksheets["SELECTED 5 ITEMS"];

                List<(string Code, string Group, bool IsDefault, bool IsKosgei)> kosgeiItems = new();
                var cells = (from cell in sheetKosgei.Cells["A:A"]
                             select new
                             {
                                 cell.Start.Row,
                                 ItemCode = cell.Value?.ToString(),
                                 Name = sheetKosgei.Cells[cell.Start.Row, 3]?.Value?.ToString()?.Trim(),
                                 Selected = sheetKosgei.Cells[cell.Start.Row, 4]?.Value?.ToString()?.Trim(),
                                 Group = sheetKosgei.Cells[cell.Start.Row, 5]?.Value?.ToString()?.Trim()
                             }).ToArray();
                var sqls = cells
                    .Where
                    (v => CustomValidations.IsValidItemCode(v.ItemCode))
                        .GroupBy(c => c.Group).SelectMany(x =>
                        {
                            var code = x.FirstOrDefault(c => c.Selected == "False")?.ItemCode ?? "XXXX";
                            return x.Select(x => new
                            {
                                Update = $@"UPDATE tbl_stock_items SET item_name = '{x.Name.Replace("'", "''")}' WHERE item_code = '{x.ItemCode}';",
                                IsActive = $@"UPDATE tbl_stock_items SET is_active = {(x.Selected == "False" ? 1 : 0)} WHERE item_code = '{x.ItemCode}';",
                                MoveStock = $@"UPDATE tbl_stock_count_sheets SET item_code = '{code}' WHERE item_code = '{x.ItemCode}';"
                            });
                        }).Select(v => v.IsActive + " \r\n" + v.Update + "\r\n" + v.MoveStock).ToArray();
                var sql = string.Join("\r\n", sqls);
				
				File.WriteAllText(Path.Combine(@"C:\Users\Eliud\Desktop\Excel Working Files","sql.sql"), sql);
            }
            catch (Exception ex)
            {
                ErrorFound(ex);
            }
        }

        async Task Harmonize(object obj)
        {

        }
        private void ErrorFound(Exception ex)
        {
            Functions.RunOnMain(() =>
            {
                this.FindControl<TextBlock>("TxbError").Text = ex.Message;
                this.FindControl<TextBlock>("TxbError").Foreground = Brushes.Blue;
            });
        }
    }
}
