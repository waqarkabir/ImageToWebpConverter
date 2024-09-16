using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using WebApp.Models; // Update this namespace to your actual namespace

namespace YourNamespace.Controllers // Update this namespace to your actual namespace
{
    public class ImageConversionController : Controller
    {
        private readonly HttpClient _httpClient;

        public ImageConversionController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: ImageConversion/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: ImageConversion/Index
        [HttpPost]
        public async Task<IActionResult> Index(ImageConversionModel model)
        {
            if (model.Files == null || model.Files.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload at least one image file.");
                return View(model);
            }

            var requestContent = new MultipartFormDataContent();

            foreach (var file in model.Files)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                requestContent.Add(fileContent, "files", file.FileName);
            }

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsync("https://localhost:7162/api/imageconversion/convert", requestContent);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while sending the request: {ex.Message}");
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Error occurred while converting images.");
                return View(model);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ConvertedFilesResponse>(responseContent);

            if (responseData == null || responseData.ConvertedFiles == null || responseData.ConvertedFiles.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No converted images were returned from the API.");
                return View(model);
            }

            // Store the converted files in TempData
            TempData["ConvertedFiles"] = JsonSerializer.Serialize(responseData.ConvertedFiles);

            return RedirectToAction("DownloadReady");
        }

        // GET: ImageConversion/DownloadReady
        public IActionResult DownloadReady()
        {
            var convertedFilesJson = TempData["ConvertedFiles"] as string;

            if (string.IsNullOrEmpty(convertedFilesJson))
            {
                return RedirectToAction("Index");
            }

            var fileList = JsonSerializer.Deserialize<List<string>>(convertedFilesJson);

            ViewBag.DownloadReady = fileList != null && fileList.Count > 0;
            ViewBag.ConvertedFiles = fileList;

            return View();
        }

        // POST: ImageConversion/Download
        [HttpPost]
        public IActionResult Download(string fileBase64, string fileName)
        {
            if (string.IsNullOrEmpty(fileBase64) || string.IsNullOrEmpty(fileName))
            {
                return RedirectToAction("Index");
            }

            var fileBytes = Convert.FromBase64String(fileBase64);

            return File(fileBytes, "image/webp", fileName);
        }
    }
}
