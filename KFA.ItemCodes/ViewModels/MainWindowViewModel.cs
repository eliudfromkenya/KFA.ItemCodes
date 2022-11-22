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
    public class MainWindowViewModel : ViewModelBase
    {
        internal static MainWindow MainWindow;
        internal static ObservableCollection<ItemCode> models;
        private ItemCode selectedItem;
        private string message;
        private string errorMessage;
        internal static ObservableCollection<ItemGroup> itemGroups;

        public string? Message { get => message; set => this.RaiseAndSetIfChanged(ref message, value); }
        public string? ErrorMessage { get => errorMessage; set => this.RaiseAndSetIfChanged(ref errorMessage, value); }

        public ICommand RefreshDataCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand UpdateItemCommand { get; }
        public ICommand MoreCommand { get; }
        public ICommand SearchItemCodeCommand { get; }
        public static BehaviorSubject<(string? title, string? message, Exception? ex)> ErrorNotifications { get; set; } = new((null, null, null));

        public static BehaviorSubject<(string? title, string? message)> Notifications { get; set; } = new((null, null));

        public ItemCode SelectedItem { get => selectedItem; set => this.RaiseAndSetIfChanged(ref selectedItem, value); }
        public ObservableCollection<ItemCode> Models { get => models; set => this.RaiseAndSetIfChanged(ref models, value); }
        public ObservableCollection<ItemGroup> ItemGroups { get => itemGroups; set => itemGroups = value; }

        public MainWindowViewModel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            RefreshDataCommand = ReactiveCommand.CreateFromTask(async() => RefreshData());
            MoreCommand = ReactiveCommand.CreateFromTask(async tt => await MoreCommands(tt));
            AddItemCommand = ReactiveCommand.CreateFromTask(AddItem);
            UpdateItemCommand = ReactiveCommand.CreateFromTask(UpdateItemCode);
            SearchItemCodeCommand = ReactiveCommand.CreateFromTask(SearchItemCode);
            RefreshData();
            Notifications.Subscribe(OnMessageRecieved);
            ErrorNotifications.Subscribe(OnErrorMessageRecieved);
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

        private async Task SearchItemCode()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
            {
                var page = new SearchItemsPage
                {
                    SearchBasedItemCode = SelectedItem?.Code,
                    SearchBasedName = MainWindow.FindControl<AutoCompleteBox>("TxtSearch")?.Text,
                    ItemCodes = Models?.ToList() ?? new(),
                    WindowState = WindowState.Maximized
                };
                page.Show();
                page.WindowState = WindowState.Maximized;
                //page.Topmost = true;
            }));
        }

        private async Task UpdateItemCode()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
          {
              try
              {
                  if (SelectedItem == null)
                      throw new Exception("Please select the item to update");

                  EditItemPage.ItemCode = SelectedItem?.Code;
                  EditItemPage.ItemName = SelectedItem?.OriginalName;
                  EditItemPage.isUpdate = true;

                  var page = new EditItemPage
                  {
                      WindowState = WindowState.Maximized,
                      Supplier = SelectedItem?.Distributor
                  };
                  page.FindControl<AutoCompleteBox>("TxtItemCode").IsEnabled = false;
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

        private async Task AddItem()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
             {
                 try
                 {
                     EditItemPage.ItemName = MainWindow.FindControl<AutoCompleteBox>("TxtSearch")?.Text;

                     EditItemPage.isUpdate = false;
                     var page = new EditItemPage
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
                    var (items, groups) = DbService.RefreshMySQLItems();
                    var models = new ObservableCollection<ItemCode>(items);
                    var itemGrps = new ObservableCollection<ItemGroup>(groups);
                    Functions.RunOnMain(() =>
                    {
                        Models = models;
                        ItemGroups = itemGrps;
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