using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: YouTubeToMP3Downloader <YouTube Video URL>");
            return;
        }

        string videoUrl = args[0];
        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        try
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(videoUrl);
            var title = video.Title;

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var videoFilePath = Path.Combine(outputDir, $"{title}.mp4");
            var audioFilePath = Path.Combine(outputDir, $"{title}.mp3");

            await youtube.Videos.Streams.DownloadAsync(streamInfo, videoFilePath);

            ConvertToMp3(videoFilePath, audioFilePath);

            Console.WriteLine($"Downloaded and converted to MP3: {audioFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void ConvertToMp3(string inputFile, string outputFile)
    {
        var ffmpegPath = "C:\\Users\\dimip\\Downloads\\ffmpeg-2024-06-09-git-94f2274a8b-essentials_build\\ffmpeg-2024-06-09-git-94f2274a8b-essentials_build\\bin\\ffmpeg"; // Ensure ffmpeg is in your PATH

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = $"-i \"{inputFile}\" -q:a 0 -map a \"{outputFile}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            process.Start();
            process.WaitForExit();
        }

        // Optionally delete the original video file
        File.Delete(inputFile);
    }
}
