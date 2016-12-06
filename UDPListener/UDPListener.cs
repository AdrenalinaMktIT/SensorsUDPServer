using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPListener
{
    public static class UDPListener
    {
        private const int ListenPort = 11000;

        public static void StartListener()
        {
            const bool done = false;

            var listener = new UdpClient(ListenPort);
            var groupEP = new IPEndPoint(IPAddress.Any, ListenPort);

            Console.Title = "ADRENALINA MKT&IT - UDP LISTENER";
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetWindowSize(Console.WindowWidth, Console.WindowHeight / 2);
            Console.WriteLine("190.245.179.173 - Listening on port 11000...\n");

            try
            {
                var ACKUDPDatagram = "";
                while (!done)
                {
                    var bytes = listener.Receive(ref groupEP);

                    var bits = string.Join(", ", bytes.Select(x => Convert.ToString(x, 2).PadLeft(8, '0'))).Insert(0, " {");
                    var bits2 = bits.Insert(bits.Length, "}");

                    Console.WriteLine("Received UDP Datagram from {0}\n" +
                                      "Payload(string): {1}\n" +
                                      //"Payload(ASCII): {2}\n" +
                                      //"Payload(bits): {3}\n" +
                                      "\tat {4}\n\n", groupEP.ToString(), Encoding.ASCII.GetString(bytes, 0, bytes.Length), PrintBytes(bytes), bits2, DateTime.Now);

                    var udpDatagram = new UDPDatagram(Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                    var ackudpDatagram = new ACKUDPDatagram(udpDatagram);
                    ACKUDPDatagram = udpDatagram.Tokenize() ? ackudpDatagram.MakeValidPacket() : global::UDPListener.ACKUDPDatagram.MakeInvalidPacket();
                        

                    var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    var broadcast = IPAddress.Parse(groupEP.Address.ToString());

                    var sendbuf = Encoding.ASCII.GetBytes(ACKUDPDatagram);
                    var ep = new IPEndPoint(broadcast, groupEP.Port);

                    s.SendTo(sendbuf, ep);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }
        }

        private static string PrintBytes(this byte[] byteArray)
        {
            var sb = new StringBuilder("{ ");
            for (var i = 0; i < byteArray.Length; i++)
            {
                var b = byteArray[i];
                sb.Append(b);
                if (i < byteArray.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" }");
            return sb.ToString();
        }
    }
}