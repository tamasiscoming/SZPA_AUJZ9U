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

namespace SZPA
{
    internal class Program
    {
        static Bitmap newImage = FileBrowse();
        static string asciiChars = " .:-=+*#%@";
        static Stopwatch sw = new Stopwatch();
        static string path = "test.txt";
        static List<string> lines;
        static int dividedBy = newImage.Width / 100;

        [STAThread]
        static void Main(string[] args)
        {
            //SeqMakeArt(newImage);
            ParMakeArt(newImage);

            Console.WriteLine("Finished MakeArt()");
            Thread.Sleep(1000);
            Console.ReadLine();
            Environment.Exit(0);
        }

        #region Parallel Approach
        #region ???
        static void ParMakeArt(Bitmap newImage)
        {
            sw.Start();
            // most kell valahova lockolni lock (lockolo) {...}
            Parallel.For(0, newImage.Height, row => {
                for (int column = 0; column < newImage.Width; column++)
                {
                    var pixel = newImage.GetPixel(column, row);
                    var avg = (pixel.R + pixel.G + pixel.B) / 3;
                    var c = asciiChars[avg * asciiChars.Length / 255 % asciiChars.Length];

                    Console.SetCursorPosition(column, row);
                    Console.Write(c);
                }

                //lines.Append(c.ToString());
            });

            //var lockolo = new Object();
            //Parallel.For(0, newImage.Size.Width, e =>
            //{
            //    lock (lockolo)
            //    {
            //        var pixel = newImage.GetPixel(newImage.Width, newImage.Height);
            //        var avg = (pixel.R + pixel.G + pixel.B) / 3;
            //        var c = asciiChars[avg * asciiChars.Length / 255 % asciiChars.Length];

            //        Console.SetCursorPosition(newImage.Width, newImage.Height);
            //        Console.Write(c);
            //        lines.Append(c.ToString());
            //    }

            //    lines.Append("\n");
            //});

            //Thread[] threads = new Thread[newImage.Size.Height];

            //for (int row = 0; row < newImage.Height; row++)
            //{
            //    threads[row] = new Thread(() => ThreadJob(newImage, row));
            //    threads[row].Start();
            //}

            sw.Stop();
            Console.WriteLine(sw.Elapsed);
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

        #region Sequentional Approach
        static void SeqMakeArt(Bitmap newImage)
        {
            sw.Start();

            newImage = new Bitmap(newImage, new Size(newImage.Width / dividedBy, newImage.Height / dividedBy));
            lines = new List<string>();

            for (int i = 0; i < newImage.Height; i++)
            {
                string line = "";
                for (int j = 0; j < newImage.Width; j++)
                {
                    var pixel = newImage.GetPixel(j, i);
                    var avg = (pixel.R + pixel.G + pixel.B) / 3;

                    var c = asciiChars[avg * asciiChars.Length / 255 % asciiChars.Length];
                    Console.Write(c);
                    line += c.ToString();
                    lines.Append(c.ToString());
                }
                lines.Add(line + "\n");
            }

            Console.WriteLine("-----------------------------------------------------------------------------------------------------------" +
                "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");

            File.WriteAllLines(path, lines);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
        #endregion

        #region Setup
        static Bitmap FileBrowse()
        {
            Bitmap originalImage = null;

            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";

            if (open.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(open.FileName);
            }

            return originalImage;
        }
        #endregion
    }
}
