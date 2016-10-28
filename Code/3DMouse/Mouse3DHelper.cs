using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using _3DxMouse;

namespace PointVisualizer
{
    class Mouse3DHelper : IMessageFilter
    {
        public _3DxMouse._3DxMouse my3DxMouse { get; private set; }

        public int NumberOf3DxMice { get; private set; }

        public Mouse3DHelper(IntPtr Handle)
        {
            // Connect to Raw Input & find devices
            my3DxMouse = new _3DxMouse._3DxMouse(Handle);
            NumberOf3DxMice = my3DxMouse.EnumerateDevices();

            if (NumberOf3DxMice > 0)
            {
                // Capture Window events
                Application.AddMessageFilter(this);
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (my3DxMouse != null)
            {
                my3DxMouse.ProcessMessage(m);
            }
            return false;
        }
    }
}
