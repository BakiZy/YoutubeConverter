using YoutubeExplode;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using YoutubeExplode.Videos.Streams;
using System.Diagnostics.Metrics;
using FFMpegCore;
using FFMpegCore.Enums;

namespace YoutubeConverter
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			// Set the FFmpeg binary folder
			GlobalFFOptions.Configure(options => options.BinaryFolder = @"C:\ffmpeg\bin");


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
							var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
							var audiostreaminfo = streamManifest.GetAudioOnlyStreams().ToList(); 
							Console.WriteLine("Getting video/sound data: ");
							Dictionary<int, AudioOnlyStreamInfo> listaRez = new Dictionary<int, AudioOnlyStreamInfo>(); 
							for (int i = 0; i < audiostreaminfo.Count; i++)
							{
								//dodajemo dict zbog iteratora po kom cemo birati kvalitet
								var item = audiostreaminfo[i];
								Console.WriteLine($"number {i}, birate: {item.Bitrate}, size : {item.Size}, type: {item.Container}");
								listaRez.Add(i, item);
							}
							Console.WriteLine("Choose number of file to download");
							var answer = Console.ReadLine();

							if (int.TryParse(answer, out int selectedAnswer) && listaRez.ContainsKey(selectedAnswer))
							{
								Console.WriteLine("Initializing download ...");
								var selectedStreamInfo = listaRez[selectedAnswer];
								var stream = await youtube.Videos.Streams.GetAsync(selectedStreamInfo);

								var downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "YT downloader");
								Directory.CreateDirectory(downloadDir);
								var fileToDownload = Path.Combine(downloadDir, $"{video.Title} {selectedAnswer}.mp3");

								await Console.Out.WriteLineAsync($"Saving file to: {fileToDownload}");
								await using (var output = File.Create(fileToDownload))
								{
									await stream.CopyToAsync(output);
								}
								await Console.Out.WriteLineAsync("Done.");
								Console.WriteLine("Do you want to re-encode the file with a custom bitrate? (y/n)");
								string reencodeResponse = Console.ReadLine().ToLower();
								if (reencodeResponse == "y")
								{
									Console.WriteLine("Available audio qualities:");
									var qualities = Enum.GetValues(typeof(AudioQuality)).Cast<AudioQuality>().ToList();
									for (int i = 0; i < qualities.Count; i++)
									{
										Console.WriteLine($"{i}: {qualities[i]}");
									}

									Console.WriteLine("Choose the desired quality number: ");
									string selectedQualityInput = Console.ReadLine();
									if (int.TryParse(selectedQualityInput, out int selectedQualityIndex) && selectedQualityIndex >= 0 && selectedQualityIndex < qualities.Count)
									{
										var selectedQuality = qualities[selectedQualityIndex];
										var reencodedFile = Path.Combine(downloadDir, $"{video.Title}_reencoded_{selectedQuality}.mp3");

										// Re-encode the file using FFMpegCore
										FFMpegArguments
											.FromFileInput(fileToDownload)
											.OutputToFile(reencodedFile, true, options => options
												.WithAudioBitrate(selectedQuality))
											.ProcessSynchronously();

										await Console.Out.WriteLineAsync($"Re-encoded file saved to: {reencodedFile}");
									}
									else
									{
										Console.WriteLine("Invalid bitrate selected.");
									}
								}

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
			while (Console.ReadLine().ToLower() == "y");
		}
	}
}