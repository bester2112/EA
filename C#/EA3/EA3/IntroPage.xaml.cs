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

            ButtonConfirm.Content = "Bestätigen";
            #endregion 

            user = new Person();
            allData = false;
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            bool sex = false;
            bool age = false;
            if ((Boolean) RadioButtonMale.IsChecked)
            {
                RadioButtonMale.IsChecked = false;
                user.setGender(Gender.man);
                sex = true;
            }
            else if ((Boolean) RadioButtonWoman.IsChecked) 
            {
                RadioButtonWoman.IsChecked = false;
                user.setGender(Gender.woman);
                sex = true;
            }
            else
            {
                var dialog = new MessageDialog("Bitte geben Sie auch Ihr Geschlecht an");
                await dialog.ShowAsync();
            }

            int userAge = Int32.Parse(TextBoxAge.Text);
            if (userAge > 0 && userAge <= 100)
            {
                age = true;
            } 
            else
            {
                var dialog = new MessageDialog("Bitte geben Sie auch Ihr Alter an");
                await dialog.ShowAsync();
            }

            if (age && sex)
            {
                allData = true;
            }

            if (allData)
            {
                var dialog = new MessageDialog("Danke Für Ihre Eingabe, Sie werden jetzt mit dem nächsten Schritt fortfahren");
                await dialog.ShowAsync();
                // TODO GO INTO NEXT STEP,
                // SAVE THE DATA in the MAIN 
                // 
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
