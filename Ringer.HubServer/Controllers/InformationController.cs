using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ringer.HubServer.Data;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Linq;
using Ringer.Core.Models;
using Geolocation;
using System;
using Microsoft.Extensions.Logging;
using Ringer.Core.Data;
using CsvHelper.Configuration;

namespace Ringer.HubServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class InformationController : ControllerBase
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<InformationController> _logger;

        public InformationController(RingerDbContext dbContext, ILogger<InformationController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("foot-print")]
        public async Task<IActionResult> RecordFootPrintAsync([FromBody]FootPrint footPrint)
        {
            footPrint.TimeStamp = DateTime.UtcNow;
            _dbContext.FootPrints.Add(footPrint);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("consulates/{lat}/{lon}")]
        public async Task<IActionResult> GetConsulateByCoordinates(double lat, double lon)
        {
            List<Consulate> consulates = await _dbContext.Consulates.ToListAsync();
            var origin = new Coordinate(lat, lon);

            foreach (var con in consulates)
            {
                con.Distance = GeoCalculator.GetDistance(origin, new Coordinate(con.Latitude, con.Longitude), 1, DistanceUnit.Kilometers);
            }

            //return Ok(consulates.OrderBy(c => c.Distance).Take(5));
            return Ok(consulates.Where(c => c.Distance < 1500).OrderBy(c => c.Distance));
        }

        [HttpGet("consulates")]
        public async Task<IActionResult> GetConsulates()
        {
            List<Consulate> consulates = await _dbContext.Consulates.ToListAsync();

            return Ok(consulates);
        }

        [HttpGet("consulates/{countryCode}")] // /information/consulates/us
        public async Task<IActionResult> GetConsulatesByCoundtryCode(string countryCode)
        {
            List<Consulate> consulates = await _dbContext.Consulates.Where(c => c.CountryCode == countryCode).ToListAsync();

            return Ok(consulates);
        }

        [HttpGet("set-countrycode/{id}/{countryCode}")]
        public async Task<IActionResult> SetCountryCode(int id, string countryCode)
        {
            var consulate = await _dbContext.Consulates.FirstOrDefaultAsync(c => c.Id == id);
            consulate.CountryCodeAndroid = countryCode;
            consulate.CountryCodeiOS = consulate.CountryCode;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("upload-csv")]
        public async Task<IActionResult> csv(IFormFile csv)
        {
            var consulates = new List<Consulate>();

            using (var stream = csv.OpenReadStream())
            using (var reader = new StreamReader(stream))
            using (var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            }))
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
                        GoogleMap = csvReader.GetField("GoogleMap"),
                        CountryCode = csvReader.GetField("CountryCode"),
                        CountryCodeAndroid = csvReader.GetField("CountryCodeAndroid"),
                        CountryCodeiOS = csvReader.GetField("CountryCodeiOS"),
                        City = csvReader.GetField("City"),
                        WorkingTime = csvReader.GetField("WorkingTime"),
                        Description = csvReader.GetField("Description"),
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