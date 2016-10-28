using Mogre;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PointVisualizer
{
    public class CameraMan
    {
        private Camera mCamera;
        private bool mGoingForward;
        private bool mGoingBack;
        private bool mGoingRight;
        private bool mGoingLeft;
        private bool mGoingUp;
        private bool mGoingDown;
        private bool mFastMove;

        public CameraMan(Camera camera)
        {
            mCamera = camera;
        }

        public bool GoingForward
        {
            set { mGoingForward = value; }
            get { return mGoingForward; }
        }

        public bool GoingBack
        {
            set { mGoingBack = value; }
            get { return mGoingBack; }
        }

        public bool GoingLeft
        {
            set { mGoingLeft = value; }
            get { return mGoingLeft; }
        }

        public bool GoingRight
        {
            set { mGoingRight = value; }
            get { return mGoingRight; }
        }

        public bool GoingUp
        {
            set { mGoingUp = value; }
            get { return mGoingUp; }
        }

        public bool GoingDown
        {
            set { mGoingDown = value; }
            get { return mGoingDown; }
        }

        public bool FastMove
        {
            set { mFastMove = value; }
            get { return mFastMove; }
        }

        private Stopwatch processtime = new Stopwatch();

        public void Process()
        {
            float elapsed = (float)processtime.Elapsed.TotalMilliseconds;
            processtime.Restart();

            UpdateCamera(0.02f * elapsed);
        }

        public void UpdateCamera(float timeFragment)
        {
            // build our acceleration vector based on keyboard input composite
            var move = Vector3.ZERO;
            if (mGoingForward) move += mCamera.Direction;
            if (mGoingBack) move -= mCamera.Direction;
            if (mGoingRight) move += mCamera.Right;
            if (mGoingLeft) move -= mCamera.Right;
            if (mGoingUp) move += mCamera.Up;
            if (mGoingDown) move -= mCamera.Up;

            move.Normalise();
            move *= 150; // Natural speed is 150 units/sec.
            if (mFastMove)
            {
                move *= 10;  // Super fast
            }

            if (move != Vector3.ZERO)
                mCamera.Move(move * timeFragment);
        }

        public void MouseMovement(float x, float y)
        {
            mCamera.Yaw(-x * 0.0015f);
            mCamera.Pitch(-y * 0.0015f);
        }

        public void ProcessKeyDown(Keys k)
        {
            ProcessKeys(k, true);
        }

        public void ProcessKeyUp(Keys k)
        {
            ProcessKeys(k, false);
        }

        private void ProcessKeys(Keys k, bool state)
        {
            // Remove shift modifier  (shift key itself is detected separately)
            if ((k & Keys.Shift) == Keys.Shift)
            {
                k = (Keys)(k - Keys.Shift);
            }

            switch (k)
            {
                case Keys.W:
                case Keys.Up:
                    GoingForward = state;
                    break;

                case Keys.S:
                case Keys.Down:
                    GoingBack = state;
                    break;

                case Keys.A:
                case Keys.Left:
                    GoingLeft = state;
                    break;

                case Keys.D:
                case Keys.Right:
                    GoingRight = state;
                    break;

                case Keys.E:
                case Keys.PageUp:
                    GoingUp = state;
                    break;

                case Keys.Q:
                case Keys.PageDown:
                    GoingDown = state;
                    break;

                case Keys.Shift:
                case Keys.ShiftKey:
                    FastMove = state;
                    break;
            }

            return;
        }
    }
}