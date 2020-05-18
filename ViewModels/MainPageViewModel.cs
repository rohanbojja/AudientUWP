using Microcharts;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.Statistics;
using Microsoft.ML;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.Features;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Windows;
using System.Linq;
using Path = System.IO.Path;
using System.IO;
using AudientUWP.Models;
using AudientUWP.Services;
using Plugin.SimpleAudioPlayer;
using SkiaSharp;
using Windows.UI.Xaml.Controls;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.Storage.FileProperties;

namespace AudientUWP.ViewModel
{
    public class MainPageViewModel: BaseViewModel
    {

        public MainPageViewModel()
        {
            PredictedLabel = "Init.";
            mMedia = new MediaPlayerElement();
            var entries = new[]
             {
                 new ChartEntry(212)
                 {
                     Label = "What will it be? :(",
                     ValueLabel = "212",
                     Color = SKColor.Parse("#2c3e50")
                 }
            };

            AggChart = new DonutChart() { Entries = entries, IsAnimated = true, AnimationDuration = TimeSpan.FromSeconds(2) };
        }

        float[] op = new float[10];
        System.Threading.Timer timer;
        string tag = "AudXam";
        List<float[]> featureTimeList = new List<float[]>();
        public List<float[]> tdVectors;
        MediaPlayerElement mediaPlayer;
        public MediaPlayerElement mMedia
        {
            get { return mediaPlayer; }
            set
            {
                SetProperty(ref mediaPlayer, value);
            }
        }
        public List<float[]> mfccVectors;
        int duration = 0;
        public string featureString = String.Empty;
        public string filePath;
        public string FilePath {
            get { return filePath; }
            set {
                SetProperty(ref filePath, value);
            }
        }

        public Chart aggChart;
        public Chart AggChart { get { return aggChart; } set { SetProperty(ref aggChart, value); } }
        public string predictedLabel;
        ISimpleAudioPlayer player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
        public string PredictedLabel
        {
            get { return predictedLabel; }
            set
            {
                SetProperty(ref predictedLabel, value);
            }
        }
        async public void playAudio()
        {
            mMedia.MediaPlayer.Play();
            //player.Load(FilePath);
            ////player.Play();
            //duration = Convert.ToInt32(player.Duration);
            Console.WriteLine($"{tag} DUR: {duration}");
            
            player.Play();
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMilliseconds(60);
            float[] op2 = new float[10];
            Array.Clear(op2, 0, op2.Length);
            timer = new System.Threading.Timer(async (e) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    var curPos = Convert.ToInt32(mMedia.MediaPlayer.Position.Seconds);
                    op = featureTimeList[curPos];
                    if (curPos == duration)
                    {
                        timer.Dispose();
                        for (int i = 0; i < 10; i++)
                        {
                            //if (op2[i] > op[i])
                            //{
                            //    op2[i] -= op[i] / 16.7f;
                            //}
                            //else
                            //{
                            //    op2[i] += op[i] / 16.7f;
                            //}
                            op2[i] += op[i]/duration;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            if (op2[i] > op[i] + 0.4*op[i] / 16.667f)
                            {
                                op2[i] -= op[i] / 16.6f;
                            }
                            else if (op[i] > op2[i] + 0.4*op[i] / 16.667f)
                            {
                                op2[i] += op[i] / 16.6f;
                            }
                            //op2[i] += op[i];
                        }
                    }
                    // Your UI update code goes here!
                    

                    float max_sf = 0;
                    int max_ind = 0;
                    string[] labels = "Blues, Classical, Country, Disco, HipHop, Jazz, Metal, Pop, Reggae Rock".Split(',');
                    for (int i = 0; i < 10; i++)
                    {
                        if (op2[i] > max_sf)
                        {
                            max_sf = op2[i];
                            max_ind = i;
                        }
                    }

                    PredictedLabel = $"{labels[max_ind]}!";

                    //LAST 
                    var tmpMax = 0f;
                    if (curPos == duration)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            tmpMax += op2[i];
                        }
                        for (int i = 0; i < 10; i++)
                        {
                            op2[i] = op2[i]/tmpMax*100;
                            PredictedLabel = $"{labels[max_ind]} {op2[max_ind]}%!";
                        }
                    }

                        var entries_agg = new[]
                     {
                                 new ChartEntry(op2[0])
                                 {
                                     Label = "Blues",
                                     ValueLabel = $"{op2[0]}",
                                     Color = SKColor.Parse("#2c3e50")
                                 },
                                 new ChartEntry(op2[1])
                                 {
                                     Label = "Classical",
                                     ValueLabel = $"{op2[1]}",
                                     Color = SKColor.Parse("#77d065")
                                 },
                                 new ChartEntry(op2[2])
                                 {
                                     Label = "Country",
                                     ValueLabel = $"{op2[2]}",
                                     Color = SKColor.Parse("#b455b6")
                                 },
                                 new ChartEntry(op2[3])
                                 {
                                     Label = "Disco",
                                     ValueLabel = $"{op2[3]}",
                                     Color = SKColor.Parse("#245e50")
                                 },
                                 new ChartEntry(op2[4])
                                 {
                                     Label = "Hiphop",
                                     ValueLabel = $"{op2[4]}",
                                     Color = SKColor.Parse("#3498db")
                                 },
                                 new ChartEntry(op2[5])
                                 {
                                     Label = "Jazz",
                                     ValueLabel = $"{op2[5]}",
                                     Color = SKColor.Parse("#263e50")
                                 },
                                 new ChartEntry(op2[6])
                                 {
                                     Label = "Metal",
                                     ValueLabel = $"{op2[6]}",
                                     Color = SKColor.Parse("#123456")
                                 },
                                 new ChartEntry(op2[7])
                                 {
                                     Label = "Pop",
                                     ValueLabel = $"{op2[7]}",
                                     Color = SKColor.Parse("#654321")
                                 },
                                 new ChartEntry(op2[8])
                                 {
                                     Label = "Reggae",
                                     ValueLabel = $"{op2[8]}",
                                     Color = SKColor.Parse("#526784")
                                 },
                                 new ChartEntry(op2[9])
                                 {
                                     Label = "Rock",
                                     ValueLabel = $"{op2[9]}",
                                     Color = SKColor.Parse("#404040")
                                 }
                            };

                    //Update and draw graph
                    AggChart = new DonutChart() { Entries = entries_agg, IsAnimated = false, AnimationDuration = TimeSpan.FromMilliseconds(0) };
                }
                ).AsTask();
                
            }, null, startTimeSpan, periodTimeSpan);
        }
        async public void extractFeatures(string _filepath, StorageFile sf)
        {
            op = new float[10];
            tdVectors = new List<float[]>();
            mfccVectors = new List<float[]>();


            featureTimeList = new List<float[]>();

            //NWaves
            FilePath = _filepath;
            PredictedLabel = "Ready!.";
            //player.Load(GetStreamFromFile(FilePath));
            //player.Play();
            mMedia.Source = MediaSource.CreateFromStorageFile(sf);
            bool test = player.IsPlaying;
            mMedia.AutoPlay = true;
            MusicProperties properties = await sf.Properties.GetMusicPropertiesAsync();
            TimeSpan myTrackDuration = properties.Duration;
            duration = Convert.ToInt32(myTrackDuration.TotalSeconds);
            if (FilePath != null)
            {
                DiscreteSignal signal;

                // load
                var mfcc_no = 24;
                var samplingRate = 44100;
                var mfccOptions = new MfccOptions
                {
                    SamplingRate = samplingRate,
                    FeatureCount = mfcc_no,
                    FrameDuration = 0.025/*sec*/,
                    HopDuration = 0.010/*sec*/,
                    PreEmphasis = 0.97,
                    Window = WindowTypes.Hamming
                };

                var opts = new MultiFeatureOptions
                {
                    SamplingRate = samplingRate,
                    FrameDuration = 0.025,
                    HopDuration = 0.010
                };
                var tdExtractor = new TimeDomainFeaturesExtractor(opts);
                var mfccExtractor = new MfccExtractor(mfccOptions);

                // Read from file.
                featureString = String.Empty;
                featureString = $"green,";
                //MFCC
                var mfccList = new List<List<double>>();
                var tdList = new List<List<double>>();
                //MFCC
                //TD Features
                //Spectral features
                for (var i = 0; i < mfcc_no; i++)
                {
                    mfccList.Add(new List<double>());
                }
                for (var i = 0; i < 4; i++)
                {
                    tdList.Add(new List<double>());
                }


                string specFeatures = String.Empty;
                Console.WriteLine($"{tag} Reading from file");
                using (var stream = new FileStream(FilePath, FileMode.Open))
                {
                    var waveFile = new WaveFile(stream);
                    signal = waveFile[channel: Channels.Left];
                    ////Compute MFCC
                    float[] mfvfuck = new float[25];
                    var sig_sam = signal.Samples;
                    mfccVectors = mfccExtractor.ComputeFrom(sig_sam);

                    var fftSize = 1024;
                    tdVectors = tdExtractor.ComputeFrom(signal.Samples);
                    var fft = new Fft(fftSize);
                    var resolution = (float)samplingRate / fftSize;

                    var frequencies = Enumerable.Range(0, fftSize / 2 + 1)
                                                .Select(f => f * resolution)
                                                .ToArray();

                    var spectrum = new Fft(fftSize).MagnitudeSpectrum(signal).Samples;

                    var centroid = Spectral.Centroid(spectrum, frequencies);
                    var spread = Spectral.Spread(spectrum, frequencies);
                    var flatness = Spectral.Flatness(spectrum, 0);
                    var noiseness = Spectral.Noiseness(spectrum, frequencies, 3000);
                    var rolloff = Spectral.Rolloff(spectrum, frequencies, 0.85f);
                    var crest = Spectral.Crest(spectrum);
                    var decrease = Spectral.Decrease(spectrum);
                    var entropy = Spectral.Entropy(spectrum);
                    specFeatures = $"{centroid},{spread},{flatness},{noiseness},{rolloff},{crest},{decrease},{entropy}";
                    //}
                    Console.WriteLine($"{tag} All features ready");

                    for (int calibC = 0; calibC < mfccVectors.Count;)
                    {
                        featureString = String.Empty;
                        var tmp = new ModelInput();

                        for(var j=0; j< (mfccVectors.Count / duration) -1 && calibC < mfccVectors.Count; j++)
                        {
                            for (var i = 0; i < mfcc_no; i++)
                            {
                                mfccList[i].Add(mfccVectors[calibC][i]);
                            }
                            for (var i = 0; i < 4; i++)
                            {
                                tdList[i].Add(tdVectors[calibC][i]);
                            }
                            calibC += 1;
                        }
                        
                        var mfcc_statistics = new List<double>();
                        for (var i = 0; i < mfcc_no; i++)
                        {
                            //preheader += m + "_mean";
                            //preheader += m + "_min";
                            //preheader += m + "_var";
                            //preheader += m + "_sd";
                            //preheader += m + "_med";
                            //preheader += m + "_lq";
                            //preheader += m + "_uq";
                            //preheader += m + "_skew";
                            //preheader += m + "_kurt";
                            mfcc_statistics.Add(Statistics.Mean(mfccList[i]));
                            mfcc_statistics.Add(Statistics.Minimum(mfccList[i]));
                            mfcc_statistics.Add(Statistics.Variance(mfccList[i]));
                            mfcc_statistics.Add(Statistics.StandardDeviation(mfccList[i]));
                            mfcc_statistics.Add(Statistics.Median(mfccList[i]));
                            mfcc_statistics.Add(Statistics.LowerQuartile(mfccList[i]));
                            mfcc_statistics.Add(Statistics.UpperQuartile(mfccList[i]));
                            mfcc_statistics.Add(Statistics.Skewness(mfccList[i]));
                            mfcc_statistics.Add(Statistics.Kurtosis(mfccList[i]));
                        }
                        var td_statistics = new List<double>();

                        for (var i = 0; i < 4; i++)
                        {
                            td_statistics.Add(Statistics.Mean(tdList[i]));
                            td_statistics.Add(Statistics.Minimum(tdList[i]));
                            td_statistics.Add(Statistics.Variance(tdList[i]));
                            td_statistics.Add(Statistics.StandardDeviation(tdList[i]));
                            td_statistics.Add(Statistics.Median(tdList[i]));
                            td_statistics.Add(Statistics.LowerQuartile(tdList[i]));
                            td_statistics.Add(Statistics.UpperQuartile(tdList[i]));
                            td_statistics.Add(Statistics.Skewness(tdList[i]));
                            td_statistics.Add(Statistics.Kurtosis(tdList[i]));
                        }

                        // Write MFCCs
                        featureString += String.Join(",", mfcc_statistics);
                        featureString += ",";
                        featureString += String.Join(",", td_statistics);
                        //Write Spectral features as well
                        featureString += ",";
                        featureString += specFeatures;
                        Console.WriteLine($"{tag} Feature String ready {featureString}");
                        if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp")))
                        {
                            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp"));
                            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp"), featureString);
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp"), featureString);
                        }

                        MLContext mLContext = new MLContext();

                        string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");

                        IDataView dataView = mLContext.Data.LoadFromTextFile<ModelInput>(
                                            path: fileName,
                                            hasHeader: false,
                                            separatorChar: ',',
                                            allowQuoting: true,
                                            allowSparse: false);

                        // Use first line of dataset as model input
                        // You can replace this with new test data (hardcoded or from end-user application)
                        ModelInput sampleForPrediction = mLContext.Data.CreateEnumerable<ModelInput>(dataView, false)
                                                                                    .First();
                        ModelOutput opm = ConsumeModel.Predict(sampleForPrediction);
                        featureTimeList.Add(opm.Score);
                        Console.WriteLine($"{tag} Feature vs time list ready");
                    }
                    //Console.WriteLine($"{tag} MFCC: {mfccVectors.Count}");
                    //Console.WriteLine($"{tag} TD: {tdVectors.Count}");
                    //Console.WriteLine($"{tag} featureTimeArray: {featureTimeList.Count} {featureString}");
                }

            }
            playAudio();
        }
    }
}
