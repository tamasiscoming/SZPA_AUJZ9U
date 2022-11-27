using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using static System.Windows.Forms.LinkLabel;
using System.Collections;
using System.Data.Common;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using Image = System.Drawing.Image;

namespace SZPA
{
    internal class Program
    {
        private static string gifSplitImages = "GifSplitImages";

        private static List<string> gifSplitList;
        private static string[] gifSplitArray;

        static string path;
        static int imageWidth = 0;
        static string asciiImage;
        static TimeSpan timeImage_SeqImage;
        static TimeSpan timeImage_SeqImageOneFor;
        static TimeSpan timeImage_ParallelSimple;
        static TimeSpan timeImage_ParDataParallel;

        private static TimeSpan timeGif_SeqP_SeqA_TwoFor;
        private static TimeSpan timeGif_SeqP_SeqA_OneFor;
        private static TimeSpan timeGif_ParP_SeqA;
        private static TimeSpan timeGif_SeqP_ParA; 
        private static TimeSpan timeGif_ParP_ParA; 
        private static TimeSpan timeGif_ParP_ParDataPar; 

        static readonly string asciiCharsType1 = " .:-=+*#%@";
        static readonly string asciiCharsType2 = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/|()1{}[]?-_+~<>i!lI;:,\"^`'. ";
        static string selectedCharset;
        static string selection = "";
        static string project = "";

        static readonly Stopwatch sw = new Stopwatch();

        [STAThread]
        static void Main(string[] args)
        {
            Setup();
            Console.ReadLine();
        }

        #region ---------------------------AsciiImage---------------------------
        #region ____________________Sequentional Approach____________________
        #region ____________________SeqImage____________________
        static string SeqImage(string filepath)
        {
            Bitmap image = new Bitmap(filepath);
            int width = image.Width;
            int height = image.Height;
            int pixelCount = width * height;

            string asciiImage = "";

            Rectangle rect = new Rectangle(0, 0, width, height);

            int depth = Bitmap.GetPixelFormatSize(image.PixelFormat);

            BitmapData bitmapData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

            int step = depth / 8;

            byte[] pixels = new byte[(pixelCount * step)];
            IntPtr iptr = bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(iptr, pixels, 0, pixels.Length);

            for (int y = rect.Y; y < rect.Height; y++)
            {
                for (int x = rect.X; x < rect.Width; x++)
                {
                    int index = (y * height + x) * step;
                    byte b = pixels[index];
                    byte g = pixels[index + 1];
                    byte r = pixels[index + 2];

                    double grayScale = (r * 0.3) + (g * 0.59) + (b * 0.11);

                    asciiImage += GetSpecificCharacterForEachPixel(grayScale, selectedCharset);
                }

                asciiImage += "\n";
            }

            Marshal.Copy(pixels, 0, iptr, pixels.Length);
            image.UnlockBits(bitmapData);
            return asciiImage;
        }
        #endregion

        #region ____________________SeqImageOneFor____________________
        private static string SeqImageOneFor(string filepath)
        {
            Bitmap image = new Bitmap(filepath);
            int width = image.Width;
            int height = image.Height;
            int pixelCount = width * height;

            string asciiImage = "";

            Rectangle rect = new Rectangle(0, 0, width, height);

            int depth = Bitmap.GetPixelFormatSize(image.PixelFormat);

            BitmapData bitmapData =
                image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

            int step = depth / 8;

            byte[] pixels = new byte[(pixelCount * step)];
            IntPtr iptr = bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(iptr, pixels, 0, pixels.Length);

            for (int j = 0; j < pixels.Length - 3; j += 3)
            {
                byte b = pixels[j];
                byte g = pixels[j + 1];
                byte r = pixels[j + 2];

                double grayScale = (r * 0.3) + (g * 0.59) + (b * 0.11);

                asciiImage += GetSpecificCharacterForEachPixel(grayScale, selectedCharset);

                if ((j + 3) % width == 0)
                {
                    asciiImage += "\n";
                }
            }

            Marshal.Copy(pixels, 0, iptr, pixels.Length);
            image.UnlockBits(bitmapData);
            return asciiImage;
        }
        #endregion
        #endregion

        #region ____________________Parallel Approach____________________
        #region ____________________ParSimple____________________
        private static string ParallelSimple(string filepath)
        {
            Bitmap image = new Bitmap(filepath);
            int width = image.Width;
            int height = image.Height;
            int pixelCount = width * height;

            imageWidth = width;

            Rectangle rect = new Rectangle(0, 0, width, height);

            int depth = Bitmap.GetPixelFormatSize(image.PixelFormat);

            BitmapData bitmapData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

            int step = depth / 8;

            byte[] pixels = new byte[(pixelCount * step)];
            IntPtr iptr = bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(iptr, pixels, 0, pixels.Length);

            char[] charPixels = new char[pixelCount];

            Parallel.For(0, charPixels.Length, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, i =>
            {
                int index = i * step;

                byte b = pixels[index];
                byte g = pixels[index + 1];
                byte r = pixels[index + 2];

                double grayScale = (r * 0.3) + (g * 0.59) + (b * 0.11);

                charPixels[i] = GetSpecificCharacterForEachPixel(grayScale, selectedCharset);
            });

            Marshal.Copy(pixels, 0, iptr, pixels.Length);
            image.UnlockBits(bitmapData);

            return new string(charPixels);
        }
        #endregion

        #region ____________________ParDataParallel____________________
        private static string ParDataParallel(string filepath)
        {
            int cpuCount = Environment.ProcessorCount;
            Bitmap image = new Bitmap(filepath);
            int width = image.Width;
            int height = image.Height;
            int pixelCount = width * height;
            imageWidth = width;
            Rectangle rect = new Rectangle(0, 0, width, height);

            int depth = Bitmap.GetPixelFormatSize(image.PixelFormat);

            BitmapData bitmapData =
                image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

            int step = depth / 8;

            byte[] pixels = new byte[(pixelCount * step)];
            int partialArrayCount = pixels.Length / cpuCount;
            IntPtr iptr = bitmapData.Scan0;
            string[] asciiImage = new string[cpuCount];
            
            // Copy data from pointer to array
            Marshal.Copy(iptr, pixels, 0, pixels.Length);
            Task[] tasks = new Task[cpuCount];

            for (int i = 0; i < cpuCount; i++)
            {
                int j = i;
                byte[] partialArray = pixels.Skip(j * partialArrayCount).Take(partialArrayCount).ToArray();

                Task t = new Task(() => asciiImage[j] = CalcCharacters(partialArray, width));
                t.Start();
                tasks[j] = t;
            }

            Task.WaitAll(tasks);

            return string.Join("", asciiImage);
        }
        #endregion
        #endregion
        #endregion

        #region ---------------------------Setup/SideQuests---------------------------
        private static string CalcCharacters(byte[] partialArray, int width)
        {
            string partialAsciiImage = "";
            int rowCount = 0;

            for (int j = 0; j < partialArray.Length - 3; j += 3)
            {
                byte b = partialArray[j];
                byte g = partialArray[j + 1];
                byte r = partialArray[j + 2];

                double grayScale = (r * 0.3) + (g * 0.59) + (b * 0.11);

                partialAsciiImage += GetSpecificCharacterForEachPixel(grayScale, selectedCharset);

                if (j % width == 0)
                {
                    rowCount++;
                    //partialAsciiImage += "\n";
                }
            }
            return partialAsciiImage;
        }

        private static void MakeAsciiImage(string filepath)
        {
            Stopwatch sw = new Stopwatch();
            #region ---------------------------Setup_SeqImage()---------------------------
            Console.WriteLine("---------------------------SeqImage()---------------------------");
            Thread.Sleep(1000);
            asciiImage = String.Empty;
            sw.Start();
            asciiImage = SeqImage(filepath);
            sw.Stop();
            timeImage_SeqImage = sw.Elapsed;
            WriteAsciiArtToFile("SeqImage");
            #endregion

            #region ---------------------------Setup_SeqImageOneFor()---------------------------
            Console.WriteLine("---------------------------SeqImageOneFor()---------------------------");
            Thread.Sleep(1000);
            asciiImage = String.Empty;
            sw.Reset();
            sw.Start();
            asciiImage = SeqImageOneFor(filepath);
            sw.Stop();
            timeImage_SeqImageOneFor = sw.Elapsed;
            WriteAsciiArtToFile("SeqImageOneFor");
            #endregion

            #region ---------------------------Setup_ParallelSimple()---------------------------
            Console.WriteLine("---------------------------ParallelSimple()---------------------------");
            Thread.Sleep(1000);
            asciiImage = String.Empty;
            sw.Reset();
            sw.Start();
            asciiImage = ParallelSimple(filepath);
            sw.Stop();
            timeImage_ParallelSimple = sw.Elapsed;
            asciiImage = AddNewLineToImages(asciiImage);
            WriteAsciiArtToFile("ParallelSimple");
            #endregion

            #region ---------------------------Setup_ParDataParallel()---------------------------
            Console.WriteLine("---------------------------ParDataParallel---------------------------");
            Thread.Sleep(1000);
            asciiImage = String.Empty; 
            sw.Reset();
            sw.Start();
            asciiImage = ParDataParallel(filepath);
            sw.Stop();
            timeImage_ParDataParallel = sw.Elapsed;
            asciiImage = AddNewLineToImages(asciiImage);
            WriteAsciiArtToFile("ParDataParallel");
            #endregion

            ImageResults(timeImage_SeqImage, timeImage_SeqImageOneFor, timeImage_ParallelSimple, timeImage_ParDataParallel);
        }

        static void ImageResults(TimeSpan time_SeqImage, TimeSpan time_SeqImageOneFor, TimeSpan time_ParallelSimple, TimeSpan time_ParDataParallel)
        {
            Console.Clear();
            Console.WriteLine("Measured time of each approach...");
            Console.WriteLine("Sequential approach:");
            Console.SetCursorPosition(21, 2);
            Console.WriteLine("First approach: {0} -> {1}ms", time_SeqImage, time_SeqImage.TotalSeconds);
            Console.SetCursorPosition(21, 3);
            Console.WriteLine("Second approach: {0} -> {1}ms", time_SeqImageOneFor, time_SeqImageOneFor.TotalSeconds);
            Console.WriteLine("Parallel approach:");
            Console.SetCursorPosition(21, 5);
            Console.WriteLine("First approach: {0} -> {1}ms", time_ParallelSimple, time_ParallelSimple.TotalSeconds);
            Console.SetCursorPosition(21, 6);
            Console.WriteLine("Second approach: {0} -> {1}ms", time_ParDataParallel, time_ParDataParallel.TotalSeconds);
        }

        static void SelectCharacterSet()
        {
            Console.Clear();
            Console.WriteLine("Select Ascii Character set by pressing 1 or 2 then enter...");
            Console.WriteLine("Set 1: " + asciiCharsType1);
            Console.WriteLine("Set 2: " + asciiCharsType2);

            selection = Console.ReadLine();

            if (selection == "1")
            {
                selectedCharset = asciiCharsType1;
                Console.Clear();
                Console.WriteLine("You selected set 1: " + asciiCharsType1);
            }
            else if (selection == "2")
            {
                selectedCharset = asciiCharsType2;
                Console.Clear();
                Console.WriteLine("You selected set 2: " + asciiCharsType2);
            }
        }

        static void Setup()
        {
            Console.WriteLine("Select Ascii project by pressing 1 or 2 then enter...");
            Console.WriteLine("Press 1 for converting image");
            Console.WriteLine("Press 2 for converting gif");

            project = Console.ReadLine();

            if (project == "1")
            {
                SelectCharacterSet();
                Console.WriteLine("Select an image file...");
                BrowseFile(project);
            }
            else if (project == "2")
            {
                SelectCharacterSet();
                Console.WriteLine("Select a gif file...");
                BrowseFile(project);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Wrong answer, select a correct answer...");
                Setup();
            }
            
            Thread.Sleep(1000);
        }

        static void BrowseFile(string project)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (project == "1")
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg) | *.jpg; *.jpeg;";
                openFileDialog.ShowDialog();
                openFileDialog.Multiselect = false;
                path = openFileDialog.FileName;
                MakeAsciiImage(path);   
            }
            else if (project == "2")
            {
                openFileDialog.Filter = "Image files (*.gif) | *.gif;";
                openFileDialog.ShowDialog();
                openFileDialog.Multiselect = false;
                path = openFileDialog.FileName;
                MakeAsciiGif(path);
            }
        }

        static char GetSpecificCharacterForEachPixel(double grayScaleFactor, string selectedCharset)
        {
            return selectedCharset[(int)Math.Ceiling(((selectedCharset.Length - 1) * grayScaleFactor) / 255)];
        }

        static string AddNewLineToImages(string asciiFrameImagesWithoutNewLines)
        {
            return Regex.Replace(asciiFrameImagesWithoutNewLines, ".{" + (imageWidth) + "}", "$0\n"); 
        }

        private static void WriteAsciiArtToFile(string type)
        {
            string finalPath = path + "_CharSet_" + selection + "_" + type + "_ascii.txt";
            Console.WriteLine($"Writing {type} to file...");

            File.WriteAllText(finalPath, asciiImage);
            Process p = new Process();
            p.StartInfo.FileName = finalPath;
            p.Start();
        }
        #endregion

        #region ---------------------------AsciiGif---------------------------
        private static void MakeAsciiGif(string filepath)
        {
            SeqGifProcessor();
            SeqAsciiGen_SeqA_TwoFor();
            SeqAsciiGen_SeqA_OneFor();
            ParAsciiGen_SeqA();
            SeqAsciiGen_ParA();
            ParAsciiGen_ParA();
            ParAsciiGen_ParDataPar();
            
            DisplayElapsedTimes();
            DisplayGifToConsoleFromArray();
            
            Console.ReadLine();
        }

        #region ---------------------------DisplaysToConsole---------------------------
        static void DisplayElapsedTimes()
        {
            Console.Clear();
            Console.WriteLine(
                $"Sequential process, sequential algorithm 2 for loops: {timeGif_SeqP_SeqA_TwoFor}");
            Console.WriteLine(
                $"Sequential process, sequential algorithm 1 for loops: {timeGif_SeqP_SeqA_OneFor}");
            Console.WriteLine(
                $"Parallel process, sequential algorithm 1 for loops: {timeGif_ParP_SeqA}");
            Console.WriteLine($"Sequential process, parallel algorithm: {timeGif_SeqP_ParA}");
            Console.WriteLine($"Parallel process, parallel algorithm: {timeGif_ParP_ParA}");
            Console.WriteLine($"Parallel process, DATA parallel algorithm: {timeGif_ParP_ParDataPar}");
        }

        static void DisplayGifToConsoleFromArray()
        {
            Console.WriteLine("Press any key to display the animation...");
            Console.ReadKey();
            foreach (string asciiFrame in gifSplitArray)
            {
                Console.Clear();
                Console.WriteLine(asciiFrame);
                Thread.Sleep(10);
            }
        }
        #endregion

        #region ---------------------------AddLines---------------------------
        static List<string> AddNewLineToGifImages(List<string> asciiFrameImagesWithoutNewLines)
        {
            List<string> newList = new List<string>();
            foreach (string asciiFrame in asciiFrameImagesWithoutNewLines)
            {
                List<string> l = Enumerable
                    .Range(0, asciiFrame.Length / imageWidth)
                    .Select(i => asciiFrame.Substring(i * imageWidth, imageWidth))
                    .ToList();
                newList.Add(string.Join("\n", l));
            }

            return newList;
        }

        static string[] AddNewLineToGifImages(string[] asciiFrameImagesWithoutNewLines)
        {
            List<string> returnList = new List<string>();
            for (int i = 0; i < asciiFrameImagesWithoutNewLines.Length; i++)
            {
                if (asciiFrameImagesWithoutNewLines[i] == null)
                {
                    asciiFrameImagesWithoutNewLines[i] = String.Empty;
                }

                List<string> l = Enumerable
                    .Range(0, asciiFrameImagesWithoutNewLines[i].Length / imageWidth)
                    .Select(j => asciiFrameImagesWithoutNewLines[i].Substring(j * imageWidth, imageWidth))
                    .ToList();
                returnList.Add(string.Join("\n", l));
            }

            return returnList.ToArray();
        }
        #endregion

        #region ---------------------------PreProcess---------------------------
        private static void SeqGifProcessor()
        {
            gifSplitList = new List<string>();
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            string gifPath = path;
            SeqCreateImagesFromGif(gifPath);
        }

        static void SeqCreateImagesFromGif(string gifPath)
        {
            Console.WriteLine("Generating jpgs from gif...");
            bool exists = Directory.Exists(gifSplitImages);

            if (!exists)
            {
                Directory.CreateDirectory(gifSplitImages);
            }
            else
            {
                Directory.Delete(gifSplitImages, true);
                Directory.CreateDirectory(gifSplitImages);
            }

            Image gifImg = Image.FromFile(gifPath);
            FrameDimension dimension = new FrameDimension(gifImg.FrameDimensionsList[0]);

            // Number of frames
            int frameCount = gifImg.GetFrameCount(dimension);

            Image[] frames = new Image[frameCount];
            string[] nameSplitHelper = path.Split('\\')[path.Split('\\').Length - 1].Split('.');


            for (int i = 0; i < frameCount; i++)
            {
                // Return an Image at a certain index
                gifImg.SelectActiveFrame(dimension, i);
                frames[i] = ((Image)gifImg.Clone());

                Console.WriteLine(gifSplitImages + "/" + nameSplitHelper[0] + $"_{i}.jpg");
                frames[i].Save(gifSplitImages + "/" + nameSplitHelper[0] + $"_{i}.jpg", ImageFormat.Jpeg);
            }
        }
        #endregion

        #region ---------------------------GifProcessAlgs---------------------------
        static void SeqAsciiGen_SeqA_TwoFor()
        {
            Console.WriteLine("Sequential AsciiGenerator with Sequential ImageProcess 2 for loops...");
            Thread.Sleep(2000);

            string[] imgPaths = Directory.GetFiles(gifSplitImages, "*.jpg",
                SearchOption.TopDirectoryOnly);

            gifSplitList.Clear();

            Console.WriteLine($"Gif split list contains {imgPaths.Length} images \n");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < imgPaths.Length; i++)
            {
                string asciiImage = SeqImage(imgPaths[i]);
                gifSplitList.Add(asciiImage);
                Console.WriteLine($"{i}. split from the list is ready...");
            }

            sw.Stop();
            timeGif_SeqP_SeqA_TwoFor = sw.Elapsed;
        }

        static void SeqAsciiGen_SeqA_OneFor()
        {
            Console.Clear();
            Console.WriteLine("Sequential AsciiGenerator with Sequential ImageProcess 1 for loop...");
            Thread.Sleep(2000);

            string[] imgPaths = Directory.GetFiles(gifSplitImages, "*.jpg",
                SearchOption.TopDirectoryOnly);

            gifSplitList.Clear();


            Console.WriteLine($"Gif split list contains {imgPaths.Length} images \n");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < imgPaths.Length; i++)
            {
                string asciiImage = SeqImageOneFor(imgPaths[i]);
                gifSplitList.Add(asciiImage);
                //UpdateCurrentLine($"{i}. ascii img is done");
                Console.WriteLine($"{i}. split from the list is ready...");
            }

            sw.Stop();
            timeGif_SeqP_SeqA_OneFor = sw.Elapsed;
        }

        static void ParAsciiGen_SeqA()
        {
            Console.Clear();
            Console.WriteLine("Parallel AsciiGenerator with Sequential ImageProcess 1 for loop...");
            Thread.Sleep(2000);

            string[] imgPaths = Directory.GetFiles(gifSplitImages, "*.jpg",
                SearchOption.TopDirectoryOnly);

            Console.WriteLine($"Gif split list contains {imgPaths.Length} images \n");

            gifSplitArray = new string[imgPaths.Length];

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.For(0, imgPaths.Length, i =>
            {
                int j = i;
                string asciiImage = SeqImageOneFor(imgPaths[j]);

                gifSplitArray[j] = asciiImage;
                Console.WriteLine($"{j}. split from the list is ready...");
            });

            sw.Stop();
            timeGif_ParP_SeqA = sw.Elapsed;
        }

        static void SeqAsciiGen_ParA()
        {
            Console.Clear();
            Console.WriteLine("Sequential AsciiGenerator with Parallel ImageProcess...");
            Thread.Sleep(2000);

            string[] imgPaths = Directory.GetFiles(gifSplitImages, "*.jpg",
                SearchOption.TopDirectoryOnly);

            gifSplitList.Clear();


            Console.WriteLine($"Gif split list contains {imgPaths.Length} images \n");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < imgPaths.Length; i++)
            {
                string asciiImage = ParallelSimple(imgPaths[i]);
                gifSplitList.Add(asciiImage);
                Console.WriteLine($"{i}. split from the list is ready...");
            }

            sw.Stop();
            timeGif_SeqP_ParA = sw.Elapsed;

            gifSplitList = AddNewLineToGifImages(gifSplitList);
        }

        static void ParAsciiGen_ParA()
        {
            Console.Clear();
            Console.WriteLine("Parallel AsciiGenerator with parallel ImageProcess 1 for loop...");
            Thread.Sleep(2000);

            string[] imgPaths = Directory.GetFiles(gifSplitImages, "*.jpg",
                SearchOption.TopDirectoryOnly);

            Console.WriteLine($"Gif split list contains {imgPaths.Length} images \n");

            gifSplitArray = new string[imgPaths.Length];

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.For(0, imgPaths.Length, i =>
            {
                int j = i;
                string asciiImage = ParallelSimple(imgPaths[j]);

                gifSplitArray[j] = asciiImage;
                Console.WriteLine($"{j}. split from the list is ready...");
            });

            sw.Stop();
            timeGif_ParP_ParA = sw.Elapsed;

            gifSplitArray = AddNewLineToGifImages(gifSplitArray);
        }

        static void ParAsciiGen_ParDataPar()
        {
            Console.Clear();
            Console.WriteLine("Parallel AsciiGenerator with DATA parallel ImageProcess 1 for loop...");
            Thread.Sleep(2000);

            string[] imgPaths = Directory.GetFiles(gifSplitImages, "*.jpg",
                SearchOption.TopDirectoryOnly);

            Console.WriteLine($"Gif split list contains {imgPaths.Length} images \n");

            gifSplitArray = new string[imgPaths.Length];

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.For(0, imgPaths.Length, i =>
            {
                int j = i;
                string asciiImage = ParDataParallel(imgPaths[j]);

                gifSplitArray[j] = asciiImage;
                Console.WriteLine($"{j}. split from the list is ready...");
            });

            sw.Stop();
            timeGif_ParP_ParDataPar = sw.Elapsed;
            gifSplitArray = AddNewLineToGifImages(gifSplitArray);
        }
        #endregion
        #endregion
    }
}
