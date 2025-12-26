/*****ALWAYS SET THE VARIABLES BELOW THIS LINE********/
string videoDirectoryPath = @$"C:\Users\seanh\Pictures\Video Projects\Stage\THESE_HAVE_BEEN_COMBINED_INTO_MPEGS\2025";
string photoDirectoryPath = $@"C:\Users\seanh\Pictures\Video Projects\Stage\2025 in Pictures";
ProcessType processType = ProcessType.RenameFiles;
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

string googleZipFileName = "Photos-3-001 (2).zip"; // Set this if GoogleZip is selected

/*****SET THESE BELOW IF RenameFiles IS SELECTED********/
string[] renameFilesPaths = new[]
{
    @"F:\Videos\MPEG\2021-2030",
    @"F:\Videos\MPEG\2011-2020",
    @"F:\Videos\MPEG\Vacations - Photos",
    @"G:\Videos\MPEG\2021-2030",
    @"G:\Videos\MPEG\2011-2020",
    @"G:\Videos\MPEG\Vacations - Photos",
     @"E:\2021-2030",
    @"E:\2011-2020",
    @"E:\Vacations - Photos",
}; // Array of paths to process
var fileReplacements = new Dictionary<string, string>
{
    { "Uncut IV", "Vol 4" },
    { "Uncut III", "Vol 3" },
    { "Uncut II", "Vol 2" },
    { "Vol 1I", "Vol 2" },
    { "Vol 2I", "Vol 3" },
    { "Vol 1V", "Vol 4" },
    { "Uncut I", "Vol 1" },
    { "Jan-Mar", "Vol 1" },
    { "Apr-Jun", "Vol 2" },
    { "Jul-Sep", "Vol 3" },
    { "Apr-Sept", "Vol 2-3" },
    { "Oct-Dec", "Vol 4" },
    { "Vol 1_2006 - 2011", "Vacay Photos Vol 1 (2006-2011)" },
    { "Vol 2_2012 - 2017", "Vacay Photos Vol 2 (2012-2017)" },
    { "Vol 3_2018 - 2020", "Vacay Photos Vol 3 (2018-2020)" },
    { "Vol 4_2021", "Vacay Photos Vol 4 (2021)" },
    { "Vol 5_2022", "Vacay Photos Vol 5 (2022)" },
    { "Vol 6_2023", "Vacay Photos Vol 6 (2023)" },
    { "Vol 7_2024", "Vacay Photos Vol 7 (2024)" },
    { "Vol 8_2025", "Vacay Photos Vol 8 (2025)" },
    { " - ", " " }, // Remove ' - ' from filenames
};
// check: Get-ChildItem -Recurse -File
/******SET THESE ABOVE IF RenameFiles IS SELECTED********/

//DateHelper.RemoveDatePrefix(photoDirectoryPath);
Confirm();

switch (processType)
{
    case ProcessType.EndOfYear:
        var endOfYear = new EndOfYear(year, vacationDates);
        endOfYear.GroupByQuarterAndVacation(videoDirectoryPath);
        new ByMonth().GroupByMonth(photoDirectoryPath);
        Console.WriteLine("After yearly video complete, run VideosByMonth to store these videos into month folders.");
        break;
    case ProcessType.VideosByMonth:
        new ByMonth().GroupByMonth(videoDirectoryPath);
        break;
    case ProcessType.PicturesByMonth:
        new ByMonth().GroupByMonth(photoDirectoryPath);
        break;
    case ProcessType.VideosAndPicturesByMonth:
        new ByMonth().GroupByMonth(videoDirectoryPath);
        new ByMonth().GroupByMonth(photoDirectoryPath);
        break;
    case ProcessType.FlattenVideos:
        new FlattenHelper().Flatten(videoDirectoryPath);
        break;
    case ProcessType.FlattenPictures:
        new FlattenHelper().Flatten(photoDirectoryPath);
        break;
    case ProcessType.FlattenVideosAndPictures:
        new FlattenHelper().Flatten(videoDirectoryPath);
        new FlattenHelper().Flatten(photoDirectoryPath);
        break;
    case ProcessType.FixDatesOnly:
        DateHelper.FixDates(photoDirectoryPath);
        if (photoDirectoryPath != videoDirectoryPath)
            DateHelper.FixDates(videoDirectoryPath);
        break;
    case ProcessType.GoogleZip:
        new GoogleZip().ProcessGoogleZip(videoDirectoryPath, photoDirectoryPath, googleZipFileName);
        break;
    case ProcessType.YearInPhotos:
        new YearInPhotos().CopyNthFile(photoDirectoryPath, skip: 4);
        break;
    case ProcessType.RenameFiles:
        new RenameFiles().RenameFilesInPaths(renameFilesPaths, fileReplacements);
        break;
    default:
        break;
}

void Confirm()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Process: {processType}");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Videos Path: {videoDirectoryPath}");
    Console.WriteLine($"Photos Path: {photoDirectoryPath}");
    if (processType == ProcessType.EndOfYear)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Year: {year}");
        Console.WriteLine("Vacation Dates:");
        Console.ForegroundColor = ConsoleColor.Magenta;
        foreach (var dateRange in vacationDates)
        {
            Console.WriteLine($"  From {dateRange.Item1.ToShortDateString()} to {dateRange.Item2.ToShortDateString()}");
        }
    }
    if (processType == ProcessType.GoogleZip)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Zip File: {googleZipFileName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Press enter to continue, Ctrl + C to exit.");
    Console.ResetColor();
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
    FlattenVideosAndPictures,
    GoogleZip,
    YearInPhotos,
    RenameFiles
}