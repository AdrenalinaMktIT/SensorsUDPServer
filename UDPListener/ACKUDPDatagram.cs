using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPListener
{
    /*
     * Inicio de trama, IMEI del equipo, ACK=numero de paquete, hora y fin de trama. La hora se puede no enviar, ya que no la tomo.
     * Los char "ACK=" van en posición 19, 20, 21 y 22 de la trama, arrancando por la posición 0.
     * Los números de paquete van en las posiciones 23, 24, 25 y 26. Siempre son 4 cifras, aunque sean 0001.
     *  
     * Ejemplo: "$B,292207061990000,ACK=1440,2015-08-17 18:12,$E"
     */

    public class ACKUDPDatagram
    {
        private UDPDatagram udpDatagram;

        public ACKUDPDatagram(UDPDatagram udpDatagram)
        {
            this.udpDatagram = udpDatagram;
        }

        public string MakeValidPacket()
        {
            return "$B," + udpDatagram.IMEI + ",ACK=" + udpDatagram.PacketNumber + "," + String.Format("{0:yyyy'-'MM'-'dd HH':'mm':'ss}", DateTime.Now) + ",$E";
        }

        public static string MakeInvalidPacket()
        {
            return "$B,000000000000000,ACK=9999," + String.Format("{0:yyyy'-'MM'-'dd HH':'mm':'ss}", DateTime.Now) + ",$E";
        }
    }
}
