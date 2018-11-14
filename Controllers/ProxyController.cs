using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DuploAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private static readonly HttpClientHandler handler = new HttpClientHandler();

        private static readonly HttpClient Client = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        private string GetBackendUrl(string proxy)
        {
            Debug.WriteLine(proxy);
            Debug.WriteLine("proxy");
            return Environment.GetEnvironmentVariable($"PROXY_{proxy.ToUpper()}");
        }

        [Authorize]
        [HttpGet]
        [Route("{*url}")]
        public async Task<IActionResult> GetAsync(string url)
        {
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip |
                                                 DecompressionMethods.Deflate;
            }
            var proxy = GetBackendUrl(url.Split("/")[0]);
            if (string.IsNullOrEmpty(proxy))
            {
                return BadRequest(proxy);
            }
            var regex = new Regex(Regex.Escape(url.Split("/")[0]));
            var redirectUrl = regex.Replace(url, proxy, 1);

            string proxyMessage = string.Empty;

            var responseMessage = await Client.GetAsync(redirectUrl + Request.QueryString.Value);

            var properMessage = await ReadContentAsString(responseMessage);
            try
            {
                return StatusCode((int)responseMessage.StatusCode, JToken.Parse(properMessage));
            }
            catch (JsonReaderException)
            {
                return StatusCode((int)responseMessage.StatusCode, properMessage);
            }
        }

        private async Task<string> ReadContentAsString(HttpResponseMessage response)
        {
            // Check whether response is compressed
            if (response.Content.Headers.ContentEncoding.Any(x => x == "gzip"))
            {
                // Decompress manually
                using (var s = await response.Content.ReadAsStreamAsync())
                {
                    using (var decompressed = new GZipStream(s, mode: CompressionMode.Decompress))
                    {
                        using (var rdr = new StreamReader(decompressed))
                        {
                            return await rdr.ReadToEndAsync();
                        }
                    }
                }
            }
            else
            {
                // Use standard implementation if not compressed
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}