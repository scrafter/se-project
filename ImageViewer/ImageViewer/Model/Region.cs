﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.ObjectModel;
using ImageViewer.View;
using System.Threading;

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
        public BitmapImage Bitmap { get; set; }

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

                if(File.Exists(Image.FilePath))
                {
                    File.Delete(Image.FilePath);
                }

                if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\Regions"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\Regions");
                }
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\temp.png");
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (var fileStream = new System.IO.FileStream(Image.FilePath, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                    fileStream.Close();
                }
                
                Bitmap = new BitmapImage();
                Bitmap.BeginInit();
                Bitmap.UriSource = new Uri(Image.FilePath);
                Bitmap.CacheOption = BitmapCacheOption.OnLoad;
                Bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                Bitmap.EndInit();

                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show("Save failed.");
                return false;
            }
        }
    }
}