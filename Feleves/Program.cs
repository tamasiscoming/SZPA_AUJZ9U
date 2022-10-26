using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Feleves
{
    internal class Program
    {
        static int imageWidth = 0;
        static int imageHeight = 0;
        static Bitmap newImage = null;
        static int outputWidth = 20;

        // Typical width/height for ASCII characters
        private const double FontAspectRatio = 0.6;

        // Available character set, ordered by decreasing intensity (brightness)
        private const string OutputCharSet = "@%#*+=-:. ";

        // Alternate char set uses more chars, but looks less realistic
        private const string OutputCharSetAlternate = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ";


        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Please browse an image");
            Thread.Sleep(2000);

            newImage = FileBrowse();

            


            Console.ReadLine();
        }

        static void GenerateAsciiArt()
        {
            // pixelChunkWidth/pixelChunkHeight - size of a chunk of pixels that will
            // map to 1 character.  These are doubles to avoid progressive rounding
            // error.
            double pixelChunkWidth = (double)newImage.Width / (double)outputWidth;
            double pixelChunkHeight = pixelChunkWidth / FontAspectRatio;

            // Calculate output height to capture entire image
            int outputHeight = (int)Math.Round((double)newImage.Height / pixelChunkHeight);

            // Generate output image, row by row
            double pixelOffSetTop = 0.0;
            StringBuilder sbOutput = new StringBuilder();

            for (int row = 1; row <= outputHeight; row++)
            {
                double pixelOffSetLeft = 0.0;

                for (int col = 1; col <= outputWidth; col++)
                {
                    // Calculate brightness for this set of pixels by averaging
                    // brightness across all pixels in this pixel chunk
                    double brightSum = 0.0;
                    int pixelCount = 0;
                    for (int pixelLeftInd = 0; pixelLeftInd < (int)pixelChunkWidth; pixelLeftInd++)
                        for (int pixelTopInd = 0; pixelTopInd < (int)pixelChunkHeight; pixelTopInd++)
                        {
                            // Each call to GetBrightness returns value between 0.0 and 1.0
                            int x = (int)pixelOffSetLeft + pixelLeftInd;
                            int y = (int)pixelOffSetTop + pixelTopInd;
                            if ((x < newImage.Width) && (y < newImage.Height))
                            {
                                brightSum += newImage.GetPixel(x, y).GetBrightness();
                                pixelCount++;
                            }
                        }

                    // Average brightness for this entire pixel chunk, between 0.0 and 1.0
                    double pixelChunkBrightness = brightSum / pixelCount;

                    // Target character is just relative position in ordered set of output characters
                    int outputIndex = (int)Math.Floor(pixelChunkBrightness * OutputCharSet.Length);
                    if (outputIndex == OutputCharSet.Length)
                        outputIndex--;

                    char targetChar = OutputCharSet[outputIndex];

                    sbOutput.Append(targetChar);

                    pixelOffSetLeft += pixelChunkWidth;
                }
                sbOutput.AppendLine();
                pixelOffSetTop += pixelChunkHeight;
            }

            // Dump output string to file
            File.WriteAllText(outputFile, sbOutput.ToString());
        }

        static Bitmap FileBrowse()
        {
            Bitmap originalImage = null;

            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";

            if (open.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(open.FileName);
                imageWidth = originalImage.Width;
                imageHeight = originalImage.Height;
            }
            
            return originalImage;
        }
    }
}
