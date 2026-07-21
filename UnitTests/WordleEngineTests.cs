namespace UnitTests;

using Application;

/// <summary>
/// Unit tests for the <see cref="Wordle"/> game engine, organized in rough order of
/// feature depth: scoring, history tracking, input validation, game lifecycle
/// (win/loss), hard mode, and random game generation.
/// </summary>
/// <remarks>
/// Scoring tests deliberately lean on duplicate-letter words (GEESE, LEVEE, ELATE)
/// because duplicate handling is where naive Wordle implementations most often go
/// wrong. Feedback strings use 'G' = green, 'Y' = yellow, '-' = gray.
/// </remarks>
public class WordleEngineTests
{
    /// <summary>
    /// Verifies the two-pass scoring algorithm across representative cases:
    /// no matches, all matches, and — most importantly — duplicate letters,
    /// where yellows must never exceed the count of unclaimed answer letters.
    /// e.g. "EEEEE" vs ELATE yields exactly two hits ("G---G"), not five.
    /// </summary>
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

    /// <summary>
    /// GuessHistory must record every accepted guess with its feedback,
    /// in the order the guesses were made.
    /// </summary>
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

    /// <summary>
    /// Malformed guesses must be rejected with specific, player-facing messages.
    /// Length problems distinguish too-long from too-short; any non-letter
    /// character (digits, punctuation) is rejected uniformly.
    /// </summary>
    [Theory] //Answer, Guess, Expected ArgumentExceptionMessage
    [InlineData("PLANET", "Invalid Input, Too Long")]
    [InlineData("PLAN", "Invalid Input, Too Short")]
    [InlineData("PLAN3", "Invalid Input, Non-letter characters not allowed")]
    [InlineData("PLA-E", "Invalid Input, Non-letter characters not allowed")]
    public void WordleShouldValidateGuessInput(string guess, string expectedMessage)
    {
        var exception = Assert.Throws<ArgumentException>(() => new Wordle("PLANE").Guess(guess));
        Assert.Equal(expectedMessage, exception.Message);
    }

    /// <summary>
    /// A rejected guess must not appear in history or burn one of the player's
    /// six attempts — validation happens before the guess is recorded.
    /// </summary>
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

        // Only the valid guesses should have been recorded.
        var expectedValidGuesses = guesses.Where(g => !g.expected.StartsWith("Invalid Input")).ToList();
        Assert.Equal(expectedValidGuesses, game.GuessHistory);
    }

    /// <summary>
    /// The engine must accept a full run of six guesses — including a winning
    /// guess on the final attempt — without rejecting the sixth.
    /// </summary>
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

    /// <summary>
    /// A seventh guess after six misses must be refused with an
    /// InvalidOperationException — the game is over, not merely invalid input.
    /// </summary>
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

    /// <summary>
    /// End-to-end win path: Solution stays hidden (null) while in progress,
    /// then a correct guess flips Status to Won and reveals the answer.
    /// </summary>
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

    /// <summary>
    /// End-to-end loss path: the answer remains hidden through guess five,
    /// and only the sixth miss transitions the game to Lost and reveals it.
    /// </summary>
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

    /// <summary>
    /// A won game is closed: further guesses throw "Game already won" rather
    /// than being scored. Distinct message from the loss case by design.
    /// </summary>
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

    /// <summary>
    /// Hard mode: a letter revealed as yellow must appear in every later guess.
    /// The error message names the offending letter to guide the player.
    /// </summary>
    [Fact]
    public void HardModeRequiresYellowLettersInSubsequentGuesses()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("LEVER");                                  // YY--- : L and E are clues now

        var ex = Assert.Throws<ArgumentException>(() => game.Guess("CHART"));
        Assert.Equal("Hard mode: guess must contain L", ex.Message);
    }

    /// <summary>
    /// Hard mode: a letter confirmed green is locked to its position. Merely
    /// including the letter elsewhere is not enough — this matches the official
    /// NYT interpretation of hard mode.
    /// </summary>
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

    /// <summary>
    /// Positive case for hard mode: a guess honoring all accumulated clues
    /// (green position and yellow letters) is accepted and scored normally.
    /// </summary>
    [Fact]
    public void HardModeCompliantGuessIsAccepted()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("LEVER");
        game.Guess("OLDEN");

        Assert.Equal("YG-YY", game.Guess("ALIEN"));           // honors L@2, E, N
    }

    /// <summary>
    /// A hard-mode violation is treated like any other invalid input: it is
    /// rejected before being recorded, so it costs no attempt and leaves the
    /// game in progress.
    /// </summary>
    [Fact]
    public void HardModeViolationsDoNotConsumeAttempts()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("LEVER");

        Assert.Throws<ArgumentException>(() => game.Guess("CHART"));

        Assert.Single(game.GuessHistory);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    /// <summary>
    /// Control test: with hard mode off (the default), players may freely
    /// discard earlier clues — the same guess that hard mode rejects is legal.
    /// </summary>
    [Fact]
    public void NormalModeDoesNotEnforceClueReuse()
    {
        var game = new Wordle("PLANE");
        game.Guess("LEVER");

        Assert.Equal("--G--", game.Guess("CHART"));           // abandoning clues is legal here
    }

    /// <summary>
    /// GenerateRandomWordle must draw its answer from the public WordList.
    /// The game is deliberately lost (six throwaway guesses) because Solution
    /// is only revealed after the game ends. Note "XXXXX" also demonstrates
    /// that guesses need not come from the word list.
    /// </summary>
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

    /// <summary>
    /// The injectable Random parameter makes answer selection deterministic:
    /// two games seeded identically must select the same solution. This is the
    /// hook that keeps randomness testable without exposing the answer directly.
    /// </summary>
    [Fact]
    public void SameSeedProducesSameSolution()
    {
        var game1 = Wordle.GenerateRandomWordle(rng: new Random(42));
        var game2 = Wordle.GenerateRandomWordle(rng: new Random(42));

        for (int i = 0; i < 6; i++) { game1.Guess("XXXXX"); game2.Guess("XXXXX"); }

        Assert.Equal(game1.Solution, game2.Solution);
    }

    /// <summary>
    /// Validation ordering: basic input checks (length, characters) run before
    /// hard-mode checks, so a malformed guess reports the input error even when
    /// it also violates hard-mode constraints.
    /// </summary>
    [Fact]
    public void HardModeStillValidatesInputLengthFirst()
    {
        var game = new Wordle("PLANE", hardMode: true);
        game.Guess("ELATE");                                   // green at position 5

        var ex = Assert.Throws<ArgumentException>(() => game.Guess("PLAN"));
        Assert.Equal("Invalid Input, Too Short", ex.Message);
    }
}