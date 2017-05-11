using LocaTracker2.Db;
using LocaTracker2.Db.Objects;
using LocaTracker2.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class TripDetailPage : Page
    {
        private Trip editorTrip;

        public TripDetailPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += ListDetailPage_BackRequested;
        }

        private void ListDetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof(TripListPage));
            SystemNavigationManager.GetForCurrentView().BackRequested -= ListDetailPage_BackRequested;
            e.Handled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            editorTrip = e.Parameter as Trip;
            TripIDTextBox.Text = editorTrip.TripID.ToString();
            TripNameTextBox.Text = editorTrip.Name;
            TripDescriptionTextBox.Text = editorTrip.Description;
            
            Task.Run(async () => {
                IEnumerable<TripSection> sections = editorTrip.Sections;
                await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                    TripSectionList.ItemsSource = sections;
                });
            });
        }

        private void SaveTripButton_Click(object sender, RoutedEventArgs e)
        {
            string tripName = TripNameTextBox.Text.Trim();
            string tripDescription = TripDescriptionTextBox.Text.Trim();

            var processingDialog = new ProcessingDialog();
            processingDialog.ShowAsync();
            Task.Run(async () => {
                using (var db = new LocaTrackerDbContext()) {
                    Trip editorTrip = db.Trips.First(t => t.TripID == this.editorTrip.TripID);
                    editorTrip.Name = tripName;
                    editorTrip.Description = tripDescription;
                    db.SaveChanges();

                    await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                        processingDialog.Hide();
                        Frame.GoBack();
                    });
                }
            });
        }

        private async void DeleteTripButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (await new DeleteTripDialog(editorTrip).ShowAsync() == ContentDialogResult.Primary) {
                var processingDialog = new ProcessingDialog();
                processingDialog.ShowAsync();
                await Task.Run(async () => {
                    using (var db = new LocaTrackerDbContext()) {
                        db.Trips.Remove(editorTrip);
                        db.SaveChanges();
                    }

                    await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                        processingDialog.Hide();
                        Frame.GoBack();
                    });
                });
            }
        }
    }
}
