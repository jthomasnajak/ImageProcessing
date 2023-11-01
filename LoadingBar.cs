/** 
 *  <summary>
 *  This class is used to create a loading bar animation in the console.
 *  </summary>
 */
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

    /**
     *  <summary>
     *  This is a constructor for the LoadingBar object.
     *  </summary>
     *  <param name="StartingXPosition"> The initial X position. </param>
     *  <param name="StartingYPosition"> The initial Y position. </param>
     *  <exception cref="ArgumentException">
     *  Thrown if either of the passed values are negative.
     *  </exception>
     */
    public LoadingBar(int StartingXPosition, int StartingYPosition)
    {
        if (StartingXPosition < 0 || StartingYPosition < 0)
        {
            throw new ArgumentException("Passed value for Loading Bar constructor is invalid, cannot have negative position value.");
        }

        TotalProgressPercentage = 0;
        SubProcessProgressPercentage = 0;
        LoadingBarStartXPosition = StartingXPosition;
        LoadingBarStartYPosition = StartingYPosition;

        Draw();
    }


    /**
     *  <summary>
     *  This method outputs the current loading bar information to the console.
     *  </summary>
     */
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


    /**
     *  <summary>
     *  This is a setter function to update the TotalProgressPercentage parameter.
     *  </summary>
     *  <param name="NewTotalProgressPercentage">represents the total progress percentage.</param>
     *  <exception cref="ArgumentException">
     *  Thrown if passed percentage is not between 0 and 100.
     *  </exception>
     *  "
     */
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

    /**
     *  <summary>
     *  This is a setter function to update the SubProcessProgressPercentage parameter.
     *  </summary>
     *  <param name="NewSubProcessProgress">represents the progress for the current process</param>
     *  <exception cref="ArgumentException">
     *  Thrown if passed percentage is not between 0 and 100.
     *  </exception>
     */
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

    /**
     *  <summary>
     *  This is a getter function to retreive the TotalProgressPercentage parameter.
     *  </summary>
     *  <returns>
     *  A double representing the current percentage of tasks that are completed.
     *  </returns>
     */
    public double GetTotalProgress()
    {
        return TotalProgressPercentage;
    }

    /**
     *  <summary>
     *  This is a getter function to retrieve the SubProcessProgressPercentage parameter.
     *  </summary>
     *  <returns>
     *  A double representing the current completion percentage of the current sub task. 
     *  </returns>
     */
    public double GetSubProcessProgress()
    {
        return SubProcessProgressPercentage;
    }
}