using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Windows.Storage;

namespace LocaTracker2.Logging.ETW
{
    public sealed class StorageFileEventListener : EventListener
    {
        private string lineFormat = "{0:yyyy-MM-dd HH\\:mm\\:ss\\:ffff}|{1}|{2}|{3}";

        public StorageFileEventListener(string name)
        {
            Name = name;
            AssignLocalFileAsync();
        }

        private Queue<string> buffer = new Queue<string>();
        private async void AssignLocalFileAsync()
        {
            storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{Name}.log", CreationCollisionOption.OpenIfExists);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => {
                LocaTrackerEventSource.Instance.Info($"Background Thread for Transfer to file in {Name} initialized");
                while (true) {
                    List<string> lines = new List<string>();
                    while (buffer.Count > 0) {
                        lines.Add(buffer.Dequeue());
                    }
                    if (lines.Count > 0) {
                        WriteToFile(lines);
                    }
                    await Task.Delay(5000);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public string Name { get; set; }

        private StorageFile storageFile;

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (storageFile == null) return;

            var lines = new List<String>();
            try {
                var newFormatedLine = string.Format(lineFormat, DateTime.Now, eventData.Level, eventData.EventId, eventData.Payload[0]);
                buffer.Enqueue(newFormatedLine);
                //Debug.WriteLine(newFormatedLine);
            } catch (NullReferenceException) { }
        }

        private async void WriteToFile(IEnumerable<string> lines)
        {
            await Task.Run(async () => {
                try {
                    await FileIO.AppendLinesAsync(storageFile, lines);
                } catch /*(Exception ex)*/ {
                //  // TODO: Catch Exception
                //} finally {
                }
            });
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Debug.WriteLine("OnEventSourceCreated for Listener {0} - {1} got eventSource {2}", GetHashCode(), "", eventSource.Name);
        }
    }
}
