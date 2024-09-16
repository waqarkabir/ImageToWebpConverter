namespace WebApp.Models
{
    public class ImageConversionModel
    {
        public List<IFormFile> Files { get; set; }
    }

    public class ConvertedFilesResponse
    {
        public List<string> ConvertedFiles { get; set; }
    }
}
