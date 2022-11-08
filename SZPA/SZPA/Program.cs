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
       
        static string asciiChars = " .:-=+*#%@";
        static string asciiChars2 = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/|()1{}[]?-_+~<>i!lI;:,\"^`'. ";
        static Stopwatch sw = new Stopwatch();

        [STAThread]
        static void Main(string[] args)
        {
            FileBrowse();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        #region ----------------------------------------Sequentional Approach----------------------------------------
        #region ________________________________________SeqOne________________________________________
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

                    asciiImage += GetCharacterForPixel(grayScale);
                }

                asciiImage += "\n";
            }

            Marshal.Copy(pixels, 0, iptr, pixels.Length);
            image.UnlockBits(bitmapData);
            return asciiImage;
        }
        #endregion

        // todo: fix print bug
        #region ________________________________________SeqOneFor________________________________________
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

                asciiImage += GetCharacterForPixel(grayScale);

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

        #region ----------------------------------------Parallel Approach----------------------------------------
        #region ________________________________________ParallelSimple________________________________________
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

                charPixels[i] = GetCharacterForPixel(grayScale);
            });

            Marshal.Copy(pixels, 0, iptr, pixels.Length);
            image.UnlockBits(bitmapData);

            return new string(charPixels);
        }
        #endregion
        #endregion

        #region ----------------------------------------Setup----------------------------------------
        private static void MakeAsciiArts(string filepath)
        {
            Stopwatch sw = new Stopwatch();
            #region --------------------Setup_SeqOne()--------------------
            // todo: uncomment this if you want to use sequential solution
            Console.WriteLine("--------------------SeqOne()--------------------");
            Thread.Sleep(1000);
            _asciiImage = String.Empty;
            sw.Start();
            _asciiImage = SeqOne(filepath);
            sw.Stop();
            //_sequentialasciiimageconvertertime = sw.elapsed;
            WriteAsciiArtToFile("SeqOne");
            #endregion

            #region --------------------Setup_SeqOneFor()--------------------
            Console.WriteLine("--------------------SeqOneFor()--------------------");
            Thread.Sleep(1000);
            _asciiImage = String.Empty;
            sw.Reset();
            sw.Start();
            _asciiImage = SeqOneFor(filepath);
            sw.Stop();
            //_sequentialasciiimageconverteronefortime = sw.Elapsed;
            WriteAsciiArtToFile("SeqOneFor");
            #endregion

            #region --------------------Setup_ParallelSimple()--------------------
            Console.WriteLine("--------------------ParallelSimple()--------------------");
            Thread.Sleep(1000);
            _asciiImage = String.Empty;
            sw.Reset();
            sw.Start();
            _asciiImage = ParallelSimple(filepath);
            sw.Stop();
            //_parallelAsciiImageConverterTime = sw.Elapsed;
            _asciiImage = InsertNewLineToAsciiImages(_asciiImage);
            WriteAsciiArtToFile("ParallelSimple");
            #endregion

            //_asciiImage = String.Empty;
            //sw.Reset();
            //Console.WriteLine("ParallelAsciiImageConverterUsingDataParallelism started...");
            //sw.Start();
            //_asciiImage = ParallelAsciiImageConverterUsingDataParallelism(filepath);
            //sw.Stop();
            //_parallelAsciiImageConverterUsingDataParallelismTime = sw.Elapsed;
            //_asciiImage = InsertNewLineToAsciiImages(_asciiImage);
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

        static char GetCharacterForPixel(double grayScaleFactor)
        {
            return asciiChars[(int)Math.Ceiling(((asciiChars.Length - 1) * grayScaleFactor) / 255)];
        }

        static string InsertNewLineToAsciiImages(string asciiFrameImagesWithoutNewLines)
        {
            // NOTE: .txt has a maximum char number of each line which is 1024
            return Regex.Replace(asciiFrameImagesWithoutNewLines, ".{" + (_imgWidth) + "}", "$0\n"); 
        }

        private static void WriteAsciiArtToFile()
        {
            string finalPath = _filePath + "_ascii.txt";
            Console.WriteLine("Writing file...");

            File.WriteAllText(finalPath, _asciiImage);
            Process openPrc = new Process();
            openPrc.StartInfo.FileName = finalPath;
            openPrc.Start();
        }

        private static void WriteAsciiArtToFile(string e)
        {
            string finalPath = _filePath + "_" + e + "_ascii.txt";
            Console.WriteLine($"Writing {e} to file...");

            File.WriteAllText(finalPath, _asciiImage);
            Process p = new Process();
            p.StartInfo.FileName = finalPath;
            p.Start();
        }
        #endregion
    }
}
