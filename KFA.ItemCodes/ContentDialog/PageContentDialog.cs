using Aura.UI.Controls.Primitives;
using Aura.UI.Extensions;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using KFA.ItemCodes;
using System;

namespace Aura.UI.Controls
{
    public partial class PageContentDialog : ContentDialog
    {
        public bool OkButtonIsVisible { get; set; } = true;
        private Button OkButton;
        public PageContentDialog()
        {
           
        }
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            try
            {
                base.OnApplyTemplate(e);
                //  ExperimentalAcrylicMaterial()..BackgroundSource = AcrylicBackgroundSource.
                //.Property.Property.Property.MaterialOpacityProperty.Property.BackgroundSourceProperty
                OkButton = this.GetControl<Button>(e, "PART_OkButton");
                var bs = this.GetControl<Grid>(e, "PART_GridContainer");

                if (OkButtonIsVisible)
                {
                    OkButton.IsVisible = OkButtonIsVisible;
                    OkButton.Click += (s, e) =>
                    {
                        var x = new RoutedEventArgs(OkButtonClickEvent);
                        RaiseEvent(x);
                        x.Handled = true;
                    };
                }
                else
                {
                    OkButton.IsVisible = OkButtonIsVisible;
                }
            }
            catch (System.Exception ex)
            {
                Functions.NotifyError(ex);
            }
        }

    }
}