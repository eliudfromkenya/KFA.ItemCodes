using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KFA.ItemCodes.ViewModels;
using KFA.ItemCodes.Views;

namespace KFA.ItemCodes
{
    public partial class App : Application
    {
        public static HomePage MainWindow { get; set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow =  new HomePage();
                desktop.MainWindow = new LoginPage();
			}

            base.OnFrameworkInitializationCompleted();
        }
    }
}
