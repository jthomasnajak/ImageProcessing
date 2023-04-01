/* ----------
    Project: SortingAlgorithmPractice
    Goal: practice writing my own implementations of different sorting algorithms

    ToDo:
    - implement sort algorithms
        - HeapSort
---------- */

public class SortingAlgorithmPractice
{
    /* -----
        this method is an implementation of a bubble sort
    ----- */
    public static int[] BubbleSort(int[] ArrayToSort)
    {
        for (int i = 0; i < (ArrayToSort.Length - 1); i++)
        {
            for (int j = 0; j < ((ArrayToSort.Length - 1) - i); j++)
            {
                if (ArrayToSort[j] > ArrayToSort[j + 1])
                {
                    int TempValue = ArrayToSort[j];
                    ArrayToSort[j] = ArrayToSort[j + 1];
                    ArrayToSort[j + 1] = TempValue;
                }
            }
        }

        return ArrayToSort;
    }


    /* -----
        this method is an implementation of a selection sort
    ----- */
    public static int[] SelectionSort(int[] ArrayToSort)
    {
        int SmallestValueIndex = 0;

        for (int i = 0; i < ArrayToSort.Length; i++)
        {
            SmallestValueIndex = i;
            for (int j = i; j < ArrayToSort.Length; j++)
            {
                if (ArrayToSort[j] < ArrayToSort[SmallestValueIndex])
                {
                    SmallestValueIndex = j;
                }
            }

            int TempValue = ArrayToSort[i];
            ArrayToSort[i] = ArrayToSort[SmallestValueIndex];
            ArrayToSort[SmallestValueIndex] = TempValue;
        }

        return ArrayToSort;
    }


    /* -----
        this method is an implementation of an insertion sort
    ----- */
    public static int[] InsertionSort(int[] ArrayToSort)
    {
        for (int i = 1; i < ArrayToSort.Length; i++)
        {
            int CurrentValue = ArrayToSort[i];

            for (int j = (i - 1); j >= 0; j--)
            {

                if (ArrayToSort[j] > CurrentValue)
                {
                    int TempValue = ArrayToSort[j + 1];
                    ArrayToSort[j + 1] = ArrayToSort[j];
                    ArrayToSort[j] = TempValue;
                }
                else
                {
                    break;
                }
            }
        }

        return ArrayToSort;
    }


    /* -----
        this method is an implementation of a recursive quick sort
    ----- */
    public static int[] QuickSort(int[] ArrayToSort, int LeftIndex, int RightIndex)
    {
        if (LeftIndex < 0 || RightIndex >= ArrayToSort.Length)
        {
            throw new ArgumentException("passed indeces are not within the passed array");
        }

        while(LeftIndex < RightIndex)
        {
            int PivotIndex = QuickSortPartition(ArrayToSort, LeftIndex, RightIndex);
            if ((PivotIndex - LeftIndex) <= (RightIndex - (PivotIndex + 1)))
            {
                QuickSort(ArrayToSort, LeftIndex, PivotIndex);
                LeftIndex = PivotIndex + 1;
            }
            else
            {
                QuickSort(ArrayToSort, PivotIndex + 1, RightIndex);
                RightIndex = PivotIndex;
            }
        }
        return ArrayToSort;
    }

    public static int QuickSortPartition(int[] ArrayToSort, int LeftIndex, int RightIndex)
    {
        //int PivotValue = ArrayToSort[(LeftIndex + RightIndex) / 2];
        int PivotValue = ArrayToSort[LeftIndex];
        int LeftPointer = LeftIndex;
        int RightPointer = RightIndex;

        while (true)
        {
            // if item at LeftPointer is already less than the pivot, it's already sorted.
            while ((LeftPointer < RightIndex) && (ArrayToSort[LeftPointer] < PivotValue)) 
            {
                LeftPointer++;
            }

            // if the item at RightPointer is already greater than the pivot, it's already sorted.
            while ((RightPointer > LeftIndex) && (ArrayToSort[RightPointer] > PivotValue))
            {
                RightPointer--;
            } 

            if (LeftPointer < RightPointer)
            {
                int TempValue = ArrayToSort[LeftPointer];
                ArrayToSort[LeftPointer++] = ArrayToSort[RightPointer];
                ArrayToSort[RightPointer--] = TempValue;
            }
            else
            {
                return RightPointer;
            }
        }
    }    


    /* -----
        this method is an implementation of a recursive merge sort
    ----- */
    public static int[] MergeSort(int[] ArrayToSort)
    {
        if(ArrayToSort.Length == 1)
        {
            return ArrayToSort;
        }
        else
        {
            int[] LeftHalf = new int[ArrayToSort.Length / 2];
            int[] RightHalf = new int[ArrayToSort.Length - (ArrayToSort.Length / 2)];

            for (int Index = 0; Index < ArrayToSort.Length; Index++)
            {
                if (Index < LeftHalf.Length)
                {
                    LeftHalf[Index] = ArrayToSort[Index];
                }
                else
                {
                    RightHalf[Index - (ArrayToSort.Length / 2)] = ArrayToSort[Index];
                }
            }

            ArrayToSort = Merge(MergeSort(LeftHalf), MergeSort(RightHalf));
        }

        return ArrayToSort;
    }

    public static int[] Merge(int[] FirstArray, int[] SecondArray)
    {
        int[] ReturnArray = new int[(FirstArray.Length + SecondArray.Length)];

        int FirstArrayIndex = 0;
        int SecondArrayIndex = 0;
        int ReturnArrayIndex = 0;

        while ((FirstArrayIndex < FirstArray.Length) && (SecondArrayIndex < SecondArray.Length))
        {
            if(FirstArray[FirstArrayIndex] <= SecondArray[SecondArrayIndex])
            {
                ReturnArray[ReturnArrayIndex++] = FirstArray[FirstArrayIndex++];
            }
            else if(SecondArray[SecondArrayIndex] < FirstArray[FirstArrayIndex])
            {
                ReturnArray[ReturnArrayIndex++] = SecondArray[SecondArrayIndex++];
            }
        }

        while (FirstArrayIndex < FirstArray.Length)
        {
            ReturnArray[ReturnArrayIndex++] = FirstArray[FirstArrayIndex++];
        }

        while (SecondArrayIndex < SecondArray.Length)
        {
            ReturnArray[ReturnArrayIndex++] = SecondArray[SecondArrayIndex++];
        }

        return ReturnArray;
    }

    
    /* -----
        this method is an implementation of a bucket sort
    ----- */
    public static int[] BucketSort(int[] ArrayToSort)
    {
        return ArrayToSort;
    }
    
    
    /* -----
        this method is to allow easier testing of my sorting algorithm implementations
    ----- */
    public static void SortTest(int[] ArrayToSort, string SortMethod, Boolean PrintArrays)
    {
        var Timer = new System.Diagnostics.Stopwatch();

        Timer.Start();

        // converts current array into string and writes the string to console
        string PreSortOutput = "[";
        for (int i = 0; i < ArrayToSort.Length - 1; i++)
        {
            PreSortOutput += ArrayToSort[i].ToString() + ", ";
        }
        PreSortOutput += (ArrayToSort[ArrayToSort.Length - 1].ToString() + "]");

        // calls the requested sorting method
        switch (SortMethod)
        {
            case "BubbleSort":
                ArrayToSort = BubbleSort(ArrayToSort);
                break;

            case "SelectionSort":
                ArrayToSort = SelectionSort(ArrayToSort);
                break;

            case "InsertionSort":
                ArrayToSort = InsertionSort(ArrayToSort);
                break;

            case "QuickSort":
                ArrayToSort = QuickSort(ArrayToSort, 0, ArrayToSort.Length - 1);
                break;

            case "MergeSort":
                ArrayToSort = MergeSort(ArrayToSort);
                break;

            default: 
                throw new ArgumentException("requested sort method not found");
        }
        
        // converts current array into string and writes the string to console
        string PostSortOutput = "[";
        for (int i = 0; i < ArrayToSort.Length - 1; i++)
        {
            PostSortOutput += ArrayToSort[i].ToString() + ", ";
        }
        PostSortOutput += (ArrayToSort[ArrayToSort.Length - 1].ToString() + "]");

        Timer.Stop();

        Console.WriteLine("\nSorting Test for " + SortMethod + " is complete");
        if (PrintArrays)
        {
            Console.WriteLine("Passed Array: " + PreSortOutput);
            Console.WriteLine("Sorted Array: " + PostSortOutput);
        }
        Console.WriteLine("Execution Time: " + Timer.ElapsedMilliseconds + " ms\n\n");
    }

}