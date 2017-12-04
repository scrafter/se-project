using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageViewer.Model
{
    public class Image
    {
        private string _filePath = String.Empty;

        public string FileName { get; set; }
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                if(_filePath!= null)
                    Bitmap = new BitmapImage(new Uri(_filePath));
            }
        }
        public string Extension { get ; set; }
        public BitmapImage Bitmap { get; set; }
    }
}
