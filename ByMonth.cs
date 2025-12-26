public class ByMonth()
{
    int couldNotMoveCount = 0;
    public bool GroupByMonth(string sourceDirectoryPath)
    {
        new FlattenHelper().Flatten(sourceDirectoryPath);

        DateHelper.FixDates(sourceDirectoryPath);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Grouping {sourceDirectoryPath} by month.");
        Console.ResetColor();

        int byMonthCount = 0;
        int byMonthMovedCount = 0;

        // Rename all directories from MMMM to MM_MMMM
        foreach (string folder in Directory.GetDirectories(sourceDirectoryPath))
        {
            DateTime folderDate;
            if (DateTime.TryParseExact(Path.GetFileName(folder), "MMMM", null, System.Globalization.DateTimeStyles.None, out folderDate))
            {
                var newFolderName = folderDate.ToString("MM_MMMM");
                var newFolderPath = Path.Combine(sourceDirectoryPath, newFolderName);
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.Move(folder, newFolderPath);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Renamed folder {folder} to {newFolderPath}");
                    Console.ResetColor();
                }
            }
        }

        var filesToMove = Directory.GetFiles(sourceDirectoryPath).Length;
        foreach (string path in Directory.GetFiles(sourceDirectoryPath))
        {
            try
            {
                byMonthCount++;

                DateTime takenDate = DateHelper.GetFileCreationDate(path);

                var month = takenDate.ToString("MM_MMMM");
                var monthPath = Path.Combine(sourceDirectoryPath, month);

                if (!Path.Exists(month)) Directory.CreateDirectory(monthPath);

                File.Move(path, Path.Combine(monthPath, Path.GetFileName(path)));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Moved: {Path.GetFileName(path)} with date {takenDate} to {monthPath}");
                Console.ResetColor();

                byMonthMovedCount++;

                var filesLeftToMove = filesToMove - byMonthMovedCount;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Files left to move: {filesLeftToMove}");
                Console.ResetColor();

            }
            catch (Exception ex)
            {
                couldNotMoveCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error moving {path}: {ex.Message}");
                Console.ResetColor();
            }
        }


        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("By Month Totals");
        Console.WriteLine($"File Count: {byMonthCount}");
        Console.WriteLine($"Moved Count: {byMonthMovedCount}");
        Console.WriteLine($"Could Not Move Count: {couldNotMoveCount}");
        Console.ResetColor();

        return true;
    }
}