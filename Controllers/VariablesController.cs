using DuploAuth.Models.Entities;
using DuploAuth.Models.ViewModels;
using DuploAuth.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DuploAuth.Controllers
{
    [Route("api/variables")]
    [ApiController]
    public class VariablesController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly EmailRepository _emailSender;

        public VariablesController(ApplicationDbContext applicationDbContext)
        {
            _appDbContext = applicationDbContext;
            _emailSender = new EmailRepository(applicationDbContext);
        }

        [HttpGet("getEmail")]
        public IActionResult GetEmailProperties()
        {
            var emailResults = new Dictionary<string, string>();
            var emailPropertes = _emailSender.EmailProperties();
            foreach (var property in emailPropertes)
            {
                try
                {
                    var result = _appDbContext.Variables.FindAsync(property).Result.Value;
                    emailResults.Add(property, result);
                }
                catch (NullReferenceException) { }
            }
            return Ok(emailResults);
        }

        [HttpPost("updateEmailProps")]
        public async Task<IActionResult> UpdateEmailPropertiesAsync([FromBody] EmailVariablesRequest EmailProps)
        {
            var emailPropertes = _emailSender.EmailProperties();
            foreach (var property in emailPropertes)
            {
                var result = await _appDbContext.Variables.FindAsync(property);
                Debug.WriteLine(property);
                string value = string.Empty;
                try
                {
                    value = EmailProps.GetType().GetProperty(property).GetValue(EmailProps, null).ToString();
                }
                catch (NullReferenceException)
                {
                    continue;
                }
                Debug.WriteLine(value);
                if (result == null)
                {
                    var variable = new VariablesModel() { Name = property, Value = value };
                    var qResult = await _appDbContext.Variables.AddAsync(variable);
                }
                else
                {
                    result.Value = value;
                }
                await _appDbContext.SaveChangesAsync();
            }
            return Ok();
        }
    }
}