using Microsoft.AspNetCore.Mvc;
using ST10257937cldv.Models;
using ST10257937cldv.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ST10257937cldv.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TableService _tableService;
        private readonly BlobService _blobService;
        private readonly FileService _fileService;
        private readonly QueueService _queueService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(
            ILogger<HomeController> logger,
            TableService tableService,
            BlobService blobService,
            FileService fileService,
            QueueService queueService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _tableService = tableService;
            _blobService = blobService;
            _fileService = fileService;
            _queueService = queueService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Add customer profile to Table and SQL
        [HttpPost]
        public async Task<IActionResult> AddCustomerProfile(CustomerProfile profile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _tableService.AddEntityAsync(profile); // Add to Table Storage
                    await _tableService.AddProfileToSqlAsync(profile); // Add to SQL Database
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while adding customer profile.");
                    ModelState.AddModelError("", "Error occurred while adding profile.");
                    return View("Index", profile);
                }
            }
            return View("Index", profile);
        }


        // Upload image to Blob and store in SQL
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            if (imageFile != null)
            {
                try
                {
                    using (var stream = imageFile.OpenReadStream())
                    {
                        // Upload image to Blob storage
                        await _blobService.UploadBlobAsync("customer-images", imageFile.FileName, stream);

                        using (var memoryStream = new MemoryStream())
                        {
                            await imageFile.CopyToAsync(memoryStream);
                            var imageData = memoryStream.ToArray();
                            // Insert image binary data into a service (SQL or other storage)
                            await _blobService.InsertBlobAsync(imageData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while uploading image.");
                    // Handle error: maybe return an error message to the user
                    ModelState.AddModelError("", "Error occurred while uploading the image.");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile contractFile)
        {
            if (contractFile != null)
            {
                try
                {
                    using (var stream = contractFile.OpenReadStream())
                    {
                        // Assuming you want to store the contract file in a blob or file storage
                        await _fileService.UploadFileAsync("contracts", contractFile.FileName, stream);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while uploading the contract.");
                    ModelState.AddModelError("", "Error occurred while uploading the contract.");
                    return RedirectToAction("Index"); // or return an error view if needed
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProcessOrder(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                // Add logic to process the order and send a message to the queue if necessary
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Order ID cannot be empty.");
            return View("Index");
        }

    }
}
