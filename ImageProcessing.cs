/**
 *  Project: ImageProcessing
 *  Goal: this project will be a consolidation of my current image projects as well as including any future image processing projects
 *      - ImagePixelSort
 *          Goal: receive an image as input, and output a new image consisting of the same pixels, but sorted.
 *
 *      - ImageResizer
 *          Goal: receive an input folder full of image files, iterate through the folder upsizing each image and output the upsized versions to an output folder 
 *
 *      - ImageToAscii
 *          Goal: input an image and output that image converted into ascii characters.
*/

using System.Drawing;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]


/**
 * Class <c>ImageProcessing</c> currently contains all processing logic.
 */
public class ImageProcessing
{
    static string InputFileFolder = @"D:\ProgrammingProjects\IO folders\ImageProcessing\Input";
    static string OutputFileFolder = @"D:\ProgrammingProjects\IO folders\ImageProcessing\Output";

    static LoadingBar ProgressBar = new LoadingBar(Console.CursorLeft, Console.CursorTop);

    /**
     * <summary>
     * This is the main method entry point.
     * </summary>
     */
    public static void Main()
    {
        Console.WriteLine("Beginning image processing now...");
        int CurrentImage = 0;
        int TotalNumberOfImages = Directory.GetFiles(InputFileFolder, "*.*", SearchOption.TopDirectoryOnly).Length;

        foreach (string FileName in Directory.GetFiles(InputFileFolder))
        {
            ProgressBar.SetTotalProgressPercentage(100 * ((double) CurrentImage / TotalNumberOfImages));
            Console.Write("Currently processing image " + ++CurrentImage + " of " + TotalNumberOfImages + "\n");

            string ImageName = FileName;

            // loops through removing folder names from filepath to find image file name
            while (ImageName.IndexOf("\\") != -1)
            {
                ImageName = ImageName.Substring(ImageName.IndexOf("\\") + 1);
            }

            // removes file extension from image file name
            ImageName = ImageName.Substring(0, ImageName.IndexOf("."));
            
            Bitmap InputImage = new Bitmap(FileName);

            // call methods here
            //InvertImage(ProgressBar, ConvertToSingleColorImage(ProgressBar, InputImage, Color.White)).Save(OutputFileLocation + "\\" + ImageName + " in white inverted.jpg");
            ConvertToSingleColorImage(InputImage, Color.White).Save(OutputFileFolder + "\\" + ImageName + " in white.jpg");
            PixelSortByColumns(InputImage).Save(OutputFileFolder + "\\" + ImageName + " sorted.jpg");
        }
        ProgressBar.SetTotalProgressPercentage(100);
        Console.WriteLine("\nImage processing is complete.\nPress Enter to confirm");
        Console.Read();
    }


    /**
     * <summary>
     * This method accepts a passed Bitmap and converts each pixel into a value of a single color.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be copied and converted to contain a single color. </param>
     * <param name="ColorFilter"> The desired color for the generated image. </param>
     * <returns>
     * A copy of the passed Bitmap now in only the passed color.
     * </returns>
     */
    public static Bitmap ConvertToSingleColorImage(Bitmap InputImage, Color ColorFilter)
    {
        Bitmap NewImage = new Bitmap(InputImage);

        double BrightnessRatio;

        for (int CurrentRow = 0; CurrentRow < NewImage.Height; CurrentRow++)
        {
            for (int CurrentColumn = 0; CurrentColumn < NewImage.Width; CurrentColumn++)
            {
                BrightnessRatio = (double) FindBrightnessAsInt(NewImage, CurrentColumn, CurrentRow) / 255;

                NewImage.SetPixel(CurrentColumn, CurrentRow, Color.FromArgb(
                (int) (BrightnessRatio * ColorFilter.A), 
                (int) (BrightnessRatio * ColorFilter.R), 
                (int) (BrightnessRatio * ColorFilter.G), 
                (int) (BrightnessRatio * ColorFilter.B)));
            }
            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) CurrentRow / (double) NewImage.Height));
        }

        return NewImage;
    }


    /**
     * <summary>
     * this method accepts a passed Bitmap and inverts the color of each pixel.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be edited.</param>
     * <returns>
     * A copy of the passed Bitmap with the inverted colors.
     * </returns>
     */
    public static Bitmap InvertImage(Bitmap InputImage)
    {
        Bitmap NewImage = new Bitmap(InputImage);

        int NewAlphaValue;
        int NewRedValue;
        int NewGreenValue;
        int NewBlueValue;

        for (int CurrentRow = 0; CurrentRow < NewImage.Height; CurrentRow++)
        {
            for (int CurrentColumn = 0; CurrentColumn < NewImage.Width; CurrentColumn++)
            {
                NewAlphaValue = (255 - NewImage.GetPixel(CurrentColumn, CurrentRow).A);
                NewRedValue = (255 - NewImage.GetPixel(CurrentColumn, CurrentRow).R);
                NewGreenValue = (255 - NewImage.GetPixel(CurrentColumn, CurrentRow).G);
                NewBlueValue = (255 - NewImage.GetPixel(CurrentColumn, CurrentRow).B);
                NewImage.SetPixel(CurrentColumn, CurrentRow, Color.FromArgb(NewAlphaValue, NewRedValue, NewGreenValue, NewBlueValue));
            }
            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) CurrentRow / (double) NewImage.Height));
        }

        return NewImage;
    }


    /**
     * <summary>
     * This method accepts a passed Bitmap and inverts the color of each pixel, but inverts around the average color, instead of a total inversion
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be inverted. </param>
     * <returns>
     * A copy of the passed Bitmap with inverted colors around the average color.
     * </returns>
     */
    public static Bitmap InvertImageAroundAverage(Bitmap InputImage)
    {
        Bitmap NewImage = new Bitmap(InputImage);

        int NewAlphaValue;
        int NewRedValue;
        int NewGreenValue;
        int NewBlueValue;
        Color AverageColor = FindAverageColor(NewImage);

        for (int CurrentRow = 0; CurrentRow < NewImage.Height; CurrentRow++)
        {
            for (int CurrentColumn = 0; CurrentColumn < NewImage.Width; CurrentColumn++)
            {
                NewAlphaValue = (128 + ((AverageColor.A - NewImage.GetPixel(CurrentColumn, CurrentRow).A) / 2));
                NewRedValue = (128 + ((AverageColor.R - NewImage.GetPixel(CurrentColumn, CurrentRow).R) / 2));
                NewGreenValue = (128 + ((AverageColor.G - NewImage.GetPixel(CurrentColumn, CurrentRow).G) / 2));
                NewBlueValue = (128 + ((AverageColor.B - NewImage.GetPixel(CurrentColumn, CurrentRow).B) / 2));
                NewImage.SetPixel(CurrentColumn, CurrentRow, Color.FromArgb(NewAlphaValue, NewRedValue, NewGreenValue, NewBlueValue));
            }
            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) CurrentRow / (double) NewImage.Height));
        }

        return NewImage;   
    }


    /**
     * <summary>
     * This method takes a passed Bitmap, breaks each row into its own Bitmap to be sorted, then copies each row Bitmap into a new Bitmap.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to have its pixels sorted. </param>
     * <returns>
     * A copy of the passed Bitmap with each row of pixels sorted.
     * </returns>
     */
    public static Bitmap PixelSortByRows(Bitmap InputImage)
    {
        Bitmap NewImage = new Bitmap(InputImage);
        Bitmap RowBitmap = new Bitmap(NewImage.Width, 1);

        // this loop loops through all of the rows of pixels in the passed Bitmap
        for (int Row = 0; Row < NewImage.Height; Row++)
        {
            // this loop loops through each pixel in the row, and copies it to the individual row Bitmap
            for (int Column = 0; Column < NewImage.Width; Column++)
            {
                RowBitmap.SetPixel(Column, 0, NewImage.GetPixel(Column, Row));
            }

            RowBitmap = ConvertArrayToBitmap(SortColorArray(GeneratePixelArray(RowBitmap)), RowBitmap.Width, 1);

            // this loop copies the row Bitmap back into the original
            for (int Column = 0; Column < NewImage.Width; Column++)
            {
                NewImage.SetPixel(Column, Row, RowBitmap.GetPixel(Column, 0));
            }

            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) Row / (double) NewImage.Height));
        }

        return NewImage;
    }


    /**
     * <summary>
     * This method takes a passed Bitmap, breaks each column into its own Bitmap to be sorted, then copies each column Bitmap into a new Bitmap.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to have its pixels sorted. </param>
     * <returns>
     * A copy of the passed Bitmap with each column of pixels sorted.
     * </returns>
     */
    public static Bitmap PixelSortByColumns(Bitmap InputImage)
    {
        Bitmap NewImage = new Bitmap(InputImage);
        Bitmap RowBitmap = new Bitmap(1, NewImage.Height);

        // this loop loops through all of the columns of pixels in the passed Bitmap
        for (int Column = 0; Column < NewImage.Width; Column++)
        {
            // this loop loops through each pixel in the column, and copies it to the individual column Bitmap
            for (int Row = 0; Row < NewImage.Height; Row++)
            {
                RowBitmap.SetPixel(0, Row, NewImage.GetPixel(Column, Row));
            }

            RowBitmap = ConvertArrayToBitmap(SortColorArray(GeneratePixelArray(RowBitmap)), 1, RowBitmap.Height);

            // this loop copies the row Bitmap back into the original
            for (int Row = 0; Row < NewImage.Height; Row++)
            {
                NewImage.SetPixel(Column, Row, RowBitmap.GetPixel(0, Row));
            }

            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) Column / (double) NewImage.Width));
        }

        return NewImage;
    }


    /**
     * <summary>
     * This method converts a passed Color Array into a Bitmap.
     * </summary>
     * <param name="PassedColorArray"> This is the Color array to be converted. </param>
     * <param name="ReturnedBitmapWidth"> This is the width of the generated Bitmap. </param>
     * <param name="ReturnedBitmapHeight"> This is the height of the generated Bitmap. </param>
     * <returns>
     * A new Bitmap of size PassedWidth by PassedHeight using the colors from the color array.
     * </returns>
     * <exception cref="ArgumentException">
     * Thrown if PassedWidth or PassedHeight are negative, or if the passed width and height values don't match the color array size.
     * </exception>
     */
    public static Bitmap ConvertArrayToBitmap(Color[] PassedColorArray, int ReturnedBitmapWidth, int ReturnedBitmapHeight)
    {
        if (PassedColorArray.Length != (ReturnedBitmapWidth * ReturnedBitmapHeight))
        {
            throw new ArgumentException("Passed values have a mismatch. (PassedWidth * PassedHeight) != PassedColorArray.Length");
        }

        if ((ReturnedBitmapWidth < 1) || (ReturnedBitmapHeight < 1))
        {
            throw new ArgumentException("PassedWidth and PassedHeight must be greater than 0");
        }

        Bitmap ReturnBitmap = new Bitmap(ReturnedBitmapWidth, ReturnedBitmapHeight);

        for (int Row = 0; Row < ReturnedBitmapHeight; Row++)
        {
            for (int Column = 0; Column < ReturnedBitmapWidth; Column++)
            {
                ReturnBitmap.SetPixel(Column, Row, PassedColorArray[(Row * ReturnedBitmapWidth) + Column]);
            }
        }

        return ReturnBitmap;
    }


    /**
     * <summary>
     * This method generates a color array from the pixels in the passed Bitmap.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be converted into a color array. </param>
     * <returns>
     * A Color array representing the pixels in the passed Bitmap.
     * </returns>
     * <exception cref="ArgumentException">
     * Thrown if the passed Bitmap is null.
     * </exception>
     */
    public static Color[] GeneratePixelArray(Bitmap InputImage)
    {
        if (InputImage == null)
        {
            throw new ArgumentException("Passed Bitmap cannot be null");
        }

        Color[] PixelArray = new Color[InputImage.Width * InputImage.Height];

        for (int Row = 0; Row < InputImage.Height; Row++)
        {
            for (int Column = 0; Column < InputImage.Width; Column++)
            {
                PixelArray[(Row * InputImage.Width) + Column] = InputImage.GetPixel(Column, Row);
            }
        }

        return PixelArray;
    }


    /**
     * <summary>
     * This method accepts an array of colors and sorts them using MergeSort.
     * </summary>
     * <param name="ArrayToSort"> This is the array to be sorted. </param>
     * <returns>
     * The sorted array.
     * </returns>
     */
    public static Color[] SortColorArray(Color[] ArrayToSort)
    {
        //return ColorArrayQuickSort(ArrayToSort, 0, ArrayToSort.Length - 1);
        return ColorArrayMergeSort(ArrayToSort);
    }


    /**
     * <summary>
     * This method uses a recursive merge sort algorithm to sort the passed color array.
     * </summary>
     * <param name="ArrayToSort"> This is the array to be sorted. </param>
     * <returns>
     * The sorted array.
     * </returns>
     */
    public static Color[] ColorArrayMergeSort(Color[] ArrayToSort)
    {
        if(ArrayToSort.Length == 1)
        {
            return ArrayToSort;
        }
        else
        {
            Color[] LeftHalf = new Color[ArrayToSort.Length / 2];
            Color[] RightHalf = new Color[ArrayToSort.Length - (ArrayToSort.Length / 2)];

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

            ArrayToSort = ColorArrayMerge(ColorArrayMergeSort(LeftHalf), ColorArrayMergeSort(RightHalf));
        }

        return ArrayToSort;
    }

    
    /**
     * <summary>
     * This is a merge function to help the MergeSort function. It accepts two sorted arrays and merges them into one sorted array.
     * </summary>
     * <param name="FirstArray"> This is the first sorted array to be merged. </param>
     * <param name="SecondArray"> This is the second sorted array to be merged. </param>
     * <returns>
     * A new array that contains all elements from the two array parameters.
     * </returns>
     */
    public static Color[] ColorArrayMerge(Color[] FirstArray, Color[] SecondArray)
    {
        Color[] ReturnArray = new Color[(FirstArray.Length + SecondArray.Length)];

        int FirstArrayIndex = 0;
        int SecondArrayIndex = 0;
        int ReturnArrayIndex = 0;

        while ((FirstArrayIndex < FirstArray.Length) && (SecondArrayIndex < SecondArray.Length))
        {
            if(CompareColors("<=", FirstArray[FirstArrayIndex], SecondArray[SecondArrayIndex]))
            {
                ReturnArray[ReturnArrayIndex++] = FirstArray[FirstArrayIndex++];
            }
            else if(CompareColors("<", SecondArray[SecondArrayIndex], FirstArray[FirstArrayIndex]))
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
        this method uses a recursive quick sort algorithm on the passed Color array
    ----- */
    /**
     * <summary>
     * This method uses a recursive quick sort algorithm on the passed Color array.
     * </summary>
     * <param name="ArrayToSort"> This is the array to be sorted. </param>
     * <param name="LeftIndex"> This is the left index of the range. </param>
     * <param name="RightIndex"> This is the right index of the range. </param>
     * <returns>
     * The sorted array.
     * </returns>
     */
    public static Color[] ColorArrayQuickSort(Color[] ArrayToSort, int LeftIndex, int RightIndex)
    {
        while (LeftIndex < RightIndex)
        {
            int PivotIndex = ColorArrayQuickSortPartition(ArrayToSort, LeftIndex, RightIndex);

            if ((PivotIndex - LeftIndex) <= (RightIndex - (PivotIndex + 1)))
            {
                ColorArrayQuickSort(ArrayToSort, LeftIndex, PivotIndex);
                LeftIndex = PivotIndex + 1;
            }
            else
            {
                ColorArrayQuickSort(ArrayToSort, PivotIndex + 1, RightIndex);
                RightIndex = PivotIndex - 1;
            }
        }
        return ArrayToSort;
    }


    /**
     * <summary>
     * 
     * </summary>
     * <param name="ArrayToSort"> This is the array to be sorted. </param>
     * <param name="LeftIndex"> This is the left index of the range. </param>
     * <param name="RightIndex"> This is the right index of the range. </param>
     * <returns>
     * The pivot index that the array is sorted around.
     * </returns>
     */
    public static int ColorArrayQuickSortPartition(Color[] ArrayToSort, int LeftIndex, int RightIndex)
    {
        Color PivotValue = ArrayToSort[(LeftIndex + RightIndex) / 2];
        int LeftPointer = LeftIndex;
        int RightPointer = RightIndex;

        while (true)
        {
            // if item at LeftPointer is already less than the pivot, it's already sorted.
            while ((LeftPointer < RightIndex) && CompareColors("<=", ArrayToSort[LeftPointer], PivotValue))
            {
                LeftPointer++;
            }

            // if the item at RightPointer is already greater than the pivot, it's already sorted.
            while ((RightPointer > LeftIndex) && CompareColors(">=", ArrayToSort[RightPointer], PivotValue))
            {
                RightPointer--;
            }

            // if the LeftPointer item is greater and the RightPointer item is less than, swap the two
            if (LeftPointer < RightPointer)
            {
                Color TempValue = ArrayToSort[LeftPointer];
                ArrayToSort[LeftPointer++] = ArrayToSort[RightPointer];
                ArrayToSort[RightPointer--] = TempValue;
            }
            else
            {
                return RightPointer;
            }
        }
    }


    /**
     * <summary>
     * This method compares the passed colors by comparing the average rgb value of the colors.
     * </summary>
     * <param name="Operation"> The operation to use to compare the two colors. </param>
     * <param name="FirstColor"> The first color to be compared. </param>
     * <param name="SecondColor"> The second color to be compared. </param>
     * <returns>
     * A boolean value representing how the first value compares to the second value with the passed operator.
     * </returns>
     * <exception cref="ArgumentException">
     * Thrown if the passed operation is not supported.
     * </exception>
     */
    public static bool CompareColors(string Operation, Color FirstColor, Color SecondColor)
    {
        double FirstColorRGBAverage = (FirstColor.R + FirstColor.G + FirstColor.B) / 3;
        double SecondColorRGBAverage = (SecondColor.R + SecondColor.G + SecondColor.B) / 3;

        switch (Operation)
        {
            case "=":
                return FirstColorRGBAverage == SecondColorRGBAverage;

            case "<":
                return FirstColorRGBAverage < SecondColorRGBAverage;

            case ">":
                return FirstColorRGBAverage > SecondColorRGBAverage;

            case "<=":
                return FirstColorRGBAverage <= SecondColorRGBAverage;

            case ">=":
                return FirstColorRGBAverage >= SecondColorRGBAverage;

            default:
                throw new ArgumentException("passed operation was not recognized");
        }
    }
    

    /**
     * <summary>
     * This method accepts a Bitmap and finds the average color value of all pixels.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to find the average value for. </param>
     * <returns>
     * A Color representing the average color of all pixels in the passed Bitmap.
     * </returns>
     */
    public static Color FindAverageColor(Bitmap InputImage)
    {
        long AverageAlphaValue = 0;
        long AverageRedValue = 0;
        long AverageGreenValue = 0;
        long AverageBlueValue = 0;
        int NumberOfPixels = 0;

        for (int CurrentRow = 0; CurrentRow < InputImage.Height; CurrentRow++)
        {
            for (int CurrentColumn = 0; CurrentColumn < InputImage.Width; CurrentColumn++)
            {
                AverageAlphaValue += InputImage.GetPixel(CurrentColumn, CurrentRow).A;
                AverageRedValue += InputImage.GetPixel(CurrentColumn, CurrentRow).R;
                AverageGreenValue += InputImage.GetPixel(CurrentColumn, CurrentRow).G;
                AverageBlueValue += InputImage.GetPixel(CurrentColumn, CurrentRow).B;
                NumberOfPixels++;
            }
        }

        AverageAlphaValue /= NumberOfPixels;
        AverageRedValue /= NumberOfPixels;
        AverageGreenValue /= NumberOfPixels;
        AverageBlueValue /= NumberOfPixels;


        Color AverageColor = Color.FromArgb((int) AverageAlphaValue, (int) AverageRedValue, (int) AverageGreenValue, (int) AverageBlueValue);

        return AverageColor;
    }


    /**
     * <summary>
     * This method accepts a Bitmap as input and then calls the ActualResize method on it until it is of a minimum size. If it is already larger than the passed size, it will not be resized.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be resized. </param>
     * <param name="MinimumWidth"> This is the minimum width the image will have before being returned. </param>
     * <param name="MinimumHeight"> This is the minimum height the image will have before being returned. </param>
     * <returns>
     * A copy of the Bitmap increased to the minimum size.
     * </returns>
     */
    public static Bitmap Resize(Bitmap InputImage, int MinimumWidth, int MinimumHeight)
    {
        Bitmap NewImage = new Bitmap(InputImage);

        while ((NewImage.Width < MinimumWidth) || (NewImage.Height < MinimumHeight))
        {
            NewImage = ActualResize(NewImage);
        }

        return NewImage;
    }


    /**
     * <summary>
     * This method accepts a Bitmap as input and returns a Bitmap of double the size. 
     * It currently calls GetAverageColor to determine what color to use for newly created pixels.
     * This method shouldn't be called directly, instead call from Resize.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be resized. </param>
     * <returns>
     * A copy of the Bitmap at double the width and double the height.
     * </returns>
     * <exception cref="ArgumentException">
     * Thrown if the passed Bitmap is null.
     * </exception>
     */
    private static Bitmap ActualResize(Bitmap InputImage)
    {
        if (InputImage != null)
        {
            // for newly created Bitmaps, all pixels are given rgb value 0, 0, 0
            Bitmap OutputImage = new Bitmap(InputImage.Width * 2, InputImage.Height * 2);

            // loop through input image pixels and copy them to their new position. 
            // moving pixels from x, y to 2x, 2y
            for (int j = 0; j < InputImage.Height; j++)
            {
                for (int i = 0; i < InputImage.Width; i++)
                {
                    OutputImage.SetPixel(2 * i, 2 * j, InputImage.GetPixel(i, j));
                }
                ProgressBar.SetSubProcessProgressPercentage(25 * ((double) j / InputImage.Height));
            }

            ProgressBar.SetSubProcessProgressPercentage(25);

            // next loop is to fill in the empty pixels at odd increments along the x axis
            for (int j = 0; j < OutputImage.Height; j += 2)
            {
                for (int i = 1; i < OutputImage.Width - 1; i += 2)
                {
                    OutputImage.SetPixel(i, j, GetAverageColor("horiz", OutputImage, i, j));
                }
                ProgressBar.SetSubProcessProgressPercentage(25 + (25 * ((double) j / OutputImage.Height)));
            }

            ProgressBar.SetSubProcessProgressPercentage(50);

            // next loop is to fill in the empty pixels at odd increments along the y axis
            for (int i = 0; i < OutputImage.Width; i += 2)
            {
                for (int j = 1; j < OutputImage.Height - 1; j += 2)
                {
                    OutputImage.SetPixel(i, j, GetAverageColor("vert", OutputImage, i, j));
                }
                ProgressBar.SetSubProcessProgressPercentage(50 + (25 * ((double) i / OutputImage.Width)));
            }

            ProgressBar.SetSubProcessProgressPercentage(75);

            // next loop is to fill in the remaining empty pixels
            for (int i = 1; i < OutputImage.Width - 1; i += 2)
            {
                for (int j = 1; j < OutputImage.Height - 1; j += 2)
                {
                    OutputImage.SetPixel(i, j, GetAverageColor("diag", OutputImage, i, j));
                }
                ProgressBar.SetSubProcessProgressPercentage(75 + (25 * ((double) i / InputImage.Width)));
            }

            ProgressBar.SetSubProcessProgressPercentage(100);

            return OutputImage;
            
        }
        else
        {
            throw new ArgumentException("Input Image cannot be null");
        }
    }


    /**
     * <summary>
     * This method magnifies the input image, creating a four pixel square for each pixel.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be magnified. </param>
     * <returns>
     * A copy of the passed Bitmap at double the width and double the height.
     * </returns>
     * <exception cref="ArgumentException">
     * Thrown if the passed Bitmap is null.
     * </exception>
     */
    private static Bitmap Magnify(Bitmap InputImage)
    {
        if (InputImage != null)
        {
            double Progress;
            Bitmap OutputImage = new Bitmap(InputImage.Width * 2, InputImage.Height * 2);

            for (int j = 0; j < InputImage.Height; j++)
            {
                for (int i = 0; i < InputImage.Width; i++)
                {
                    OutputImage.SetPixel(2 * i, 2 * j, InputImage.GetPixel(i, j));
                    OutputImage.SetPixel((2 * i) + 1, 2 * j, InputImage.GetPixel(i, j));
                    OutputImage.SetPixel(2 * i, (2 * j) + 1, InputImage.GetPixel(i, j));
                    OutputImage.SetPixel((2 * i) + 1, (2 * j) + 1, InputImage.GetPixel(i, j));
                }

                Progress = (100 * ((double) j / (double) InputImage.Height));
                ProgressBar.SetSubProcessProgressPercentage(Progress);
            }

            return OutputImage;
        }
        else
        {
            throw new ArgumentException("Passed Bitmap cannot be null");
        }
    }


    /**
     * <summary>
     * this method finds the average (mean) color of a given set of pixels depending on the input:
     * horiz: averages only pixels to the left and right of passed pixel
     * vert: averages only pixels above and below the passed pixel
     * diag: averages pixels diagonal to the passed pixel
     * </summary>
     * <param name="ComparisonDirection"> This is the direction to compare pixels. </param>
     * <param name="InputImage"> This is the passed Bitmap to compare pixels in. </param>
     * <param name="CurrentPixelX"> This is the current pixels X position. </param>
     * <param name="CurrentPixelY"> this is the current pixels Y position. </param>
     * <returns>
     * An average of the color value of the group of pixels defined.
     * </returns>
     * <exception cref="ArgumentException">
     * Thrown if the passed Bitmap is null.
     * or
     * The passed X position is less than zero or greater than the passed Bitmap's width.
     * or
     * The passed Y position is less than zero or greater than the passed Bitmap's height.
     * or
     * The passed comparison direction is not supported.
     * </exception>
     */
    private static Color GetAverageColor(string ComparisonDirection, Bitmap InputImage, int CurrentPixelX, int CurrentPixelY)
    {
        if (InputImage == null)
        {
            throw new ArgumentException("passed Bitmap cannot be null");
        }

        if (CurrentPixelX >= 0 && CurrentPixelY >= 0 && CurrentPixelX < InputImage.Width && CurrentPixelY < InputImage.Height)
        {
            List<Color> NearbyPixels = new List<Color>();

            switch (ComparisonDirection)
            {
                case "diag":
                    NearbyPixels.Add(InputImage.GetPixel(CurrentPixelX - 1, CurrentPixelY - 1));
                    NearbyPixels.Add(InputImage.GetPixel(CurrentPixelX - 1, CurrentPixelY + 1));
                    NearbyPixels.Add(InputImage.GetPixel(CurrentPixelX + 1, CurrentPixelY - 1));
                    NearbyPixels.Add(InputImage.GetPixel(CurrentPixelX + 1, CurrentPixelY + 1));
                    break;

                case "vert":
                    NearbyPixels.Add(InputImage.GetPixel(CurrentPixelX, CurrentPixelY - 1));
                    NearbyPixels.Add(InputImage.GetPixel(CurrentPixelX, CurrentPixelY + 1));
                    break;

                case "horiz":
                    NearbyPixels.Add(InputImage.GetPixel(CurrentPixelX - 1, CurrentPixelY));
                    NearbyPixels.Add(InputImage.GetPixel(CurrentPixelX + 1, CurrentPixelY));
                    break;

                default:
                    throw new ArgumentException("passed value for ComparisonDirection parameter must be either 'noise', 'horiz', 'vert', or 'diag'");
            }

            int AlphaAverage = 0;
            int RedAverage = 0;
            int GreenAverage = 0;
            int BlueAverage = 0;

            for (int i = 0; i < NearbyPixels.Count; i++)
            {
                AlphaAverage += NearbyPixels[i].A;
                RedAverage += NearbyPixels[i].R;
                GreenAverage += NearbyPixels[i].G;
                BlueAverage += NearbyPixels[i].B;
            }

            AlphaAverage /= NearbyPixels.Count;
            RedAverage /= NearbyPixels.Count;
            GreenAverage /= NearbyPixels.Count;
            BlueAverage /= NearbyPixels.Count;

            Color AverageColor = Color.FromArgb(AlphaAverage, RedAverage, GreenAverage, BlueAverage);

            return AverageColor;

        }
        else
        {
            throw new ArgumentException("Passed X and Y values must be greater than 0 and less than the passed Bitmap's width and height respectively");
        }
    }


    /* -----
    this method uses anisotropic diffusion to reduce image noise.
    ----- */

    /// <param name="dt">Heat difusion value. Upper = more rapid convergence. usually 0 < dt <= 1/3 </param>
    /// <param name="lambda">The shape of the diffusion coefficient g(), controlling the Perona Malik diffusion g(delta) = 1/((1 +  delta2)  / lambda2). Upper = more blurred image & more noise removed</param>
    /// <param name="iterations">Determines the maximum number of iteration steps of the filter. Upper = less speed & more noise removed</param>

    private static Bitmap ADReduceNoise(Bitmap InputImage, double dt, int lambda, int iterations)
    {

        if (InputImage == null) throw new ArgumentException("Passed Bitmap cannot be null");

        // todo
        //test parameters
        if (dt < 0)
            throw new Exception("DT negative value not allowed");
        if (lambda < 0)
            throw new Exception("lambda must be greater than 0");
        if (iterations <= 0)
            throw new Exception("Iterations must be greater than 0");

        //Make temp Bitmap
        Bitmap ReturnBitmap = (Bitmap) InputImage.Clone();

        //Precalculate tables (for speed up)
        double[] precal = new double[512];
        double lambda2 = lambda * lambda;
        for (int f = 0; f < 512; f++)
        {
            int delta = f - 255;
            // equation 2
            precal[f] = -dt * delta * lambda2 / (lambda2 + delta * delta);
            // equation 1 (exponential)
            //precal[f] = -dt * delta * Math.Exp(-(delta * delta / lambda2));
        }

        //Apply the filter
        for (int n = 0; n < iterations; n++)
        {
            for (int h = 0; h < InputImage.Height; h++)
                for (int w = 0; w < InputImage.Width; w++)
                {
                    int px = w - 1;
                    int nx = w + 1;
                    int py = h - 1;
                    int ny = h + 1;
                    if (px < 0)
                        px = 0;
                    if (nx >= InputImage.Width)
                        nx = InputImage.Width - 1;
                    if (py < 0)
                        py = 0;
                    if (ny >= InputImage.Height)
                        ny = InputImage.Height - 1;


                    int current = InputImage.GetPixel(w, h).A;

                    int AlphaValue = (int) (precal[255 + current - InputImage.GetPixel(px, h).A] +
                                    precal[255 + current - InputImage.GetPixel(nx, h).A] +
                                    precal[255 + current - InputImage.GetPixel(w, py).A] +
                                    precal[255 + current - InputImage.GetPixel(w, ny).A] +
                                    InputImage.GetPixel(w, h).A);


                    current = InputImage.GetPixel(w, h).R;

                    int RedValue = (int) (precal[255 + current - InputImage.GetPixel(px, h).R] +
                                    precal[255 + current - InputImage.GetPixel(nx, h).R] +
                                    precal[255 + current - InputImage.GetPixel(w, py).R] +
                                    precal[255 + current - InputImage.GetPixel(w, ny).R] +
                                    InputImage.GetPixel(w, h).R);


                    current = InputImage.GetPixel(w, h).G;

                    int GreenValue = (int) (precal[255 + current - InputImage.GetPixel(px, h).G] +
                                    precal[255 + current - InputImage.GetPixel(nx, h).G] +
                                    precal[255 + current - InputImage.GetPixel(w, py).G] +
                                    precal[255 + current - InputImage.GetPixel(w, ny).G] +
                                    InputImage.GetPixel(w, h).G);

                    current = InputImage.GetPixel(w, h).B;

                    int BlueValue = (int) (precal[255 + current - InputImage.GetPixel(px, h).B] +
                                    precal[255 + current - InputImage.GetPixel(nx, h).B] +
                                    precal[255 + current - InputImage.GetPixel(w, py).B] +
                                    precal[255 + current - InputImage.GetPixel(w, ny).B] +
                                    InputImage.GetPixel(w, h).B);
                    

                    ReturnBitmap.SetPixel(w, h, Color.FromArgb(AlphaValue, RedValue, GreenValue, BlueValue));
                }
        }

        return ReturnBitmap;
    }


    /**
     * <summary>
     * This method searches the passed Bitmap for pixels that lie on an edge and updates the pixels.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be processed. </param>
     * <returns>
     * A copy of the passed Bitmap
     * </returns>
     * <exception cref="ArgumentException">
     * Thrown if the passed Bitmap is null.
     * </exception>
     */
    private static Bitmap SharpenEdges(Bitmap InputImage)
    {
        if (InputImage != null)
        {
            // represents how close a pixel r, g, or b value must be to be considered similar
            double SimilarityRange = 0;  
            Bitmap ReturnBitmap = (Bitmap) InputImage.Clone();

            // four nested for loops?! there must be a better way to do this. look into improvements.
            for (int j = 4; j < InputImage.Height - 4; j += 2)
            {
                for (int i = 4; i < InputImage.Width - 4; i += 2)
                {
                    bool[,] SimilarPixels = new bool[3, 3];
                    for (int a = i - 2; a <= i + 2; a += 2)
                    {
                        for (int b = j - 2; b <= j + 2; b += 2)
                        {
                            // set the flag to false by default
                            SimilarPixels[((a - i) + 2) / 2, ((b - j) + 2) / 2] = false;

                            // if the pixels a, r, g, and b values are within the similarity range, flag as true
                            if (InputImage.GetPixel(a, b).A > (InputImage.GetPixel(i, j).A - SimilarityRange) && (InputImage.GetPixel(a, b).A < (InputImage.GetPixel(i, j).A + SimilarityRange)) &&
                                InputImage.GetPixel(a, b).R > (InputImage.GetPixel(i, j).R - SimilarityRange) && (InputImage.GetPixel(a, b).R < (InputImage.GetPixel(i, j).R + SimilarityRange)) &&
                                InputImage.GetPixel(a, b).G > (InputImage.GetPixel(i, j).G - SimilarityRange) && (InputImage.GetPixel(a, b).G < (InputImage.GetPixel(i, j).G + SimilarityRange)) &&
                                InputImage.GetPixel(a, b).B > (InputImage.GetPixel(i, j).B - SimilarityRange) && (InputImage.GetPixel(a, b).B < (InputImage.GetPixel(i, j).B + SimilarityRange)))
                            {
                                SimilarPixels[(a - i + 2) / 2, ((b - j) + 2) / 2] = true;
                            }
                        }
                    }

                    // check for '\' edge
                    if (SimilarPixels[0, 0] && !SimilarPixels[0, 1] && !SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && !SimilarPixels[2, 0] && !SimilarPixels[2, 1] && SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i - 1, j + 1, InputImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j - 1, InputImage.GetPixel(i, j));
                    }

                    // check for '/' edge
                    if (!SimilarPixels[0, 0] && !SimilarPixels[0, 1] && SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i - 1, j - 1, InputImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j + 1, InputImage.GetPixel(i, j));
                    }

                    // check for '-' edge
                    if (!SimilarPixels[0, 0] && !SimilarPixels[0, 1] && !SimilarPixels[0, 2] && SimilarPixels[1, 0] && SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i - 1, j, InputImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j, InputImage.GetPixel(i, j));
                    }

                    // check for '|' edge
                    if (!SimilarPixels[0, 0] && SimilarPixels[0, 1] && !SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && !SimilarPixels[2, 0] && SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i, j + 1, InputImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i, j - 1, InputImage.GetPixel(i, j));
                    }

                    // check for 'X' edge
                    if (SimilarPixels[0, 0] && !SimilarPixels[0, 1] && SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i - 1, j - 1, InputImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i - 1, j + 1, InputImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j - 1, InputImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j + 1, InputImage.GetPixel(i, j));
                    }
                }
            }

            return ReturnBitmap;
        }
        else
        {
            throw new ArgumentException("Passed Bitmap cannot be null");
        }
    }


    /**
     * <summary>
     * This method accepts a Bitmap and converts it into ASCII characters, then saves it as a text file.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be converted into ASCII characters. </param>
     * <param name="PassedFileName"> This is the filename the ASCII character text file will be saved as. </param>"
     * <exception cref="ArgumentException">
     * Thrown if the passed Bitmap is null.
     * </exception>
     */
    private static void ConvertImageToAscii(Bitmap InputImage, string PassedFileName)
    {

        string OutputAsciiChars = " _.,-=+:;abc!?0123456789$W#@Ñ";
        //string OutputAsciiChars = "Ñ@#W$9876543210?!abc;:+=-,._ ";

        if (InputImage != null)
        {
            char[] OutputChars = OutputAsciiChars.ToCharArray();
            double ConversionValue = (256.0 / (OutputChars.Length - 1));
            StreamWriter OutputFile = File.CreateText(PassedFileName);

            for (int j = 0; j < InputImage.Height; j++)
            {
                for (int i = 0; i < InputImage.Width; i++)
                {
                        OutputFile.Write(" " + OutputChars[(int) ((FindBrightnessAsInt(InputImage, i, j) / ConversionValue))]);
                }
                OutputFile.Write("\n");
            }
        }
        else
        {
            throw new ArgumentException("Passed Bitmap cannot be null");
        }
    }


    /**
     * <summary>
     * This method returns the calculated brightness value for a specified pixel within a Bitmap.
     * </summary>
     * <param name="InputImage"> This is the Bitmap containing the pixel. </param>
     * <param name="PositionX"> This is the X position of the pixel. </param>
     * <param name="PositionY"> This is the Y position of the pixel. </param>
     * <returns>
     * An integer representing the calculated brightness for the passed pixel.
     * </returns>
     */
    private static int FindBrightnessAsInt(Bitmap InputImage, int PositionX, int PositionY)
    {
        // below formula weighs r, g, and b values equally
        //return (int) ((InputImage.GetPixel(PositionX, PositionY).R + InputImage.GetPixel(PositionX, PositionY).G + InputImage.GetPixel(PositionX, PositionY).B) / 3);

        // below formula returns Luminance
        return (int) ((InputImage.GetPixel(PositionX, PositionY).R * 0.3) + (InputImage.GetPixel(PositionX, PositionY).G * 0.59) + (InputImage.GetPixel(PositionX, PositionY).B * 0.11));
    }


    /**
     * <summary>
     * This method searches the passed Bitmap for pixels that lie on an edge.
     * </summary>
     * <param name="InputImage"> This is the Bitmap to be searched for edges. </param>
     * <returns>
     * A char array where each char represents a possible edge that the pixel lies on.
     * </returns>
     * <exception cref="ArgumentException">
     * Thrown if the passed Bitmap is null.
     * </exception>
     */
    private static char[,] FindEdges(Bitmap InputImage)
    {
        if (InputImage != null)
        {
            // represents how close a pixel r, g, or b value must be to be considered similar
            double SimilarityRange = 5;  
            char[,] ReturnArray = new char[InputImage.Width, InputImage.Height];

            // four nested for loops?! there must be a better way to do this. look into improvements.
            for (int j = 1; j < InputImage.Height - 1; j ++)
            {
                for (int i = 1; i < InputImage.Width - 1; i++)
                {
                    // set edge flag character to empty space by default
                    ReturnArray[i, j] = ' ';

                    bool[,] SimilarPixels = new bool[3, 3];
                    for (int a = i - 1; a <= i + 1; a++)
                    {
                        for (int b = j - 1; b <= j + 1; b++)
                        {
                            // set the flag to false by default
                            SimilarPixels[(a - i) + 1, (b - j) + 1] = false;

                            // if the pixels a, r, g, and b values are within the similarity range, flag as true
                            if ((InputImage.GetPixel(a, b).A > (InputImage.GetPixel(i, j).A - SimilarityRange) && (InputImage.GetPixel(a, b).A < (InputImage.GetPixel(i, j).A + SimilarityRange))) &&
                                (InputImage.GetPixel(a, b).R > (InputImage.GetPixel(i, j).R - SimilarityRange) && (InputImage.GetPixel(a, b).R < (InputImage.GetPixel(i, j).R + SimilarityRange))) &&
                                (InputImage.GetPixel(a, b).G > (InputImage.GetPixel(i, j).G - SimilarityRange) && (InputImage.GetPixel(a, b).G < (InputImage.GetPixel(i, j).G + SimilarityRange))) &&
                                (InputImage.GetPixel(a, b).B > (InputImage.GetPixel(i, j).B - SimilarityRange) && (InputImage.GetPixel(a, b).B < (InputImage.GetPixel(i, j).B + SimilarityRange))))
                            {
                                SimilarPixels[(a - i) + 1, (b - j) + 1] = true;
                            }
                        }
                    }

                    // check for '\' edge
                    if (SimilarPixels[0, 0] && !SimilarPixels[0, 1] && !SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && !SimilarPixels[2, 0] && !SimilarPixels[2, 1] && SimilarPixels[2, 2])
                    {
                        ReturnArray[i, j] = '\\';
                    }

                    // check for '/' edge
                    if (!SimilarPixels[0, 0] && !SimilarPixels[0, 1] && SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnArray[i, j] = '/';
                    }

                    // check for '-' edge
                    if (!SimilarPixels[0, 0] && !SimilarPixels[0, 1] && !SimilarPixels[0, 2] && SimilarPixels[1, 0] && SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnArray[i, j] = '-';
                    }

                    // check for '|' edge
                    if (!SimilarPixels[0, 0] && SimilarPixels[0, 1] && !SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && !SimilarPixels[2, 0] && SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnArray[i, j] = '|';
                    }

                    // check for 'X' edge
                    if (SimilarPixels[0, 0] && !SimilarPixels[0, 1] && SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && SimilarPixels[2, 2])
                    {
                        ReturnArray[i, j] = 'X';
                    }
                }
            }

            return ReturnArray;
        }
        else
        {
            throw new ArgumentException("Passed Bitmap cannot be null");
        }
    }
}