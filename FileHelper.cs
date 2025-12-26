public static class FileHelper
{
    public static void MoveFile(string destPath, string path, DateTime mediaCreatedDate)
    {
        // move the file
        try
        {
            File.Move(path, Path.Combine(destPath, Path.GetFileName(path)));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Moved: {Path.GetFileName(path)} with date {mediaCreatedDate} to {destPath}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error moving {path}: {ex.Message}");
            Console.ResetColor();
        }
    }
}