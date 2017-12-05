using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageViewer.Model
{
    class MagnifyingGlass : ITool
    {

        public MagnifyingGlass()
        {
           
        }
        public void AffectImage(Dictionary<String, Object> args)
        {
        }

        public Tools GetToolEnum()
        {
            return Tools.Magnifier;
        }
    }
}
