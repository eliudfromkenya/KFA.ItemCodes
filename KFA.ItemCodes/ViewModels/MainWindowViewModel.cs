using Aura.UI.Controls;
using Avalonia.Controls;
using Avalonia.Media;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.Views;
using LevenshteinDistanceAlgorithm;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KFA.ItemCodes.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    { 
        internal static MainWindow MainWindow;
        private ObservableCollection<ItemCode> models;
        public ICommand RefreshDataCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand UpdateItemCommand { get; }
        public ICommand SearchItemCodeCommand { get; }
        public ObservableCollection<LevenshteinDistanceAlgorithm.ItemCode> Models { get => models; set => this.RaiseAndSetIfChanged(ref models, value); }
        public MainWindowViewModel()
        {
            RefreshDataCommand = ReactiveCommand.CreateFromTask(RefreshData);
            AddItemCommand = ReactiveCommand.CreateFromTask(AddItem);
            UpdateItemCommand = ReactiveCommand.CreateFromTask(UpdateItemCode);
            SearchItemCodeCommand = ReactiveCommand.CreateFromTask(SearchItemCode);
            AsyncUtil.RunSync(RefreshData());
        }

        private async Task SearchItemCode()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
            {
                var page = new SearchItemsPage { ItemCodes = Models?.ToList() ?? new(), WindowState = WindowState.FullScreen };
                page.Show();
                page.WindowState = WindowState.FullScreen;
                page.Topmost = true;
            }));    
        }

        private async Task UpdateItemCode()
        {
            await Task.Run(() => Functions.RunOnMain(() =>
          {
              try
              {

                  var page = new EditItemPage { WindowState = WindowState.FullScreen };
                  page.Show();
                  page.WindowState = WindowState.FullScreen;
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

                     var page = new EditItemPage { WindowState = WindowState.FullScreen };
                     page.Show();
                     page.WindowState = WindowState.FullScreen;
                     //page.Topmost = true;
                 }
                 catch (Exception ex)
                 {
                     Functions.NotifyError(ex);
                 }
             }));
        }

        async Task RefreshData()
        {
            Functions.RunOnBackground(() =>
          {
              var models = new ObservableCollection<ItemCode>(DbService.RefreshMySQLItems());
              Functions.RunOnMain(() => Models = models);
          }, 1200);
        }
    }
}
