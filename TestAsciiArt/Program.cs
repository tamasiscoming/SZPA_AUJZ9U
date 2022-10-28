using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace TestAsciiArt
{
    internal class Program
    {
        static Bitmap newImage = null;
        static string asciiChars = "  .,:ilwW@@";
        static Stopwatch stopwatch = new Stopwatch();

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Please browse an image");
            Thread.Sleep(1000);

            newImage = FileBrowse();
            MakeArt(newImage);

            Console.ReadLine();
        }

        static void MakeArt(Bitmap newImage)
        {
            var dividedBy = newImage.Width / 130;

            newImage = new Bitmap(newImage, new Size(newImage.Width / dividedBy, newImage.Height / dividedBy));

            for (int i = 0; i < newImage.Height; i++)
            {
                for (int j = 0; j < newImage.Width; j++)
                {
                    var pixel = newImage.GetPixel(j, i);
                    var avg = (pixel.R + pixel.G + pixel.B) / 3;

                    var c = asciiChars[avg * asciiChars.Length / 255 % asciiChars.Length];

                    Console.Write(c);
                }
                Console.WriteLine();    
            }
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
