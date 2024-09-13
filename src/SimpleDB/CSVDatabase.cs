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

    private CSVDatabase() {}

    public static CSVDatabase<T> getInstance()
    {
        
        if (instance == null)
        {
            instance = new CSVDatabase<T>();
        }
        return instance;
    }


    private readonly string csvPath;
    private readonly CsvConfiguration csvWriterConfig;
    private readonly CsvConfiguration csvReaderConfig;


    public CSVDatabase(string csvPath)
    {
        this.csvPath = csvPath;

        // invariant culture ensures proper parsing of decimal numbers(with .) as well as timestamps
        this.csvWriterConfig = new CsvConfiguration(CultureInfo.InvariantCulture) {
            ShouldQuote = (args) => false, // Ensures that we don't end up with double qoutes around qouted text  
            HasHeaderRecord = false // ensures that we don't write additional headers inside the file
        };
        csvReaderConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
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
        using var writer = new StreamWriter(csvPath, true);
        using var csvWriter = new CsvWriter(writer, csvWriterConfig);
        csvWriter.WriteRecord<T>(record);
        csvWriter.NextRecord();
    }

    public void Store(IEnumerable<T> records)
    {
        using var writer = new StreamWriter(csvPath, true);
        using var csvWriter = new CsvWriter(writer, csvWriterConfig);
        csvWriter.WriteRecords<T>(records);
    }
}