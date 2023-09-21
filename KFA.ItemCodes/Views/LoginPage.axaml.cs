using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using KFA.ItemCodes.Classes;
using ReactiveUI;

namespace KFA.ItemCodes.Views
{
	public partial class LoginPage : Window
	{
		public LoginPage()
		{
			InitializeComponent();
			DataContext = this;
			this.FindControl<Button>("BtnLogin").Command = ReactiveCommand.CreateFromTask(Login);

			this.FindControl<Button>("BtnClose").Click += (dd, ff) => this.Close();
		}

		private async Task Login()
		{
			await Task.Run(() => Functions.RunOnMain(() =>
			  {
				  try
				  {
					  var username = TxtUsername.Text?.ToLower();
					  var password = TxtPassword.Text;

					  if(DbService.Login(username, password))
					  {
						  Functions.Notify($"Successfully logged in");
						  App.MainWindow.Show();
						  this.Close();
					  }
					  else
					  {
						  ErrorFound(new Exception("Invalid user login credentials"));
					  }					 
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