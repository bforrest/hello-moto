using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarsRoverImageLoader
{
    class Program
    {
        private const string NASA_URI = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos";
        private const string DATA_FILE = "dates.txt";
        
        private static string ApiKey;
        private static string PhotoSaveRoot;
        private static string ProjectRootPath;
        
        private static HttpClient client;
        
        static async Task Main(string[] args)
        {
            DefaultForeground = Console.ForegroundColor;
            ErrorColor = ConsoleColor.Red;
            
            ProjectRootPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            
            ApiKey = Environment.GetEnvironmentVariable("API_KEY") ?? "DEMO_KEY";
                
            PhotoSaveRoot = Environment.GetEnvironmentVariable("PhotoSaveRoot") ?? Path.Join(ProjectRootPath , "Photos");

            if (!Directory.Exists(PhotoSaveRoot))
            {
                Directory.CreateDirectory(PhotoSaveRoot);
            }
            
            client = new HttpClient();

            Console.WriteLine($"Project directory: {ProjectRootPath}");
            

            var expectedPath = Path.Join(ProjectRootPath, DATA_FILE);
            
            if (!File.Exists(expectedPath))
            {
                LogError($"File {DATA_FILE} was not found in current directory {ProjectRootPath}.");
                return;
            }
            
            var inputStrings = File.ReadAllLines(expectedPath);

            Console.WriteLine("Parsing dates from source file.");
            
            foreach (var item in inputStrings)
            {
                Console.WriteLine($"processing value {item}");

                if (DateTime.TryParse(item, out var aDate))
                {
                    await GetImageList(aDate);
                }
                else
                {
                    LogError($"The value {item} is not a valid date.");
                }
            }
            
            Console.WriteLine("Date parsing complete.");
        }

        private static async Task FetchImageList(IEnumerable<DateTime> requestedDates)
        {
            Console.WriteLine("Fetching images for requested dates.");

            foreach (var date in requestedDates)
            {
                await GetImageList(date);
            }
        }

        private static async Task GetImageList(DateTime date)
        {
            var uri = new Uri($"{NASA_URI}?earth_date={date.ToString("yyyy-MM-dd")}&api_key={ApiKey}");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            try
            {
                Console.WriteLine("Fetching images for requested dates.");
                var nasaReply = await client.SendAsync(request);

                if (!nasaReply.IsSuccessStatusCode)
                {
                    LogError($"Request for {date} returned status code: {nasaReply.StatusCode}");
                    return;
                }

                var responseJson = await nasaReply.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<RoverDtoClasses.Root>(responseJson);

                Console.WriteLine($"Date {date} has #{response.photos.Capacity} photos to retrieve.");
                foreach (var photo in response.photos)
                {
                    await DownloadImage(photo);
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
                throw;
            }
            
            Console.WriteLine($"Completed photo list for {date}!");
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
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Inheritable, 4096,
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
        
        private static ConsoleColor DefaultForeground;

        private static ConsoleColor ErrorColor;
        /*
         *  var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    if (responseIsJson)
                    {
                        var responseJson = await httpResponseMessage.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<T>(responseJson);
                        return response;
                    }
                    else if (typeof(T) == typeof(Stream))
                    {
                        return await httpResponseMessage.Content.ReadAsStreamAsync() as T;
                    }

                    return default(T);
                }
         */
    }
}