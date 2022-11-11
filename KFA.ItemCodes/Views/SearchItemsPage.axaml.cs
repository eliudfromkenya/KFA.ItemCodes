using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KFA.ItemCodes.ViewModels;
using LevenshteinDistanceAlgorithm;

namespace KFA.ItemCodes.Views
{
    public partial class SearchItemsPage : Window
    {
        internal List<ItemCode> ItemCodes { get; set; }
        public string? SearchBasedName { get; internal set; }
        public string? SearchBasedItemCode { get; internal set; }

        public SearchItemsPage()
        {
            InitializeComponent();
            DataContext = this;
            this.FindControl<Button>("BtnSearchBackwards").Click += (xx,yy) => Search_Forward();
            this.FindControl<Button>("BtnSearchForwards").Click += (xx,yy) => Search_Backward();
            this.FindControl<Button>("BtnAddItem").Click += (xx,yy) => {
                try
                {
                    EditItemPage.ItemName = SearchBasedName;

                    EditItemPage.isUpdate = false;
                    var page = new EditItemPage
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
            var dgItems = this.FindControl<DataGrid>("DgItems");
            dgItems.SelectionChanged += (xx, yy) =>
            {
                try
                {
                    if (dgItems.SelectedItem != null)
                        EditItemPage.ItemCode =
                           ((dynamic)dgItems.SelectedItem)
                           .itemFrom;
                }
                catch { }
            };

            Functions.RunOnBackground(() =>
            {
                try
                {
                    if(CustomValidations.IsValidItemCode(SearchBasedItemCode ?? " "))
                    {
                        Functions.RunOnMain(() => this.FindControl<AutoCompleteBox>("TxtSearch").Text = SearchBasedItemCode);
                    }
                    else if (!string.IsNullOrEmpty(SearchBasedName))
                    {
                        var code = ItemChecker
                        .SearchItemByName(SearchBasedName, ItemCodes)?
                        .FirstOrDefault()?
                        .Code;

                        if (CustomValidations.IsValidItemCode(code ?? ""))
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
                var dgItems = this.FindControl<DataGrid>("DgItems");
                var code = this.FindControl<AutoCompleteBox>("TxtSearch").Text;
                var items = ItemChecker.SearchItemForward(code, ItemCodes);
                dgItems.Items = items.Select(v => new
                {
                    v.itemFrom,
                    v.itemTo,
                    v.count,
                    Group = MainWindowViewModel.itemGroups.FirstOrDefault(m => m.GroupId == v.itemFrom?[..2])?.GroupName,
                    Text = v.count == 0 ? v.itemFrom : $"{v.itemFrom}-{v.itemTo} ({v.count + 1} spaces)"
                });
                EditItemPage.ItemCode = items?.First().itemFrom;
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
                var items = ItemChecker.SearchItemBackward(code, ItemCodes);
                this.FindControl<DataGrid>("DgItems").Items = items.Select(v => new
                {
                    v.itemFrom,
                    v.itemTo,
                    v.count,
                    Group = MainWindowViewModel.itemGroups.FirstOrDefault(m => m.GroupId == v.itemFrom?[..2])?.GroupName,
                    Text = v.count == 0 ? v.itemFrom : $"{v.itemFrom}-{v.itemTo} ({v.count + 1} spaces)"
                }); 
                EditItemPage.ItemCode = items?.First().itemFrom;
            }
            catch (Exception ex)
            {
                Functions.NotifyError(ex);
            }
        }

    }
}
