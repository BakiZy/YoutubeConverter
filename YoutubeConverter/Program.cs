using YoutubeExplode;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using YoutubeExplode.Videos.Streams;
using System.Diagnostics.Metrics;

namespace YoutubeConverter
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			async Task DownloadYoutubeVd()
			{
				await Console.Out.WriteLineAsync("===========YTCONVERTER==========");
				Console.WriteLine("Enter the youtube video URL: ");
				string url;
				url = Console.ReadLine();
				if (!string.IsNullOrEmpty(url) && url.Contains("https://www.youtube.com"))
				{
					var youtube = new YoutubeClient();
					try
					{   //yt explode stuff
						var video = await youtube.Videos.GetAsync(url);
						if (video == null)
						{
							Console.WriteLine("Youtube video hasn't been found ");
							Console.Clear();
							await DownloadYoutubeVd();
						}
						else
						{
							Console.WriteLine("Initializing download ...");
							var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
							var audiostreaminfo = streamManifest.GetAudioOnlyStreams().ToList(); //select highest bitrate quality
							Console.WriteLine("Getting vide/sound data: ");
							Dictionary<int, AudioOnlyStreamInfo> listaRez = new Dictionary<int, AudioOnlyStreamInfo>();
							for (int i = 0; i < audiostreaminfo.Count; i++)
							{
								var item = audiostreaminfo[i];
								Console.WriteLine($"number {i}, birate: {item.Bitrate}, size : {item.Size}, type: {item.Container}");
								listaRez.Add(i, item);
							}
							Console.WriteLine("Choose number of file to download");
							var odgovor = Console.ReadLine();

							if (int.TryParse(odgovor, out int selectedIndex) && listaRez.ContainsKey(selectedIndex))
							{
								var selectedStreamInfo = listaRez[selectedIndex];
								var stream = await youtube.Videos.Streams.GetAsync(selectedStreamInfo);

								var downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "YT downloader");
								Directory.CreateDirectory(downloadDir);
								var downloadFile = Path.Combine(downloadDir, $"{video.Title}.mp3");

								await Console.Out.WriteLineAsync($"Saving file to: {downloadFile}");
								await using (var output = File.Create(downloadFile))
								{
									await stream.CopyToAsync(output);
								}
								await Console.Out.WriteLineAsync("Done.");
								Console.WriteLine("Download another song? (y/n)");
							} else
							{
								Console.WriteLine("Invalid selection. Please try again.");
							}

						}
					
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
				else
				{
					Console.WriteLine("Please enter valid URL ...");

				}
			}


			do await DownloadYoutubeVd();
			while (Console.ReadLine() == "y" || Console.ReadLine() == "Y");
		}
	}
}