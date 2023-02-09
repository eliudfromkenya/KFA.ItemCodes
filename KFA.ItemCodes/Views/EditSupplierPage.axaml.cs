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
using KFA.ItemCodes.LevenshteinDistanceAlgorithm;

namespace KFA.ItemCodes.Views
{
    public partial class EditSupplierPage :Window
    {
        internal Branch Branch;
        internal static bool isUpdate = false;
        static internal string SupplierCode, SupplierName;

          public EditSupplierPage()
        {
            InitializeComponent();
            DataContext = this;
             this.FindControl<Button>("BtnSave").Command = ReactiveCommand.CreateFromTask(Save);
             this.FindControl<Button>("BtnDelete").Command  = ReactiveCommand.CreateFromTask(Delete) ;
             this.FindControl<Button>("BtnReset").Command = ReactiveCommand.CreateFromTask(Reset);
            this.FindControl<Button>("BtnClose").Click += (dd, ff) => this.Close();

            this.FindControl<TextBlock>("TxbHeader").Text = isUpdate ? "UPDATING SUPPLIER" : "ADDING SUPPLIER";

            var txtCode = this.FindControl<AutoCompleteBox>("TxtSupplierCode");
            var txtGroup = this.FindControl<TextBlock>("TxbBranch");
            txtCode.Text = SupplierCode;
            this.FindControl<AutoCompleteBox>("TxtSupplierName").Text = SupplierName?.ToUpper();
            this.FindControl<AutoCompleteBox>("TxtBranch").Text = $"{Branch?.Code} - {Branch?.BranchName?.ToUpper()}";
            txtCode.TextChanged += (vv, yy) =>
            {
                try
                {
                    txtGroup.Text = MainSupplierWindowViewModel.Branches.FirstOrDefault(m => m.Prefix == txtCode.Text?[..3])?.Code;
                }
                catch { }
            };
            if (isUpdate)
                this.FindControl<AutoCompleteBox>("TxtSupplierCode").Focusable = false;
            
        }
        

        private async Task Delete()
        {
            try
            {
               // using var con = 
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task Reset()
        {
            this.FindControl<AutoCompleteBox>("TxtSupplierCode").Text =
            this.FindControl<AutoCompleteBox>("TxtSupplierName").Text =
            this.FindControl<AutoCompleteBox>("TxtBranch").Text = null;
        }

        private async Task Save()
        {
            await Task.Run(() => Functions.RunOnMain(async () =>
              {
                  try
                  {
                      var supplierCode = this.FindControl<AutoCompleteBox>("TxtSupplierCode").Text?.ToUpper();
                      var supplierName = this.FindControl<AutoCompleteBox>("TxtSupplierName").Text?.ToUpper();
                        var telephone = this.FindControl<AutoCompleteBox>("TxtTelephone").Text?.ToUpper();
                        var email = this.FindControl<AutoCompleteBox>("TxtEmail").Text?.ToUpper();
                        var address = this.FindControl<AutoCompleteBox>("TxtAddress").Text?.ToUpper();
                      var branch = this.FindControl<AutoCompleteBox>("TxtBranch").Text?.ToUpper();

                      var supplier = MainSupplierWindowViewModel.Branches.FirstOrDefault(c => c.Code == branch?[..4]);

                      await SupplierDbService.SaveSupplier(supplierCode, supplierName, telephone,email,address, supplier, isUpdate);
                      if(isUpdate)
                      {
                          var supp = MainSupplierWindowViewModel.models.FirstOrDefault(c => c.Code == supplierCode);
                          if (supplier != null)
                          {
                              supp.OriginalName = supplierName;
                              supp.Branch = supplier;
                          }
                          Functions.Notify($"Successfully updated supplier {supplierCode} - {SupplierName}");
                      }
                      else
                      {
                          List<SupplierCode> suppliers = new() 
                          {
                              new SupplierCode 
                              { 
                                  Code = supplierCode, 
                                  Name = supplierName, 
                                  OriginalName = supplierName, 
                                  Telephone=telephone,
                                  Email=email,
                                  Address=address,
                                  Branch = supplier 
                              }
                          };
                          //Matcher.CheckSupplierCodes(ref suppliers);
                          MainSupplierWindowViewModel.models.AddRange(suppliers);
                            Functions.Notify($"Successfully added supplier {supplierCode} - {supplierName}");
                      }

                      this.Close();
                  }
                  catch (Exception ex)
                  {
                      ErrorFound(ex);
                  }
              }));
        }

        private void ErrorFound(Exception ex)
        {
            Functions.RunOnMain(() => this.FindControl<TextBlock>("TxbError").Text = ex.Message);
        }
    }
}
