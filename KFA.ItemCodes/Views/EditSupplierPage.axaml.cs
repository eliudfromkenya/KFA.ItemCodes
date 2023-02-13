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
using System.Reactive.Linq;

namespace KFA.ItemCodes.Views
{
    public partial class EditSupplierPage :Window
    {
        internal Branch Branch;
        internal static bool isUpdate = false;
        static internal string SupplierCode, SupplierName;
        public AutoCompleteBox TxtBranches { get; }

        public EditSupplierPage()
        {
            InitializeComponent();
            TxtSupplierCode = this.FindControl<AutoCompleteBox>("TxtSupplierCode");
            TxtSupplierName = this.FindControl<AutoCompleteBox>("TxtSupplierName");
            TxtBranches = this.FindControl<AutoCompleteBox>("TxtBranchs");
            TxtTelephone = this.FindControl<AutoCompleteBox>("TxtTelephone");
            TxtEmail = this.FindControl<AutoCompleteBox>("TxtEmail");
            TxtAddress = this.FindControl<AutoCompleteBox>("TxtAddress");

            foreach (var txt in new[] {TxtSupplierCode,TxtSupplierName,TxtBranches,TxtTelephone/*TxtEmail,*//*TxtAddress*/})
            {
                txt.Events()
                   .TextChanged.Throttle(TimeSpan.FromMilliseconds(300))
                   .Subscribe(tt => Functions.RunOnMain(() => txt.Text = txt.Text?.ToUpper()));
            }

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

            TxtBranches = this.FindControl<AutoCompleteBox>("TxtBranchs");
            TxtBranches.Text = $"{Branch?.Code} - {Branch?.BranchName?.ToUpper()}";
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
            TxtAddress.Text = TxtBranches.Text = TxtEmail.Text = TxtSupplierCode.Text = TxtSupplierName.Text = TxtTelephone.Text = null;

            TxtSupplierCode.Text = MainSupplierWindowViewModel.nextId;
            TxtBranches.Text = MainSupplierWindowViewModel.CurrentBranch?.ToString();

            //this.FindControl<AutoCompleteBox>("TxtSupplierCode").Text =
            //this.FindControl<AutoCompleteBox>("TxtSupplierName").Text =
            //this.FindControl<AutoCompleteBox>("TxtBranchs").Text = null;
        }

        private async Task Save()
        {
            await Task.Run(() => Functions.RunOnMain(async () =>
              {
                  try
                  {
                      var supplierCode =TxtSupplierCode.Text?.ToUpper();
                      var supplierName = TxtSupplierName.Text?.ToUpper();
                      var telephone  = TxtTelephone.Text?.ToUpper();
                      var email = TxtEmail.Text?.ToUpper();
                      var address = TxtAddress.Text?.ToUpper();
                      var branch = TxtBranches.Text?.ToUpper();

                      if (string.IsNullOrWhiteSpace(supplierCode))
                          throw new Exception("Supplier code is required please");
                      if (string.IsNullOrWhiteSpace(supplierCode))
                          throw new Exception("Supplier code is required please");

                      if(branch?.Length < 4)
                          throw new Exception("branch code is required please");

                      var supplier = MainSupplierWindowViewModel.Branches
                          .FirstOrDefault(c => c.Code == branch?[..4]);

                      if (supplier == null)
                          throw new Exception("Branch is not valid");

                      if (!CustomValidations.IsValidSupplierCode(supplierCode))
                          throw new Exception("Supplier code is not valid");
                      if (!string.IsNullOrWhiteSpace(email) &&
                         !CustomValidations.IsValidEmail(email??""))
                          throw new Exception("email is not valid");
                      if (!string.IsNullOrWhiteSpace(telephone) &&
                         !CustomValidations.IsValidTelephone(telephone?.Replace("-","").Replace(" ","") ?? ""))
                          throw new Exception("Phone number is not valid");
                      if (!supplierCode.StartsWith(supplier?.Prefix??""))
                          throw new Exception($"Supplier code does not belong to {supplier?.BranchName}");
                      if (string.IsNullOrWhiteSpace(supplierName))
                          throw new Exception("Supplier name is required please");

                      await SupplierDbService.SaveSupplier(supplierCode, supplierName, telephone,email,address, supplier, isUpdate);

                      Functions.RunOnMain(async () =>
                                     {
                                         try
                                         {
                                            MainSupplierWindowViewModel.nextId = UpdateNextId();
                                             await Reset();
                                             this.Close();
                                         }
                                         catch (Exception)
                                         { }
                                     }, 500);
                      

                      if (isUpdate)
                      {
                          var supp = MainSupplierWindowViewModel.models.FirstOrDefault(c => c.Code == supplierCode);
                          if (supp != null)
                          {
                              supp.Code = supplierCode;
                              supp.Name = supplierName;
                              supp.OriginalName = supplierName;
                              supp.Telephone = telephone;
                              supp.Email = email;
                              supp.Address = address;
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
                     
                  }
                  catch (Exception ex)
                  {
                      ErrorFound(ex);
                  }
              }));
        }

        private string? UpdateNextId()
        {
            var nxt = MainSupplierWindowViewModel.nextId?[3..];
            var prefix = MainSupplierWindowViewModel.nextId?[..3];
            if (int.TryParse(nxt, out int val))
            {
                nxt = $"{prefix}{(++val):000}";
                if(val > 699 && val < 800)
                {
                     nxt = $"{prefix}{800}";
                }
               else if (val == 499)
                    nxt = $"{prefix}{500}";
            }
            return nxt;
        }

        private void ErrorFound(Exception ex)
        {
            Functions.RunOnMain(() => this.FindControl<TextBlock>("TxbError").Text = ex.Message);
        }
    }
}
