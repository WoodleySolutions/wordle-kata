namespace UnitTests;

using Application;

public class WordleEngineTests
{
    private readonly Wordle _wordle = new Wordle();

    [Theory] //Answer, Guess, Expected
    [InlineData("PLANE", "CHART", "--G--")]
    [InlineData("PLANE", "PLANE", "GGGGG")]
    [InlineData("PLANE", "DITCH", "-----")]
    [InlineData("PLANE", "WORLD", "---Y-")]
    [InlineData("PLANE", "LEVER", "YY---")]
    [InlineData("PLANE", "ELATE", "-GG-G")]
    public void WordleGuess_ReturnsExpectedResult(string answer, string guess, string expected)
    {
        Assert.Equal(expected, _wordle.Guess(answer, guess));
    }
}
