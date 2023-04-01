/* ----------
    This class will be used to create a loading bar animation in the terminal.
---------- */

public class LoadingBar
{
    double TotalProgressPercentage;
    double SubProcessProgressPercentage;

    int CurrentDisplayedTotalPercentage;
    int CurrentDisplayedSubPercentage;

    int LoadingBarStartXPosition;
    int LoadingBarStartYPosition;

    char IncompleteChar = '-';
    char CompleteChar = 'O';

    int NumberOfIncrements = 20;

    public LoadingBar(int StartingXPosition, int StartingYPosition)
    {
        if (StartingXPosition < 0 || StartingYPosition < 0)
        {
            throw new ArgumentException("Passed value for Loading Bar constructor is invalid");
        }

        TotalProgressPercentage = 0;
        SubProcessProgressPercentage = 0;
        LoadingBarStartXPosition = StartingXPosition;
        LoadingBarStartYPosition = StartingYPosition;

        Draw();
    }


    /* -----
        this method outputs the current loading bar information
    ----- */
    public void Draw()
    {
        string TotalProgressBar = "Folder Progress:   [";
        string SubProgressBar = "Image Progress:    [";

        CurrentDisplayedTotalPercentage = 0;
        CurrentDisplayedSubPercentage = 0;

        for (int CurrentLoadingBarIncrement = 0; CurrentLoadingBarIncrement < NumberOfIncrements; CurrentLoadingBarIncrement++)
        {
            if (CurrentLoadingBarIncrement < (TotalProgressPercentage / (100 / NumberOfIncrements)))
            {
                TotalProgressBar += CompleteChar;
                CurrentDisplayedTotalPercentage += (100 / NumberOfIncrements);
            }
            else
            {
                TotalProgressBar += IncompleteChar;
            }

            if (CurrentLoadingBarIncrement < (SubProcessProgressPercentage / (100 / NumberOfIncrements)))
            {
                SubProgressBar += CompleteChar;
                CurrentDisplayedSubPercentage += (100 / NumberOfIncrements);
            }
            else
            {
                SubProgressBar += IncompleteChar;
            }
        }

        TotalProgressBar += "]";
        SubProgressBar += "]";

        Console.CursorVisible = false;
        Console.SetCursorPosition(LoadingBarStartXPosition, LoadingBarStartYPosition);
        Console.WriteLine(TotalProgressBar);
        Console.WriteLine(SubProgressBar);
        Console.CursorVisible = true;
    }



    public void SetTotalProgressPercentage(double NewTotalProgressPercentage)
    {
        if (NewTotalProgressPercentage < 0 || NewTotalProgressPercentage > 100)
        {
            throw new ArgumentException("Passed value " + NewTotalProgressPercentage + " is not a valid percentage. Expected value is between 0 and 100");
        }

        TotalProgressPercentage = NewTotalProgressPercentage;
        if (CurrentDisplayedTotalPercentage != ((int) (NewTotalProgressPercentage / (100 / NumberOfIncrements)) * (100 / NumberOfIncrements)))
        {
            Draw();
        }
    }

    public void SetSubProcessProgressPercentage(double NewSubProcessProgress)
    {
        if (NewSubProcessProgress < 0 || NewSubProcessProgress > 100)
        {
            throw new ArgumentException("Passed value " + NewSubProcessProgress + " is not a valid percentage. Expected value is between 0 and 100");
        }

        SubProcessProgressPercentage = NewSubProcessProgress;
        if (CurrentDisplayedSubPercentage != ((int) (NewSubProcessProgress / (100 / NumberOfIncrements)) * (100 / NumberOfIncrements)))
        {
            Draw();
        }
    }

    public double GetTotalProgress()
    {
        return TotalProgressPercentage;
    }

    public double GetSubProcessProgress()
    {
        return SubProcessProgressPercentage;
    }
}