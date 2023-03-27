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
			async Task DownloadYoutubeVd()
			{
				await Console.Out.WriteLineAsync("===========JUCJUB==========");
				Console.WriteLine("Enter the youtube video URL: ");
				string url;
				url = Console.ReadLine();
				if (!string.IsNullOrEmpty(url) || url.Contains("https://www.youtube.com"))
				{
					Console.WriteLine("Initializing download ...");
					var youtube = new YoutubeClient();
					try
					{   //yt explode stuff
						var video = await youtube.Videos.GetAsync(url);
						var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
						var audiostreaminfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(x => x.Bitrate).FirstOrDefault(); //select highest bitrate quality
						var audioStream = await youtube.Videos.Streams.GetAsync(audiostreaminfo);
						//folder and file
						var downladoDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "YT downloader"); //defaults to MyMusic in win
						Directory.CreateDirectory(downladoDir);
						var downloadFile = Path.Combine(downladoDir, $"{video.Title}.mp3");
						await Console.Out.WriteLineAsync($"Saving file to: {downloadFile}");
						await using (var output = File.Create(downloadFile))
						{
							await audioStream.CopyToAsync(output);
						}
						await Console.Out.WriteLineAsync("Done.");
						Console.WriteLine("Doownload another song? (y/n)");
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
			}

			do await DownloadYoutubeVd();
			while (Console.ReadLine() == "y" || Console.ReadLine() == "Y");
		}
	}
}