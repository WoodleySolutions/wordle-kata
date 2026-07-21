namespace Application;

public class Wordle(string answer, bool hardMode = false)
{
    public static readonly IReadOnlyList<string> WordList =
    ["PLANE", "CHART", "ALIEN", "GEESE", "LEVEE", "ELATE", "BEVEL", "DITCH", "WORLD", "OLDEN"];

    private readonly char green = 'G';
    private readonly char yellow = 'Y';
    private readonly char gray = '-';

    private const int MaxGuessCount = 6;

    private List<(string guess, string feedback)> guessHistory = new();
    private readonly string answer = answer;

    public IReadOnlyList<(string guess, string feedback)> GuessHistory => guessHistory;

    public GameStatus Status { get; private set; } = GameStatus.InProgress;
    public string? Solution => Status == GameStatus.InProgress ? null : answer;


    public string Guess(string guess)
    {
        ValidateInput(guess);

        var score = Score(guess);

        guessHistory.Add((guess, score));

        UpdateGameStatus(score);

        return score;
    }

    private string Score(string guess)
    {
        char[] result = [gray, gray, gray, gray, gray];
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
        // Pass 2: non-green guess letters spend the tally, left to right
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

    private void ValidateInput(string guess)
    {
        if (guess.Length != answer.Length)
        {
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

    private void EnsureHardModeCompliance(string guess)
    {
        if (!hardMode) return;

        foreach (var (previousGuess, feedback) in guessHistory)
        {
            for (int i = 0; i < feedback.Length; i++)
            {
                if (feedback[i] == green && guess[i] != previousGuess[i])
                {
                    throw new ArgumentException($"Hard mode: {previousGuess[i]} must be in position {i + 1}");
                }
                if (feedback[i] == yellow && !guess.Contains(previousGuess[i]))
                {
                    throw new ArgumentException($"Hard mode: guess must contain {previousGuess[i]}");
                }
            }
        }
    }

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

    public static Wordle GenerateRandomWordle(bool hardMode = false, Random? rng = null)
    {
        rng ??= Random.Shared;
        return new Wordle(WordList[rng.Next(WordList.Count)], hardMode);
    }
}

public enum GameStatus
{
    InProgress,
    Won,
    Lost
}
