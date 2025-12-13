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
    default:
        break;
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