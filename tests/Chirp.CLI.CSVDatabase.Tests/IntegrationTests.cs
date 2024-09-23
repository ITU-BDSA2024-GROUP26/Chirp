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
    
    [Fact]
    public void readTest()
    {
        
        //arange 
        const string tempCsv = "tempTestFile.csv";
        CSVDatabase<Cheep>.SetPath(tempCsv);
        Cheep cheep = new Cheep("juju","Hello kitti <3 ;)", 1690979858);
        
        // //act 
        var record = CSVDatabase<Cheep>.getInstance().Read();

        //assert
        foreach (var output in record)
        {
            Assert.Equal(cheep,output);
        }
    
    }
    
}