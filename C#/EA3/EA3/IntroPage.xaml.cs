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
    public sealed partial class IntroPage : Page
    {
        private MainPage rootPage;
        private Person user;
        private bool allData;
        public IntroPage()
        {
            this.InitializeComponent();
            

            #region UI Initialisierung 
            FirstLineText.Text = "Angaben zur Person";

            TextBlockMan.Text = "Männlich";
            TextBlockWoman.Text = "Weiblich";
            TextBlockSex.Text = "Geschlecht";
            TextBlockAge.Text = "Alter";
            TextBlockMusic.Text = "Empfinden Sie sich als musikalisch?";
            TextBlockMusicYes.Text = "Ja";
            TextBlockMusicNo.Text = "Nein";

            ButtonConfirm.Content = "Bestätigen";
            #endregion 

            user = new Person();
            allData = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            bool sex = false;
            bool age = false;
            bool musically = false;
            if ((Boolean) RadioButtonMale.IsChecked)
            {
                RadioButtonMale.IsChecked = false;
                user.setGender(Gender.MAN);
                sex = true;
            }
            else if ((Boolean) RadioButtonWoman.IsChecked) 
            {
                RadioButtonWoman.IsChecked = false;
                user.setGender(Gender.WOMAN);
                sex = true;
            }
            else if ((Boolean) RadioButtonNA.IsChecked)
            {
                RadioButtonNA.IsChecked = false;
                user.setGender(Gender.NOINPUT);
                sex = true;
            }

            int userAge = -1;
            string s = TextBoxAge.Text;
            if (s != "")
            {
                userAge = Int32.Parse(TextBoxAge.Text);
            }
            if (userAge > 0 && userAge <= 100)
            {
                user.setAge(userAge);
                age = true;
            }

            if ((Boolean) RadioButtonMusicYes.IsChecked)
            {
                RadioButtonMusicYes.IsChecked = false;
                musically = true;
                user.setMusically(musically);
            }
            else if ((Boolean) RadioButtonMusicNo.IsChecked)
            {
                RadioButtonMusicNo.IsChecked = false;
                musically = true;
                user.setMusically(!musically);
            }

            if (age && sex && musically)
            {
                allData = true;
            }
            else
            {
                musically = false;
                var dialog = new MessageDialog("Bitte beantworten Sie alle Fragen");
                await dialog.ShowAsync();
            }

            if (allData)
            {
                var dialog = new MessageDialog("Danke Für Ihre Eingabe, Sie werden jetzt mit dem nächsten Schritt fortfahren");
                await dialog.ShowAsync();

                // Daten werden in der rootpage gespeichert
                rootPage.setPerson(user);
                // Frame wird zu InitSignalPage gewechelt
                rootPage.changeToFrame(typeof(IntroPage2));
                
            }
        }

        private void TextBoxAge_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(e.Key.ToString(), "[0-9]"))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
