using LocaTracker2.Db;
using LocaTracker2.Db.Objects;
using LocaTracker2.Exchange.Import;
using LocaTracker2.Logging;
using LocaTracker2.Logging.ETW;
using LocaTracker2.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LocaTracker2.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TripListPage : Page
    {
        public TripListPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += TripListPage_BackRequested;
        }

        private void TripListPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            SystemNavigationManager.GetForCurrentView().BackRequested -= TripListPage_BackRequested;
            e.Handled = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadTripList();
        }

        private void ReloadTripList()
        {
            using (var db = new LocaTrackerDbContext()) {
                TripListView.ItemsSource = db.Trips.ToList();
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.CreateTripDialog();
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary) {
                Trip trip = dialog.GetTripFromEntry();
                using (var db = new LocaTrackerDbContext()) {
                    db.Trips.Add(trip);
                    db.SaveChanges();
                }
                ReloadTripList();
            }
        }

        private void TripListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (TripListView.SelectedItem == null) { return; }
            Frame.Navigate(typeof(TripDetailPage), TripListView.SelectedItem);
        }

        private async void RunImporter(BaseImporter importer, StorageFile file)
        {
            LocaTrackerEventSource.Instance.Info($"Starting Import: {importer.GetType().Name}");
            var processingDialog = new ProcessingDialog();
            processingDialog.ShowAsync();
            await Task.Run(async () => {
                await importer.DoImport(file);

                string resultMessage;
                if (importer.Result == ImportResult.Success) {
                    resultMessage = "File Import completed";
                } else if (importer.ErrorCauseByException != null) {
                    resultMessage = LoggingUtilities.GetExceptionMessage(importer.ErrorCauseByException);
                } else {
                    resultMessage = "File Import failed!";
                }

                await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
#pragma warning disable CS4014
                    new MessageDialog(resultMessage, "Import").ShowAsync();
#pragma warning restore CS4014
                    processingDialog.Hide();
                    ReloadTripList();
                });
            });
        }

        private async Task<StorageFile> RequestImportFile(string title, string filterName, string filterExtension)
        {
            var openPicker = new FileOpenPicker() {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                CommitButtonText = title
            };
            openPicker.FileTypeFilter.Add(filterExtension);
            return await openPicker.PickSingleFileAsync();
        }

        private async void ImportFromLC1Button_Click(object sender, RoutedEventArgs e)
        {
            StorageFile importLC1File = await RequestImportFile("Import from LocaTracker 1", "CSV File", ".csv");
            if (importLC1File != null) {
                RunImporter(new LocaTracker1CsvImporter(), importLC1File);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), this);
        }
    }
}
