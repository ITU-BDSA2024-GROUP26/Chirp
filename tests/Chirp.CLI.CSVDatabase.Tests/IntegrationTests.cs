namespace Chirp.CLI.SimpleDB.Tests;
using System.IO;

public class UnitTest1
{
    
    [Fact]
    public void StoreTest() 
    {
        //arange 
        const string tempCsv = "tempTestFile.csv";
        FileInfo fInfo = new(tempCsv);
        if(fInfo.Exists) {
            fInfo.Delete();
        }
        // we are NOT testing whether the file is created in the proper manner at the moment
        // so we ensure that the headers are correct
        using (var writer = fInfo.AppendText()) 
        {
            writer.WriteLine("Author,Message,Timestamp");
            writer.Flush(); //ensures we write
        }


        CSVDatabase<Cheep>.SetPath(fInfo.FullName);
        Cheep cheep = new Cheep("juju2","Hello kitti <3 ;)", 1690979858);

        //act
        CSVDatabase<Cheep>.getInstance().Store(cheep);
        var record = CSVDatabase<Cheep>.getInstance().Read();

        //assert
        foreach (var output in record)
        {
            Assert.Equal(cheep, output);
        }
    }
    
    [Theory]
    [InlineData("ropf", "Hello, BDSA students!", 1690891760)]
    public void readTest(string author, string message, int timestamp)
    {
        string[] testAuthors = { "adho", "adho", "ropf"};
        string[] testMessages = {
            "\"Welcome to the course!\"", 
            "\"I hope you had a good summer.\"", 
            "\"Cheeping cheeps on Chirp :)\""
        };
        int[] timestamps = { 1690978778,  1690979858,  1690981487 };
        //arange 
        //arange 
        const string tempCsv = "tempTestFile.csv";
        FileInfo fInfo = new(tempCsv);
        if(fInfo.Exists) {
            fInfo.Delete();
        }
        List<Cheep> cheeps = new List<Cheep>(); 
        // we are NOT testing whether the file is created in the proper manner at the moment
        // so we ensure that the headers are correct
        using var writer = new StreamWriter(fInfo.FullName, true);
        using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) {
            ShouldQuote = (args) => false, // Ensures that we don't end up with double qoutes around qouted text  
            HasHeaderRecord = false // ensures that we don't write additional headers inside the file
        });
        for(int i = 0; i < testAuthors.Length; i++) {
            cheeps.Add(new Cheep(testAuthors[i], testMessages[i], timestamps[i]));
            csvWriter.WriteRecord(cheeps.Last());
        }
        cheeps.Add(new Cheep(author, message, timestamp));
        csvWriter.WriteRecord(cheeps.Last());
        csvWriter.NextRecord();

        CSVDatabase<Cheep>.SetPath(tempCsv);
        //act 
        var records = CSVDatabase<Cheep>.getInstance().Read();
        //assert
        int k = 0;
        foreach (var output in records)
        {
            Assert.Equal(cheeps[k], output);
            k++;
        }
    
    }
    
}