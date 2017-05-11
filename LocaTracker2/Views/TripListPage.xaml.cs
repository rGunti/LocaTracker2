using LocaTracker2.Db;
using LocaTracker2.Db.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
            Frame.Navigate(typeof(TripDetailPage), TripListView.SelectedItem);
        }
    }
}
