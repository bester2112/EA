using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace EA3
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class IntroPage2 : Page
    {
        private MainPage rootPage;
        private bool allData;
        private bool usedTactil;
        private bool usedWatch;
        private bool playedGames;
        public IntroPage2()
        {
            this.InitializeComponent();


            #region UI Initialisierung 
            FirstLineText.Text = "Allgemeine Fragen";

            TextBlockGames.Text = "Spielen Sie gelegendlich Spiele?";
            TextBlockGamesYes.Text = "Ja";
            TextBlockGamesNo.Text = "Nein";
            TextBlockTaktil.Text = "Haben Sie Erfahrungen mit Taktilen Geräten ?";
            TextBlockTaktilYes.Text = "Ja";
            TextBlockTaktilNo.Text = "Nein";
            TextBlockClock.Text = "Haben Sie schon einmal eine Pulsuhr / Smartwatch verwendet ?";
            TextBlockClockYes.Text = "Ja";
            TextBlockClockNo.Text = "Nein";

            ButtonConfirm.Content = "Bestätigen";
            #endregion
            
            allData = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            bool games = false;
            bool tactil = false;
            bool watch = false;
            if ((Boolean)RadioButtonGamesYes.IsChecked)
            {
                RadioButtonGamesYes.IsChecked = false;
                playedGames = true;
                games = true;
            }
            else if ((Boolean)RadioButtonGamesNo.IsChecked)
            {
                RadioButtonGamesNo.IsChecked = false;
                playedGames = false;
                games = true;
            }

            if ((Boolean)RadioButtonClockYes.IsChecked)
            {
                RadioButtonClockYes.IsChecked = false;
                usedWatch = true;
                watch = true;
            }
            else if ((Boolean)RadioButtonClockNo.IsChecked)
            {
                RadioButtonClockYes.IsChecked = false;
                usedWatch = false;
                watch = true;
            }

            if ((Boolean)RadioButtonTaktilYes.IsChecked)
            {
                RadioButtonClockYes.IsChecked = false;
                usedTactil = true;
                tactil = true;
            }
            else if ((Boolean)RadioButtonTaktilNo.IsChecked)
            {
                RadioButtonClockYes.IsChecked = false;
                usedTactil = false;
                tactil = true;
            }

            if (tactil && watch && games)
            {
                allData = true;
            }
            else
            {
                var dialog = new MessageDialog("Bitte beantworten Sie alle Fragen");
                await dialog.ShowAsync();
            }

            if (allData)
            {
                var dialog = new MessageDialog("Danke Für Ihre Eingabe, Sie werden jetzt mit dem nächsten Schritt fortfahren");
                await dialog.ShowAsync();

                // Daten werden in der rootpage gespeichert
                rootPage.setOtherPersonValues(this.usedTactil, this.usedWatch, this.playedGames);
                // Frame wird zu InitSignalPage gewechelt
                rootPage.changeToFrame(typeof(InitSignalPage));

            }
        }
    }
}
