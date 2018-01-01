using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace EA3
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SignalTyp untypedSignal;
        MainProgram setup;

        public MainPage()
        {
            this.InitializeComponent();
            initCode();
            setup = new MainProgram();

            //textBlock.Text = setup.playSignalStart();

            
        }

        private void initCode()
        {
            untypedSignal = SignalTyp.NODATA;
        }

        private async void NextSignalButton_Click_1(object sender, RoutedEventArgs e)
        {
            /*MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream =
                await synth.SynthesizeTextToStreamAsync("Hello, World!");
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();*/

            // Falls nicht auf Next gedrückt worden ist, dann soll eine Fehler meldung erscheinen.
            if (untypedSignal == SignalTyp.NODATA) {
                var dialog = new MessageDialog("Sie haben Keine Eingabe getätigt \n" +
                    "Bitte waehlen Sie Ihre Auswahl und bestaetigen Sie Ihre Eingabe.");
                await dialog.ShowAsync();
            } else {
                setup.nextSignal(untypedSignal);
                switch (untypedSignal)
                {
                    case SignalTyp.KURZ:
                        radioButtonKurz.IsChecked = false; 
                    break;
                    case SignalTyp.MITTEL:
                        radioButtonMittel.IsChecked = false;
                    break;
                    case SignalTyp.LANG:
                        radioButtonLang.IsChecked = false;
                    break;
                    default:
                        Debug.WriteLine("ERROR in der  NextSignalButton_Click_1 Methode");
                    break;
                }

                NextSignalButton.IsEnabled = false;
                playSignalButton.IsEnabled = true;
                untypedSignal = SignalTyp.NODATA;
            }
        }

        private void playSignalButton_Click(object sender, RoutedEventArgs e)
        {
            if (setup.isStartElementAvailable())
            {
                String s = setup.playStartSignal();
                textBlock.Text = s;
                NextSignalButton.IsEnabled = true;
                playSignalButton.IsEnabled = false;
            } else {
                NextSignalButton.IsEnabled = true;
                // Eingabe des Benutzers ist fertig für die validierung der gleichverteilten Population. 
                // Es kann jetzt die Berechnung der Grenzen für Kurz, Mittel und Lang erfolgen.
                setup.calculateStartZones();
            }
        }

        private void radioButtonKurz_Checked(object sender, RoutedEventArgs e)
        {
            untypedSignal = SignalTyp.KURZ;
        }

        private void radioButtonMittel_Checked(object sender, RoutedEventArgs e)
        {
            untypedSignal = SignalTyp.MITTEL;
        }

        private void radioButtonLang_Checked(object sender, RoutedEventArgs e)
        {
            untypedSignal = SignalTyp.LANG;
        }
    }
}
