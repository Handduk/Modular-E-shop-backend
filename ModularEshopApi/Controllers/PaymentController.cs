using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentController(IConfiguration configuration, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _env = env;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("check-env")]
    public IActionResult CheckEnv()
    {
        var apiKey = _configuration["Nets:SecretKey"];
        var checkoutKey = _configuration["Nets:CheckoutKey"];
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        return Ok(new { apiKey, checkoutKey, env });
    }

    [HttpGet("create-payment")]
    public async Task<IActionResult> CreatePayment()
    {
        var secretKey = _configuration["Nets:SecretKey"];
        Console.WriteLine($"Nets Secret Key: {secretKey}");
        if (string.IsNullOrEmpty(secretKey))
        {
            return BadRequest("Missing Nets secret API key in configuration.");
        }

        var filePath = Path.Combine(_env.ContentRootPath, "payload.json");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Payload file not found.");
        }
        var payload = await System.IO.File.ReadAllTextAsync(filePath);

        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "https://test.api.dibspayment.eu/v1/payments");
        request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", secretKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error response from Nets: ");
                Console.WriteLine(responseBody);
                return StatusCode((int)response.StatusCode, responseBody);
            }
            Console.WriteLine("Payment created successfully.");
            Console.WriteLine(responseBody);
            return Content(responseBody, "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception when calling Nets API:");
            Console.WriteLine(ex);
            return StatusCode(500, "Internal Server Error");
        }
    }
}