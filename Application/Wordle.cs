namespace Application;

public class Wordle
{
    private readonly char green = 'G';
    private readonly char yellow = 'Y';
    private readonly char gray = '-';

    private List<(string answer, string guess, string expected)> guessHistory = new();

    public new List<(string answer, string guess, string expected)> GetGuessHistory()
    {
        return guessHistory;
    }

    public string Guess(string answer, string guess)
    {
        char[] result = [gray, gray, gray, gray, gray];

        List<char> usedChars = new();

        for (int i = 0; i < answer.Length; i++)
        {
            if (guess[i] == answer[i])
            {
                result[i] = green;
                if (usedChars.Contains(guess[i]))
                {
                    var index = Array.IndexOf(guess.ToCharArray(), guess[i]);
                    result[index] = gray;
                }
            }
            else if (usedChars.Count(c => c == guess[i]) < answer.Count(c => c == guess[i]))
            {
                result[i] = yellow;
                usedChars.Add(guess[i]);
            }
        }
        var resultString = new string(result);

        guessHistory.Add(new(answer, guess, resultString));

        return resultString;
    }
}
