namespace UnitTests;

using Application;

public class WordleEngineTests
{
    private readonly Wordle _wordle = new Wordle();

    [Theory]
    [InlineData("PLANE", "CHART", "--G--")]
    [InlineData("PLANE", "PLANE", "GGGGG")]
    public void WordleGuess_ReturnsExpectedResult(string answer, string guess, string expected)
    {
        Assert.Equal(expected, _wordle.Guess(answer, guess));
    }
}
