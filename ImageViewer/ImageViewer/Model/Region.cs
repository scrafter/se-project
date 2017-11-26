using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;

namespace ImageViewer.Model
{
    public class Region
    {
        public Image Image { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; }

        public Region(Point position, Size size, string name) 
        {
            Position = position;
            Size = size;
            Image = new Image();
            Image.Extension = ".png";
            Image.FileName = name;
            Image.FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\Regions\" + Image.FileName + Image.Extension;
        }

        public void Save()
        {
            BitmapSource source = new BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\temp.png"));
            try
            {
                if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\Regions"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\Regions");
                }
                MemoryStream TransportStream = new MemoryStream();
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(source));
                enc.Save(TransportStream);
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(TransportStream);
                bmp.Save(Image.FilePath);
            }
            catch(Exception)
            {
                MessageBox.Show("Save failed");
                return;
            }
        }
    }
}
