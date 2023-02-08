using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.ViewModels;
using LevenshteinDistanceAlgorithm;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace KFA.ItemCodes.Views
{
    public partial class SupplierMainWindow : ReactiveWindow<MainSupplierWindowViewModel>
    {
        private static SupplierMainWindow? page;
        public static SupplierMainWindow Page { get => page ?? new SupplierMainWindow(); }
        public SupplierMainWindow()
        {
            InitializeComponent();
            DataContext = new MainSupplierWindowViewModel() { };
            MainSupplierWindowViewModel.MainWindow = this;
            this.FontSize = 18;
            this.WhenActivated(IsActivated);

        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
        private void IsActivated(CompositeDisposable disposable)
        {
            this.FindControl<Button>("CloseButton")
               .Events().Click.Subscribe(cc =>
               {
                   this.Close();
               }).DisposeWith(disposable);

            var searchCtrl = this.FindControl<AutoCompleteBox>("TxtSearch");
            var rbNormalSearch = this.FindControl<RadioButton>("rbNormalSearch");
            var rbAdvSearch = this.FindControl<RadioButton>("rbAdvancedSearch");
            var dgItems = this.FindControl<DataGrid>("DgItems");

            void ReloadDataGrid(bool? advancedSearch = null)
            {
                try
                {
                    var text = searchCtrl?.Text;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        dgItems.Items = ViewModel?.Models;
                        return;
                    }

                    advancedSearch ??= rbAdvSearch?.IsChecked;
                    text = Matcher.CheckCodesName(Matcher.CheckHarmonizedName(text?.ToUpper())).name;
                    dgItems.Items = SearchService.SearchItemCode(text, ViewModel?.Models, advancedSearch ?? false);
                }
                catch (Exception ex)
                {
                    Functions.NotifyError(ex);
                }
            }

            rbAdvSearch.Checked += (xx, yy) => ReloadDataGrid(true);
            rbNormalSearch.Checked += (xx, yy) => ReloadDataGrid(false);
            this.FindControl<Button>("RefreshButton").Click += (xx, yy) =>
            {
                try
                {
                    if (DataContext is MainSupplierWindowViewModel vm)
                        vm.RefreshData();
                    ReloadDataGrid();
                }
                catch (Exception ex)
                {
                    Functions.NotifyError(ex);
                }
            };
            searchCtrl.Events().TextChanged
              .Throttle(TimeSpan.FromMilliseconds(500))
              .Subscribe(cc => Functions.RunOnMain(() =>
              {
                  ReloadDataGrid();
              })).DisposeWith(disposable);
        }
    }
}
