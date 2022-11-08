using System;
using System.Windows.Input;
using Avalonia.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Windows.Input;
using ReactiveUI;
using System.Threading.Tasks;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.ViewModels;
using System.Linq;
using AvaloniaEdit.Utils;
using LevenshteinDistanceAlgorithm;
using System.Collections.Generic;

namespace KFA.ItemCodes.Views
{
    public partial class EditItemPage :Window
    {
        internal string Supplier;
        internal bool isUpdate = false;
        static internal string ItemCode, ItemName;

          public EditItemPage()
        {
            InitializeComponent();
            DataContext = this;
             this.FindControl<Button>("BtnSave").Command = ReactiveCommand.CreateFromTask(Save);
             this.FindControl<Button>("BtnDelete").Command  = ReactiveCommand.CreateFromTask(Delete) ;
             this.FindControl<Button>("BtnReset").Command = ReactiveCommand.CreateFromTask(Reset);
            this.FindControl<Button>("BtnClose").Click += (dd, ff) => this.Close();

            this.FindControl<TextBlock>("TxbHeader").Text = isUpdate ? "UPDATING ITEM" : "ADDING ITEM";
            this.FindControl<AutoCompleteBox>("TxtItemCode").Text = ItemCode;
            this.FindControl<AutoCompleteBox>("TxtItemName").Text = ItemName?.ToUpper();
            this.FindControl<AutoCompleteBox>("TxtItemSupplier").Text = Supplier?.ToUpper();

            if (isUpdate)
                this.FindControl<AutoCompleteBox>("TxtItemCode").Focusable = false;
            
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

                      await DbService.SaveItem(itemCode, itemName, supplier, isUpdate);
                      if(isUpdate)
                      {
                          var item = MainWindowViewModel.models.FirstOrDefault(c => c.Code == itemCode);
                          if (item != null)
                          {
                              item.OriginalName = itemName;
                              item.Distributor = supplier;
                          }
                          Functions.Notify($"Successfully updated item {itemCode} - {ItemName}");
                      }
                      else
                      {
                          List<ItemCode> items = new() { new ItemCode { Code = itemCode, Name = itemName, OriginalName = itemName, Distributor = supplier } };
                           Matcher.CheckCodes(ref items);
                          MainWindowViewModel.models.AddRange(items);
                            Functions.Notify($"Successfully added item {itemCode} - {ItemName}");
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
