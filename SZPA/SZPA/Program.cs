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
       
        static string asciiChars2 = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/|()1{}[]?-_+~<>i!lI;:,\"^`'. ";
        static string asciiChars = " .:-=+*#%@";
        static Stopwatch sw = new Stopwatch();
        static string path = "test.txt";
        static List<string> lines;

        [STAThread]
        static void Main(string[] args)
        {
            FileBrowse();
            WriteAsciiArtToFile();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        #region ----------------------------------------Parallel Approach
        #region ________________________________________GetPixel________________________________________
        static void ParMakeArt(Bitmap newImage)
        {
            //var pixel = newImage.GetPixel(j, i);
            //var avg = (pixel.R + pixel.G + pixel.B) / 3;
            //var c = asciiChars[avg * asciiChars.Length / 255 % asciiChars.Length];
            //Console.Write(c);
        }
        #endregion

        static void ThreadJob(Bitmap newImage, int row)
        {
            lines = new List<string>();
            for (int column = 0; column < newImage.Width; column++)
            {
                var pixel = newImage.GetPixel(column, row);
                var avg = (pixel.R + pixel.G + pixel.B) / 3;
                var c = asciiChars[avg * asciiChars.Length / 255 % asciiChars.Length];

                Console.SetCursorPosition(row, column);
                Console.Write(c);
                lines.Append(c.ToString());
            }

            lines.Add("\n");
            File.WriteAllLines(path, lines);
        }
        #endregion

        #region ----------------------------------------Sequentional Approach----------------------------------------
        #region ________________________________________GetPixel________________________________________
        static void SeqGetPixel(Bitmap newImage)
        {
            //newImage = new Bitmap(newImage, new Size(newImage.Width / dividedBy, newImage.Height / dividedBy));
            lines = new List<string>();

            for (int y = 0; y < newImage.Height; y++)
            {
                string line = "";
                for (int x = 0; x < newImage.Width; x++)
                {
                    var pixel = newImage.GetPixel(x, y);
                    var avg = (pixel.R + pixel.G + pixel.B) / 3;
                    var c = asciiChars[avg * asciiChars.Length / 255 % asciiChars.Length];
                    line += c.ToString();
                    Console.Write(c);
                }
                lines.Add(line + "\n");
            }
            File.WriteAllLines(path, lines);
        }
        #endregion

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

        static char GetCharacterForPixel(double grayScaleFactor)
        {
            return asciiChars[(int)Math.Ceiling(
                ((asciiChars.Length - 1) * grayScaleFactor) / 255)];
        }

        #endregion

        #region ----------------------------------------Setup----------------------------------------
        private static void MakeAsciiArts(string filepath)
        {
            Stopwatch sw = new Stopwatch();

            // Todo: uncomment this if you want to use sequential solution
            Console.WriteLine("SequentialAsciiImageConverter started...");
            sw.Start();
            _asciiImage = SeqOne(filepath);
            sw.Stop();
            //_sequentialAsciiImageConverterTime = sw.Elapsed;

            //_asciiImage = String.Empty;
            //sw.Reset();
            //Console.WriteLine("SequentialAsciiImageConverterOneFor started...");
            //sw.Start();
            //_asciiImage = SequentialAsciiImageConverterOneFor(filepath);
            //sw.Stop();
            //_sequentialAsciiImageConverterOneForTime = sw.Elapsed;

            //_asciiImage = String.Empty;
            //sw.Reset();
            //Console.WriteLine("ParallelAsciiImageConverter started...");
            //sw.Start();
            //_asciiImage = ParallelAsciiImageConverter(filepath);
            //sw.Stop();
            //_parallelAsciiImageConverterTime = sw.Elapsed;
            //_asciiImage = InsertNewLineToAsciiImages(_asciiImage);

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
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg;";
            openFileDialog.ShowDialog();
            openFileDialog.Multiselect = false;
            _filePath = openFileDialog.FileName;

            MakeAsciiArts(_filePath);
        }

        static string InsertNewLineToAsciiImages(string asciiFrameImagesWithoutNewLines)
        {
            return Regex.Replace(asciiFrameImagesWithoutNewLines, ".{" + (_imgWidth + 1) + "}", "$0\n"); // NOTE: .txt has a maximum char number of each line which is 1024
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
        #endregion
    }
}
