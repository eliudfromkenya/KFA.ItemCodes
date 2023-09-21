using Avalonia.Controls;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.LevenshteinDistanceAlgorithm;
using KFA.ItemCodes.Views;
using LevenshteinDistanceAlgorithm;
using OfficeOpenXml;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KFA.ItemCodes.ViewModels
{
    public class MainSupplierWindowViewModel : ViewModelBase
    {
        internal static SupplierMainWindow MainWindow;
        internal static ObservableCollection<SupplierCode> models;
        private SupplierCode selectedItem;
        private string message;
        private string errorMessage;
        internal static ObservableCollection<Branch> itemGroups;
        internal static string nextId = null;

        public string? Message { get => message; set => this.RaiseAndSetIfChanged(ref message, value); }
        public string? ErrorMessage { get => errorMessage; set => this.RaiseAndSetIfChanged(ref errorMessage, value); }

        public ICommand RefreshDataCommand { get; }
        public ICommand LoadStockItemsCommand { get; }
        public ICommand AddSupplierCommand { get; }
        public ICommand UpdateSupplierCommand { get; }
        public ICommand MoreCommand { get; }
        public ICommand SearchSupplierCodeCommand { get; }
        public static BehaviorSubject<(string? title, string? message, Exception? ex)> ErrorNotifications { get; set; } = new((null, null, null));

        public static BehaviorSubject<(string? title, string? message)> Notifications { get; set; } = new((null, null));

        public SupplierCode SelectedSupplier { get => selectedItem; set => this.RaiseAndSetIfChanged(ref selectedItem, value); }
        public ObservableCollection<SupplierCode> Models { get => models; set => this.RaiseAndSetIfChanged(ref models, value); }
        public static ObservableCollection<Branch> Branches { get => itemGroups; set => itemGroups = value; }
        public static Branch? CurrentBranch { get; internal set; }

        public MainSupplierWindowViewModel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            RefreshDataCommand = ReactiveCommand.CreateFromTask(async() => RefreshData());
            MoreCommand = ReactiveCommand.CreateFromTask(async tt => await MoreCommands(tt));
            AddSupplierCommand = ReactiveCommand.CreateFromTask(AddSupplier);
            UpdateSupplierCommand = ReactiveCommand.CreateFromTask(UpdateSupplierCode);
            SearchSupplierCodeCommand = ReactiveCommand.CreateFromTask(SearchSupplierCode);
            LoadStockItemsCommand = ReactiveCommand.CreateFromTask(LoadStockItems);
            RefreshData();
            Notifications.Subscribe(OnMessageRecieved);

            ErrorNotifications.Subscribe(OnErrorMessageRecieved);
        }

        private async Task LoadStockItems()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
             {
                 try
                 {
                     App.MainWindow.FindControl<ItemsMainWindow>("ItemsPage").IsVisible = true;
                    //KFA.ItemCodes.Views.MainWindow.Page.Show();
                 }
                 catch (Exception ex)
                 {
                      ErrorMessage = ex.Message;
                 }
             }));
        }

        async Task MoreCommands(object obj)
        {
            await Task.Run(() => Functions.RunOnMain(() =>
             {
                 var page = new MoreCommandsPage
                 {
                      WindowState = WindowState.FullScreen
                 };
                 page.Show();
                 page.WindowState = WindowState.FullScreen;
                 //page.Topmost = true;
             }));
        }
        private void OnErrorMessageRecieved((string title, string message, Exception ex) tt)
        {
            Functions.RunOnMain(() =>
            {
                try
                {
                    ErrorMessage = Message = null;
                    ErrorMessage =
                        string.IsNullOrWhiteSpace(tt.message)
                            ? (tt.ex?.Message)
                            : tt.message;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }

                Functions.RunOnMain(() => ErrorMessage = Message = null, 10000);
            });
        }

        private void OnMessageRecieved((string title, string message) tt)
        {
            Functions.RunOnMain(() =>
            {
                try
                {
                    ErrorMessage = Message = null;
                    Message = tt.message ?? tt.title;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
                Functions.RunOnMain(() => ErrorMessage = Message = null, 10000);
            });
        }

        private async Task SearchSupplierCode()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
            {
                var page = new SearchSuppliersPage
                {
                    SearchBasedSupplierCode = SelectedSupplier?.Code,
                    SearchBasedName = MainWindow.FindControl<AutoCompleteBox>("TxtSearch")?.Text,
                    SupplierCodes = Models?.ToList() ?? new(),
                    WindowState = WindowState.Maximized
                };
                page.Show();
                page.WindowState = WindowState.Maximized;
                //page.Topmost = true;
            }));
        }

        private async Task UpdateSupplierCode()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
          {
              try
              {
                  if (SelectedSupplier == null)
                      throw new Exception("Please select the item to update");

                  EditSupplierPage.SupplierCode = SelectedSupplier?.Code;
                  EditSupplierPage.SupplierName = SelectedSupplier?.OriginalName;
                  EditSupplierPage.isUpdate = true;


                  var page = new EditSupplierPage
                  {
                      WindowState = WindowState.Maximized,
                      Branch = SelectedSupplier?.Branch
                  };

                  if(SelectedSupplier != null)
                  {
                      page.TxtAddress.Text = SelectedSupplier.Address;
                      page.TxtBranches.Text = SelectedSupplier.Branch?.ToString();
                      page.TxtEmail.Text = SelectedSupplier.Email;
                      page.TxtSupplierCode.Text = SelectedSupplier.Code;
                      page.TxtSupplierName.Text = SelectedSupplier.Name;
                      page.TxtTelephone.Text = SelectedSupplier.Telephone;
                  }
                  page.FindControl<AutoCompleteBox>("TxtSupplierCode").IsEnabled = false;
                  page.Show();
                  page.WindowState = WindowState.Maximized;
                  //page.Topmost = true;
              }
              catch (Exception ex)
              {
                  Functions.NotifyError(ex);
              }
          }));
        }

        private async Task AddSupplier()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
             {
                 try
                 {
                     EditSupplierPage.SupplierName = MainWindow.FindControl<AutoCompleteBox>("TxtSearch")?.Text;

                     EditSupplierPage.isUpdate = false;
                     var page = new EditSupplierPage
                     {
                         WindowState = WindowState.Maximized
                     };

                     page.Show();                    

                     page.TxtSupplierName.Text = EditSupplierPage.SupplierName;
                     
                     page.WindowState = WindowState.Maximized;

                     Functions.RunOnBackground(() =>
                     {
                         for (int i = 0; i < 30; i++)
                         {
                             var nxt = MainSupplierWindowViewModel.nextId??"";
                             if(nxt.Length > 3)
                             {
                                   Functions.RunOnMain(() =>
                                   {
                                       try
                                       {
                                           page.TxtSupplierCode.Text = nxt;
                                           var branch = MainSupplierWindowViewModel.CurrentBranch;
                                           if (branch != null)
                                           {
                                               page.TxtBranches.Text = branch.ToString();
                                           }
                                           else
                                           {
                                               page.TxtBranches.Text = $"{MainWindow.FindControl<AutoCompleteBox>("TxtBranchCode")?.Text} - {MainWindow.FindControl<Label>("lblBranchName")?.Content}";
                                           }
                                       }
                                       catch { }
                                   });
                                 break;
                             }
                             Thread.Sleep(TimeSpan.FromSeconds(1));
                         }
                         //var prefix = "S5A";
//                         try
//                         {
//                             var sql = $@"SELECT MAX(CAST(SUBSTR(code, 4) AS UNSIGNED))+1 num FROM
//(SELECT supplier_code code FROM tbl_suppliers UNION SELECT ledger_account_code code FROM tbl_ledger_accounts) A
//WHERE code LIKE '{prefix}%' AND code NOT LIKE '{prefix}7%' AND code != '{prefix}499'";
//                             if (int.TryParse(SupplierDbService.GetMySqlScalar(sql)?.ToString(), out int mm) && mm > 0)
//                                 Functions.RunOnMain(() => page.TxtSupplierCode.Text = $"{prefix}{mm:000}");
//                         }
//                         catch (Exception ex)
//                         {
//                             Functions.NotifyError(ex);
//                         }
                     });
                     //page.Topmost = true;
                 }
                 catch (Exception ex)
                 {
                     Functions.NotifyError(ex);
                 }
             }));
        }

        internal void RefreshData()
        {
            Functions.RunOnBackground(() =>
            {
                try
                {
                    var (suppliers, branches) = SupplierDbService.RefreshMySQLSuppliers();
                    var models = new ObservableCollection<SupplierCode>(suppliers);
                    var allBranches = new ObservableCollection<Branch>(branches);
                    Functions.RunOnMain(() =>
                    {
                        try
                        {
                            Models = models;
                            Branches = allBranches;
                            if (MainWindow.FindControl<AutoCompleteBox>("TxtBranchCode") is AutoCompleteBox auto)
                            {
                                auto.Items = allBranches.Select(x => $"{x.Code}-{x.BranchName}").ToList();
                                var txt = auto.Text;
                                auto.Text = null;
                                auto.Text = txt;
                            }
                        }
                        catch { }
                        try
                        {
                            MainWindow.ReloadData(MainWindow.rbAdvancedSearch.IsChecked);
                        }
                        catch { }
                    });
                }
                catch (Exception ex)
                {
                    Functions.NotifyError(ex);
                }
            }, 1200);
        }
    }
}