using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.ViewModels;
using LevenshteinDistanceAlgorithm;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace KFA.ItemCodes.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += (cc, tt) => DbService.Logout();
        }

		public static bool CanUpdateData { get; internal set; }
	}
}
