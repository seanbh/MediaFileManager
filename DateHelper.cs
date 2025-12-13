using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Shell;

public static class DateHelper
{
    public static DateTime GetFileCreationDate(string filePath)
    {
        return File.GetCreationTime(filePath);
    }

    public static void FixDates(string directoryPath)
    {
        Console.WriteLine($"Fixing dates in {directoryPath}");

        foreach (string path in Directory.GetFiles(directoryPath))
        {
            try
            {
                DateTime? mediaCreatedDate = GetDateTakenDate(path);
                Console.WriteLine($"Date taken for {Path.GetFileName(path)}: {mediaCreatedDate}");

                if (!mediaCreatedDate.HasValue)
                {
                    mediaCreatedDate = GetMediaCreatedDate(path);
                    Console.WriteLine($"Media created date for {Path.GetFileName(path)}: {mediaCreatedDate}");
                }

                if (!mediaCreatedDate.HasValue)
                {
                    mediaCreatedDate = GetDateUsingExif(path);
                    Console.WriteLine($"Exif date for {Path.GetFileName(path)}: {mediaCreatedDate}");
                }

                if (mediaCreatedDate.HasValue)
                {
                    File.SetCreationTime(path, mediaCreatedDate.Value);
                    File.SetLastWriteTime(path, mediaCreatedDate.Value);
                    Console.WriteLine($"Fixed date for: {Path.GetFileName(path)} to {mediaCreatedDate.Value}");

                    string newFileName = $"{mediaCreatedDate.Value:yyyy-MM-dd_HH-mm-ss}_{Path.GetFileName(path)}";
                    string newFilePath = Path.Combine(directoryPath, newFileName);
                    File.Move(path, newFilePath);
                    Console.WriteLine($"Renamed {Path.GetFileName(path)} to {newFileName}");
                }
                else
                {
                    Console.WriteLine($"Could not get date for {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing date for {path}: {ex.Message}");
            }
        }
    }

    private static DateTime? GetMediaCreatedDate(string filePath)
    {
        ShellObject shell = ShellObject.FromParsingName(filePath);

        var data = shell.Properties.System.Media.DateEncoded;

        return data?.Value;
    }

    private static DateTime? GetDateTakenDate(string filePath)
    {
        ShellObject shell = ShellObject.FromParsingName(filePath);

        var data = shell.Properties.System.Photo.DateTaken;

        return data?.Value;
    }

    private static DateTime? GetDateUsingExif(string filePath)
    {
        Console.WriteLine($"Could not get date, trying Exif...");

        string exifToolPath = @"C:\Program Files\ExifTool\exiftool-13.10_64\exiftool.exe";
        try
        {
            // Run ExifTool to update metadata
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exifToolPath,
                    Arguments = $"-overwrite_original \"-FileCreateDate<CreateDate\" \"{filePath}\"",
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            Console.WriteLine($"Updated metadata for: {filePath}");

            var mediaCreatedDate = new FileInfo(filePath).CreationTime;

            // old way
            // Console.WriteLine($"Subtracting {offsetHours} hours from {mediaCreatedDate} for {Path.GetFileName(filePath)}");
            // mediaCreatedDate = mediaCreatedDate.Subtract(new TimeSpan(0, offsetHours, 0, 0));

            // DST-aware offset: choose the timezone that should be applied to the file timestamps.
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            }
            catch
            {
                tz = TimeZoneInfo.Local;
            }

            var creation = DateTime.SpecifyKind(mediaCreatedDate, DateTimeKind.Unspecified);
            var utcOffset = tz.GetUtcOffset(creation);
            Console.WriteLine($"Applying offset {utcOffset.TotalHours} hours for {tz.Id} at {creation} for {Path.GetFileName(filePath)}");
            mediaCreatedDate = creation - utcOffset;

            return mediaCreatedDate;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            return null;
        }
    }


}