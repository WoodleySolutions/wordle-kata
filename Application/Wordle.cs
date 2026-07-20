namespace Application;

public class Wordle
{
    private char green = 'G';
    private char yellow = 'Y';
    private char gray = '-';

    public string Guess(string answer, string guess)
    {
        char[] result = [gray, gray, gray, gray, gray];

        for (int i = 0; i < answer.Length; i++)
        {
            if (guess[i] == answer[i])
            {
                result[i] = green;
            }
        }


        return new string(result);
    }
}
