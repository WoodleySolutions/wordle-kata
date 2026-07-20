namespace Application;

public class Wordle
{
    private readonly char green = 'G';
    private readonly char yellow = 'Y';
    private readonly char gray = '-';

    private List<(string answer, string guess, string expected)> guessHistory = new();
    public IReadOnlyList<(string answer, string guess, string feedback)> GuessHistory => guessHistory;

    public string Guess(string answer, string guess)
    {
        ValidateInput(answer, guess);

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

        var resultString = new string(result);

        guessHistory.Add((answer, guess, resultString));

        return resultString;
    }

    private static void ValidateInput(string answer, string guess)
    {
        if (guess.Length != answer.Length)
        {
            throw new ArgumentException(guess.Length > answer.Length ? "Invalid Input, Too Long" : "Invalid Input, Too Short");
        }
    }
}
