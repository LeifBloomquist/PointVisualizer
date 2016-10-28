using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PointVisualizer
{
    class FileHandler
    {
        public static string LastPointFile { get; private set; }
        private static String FileFilter = "CSV Point Data (*.csv)|*.csv";
        private static OgreForm owner = null;

        /// <summary>
        /// Load a file after prompting in a dialog box.
        /// </summary>
        /// <param name="defaultpath">Default path for loading from.</param>
        /// <returnsTrue if file was loaded, false on error or cancel</returns>
        public static bool LoadPointsFile(OgreForm _owner)
        {
            owner = _owner;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = Registry.GetLastPath();
            openFileDialog1.Filter = FileFilter;
            openFileDialog1.FilterIndex = Registry.GetLastFileFilter();
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LastPointFile = openFileDialog1.FileName;
                
                // Spawn a thread to do the parsing and calibration so GUI doesn't hang.
                Thread myThread = new Thread(() => ParsingThread());
                myThread.Start();

                Registry.WriteRegistry(Path.GetDirectoryName(LastPointFile), openFileDialog1.FilterIndex);

                return true;
            }
            return false;
        }

        private static void ParsingThread()
        {
            // 1. Load and the data, depending on format
            owner.SafeUpdateStatus("Loading...", true);
            List<Point3D> LoadedPoints = LoadPointsFromLastFile();

            if (LoadedPoints == null) return;

            // 2. Instruct GUI to add these points to its display
            owner.SafeUpdateStatus("Creating point cloud...", false);
            owner.PlotPointDataThread(LoadedPoints);
        }         
      
        private static void UpdateGUI(string message)
        {
            owner.SafeUpdateStatus(message, false);
        }

        private static List<Point3D> LoadPointsFromLastFile()
        {              
           return null;  
        }
    }
}
