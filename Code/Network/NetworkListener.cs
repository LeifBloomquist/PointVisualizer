using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PointVisualizer
{
    static class Utilities
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }

    class NetworkListener
    {
        // Save reference to owner for sending updates, etc.
        OgreForm owner = null; 

        // The server
        TcpListener server=null;   

        // Flag to stay connected
        private bool connected=true;

        public NetworkListener(OgreForm owner)
        {
            this.owner = owner;

            IPAddress myAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (myAddress != null)
            {
                server = new TcpListener(myAddress, 5000);
            }
            else            
            {
                server = new TcpListener(IPAddress.Any, 5000);
            }            
        }

        public void StartListening()
        {
            Thread myThread = new Thread(() => ListenerThread());  // Get the network connection off the GUI Thread            
            myThread.Start();
        }

        private void ListenerThread()
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Network Thread";

            connected = true;

            try
            {
                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[100000];

                // Enter the listening loop. 
                while (connected)
                {
                    String s = "Waiting for a connection on " +
                           IPAddress.Parse(((IPEndPoint)server.LocalEndpoint).Address.ToString()) + ":" +
                           ((IPEndPoint)server.LocalEndpoint).Port.ToString();

                    owner.SafeUpdateStatus(s, false);
                 

                    // Perform a blocking call to accept requests. 
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();

                    string clientIPAddress = "Connection from " + IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                    owner.SafeUpdateStatus(clientIPAddress, false);

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client. 

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        owner.SafeUpdateStatus("Received " + i + " bytes!", false);
                        owner.PlotIncomingData(bytes.SubArray(0, bytes.Length));
                    }

                    // Shutdown and end connection
                    owner.SafeUpdateStatus("Client disconnected.", false);
                    client.Close();                   
                  
                } // while
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.Interrupted)   // This is normal when closing the connection manually, so ignore it.  Flag all others.
                {
                    MessageBox.Show("Listener SocketException: " + e.Message);
                }
            }
            catch (IOException)  // This is normal when client closes connection "forcibly"
            {
                ;
            }
            finally
            {
                // Stop listening for new clients.
                if (server != null)
                {
                    server.Stop();
                }

                owner.Invoke(new Action(() => owner.Disconnect()));   // Restores state of GUI buttons                
            }
        }


        public void StopListening()
        {
            try
            {
                server.Stop();
            }
            catch (Exception e)
            {
                MessageBox.Show("Stopping server, Exception: " + e.Message);
            }

            owner.SafeUpdateStatus("Listening stopped.", false);
            connected = false;
        }
    }
}
