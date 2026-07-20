namespace UnitTests;

using Application;

public class WordleEngineTests
{
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
        Assert.Equal(expected, new Wordle(answer).Guess(guess));
    }

    [Fact]
    public void WordleCanTrackPreviousGuesses()
    {
        var guesses = new List<(string guess, string expected)>
        {
            ("DITCH", "-----"),
            ("CHART", "--G--"),
            ("ELATE", "-GG-G"),
            ("PLANE", "GGGGG")
        };

        var game = new Wordle("PLANE");

        foreach (var (guess, _) in guesses)
        {
            game.Guess(guess);
        }

        Assert.Equal(guesses, game.GuessHistory);
    }


    [Theory] //Answer, Guess, Expected ArgumentExceptionMessage
    [InlineData("PLANET", "Invalid Input, Too Long")]
    [InlineData("PLAN", "Invalid Input, Too Short")]
    [InlineData("PLAN3", "Invalid Input, Non-letter characters not alloed")]
    [InlineData("PLA-E", "Invalid Input, Non-letter characters not alloed")]
    public void WordleShouldValidateGuessInput(string guess, string expectedMessage)
    {
        var exception = Assert.Throws<ArgumentException>(() => new Wordle("PLANE").Guess(guess));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void WordleInvalidInputShouldNotCountAsGuess()
    {
        var guesses = new List<(string guess, string expected)>
        {
            ("DITCH", "-----"),
            ("CHART", "--G--"),
            ("PLANET", "Invalid Input, Too Long"), // Invalid input
            ("ELATE", "-GG-G"),
            ("PLAN", "Invalid Input, Too Short"), // Invalid input
            ("PLANE", "GGGGG")
        };

        var game = new Wordle("PLANE");

        foreach (var (guess, expected) in guesses)
        {
            if (expected.StartsWith("Invalid Input"))
            {
                Assert.Throws<ArgumentException>(() => game.Guess(guess));
            }
            else
            {
                game.Guess(guess);
            }
        }

        var expectedValidGuesses = guesses.Where(g => !g.expected.StartsWith("Invalid Input")).ToList();
        Assert.Equal(expectedValidGuesses, game.GuessHistory);
    }

    [Fact]
    public void WordleHsSixAttemptsToGuessTheAnswer()
    {
        var answer = "PLANE";
        var guesses = new List<string> { "DITCH", "CHART", "ELATE", "WORLD", "LEVER", "PLANE" };

        var game = new Wordle(answer);

        foreach (var guess in guesses)
        {
            game.Guess(guess);
        }
        Assert.Equal(guesses.Count, game.GuessHistory.Count);
    }

    [Fact]
    public void WordleThrowsAnErrorAfterSixGuesses()
    {
        var answer = "PLANE";
        var guesses = new List<string> { "DITCH", "CHART", "ELATE", "WORLD", "LEVER", "BEVEL", "PLANE" };

        var game = new Wordle(answer);

        game.Guess(guesses[0]);
        game.Guess(guesses[1]);
        game.Guess(guesses[2]);
        game.Guess(guesses[3]);
        game.Guess(guesses[4]);
        game.Guess(guesses[5]);

        var exception = Assert.Throws<InvalidOperationException>(() => game.Guess(guesses[6]));
        Assert.Equal("Maximum number of guesses reached", exception.Message);
    }

    [Fact]
    public void WordleCorrectGuessWinsTheGame()
    {
        var answer = "PLANE";
        var game = new Wordle(answer);

        //check initial state of the game
        Assert.Null(game.Solution);
        Assert.Equal(GameStatus.InProgress, game.Status);

        var feedback = game.Guess(answer);

        //check the state of the game after a correct guess
        Assert.Equal("GGGGG", feedback);
        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(answer, game.Solution);
    }


    [Fact]
    public void WordleSixIncorrectGuessesLosesTheGameAndRevealsAnswer()
    {
        var answer = "PLANE";
        var guesses = new List<string> { "DITCH", "CHART", "ELATE", "WORLD", "LEVER", "BEVEL" };

        var game = new Wordle(answer);

        game.Guess(guesses[0]);
        game.Guess(guesses[1]);
        game.Guess(guesses[2]);
        game.Guess(guesses[3]);
        game.Guess(guesses[4]);

        //Check the state of the game after five incorrect guesses
        Assert.Null(game.Solution);
        Assert.Equal(GameStatus.InProgress, game.Status);

        game.Guess(guesses[5]);

        //Check the state of the game after the sixth incorrect guess
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(answer, game.Solution);

    }

    [Fact]
    public void WordleWinningEndsTheGame()
    {
        var answer = "PLANE";
        var game = new Wordle(answer);

        game.Guess(answer);

        //check the state of the game after a correct guess
        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(answer, game.Solution);

        //check that further guesses are not allowed after winning the game
        var exception = Assert.Throws<InvalidOperationException>(() => game.Guess(game.Guess("WINNR")));
        Assert.Equal("Game already won", exception.Message);
    }
}
