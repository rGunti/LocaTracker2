using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LocaTracker2.Logging
{
    public class StorageFileLogger : BaseLogger<StorageFileLogger>
    {
        private Queue<string> messageQueue = new Queue<string>();
        private StorageFile storageFile;
        private bool isRunning = false;

        protected override async void InitializeLogger()
        {
            storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                $"LocaTracker2_{DateTime.UtcNow:yyyyMMdd_HHmmss}Z.log", 
                CreationCollisionOption.OpenIfExists
            );
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => {
                I(this, $"Background Thread has been written...");
                isRunning = true;

                while (isRunning) {
                    List<string> lines = new List<string>();
                    while (messageQueue.Count > 0) {
                        WriteToFile(messageQueue.Dequeue());
                    }
                    await Task.Delay(1000);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async void WriteToFile(string line)
        {
            try {
                await FileIO.AppendTextAsync(storageFile, line + Environment.NewLine);
            } catch { }
        }

        protected override void ProcessEntry(string entry) {
            messageQueue.Enqueue(entry);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(entry);
#endif
        }

        public void Shutdown() { isRunning = false; }
    }
}
