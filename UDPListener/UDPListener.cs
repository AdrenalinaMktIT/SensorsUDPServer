using NLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;

namespace UDPListener
{
    public static class UDPListener
    {
        /*
         * NLog tutorial suggests to use a static Logger, so here it is
         */
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const int ListenPort = 11000;

        public static void StartListener()
        {
            // connect to a single MongoDB server using a connection string.
            var client = new MongoClient("mongodb://localhost:27017");

            var sensorsDb = client.GetDatabase("sensors");

            var measuresCollection = sensorsDb.GetCollection<BsonDocument>("measures");

            const bool done = false;

            var listener = new UdpClient(ListenPort);
            var groupEP = new IPEndPoint(IPAddress.Any, ListenPort);

            Console.Title = "ADRENALINA MKT&IT - UDP LISTENER";
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetWindowSize(Console.WindowWidth, Console.WindowHeight / 2);
            Console.WriteLine("190.245.153.142 - Listening on port 11000...\n");

            logger.Info("ADRENALINA MKT&IT - UDP LISTENER");
            logger.Info("190.245.153.142 - Listening on port 11000...");

            try
            {
                var ACKUDPDatagram = "";
                while (!done)
                {
                    var bytes = listener.Receive(ref groupEP);

                    var bits = string.Join(", ", bytes.Select(x => Convert.ToString(x, 2).PadLeft(8, '0'))).Insert(0, " {");
                    var bits2 = bits.Insert(bits.Length, "}");

                    Console.WriteLine("Received UDP Datagram from {0}\n" +
                                      "Payload(string): {1}" +
                                      //"Payload(ASCII): {2}\n" +
                                      //"Payload(bits): {3}\n" +
                                      "\tat {4}\n\n", groupEP.ToString(), Encoding.ASCII.GetString(bytes, 0, bytes.Length), PrintBytes(bytes), bits2, DateTime.Now);

                    logger.Info("Received UDP Datagram from {0}\n" +
                                      "Payload(string): {1}" +
                                      //"Payload(ASCII): {2}\n" +
                                      //"Payload(bits): {3}\n" +
                                      "\tat {4}\n\n", groupEP.ToString(), Encoding.ASCII.GetString(bytes, 0, bytes.Length), PrintBytes(bytes), bits2, DateTime.Now);

                    var udpDatagram = new UDPDatagram(Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                    var ackudpDatagram = new ACKUDPDatagram(udpDatagram);
                    ACKUDPDatagram = udpDatagram.Tokenize() ? ackudpDatagram.MakeValidPacket() : global::UDPListener.ACKUDPDatagram.MakeInvalidPacket();

                    // si es valido guardo en la base
                    if (udpDatagram.Valid)
                    {
                        var measuresDocument = new BsonDocument {
                            { "imei", udpDatagram.IMEI },
                            { "packetNumber", udpDatagram.PacketNumber },
                            { "triggerEvent", udpDatagram.TriggerEvent },
                            { "gsmSignalStrength", udpDatagram.GSMSignalStrength },
                            { "data", new BsonArray { udpDatagram.Data1, udpDatagram.Data2, udpDatagram.Data3, udpDatagram.Data4, udpDatagram.Data5, udpDatagram.Data6, udpDatagram.Data7, udpDatagram.Data8, udpDatagram.Data9, udpDatagram.Data10 } },
                            { "coord", new BsonArray { udpDatagram.Latitude, udpDatagram.Longitude } },
                            { "modelAndVersion", udpDatagram.DeviceModelAndVersion },
                            { "timestamp", udpDatagram.MeasureTimestamp }
                        };

                        measuresCollection.InsertOne(measuresDocument);
                    }


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
 