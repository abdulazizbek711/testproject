using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/home/hp/RiderProjects/App/App/Settings.json");
            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
            string csvFilePath = settings.CSVFilePath;
            List<CSVDATA> records;
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false,
            }))
            {
                csv.Read();
                records = csv.GetRecords<CSVDATA>().ToList();
            }

            foreach (var record in records)
            {
                Console.WriteLine($"{record.UserId}, {record.Name}, {record.SecondName}, {record.Number}");
            }

            XDocument xdoc = XDocument.Load(settings.XMLFilePath);
            List<XMLDATA> xmlRecords = xdoc.Descendants("Card")
                .Select(card => new XMLDATA()
                {
                    UserId = card.Attribute("UserId").Value,
                    Pan = card.Element("Pan").Value,
                    ExpDate = card.Element("ExpDate").Value
                })
                .ToList();

            var mergedData = (from csvRecord in records
                              join xmlRecord in xmlRecords on csvRecord.UserId equals xmlRecord.UserId
                              select new MERGEDDATA()
                              {
                                  UserId = csvRecord.UserId,
                                  Pan = xmlRecord.Pan,
                                  ExpDate = xmlRecord.ExpDate,
                                  Name = csvRecord.Name,
                                  SecondName = csvRecord.SecondName,
                                  Number = csvRecord.Number
                              }).ToList();
            
            Console.WriteLine($"Preparing to generate a report with {mergedData.Count} records.");
            Console.Write("Do you want to proceed? (Y/N): ");
            string response = Console.ReadLine();

            if (response.Trim().ToUpper() == "Y")
            {
                SerializeToJson(mergedData, settings.JSONFilePath);
                Console.WriteLine("Merged data saved to JSON.");
            }
            else
            {
                Console.WriteLine("Report generation cancelled by user.");
            }
        }
        
        private static void SerializeToJson<T>(IEnumerable<T> data, string filePath)
        {
            string json = JsonConvert.SerializeObject(new { Records = data }, Formatting.Indented);

            using (TextWriter writer = new StreamWriter(filePath))
            {
                writer.Write(json);
            }
        }
    }
}
