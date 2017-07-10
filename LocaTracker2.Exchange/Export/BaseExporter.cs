using LocaTracker2.Db.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LocaTracker2.Exchange.Export
{
    public enum ExportResult
    {
        NotExecuted = 0,
        Success = 1,
        Failed = 2
    }

    public abstract class BaseExporter
    {
        public abstract Task DoExport(StorageFile file, Trip trip);

        public ExportResult Result { get; protected set; } = ExportResult.NotExecuted;
        public Exception ErrorCauseByException { get; protected set; }
    }
}
