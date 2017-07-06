using LocaTracker2.Db;
using LocaTracker2.Db.Objects;
using LocaTracker2.Settings;
using LocaTracker2.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
        static Brush controlBrush = new SolidColorBrush(Color.FromArgb(255, 0, 160, 0));

        private Trip editorTrip;

        public TripDetailPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += ListDetailPage_BackRequested;
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            await Task.Run(() => {

            });
        }

        private void ListDetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof(TripListPage));
            SystemNavigationManager.GetForCurrentView().BackRequested -= ListDetailPage_BackRequested;
            e.Handled = true;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            editorTrip = e.Parameter as Trip;
            this.DataContext = editorTrip;
            //TripIDTextBox.Text = editorTrip.TripID.ToString();
            //TripNameTextBox.Text = editorTrip.Name;
            //TripDescriptionTextBox.Text = editorTrip.Description;
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                SelectRecordingTripButton.Background = (RecordingSettingsReader.Instance.RecordingTripID == editorTrip.TripID ? controlBrush : null);
            });

            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await Task.Run(async () => {
                await Task.Delay(150);
                using (var db = LocaTrackerDbContext.GetNonTrackingInstance()) {
                    IEnumerable<TripSection> sections = db.TripSections.Where(s => s.TripID == editorTrip.TripID).ToList();
                    foreach (TripSection section in sections.Where(s => s.IsActive)) {
                        section.CalculateSectionDistance();
                    }
                    await Task.Delay(150);
                    await dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                        LoadingProgressBar.Visibility = Visibility.Collapsed;
                        TripSectionList.ItemsSource = sections;
                    });
                }
            });
        }

        private void SaveTripButton_Click(object sender, RoutedEventArgs e)
        {
            var processingDialog = new ProcessingDialog();
            processingDialog.ShowAsync();
            Task.Run(async () => {
                using (var db = LocaTrackerDbContext.GetNonTrackingInstance()) {
                    Trip editorTrip = db.Trips.First(t => t.TripID == this.editorTrip.TripID);
                    editorTrip.Name = this.editorTrip.Name;
                    editorTrip.Description = this.editorTrip.Description;

                    db.Update(editorTrip);
                    db.SaveChanges();

                    await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                        processingDialog.Hide();
                        Frame.GoBack();
                    });
                }
            });
        }

        private async void DeleteTripButton_Click(object sender, RoutedEventArgs e)
        {
            if (await new DeleteTripDialog(editorTrip).ShowAsync() == ContentDialogResult.Primary) {
                var processingDialog = new ProcessingDialog();
                processingDialog.ShowAsync();
                await Task.Run(async () => {
                    using (var db = new LocaTrackerDbContext()) {
                        db.Remove(editorTrip);
                        db.SaveChanges();
                    }

                    await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                        processingDialog.Hide();
                        Frame.GoBack();
                    });
                });
            }
        }

        private void TripPropertyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            editorTrip.Set(textBox.Tag.ToString(), textBox.Text);
        }

        private void SelectRecordingTripButton_Click(object sender, RoutedEventArgs e)
        {
            RecordingSettingsReader.Instance.RecordingTripID = editorTrip.TripID;
            Frame.Navigate(typeof(TripListPage));
        }
    }
}
