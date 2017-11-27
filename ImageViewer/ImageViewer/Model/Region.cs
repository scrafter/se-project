using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.ObjectModel;
using ImageViewer.View;

namespace ImageViewer.Model
{
    public class Region
    {
        public Image Image { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; }
        public int DpiX { get; set;}
        public int DpiY { get; set; }
        public ObservableCollection<Image> ImageList { get; set; }
        public Image AttachedImage { get; set; }

        public Region(Point position, Size size, string name, Vector Dpi, ObservableCollection<Image> imageList, Image image) 
        {
            Position = position;
            Size = size;
            Image = new Image();
            Image.Extension = ".png";
            Image.FileName = name;
            Image.FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\Regions\" + Image.FileName + Image.Extension;
            DpiX = (int)Dpi.X;
            DpiY = (int)Dpi.Y;
            ImageList = imageList;
            AttachedImage = image;
        }

        public bool Save()
        {
            try
            {
                BitmapSource source = new BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\temp.png"));

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
                TransportStream.Close();
                TransportStream.Dispose();
                
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show("Save failed");
                return false;
            }
        }
    }
}
