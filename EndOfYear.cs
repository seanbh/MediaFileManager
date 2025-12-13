public class EndOfYear
{
    int fileCount = 0;
    int vacationCount = 0;
    int janCount = 0;
    int aprCount = 0;
    int julCount = 0;
    int octCount = 0;
    int noDateCount = 0;
    int couldNotMoveCount = 0;
    private int year;
    private List<Tuple<DateTime, DateTime>> vacationDates;

    public EndOfYear(int year, List<Tuple<DateTime, DateTime>> vacationDates)
    {
        this.year = year;
        this.vacationDates = vacationDates;
    }

    public bool GroupByQuarterAndVacation(string sourceDirectoryPath)
    {
        if (!Path.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Path {sourceDirectoryPath} does not exist");
            return false;
        }

        DateHelper.FixDates(sourceDirectoryPath);

        string vacationPath = Path.Combine(sourceDirectoryPath, "Vacations");
        string janPath = Path.Combine(sourceDirectoryPath, "Jan-Mar");
        string aprPath = Path.Combine(sourceDirectoryPath, "Apr-Jun");
        string julPath = Path.Combine(sourceDirectoryPath, "Jul-Sep");
        string octPath = Path.Combine(sourceDirectoryPath, "Oct-Dec");
        if (!Path.Exists(vacationPath)) Directory.CreateDirectory(vacationPath);
        if (!Path.Exists(janPath)) Directory.CreateDirectory(janPath);
        if (!Path.Exists(aprPath)) Directory.CreateDirectory(aprPath);
        if (!Path.Exists(julPath)) Directory.CreateDirectory(julPath);
        if (!Path.Exists(octPath)) Directory.CreateDirectory(octPath);

        var filesToMove = Directory.GetFiles(sourceDirectoryPath).Length;
        foreach (string path in Directory.GetFiles(sourceDirectoryPath))
        {
            try
            {
                fileCount++;
                var movedToVacation = false;

                DateTime mediaCreatedDate = DateHelper.GetFileCreationDate(path);

                foreach (var dateRange in vacationDates)
                {
                    if (mediaCreatedDate >= dateRange.Item1 && mediaCreatedDate < dateRange.Item2.AddDays(1) && File.Exists(path))
                    {
                        FileHelper.MoveFile(vacationPath, path, mediaCreatedDate);
                        vacationCount++;
                        movedToVacation = true;
                    }
                }

                if (!movedToVacation && File.Exists(path))
                {
                    if (mediaCreatedDate >= new DateTime(year, 1, 1) && mediaCreatedDate < new DateTime(year, 4, 1))
                    {
                        FileHelper.MoveFile(janPath, path, mediaCreatedDate);
                        janCount++;
                    }
                    else if (mediaCreatedDate >= new DateTime(year, 4, 1) && mediaCreatedDate <= new DateTime(year, 7, 1))
                    {
                        FileHelper.MoveFile(aprPath, path, mediaCreatedDate);
                        aprCount++;
                    }
                    else if (mediaCreatedDate >= new DateTime(year, 7, 1) && mediaCreatedDate <= new DateTime(year, 10, 1))
                    {
                        FileHelper.MoveFile(julPath, path, mediaCreatedDate);
                        julCount++;
                    }
                    else if (mediaCreatedDate >= new DateTime(year, 10, 1) && mediaCreatedDate <= new DateTime(year + 1, 1, 1))
                    {
                        FileHelper.MoveFile(octPath, path, mediaCreatedDate);
                        octCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Could not move : {path} with date {mediaCreatedDate}");
                        couldNotMoveCount++;
                    }
                }

            }
            catch (Exception ex)
            {
                couldNotMoveCount++;
                Console.WriteLine($"Error moving {path}: {ex.Message}");
            }
            finally
            {
                var filesLeftToMove = filesToMove - fileCount;
                Console.WriteLine($"Files left to move: {filesLeftToMove}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("By Quarter/Vcacation Totals");
        Console.WriteLine($"File Count: {fileCount}");
        Console.WriteLine($"Vacation File Count: {vacationCount}");
        Console.WriteLine($"Jan-Mar File Count: {janCount}");
        Console.WriteLine($"Apr-Jun File Count: {aprCount}");
        Console.WriteLine($"Jul-Sep File Count: {julCount}");
        Console.WriteLine($"Oct-Dec File Count: {octCount}");
        Console.WriteLine($"No Media Date File Count: {noDateCount}");
        Console.WriteLine($"Could not move File Count: {couldNotMoveCount}");
        Console.WriteLine($"Total Processed: {vacationCount + janCount + aprCount + julCount + octCount + noDateCount + couldNotMoveCount}");

        return true;
    }

}