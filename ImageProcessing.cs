/* --------------------
    Project: ImageProcessing
    Goal: this project will be a consolidation of my current image projects as well as including any future image processing projects
        - ImagePixelSort
            Goal: receive an image as input, and output a new image consisting of the same pixels, 
            but sorted.

        - ImageResizer
            Goal: receive an input folder full of image files, iterate through the folder upsizing each image and output the upsized versions to an output folder 

        - ImageToAscii
            Goal: input an image and output that image converted into ascii characters.


    ToDo:
        - improve loading bar. Perhaps by using a windows form?

    - ImagePixelSort
        - decide/implement how sorted pixels will be arranged in new image
        - instead of sorting all pixels in a column/row, have two groups of pixels (determined by contrast)
            where one group is sorted, but not the other.

    - ImageResizer
        - allow using windows explorer to select input folder destination and output folder
        - add error catching to verify file location
        - asynchronous processing to make it faster?
        - find or develop a better algorithm to reduce noise
        - the current anisotropic diffusion noise reduction method seems to use a guassian bell curve. Maybe instead of centering this on ~128, center it on median of image?
        - current noise reduction makes image more blurry, find improvement
        - learn more about anisotropic diffusion noise reduction
        - improve edge detection
        - I have been assuming the original picture is the most accurate, but perhaps I should try to improve those pixels too.

    - ImageToAscii
        - add error catching to verify file location
        - text file does not zoom easily. perhaps a different file type would be beter?
        - add edge detection. improve edge detection?
        - try different formulas for brightness

-------------------- */

using System.Drawing;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]


public class ImageProcessing
{
    static string InputFileLocation = @"D:\ProgrammingProjects\IO folders\ImageProcessing\Input";
    static string OutputFileLocation = @"D:\ProgrammingProjects\IO folders\ImageProcessing\Output";


    public static void Main()
    {
        Console.WriteLine("Beginning image processing now...");
        LoadingBar ProgressBar = new LoadingBar(Console.CursorLeft, Console.CursorTop);
        int CurrentImageCount = 0;
        int TotalNumberOfImages = Directory.GetFiles(InputFileLocation, "*.*", SearchOption.TopDirectoryOnly).Length;

        foreach (string FileName in Directory.GetFiles(InputFileLocation))
        {
            ProgressBar.SetTotalProgressPercentage(100 * ((double) CurrentImageCount / TotalNumberOfImages));
            Console.Write("Currently processing image " + ++CurrentImageCount + " of " + TotalNumberOfImages + "\n");

            string ImageName = FileName;

            // loops through removing folder names from filepath to find image file name
            while (ImageName.IndexOf("\\") != -1)
            {
                ImageName = ImageName.Substring(ImageName.IndexOf("\\") + 1);
            }

            // removes file extension from image file name
            ImageName = (ImageName.Substring(0, ImageName.IndexOf(".")));
            
            Bitmap InputImage = new Bitmap(FileName);

            // call methods here
            InvertImage(ProgressBar, ConvertToSingleColorImage(ProgressBar, InputImage, Color.White)).Save(OutputFileLocation + "\\" + ImageName + " in white inverted.jpg");
        }
        ProgressBar.SetTotalProgressPercentage(100);
        Console.WriteLine("\nImage processing is complete.\nPress Enter to confirm");
        Console.ReadKey();

    }


    /* -----
        this method accepts a passed bitmap and converts each pixel into a value of only a single color
    ----- */
    public static Bitmap ConvertToSingleColorImage(LoadingBar ProgressBar, Bitmap PassedImage, Color PassedColor)
    {

        double BrightnessRatio = 0;

        for (int CurrentRow = 0; CurrentRow < PassedImage.Height; CurrentRow++)
        {
            for (int CurrentColumn = 0; CurrentColumn < PassedImage.Width; CurrentColumn++)
            {
                BrightnessRatio = ((double) FindBrightnessAsInt(PassedImage, CurrentColumn, CurrentRow) / 255);

                PassedImage.SetPixel(CurrentColumn, CurrentRow, Color.FromArgb(
                (int) (BrightnessRatio * PassedColor.A), 
                (int) (BrightnessRatio * PassedColor.R), 
                (int) (BrightnessRatio * PassedColor.G), 
                (int) (BrightnessRatio * PassedColor.B)));
            }
            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) CurrentRow / (double) PassedImage.Height));
        }

        return PassedImage;
    }


    /* -----
        this method accepts a passed bitmap and inverts the color of each pixel
    ----- */
    public static Bitmap InvertImage(LoadingBar ProgressBar, Bitmap PassedImage)
    {
        int NewAlphaValue = 0;
        int NewRedValue = 0;
        int NewGreenValue = 0;
        int NewBlueValue = 0;

        for (int CurrentRow = 0; CurrentRow < PassedImage.Height; CurrentRow++)
        {
            for (int CurrentColumn = 0; CurrentColumn < PassedImage.Width; CurrentColumn++)
            {
                NewAlphaValue = (255 - PassedImage.GetPixel(CurrentColumn, CurrentRow).A);
                NewRedValue = (255 - PassedImage.GetPixel(CurrentColumn, CurrentRow).R);
                NewGreenValue = (255 - PassedImage.GetPixel(CurrentColumn, CurrentRow).G);
                NewBlueValue = (255 - PassedImage.GetPixel(CurrentColumn, CurrentRow).B);
                PassedImage.SetPixel(CurrentColumn, CurrentRow, Color.FromArgb(NewAlphaValue, NewRedValue, NewGreenValue, NewBlueValue));
            }
            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) CurrentRow / (double) PassedImage.Height));
        }

        return PassedImage;
    }


    /* -----
        this method accepts a passed bitmap and inverts the color of each pixel, but inverts around the average color, instead of a total inversion
    ----- */
    public static Bitmap InvertImageAroundAverage(Bitmap PassedImage)
    {
        int NewAlphaValue = 0;
        int NewRedValue = 0;
        int NewGreenValue = 0;
        int NewBlueValue = 0;
        Color AverageColor = FindAverageColor(PassedImage);

        for (int CurrentRow = 0; CurrentRow < PassedImage.Height; CurrentRow++)
        {
            for (int CurrentColumn = 0; CurrentColumn < PassedImage.Width; CurrentColumn++)
            {
                NewAlphaValue = (128 + ((AverageColor.A - PassedImage.GetPixel(CurrentColumn, CurrentRow).A) / 2));
                NewRedValue = (128 + ((AverageColor.R - PassedImage.GetPixel(CurrentColumn, CurrentRow).R) / 2));
                NewGreenValue = (128 + ((AverageColor.G - PassedImage.GetPixel(CurrentColumn, CurrentRow).G) / 2));
                NewBlueValue = (128 + ((AverageColor.B - PassedImage.GetPixel(CurrentColumn, CurrentRow).B) / 2));
                PassedImage.SetPixel(CurrentColumn, CurrentRow, Color.FromArgb(NewAlphaValue, NewRedValue, NewGreenValue, NewBlueValue));
            }
        }

        return PassedImage;   
    }

    
    /* -----
        this method takes a passed bitmap, and breaks each row into its own bitmap to be sorted. Then copies that bitmap back into the original.
    ----- */
    public static Bitmap PixelSortByRows(LoadingBar ProgressBar, Bitmap PassedImage)
    {
        Bitmap RowBitmap = new Bitmap(PassedImage.Width, 1);

        // this loop loops through all of the rows of pixels in the passed bitmap
        for (int Row = 0; Row < PassedImage.Height; Row++)
        {
            // this loop loops through each pixel in the row, and copies it to the individual row bitmap
            for (int Column = 0; Column < PassedImage.Width; Column++)
            {
                RowBitmap.SetPixel(Column, 0, PassedImage.GetPixel(Column, Row));
            }

            RowBitmap = ConvertArrayToBitmap(SortColorArray(GeneratePixelArray(RowBitmap)), RowBitmap.Width, 1);

            // this loop copies the row bitmap back into the original
            for (int Column = 0; Column < PassedImage.Width; Column++)
            {
                PassedImage.SetPixel(Column, Row, RowBitmap.GetPixel(Column, 0));
            }

            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) Row / (double) PassedImage.Height));
        }

        return PassedImage;
    }


    /* -----
        this method takes a passed bitmap, and breaks each column into its own bitmap to be sorted. Then copies that bitmap back into the original.
    ----- */
    public static Bitmap PixelSortByColumns(LoadingBar ProgressBar, Bitmap PassedImage)
    {
        Bitmap RowBitmap = new Bitmap(1, PassedImage.Height);

        // this loop loops through all of the columns of pixels in the passed bitmap
        for (int Column = 0; Column < PassedImage.Width; Column++)
        {
            // this loop loops through each pixel in the column, and copies it to the individual column bitmap
            for (int Row = 0; Row < PassedImage.Height; Row++)
            {
                RowBitmap.SetPixel(0, Row, PassedImage.GetPixel(Column, Row));
            }

            RowBitmap = ConvertArrayToBitmap(SortColorArray(GeneratePixelArray(RowBitmap)), 1, RowBitmap.Height);

            // this loop copies the row bitmap back into the original
            for (int Row = 0; Row < PassedImage.Height; Row++)
            {
                PassedImage.SetPixel(Column, Row, RowBitmap.GetPixel(0, Row));
            }

            ProgressBar.SetSubProcessProgressPercentage(100 * ((double) Column / (double) PassedImage.Width));
        }

        return PassedImage;
    }


    /* -----
        this method converts a passed Color Array into a Bitmap.
    ----- */
    public static Bitmap ConvertArrayToBitmap(Color[] PassedColorArray, int PassedWidth, int PassedHeight)
    {
        if (PassedColorArray.Length != (PassedWidth * PassedHeight))
        {
            throw new ArgumentException("Passed values have a mismatch. (PassedWidth * PassedHeight) != PassedColorArray.Length");
        }

        if ((PassedWidth < 1) || (PassedHeight < 1))
        {
            throw new ArgumentException("PassedWidth and PassedHeight must be greater than 0");
        }

        Bitmap ReturnBitmap = new Bitmap(PassedWidth, PassedHeight);

        for (int Row = 0; Row < PassedHeight; Row++)
        {
            for (int Column = 0; Column < PassedWidth; Column++)
            {
                ReturnBitmap.SetPixel(Column, Row, PassedColorArray[(Row * PassedWidth) + Column]);
            }
        }

        return ReturnBitmap;
    }

    /* -----
        this method generates an array with the pixels in the passed bitmap
    ----- */
    public static Color[] GeneratePixelArray(Bitmap PassedImage)
    {
        if (PassedImage == null)
        {
            throw new ArgumentException("Passed Bitmap cannot be null");
        }

        Color[] PixelArray = new Color[PassedImage.Width * PassedImage.Height];

        for (int Row = 0; Row < PassedImage.Height; Row++)
        {
            for (int Column = 0; Column < PassedImage.Width; Column++)
            {
                PixelArray[(Row * PassedImage.Width) + Column] = PassedImage.GetPixel(Column, Row);
            }
        }

        return PixelArray;
    }


    /* -----
        this method accepts an array of colors, and sorts them
        currently uses Merge Sort
    ----- */
    public static Color[] SortColorArray(Color[] ArrayToSort)
    {
        //return ColorArrayQuickSort(ArrayToSort, 0, ArrayToSort.Length - 1);
        return ColorArrayMergeSort(ArrayToSort);
    }


    /* -----
        this method uses a recursive merge sort algorithm on the passed Color array
    ----- */
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


    /* -----
        this method compares the passed colors
        currently simply compares the average rgb value of the colors
    ----- */
    public static Boolean CompareColors(string Operation, Color FirstColor, Color SecondColor)
    {
        double FirstColorRGBAverage = ((FirstColor.R + FirstColor.G + FirstColor.B) / 3);
        double SecondColorRGBAverage = ((SecondColor.R + SecondColor.G + SecondColor.B) / 3);

        switch (Operation)
        {
            case "=":
                return (FirstColorRGBAverage == SecondColorRGBAverage);

            case "<":
                return (FirstColorRGBAverage < SecondColorRGBAverage);

            case ">":
                return (FirstColorRGBAverage > SecondColorRGBAverage);

            case "<=":
                return (FirstColorRGBAverage <= SecondColorRGBAverage);

            case ">=":
                return (FirstColorRGBAverage >= SecondColorRGBAverage);

            default:
                throw new ArgumentException("passed operation was not recognized");
        }
    }
    

    /* -----
    this method accepts a bitmap and finds the average color value
    ----- */
    public static Color FindAverageColor(Bitmap PassedImage)
    {
        long AverageAlphaValue = 0;
        long AverageRedValue = 0;
        long AverageGreenValue = 0;
        long AverageBlueValue = 0;
        int NumberOfPixels = 0;

        for (int CurrentRow = 0; CurrentRow < PassedImage.Height; CurrentRow++)
        {
            for (int CurrentColumn = 0; CurrentColumn < PassedImage.Width; CurrentColumn++)
            {
                AverageAlphaValue += PassedImage.GetPixel(CurrentColumn, CurrentRow).A;
                AverageRedValue += PassedImage.GetPixel(CurrentColumn, CurrentRow).R;
                AverageGreenValue += PassedImage.GetPixel(CurrentColumn, CurrentRow).G;
                AverageBlueValue += PassedImage.GetPixel(CurrentColumn, CurrentRow).B;
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


    /* -----
        this method accepts a bitmap as input, and then calls the ActualResize method on it, until it is of a minimum size.
        should be updated to also call noise reduction on the image
    ----- */
    public static Bitmap Resize(LoadingBar ProgressBar, Bitmap PassedImage)
    {
        // minimum width and height required to return image
        int MinimumWidth = 3840;
        int MinimumHeight = 2160;

        while ((PassedImage.Width < MinimumWidth) || (PassedImage.Height < MinimumHeight))
        {
            PassedImage = ActualResize(ProgressBar, PassedImage);
        }

        return PassedImage;
    }


    /* -----
        this method accepts a bitmap as input, and returns a bitmap of double the size. it currently calls GetAverageColor to determine what color to use for newly created pixels.
        this method shouldn't be called directly, instead call from Resize.
    ----- */  
    private static Bitmap ActualResize(LoadingBar ProgressBar, Bitmap InputImage)
    {
        if (InputImage != null)
        {
            // for newly created bitmaps, all pixels are given rgb value 0, 0, 0
            Bitmap OutputImage = new Bitmap((InputImage.Width * 2), (InputImage.Height * 2));

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
            
            //OutputImage.Save(OutputFileLocation + @"\OutputFile.jpg");
            //Console.SetCursorPosition(originalX, originalY);
        }
        else
        {
            throw new ArgumentException("Input Image cannot be null");
        }
    }


    /* -----
        this method just magnifies the input image, creating a four pixel square for each input pixel
    ----- */
    private static Bitmap Magnify(LoadingBar ProgressBar, Bitmap PassedBitmap)
    {
        if (PassedBitmap != null)
        {
            double Progress = 0;
            Bitmap OutputImage = new Bitmap((PassedBitmap.Width * 2), (PassedBitmap.Height * 2));

            for (int j = 0; j < PassedBitmap.Height; j++)
            {
                for (int i = 0; i < PassedBitmap.Width; i++)
                {
                    OutputImage.SetPixel(2 * i, 2 * j, PassedBitmap.GetPixel(i, j));
                    OutputImage.SetPixel((2 * i) + 1, 2 * j, PassedBitmap.GetPixel(i, j));
                    OutputImage.SetPixel(2 * i, (2 * j) + 1, PassedBitmap.GetPixel(i, j));
                    OutputImage.SetPixel((2 * i) + 1, (2 * j) + 1, PassedBitmap.GetPixel(i, j));
                }

                Progress = (100 * ((double) j / (double) PassedBitmap.Height));
                ProgressBar.SetSubProcessProgressPercentage(Progress);
            }

            return OutputImage;
        }
        else
        {
            throw new ArgumentException("Passed Bitmap cannot be null");
        }
    }


    /* -----
        this method finds the average (mean) color of a given set of pixels depending on the input:
        horiz: averages only pixels to the left and right of passed pixel
        vert: averages only pixels above and below the passed pixel
        diag: averages pixels diagonal to the passed pixel
    ----- */
    private static Color GetAverageColor(string ComparisonDirection, Bitmap PassedImage, int CurrentPixelX, int CurrentPixelY)
    {
        if (PassedImage == null)
        {
            throw new ArgumentException("passed bitmap paramter for PassedImage is null");
        }

        if (CurrentPixelX >= 0 && CurrentPixelY >= 0 && CurrentPixelX < PassedImage.Width && CurrentPixelY < PassedImage.Height)
        {
            List<Color> NearbyPixels = new List<Color>();

            switch (ComparisonDirection)
            {
                case "diag":
                    NearbyPixels.Add(PassedImage.GetPixel(CurrentPixelX - 1, CurrentPixelY - 1));
                    NearbyPixels.Add(PassedImage.GetPixel(CurrentPixelX - 1, CurrentPixelY + 1));
                    NearbyPixels.Add(PassedImage.GetPixel(CurrentPixelX + 1, CurrentPixelY - 1));
                    NearbyPixels.Add(PassedImage.GetPixel(CurrentPixelX + 1, CurrentPixelY + 1));
                    break;

                case "vert":
                    NearbyPixels.Add(PassedImage.GetPixel(CurrentPixelX, CurrentPixelY - 1));
                    NearbyPixels.Add(PassedImage.GetPixel(CurrentPixelX, CurrentPixelY + 1));
                    break;

                case "horiz":
                    NearbyPixels.Add(PassedImage.GetPixel(CurrentPixelX - 1, CurrentPixelY));
                    NearbyPixels.Add(PassedImage.GetPixel(CurrentPixelX + 1, CurrentPixelY));
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
            throw new ArgumentException("Passed X and Y values must be greater than 0 and less than the passed bitmap's width and height respectively");
        }
    }


    /* -----
    this method uses anisotropic diffusion to reduce image noise.
    ----- */

    /// <param name="dt">Heat difusion value. Upper = more rapid convergence. usually 0 < dt <= 1/3 </param>
    /// <param name="lambda">The shape of the diffusion coefficient g(), controlling the Perona Malik diffusion g(delta) = 1/((1 +  delta2)  / lambda2). Upper = more blurred image & more noise removed</param>
    /// <param name="iterations">Determines the maximum number of iteration steps of the filter. Upper = less speed & more noise removed</param>

    private static Bitmap ADReduceNoise(LoadingBar ProgressBar, Bitmap PassedImage, double dt, int lambda, int iterations)
    {
        if (PassedImage != null)
        {
            // todo
            //test parameters
            if (dt < 0)
                throw new Exception("DT negative value not allowed");
            if (lambda < 0)
                throw new Exception("lambda must be greater than 0");
            if (iterations <= 0)
                throw new Exception("Iterations must be greater than 0");

            //Make temp bitmap
            Bitmap ReturnBitmap = (Bitmap) PassedImage.Clone();

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
                for (int h = 0; h < PassedImage.Height; h++)
                    for (int w = 0; w < PassedImage.Width; w++)
                    {
                        int px = w - 1;
                        int nx = w + 1;
                        int py = h - 1;
                        int ny = h + 1;
                        if (px < 0)
                            px = 0;
                        if (nx >= PassedImage.Width)
                            nx = PassedImage.Width - 1;
                        if (py < 0)
                            py = 0;
                        if (ny >= PassedImage.Height)
                            ny = PassedImage.Height - 1;


                        int current = PassedImage.GetPixel(w, h).A;

                        int AlphaValue = (int) (precal[255 + current - PassedImage.GetPixel(px, h).A] +
                                        precal[255 + current - PassedImage.GetPixel(nx, h).A] +
                                        precal[255 + current - PassedImage.GetPixel(w, py).A] +
                                        precal[255 + current - PassedImage.GetPixel(w, ny).A] +
                                        PassedImage.GetPixel(w, h).A);


                        current = PassedImage.GetPixel(w, h).R;

                        int RedValue = (int) (precal[255 + current - PassedImage.GetPixel(px, h).R] +
                                        precal[255 + current - PassedImage.GetPixel(nx, h).R] +
                                        precal[255 + current - PassedImage.GetPixel(w, py).R] +
                                        precal[255 + current - PassedImage.GetPixel(w, ny).R] +
                                        PassedImage.GetPixel(w, h).R);


                        current = PassedImage.GetPixel(w, h).G;

                        int GreenValue = (int) (precal[255 + current - PassedImage.GetPixel(px, h).G] +
                                        precal[255 + current - PassedImage.GetPixel(nx, h).G] +
                                        precal[255 + current - PassedImage.GetPixel(w, py).G] +
                                        precal[255 + current - PassedImage.GetPixel(w, ny).G] +
                                        PassedImage.GetPixel(w, h).G);

                        current = PassedImage.GetPixel(w, h).B;

                        int BlueValue = (int) (precal[255 + current - PassedImage.GetPixel(px, h).B] +
                                        precal[255 + current - PassedImage.GetPixel(nx, h).B] +
                                        precal[255 + current - PassedImage.GetPixel(w, py).B] +
                                        precal[255 + current - PassedImage.GetPixel(w, ny).B] +
                                        PassedImage.GetPixel(w, h).B);
                        

                        ReturnBitmap.SetPixel(w, h, Color.FromArgb(AlphaValue, RedValue, GreenValue, BlueValue));
                    }
            }

            return ReturnBitmap;
        }
        else
        {
            throw new ArgumentException("Passed bitmap cannot be null");
        }
    }


    /* -----
        this method will search the passed bitmap for pixels that lie on an edge and update the pixels
        currently checks only pixels that were copied directly from original image, and updates newly created pixels
    ----- */
    private static Bitmap SharpenEdges(LoadingBar ProgressBar, Bitmap PassedImage)
    {
        if (PassedImage != null)
        {
            // represents how close a pixel r, g, or b value must be to be considered similar
            double SimilarityRange = 0;  
            Bitmap ReturnBitmap = (Bitmap) PassedImage.Clone();

            // four nested for loops?! there must be a better way to do this. look into improvements.
            for (int j = 4; j < PassedImage.Height - 4; j += 2)
            {
                for (int i = 4; i < PassedImage.Width - 4; i += 2)
                {
                    bool[,] SimilarPixels = new bool[3, 3];
                    for (int a = i - 2; a <= i + 2; a += 2)
                    {
                        for (int b = j - 2; b <= j + 2; b += 2)
                        {
                            // set the flag to false by default
                            SimilarPixels[((a - i) + 2) / 2, ((b - j) + 2) / 2] = false;

                            // if the pixels a, r, g, and b values are within the similarity range, flag as true
                            if ((PassedImage.GetPixel(a, b).A > (PassedImage.GetPixel(i, j).A - SimilarityRange) && (PassedImage.GetPixel(a, b).A < (PassedImage.GetPixel(i, j).A + SimilarityRange))) &&
                                (PassedImage.GetPixel(a, b).R > (PassedImage.GetPixel(i, j).R - SimilarityRange) && (PassedImage.GetPixel(a, b).R < (PassedImage.GetPixel(i, j).R + SimilarityRange))) &&
                                (PassedImage.GetPixel(a, b).G > (PassedImage.GetPixel(i, j).G - SimilarityRange) && (PassedImage.GetPixel(a, b).G < (PassedImage.GetPixel(i, j).G + SimilarityRange))) &&
                                (PassedImage.GetPixel(a, b).B > (PassedImage.GetPixel(i, j).B - SimilarityRange) && (PassedImage.GetPixel(a, b).B < (PassedImage.GetPixel(i, j).B + SimilarityRange))))
                            {
                                SimilarPixels[((a - i) + 2) / 2, ((b - j) + 2) / 2] = true;
                            }
                        }
                    }

                    // check for '\' edge
                    if (SimilarPixels[0, 0] && !SimilarPixels[0, 1] && !SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && !SimilarPixels[2, 0] && !SimilarPixels[2, 1] && SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i - 1, j + 1, PassedImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j - 1, PassedImage.GetPixel(i, j));
                    }

                    // check for '/' edge
                    if (!SimilarPixels[0, 0] && !SimilarPixels[0, 1] && SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i - 1, j - 1, PassedImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j + 1, PassedImage.GetPixel(i, j));
                    }

                    // check for '-' edge
                    if (!SimilarPixels[0, 0] && !SimilarPixels[0, 1] && !SimilarPixels[0, 2] && SimilarPixels[1, 0] && SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i - 1, j, PassedImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j, PassedImage.GetPixel(i, j));
                    }

                    // check for '|' edge
                    if (!SimilarPixels[0, 0] && SimilarPixels[0, 1] && !SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && !SimilarPixels[2, 0] && SimilarPixels[2, 1] && !SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i, j + 1, PassedImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i, j - 1, PassedImage.GetPixel(i, j));
                    }

                    // check for 'X' edge
                    if (SimilarPixels[0, 0] && !SimilarPixels[0, 1] && SimilarPixels[0, 2] && !SimilarPixels[1, 0] && !SimilarPixels[1, 2] && SimilarPixels[2, 0] && !SimilarPixels[2, 1] && SimilarPixels[2, 2])
                    {
                        ReturnBitmap.SetPixel(i - 1, j - 1, PassedImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i - 1, j + 1, PassedImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j - 1, PassedImage.GetPixel(i, j));
                        ReturnBitmap.SetPixel(i + 1, j + 1, PassedImage.GetPixel(i, j));
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


    /* -----
        this method accepts a bitmap and converts it into ascii characters, then saves the created text file
    ----- */
    private static void ConvertImageToAscii(Bitmap PassedImage, string PassedFileName)
    {

        string OutputAsciiChars = " _.,-=+:;abc!?0123456789$W#@Ñ";
        //string OutputAsciiChars = "Ñ@#W$9876543210?!abc;:+=-,._ ";

        if (PassedImage != null)
        {
            char[] OutputChars = OutputAsciiChars.ToCharArray();
            double ConversionValue = (256.0 / (OutputChars.Length - 1));
            StreamWriter OutputFile = File.CreateText(PassedFileName);

            for (int j = 0; j < PassedImage.Height; j++)
            {
                for (int i = 0; i < PassedImage.Width; i++)
                {
                        OutputFile.Write(" " + OutputChars[(int) ((FindBrightnessAsInt(PassedImage, i, j) / ConversionValue))]);
                }
                OutputFile.Write("\n");
            }
        }
        else
        {
            throw new ArgumentException("Passed bitmap cannot be null");
        }
    }


    /* -----
        this method returns the converted brightness value for the passed pixel in the passed bitmap
    ----- */
    private static int FindBrightnessAsInt(Bitmap PassedImage, int PositionX, int PositionY)
    {
        // current logic weighs r, g, and b values equally
        //return (int) ((PassedImage.GetPixel(PositionX, PositionY).R + PassedImage.GetPixel(PositionX, PositionY).G + PassedImage.GetPixel(PositionX, PositionY).B) / 3);
        // below formula returns Luminance
        return (int) ((PassedImage.GetPixel(PositionX, PositionY).R * 0.3) + (PassedImage.GetPixel(PositionX, PositionY).G * 0.59) + (PassedImage.GetPixel(PositionX, PositionY).B * 0.11));
    }


    /* -----
        this method will search the passed bitmap for pixels that lie on an edge. Then returns an array representing edges found
    ----- */
    private static char[,] FindEdges(Bitmap PassedImage)
    {
        if (PassedImage != null)
        {
            // represents how close a pixel r, g, or b value must be to be considered similar
            double SimilarityRange = 5;  
            char[,] ReturnArray = new char[PassedImage.Width, PassedImage.Height];

            // four nested for loops?! there must be a better way to do this. look into improvements.
            for (int j = 1; j < PassedImage.Height - 1; j ++)
            {
                for (int i = 1; i < PassedImage.Width - 1; i++)
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
                            if ((PassedImage.GetPixel(a, b).A > (PassedImage.GetPixel(i, j).A - SimilarityRange) && (PassedImage.GetPixel(a, b).A < (PassedImage.GetPixel(i, j).A + SimilarityRange))) &&
                                (PassedImage.GetPixel(a, b).R > (PassedImage.GetPixel(i, j).R - SimilarityRange) && (PassedImage.GetPixel(a, b).R < (PassedImage.GetPixel(i, j).R + SimilarityRange))) &&
                                (PassedImage.GetPixel(a, b).G > (PassedImage.GetPixel(i, j).G - SimilarityRange) && (PassedImage.GetPixel(a, b).G < (PassedImage.GetPixel(i, j).G + SimilarityRange))) &&
                                (PassedImage.GetPixel(a, b).B > (PassedImage.GetPixel(i, j).B - SimilarityRange) && (PassedImage.GetPixel(a, b).B < (PassedImage.GetPixel(i, j).B + SimilarityRange))))
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