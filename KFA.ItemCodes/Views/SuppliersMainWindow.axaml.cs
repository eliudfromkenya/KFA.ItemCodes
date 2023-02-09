using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.LevenshteinDistanceAlgorithm;
using KFA.ItemCodes.ViewModels;
using LevenshteinDistanceAlgorithm;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace KFA.ItemCodes.Views
{
    public partial class SupplierMainWindow : ReactiveUserControl<MainSupplierWindowViewModel>
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
                   Environment.Exit(0);
               }).DisposeWith(disposable);

            var searchCtrl = this.FindControl<AutoCompleteBox>("TxtSearch");
            var rbNormalSearch = this.FindControl<RadioButton>("rbNormalSearch");
            var rbAdvSearch = this.FindControl<RadioButton>("rbAdvancedSearch");
            var dgSuppliers = this.FindControl<DataGrid>("DgSuppliers");
            var lblBranchName = this.FindControl<Label>("lblBranchName");
            var txtBranchCode = this.FindControl<AutoCompleteBox>("TxtBranchCode");
            List<Branch?>? branches = null;

            if (txtBranchCode != null)
            {
                var last = "";
               txtBranchCode.Events().TextChanged
                    .Throttle(TimeSpan.FromMilliseconds(300))
                    .Subscribe(tt => Functions.RunOnMain(() =>
                    {
                        try
                        {
                            var txt = txtBranchCode.Text;
                            if (txt == last)
                                return;

                            if(branches == null)
                                if (DataContext is MainSupplierWindowViewModel vm)
                                {
                                    branches = vm.Models
                                       .Select(c => c?.Branch)
                                       .GroupBy(c => c?.Code)
                                       .Select(c => c.First())
                                       .ToList();
                                }


                            if (txt?.Length > 3)
                            {
                                var branchCode = txt?[..4];
                                var br = branches?.First(c => c?.Code == branchCode);
                                if (br != null)
                                {
                                    lblBranchName.Content = br?.BranchName;
                                    ReloadDataGrid(rbAdvancedSearch?.IsChecked ?? false);
                                //    if (branches != null)
                                //        if (DataContext is MainSupplierWindowViewModel vm)
                                //        {
                                //            dgSuppliers.Items =
                                //                vm.Models.Where(n => n.Code?
                                //                .StartsWith(br?.Prefix ?? "") ?? false)
                                //                .ToList();
                                //       }
                                }
                                else lblBranchName.Content = txt?[4..];

                                txtBranchCode.Text = branchCode;
                            }
                        }
                        catch { }
                    }));
            }

            void ReloadDataGrid(bool? advancedSearch = null)
            {
                try
                {
                    var text = searchCtrl?.Text;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        dgSuppliers.Items = ViewModel?.Models;
                        return;
                    }

                    advancedSearch ??= rbAdvSearch?.IsChecked;
                    text = Matcher.CheckCodesName(Matcher.CheckHarmonizedName(text?.ToUpper())).name;

                    var models = ViewModel?.Models;

                    var branchCode = txtBranchCode?.Text?[..4];
                    var br = branches?.First(c => c?.Code == branchCode);
                    if (br != null)
                    {
                        lblBranchName.Content = br?.BranchName;
                        if (branches != null)
                            if (DataContext is MainSupplierWindowViewModel vm)
                            {
                               models = new System.Collections.ObjectModel.ObservableCollection<SupplierCode>(  vm.Models.Where(n => n.Code?
                                    .StartsWith(br?.Prefix ?? "") ?? false));
                            }
                    }


                    dgSuppliers.Items = SearchService.SearchSupplierCode(text, models, advancedSearch ?? false);
                }
                catch (Exception ex)
                {
                    Functions.NotifyError(ex);
                }
            }
            void RefreshList()
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
            }



            rbAdvSearch.Checked += (xx, yy) => ReloadDataGrid(true);
            rbNormalSearch.Checked += (xx, yy) => ReloadDataGrid(false);
            this.FindControl<Button>("RefreshButton").Click += (xx, yy) => RefreshList();
            searchCtrl.Events().TextChanged
              .Throttle(TimeSpan.FromMilliseconds(500))
              .Subscribe(cc => Functions.RunOnMain(() =>
              {
                  ReloadDataGrid();
              })).DisposeWith(disposable);

            RefreshList();
        }


    }
}