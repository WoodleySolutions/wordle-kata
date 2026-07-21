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
        var exception = Assert.Throws<InvalidOperationException>(() => game.Guess("WINNR"));
        Assert.Equal("Game already won", exception.Message);
    }

    [Fact]
    public void HardModeRequiresYellowLettersInSubsequentGuesses()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("LEVER");                                  // YY--- : L and E are clues now

        var ex = Assert.Throws<ArgumentException>(() => game.Guess("CHART"));
        Assert.Equal("Hard mode: guess must contain L", ex.Message);
    }

    [Fact]
    public void HardModeRequiresGreenLettersToStayInPosition()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("LEVER");                                  // YY---
        game.Guess("OLDEN");                                  // -G-YY : L locked at position 2

        // LANES contains L, E, N — but L abandoned its position. Official-rules reading.
        var ex = Assert.Throws<ArgumentException>(() => game.Guess("LANES"));
        Assert.Equal("Hard mode: L must be in position 2", ex.Message);
    }

    [Fact]
    public void HardModeCompliantGuessIsAccepted()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("LEVER");
        game.Guess("OLDEN");

        Assert.Equal("YG-YY", game.Guess("ALIEN"));           // honors L@2, E, N
    }

    [Fact]
    public void HardModeViolationsDoNotConsumeAttempts()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("LEVER");

        Assert.Throws<ArgumentException>(() => game.Guess("CHART"));

        Assert.Single(game.GuessHistory);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void NormalModeDoesNotEnforceClueReuse()
    {
        var game = new Wordle("PLANE");
        game.Guess("LEVER");

        Assert.Equal("--G--", game.Guess("CHART"));           // abandoning clues is legal here
    }

    [Fact]
    public void RandomWordleSelectsSolutionFromWordList()
    {
        var game = Wordle.GenerateRandomWordle();

        for (int i = 0; i < 6; i++)
        {
            try { game.Guess("XXXXX"); } catch (InvalidOperationException) { break; }
        }

        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Contains(game.Solution, Wordle.WordList);
    }

    [Fact]
    public void SameSeedProducesSameSolution()
    {
        var game1 = Wordle.GenerateRandomWordle(rng: new Random(42));
        var game2 = Wordle.GenerateRandomWordle(rng: new Random(42));

        for (int i = 0; i < 6; i++) { game1.Guess("XXXXX"); game2.Guess("XXXXX"); }

        Assert.Equal(game1.Solution, game2.Solution);
    }

    [Fact]
    public void HardModeStillValidatesInputLengthFirst()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("ELATE");                                   // green at position 5

        var ex = Assert.Throws<ArgumentException>(() => game.Guess("PLAN"));
        Assert.Equal("Invalid Input, Too Short", ex.Message);
    }
}
