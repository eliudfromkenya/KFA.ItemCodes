using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KFA.ItemCodes.ViewModels;
using LevenshteinDistanceAlgorithm;

namespace KFA.ItemCodes.Views
{
    public partial class SearchSuppliersPage : Window
    {
        internal List<SupplierCode> SupplierCodes { get; set; }
        public string? SearchBasedName { get; internal set; }
        public string? SearchBasedSupplierCode { get; internal set; }

        public SearchSuppliersPage()
        {
            InitializeComponent();
            DataContext = this;
            this.FindControl<Button>("BtnSearchBackwards").Click += (xx,yy) => Search_Forward();
            this.FindControl<Button>("BtnSearchForwards").Click += (xx,yy) => Search_Backward();
            this.FindControl<Button>("BtnAddSupplier").Click += (xx,yy) => {
                try
                {
                    EditSupplierPage.SupplierName = SearchBasedName;

                    EditSupplierPage.isUpdate = false;
                    var page = new EditSupplierPage
                    {
                        WindowState = WindowState.Maximized
                    };

                    page.Show();
                    page.WindowState = WindowState.Maximized;
                    this.Close();
                }
                catch (Exception ex)
                {
                    Functions.NotifyError(ex);
                }
            };
            this.FindControl<Button>("BtnClose").Click += (xx,yy) => this.Close();
            var dgSuppliers = this.FindControl<DataGrid>("DgSuppliers");
            dgSuppliers.SelectionChanged += (xx, yy) =>
            {
                try
                {
                    if (dgSuppliers.SelectedItem != null)
                        EditSupplierPage.SupplierCode =
                           ((dynamic)dgSuppliers.SelectedItem)
                           .supplierFrom;
                }
                catch { }
            };

            Functions.RunOnBackground(() =>
            {
                try
                {
                    if(CustomValidations.IsValidSupplierCode(SearchBasedSupplierCode ?? " "))
                    {
                        Functions.RunOnMain(() => this.FindControl<AutoCompleteBox>("TxtSearch").Text = SearchBasedSupplierCode);
                    }
                    else if (!string.IsNullOrEmpty(SearchBasedName))
                    {
                        var code = SupplierChecker
                        .SearchSupplierByName(SearchBasedName, SupplierCodes)?
                        .FirstOrDefault()?
                        .Code;

                        if (CustomValidations.IsValidSupplierCode(code ?? ""))
                            Functions.RunOnMain(() => this.FindControl<AutoCompleteBox>("TxtSearch").Text = code);
                    }
                }
                catch { }
            });
        }

        private void Search_Backward()
        {
            try
            { 
                var dgSuppliers = this.FindControl<DataGrid>("DgSuppliers");
                var code = this.FindControl<AutoCompleteBox>("TxtSearch").Text;
                var suppliers = SupplierChecker.SearchSupplierForward(code, SupplierCodes);
                dgSuppliers.Items = suppliers.Select(v => new
                {
                    v.supplierFrom,
                    v.supplierTo,
                    v.count,
                    Group = MainSupplierWindowViewModel.Branches.FirstOrDefault(m => m.Code == v.supplierFrom?[..6])?.GroupName,
                    Text = v.count == 0 ? v.supplierFrom : $"{v.supplierFrom}-{v.supplierTo} ({v.count + 1} spaces)"
                });
                EditSupplierPage.SupplierCode = suppliers?.First().supplierFrom;
            }
            catch (Exception ex)
            {
                Functions.NotifyError(ex);
            }
        }

        private void Search_Forward()
        {
            try
            {
                var code = this.FindControl<AutoCompleteBox>("TxtSearch").Text;
                var suppliers = SupplierChecker.SearchSupplierBackward(code, SupplierCodes);
                this.FindControl<DataGrid>("DgSuppliers").Items = suppliers.Select(v => new
                {
                    v.supplierFrom,
                    v.supplierTo,
                    v.count,
                    Group = MainSupplierWindowViewModel.Branches.FirstOrDefault(m => m.Code == v.supplierFrom?[..6])?.GroupName,
                    Text = v.count == 0 ? v.supplierFrom : $"{v.supplierFrom}-{v.supplierTo} ({v.count + 1} spaces)"
                }); 
                EditSupplierPage.SupplierCode = suppliers?.First().supplierFrom;
            }
            catch (Exception ex)
            {
                Functions.NotifyError(ex);
            }
        }

    }
}
