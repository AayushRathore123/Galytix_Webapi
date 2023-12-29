using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CsvHelper.Configuration;
using System.Threading.Tasks;
[ApiController]
[Route("api/[controller]")]
public class CountryGwpController : ControllerBase
{
    private readonly List<GwpEntry> _data;

    public CountryGwpController()
    {
        // Read CSV data and store it in '_data' list
        using var reader = new StreamReader("Data/gwpByCountry.csv");
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        _data = csv.GetRecords<GwpEntry>().ToList();
    }
    
    [HttpPost("avg")]
    
    public async Task<IActionResult> GetAverageGwp([FromBody] GwpRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Country) || request.Lob == null || !request.Lob.Any())
        {
            return BadRequest("Invalid input format");
        }
        var filePath = "C:\\Users\\DELL\\Downloads\\Galytix.WebApi (4)\\Data\\gwpByCountry.csv";

        if (!System.IO.File.Exists(filePath))
        {
            return BadRequest("CSV file not found.");
        }

        var data = await ReadCsv(filePath);
        var result = new Dictionary<string, double>();

        // foreach (var category in request.Lob)
        // {
        //     var filteredData = _data.Where(entry => entry.Country == request.Country && entry.Lob == category);
        //     var totalGwp = filteredData.Sum(entry => entry.Gwp);
        //     result[category] = decimal.Round(totalGwp, 1);
        // }
        foreach (var lob in request.Lob)
        {
            var total = _data
                .Where(d => d.Country == request.Country && d.Lob == lob)
                .SelectMany(d => d.YearData)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
                .Sum();

            result.Add(lob, total);

        }
        return Ok(result);
    }
    private async Task<List<GwpEntry>> ReadCsv(string filePath)
        {
            var lines = await System.IO.File.ReadAllLinesAsync(filePath);

            var data = new List<GwpEntry>();

            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(',');

                var model = new GwpEntry
                {
                    Country = values[0].Trim(),
                   
                    Lob = values[1].Trim(),
                    YearData = values.Skip(4).Select(d => d.Trim()).ToList()
                };

                data.Add(model);
            }
            return data;
        }
}