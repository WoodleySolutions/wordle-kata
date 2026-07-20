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
        var guesses = new List<(string answer, string guess, string expected)>
        {
            ("PLANE", "DITCH", "-----"),
            ("PLANE", "CHART", "--G--"),
            ("PLANE", "ELATE", "-GG-G"),
            ("PLANE", "PLANE", "GGGGG")
        };

        foreach (var (answer, guess, _) in guesses)
        {
            _wordle.Guess(answer, guess);
        }

        Assert.Equal(guesses, _wordle.GuessHistory);
    }


    [Theory] //Answer, Guess, Expected ArgumentExceptionMessage
    [InlineData("PLANE", "PLANET", "Invalid Input, Too Long")]
    [InlineData("PLANE", "PLAN", "Invalid Input, Too Short")]
    [InlineData("PLANE", "PLAN3", "Invalid Input, Non-letter characters not alloed")]
    [InlineData("PLANE", "PLA-E", "Invalid Input, Non-letter characters not alloed")]
    public void WordleShouldValidateGuessInput(string answer, string guess, string expectedMessage)
    {
        var exception = Assert.Throws<ArgumentException>(() => _wordle.Guess(answer, guess));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void WordleInvalidInputShouldNotCountAsGuess()
    {
        var guesses = new List<(string answer, string guess, string expected)>
        {
            ("PLANE", "DITCH", "-----"),
            ("PLANE", "CHART", "--G--"),
            ("PLANE", "PLANET", "Invalid Input, Too Long"), // Invalid input
            ("PLANE", "ELATE", "-GG-G"),
            ("PLANE", "PLAN", "Invalid Input, Too Short"), // Invalid input
            ("PLANE", "PLANE", "GGGGG")
        };
        foreach (var (answer, guess, expected) in guesses)
        {
            if (expected.StartsWith("Invalid Input"))
            {
                Assert.Throws<ArgumentException>(() => _wordle.Guess(answer, guess));
            }
            else
            {
                _wordle.Guess(answer, guess);
            }
        }
        var expectedValidGuesses = guesses.Where(g => !g.expected.StartsWith("Invalid Input")).ToList();
        Assert.Equal(expectedValidGuesses, _wordle.GuessHistory);
    }
}
