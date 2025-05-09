using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace MapFusion.Models
{
    public static class CsvHelper
    {
        public static void WriteGmapData(List<GmapPlace> places, string filePath)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Установка разделителя полей
                Delimiter = ",",
                // Установка заголовков (названий полей) в первой строке файла
                HasHeaderRecord = true,
            };

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, csvConfig))
            {
                // Запись заголовков (названий полей)
                csv.WriteRecords(places);
            }
        }

        public static void WriteYmapData(List<YmapPlace> places, string filePath)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Установка разделителя полей
                Delimiter = ",",
                // Установка заголовков (названий полей) в первой строке файла
                HasHeaderRecord = true,
            };

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, csvConfig))
            {
                // Запись заголовков (названий полей)
                csv.WriteRecords(places);
            }
        }

        public static void WriteTwoGISmapData(List<TwoGISmapPlace> places, string filePath)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Установка разделителя полей
                Delimiter = ",",
                // Установка заголовков (названий полей) в первой строке файла
                HasHeaderRecord = true,
            };

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, csvConfig))
            {
                // Запись заголовков (названий полей)
                csv.WriteRecords(places);
            }
        }
    }
}
