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
    public partial class OgreForm : Form
    {
        // Flag to use when exiting
        private bool running = true;

        // Useful Constants
        public static string TITLE = "3D Point Cloud Visualizer";
        private const float PI = (float)System.Math.PI;

        // Ogre stuff
        Root mRoot;
        RenderWindow mWindow;
        SceneManager mgr = null;
        ManualObject manual = null;
        Camera cam = null;
        CameraMan manager = null;

        // Animation Timer
        private System.Timers.Timer myTimer = new System.Timers.Timer();
        double rxpulse = 0d;   

        // Network Stuff
        NetworkListener listener = null;

        public OgreForm()
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "GUI Thread";

            InitializeComponent();

            Disposed += new EventHandler(OgreForm_Disposed);
            Resize += new EventHandler(OgreForm_Resize);

            Init3DMouse();

            this.MouseWheel += new MouseEventHandler(OgreForm_MouseWheel);
        }

        public void Init()
        {
            // Create root object
            mRoot = new Root();

            // Define Resources
            ConfigFile cf = new ConfigFile();
            cf.Load("resources.cfg", "\t:=", true);
            ConfigFile.SectionIterator seci = cf.GetSectionIterator();
            String secName, typeName, archName;

            while (seci.MoveNext())
            {
                secName = seci.CurrentKey;
                ConfigFile.SettingsMultiMap settings = seci.Current;
                foreach (KeyValuePair<string, string> pair in settings)
                {
                    typeName = pair.Key;
                    archName = pair.Value;
                    ResourceGroupManager.Singleton.AddResourceLocation(archName, typeName, secName);
                }
            }

            // Setup RenderSystem
            RenderSystem rs = mRoot.GetRenderSystemByName("Direct3D9 Rendering Subsystem");
            //RenderSystem rs = mRoot.GetRenderSystemByName("OpenGL Rendering Subsystem");

            mRoot.RenderSystem = rs;
            rs.SetConfigOption("Full Screen", "No");
            rs.SetConfigOption("Video Mode", "800 x 600 @ 32-bit colour");

            // Create Render Window  (inside PictureBox)
            mRoot.Initialise(false, "Main Ogre Window");
            NameValuePairList misc = new NameValuePairList();
            misc["externalWindowHandle"] = OgreBox.Handle.ToString();   //  Handle.ToString();
            misc["FSAA"] = "2";        // anti aliasing factor (0, 2, 4 ...)
            misc["vsync"] = "true";    // by Ogre default: false

            mWindow = mRoot.CreateRenderWindow("Main RenderWindow", 800, 600, false, misc);

            // Init resources
            TextureManager.Singleton.DefaultNumMipmaps = 5;
            ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

            // Create a Simple Scene            
            mgr = mRoot.CreateSceneManager(SceneType.ST_GENERIC);
            cam = mgr.CreateCamera("Camera");
            cam.AutoAspectRatio = true;
            cam.FarClipDistance = float.MaxValue;
            cam.NearClipDistance = 0.01f;

            mWindow.AddViewport(cam);
            CreateScene();

            try
            {
                // Start the Camera Manager
                manager = new CameraMan(cam);

                // Start the network
                listener = new NetworkListener(this);
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "Exception:\n\n" + e.Message + "\n\n" + e.GetBaseException().ToString());
            }
        }

        public void Go()
        {
            Show();

            // Sets the timer interval to 50 milliseconds.
            myTimer.Elapsed += TimerEventProcessor;
            myTimer.Interval = 50;
            myTimer.Start();

            while (running)
            {
                if (mRoot != null)
                {
                    mRoot.RenderOneFrame();
                }

                manager.Process();

                try
                {
                    RenderTarget.FrameStats stats = mWindow.GetStatistics();
                    FPSLabel.Text = "FPS: " + stats.LastFPS.ToString("0.00");
                    String pos = " X:" + cam.Position.x.ToString("0.00") +
                                 " Y:" + cam.Position.y.ToString("0.00") +
                                 " Z:" + cam.Position.z.ToString("0.00");
                    CamLabel.Text = "Camera:" + pos;
                }
                catch (Exception e)
                {
                    StatusLabel.Text = "Exception: " + e.Message;
                }

                Application.DoEvents();
            }
        }

        void OgreForm_Disposed(object sender, EventArgs e)
        {
            mRoot.Dispose();
            mRoot = null;
        }

        void OgreForm_Resize(object sender, EventArgs e)
        {
            mWindow.WindowMovedOrResized();
        }

        private void OgreForm_MouseWheel(object sender, MouseEventArgs e)
        {
            int delta = e.Delta;

            var move = Vector3.ZERO;            
            move += cam.Direction * delta;

            if (manager.FastMove)
            {
                move *= 10;  // Super fast
            }     

            if (move != Vector3.ZERO)
            {
                cam.Move(move);
            }
        }

        private void CreateScene()
        {
            if (mgr == null) return;
            mgr.ClearScene();

            mgr.AmbientLight = new ColourValue(1, 1, 1);

            manual = mgr.CreateManualObject("PointCloud");
            mgr.RootSceneNode.CreateChildSceneNode("PointCloudNode").AttachObject(manual);
            mgr.DisplaySceneNodes = false;

            manual.Dynamic = true;
            //manual.EstimateIndexCount((uint)MAXPOINTS);      Recommended in MOGRE docs, but doesn't seem to have any effect
            //manual.EstimateVertexCount((uint)MAXPOINTS); 
            HomePosition();
        }

        // This is the method to run when the timer is raised.
        // Animates the background of the RX LEDs.
        private void TimerEventProcessor(Object myObject, ElapsedEventArgs myEventArgs)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Timer Thread";

            int red = (int)(rxpulse * 255);
            rxpulse *= 0.4f;

            try
            {
                this.Invoke(new Action(() =>
                {
                    RXLabel.BackColor = Color.FromArgb(red, 0, 0);
                }));
            }
            catch (Exception)
            {
                ;
            }
        }

        private void LoadPointsButton_Click(object sender, EventArgs e)
        {
            // 1. Select and Load the File.  If successful, this does so on a background thread.
            bool success = FileHandler.LoadPointsFile(this);

            if (success == false) // Error or Cancelled
            {
                return;
            }

            // 2. set title          
            this.Text = OgreForm.TITLE + " - " + FileHandler.LastPointFile;

            // 3. Reset view to home position
            HomePosition();
        }

        // Called from Network thread
        public void PlotIncomingData(byte[] PointsData)
        {
            this.Invoke(new Action(() =>
            {
                rxpulse = 1.0;
                List<Point3D> Points = null;   // TODO, Parse binary point data depending on format
                PlotPointDataThread(Points);  
            }));
        }

        private void UpdateStatus(string message)
        {
            SafeUpdateStatus(message, false);
        }

        public void SafeUpdateStatus(String message, bool refresh)
        {
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                this.Invoke(new Action(() => 
                { 
                    StatusLabel.Text = message;
                    if (refresh) Refresh();
                }));
            }
            catch (Exception)  //  Despite the above check, this still occasionally happens on shutdown.  Weird!
            {
                ;   // Ignore silently.
            }
        }

        private void OgreBox_Click(object sender, EventArgs e)
        {
        }

        private bool dragging = false;
        private Point previousLocation;

        private void OgreBox_MouseDown(object sender, MouseEventArgs e)
        {
            OgreBox.Cursor = Cursors.SizeAll;
            dragging = true;
        }

        private void OgreBox_MouseUp(object sender, MouseEventArgs e)
        {
            OgreBox.Cursor = Cursors.Hand;
            dragging = false;
        }      

        private void OgreBox_MouseMove(object sender, MouseEventArgs e)
        {
            Point difference = e.Location - (Size)previousLocation;

            if (dragging)
            {
                manager.MouseMovement(-(float)difference.X, -(float)difference.Y);                
            }  

            previousLocation = e.Location;          
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys key)
        {
            manager.ProcessKeyDown(key);
            return base.ProcessCmdKey(ref msg, key);            
        }

        private void OgreForm_KeyDown(object sender, KeyEventArgs e)
        {
            Keys key = (Keys)e.KeyCode;
            manager.ProcessKeyDown(key);
        }

        private void OgreForm_KeyUp(object sender, KeyEventArgs e)
        {
            Keys key = (Keys)e.KeyCode;
            manager.ProcessKeyUp(key);
        }

        private void ViewAllButton_Click(object sender, EventArgs e)
        {
            if (cam == null) return;

            try
            {
                cam.LookAt(manual.BoundingBox.Center);
            }
            catch (Exception)
            {
                ;   // Masks the error when scene is empty, no bounding box
            }
        }

        private void HomePosition()
        {
            cam.Position = new Vector3(0, 0, 1000);
            cam.LookAt(new Vector3(0, 0, 0));
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            HomePosition();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure?", "Clear Point Cloud", MessageBoxButtons.YesNo);

            if (result == DialogResult.No)
            {
                return;
            }

            manual.DetachFromParent();
            manual.Dispose();
            CreateScene();

            this.Text = OgreForm.TITLE;
        }

        private void OgreForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            myTimer.Stop();
            running = false;
            Application.Exit();
        }

        private void BackgroundButton_Click(object sender, EventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();       
            MyDialog.AllowFullOpen = true;
            // Allows the user to get help. (The default is false.)
            MyDialog.ShowHelp = false;
            // Sets the initial color select to the current text color.
            MyDialog.Color = Color.Black;

            // Update the text box color if the user clicks OK  
            if (MyDialog.ShowDialog() == DialogResult.OK)
            {
                mWindow.GetViewport(0).BackgroundColour = new ColourValue(MyDialog.Color.R / 255f,
                                                                          MyDialog.Color.G / 255f,
                                                                          MyDialog.Color.B / 255f,
                                                                          MyDialog.Color.A / 255f);
                        
            }
        }

        // Callback point for background threads (file load, network) to display loaded data.
        public void PlotPointDataThread(List<Point3D> PointsList)
        {
            UpdateCloudPoints(PointsList, Threshold.Value);
            SafeUpdateStatus("Plotted " + PointsList.Count + " points.", false);
        }

        private void UpdateCloudPoints(List<Point3D> PointsList, Decimal d_threshold_meters)
        {
            if (manual == null) return;
            if (PointsList == null) return;

            double threshold = (double)d_threshold_meters * 1000d; // convert to mm

            try
            {
                manual.Begin("BaseWhiteNoLighting", RenderOperation.OperationTypes.OT_POINT_LIST);

                // Real data
                foreach (Point3D sp in PointsList)
                {
                    ColourValue cv = sp.color;
                    Vector3 v = new Vector3((float)sp.x, (float)sp.y, (float)sp.z);

                    manual.Position(v);
                    manual.Colour(cv);
                }
                manual.End();
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception when plotting points: " + e.Message);
                // ignore throw e;
            }
        }

        private void HideButton_Click(object sender, EventArgs e)
        {
            ControlsPanel.Visible = false;
            HideButton.Visible = false;
            ShowButton.Visible = true;
        }
        
        private void ShowButton_Click(object sender, EventArgs e)
        {
            ControlsPanel.Visible = true;
            HideButton.Visible = true;
            ShowButton.Visible = false;
        }

        private void NetworkListenButton_Click(object sender, EventArgs e)
        {
            listener.StartListening();
            NetworkListenButton.Visible = false;
            NetworkDisconnectButton.Visible = true;
            LoadPointsButton.Enabled = false;
        }

        public void Disconnect()
        {
            listener.StopListening();
            NetworkListenButton.Visible = true;
            NetworkDisconnectButton.Visible = false;
            LoadPointsButton.Enabled = true;
        }

        private void NetworkDisconnectButton_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void MandelbrotTestButton_Click(object sender, EventArgs e)
        {
            PlotMandelbrot();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PlotSpiral();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PlotRandom();
            // PlotLines();
        }
    }
}
