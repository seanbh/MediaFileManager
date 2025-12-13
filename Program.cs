/*****ALWAYS SET THE VARIABLES BELOW THIS LINE********/
string videoDirectoryPath = @$"C:\Users\seanh\Pictures\Video Projects\Stage\THESE_HAVE_BEEN_COMBINED_INTO_MPEGS\2025";
string photoDirectoryPath = $@"F:\Pictures\2025";
ProcessType processType = ProcessType.FlattenVideosAndPictures;
/******ALWAYS SET THE VARIABLES ABOVE THIS LINE********/

/*****SET THESE BELOW IF EndOfYear IS SELECTED********/
int year = 2025; // you still have to change vacation dates manually
// Define the date ranges for deletion
var vacationDates = new List<Tuple<DateTime, DateTime>>()
{
    new(new(year, 1, 24), new(year, 1, 29)),
    new(new(year, 6, 18), new(year, 7, 2)),
    new(new(year, 11, 13), new(year, 11, 16))
};
/******SET THESE ABOVE IF EndOfYear IS SELECTED********/

int fileCount = 0;
int vacationCount = 0;
int janCount = 0;
int aprCount = 0;
int julCount = 0;
int octCount = 0;
int noDateCount = 0;
int couldNotMoveCount = 0;

Confirm();

switch (processType)
{
    case ProcessType.EndOfYear:
        GroupByQuarterAndVacation(videoDirectoryPath);
        GroupByMonth(photoDirectoryPath);
        Console.WriteLine("After yearly video complete, run VideosByMonth to store these videos into month folders.");
        break;
    case ProcessType.VideosByMonth:
        GroupByMonth(videoDirectoryPath);
        break;
    case ProcessType.PicturesByMonth:
        GroupByMonth(photoDirectoryPath);
        break;
    case ProcessType.VideosAndPicturesByMonth:
        GroupByMonth(videoDirectoryPath);
        GroupByMonth(photoDirectoryPath);
        break;
    case ProcessType.FlattenVideos:
        Flatten(videoDirectoryPath);
        break;
    case ProcessType.FlattenPictures:
        Flatten(photoDirectoryPath);
        break;
    case ProcessType.FlattenVideosAndPictures:
        Flatten(videoDirectoryPath);
        Flatten(photoDirectoryPath);
        break;
    case ProcessType.FixDatesOnly:
        DateHelper.FixDates(photoDirectoryPath);
        if (photoDirectoryPath != videoDirectoryPath)
            DateHelper.FixDates(videoDirectoryPath);
        break;
    default:
        break;
}

void MoveFile(string destPath, string path, DateTime mediaCreatedDate)
{
    // move the file
    try
    {
        File.Move(path, Path.Combine(destPath, Path.GetFileName(path)));
        Console.WriteLine($"Moved: {Path.GetFileName(path)} with date {mediaCreatedDate} to {destPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error moving {path}: {ex.Message}");
    }
}


bool GroupByQuarterAndVacation(string sourceDirectoryPath)
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
                    MoveFile(vacationPath, path, mediaCreatedDate);
                    vacationCount++;
                    movedToVacation = true;
                }
            }

            if (!movedToVacation && File.Exists(path))
            {
                if (mediaCreatedDate >= new DateTime(year, 1, 1) && mediaCreatedDate < new DateTime(year, 4, 1))
                {
                    MoveFile(janPath, path, mediaCreatedDate);
                    janCount++;
                }
                else if (mediaCreatedDate >= new DateTime(year, 4, 1) && mediaCreatedDate <= new DateTime(year, 7, 1))
                {
                    MoveFile(aprPath, path, mediaCreatedDate);
                    aprCount++;
                }
                else if (mediaCreatedDate >= new DateTime(year, 7, 1) && mediaCreatedDate <= new DateTime(year, 10, 1))
                {
                    MoveFile(julPath, path, mediaCreatedDate);
                    julCount++;
                }
                else if (mediaCreatedDate >= new DateTime(year, 10, 1) && mediaCreatedDate <= new DateTime(year + 1, 1, 1))
                {
                    MoveFile(octPath, path, mediaCreatedDate);
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

bool GroupByMonth(string sourceDirectoryPath)
{
    DateHelper.FixDates(sourceDirectoryPath);

    Console.WriteLine();
    Console.WriteLine($"Grouping {sourceDirectoryPath} by month.");

    int byMonthCount = 0;
    int byMonthMovedCount = 0;
    int byMonthNoDateCount = 0;

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
    Console.WriteLine($"Could Not Move Count: {byMonthNoDateCount}");

    return true;
}

void Flatten(string sourceDirectoryPath)
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
}

void Confirm()
{
    Console.WriteLine($"Process: {processType}");
    Console.WriteLine($"Videos Path: {videoDirectoryPath}");
    Console.WriteLine($"Photos Path: {photoDirectoryPath}");
    if (processType == ProcessType.EndOfYear)
    {
        Console.WriteLine($"Year: {year}");
        Console.WriteLine("Vacation Dates:");
        foreach (var dateRange in vacationDates)
        {
            Console.WriteLine($"  From {dateRange.Item1.ToShortDateString()} to {dateRange.Item2.ToShortDateString()}");
        }
    }
    Console.WriteLine("Press enter to continue, Ctrl + C to exit.");
    Console.ReadLine();
}


enum ProcessType
{
    EndOfYear,
    VideosByMonth,
    PicturesByMonth,
    VideosAndPicturesByMonth,
    FixDatesOnly,
    FlattenVideos,
    FlattenPictures,
    FlattenVideosAndPictures
}