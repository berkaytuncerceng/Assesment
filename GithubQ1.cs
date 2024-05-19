using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string inputFilePath = "country_vaccination_data.csv";
        string outputFilePath = "country_vaccination_data_output.csv";

        // Read the input data
        var lines = File.ReadAllLines(inputFilePath).ToList();
        var header = lines[0];
        var dataLines = lines.Skip(1).ToList();

        // Parse the data
        var data = new List<VaccinationRecord>();
        foreach (var line in dataLines)
        {
            var fields = line.Split(',');
            data.Add(new VaccinationRecord
            {
                Country = fields[0],
                Date = DateTime.Parse(fields[1]),
                DailyVaccinations = string.IsNullOrEmpty(fields[2]) ? (int?)null : int.Parse(fields[2]),
                Vaccines = fields[3]
            });
        }

        // Impute missing data
        var groupedData = data.GroupBy(d => d.Country);
        foreach (var group in groupedData)
        {
            var minVaccination = group.Where(d => d.DailyVaccinations.HasValue).Select(d => d.DailyVaccinations.Value).DefaultIfEmpty(0).Min();
            foreach (var record in group)
            {
                if (!record.DailyVaccinations.HasValue)
                {
                    record.DailyVaccinations = minVaccination;
                }
            }
        }

        // Write the output data
        using (var writer = new StreamWriter(outputFilePath))
        {
            writer.WriteLine(header);
            foreach (var record in data)
            {
                writer.WriteLine($"{record.Country},{record.Date.ToString("M/d/yyyy")},{record.DailyVaccinations},{record.Vaccines}");
            }
        }

        Console.WriteLine($"Output written to {outputFilePath}");
    }

    class VaccinationRecord
    {
        public string Country { get; set; }
        public DateTime Date { get; set; }
        public int? DailyVaccinations { get; set; }
        public string Vaccines { get; set; }
    }
}
