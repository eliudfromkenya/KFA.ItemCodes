using Avalonia.Controls;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.Views;
using LevenshteinDistanceAlgorithm;
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

        public string? Message { get => message; set => this.RaiseAndSetIfChanged(ref message, value); }
        public string? ErrorMessage { get => errorMessage; set => this.RaiseAndSetIfChanged(ref errorMessage, value); }

        public ICommand RefreshDataCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand UpdateItemCommand { get; }
        public ICommand SearchItemCodeCommand { get; }
        public static BehaviorSubject<(string? title, string? message, Exception? ex)> ErrorNotifications { get; set; } = new((null, null, null));

        public static BehaviorSubject<(string? title, string? message)> Notifications { get; set; } = new((null, null));

        public ItemCode SelectedItem { get => selectedItem; set => this.RaiseAndSetIfChanged(ref selectedItem, value); }
        public ObservableCollection<LevenshteinDistanceAlgorithm.ItemCode> Models { get => models; set => this.RaiseAndSetIfChanged(ref models, value); }

        public MainWindowViewModel()
        {
            RefreshDataCommand = ReactiveCommand.CreateFromTask(RefreshData);
            AddItemCommand = ReactiveCommand.CreateFromTask(AddItem);
            UpdateItemCommand = ReactiveCommand.CreateFromTask(UpdateItemCode);
            SearchItemCodeCommand = ReactiveCommand.CreateFromTask(SearchItemCode);
            AsyncUtil.RunSync(RefreshData());
            Notifications.Subscribe(OnMessageRecieved);
            ErrorNotifications.Subscribe(OnErrorMessageRecieved);
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

                  var page = new EditItemPage
                  {
                      WindowState = WindowState.Maximized,
                      Supplier = SelectedItem?.Distributor,
                      isUpdate = true
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

                     var page = new EditItemPage
                     {
                         WindowState = WindowState.Maximized,
                         isUpdate = false,
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

        private async Task RefreshData()
        {
            Functions.RunOnBackground(() =>
          {
              var models = new ObservableCollection<ItemCode>(DbService.RefreshMySQLItems());
              Functions.RunOnMain(() => Models = models);
          }, 1200);
        }
    }
}