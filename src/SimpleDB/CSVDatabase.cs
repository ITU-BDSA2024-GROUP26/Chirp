namespace SimpleDB;

using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

// sealed ensures you cannot create further subclasses of CSVDatabase
public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private static CSVDatabase<T> instance; 
    private static string csvPath;
    private readonly CsvConfiguration csvWriterConfig;
    private readonly CsvConfiguration csvReaderConfig;

    private CSVDatabase()
    {
        if (csvPath == null)
        {
            throw new System.Exception("CSV Path not set");
        }
        // invariant culture ensures proper parsing of decimal numbers(with .) as well as timestamps
        this.csvWriterConfig = new CsvConfiguration(CultureInfo.InvariantCulture) {
            ShouldQuote = (args) => false, // Ensures that we don't end up with double qoutes around qouted text  
            HasHeaderRecord = false // ensures that we don't write additional headers inside the file
        };
        csvReaderConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
    }

    public static CSVDatabase<T> getInstance()
    {
        
        if (instance == null)
        {
            instance = new CSVDatabase<T>();
        }
        return instance;
    }

    public static void SetPath(string path)
    {
        csvPath = path;
        
        // Check if the file exists, if not, create it and add the header
        if(!File.Exists(csvPath))
        {
            using (var writer = new StreamWriter(csvPath, false)) // Create new file (overwrite if exists)
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                // Write the fixed header "Author, Message, Timestamp"
                writer.WriteLine("Author,Message,Timestamp");
                writer.Flush(); // Ensure that the header is written to the file
            }
        }
    }
    
    // Checks if the header exists and returns true if it does, false otherwise
    private bool CheckHeader()
    {
        if (!File.Exists(csvPath)) return false; // File doesn't exist, so no header

        using var reader = new StreamReader(csvPath);
        string firstLine = reader.ReadLine();

        // Check if the first line is equal to the expected header
        return firstLine == "Author,Message,Timestamp";
    }

    // Method to clear the file and write the correct header if the header is missing
    private void EnsureHeader()
    {
        if (!CheckHeader()) // If the header is missing
        {
            using (var writer = new StreamWriter(csvPath, false)) // Overwrite the file
            {
                // Write the correct header
                writer.WriteLine("Author,Message,Timestamp");
                writer.Flush(); // Ensure header is written
            }
        }
    }

    public IEnumerable<T> Read(int? limit = null)
    {
        using (var reader = new StreamReader(csvPath)) 
        using (CsvReader csv = new CsvReader(reader, csvReaderConfig))
        {
            if(limit == null) { return csv.GetRecords<T>().ToList<T>(); }

            List<T> retArr = new List<T>(); 

            for(int i = 0; i < limit; i++) {
                retArr.Add(csv.GetRecord<T>());
            }

            return retArr.ToList<T>();
        }
    }

    public void Store(T record)
    {
        EnsureHeader(); // Ensure the correct header is present before storing a cheep
        using var writer = new StreamWriter(csvPath, true);
        using var csvWriter = new CsvWriter(writer, csvWriterConfig);
        csvWriter.WriteRecord<T>(record);
        csvWriter.NextRecord();
    }

    public void Store(IEnumerable<T> records)
    {
        EnsureHeader(); // Ensure the correct header is present before storing cheeps
        using var writer = new StreamWriter(csvPath, true);
        using var csvWriter = new CsvWriter(writer, csvWriterConfig);
        csvWriter.WriteRecords<T>(records);
    }
}