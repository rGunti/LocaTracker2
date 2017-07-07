using LocaTracker2.Logging;
using LocaTracker2.Logging.ETW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Settings.MaintenanceTask
{
    public enum MaintenanceTaskResult
    {
        Unknown = 0,

        Success = 1,
        Failed = 2
    }

    public abstract class BaseMaintenanceTask
    {
        public delegate void OnReportStatusDelegate(string status);
        public event OnReportStatusDelegate OnReportStatus;

        protected void ReportStatus(string status) { OnReportStatus?.Invoke(status); }

        public void Execute()
        {
            try {
                StorageFileLogger.Instance.I(this, $"Starting Maintenance Task {GetType().Name}...");
                DoJob();
            } catch (Exception ex) {
                StorageFileLogger.Instance.E(this, $"Error while executing Maint. Task {GetType().Name}. Detail:\n{LoggingUtilities.GetExceptionMessage(ex)}");
                SetTaskResult(MaintenanceTaskResult.Failed);
            }
        }

        protected abstract void DoJob();

        public MaintenanceTaskResult TaskResult { get; private set; } = MaintenanceTaskResult.Unknown;

        protected void SetTaskResult(MaintenanceTaskResult result) { this.TaskResult = result; }
    }
}
