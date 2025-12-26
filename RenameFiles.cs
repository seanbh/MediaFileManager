public class RenameFiles()
{
    public bool RenameFilesInPaths(string[] paths, Dictionary<string, string> replacements)
    {
        if (paths == null || paths.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No paths provided for renaming.");
            Console.ResetColor();
            return false;
        }

        if (replacements == null || replacements.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No replacements defined.");
            Console.ResetColor();
            return false;
        }

        int totalRenamed = 0;

        foreach (var directoryPath in paths)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Directory does not exist: {directoryPath}");
                Console.ResetColor();
                continue;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Processing files in: {directoryPath}");
            Console.ResetColor();

            try
            {
                var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

                foreach (var filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);
                    string newFileName = fileName;

                    // Apply each replacement in order
                    foreach (var replacement in replacements)
                    {
                        newFileName = newFileName.Replace(replacement.Key, replacement.Value);
                    }

                    // If the filename changed, rename the file
                    if (newFileName != fileName)
                    {
                        string directoryName = Path.GetDirectoryName(filePath);
                        string newFilePath = Path.Combine(directoryName, newFileName);

                        try
                        {
                            // Check if the target file already exists
                            if (File.Exists(newFilePath))
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"File already exists, skipping: {fileName} -> {newFileName}");
                                Console.ResetColor();
                            }
                            else
                            {
                                File.Move(filePath, newFilePath);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Renamed: {fileName} -> {newFileName}");
                                Console.ResetColor();
                                totalRenamed++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error renaming {fileName}: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error processing directory {directoryPath}: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Total files renamed: {totalRenamed}");
        Console.ResetColor();

        return true;
    }
}
