using AudientUWP.ViewModel;
using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AudientUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel { get; set; }
        public MainPage()
        { 
            this.InitializeComponent();
            ViewModel = new MainPageViewModel();
            //this.chartView.Chart = ViewModel.AggChart;
            this.mediaPlayer = ViewModel.mMedia;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            ViewModel.PredictedLabel = "Processing file.";
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    var storageFile = items[0] as StorageFile;
                    StorageFile copiedFile = await storageFile.CopyAsync(localFolder, "temp.wav", NameCollisionOption.ReplaceExisting);
                    Console.WriteLine($"HELLO! {copiedFile.Path}");
                    ViewModel.extractFeatures(copiedFile.Path, storageFile);
                }
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            // To display the data which is dragged    
            e.DragUIOverride.Caption = "Drop here to analyse the audio file.";
            e.DragUIOverride.IsGlyphVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsCaptionVisible = true;
        }
    }
}
