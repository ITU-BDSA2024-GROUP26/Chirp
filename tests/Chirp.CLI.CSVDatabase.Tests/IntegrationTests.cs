using System.Globalization;

using System.IO;
namespace Chirp.CLI.SimpleDB.Tests;
using CsvHelper; 
public class IntegrationTests 
{
    
    [Fact]
    public void storeTest() 
    {
        //arange 
        const string tempCsv = "/Users/jovananovovic/Documents/Chirp/tests/Chirp.CLI.CSVDatabase.Tests/tempTestFile.csv";
        CSVDatabase<Cheep>.SetPath(tempCsv);
        Cheep cheep = new Cheep("juju","Hello kitti <3 ;)", 1690979858);

        //act
        CSVDatabase<Cheep>.getInstance().Store(cheep);
        var record = CSVDatabase<Cheep>.getInstance().Read();

        //assert
        foreach (var output in record)
        {
            Assert.Equal(cheep,output);
        }
    }
    
    [Fact]
    public void readTest()
    {
        
        //arange 
        const string tempCsv = "/Users/jovananovovic/Documents/Chirp/tests/Chirp.CLI.CSVDatabase.Tests/tempTestFile.csv";
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
    
    [Fact]
    public void fileTest()
    {
        //arrange
        const string tempCsv = "/Users/jovananovovic/Documents/file.csv";
        CSVDatabase<Cheep>.SetPath(tempCsv);
        Cheep cheep = new Cheep("juju","Hello kitti <3 ;)", 1690979858);
        
        //act
        CSVDatabase<Cheep>.getInstance().Store(cheep);
        var record = CSVDatabase<Cheep>.getInstance().Read();
        
        //assert 
        foreach (var output in record)
        {
            Assert.Equal(cheep,output);
        }
        
    }

    [Fact]
    public void headerTest()
    {
        // making a new cheep 
        Cheep cheep = new Cheep("juju","Hello kitti <3 ;)", 1690979858);
        
        //arrange 
        //inspired by: https://www.youtube.com/watch?v=fRaSeLYYrcQ
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "header.csv"); //Find the path to the newly created file 

        // Open a stream for writing the CSV file
        using (var streamWriter = new StreamWriter(csvPath))
        {
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecord(cheep);// Writing the record
            }
        }
        
        //act
        CSVDatabase<Cheep>.SetPath(csvPath); //Give the database our new path 
        var cheepTwo = new Cheep("jojo", "Hello missi <3 ;)", 1690979858); // new cheep
        CSVDatabase<Cheep>.getInstance().Store(cheepTwo); // store the cheep
        var record = CSVDatabase<Cheep>.getInstance().Read(); // read cheeps 
        
        //assert
        foreach (var output in record) // iterates over the cheeps in the database 
        {
            Assert.Equal(cheepTwo,output); //checks if the first cheep has been deleted, and the output is now the second cheep 
        }
    }
    
}