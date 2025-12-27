public class FlattenHelper()
{
    int couldNotMoveCount = 0;

    public void Flatten(string sourceDirectoryPath)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Reversing...");
        Console.ResetColor();

        var fileCount = 0;
        var fileMovedCount = 0;

        foreach (string folder in Directory.GetDirectories(sourceDirectoryPath).Where(d => !Constants.IgnoredFolders.Any(ig => ig.Equals(Path.GetFileName(d)))))
        {
            foreach (string path in Directory.GetFiles(folder))
            {
                try
                {
                    fileCount++;
                    File.Move(path, Path.Combine(sourceDirectoryPath, Path.GetFileName(path)));
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Moved: {Path.GetFileName(path)} to {sourceDirectoryPath}");
                    Console.ResetColor();
                    fileMovedCount++;
                }
                catch (Exception ex)
                {
                    couldNotMoveCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error moving {path}: {ex.Message}");
                    Console.ResetColor();
                    throw;
                }
            }

            var left = Directory.GetFiles(folder).Length;
            if (left == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Deleting folder {folder}");
                Console.ResetColor();
                Directory.Delete(folder);
            }
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Moved {fileMovedCount} of {fileCount} to base folder");
        Console.WriteLine($"Could not move count: {couldNotMoveCount}");
        Console.ResetColor();
    }
}