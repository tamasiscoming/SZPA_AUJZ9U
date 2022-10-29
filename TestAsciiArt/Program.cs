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

namespace TestAsciiArt
{
    internal class Program
    {
        static Bitmap newImage = null;
        static string asciiChars = " .:-=+*#%@";
        static Stopwatch sw = new Stopwatch();

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Please browse an image");
            Thread.Sleep(1000);

            newImage = FileBrowse();
            MakeArt(newImage);

            Console.WriteLine("Finished MakeArt()");
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        static void MakeArt(Bitmap newImage)
        {
            sw.Start();
            var dividedBy = newImage.Width / 100;
            var height = newImage.Height;

            newImage = new Bitmap(newImage, new Size(newImage.Width / dividedBy, newImage.Height / dividedBy));
            List<string> lines = new List<string>();
            var path = "test.txt";

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
                }
                lines.Add(line + "\n");

                Console.WriteLine();    
            }

            Console.WriteLine("-----------------------------------------------------------------------------------------------------------");

            File.WriteAllLines(path, lines);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

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
    }
}
