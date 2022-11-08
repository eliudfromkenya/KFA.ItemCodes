using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LevenshteinDistanceAlgorithm;

namespace KFA.ItemCodes.Views
{
    public partial class SearchItemsPage : Window
    {
        internal List<ItemCode> ItemCodes { get; set; }
        public SearchItemsPage()
        {
            InitializeComponent();
            DataContext = this;
            this.FindControl<Button>("BtnSearchBackwards").Click += (xx,yy) => Search_Forward();
            this.FindControl<Button>("BtnSearchForwards").Click += (xx,yy) => Search_Backward();
            this.FindControl<Button>("BtnClose").Click += (xx,yy) => this.Close();
        }

        private void Search_Backward()
        {
            try
            {
                var code = this.FindControl<AutoCompleteBox>("TxtSearch").Text;
                var items = ItemChecker.SearchItemForward(code, ItemCodes);
                this.FindControl<DataGrid>("DgItems").Items = items.Select(v => new
                {
                    v.itemFrom,
                    v.itemTo,
                    v.count,
                    Text = v.count == 0 ? v.itemFrom : $"{v.itemFrom}-{v.itemTo} ({v.count + 1} spaces)"
                }); ;
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
                    Text = v.count == 0 ? v.itemFrom : $"{v.itemFrom}-{v.itemTo} ({v.count + 1} spaces)"
                }); ;
            }
            catch (Exception ex)
            {
                Functions.NotifyError(ex);
            }
        }

    }
}
