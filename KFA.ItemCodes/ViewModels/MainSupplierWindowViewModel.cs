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

        public SupplierCode SelectedItem { get => selectedItem; set => this.RaiseAndSetIfChanged(ref selectedItem, value); }
        public ObservableCollection<SupplierCode> Models { get => models; set => this.RaiseAndSetIfChanged(ref models, value); }
        public static ObservableCollection<Branch> Branches { get => itemGroups; set => itemGroups = value; }

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
                    KFA.ItemCodes.Views.MainWindow.Page.Show();
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
                    SearchBasedSupplierCode = SelectedItem?.Code,
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
                  if (SelectedItem == null)
                      throw new Exception("Please select the item to update");

                  EditSupplierPage.SupplierCode = SelectedItem?.Code;
                  EditSupplierPage.SupplierName = SelectedItem?.OriginalName;
                  EditSupplierPage.isUpdate = true;

                  var page = new EditSupplierPage
                  {
                      WindowState = WindowState.Maximized,
                      Supplier = SelectedItem?.Branch
                  };
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
                     page.WindowState = WindowState.Maximized;
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
                    var (items, groups) = DbService.RefreshMySQLSuppliers();
                    var models = new ObservableCollection<SupplierCode>(items);
                    var itemGrps = new ObservableCollection<Branch>(groups);
                    Functions.RunOnMain(() =>
                    {
                        Models = models;
                        Branchs = itemGrps;
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