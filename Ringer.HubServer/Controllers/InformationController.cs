using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ringer.HubServer.Data;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Ringer.HubServer.Models;
using System.Text.Json;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Net.Http.Headers;

namespace Ringer.HubServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class InformationController : ControllerBase
    {
        private readonly RingerDbContext _dbContext;
        public InformationController(RingerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("consulates")]
        public async Task<IActionResult> GetConsulates()
        {
            var consulates = await _dbContext.Consulates.ToListAsync();

            return Ok(consulates);
        }


        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {

            var consulates = await JsonSerializer.DeserializeAsync<List<Consulate>>(file.OpenReadStream());

            await _dbContext.Consulates.AddRangeAsync(consulates);
            await _dbContext.SaveChangesAsync();

            var savedConsulates = await _dbContext.Consulates.ToListAsync();

            return Ok(savedConsulates);
        }

        [HttpPost("dropzone")]
        public async Task<IActionResult> Dropzone()
        {
            var files = HttpContext.Request.Form.Files;
            var consulates = new List<Consulate>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using(var stream = file.OpenReadStream())
                    using(var reader = new StreamReader(stream))
                    using(var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csvReader.Read();
                        csvReader.ReadHeader();
                        while (csvReader.Read())
                        {
                            var consulate = new Consulate
                            {
                                ConsulateType = csvReader.GetField("ConsulateType"),
                                Country = csvReader.GetField("Country"),
                                KoreanName = csvReader.GetField("KoreanName"),
                                LocalName = csvReader.GetField("LocalName"),
                                PhoneNumber = csvReader.GetField("PhoneNumber"),
                                EmergencyPhoneNumber = csvReader.GetField("EmergencyPhoneNumber"),
                                Email = csvReader.GetField("Email"),
                                Address = csvReader.GetField("Address"),
                                Homepage = csvReader.GetField("Homepage"),
                                Latitude = csvReader.GetField<double>("Latitude"),
                                Longitude = csvReader.GetField<double>("Longitude"),
                                GoogleMap = csvReader.GetField("GoogleMap")
                            };
                            consulates.Add(consulate);
                        }
                    }
                }
            }

            await _dbContext.Consulates.AddRangeAsync(consulates);
            await _dbContext.SaveChangesAsync();

            var savedConsulates = await _dbContext.Consulates.ToListAsync();

            return Ok(savedConsulates);
        }

        [HttpPost("upload-csv")]
        public async Task<IActionResult> csv(IFormFile csv)
        {
            var consulates = new List<Consulate>();

            using(var stream = csv.OpenReadStream())
            using(var reader = new StreamReader(stream))
            using(var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csvReader.Read();
                csvReader.ReadHeader();
                while (csvReader.Read())
                {
                    var consulate = new Consulate
                    {
                        ConsulateType = csvReader.GetField("ConsulateType"),
                        Country = csvReader.GetField("Country"),
                        KoreanName = csvReader.GetField("KoreanName"),
                        LocalName = csvReader.GetField("LocalName"),
                        PhoneNumber = csvReader.GetField("PhoneNumber"),
                        EmergencyPhoneNumber = csvReader.GetField("EmergencyPhoneNumber"),
                        Email = csvReader.GetField("Email"),
                        Address = csvReader.GetField("Address"),
                        Homepage = csvReader.GetField("Homepage"),
                        Latitude = csvReader.GetField<double>("Latitude"),
                        Longitude = csvReader.GetField<double>("Longitude"),
                        GoogleMap = csvReader.GetField("GoogleMap")
                    };

                    consulates.Add(consulate);
                }
            }

            await _dbContext.Consulates.AddRangeAsync(consulates);
            await _dbContext.SaveChangesAsync();

            var savedConsulates = await _dbContext.Consulates.ToListAsync();

            return Ok(savedConsulates);
        }
    }
}