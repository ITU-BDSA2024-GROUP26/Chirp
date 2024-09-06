namespace SimpleDB;

using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

// sealed ensures you cannot create further subclasses of CSVDatabase
public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private readonly string csvPath;
    private readonly CsvConfiguration csvWriterConfig;
    public CSVDatabase(string csvPath)
    {
        this.csvPath = csvPath;
        this.csvWriterConfig = new CsvConfiguration(CultureInfo.InvariantCulture) {
            ShouldQuote = (args) => false, // Ensures that we don't end up with double qoutes around qouted text  
            HasHeaderRecord = false // ensures that we don't write additional headers inside the file
        };
    }

    public IEnumerable<T> Read(int? limit = null)
    {
        using (var reader = new StreamReader(csvPath)) 
        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
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