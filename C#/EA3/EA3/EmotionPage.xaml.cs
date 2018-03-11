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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace EA3
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class EmotionPage : Page
    {
        private MainPage rootPage;
        private Emotion emote;
        
        public EmotionPage()
        {
            this.InitializeComponent();
            emote = Emotion.NA;

            #region UI Initialisierung 
            FirstLineText.Text = "Stimmung Bewerten";
            TextBlockFrage.Text = "Wie gelangweilt sind Sie ?";
            #endregion
        }
    
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }

        #region Smiley Buttons
        private void VeryMadButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButtonVeryMad.IsChecked = true;
            setEmote(Emotion.VERYMAD);
        }

        private void MadButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButtonMad.IsChecked = true;
            setEmote(Emotion.MAD);
        }

        private void NeutralButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButtonOK.IsChecked = true;
            setEmote(Emotion.OK);
        }

        private void GoodButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButtonGood.IsChecked = true;
            setEmote(Emotion.GOOD);

        }

        private void VeryGoodButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButtonVeryGood.IsChecked = true;
            setEmote(Emotion.VERYGOOD);
        }

        #endregion

        #region Smiley RadioButtons
        private void RadioButtonVeryMad_Clicked(object sender, RoutedEventArgs e)
        {
            setEmote(Emotion.VERYMAD);
        }
        private void RadioButtonMad_Clicked(object sender, RoutedEventArgs e)
        {
            setEmote(Emotion.MAD);
        }
        private void RadioButtonOK_Clicked(object sender, RoutedEventArgs e)
        {
            setEmote(Emotion.OK);
        }
        private void RadioButtonGood_Clicked(object sender, RoutedEventArgs e)
        {
            setEmote(Emotion.GOOD);
        }
        private void RadioButtonVeryGood_Clicked(object sender, RoutedEventArgs e)
        {
            setEmote(Emotion.VERYGOOD);
        }
        #endregion

        private void setEmote(Emotion emoji)
        {
            this.emote = emoji;
            rootPage.setEmotion(this.emote);
            // TODO in Reihenfolge aufufen
            int generation = rootPage.getGeneration();
            if (generation <= 4)
            {
                rootPage.changeToFrame(typeof(AlgoSignalPage));
                rootPage.saveAllData();
            }
            else
            {
                rootPage.changeToFrame(typeof(ErkennungPage));
            }
        }
    }
}
