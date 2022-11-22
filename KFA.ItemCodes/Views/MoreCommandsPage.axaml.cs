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
