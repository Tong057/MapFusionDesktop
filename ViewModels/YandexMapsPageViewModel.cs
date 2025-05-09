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
    public partial class YandexMapsPageViewModel : ObservableObject
    {
        private List<YmapPlace> _processedPlaces;

        public YandexMapsPageViewModel(MainViewModel mainVM)
        {
            MainVM = mainVM;
            _processedPlaces = new List<YmapPlace>();
            SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(1000));
        }

        [ObservableProperty]
        private MainViewModel _mainVM;

        [ObservableProperty]
        private SnackbarMessageQueue _snackbarMessageQueue;

        [ObservableProperty]
        private bool _isExportAvaliable = false;

        [ObservableProperty]
        private int _selectedDepth = 10;

        [ObservableProperty]
        private string _outputText = "";

        [ObservableProperty]
        private Query _currentQuery = new Query();

        [RelayCommand]
        private async void LaunchParsing()
        {
            try
            {
                IsExportAvaliable = false;
                MainVM.IsBusy = true;
                OutputText += "\n[Подготовка...]";
                MainVM.ParseStatus = "[Подготовка]";

                var exitCode = await Task.Run(() => Program.Main(new[] { "install" })).ConfigureAwait(false);
                if (exitCode != 0)
                {
                    throw new Exception($"Playwright exited with code {exitCode}");
                }

                using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);
                using var browserPool = new BrowserPool(1);

                OutputText += "\n[Анализ мест...]";
                MainVM.ParseStatus = "[Анализ мест]";
                var ymapProcessor = new YmapProcessor("ru", CurrentQuery.ToString(), SelectedDepth);
                _processedPlaces = await ymapProcessor.ProcessAsync(await browserPool.RentBrowserAsync(playwright)).ConfigureAwait(false);

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
                Models.CsvHelper.WriteYmapData(_processedPlaces, filePath);
            }

            SnackbarMessageQueue.Enqueue("Файл успешно сохранён.");

        }
    }
}
