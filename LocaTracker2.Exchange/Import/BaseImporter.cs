using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LocaTracker2.Exchange.Import
{
    public enum ImportResult
    {
        NotExecuted = 0,
        Success = 1,
        Failed = 2
    }

    public abstract class BaseImporter
    {
        public abstract Task DoImport(StorageFile file);

        public ImportResult Result { get; protected set; } = ImportResult.NotExecuted;
        public Exception ErrorCauseByException { get; protected set; }
    }
}
