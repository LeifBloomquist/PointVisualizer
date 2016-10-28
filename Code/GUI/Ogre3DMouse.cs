using Mogre;
using PointVisualizer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace PointVisualizer
{
    public partial class OgreForm 
    {
        // 3D Mouse Handlers
        const float MOUSE_SPEED_FACTOR = (30f / 350.0f);
        const float ROLL_SPEED_FACTOR = (0.01f / 350.0f);

        private void Init3DMouse()
        {
            // 3D Mouse Support
            Mouse3DHelper helper = new Mouse3DHelper(this.Handle);
            helper.my3DxMouse.ButtonEvent += my3DxMouse_ButtonEvent;
            helper.my3DxMouse.MotionEvent += my3DxMouse_MotionEvent;

            if (helper.NumberOf3DxMice > 0)
            {
                Mouse3DLabel.BackColor = Color.Green;
                Mouse3DLabel.Text = "3D";
            }
            else
            {
                Mouse3DLabel.BackColor = Color.Black;
                Mouse3DLabel.Text = "--";
            }
        }

        private void my3DxMouse_MotionEvent(object sender, _3DxMouse._3DxMouse.MotionEventArgs e)
        {
            int tx = 0, ty = 0, tz = 0, rx = 0, ry = 0, rz = 0;

            // Translation Vector?
            if (e.TranslationVector != null)
            {
                tx = e.TranslationVector.X;
                ty = e.TranslationVector.Y;
                tz = e.TranslationVector.Z;
            }

            // Rotation Vector?
            if (e.RotationVector != null)
            {
                rx = e.RotationVector.X;
                ry = e.RotationVector.Y;
                rz = e.RotationVector.Z;
            }

            float scale=1f;

            if (manager.FastMove)
            {
                scale = 100f;
            }

            float f_tx = tx * MOUSE_SPEED_FACTOR * scale;
            float f_ty =-tz * MOUSE_SPEED_FACTOR * scale;  // Note y,z swapped and z negative
            float f_tz = ty * MOUSE_SPEED_FACTOR * scale;
            float f_rx = rx * ROLL_SPEED_FACTOR;
            float f_ry = ry * ROLL_SPEED_FACTOR;
            float f_rz = rz * ROLL_SPEED_FACTOR;

            Vector3 vec = new Vector3(f_tx,f_ty,f_tz);

            cam.MoveRelative(vec);
    
            cam.Pitch(f_rx);
            cam.Roll(f_ry); 
            cam.Yaw(-f_rz);   // Note negative
        }

        private void my3DxMouse_ButtonEvent(object sender, _3DxMouse._3DxMouse.ButtonEventArgs e)
        {
            if ((e.ButtonMask.Pressed & 0x1) == 0x1)
            {
                manager.FastMove = true;
                Mouse3DLabel.BackColor = Color.LightGreen;
            }
            else
            {
                manager.FastMove = false;
                Mouse3DLabel.BackColor = Color.Green;
            }
        }
    }
}
