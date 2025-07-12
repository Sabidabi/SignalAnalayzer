using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SignalAnalayzer.Models;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace SignalAnalayzer.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string SelectFileText => LanguageManager.GetString("SelectFile");
        public string ProcessSignalsText => LanguageManager.GetString("ProcessSignals");
        public string FileNameHeader => LanguageManager.GetString("FileName");
        public string MaxHeader => LanguageManager.GetString("Max");
        public string MinHeader => LanguageManager.GetString("Min");
        public string ExpectMateHeader => LanguageManager.GetString("ExpectMate");

        public string SignalNameHeader => LanguageManager.GetString("SignalName");
        public string FrequencyHeader => LanguageManager.GetString("Frequency");
        public string DurationHeader => LanguageManager.GetString("Duration");
        private string directory="";
        public ObservableCollection<SignalFileInfo> SignalFiles { get;} = new ObservableCollection<SignalFileInfo>();

        private static readonly XmlSerializer formatter = new XmlSerializer(typeof(BOSMeth));

        [RelayCommand]
        private async Task OpenFile()
        {
            ErrorMessages?.Clear();
            try
            {
                var file = await DoOpenFilePickerAsync();
                if (file == null) return;
                directory = Path.GetDirectoryName(file.Path.LocalPath);
                SignalFiles.Clear();
                await using var readStream = await file.OpenReadAsync();
                using var reader = new StreamReader(readStream);
                BOSMeth? bosMeth = formatter.Deserialize(reader) as BOSMeth;
                if (bosMeth is null) return;
                foreach (var channel in bosMeth.Channels)
                {
                    SignalFiles.Add(new SignalFileInfo
                    {
                        FileName = channel.SignalFileName,
                        SamplingFreq = channel.EffectiveFd,
                        DurationSec = 0,
                        SignalName = NameOfSignal(channel.Type)
                    });
                }
            }
            catch (Exception e)
            {
                ErrorMessages?.Add(e.Message);
            }
        }
        private async Task<IStorageFile?> DoOpenFilePickerAsync()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider");
            var fileTypeFilters = new List<FilePickerFileType>
            {
                new FilePickerFileType("XML Files")
                {
                    Patterns = new[] { "*.xml" },
                    AppleUniformTypeIdentifiers = new [] {"public.xml"},
                    MimeTypes = new[] { "text/xml" }
                }
            };
            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open XML File",
                AllowMultiple = false,
                FileTypeFilter = fileTypeFilters
            });

            return files?.Count == 1 ? files[0] : null;
        }
        [RelayCommand]
        private async Task ProcessSignals()
        {
            var tasks = new List<Task>();
            var numBatchReports = 100;
            foreach (var file in SignalFiles)
            {
                var task = ProcessSingleFileAsync(file, numBatchReports);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessSingleFileAsync(SignalFileInfo file, int numReports)
        {
            try
            {
                using var stream = new FileStream(Path.Combine(directory, file.FileName), FileMode.Open);
                var buffer = new byte[sizeof(double) * numReports];
                int doublesToReadAll=0;
                int bytesRead;
                List<double> data = [];
                SignalCalculatorStat signalCalculatorStat = new SignalCalculatorStat();
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    int doublesToRead = bytesRead / sizeof(double);
                    doublesToReadAll += doublesToRead;
                    data.Clear();
                    for (int i = 0; i < doublesToRead; i++)
                    {
                        data.Add(BitConverter.ToDouble(buffer, i * sizeof(double)));
                    }
                    signalCalculatorStat.CalculateNewBlock(data);
                }
                file.Max=signalCalculatorStat.Max;
                file.Min=signalCalculatorStat.Min;
                file.ExpectMate = signalCalculatorStat.ExpectMate;
                if(file.SamplingFreq!=0) file.DurationSec = doublesToReadAll / file.SamplingFreq;
                else ErrorMessages?.Add($"Sampling frequency is zero for {file.FileName}");
            }
            catch (Exception e)
            {
                ErrorMessages?.Add(e.Message);
            }
        }
        private string NameOfSignal(int type)
        {
            string name = type switch
            {
                1 => "ЭЭГ",
                2 => "ЭКГ",
                3 => "ЭМГ",
                _ => type.ToString(),
            };
            return name;
        }
    }
}
