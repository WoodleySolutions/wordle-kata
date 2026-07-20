namespace UnitTests;

using Application;

public class WordleEngineTests
{
    private readonly Wordle _wordle = new Wordle();

    [Fact]
    public void WordleCanMarkCorrectLettersGreen()
    {
        Assert.Equal("--G--", _wordle.Guess("PLANE", "CHART"));
    }
}
