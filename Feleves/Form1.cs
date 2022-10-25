using System.Drawing.Imaging;
using System.Xml.Linq;

namespace Feleves
{
    public partial class Form1 : Form
    {
        #region Variables
        int imageWidth = 0;
        int imageHeight = 0;
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        #region Button actions
        private void Browse_Click(object sender, EventArgs e)
        {
            Browsing();
        }

        private void ASCIIArt_Click(object sender, EventArgs e)
        {
            ASCIIArt_Generator();
        }
        #endregion

        #region Actions
        void Browsing()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                originalImage.Image = new Bitmap(open.FileName);
                imageWidth = originalImage.Width;
                imageHeight = originalImage.Height;
            }
        }

        void SaveToFile(Image img, string name)
        {
            Bitmap bmp = new Bitmap(imageWidth, imageHeight);
            newImage.DrawToBitmap(bmp, new Rectangle(0, 0, imageWidth, imageHeight));
            bmp.Save(name, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        void ASCIIArt_Generator()
        {
            newImage.Image = MakeGrayscale3((Bitmap)originalImage.Image);
        }
        #endregion
    }
}