namespace Chirp.CLI.SimpleDB.Tests;

public class UnitTest1
{
    
    [Fact]
    public void storeTest() 
    {
        //arange 
        const string tempCsv = "/Users/jovananovovic/Documents/Chirp/tests/Chirp.CLI.CSVDatabase.Tests/tempTestFile.csv";
        IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(tempCsv);
        Cheep cheep = new Cheep("juju","Hello kitti <3 ;)", 1690979858);

        //act
        database.Store(cheep);
        var record = database.Read();

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
        IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(tempCsv);
        Cheep cheep = new Cheep("juju","Hello kitti <3 ;)", 1690979858);
        
        // //act 
        var record = database.Read(); 

        //assert
        foreach (var output in record)
        {
            Assert.Equal(cheep,output);
        }
    
    }
    
}