using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string inputFilePath = "vaccination_data.csv";

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

        // Calculate median daily vaccinations per country
        var medianVaccinations = groupedData
            .Select(group => new
            {
                Country = group.Key,
                MedianVaccination = CalculateMedian(group.Select(d => d.DailyVaccinations.Value).ToList())
            })
            .OrderByDescending(x => x.MedianVaccination)
            .Take(3)
            .ToList();

        // Output the top-3 countries with the highest median daily vaccinations
        Console.WriteLine("Top-3 countries with highest median daily vaccination numbers:");
        foreach (var item in medianVaccinations)
        {
            Console.WriteLine($"{item.Country}: {item.MedianVaccination}");
        }
    }

    // Function to calculate the median of a list of integers
    static double CalculateMedian(List<int> numbers)
    {
        numbers.Sort();
        int count = numbers.Count;
        if (count % 2 == 0)
        {
            // If even, average of two middle elements
            return (numbers[count / 2 - 1] + numbers[count / 2]) / 2.0;
        }
        else
        {
            // If odd, the middle element
            return numbers[count / 2];
        }
    }

    class VaccinationRecord
    {
        public string Country { get; set; }
        public DateTime Date { get; set; }
        public int? DailyVaccinations { get; set; }
        public string Vaccines { get; set; }
    }
}
