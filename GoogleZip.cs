using System.IO.Compression;

public class GoogleZip
{
    private int movedPhotoCount = 0;
    private int movedVideoCount = 0;
    private int deletedCount = 0;
    private int couldNotMoveCount = 0;

    public bool ProcessGoogleZip(string videoDirectoryPath, string photoDirectoryPath, string zipFileName)
    {
        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string zipFilePath = Path.Combine(downloadsPath, zipFileName);

        if (!File.Exists(zipFilePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Zip file not found: {zipFilePath}");
            Console.ResetColor();
            return false;
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Processing Google Zip: {zipFileName}");
        Console.WriteLine($"Extracting to: {downloadsPath}");
        Console.ResetColor();

        try
        {
            // Create directories if they don't exist
            if (!Directory.Exists(videoDirectoryPath))
                Directory.CreateDirectory(videoDirectoryPath);
            if (!Directory.Exists(photoDirectoryPath))
                Directory.CreateDirectory(photoDirectoryPath);

            // Extract the zip file to the Downloads directory
            ZipFile.ExtractToDirectory(zipFilePath, downloadsPath, true);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Extracted {zipFileName}");
            Console.ResetColor();

            // Process all files in the Downloads directory
            ProcessFilesInDownloads(downloadsPath, videoDirectoryPath, photoDirectoryPath);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Photos moved: {movedPhotoCount}");
            Console.WriteLine($"Videos moved: {movedVideoCount}");
            Console.WriteLine($"Files deleted: {deletedCount}");
            Console.WriteLine($"Files that could not be moved: {couldNotMoveCount}");
            Console.ResetColor();

            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error processing zip file: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }

    private void ProcessFilesInDownloads(string downloadsPath, string videoDirectoryPath, string photoDirectoryPath)
    {
        // Get all files recursively
        var allFiles = Directory.GetFiles(downloadsPath, "*.*", SearchOption.AllDirectories);

        foreach (string filePath in allFiles)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                string extension = Path.GetExtension(filePath).ToLower();

                // Move photo files
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".heic")
                {
                    File.Move(filePath, Path.Combine(photoDirectoryPath, fileName), overwrite: true);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Moved photo: {fileName} to {photoDirectoryPath}");
                    Console.ResetColor();
                    movedPhotoCount++;
                }
                // Move video files
                else if (extension == ".mp4" || extension == ".mov" || extension == ".avi" || extension == ".mkv")
                {
                    File.Move(filePath, Path.Combine(videoDirectoryPath, fileName), overwrite: true);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Moved video: {fileName} to {videoDirectoryPath}");
                    Console.ResetColor();
                    movedVideoCount++;
                }
                // Delete specified file types
                else if (extension == ".3gp" || extension == ".json" || extension == ".png")
                {
                    File.Delete(filePath);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Deleted: {fileName}");
                    Console.ResetColor();
                    deletedCount++;
                }
            }
            catch (Exception ex)
            {
                couldNotMoveCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
