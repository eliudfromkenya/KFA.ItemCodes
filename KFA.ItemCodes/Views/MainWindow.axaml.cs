using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.ViewModels;
using LevenshteinDistanceAlgorithm;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace KFA.ItemCodes.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += FormClosing;
        }

		private void FormClosing(object? sender, CancelEventArgs e)
		{
			try
			{
				e.Cancel = true;
				DbService.Logout();
				Environment.Exit(0);
			}
			catch { }
			Environment.Exit(0);
		}

		public static bool CanUpdateData { get; internal set; }
	}
}
