using System.Text.RegularExpressions;

public class YearInPhotos()
{
    public void CopyNthFile(string sourceDirectoryPath, int skip = 3)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        var destinationDirectoryPath = Path.Combine(sourceDirectoryPath, "YearInPhotos");
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

        // Get all files recursively and filter by directory name starting with 2 digits
        var files = Directory.GetFiles(sourceDirectoryPath, "*.*", SearchOption.AllDirectories)
            .Where(f =>
            {
                var directoryName = new DirectoryInfo(Path.GetDirectoryName(f)).Name;
                return Regex.IsMatch(directoryName, @"^\d{2}");
            })
            .OrderBy(f => f)
            .ToArray();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Found {files.Length} files matching criteria");
        Console.ResetColor();

        int copiedCount = 0;
        const long minFileSize = 250 * 1024; // 250 KB in bytes

        for (int idx = 0; idx < files.Length; idx += skip)
        {
            try
            {
                var fileInfo = new FileInfo(files[idx]);

                if (fileInfo.Length >= minFileSize)
                {
                    string destinationPath = Path.Combine(destinationDirectoryPath, fileInfo.Name);

                    File.Copy(files[idx], destinationPath, overwrite: true);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Copied: {files[idx]} ({FormatFileSize(fileInfo.Length)})");
                    Console.ResetColor();
                    copiedCount++;
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
