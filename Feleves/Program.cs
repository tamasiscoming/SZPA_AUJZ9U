using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Feleves
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Bitmap originalImage = null;
            int imageWidth = 0;
            int imageHeight = 0;

            Console.WriteLine("Press -B- to Browse an image");
            Console.WriteLine("Press -Q- to Quit the program");

            if (Console.ReadLine().ToUpper() == "B")
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";

                if (open.ShowDialog() == DialogResult.OK)
                {
                    originalImage = new Bitmap(open.FileName);
                    imageWidth = originalImage.Width;
                    imageHeight = originalImage.Height;
                }
            }
            else if (Console.ReadLine().ToUpper() == "Q")
            {
                Environment.Exit(0);
            }
        }
    }
}
