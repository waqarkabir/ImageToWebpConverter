using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the paths to image files or a directory.");
                return;
            }

            foreach (string path in args)
            {
                if (System.IO.Directory.Exists(path))
                {
                    // If the path is a directory, process all images in the directory
                    ProcessDirectory(path);
                }
                else if (System.IO.File.Exists(path))
                {
                    // If the path is a file, process that single file
                    ConvertImageToWebP(path);
                }
                else
                {
                    Console.WriteLine($"Invalid path: {path}");
                }
            }
        }

        static void ProcessDirectory(string directoryPath)
        {
            // Get all image files in the directory (you can filter by specific extensions if needed)
            string[] imageFiles = System.IO.Directory.GetFiles(directoryPath, "*.*")
                .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string file in imageFiles)
            {
                ConvertImageToWebP(file);
            }
        }

        static void ConvertImageToWebP(string inputFilePath)
        {
            string outputFilePath = System.IO.Path.ChangeExtension(inputFilePath, ".webp");

            try
            {
                // Load the image
                using (Image image = Image.Load(inputFilePath))
                {
                    // Save the image in WebP format
                    image.Save(outputFilePath, new WebpEncoder());
                }

                Console.WriteLine($"Image converted successfully to {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting {inputFilePath}: {ex.Message}");
            }
        }
    }
}
