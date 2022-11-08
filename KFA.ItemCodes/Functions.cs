using Aura.UI.Controls;
using Aura.UI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using KFA.ItemCodes.Classes;
using KFA.ItemCodes.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KFA.ItemCodes
{
    internal static class Functions
    {
        static IDbDataAdapter GetAdapter(IDbConnection connection)
        {
            var assembly = connection.GetType().Assembly;
            var @namespace = connection.GetType().Namespace;

            // Assumes the factory is in the same namespace
            var factoryType = assembly.GetTypes()
                                .Where(x => x.Namespace == @namespace)
                                .Where(x => x.IsSubclassOf(typeof(DbProviderFactory)))
                                .Single();

            // SqlClientFactory and OleDbFactory both have an Instance field.
            var instanceFieldInfo = factoryType.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
            var factory = (DbProviderFactory)instanceFieldInfo.GetValue(null);

            return factory.CreateDataAdapter();
        }

        public static DataSet GetDbDataSet(IDbConnection con, string sql)
        {
            using var cmd = con.CreateCommand();
            if (con.State != ConnectionState.Open)
                con.Open();

            var ds = new DataSet();
            var adapter = GetAdapter(con);
            IDbCommand dbCommand = con.CreateCommand();
            dbCommand.CommandText = sql;
            dbCommand.CommandType = CommandType.Text;
            adapter.SelectCommand = dbCommand;
            adapter.Fill(ds);
            return ds;
        }

        public static DataSet GetDbDataSet(string sql, IDbConnection con)
        {
            using var cmd = con.CreateCommand();
            if (con.State != ConnectionState.Open)
                con.Open();

            var ds = new DataSet();
            var adapter = GetAdapter(con);
            IDbCommand dbCommand = con.CreateCommand();
            dbCommand.CommandText = sql;
            dbCommand.CommandType = CommandType.Text;
            adapter.SelectCommand = dbCommand;
            adapter.Fill(ds);
            return ds;
        }

        public static void RunOnBackground(Action action, int sleepTime = 0)
        {
            try
            {
                RunOnBackground(out BackgroundWorker worker, action,sleepTime);
            }
            catch (Exception ex)
            {
                NotifyError(ex.Message, "Unhandled Background Error", ex);
            }
        }

        internal static void NotifyError(Exception ex)
        {
            MainWindowViewModel.ErrorNotifications.OnNext(("Error", ex.Message, ex));
        }

        internal static void NotifyError(string message, string v, Exception ex)
        {
            MainWindowViewModel.ErrorNotifications.OnNext((v, message, ex));
        }

        public static void NewContentDialog<TContentDialog>(this WindowBase owner,
                                       object content,
                                       Action<object, RoutedEventArgs>? OnOKButtonClick,
                                       Action<object, RoutedEventArgs>? OnCancelButtonClick,
                                       object? OkButtonContent,
                                       object? CancelButtonContent) where TContentDialog : ContentDialog, new()
        {
            var dialog = new TContentDialog();
            dialog.FontSize = 32;
            dialog.Width = 900;// Declarations.AppWidth.Value;
            dialog.Content = content;
            dialog.SetOwner(owner);

            // returns Ok when OkButton content is false or returns the custom content
            dialog.OkButtonContent = OkButtonContent == null ? "Ok" : OkButtonContent;

            // the same but with CancelButton content
            dialog.CancelButtonContent = CancelButtonContent == null ? "Cancel" : CancelButtonContent;

            // sets the actions and the closing
            dialog.OkButtonClick += (s, e) =>
            {
                if (OnOKButtonClick != null)
                {
                    OnOKButtonClick.Invoke(s, e);
                }
                dialog.Close();
            };
            dialog.CancelButtonClick += (s, e) =>
            {
                if (OnCancelButtonClick != null)
                {
                    OnCancelButtonClick.Invoke(s, e);
                }
                dialog.Close();
            };
            dialog.InvalidateVisual();
            dialog.Show();
        }

        static object lockObject = new object();
        public static void RunOnBackground(out BackgroundWorker worker, Action action, int sleepTime = 0)
        {
            lock (lockObject)
            {
                try
                {
                    var action1 = action;

                    action = () =>
                    {
                        try
                        {
                            if(sleepTime > 0)
                                Thread.Sleep(sleepTime);    
                            action1();
                        }
                        catch (Exception ex)
                        {
                            NotifyError(ex.Message, "Unhandled Background Error", ex);
                        }
                    };

                    var helper = new BackgroundWorkHelper();
                    worker = helper.BackgroundWorker;
                    var actions = new List<Action> { action };
                    helper.SetActionsTodo(actions);
                    helper.IsParallel = true;

                    if (helper.BackgroundWorker.IsBusy)
                        helper.SetActionsTodo(actions);
                    else
                        helper.BackgroundWorker.RunWorkerAsync();
                }
                catch (Exception e)
                {
                    NotifyError(e.Message, "Unhandled Background Error", e);
                    worker = null;
                }
            }
        }


        public static void RunOnMain(Action action, int sleepTime = 0, Dispatcher dispatcher = null)
        {
            RunOnBackground(async () =>
            {
                try
                {
                    void ax()
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            NotifyError(ex.Message, "Unhandled Background Error", ex);
                        }
                    }

                    if (sleepTime > 0) Thread.Sleep(sleepTime);

                    await (dispatcher ?? Dispatcher.UIThread)
                    .InvokeAsync(ax, DispatcherPriority.ApplicationIdle)
                    .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    NotifyError(ex.Message, "Unhandled Background Error", ex);
                }
            });
        }

        internal static void Notify(string message, string title = "Codes")
        {
            MainWindowViewModel.Notifications.OnNext((title, message));
        }
    }
}
