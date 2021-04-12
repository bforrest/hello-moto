using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarsRoverImageLoader
{
    internal static class Program
    {
        private const string NASA_URI = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos";
        private const string DATA_FILE = "dates.txt";
        
        private static string ApiKey;
        private static string PhotoSaveRoot;
        private static string ProjectRootPath;
        
        private static HttpClient client;
        
        private static async Task Main(string[] args)
        {
            DefaultForeground = Console.ForegroundColor;
            ErrorColor = ConsoleColor.Red;
            MilestoneColor = ConsoleColor.Green;

            ProjectRootPath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            
            ApiKey = Environment.GetEnvironmentVariable("API_KEY") ?? "DEMO_KEY";
                
            PhotoSaveRoot = Environment.GetEnvironmentVariable("PhotoSaveRoot") ?? Path.Join(ProjectRootPath , "Photos");

            if (!Directory.Exists(PhotoSaveRoot))
            {
                Directory.CreateDirectory(PhotoSaveRoot);
            }

            client = new HttpClient();

            LogMileStone($"Project directory: {ProjectRootPath}");
            LogMileStone($"Photo directory - {PhotoSaveRoot}");

            var expectedPath = Path.Join(ProjectRootPath, DATA_FILE);
            
            if (!File.Exists(expectedPath))
            {
                LogError($"File {DATA_FILE} was not found in current directory {ProjectRootPath}.");
                return;
            }
            
            var inputStrings = await File.ReadAllLinesAsync(expectedPath);

            Console.WriteLine("Parsing dates from source file.");

            var dates = new List<DateTime>();
            
            foreach (var item in inputStrings)
            {
                Console.WriteLine($"validating value {item}");

                if (DateTime.TryParse(item, out var aDate))
                {
                    dates.Add(aDate);
                }
                else
                {
                    LogError($"The value {item} is not a valid date.");
                }
            }
            
            LogMileStone("Text file data validated.");
            
            var tasks = dates.Select(async item =>
            {
                await GetImageList(item);
            });

            await Task.WhenAll(tasks);

            LogMileStone("Image retrieval completed.");
        }

        private static async Task GetImageList(DateTime date)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var uri = new Uri($"{NASA_URI}?earth_date={formattedDate}&api_key={ApiKey}");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            try
            {
                Console.WriteLine($"Fetching images for {formattedDate}.");
                var nasaReply = await client.SendAsync(request);

                if (!nasaReply.IsSuccessStatusCode)
                {
                    LogError($"Request for {formattedDate} returned status code: {nasaReply.StatusCode}");
                    return;
                }

                var responseJson = await nasaReply.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<RoverDtoClasses.Root>(responseJson);

                LogMileStone($"Date {formattedDate} has #{response.photos.Count} photos to retrieve.");

                var tasks = response.photos.Select(async item =>
                {
                    await DownloadImage(item);
                });

                await Task.WhenAll(tasks);
                LogMileStone($"Completed photo list for {formattedDate}!");
            }
            catch (Exception e)
            {
                LogError(e.Message);
                throw;
            }
        }

        private static async Task DownloadImage(RoverDtoClasses.Photo photo)
        {
            var lastSegment = photo.img_src.Split("/").Last();
            var path = Path.Combine(PhotoSaveRoot, lastSegment);

            if (File.Exists(path))
            {
                Console.WriteLine($"File {lastSegment} already stored.");
                return;
            }
            
            var imageReply = await client.GetAsync(photo.img_src);

            if (!imageReply.IsSuccessStatusCode)
            {
                LogError($"Unable to download message for URI {photo.img_src}");
            }
            else
            {
                await using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Inheritable, 4096,
                    FileOptions.Asynchronous))
                {
                    await imageReply.Content.CopyToAsync(fs);
                }
                Console.WriteLine($"Saved file {lastSegment} locally.");
            }
        }
        
        private static void ToggleErrorColor()
        {
            Console.ForegroundColor = Console.ForegroundColor == DefaultForeground ? ErrorColor : DefaultForeground;
        }

        private static void LogError(string message)
        {
            ToggleErrorColor();
            Console.WriteLine(message);
            ToggleErrorColor();
        }

        private static void LogMileStone(string message)
        {
            ToggleMilestone();
            Console.WriteLine(message);
            ToggleMilestone();
        }

        private static void ToggleMilestone()
        {
            Console.ForegroundColor = Console.ForegroundColor == DefaultForeground ? MilestoneColor : DefaultForeground;
        }
        
        private static ConsoleColor DefaultForeground;

        private static ConsoleColor ErrorColor;

        private static ConsoleColor MilestoneColor;
    }
}