using System;
using System.Windows.Input;
using Avalonia.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Windows.Input;
using ReactiveUI;
using System.Threading.Tasks;
using KFA.ItemCodes.Classes;

namespace KFA.ItemCodes.Views
{
    public partial class EditItemPage :Window
    {
        internal string ItemCode, ItemName, Supplier;
        internal bool isUpdate = false;

          public EditItemPage()
        {
            InitializeComponent();
            DataContext = this;
             this.FindControl<Button>("BtnSave").Command = ReactiveCommand.CreateFromTask(Save);
             this.FindControl<Button>("BtnDelete").Command  = ReactiveCommand.CreateFromTask(Delete) ;
             this.FindControl<Button>("BtnReset").Command = ReactiveCommand.CreateFromTask(Reset);
            this.FindControl<Button>("BtnClose").Click += (dd, ff) => this.Close();

            this.FindControl<TextBlock>("TxbHeader").Text = "UPDATING ITEM";
            this.FindControl<AutoCompleteBox>("TxtItemCode").Text = ItemCode;
            this.FindControl<AutoCompleteBox>("TxtItemName").Text = ItemName;
            this.FindControl<AutoCompleteBox>("TxtItemSupplier").Text = Supplier;
            
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
            this.FindControl<AutoCompleteBox>("TxtItemCode").Text =
            this.FindControl<AutoCompleteBox>("TxtItemName").Text =
            this.FindControl<AutoCompleteBox>("TxtItemSupplier").Text = null;
        }

        private async Task Save()
        {
            await Task.Run(() => Functions.RunOnMain(async () =>
              {
                  try
                  {
                      var itemCode = this.FindControl<AutoCompleteBox>("TxtItemCode").Text?.ToUpper();
                      var itemName = this.FindControl<AutoCompleteBox>("TxtItemName").Text?.ToUpper();
                      var supplier = this.FindControl<AutoCompleteBox>("TxtItemSupplier").Text?.ToUpper();

                      await DbService.SaveItem(itemCode, itemName, supplier);
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
