using YoutubeExplode;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using YoutubeExplode.Videos.Streams;

namespace YoutubeConverter
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			async Task  DownloadYoutubeVd()
			{
                await Console.Out.WriteLineAsync("===========JUCJUB==========");
                Console.WriteLine("Enter the youtube video URL: ");
				var url = Console.ReadLine();
				Console.WriteLine("Initializing download ...");
				//make me an instance of the youtube client
				if (string.IsNullOrEmpty(url) || !url.Contains("https://www.youtube.com"))
				{
					Console.Write("Please enter valid link of video");
					return;
				}
				var youtube = new YoutubeClient();
				try
				{   //yt explode stuff
					var video = await youtube.Videos.GetAsync(url);
					var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
					var audiostreaminfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(x => x.Bitrate).FirstOrDefault();
					var audioStream = await youtube.Videos.Streams.GetAsync(audiostreaminfo);
					//folder and file
					var downladoDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "YT downloader");
					Directory.CreateDirectory(downladoDir);
					var downloadFile = Path.Combine(downladoDir, $"{video.Title}.mp3");
					await Console.Out.WriteLineAsync($"Saving file to: {downloadFile}");
					await using (var output = File.Create(downloadFile))
					{
						await audioStream.CopyToAsync(output);
					}
					await Console.Out.WriteLineAsync("Done.");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}

			await DownloadYoutubeVd();
            await Console.Out.WriteLineAsync("Download another video? (y/n)");
			var answer = Console.ReadLine();
			if (answer.Equals("y") || answer.Equals("Y"))
			{
				await DownloadYoutubeVd();
			}
			Environment.Exit(0);

            //make function that asks for user input and choose 1 if he wants to repeat or 0 to exit console application

        }
	}
}