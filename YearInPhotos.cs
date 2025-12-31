using System.Text.RegularExpressions;

public class YearInPhotos()
{
    public void CopyNthFile(string sourceDirectoryPath, int skip, int minuteInterval, bool dryRun = false)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        var destinationDirectoryPath = Path.Combine(@"C:\Users\seanh\Pictures\Video Projects\Stage\YearInPhotos", Path.GetFileName(sourceDirectoryPath));
        Console.WriteLine($"Copying every {skip}th file from {sourceDirectoryPath} to {destinationDirectoryPath}");
        Console.ResetColor();

        // Create destination directory if it doesn't exist
        if (!Directory.Exists(destinationDirectoryPath))
        {
            Directory.CreateDirectory(destinationDirectoryPath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Created destination directory: {destinationDirectoryPath}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            // delete existing contents
            Console.WriteLine($"Destination directory already exists: {destinationDirectoryPath}. Deleting existing contents...");
            Directory.Delete(destinationDirectoryPath, true); // true = recursive
            Directory.CreateDirectory(destinationDirectoryPath);
            Console.ResetColor();
        }

        // Create initial volume directory (volumes will be created dynamically as needed)
        string vol1Path = Path.Combine(destinationDirectoryPath, "Vol1");
        Directory.CreateDirectory(vol1Path);

        // Get all files recursively and filter by directory name starting with 2 digits and file extension is jpg/jpeg
        var files = Directory.GetFiles(sourceDirectoryPath, "*.*", SearchOption.AllDirectories)
            .Where(f =>
            {
                var directoryName = new DirectoryInfo(Path.GetDirectoryName(f)).Name;
                var extension = Path.GetExtension(f).ToLower();
                return Regex.IsMatch(directoryName, @"^\d{2}") && (extension == ".jpg" || extension == ".jpeg");
            })
            .OrderBy(f => f)
            .ToArray();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Found {files.Length} files matching criteria");
        Console.ResetColor();

        int copiedCount = 0;
        int volumeCounter = 0; // Counter for files in current volume
        int currentVolume = 1; // Current volume
        const long minFileSize = 250 * 1024; // 250 KB in bytes
        const int picturesPerVolume = 300; // Fixed number of pictures per volume
        DateTime lastFileDate = DateTime.MinValue;

        for (int idx = 0; idx < files.Length; idx += skip)
        {
            try
            {
                var fileInfo = new FileInfo(files[idx]);

                if (fileInfo.Length >= minFileSize)
                {
                    DateTime fileDate = DateHelper.GetFileCreationDate(files[idx]);

                    // smaller interval on Christmas Day
                    var minuteIntervalToUse = fileDate.Month == 12 && fileDate.Day == 25 ? 1 : minuteInterval;

                    if (fileDate.Subtract(lastFileDate).TotalMinutes > minuteIntervalToUse)
                    {
                        // Switch to next volume if counter reaches 300
                        if (volumeCounter >= picturesPerVolume)
                        {
                            currentVolume++;
                            volumeCounter = 0;
                        }

                        // Create volume directory dynamically if it doesn't exist
                        string volumeDir = Path.Combine(destinationDirectoryPath, $"Vol{currentVolume}");
                        if (!Directory.Exists(volumeDir))
                        {
                            Directory.CreateDirectory(volumeDir);
                        }

                        string destinationPath = Path.Combine(volumeDir, fileInfo.Name);

                        if (!dryRun)
                        {
                            File.Copy(files[idx], destinationPath, overwrite: true);
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Copied: {files[idx]} ({FormatFileSize(fileInfo.Length)}) -> Vol{currentVolume}");
                        Console.ResetColor();
                        copiedCount++;
                        volumeCounter++;

                        lastFileDate = fileDate;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Skipped (less than {minuteInterval} minutes since previous): {files[idx]}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Skipped (too small): {files[idx]} ({FormatFileSize(fileInfo.Length)})");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error copying {files[idx]}: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Total files copied: {copiedCount}");
        Console.ResetColor();
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
