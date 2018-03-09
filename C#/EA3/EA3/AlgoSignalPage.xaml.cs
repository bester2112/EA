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
    public sealed partial class AlgoSignalPage : Page
    {
        public AlgoSignalPage()
        {
            this.InitializeComponent();

            #region UI Initialisierung 
            FirstLineText.Text = "Bitte bewerten Sie das Signal.";
            TextBlockFrage1.Text = "Was für einen Signaltypen haben Sie erkannt?";
            TextBlockFrage2.Text = "Wie gut haben Sie das Signal als X Signal erkannt?";
            TextBlockFrage3.Text = "Wie empfanden Sie die Stärke des Signals?";
            #endregion
        }
    }
}
