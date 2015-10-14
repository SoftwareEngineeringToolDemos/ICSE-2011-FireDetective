using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace FireDetectiveAnalyzer
{
    public interface IEventProcessor
    {
        void HandleMessage(string message, Func<string> getNextMessage, Func<int, byte[]> getBinaryData);
    }

    public class TraceProviderConnection
    {
        private StringMessageStream m_Stream;
        private IEventProcessor m_Evp;

        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler StartRequested;
        public event EventHandler StopRequested;
        public event EventHandler IsBusy;
        public event EventHandler Closed;

        public TraceProviderConnection()
        {
        }

        public bool ConnectWithLocalFirefox(Firefox.FirefoxEventProcessor evp)
        {
            m_Evp = evp;

            // Actually, Unicode means UTF-16, whereas we need UCS-2. 
            // UTF-16 might insert multi-word characters, which we do not want.
            // However, since .NET strings are stored as UCS-2, we won't get multi-word characters anyway.
            return Connect(IPAddress.Loopback, 34971, Encoding.UTF8, new UnicodeEncoding(true, false), true);
        }

        /*public bool ConnectWithLocalPython(Python.PythonEventProcessor evp)
        {
            m_Evp = evp;

            return Connect(IPAddress.Loopback, 55483, Encoding.ASCII, Encoding.ASCII, false);
        }*/

        public bool ConnectWithLocalJava(Java.JavaEventProcessor evp)
        {
            m_Evp = evp;

            return Connect(IPAddress.Loopback, 38056, Encoding.ASCII, Encoding.ASCII, false);
        }
        
        private bool Connect(IPAddress ip, int port, Encoding receiveEncoding, Encoding sendEncoding, bool bigEndian)
        {
            TcpClient tcpClient;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(new IPEndPoint(ip, port));
            }
            catch (SocketException)
            {
                return false;
            }

            m_Stream = new StringMessageStream(tcpClient.GetStream(), receiveEncoding, sendEncoding, bigEndian);

            Thread t = new Thread(new ThreadStart(() =>
            {
                try
                {
                    while (true)
                    {
                        string msg = m_Stream.ReadString();
                        DispatchMessage(msg);
                    }
                }
                catch (EndOfStreamException)
                {
                }
                catch (SocketException)
                {
                }
                catch (IOException)
                {
                }

                //catch (Exception ex)
                //{
                //    MessageBox.Show("Network thread stopped: " + ex.Message);
                //}

                if (Closed != null)
                    Closed(this, EventArgs.Empty);
            }));
            t.IsBackground = true;
            t.Start();
            return true;
        }

        private void DispatchMessage(string message)
        {
            /*if (message == "START-OK")
            {
                if (Started != null)
                    Started(this, EventArgs.Empty);
            }
            else if (message == "STOP-OK")
            {
                if (Stopped != null)
                    Stopped(this, EventArgs.Empty);
            }
            else if (message == "REQUEST-START")
            {
                if (StartRequested != null)
                    StartRequested(this, EventArgs.Empty);
            }
            else if (message == "REQUEST-STOP")
            {
                if (StopRequested != null)
                    StopRequested(this, EventArgs.Empty);
            }
            else*/
            if (message == "BUSY")
            {
                if (IsBusy != null)
                    IsBusy(this, EventArgs.Empty);
            }
            else
            {
                m_Evp.HandleMessage(message, () => m_Stream.ReadString(), n => m_Stream.ReadBytes(n));  
            }
        }

        /*
        public void SendStart()
        {
            m_Stream.WriteString("START");
        }

        public void SendStop()
        {
            m_Stream.WriteString("STOP");
        }

        public void SendMayRequestStart(bool yes)
        {
            if (yes)
                m_Stream.WriteString("MAY-REQUEST-START");
            else
                m_Stream.WriteString("MAY-NOT-REQUEST-START");
        }

        public void SendHighlight(IEnumerable<int> nodeIds)
        {
            m_Stream.WriteString("HIGHLIGHT," + string.Join(",", nodeIds.Select(id => id.ToString()).ToArray()));
        }*/
    }
    
}
