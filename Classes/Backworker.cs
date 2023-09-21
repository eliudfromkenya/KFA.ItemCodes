#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace KFA.ItemCodes.Classes
{
    public class BackgroundWorkHelper
    {
        readonly ValueMonitor<int> percentageProgress = new ValueMonitor<int>(0);
        readonly ValueMonitor<TimeSpan> timeLeft = new ValueMonitor<TimeSpan>(TimeSpan.MaxValue);
        DateTime startTime;
        List<Action> toDo;
        BackgroundWorker worker;

        public BackgroundWorkHelper()
        {
            IsParallel = false;
            BackgroundWorker.WorkerReportsProgress = true;
            BackgroundWorker.WorkerSupportsCancellation = true;
            percentageProgress.ValueChanged += percentageProgress_ValueChanged;

            BackgroundWorker.DoWork += worker_DoWork;
        }

        public BackgroundWorkHelper(List<Action> actionsToDo)
            : this()
        {
            toDo = actionsToDo;
        }

        public BackgroundWorker BackgroundWorker => worker ?? (worker = new BackgroundWorker());

        public bool IsParallel { get; set; }

        public IValueMonitor<TimeSpan> TimeLeft => timeLeft;

        public int Total => toDo == null ? 0 : toDo.Count;

        public void SetActionsTodo(List<Action> toDoActions, bool cancelCurrent = false)
        {
            if (BackgroundWorker.IsBusy && cancelCurrent)
                BackgroundWorker.CancelAsync();

            BackgroundWorker.DoWork -= worker_DoWork;
            BackgroundWorker.DoWork += worker_DoWork;
            BackgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            toDo = toDoActions;
        }

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ((BackgroundWorker)sender).Dispose();
        }

        void percentageProgress_ValueChanged(int oldValue, int newValue)
        {
            BackgroundWorker.ReportProgress(newValue);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (toDo == null) throw new InvalidOperationException("You must provide actions to execute");
            Thread.Sleep(10);
            var total = toDo.Count;
            startTime = DateTime.Now;
            var current = 0;

            if (IsParallel == false)
                foreach (var next in toDo)
                {
                    next();
                    current++;
                    if (worker.CancellationPending) return;
                    percentageProgress.Value = (int)(current / (double)total * 100.0);
                    var passedMs = (DateTime.Now - startTime).TotalMilliseconds;
                    var oneUnitMs = passedMs / current;
                    var leftMs = (total - current) * oneUnitMs;
                    timeLeft.Value = TimeSpan.FromMilliseconds(leftMs);
                }
            else
                try
                {
                    Parallel.For(0, total,
                        (index, loopstate) =>
                        {
                            toDo.ElementAt(index)();
                            if (worker.CancellationPending) loopstate.Stop();
                            Interlocked.Increment(ref current);

                            percentageProgress.Value = (int)(current / (double)total * 100.0);
                            var passedMs = (DateTime.Now - startTime).TotalMilliseconds;
                            var oneUnitMs = passedMs / current;
                            var leftMs = (total - current) * oneUnitMs;
                            timeLeft.Value = TimeSpan.FromMilliseconds(leftMs);
                        });
                }
                catch (Exception ex)
                {
                    Functions.NotifyError("Background Action Error", ex.Message, ex);
                }
        }
    }
}