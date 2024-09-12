namespace Chirp.CLI.SimpleDB.Tests;

public class UnitTest1
{
    [Fact]
    public void readTest()
    {
    //arange 
    const string tempCsv = "tempTestFile.csv";
    IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(tempCsv);
    Cheep cheep = new Cheep("bobo","Helooo", 1725366268);
    
    // //act 
    // database.Store(cheep);
    var record = database.Read(); 

    //assert
    foreach (var output in record)
        {
            Assert.Equal(output,cheep);
        }
    
    }

    [Fact]
    public void storeTest() 
    {
    //arange 
    const string tempCsv = "tempTestFile.csv";
    IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(tempCsv);
    

    //act 


    //assert    
    }
}