using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapFusion.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.Playwright;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Windows;

namespace MapFusion.ViewModels
{
    public partial class GoogleMapsPageViewModel : ObservableObject
    {
        private List<GmapPlace> _processedPlaces;

        public GoogleMapsPageViewModel(MainViewModel mainVM)
        {
            MainVM = mainVM;
            _processedPlaces = new List<GmapPlace>();
            SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(1000));
        }

        [ObservableProperty]
        private MainViewModel _mainVM;

        [ObservableProperty]
        private SnackbarMessageQueue _snackbarMessageQueue;

        [ObservableProperty]
        private bool _isExportAvaliable = false;

        [ObservableProperty]
        private int _selectedDepth = 5;

        [ObservableProperty]
        private string _outputText;

        [ObservableProperty]
        private Query _currentQuery = new Query();

        [RelayCommand]
        private async void LaunchParsing()
        {
            try
            {
                IsExportAvaliable = false;
                MainVM.IsBusy = true;
                OutputText = "";
                OutputText += "\n[Подготовка...]";
                MainVM.ParseStatus = "[Подготовка]";

                var exitCode = await Task.Run(() => Program.Main(new[] { "install" }));
                if (exitCode != 0)
                {
                    throw new Exception($"Playwright exited with code {exitCode}");
                }

                using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);
                using var browserPool = new BrowserPool(10);

                OutputText += "\n[Анализ мест...]";
                MainVM.ParseStatus = "[Анализ мест]";
                var gmapProcessor = new GmapProcessor("ru", CurrentQuery.ToString(), SelectedDepth);
                _processedPlaces = await gmapProcessor.ProcessAsync(await browserPool.RentBrowserAsync(playwright)).ConfigureAwait(false);

                var tasks = new List<Task>();
                foreach (var place in _processedPlaces)
                {
                    Debug.WriteLine(place.URL);
                    OutputText += "\n" + place.URL;

                    var browser = await browserPool.RentBrowserAsync(playwright).ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await place.ProcessAsync(browser).ConfigureAwait(false);

                            double progress = (double)tasks.Count(t => t.IsCompleted) / tasks.Count * 100;
                            MainVM.ParseStatus = $"{(int)progress}%";
                        }
                        finally
                        {
                            browserPool.ReturnBrowser(browser);
                        }
                    }));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                OutputText += "\n[Парсинг завершен]";
                MainVM.ParseStatus = "[Парсинг завершен]";
                MainVM.IsBusy = false;
                IsExportAvaliable = true;
            }
            catch (Exception ex)
            {
                // Обработка исключения здесь
                OutputText += $"\nПроизошла ошибка: {ex.Message}";
                MainVM.ParseStatus = "[Парсинг завершён]";
                SnackbarMessageQueue.Enqueue("Произошла ошибка парсинга.");
                MainVM.IsBusy = false;
                IsExportAvaliable = false;
            }
        }


        [RelayCommand]
        private void ExportToCsv()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Установка свойств диалогового окна сохранения
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv"; // Фильтр для типа файлов
            saveFileDialog.FilterIndex = 1; // Индекс выбранного фильтра (начинается с 1)
            saveFileDialog.FileName = "Новый документ.csv"; // Начальное имя файла
            saveFileDialog.DefaultExt = ".csv"; // Расширение файла по умолчанию
            saveFileDialog.Title = "Сохранить CSV файл"; // Заголовок диалогового окна

            // Открытие диалогового окна сохранения файла и обработка результата
            bool? result = saveFileDialog.ShowDialog();

            // Проверка результата
            if (result == true)
            {
                // Получение пути к выбранному файлу
                string filePath = saveFileDialog.FileName;

                // Пример сохранения CSV файла
                Models.CsvHelper.WriteGmapData(_processedPlaces, filePath);
            }

            SnackbarMessageQueue.Enqueue("Файл успешно сохранён.");

        }
    }
}
