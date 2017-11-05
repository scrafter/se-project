using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageViewer.Model
{
    class MagnifyingGlass : ITool
    {

        public MagnifyingGlass()
        {
            Cursor cur = new Cursor("./Resources/CursorImages/magnifying-glass.cur");

            
        }

        public void AffectImage()
        {

        }
    }
}
