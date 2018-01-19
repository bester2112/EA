using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace WriteFileTest
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        
        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            test();
        }

        public async void test()
        {
            //Uri myUri = new Uri("ms-appx:///file.txt");
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(myUri);

            StorageFolder storageFolder2 = KnownFolders.PicturesLibrary;
            StorageFile file = await storageFolder2.CreateFileAsync("sample.png", CreationCollisionOption.ReplaceExisting);


            StorageFolder storageFolder = await KnownFolders.GetFolderForUserAsync(null /* current user*/, KnownFolderId.PicturesLibrary);
            const string filename = "SAMPLE.dat";
            StorageFile sampleFile = null;

            try
            {
                sampleFile = await storageFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                var dialog = new MessageDialog(String.Format("The file '{0} was created.", sampleFile.Name));
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                // I/O errors are reported as exceptions.
                var dialog = new MessageDialog(String.Format("Error creating the file {0}: {1}", filename, ex.Message));
                dialog.ShowAsync();
            }
        }

    }
}
