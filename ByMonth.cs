public class ByMonth()
{
    int couldNotMoveCount = 0;
    public bool GroupByMonth(string sourceDirectoryPath)
    {
        new FlattenHelper().Flatten(sourceDirectoryPath);

        DateHelper.FixDates(sourceDirectoryPath);

        Console.WriteLine();
        Console.WriteLine($"Grouping {sourceDirectoryPath} by month.");

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
                    Console.WriteLine($"Renamed folder {folder} to {newFolderPath}");
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
                Console.WriteLine($"Moved: {Path.GetFileName(path)} with date {takenDate} to {monthPath}");

                byMonthMovedCount++;

                var filesLeftToMove = filesToMove - byMonthMovedCount;
                Console.WriteLine($"Files left to move: {filesLeftToMove}");

            }
            catch (Exception ex)
            {
                couldNotMoveCount++;
                Console.WriteLine($"Error moving {path}: {ex.Message}");
            }
        }


        Console.WriteLine();
        Console.WriteLine("By Month Totals");
        Console.WriteLine($"File Count: {byMonthCount}");
        Console.WriteLine($"Moved Count: {byMonthMovedCount}");
        Console.WriteLine($"Could Not Move Count: {couldNotMoveCount}");

        return true;
    }
}