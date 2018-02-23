using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
using Windows.Storage;
using System.Diagnostics;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

#region LINKS

// Erstellen, schreiben und lesen einer Datei
// https://docs.microsoft.com/de-de/windows/uwp/files/quickstart-reading-and-writing-files


#endregion LINKS

namespace BLE_2
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class FilePage : Page
    {
        bool fileExists;
        private static Random random = new Random();
        private static readonly string FILENAME = "UserStudy-";
        private static readonly string FILETYPE = ".txt";
        private string FilePath = "";
        StorageFolder storageFolder;
        bool inFunction;


        public FilePage()
        {
            this.InitializeComponent();
            fileExists = false;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }


        /// <summary>
        /// Überprüft ob eine Datei mit dem schon existiert.
        /// Falls ja => fileExists hat den Wert true 
        /// falls nein, fileExists hat den Wert false
        /// nach dem beenden der Funktion
        /// </summary>
        /// <param name="filename"></param>
        public async void ExistFileWithName(string filename)
        {
            

            filename = "UserStudy-Olz.dat";
            StorageFolder picturesLibraryS = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
            StorageFile sampleFile = (StorageFile)await picturesLibraryS.TryGetItemAsync(filename);
            if (sampleFile == null)
            {
                // If file doesn't exist, indicate users to use scenario 1
                Debug.WriteLine(" If file doesn't exist, indicate users to use scenario 1");

            }

            Debug.WriteLine("Testausgabe 1");
            StorageFolder picturesLibrary = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
            StorageFile testFile = (StorageFile)await picturesLibrary.TryGetItemAsync(filename);

            Debug.WriteLine("Testausgabe 2");

            if (testFile == null)
            {
                // Datei existiert nicht 
                fileExists = false;
            }
            else {
                // Datei existiert bereits
                fileExists = true;
            }
            lock (this)
            {
                inFunction = false;
                Debug.WriteLine("Testausgabe 3");
            }
        }



        private async void writeFile(object sender, RoutedEventArgs e)
        {
            if (FilePath != "")
            {
                //storageFolder = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
                StorageFile sampleFile = null;

                string filename = "UserStudy-Olz.dat";
                filename = getNewFileName(30);

                //CanOpenFile(filename);

                try
                {
                    sampleFile = await storageFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                    await Windows.Storage.FileIO.WriteTextAsync(sampleFile, "Swift as a shadow");
                    Debug.WriteLine("The file '{0}' was created.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error ist passiert." + ex + " + " + ex.Message);
                }
            }
            else
            {
                PathTextBlock.Text = "Es wurde noch Kein Pfad ausgewählt. Bitte wählen Sie einen Pfad aus.";
            }
        }

        private string getNewFileName(int length)
        {
            string uniqueFileName = "";
            string uniqueFilePath = "";

            int i = 0;

            bool res = false;
            do
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                //string randomText = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
                //uniqueName = FILENAME + randomText + FILETYPE;
                uniqueFilePath = FilePath + FILENAME + i + FILETYPE;
                uniqueFileName = FILENAME + i + FILETYPE;
                string curFile = @"c:\temp\test.txt";
                curFile = FilePath + "\\test.txt";
                Debug.WriteLine(File.Exists(curFile) ? "File exists." : "File does not exist.");
                i++;

                //ExistFileWithName(uniqueName);

                res = File.Exists(uniqueFilePath);
                if (res)
                {
                    Debug.WriteLine(uniqueFilePath + " is true;");
                } else {
                    Debug.WriteLine(uniqueFilePath + " is false;");
                }

            } while (res);
            
            return uniqueFileName; 
        }

        private async void ChooseDirectory(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                PathTextBlock.Text = "Picked folder: " + folder.Path;
                FilePath = folder.Path;
                storageFolder = folder;
            }
            else
            {
                PathTextBlock.Text = "Operation cancelled.";
            }
        }

        private bool CanOpenFile(string filename)
        {
            bool res = false;

            FileInfo fInfo = new FileInfo(storageFolder.Path + "\\" + filename);

            return fInfo.IsReadOnly;

            //return res;
        }
    }
}
