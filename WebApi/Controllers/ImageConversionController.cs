using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageConversionController : ControllerBase
    {
        [HttpPost("convert")]
        public async Task<IActionResult> ConvertToWebP([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("Please upload at least one image file.");
            }

            var convertedFiles = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using (var stream = file.OpenReadStream())
                    {
                        try
                        {
                            using (Image image = Image.Load(stream))
                            {
                                var outputStream = new MemoryStream();
                                image.Save(outputStream, new WebpEncoder());
                                outputStream.Position = 0;

                                // Convert the stream to a byte array for returning
                                var fileBytes = outputStream.ToArray();

                                var base64File = Convert.ToBase64String(fileBytes);
                                convertedFiles.Add(base64File);
                            }
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, $"Error converting file {file.FileName}: {ex.Message}");
                        }
                    }
                }
            }

            return Ok(new { ConvertedFiles = convertedFiles });
        }
    }
}
