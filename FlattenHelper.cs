public class FlattenHelper()
{
    int couldNotMoveCount = 0;

    public void Flatten(string sourceDirectoryPath)
    {
        Console.WriteLine("Reversing...");

        var fileCount = 0;
        var fileMovedCount = 0;

        foreach (string folder in Directory.GetDirectories(sourceDirectoryPath))
        {
            foreach (string path in Directory.GetFiles(folder))
            {
                try
                {
                    fileCount++;
                    File.Move(path, Path.Combine(sourceDirectoryPath, Path.GetFileName(path)));
                    Console.WriteLine($"Moved: {Path.GetFileName(path)} to {sourceDirectoryPath}");
                    fileMovedCount++;
                }
                catch (Exception ex)
                {
                    couldNotMoveCount++;
                    Console.WriteLine($"Error moving {path}: {ex.Message}");
                    throw;
                }
            }

            var left = Directory.GetFiles(folder).Length;
            if (left == 0)
            {
                Console.WriteLine($"Deleting folder {folder}");
                Directory.Delete(folder);
            }
        }

        Console.WriteLine($"Moved {fileMovedCount} of {fileCount} to base folder");
        Console.WriteLine($"Could not move count: {couldNotMoveCount}");
    }
}