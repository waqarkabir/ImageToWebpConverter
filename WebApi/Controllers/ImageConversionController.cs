using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageConversionController : ControllerBase
    {
        private readonly List<string> _permittedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        private const long _fileSizeLimit = 2 * 1024 * 1024; // 2 MB limit for files

        /// <summary>
        /// Converts the provided images to WebP format.
        /// </summary>
        /// <param name="files">The image files to convert.</param>
        /// <returns>A list of converted images in WebP format.</returns>
        [HttpPost("convert")]
        public async Task<IActionResult> Convert(IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("Please upload at least one image file.");
            }

            var convertedFiles = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > _fileSizeLimit)
                {
                    return BadRequest($"File {file.FileName} exceeds the 2 MB size limit.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_permittedExtensions.Contains(extension))
                {
                    return BadRequest($"File {file.FileName} has an unsupported format.");
                }

                // Save the converted file to a temporary location (or return it as a byte array)
                var convertedFilePath = await ConvertImageToWebP(file);
                convertedFiles.Add(convertedFilePath);
            }

            return Ok(new { ConvertedFiles = convertedFiles });
        }

        private async Task<string> ConvertImageToWebP(IFormFile file)
        {
            var tempPath = Path.GetTempPath();
            var outputFilePath = Path.Combine(tempPath, Path.ChangeExtension(file.FileName, ".webp"));

            try
            {
                using (var inputStream = file.OpenReadStream())
                using (var image = await Image.LoadAsync(inputStream))
                using (var outputStream = new FileStream(outputFilePath, FileMode.Create))
                {
                    var encoder = new WebpEncoder();
                    await image.SaveAsync(outputStream, encoder);
                }
                return outputFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {file.FileName}: {ex.Message}");
            }
        }
    }
}
