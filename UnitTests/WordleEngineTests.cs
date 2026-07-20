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
    [InlineData("HELLO", "LLAMA", "YY---")]
    [InlineData("GEESE", "EMCEE", "Y--YG")]
    [InlineData("LEVEE", "EAGLE", "Y--YG")]
    [InlineData("ELATE", "EEEEE", "G---G")]
    public void WordleGuess_ReturnsExpectedResult(string answer, string guess, string expected)
    {
        Assert.Equal(expected, _wordle.Guess(answer, guess));
    }

    [Fact]
    public void WordleCanTrackPreviousGuesses()
    {
        //assumptions: it's worthwhile to cache results of previous guesses to avoid recalculating them.

        var guesses = new List<(string answer, string guess, string expected)>
        {
            ("PLANE", "DITCH", "-----"),
            ("PLANE", "CHART", "--G--"),
            ("PLANE", "ELATE", "-GG-G"),
            ("PLANE", "PLANE", "GGGGG")
        };

        foreach (var (answer, guess, expected) in guesses)
        {
            _wordle.Guess(answer, guess);
        }

        var history = _wordle.GetGuessHistory();

        Assert.Equal(guesses, history);
    }
}
