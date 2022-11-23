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

namespace SZPA
{
    internal class Program
    {
        static string _filePath;
        static int _imgWidth = 0;
        static string _asciiImage;
        static TimeSpan time_SeqOne;
        static TimeSpan time_SeqOneFor;
        static TimeSpan time_ParallelSimple;
        static TimeSpan time_ParDataParallel;

        static string asciiCharsType1 = " .:-=+*#%@";
        static string asciiCharsType2 = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/|()1{}[]?-_+~<>i!lI;:,\"^`'. ";
        static string selectedCharset;
        static string selection = "";
        static Stopwatch sw = new Stopwatch();

        [STAThread]
        static void Main(string[] args)
        {
            Setup();

            ShowResults(time_SeqOne, time_SeqOneFor, time_ParallelSimple, time_ParDataParallel);
            Console.ReadLine();
        }
        
        #region ---------------------------Sequentional Approach---------------------------
        #region ____________________SeqOne____________________
        static string SeqOne(string filepath)
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

                    asciiImage += GetCharacterForPixel(grayScale, selectedCharset);
                }

                asciiImage += "\n";
            }

            Marshal.Copy(pixels, 0, iptr, pixels.Length);
            image.UnlockBits(bitmapData);
            return asciiImage;
        }
        #endregion

        #region ____________________SeqOneFor____________________
        private static string SeqOneFor(string filepath)
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

                asciiImage += GetCharacterForPixel(grayScale, selectedCharset);

                if ((j+3) % width == 0 && j != 0)
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

        #region ---------------------------Parallel Approach---------------------------
        #region ____________________ParSimple____________________
        private static string ParallelSimple(string filepath)
        {
            Bitmap image = new Bitmap(filepath);
            int width = image.Width;
            int height = image.Height;
            int pixelCount = width * height;

            _imgWidth = width;

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

                charPixels[i] = GetCharacterForPixel(grayScale, selectedCharset);
            });

            Marshal.Copy(pixels, 0, iptr, pixels.Length);
            image.UnlockBits(bitmapData);

            return new string(charPixels);
        }
        #endregion
        
        #region ParDataParallel
        private static string ParDataParallel(string filepath)
        {
            int cpuCount = Environment.ProcessorCount;
            Bitmap image = new Bitmap(filepath);
            int width = image.Width;
            int height = image.Height;
            int pixelCount = width * height;
            _imgWidth = width;
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

                Task t = new Task(() => asciiImage[j] = CalculateCharactersForPartialArray(partialArray, width));
                t.Start();
                tasks[j] = t;
            }

            Task.WaitAll(tasks);

            return string.Join("", asciiImage);
        }
        #endregion
        #endregion
        #region ---------------------------Setup/SideQuests---------------------------
        private static string CalculateCharactersForPartialArray(byte[] partialArray, int width)
        {
            string partialAsciiImage = "";
            int rowCount = 0;

            for (int j = 0; j < partialArray.Length - 3; j += 3)
            {
                byte b = partialArray[j];
                byte g = partialArray[j + 1];
                byte r = partialArray[j + 2];

                double grayScale = (r * 0.3) + (g * 0.59) + (b * 0.11);

                partialAsciiImage += GetCharacterForPixel(grayScale, selectedCharset);

                if (j % width == 0)
                {
                    rowCount++;
                    //partialAsciiImage += "\n";
                }
            }
            return partialAsciiImage;
        }

        private static void MakeAsciiArts(string filepath)
        {
            Stopwatch sw = new Stopwatch();
            #region ---------------------------Setup_SeqOne()---------------------------
            Console.WriteLine("---------------------------SeqOne()---------------------------");
            Thread.Sleep(1000);
            _asciiImage = String.Empty;
            sw.Start();
            _asciiImage = SeqOne(filepath);
            sw.Stop();
            time_SeqOne = sw.Elapsed;
            WriteAsciiArtToFile("SeqOne");
            #endregion

            #region ---------------------------Setup_SeqOneFor()---------------------------
            Console.WriteLine("---------------------------SeqOneFor()---------------------------");
            Thread.Sleep(1000);
            _asciiImage = String.Empty;
            sw.Reset();
            sw.Start();
            _asciiImage = SeqOneFor(filepath);
            sw.Stop();
            time_SeqOneFor = sw.Elapsed;
            WriteAsciiArtToFile("SeqOneFor");
            #endregion

            #region ---------------------------Setup_ParallelSimple()---------------------------
            Console.WriteLine("---------------------------ParallelSimple()---------------------------");
            Thread.Sleep(1000);
            _asciiImage = String.Empty;
            sw.Reset();
            sw.Start();
            _asciiImage = ParallelSimple(filepath);
            sw.Stop();
            time_ParallelSimple = sw.Elapsed;
            _asciiImage = InsertNewLineToAsciiImages(_asciiImage);
            WriteAsciiArtToFile("ParallelSimple");
            #endregion

            #region ---------------------------Setup_ParDataParallel()---------------------------
            Console.WriteLine("---------------------------ParDataParallel---------------------------");
            Thread.Sleep(1000);
            _asciiImage = String.Empty; 
            sw.Reset();
            sw.Start();
            _asciiImage = ParDataParallel(filepath);
            sw.Stop();
            time_ParDataParallel = sw.Elapsed;
            _asciiImage = InsertNewLineToAsciiImages(_asciiImage);
            WriteAsciiArtToFile("ParDataParallel");
            #endregion
        }

        static void ShowResults(TimeSpan time_seqOne, TimeSpan time_seqOneFor, TimeSpan time_ParallelSimple, TimeSpan time_ParDataParallel)
        {
            Console.Clear();
            Console.WriteLine("Measured time of each approach...");
            Console.WriteLine("Sequential approach:");
            Console.SetCursorPosition(21, 3);
            Console.WriteLine("First approach: {0} -> {1}ms", time_seqOne, time_seqOne.Milliseconds);
            Console.SetCursorPosition(21, 4);
            Console.WriteLine("Second approach: {0} -> {1}ms", time_seqOneFor, time_seqOneFor.Milliseconds);
            Console.WriteLine("Parallel approach:");
            Console.SetCursorPosition(21, 6);
            Console.WriteLine("First approach: {0} -> {1}ms", time_ParallelSimple, time_ParallelSimple.Milliseconds);
            Console.SetCursorPosition(21, 7);
            Console.WriteLine("Second approach: {0} -> {1}ms", time_ParDataParallel, time_ParDataParallel.Milliseconds);
        }

        static void Setup()
        {
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

            Thread.Sleep(1000);

            Console.WriteLine("Select an image file...");
            Thread.Sleep(1000);

            FileBrowse();
            Thread.Sleep(1000);
        }

        static void FileBrowse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg) | *.jpg; *.jpeg;";
            openFileDialog.ShowDialog();
            openFileDialog.Multiselect = false;
            _filePath = openFileDialog.FileName;

            MakeAsciiArts(_filePath);
        }

        static char GetCharacterForPixel(double grayScaleFactor, string selectedCharset)
        {
            return selectedCharset[(int)Math.Ceiling(((selectedCharset.Length - 1) * grayScaleFactor) / 255)];
        }

        static string InsertNewLineToAsciiImages(string asciiFrameImagesWithoutNewLines)
        {
            // NOTE: .txt has a maximum char number of each line which is 1024
            return Regex.Replace(asciiFrameImagesWithoutNewLines, ".{" + (_imgWidth) + "}", "$0\n"); 
        }

        private static void WriteAsciiArtToFile(string type)
        {
            string finalPath = _filePath + "_CharSet_" + selection + "_" + type + "_ascii.txt";
            Console.WriteLine($"Writing {type} to file...");

            File.WriteAllText(finalPath, _asciiImage);
            Process p = new Process();
            p.StartInfo.FileName = finalPath;
            p.Start();
        }
        #endregion
    }
}
