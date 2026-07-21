namespace Application;

/// <summary>
/// Represents a single game of Wordle: the player has a fixed number of attempts
/// to guess a hidden word, receiving per-letter feedback after each guess.
/// </summary>
/// <remarks>
/// This class is self-contained and holds all game state (the answer, guess history,
/// and win/loss status). Instantiate it once per game. For a game with a randomly
/// selected answer, use <see cref="GenerateRandomWordle"/> rather than the constructor.
/// </remarks>
/// <param name="answer">The hidden word the player is trying to guess. All guesses are validated against its length.</param>
/// <param name="hardMode">
/// When enabled, every guess must reuse all information revealed so far:
/// confirmed (green) letters must stay in position, and present (yellow) letters must appear somewhere in the guess.
/// </param>
public class Wordle(string answer, bool hardMode = false)
{
    /// <summary>
    /// The pool of candidate answers used by <see cref="GenerateRandomWordle"/>.
    /// Note: guesses are intentionally NOT restricted to this list — any five-letter
    /// string of letters is a legal guess. The list constrains answers only.
    /// It is exposed publicly so callers (and tests) can verify solution membership.
    /// </summary>
    public static readonly IReadOnlyList<string> WordList =
    ["PLANE", "CHART", "ALIEN", "GEESE", "LEVEE", "ELATE", "BEVEL", "DITCH", "WORLD", "OLDEN"];

    // Feedback symbols used in the score string returned to the caller.
    // A score is one character per letter position, e.g. "G-Y--".
    private readonly char green = 'G';   // Correct letter, correct position
    private readonly char yellow = 'Y';  // Letter exists in the answer, wrong position
    private readonly char gray = '-';    // Letter not in the answer (or all copies already accounted for)

    /// <summary>The player loses after this many unsuccessful guesses.</summary>
    private const int MaxGuessCount = 6;

    // Every accepted guess and the feedback it produced, in play order.
    // Also drives hard-mode validation, which replays prior feedback against new guesses.
    private List<(string guess, string feedback)> guessHistory = new();

    // Captured from the primary constructor so it can be referenced throughout the class.
    private readonly string answer = answer;

    /// <summary>
    /// The full sequence of guesses made so far, paired with the feedback each received.
    /// Read-only to callers; the game itself is the only writer.
    /// </summary>
    public IReadOnlyList<(string guess, string feedback)> GuessHistory => guessHistory;

    /// <summary>The current state of the game: in progress, won, or lost.</summary>
    public GameStatus Status { get; private set; } = GameStatus.InProgress;

    /// <summary>
    /// The answer, revealed only once the game has concluded.
    /// Returns <c>null</c> while the game is still in progress so the answer cannot leak mid-game.
    /// </summary>
    public string? Solution => Status == GameStatus.InProgress ? null : answer;

    /// <summary>
    /// Submits a guess, records it, and returns the per-letter feedback string.
    /// </summary>
    /// <param name="guess">The player's guess. Must match the answer's length and contain only letters.</param>
    /// <returns>
    /// A feedback string with one character per position:
    /// 'G' (correct position), 'Y' (in the word, wrong position), or '-' (absent).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the guess is the wrong length, contains non-letter characters,
    /// or violates hard-mode constraints.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the game has already ended.</exception>
    public string Guess(string guess)
    {
        ValidateInput(guess);
        var score = Score(guess);
        guessHistory.Add((guess, score));
        UpdateGameStatus(score);
        return score;
    }

    /// <summary>
    /// Computes the feedback string for a guess using the standard two-pass Wordle algorithm.
    /// </summary>
    /// <remarks>
    /// The two-pass approach exists to handle duplicate letters correctly. Greens are
    /// claimed first; yellows may then only "spend" answer letters that greens did not
    /// consume. This guarantees, for example, that guessing "LEVEL" against "LEVEE"
    /// never reports more copies of a letter than the answer actually contains.
    /// </remarks>
    private string Score(string guess)
    {
        char[] result = [gray, gray, gray, gray, gray];

        // Tally of answer letters still available to be matched as yellows.
        Dictionary<char, int> unmatched = [];

        // Pass 1: mark greens; tally the answer letters greens didn't claim
        for (int i = 0; i < answer.Length; i++)
        {
            if (guess[i] == answer[i])
            {
                result[i] = green;
            }
            else
            {
                unmatched[answer[i]] = unmatched.GetValueOrDefault(answer[i]) + 1;
            }
        }

        // Pass 2: non-green guess letters spend the tally, left to right.
        // Once a letter's tally is exhausted, further copies of it stay gray.
        for (int i = 0; i < answer.Length; i++)
        {
            if (result[i] == green) continue;
            if (unmatched.GetValueOrDefault(guess[i]) > 0)
            {
                result[i] = yellow;
                unmatched[guess[i]]--;
            }
        }
        return new string(result);
    }

    /// <summary>
    /// Rejects malformed guesses and guesses made after the game has ended.
    /// Checks are ordered so the player receives the most actionable message first.
    /// </summary>
    private void ValidateInput(string guess)
    {
        if (guess.Length != answer.Length)
        {
            // Distinguish too-long from too-short so the error message is specific.
            throw new ArgumentException(guess.Length > answer.Length ? "Invalid Input, Too Long" : "Invalid Input, Too Short");
        }
        else if (!guess.All(char.IsLetter))
        {
            throw new ArgumentException("Invalid Input, Non-letter characters not allowed");
        }
        else if (Status == GameStatus.Lost)
        {
            throw new InvalidOperationException("Maximum number of guesses reached");
        }
        else if (Status == GameStatus.Won)
        {
            throw new InvalidOperationException("Game already won");
        }
        EnsureHardModeCompliance(guess);
    }

    /// <summary>
    /// Enforces hard-mode rules by replaying all prior feedback against the new guess:
    /// green letters must remain in their revealed positions, and yellow letters
    /// must appear somewhere in the guess. No-op when hard mode is disabled.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown with a message identifying the first violated constraint.</exception>
    private void EnsureHardModeCompliance(string guess)
    {
        if (!hardMode) return;
        foreach (var (previousGuess, feedback) in guessHistory)
        {
            for (int i = 0; i < feedback.Length; i++)
            {
                // A previously confirmed (green) position must be reused exactly.
                if (feedback[i] == green && guess[i] != previousGuess[i])
                {
                    throw new ArgumentException($"Hard mode: {previousGuess[i]} must be in position {i + 1}");
                }
                // A previously revealed (yellow) letter must appear somewhere in the guess.
                if (feedback[i] == yellow && !guess.Contains(previousGuess[i]))
                {
                    throw new ArgumentException($"Hard mode: guess must contain {previousGuess[i]}");
                }
            }
        }
    }

    /// <summary>
    /// Transitions the game to Won on an all-green score, or to Lost when the
    /// guess limit is exhausted. Called after each accepted guess; the win check
    /// runs first so a correct final guess still counts as a win.
    /// </summary>
    private void UpdateGameStatus(string score)
    {
        if (score == new string(green, answer.Length))
        {
            Status = GameStatus.Won;
        }
        else if (guessHistory.Count >= MaxGuessCount)
        {
            Status = GameStatus.Lost;
        }
    }

    /// <summary>
    /// Creates a new game with an answer drawn at random from <see cref="WordList"/>.
    /// </summary>
    /// <param name="hardMode">Whether the new game enforces hard-mode rules.</param>
    /// <param name="rng">
    /// Optional random source; defaults to <see cref="Random.Shared"/>.
    /// Supplying a seeded instance makes answer selection deterministic, which is useful in tests.
    /// </param>
    public static Wordle GenerateRandomWordle(bool hardMode = false, Random? rng = null)
    {
        rng ??= Random.Shared;
        return new Wordle(WordList[rng.Next(WordList.Count)], hardMode);
    }
}

/// <summary>The lifecycle states of a Wordle game.</summary>
public enum GameStatus
{
    /// <summary>The game is still accepting guesses.</summary>
    InProgress,
    /// <summary>The player matched the answer within the guess limit.</summary>
    Won,
    /// <summary>The player exhausted all guesses without matching the answer.</summary>
    Lost
}