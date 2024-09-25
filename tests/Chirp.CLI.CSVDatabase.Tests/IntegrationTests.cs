namespace Chirp.CLI.SimpleDB.Tests;
using System.IO;

public class UnitTest1
{
    
    [Fact]
    public void StoreTest() 
    {
        //ARRANGE 
        
        // Ensure that we have a csv file that won't contain anything other than what we want it to
        const string tempCsv = "tempTestFile.csv";
        FileInfo fInfo = new(tempCsv);
        if(fInfo.Exists) {
            fInfo.Delete();
        }

        // we are NOT testing whether the file is created in the proper manner at the moment
        // so we ensure that the headers are correct
        // (we should have another unit test to test that functionality)
        using (var writer = fInfo.AppendText()) 
        {
            writer.WriteLine("Author,Message,Timestamp");
            writer.Flush(); //ensures we write
        }


        // create our test data
        CSVDatabase<Cheep>.SetPath(tempCsv);
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
        // ARRANGE

        // first some random test data to ensure that we don't get any fuckery from an empty csv file
        string[] testAuthors = { "adho", "adho", "ropf"};
        string[] testMessages = {
            "\"Welcome to the course!\"", 
            "\"I hope you had a good summer.\"", 
            "\"Cheeping cheeps on Chirp :)\""
        };
        int[] timestamps = { 1690978778,  1690979858,  1690981487 };

        // Then ensure that we create a new file that ONLY contains what we want it to
        const string tempCsv = "tempTestFile.csv";
        FileInfo fInfo = new(tempCsv);
        if(fInfo.Exists) {
            fInfo.Delete();
        }

        // then we create our cheeps and write them to the file
        // they will be indentical UNLESS csvhelper fails, which we assume it doesn't
        List<Cheep> cheeps = new List<Cheep>(); 
        using var writer = new StreamWriter(fInfo.FullName, true);
        // note the csvconfig was stolen from the CSVDatabase code
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

        //act 
        // finally we let CSVDatabase retrieve the files
        // note that the database should be able to find the file in the local directory
        // thus we give it the relative path
        CSVDatabase<Cheep>.SetPath(tempCsv);
        var records = CSVDatabase<Cheep>.getInstance().Read();
        //assert
        // and finally we simply test if we get back out what we put in 
        int k = 0;
        foreach (var output in records)
        {
            Assert.Equal(cheeps[k], output);
            k++;
        }
    
    }
    
}