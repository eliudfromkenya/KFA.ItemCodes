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

        internal Action<bool?> ReloadData { get; private set; }
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
				   DbService.Logout();
				   App.MainWindow.Close();
               }).DisposeWith(disposable);

			ViewModel.CanUpdate = Views.MainWindow.CanUpdateData;

			var searchCtrl = this.FindControl<AutoCompleteBox>("TxtSearch");
            var rbNormalSearch = this.FindControl<RadioButton>("rbNormalSearch");
            var rbAdvSearch = rbAdvancedSearch = this.FindControl<RadioButton>("rbAdvancedSearch");
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
                                if (MainSupplierWindowViewModel.Branches != null)
                                {
                                    branches = MainSupplierWindowViewModel.Branches
                                       .ToList();
                                }


                            if (txt?.Length > 3)
                            {
                                MainSupplierWindowViewModel.nextId = null;
                                var branchCode = txt?[..4];
                                var br = MainSupplierWindowViewModel.CurrentBranch = branches?.First(c => c?.Code == branchCode);
                                if (br != null)
                                {
                                    lblBranchName.Content = br?.BranchName;
                                    ReloadDataGrid(rbAdvancedSearch?.IsChecked ?? false);
                                    LoadNextId(br);
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
                        try
                        {
                            ReloadData(rbAdvancedSearch?.IsChecked);
                        }
                        catch { }
                    }));
            }

            void ReloadDataGrid(bool? advancedSearch = null)
            {
                try
                {
                    var text = searchCtrl?.Text ?? "";
                    //if (string.IsNullOrWhiteSpace(text))
                    //{
                    //    dgSuppliers.Items = ViewModel?.Models;
                    //    return;
                    //}

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


                    dgSuppliers.Items = string.IsNullOrWhiteSpace(text) ? models : SearchService.SearchSupplierCode(text, models, advancedSearch ?? false);
                }
                catch (Exception ex)
                {
                    Functions.NotifyError(ex);
                }
            }

            ReloadData = ReloadDataGrid;
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

        private void LoadNextId(Branch? br)
        {
            try
            {
                Functions.RunOnBackground(() =>
               {
                   var prefix = br?.Prefix;
                   if (prefix?.Length < 3)
                       return;
                   try
                   {
					   //                       var sql = $@"SELECT MAX(CAST(SUBSTR(code, 4) AS UNSIGNED))+1 num FROM
					   //(SELECT supplier_code code FROM tbl_suppliers UNION SELECT ledger_account_code code FROM tbl_ledger_accounts) A
					   //WHERE code LIKE '{prefix}%' AND code NOT LIKE '{prefix}7%' AND LENGTH(code) = 6 AND code != '{prefix}499'";

					   var sql = $@"DROP TABLE IF EXISTS tbl_supplier_codes;
DROP TABLE IF EXISTS tbl_temp_nums;
DROP TABLE IF EXISTS tbl_temp_generated_codes;

SET @prefix = '{prefix}';  
CREATE TEMPORARY TABLE tbl_supplier_codes 
(Digit int);

CREATE TABLE tbl_temp_nums 
(Digit int);

INSERT INTO tbl_temp_nums VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9);

-- SELECT * FROM tbl_temp_nums;

INSERT INTO tbl_supplier_codes (Digit) 
SELECT id
FROM
(
SELECT t4.digit * 1000 + t3.digit * 100 + t2.digit * 10 + t1.digit + 1 AS id
FROM tbl_temp_nums  AS t1 CROSS JOIN tbl_temp_nums AS t2
  CROSS JOIN tbl_temp_nums AS t3
  CROSS JOIN tbl_temp_nums AS t4
) t;


CREATE TEMPORARY TABLE tbl_temp_generated_codes AS SELECT CONCAT(@prefix, LPAD(Digit,3,'0')) code FROM tbl_supplier_codes WHERE Digit > 450 AND Digit <> 499 AND Digit NOT LIKE '7%';

SELECT code FROM tbl_temp_generated_codes WHERE code NOT IN (SELECT DISTINCT ledger_account_code FROM
(SELECT ledger_account_code FROM tbl_ledger_accounts
UNION SELECT ledger_account_id FROM tbl_ledger_accounts
UNION SELECT supplier_id FROM tbl_suppliers
UNION SELECT supplier_code FROM tbl_suppliers) A
WHERE ledger_account_code LIKE CONCAT(@prefix,'%')) LIMIT 1;

DROP TABLE IF EXISTS tbl_supplier_codes;
DROP TABLE IF EXISTS tbl_temp_nums;
DROP TABLE IF EXISTS tbl_temp_generated_codes;
";
                       var code = SupplierDbService.GetMySqlScalar(sql)?.ToString();
					   if (int.TryParse(code, out int mm) && mm > 0)
                       {
                           if (mm < 800 && mm > 699)
                               mm = 800;
                           MainSupplierWindowViewModel.nextId = $"{prefix}{mm.ToString("000")}";
                       }
                       else
                       {
						   MainSupplierWindowViewModel.nextId = code;
					   }

                   }
                   catch (Exception ex)
                   {
                       Functions.NotifyError(ex);
                   }
               });
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}