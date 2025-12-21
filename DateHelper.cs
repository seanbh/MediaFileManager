using System.Diagnostics;
using System.Globalization;
using System.Drawing;
using System.Text.RegularExpressions;

public static class DateHelper
{
    public static DateTime GetFileCreationDate(string filePath)
    {
        try
        {
            string fileName = Path.GetFileName(filePath);

            // Pattern: yyyy-MM-dd_HH-mm-ss_
            var m = Regex.Match(fileName, "^(\\d{4}-\\d{2}-\\d{2}_\\d{2}-\\d{2}-\\d{2})_");
            if (m.Success)
            {
                var ts = m.Groups[1].Value;
                if (DateTime.TryParseExact(ts, "yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    return dt;
                }
            }
        }
        catch
        {
            // Ignore parse errors and fall back to file system timestamp
        }

        return File.GetCreationTime(filePath);
    }

    public static void FixDates(string directoryPath)
    {
        Console.WriteLine($"Fixing dates in {directoryPath}");

        foreach (string path in Directory.GetFiles(directoryPath))
        {
            try
            {
                string fileNameOnly = Path.GetFileName(path);

                // If filename already starts with yyyy-MM-dd_HH-mm-ss_, skip it.
                if (Regex.IsMatch(fileNameOnly, "^\\d{4}-\\d{2}-\\d{2}_\\d{2}-\\d{2}-\\d{2}_"))
                {
                    Console.WriteLine($"Skipping {fileNameOnly}: already starts with timestamp");
                    continue;
                }

                DateTime? mediaCreatedDate = GetDateTaken(path) ?? GetDateUsingExif(path);

                if (mediaCreatedDate.HasValue)
                {
                    File.SetCreationTime(path, mediaCreatedDate.Value);
                    File.SetLastWriteTime(path, mediaCreatedDate.Value);
                    Console.WriteLine($"Fixed date for: {fileNameOnly} to {mediaCreatedDate.Value}");

                    string newFileName = $"{mediaCreatedDate.Value:yyyy-MM-dd_HH-mm-ss}_{fileNameOnly}";
                    string newFilePath = Path.Combine(directoryPath, newFileName);
                    File.Move(path, newFilePath);
                    Console.WriteLine($"Renamed {fileNameOnly} to {newFileName}");
                }
                else
                {
                    Console.WriteLine($"Could not get date for {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing date for {path}: {ex.Message}");
                break;
            }
        }
    }

    public static void RemoveDatePrefix(string directoryPath)
    {
        Console.WriteLine($"Removing date prefixes in {directoryPath}");

        foreach (string path in Directory.GetFiles(directoryPath))
        {
            try
            {
                string fileNameOnly = Path.GetFileName(path);

                // Pattern: yyyy-MM-dd_HH-mm-ss_
                if (!Regex.IsMatch(fileNameOnly, "^\\d{4}-\\d{2}-\\d{2}_\\d{2}-\\d{2}-\\d{2}_"))
                {
                    continue;
                }

                string cleaned = Regex.Replace(fileNameOnly, "^\\d{4}-\\d{2}-\\d{2}_\\d{2}-\\d{2}-\\d{2}_", "");
                string newPath = Path.Combine(directoryPath, cleaned);

                // If target exists, try to find a unique name by appending (n)
                if (File.Exists(newPath))
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(cleaned);
                    string ext = Path.GetExtension(cleaned);
                    int i = 1;
                    string candidate;
                    do
                    {
                        candidate = $"{nameWithoutExt} ({i}){ext}";
                        newPath = Path.Combine(directoryPath, candidate);
                        i++;
                    }
                    while (File.Exists(newPath));
                }

                File.Move(path, newPath);
                Console.WriteLine($"Renamed {fileNameOnly} to {Path.GetFileName(newPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing prefix for {path}: {ex.Message}");
            }
        }
    }

    private static DateTime? GetDateUsingExif(string filePath)
    {
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
            if (!filePath.ToLower().EndsWith(".heic"))
            {
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
                mediaCreatedDate = creation + utcOffset;
            }

            return mediaCreatedDate;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            return null;
        }
    }


    private static DateTime? GetDateTaken(string filePath)
    {
        try
        {
            // Only attempt for image files that System.Drawing can open.
            using var fs = File.OpenRead(filePath);
            using var img = Image.FromStream(fs, false, false);

            const int DateTakenId = 0x9003; // PropertyTagDateTimeOriginal
            if (img.PropertyIdList != null && img.PropertyIdList.Contains(DateTakenId))
            {
                var prop = img.GetPropertyItem(DateTakenId);
                string value = System.Text.Encoding.ASCII.GetString(prop.Value).Trim('\0');
                if (DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    Console.WriteLine($"Extracted Date from DateTakenId Property: {value}");
                    return dt;
                }
                // Try a more general parse as a fallback
                if (DateTime.TryParse(value, out dt))
                {
                    Console.WriteLine($"Extracted Date from DateTakenId Property: {value}");
                    return dt;
                }
            }
        }
        catch
        {
            // Ignore errors — not all files support image metadata.
        }

        return null;
    }


}