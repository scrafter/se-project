using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Model;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageViewer.Methods
{
    public class OutputSerializer
    {
        public OutputSerializer() { }

        public void SerializeList(ObservableCollection<ImageViewer.Model.Image> list, int regionWidth, int regionHeight, Thickness regionPosition)
        {

            if (regionWidth > 0 && regionHeight > 0)
            {
                int counter = 0;
                bool isWarned = false;
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.Cancel || result == System.Windows.Forms.DialogResult.None)
                        return;

                    foreach (ImageViewer.Model.Image image in list)
                    {
                        BitmapSource bitmapSource = new BitmapImage(new Uri(image.FilePath));
                        Bitmap bitmap;
                        using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                        {
                            int width = regionWidth;
                            int height = regionHeight;
                            Thickness position = regionPosition;
                            Normalize(ref width, ref height, ref position, bitmapSource);
                            bitmap = CreateRegion.GetBitmap(bitmapSource);
                            if ((int)position.Left + width > bitmap.Width || (int)position.Top + height > bitmap.Height)
                            {
                                isWarned = true;
                                if ((int)position.Left > bitmap.Width || (int)position.Top > bitmap.Height)
                                {
                                    continue;
                                }
                                else
                                {
                                    if ((int)position.Left + width > bitmap.Width)
                                        width = bitmap.Width - (int)position.Left;
                                    if ((int)position.Top + height > bitmap.Height)
                                        height = bitmap.Height - (int)position.Top;
                                }
                            }
                            bitmap = CreateRegion.GetBitmapFragment(bitmap, (int)position.Left, (int)position.Top, (int)width, (int)height);
                        }
                        String fileName = $"Out_{++counter}.png";
                        String path = dialog.SelectedPath + $"\\{fileName}";
                        if (File.Exists(path))
                        {
                            MessageBoxResult overwriteResult = MessageBox.Show($"{fileName} already exists in this location. Do you want to overwrite it?", "Confirmation", MessageBoxButton.YesNoCancel);
                            if (overwriteResult == MessageBoxResult.Yes)
                            {
                                bitmap.Save(path, ImageFormat.Png);
                            }
                            else if (overwriteResult == MessageBoxResult.No)
                            {
                                continue;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            bitmap.Save(path, ImageFormat.Png);
                        }
                    }
                }
                if(isWarned)
                    MessageBox.Show("Region exceeds size of one or more images. Those images will be ignored or their size will be reduced.");
            }
            else
            {
                MessageBox.Show("Region not selected.");
            }
        }
        private void Normalize(ref int width, ref int height, ref Thickness position, BitmapSource source)
        {
            position.Left = position.Left * source.DpiX / 96.0;
            position.Top = position.Top * source.DpiY / 96.0;
            width = (int)(width * source.DpiX / 96.0);
            height = (int)(height * source.DpiY / 96.0);
        }
    }
}
