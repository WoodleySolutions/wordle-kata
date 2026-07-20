namespace Application;

public class Wordle
{
    private readonly char green = 'G';
    private readonly char yellow = 'Y';
    private readonly char gray = '-';

    public string Guess(string answer, string guess)
    {
        char[] result = [gray, gray, gray, gray, gray];

        List<char> usedChars = new();

        for (int i = 0; i < answer.Length; i++)
        {
            if (guess[i] == answer[i])
            {
                result[i] = green;
            }
            else if (answer.Contains(guess[i]) && !usedChars.Contains(guess[i]))
            {
                result[i] = yellow;
                usedChars.Add(guess[i]);
            }
        }

        return new string(result);
    }
}
