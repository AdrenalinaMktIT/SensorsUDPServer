using System;
using System.Linq;

namespace UDPListener
{
    /*
         * Ejemplo: "$B,292207061990000,1440,0,0,110.2,89.4,-99,0,0,0,0,0,0,0,0,0,MD_ESP85@V1.3,2015-08-17 18:12,$E"
         * 1) Inicio de trama ($B)
         * 2) IMEI (15 dígitos)
         * 3) Número de paquete (del 1 al 1440 máximo, para tener 1 por minuto del día).
         * 4) Evento que generó el paquete. (0 para envío de datos, 10 para arranque del equipo (cuando se corta la luz, por ej))
         * 5) Nivel de señal GSM (0 siempre con estos equipos sin GSM)
         * 6 al 15) Datos (siempre 10, separados por comas, si el equipo no tiene un sensor asignado a esa posición se envía -99)
         * 16) Latitud (0 siempre con estos equipos sin GPS).
         * 17) Longitud (0 siempre con estos equipos sin GPS).
         * 18) Modelo y versión del equipo
         * 19) Fecha (Formato MySQL yyyy-MM-DD HH:mm:ss, solo que no mando los segundos)
         * 20) Fin de trama ($E)
         */
    public class UDPDatagram
    {
        public UDPDatagram(string datagram)
        {
            Datagram = datagram;
        }

        private string Datagram { get; set; }

        private string[] TokenizedDatagram { get; set; }

        public string IMEI
        {
            get { return TokenizedDatagram[1]; }
        }

        public string PacketNumber
        {
            get { return TokenizedDatagram[2]; }
        }

        public string TriggerEvent
        {
            get { return TokenizedDatagram[3]; }
        }

        public int GSMSignalStrength
        {
            get { return Convert.ToInt32(TokenizedDatagram[4]); }
        }

        public string Data1
        {
            get { return TokenizedDatagram[5]; }
        }
        public string Data2
        {
            get { return TokenizedDatagram[6]; }
        }
        public string Data3
        {
            get { return TokenizedDatagram[7]; }
        }
        public string Data4
        {
            get { return TokenizedDatagram[8]; }
        }
        public string Data5
        {
            get { return TokenizedDatagram[9]; }
        }
        public string Data6
        {
            get { return TokenizedDatagram[10]; }
        }
        public string Data7
        {
            get { return TokenizedDatagram[11]; }
        }
        public string Data8
        {
            get { return TokenizedDatagram[12]; }
        }
        public string Data9
        {
            get { return TokenizedDatagram[13]; }
        }
        public string Data10
        {
            get { return TokenizedDatagram[14]; }
        }

        public double Latitude
        {
            get { return Convert.ToDouble(TokenizedDatagram[15]); }
        }

        public double Longitude
        {
            get { return Convert.ToDouble(TokenizedDatagram[16]); }
        }

        public string DeviceModelAndVersion
        {
            get { return TokenizedDatagram[17]; }
        }

        public string MeasureTimestamp
        {
            get { return TokenizedDatagram[18]; }
        }

        private bool Validate()
        {
            

            // La trama debe poseer exactamente 20 componentes una vez tokenizada; caso contrario esta mal formada.
            if (TokenizedDatagram.Length != 20)
                return false;

            // La trama debe comenzar y terminar exactamente con marcas puntuales. ($B y $E, respectivamente).
            if (!TokenizedDatagram[0].Equals("$B"))
            {
                return false;
            }
            
            if (!TokenizedDatagram[19].Equals("$E"))
            {
                return false;
            }

            // Verifico los tipos de datos de la trama.
            if (!IMEI.All(char.IsDigit) || IMEI.Count() != 15)
            {
                return false;
            }

            if (!PacketNumber.All(char.IsDigit) || PacketNumber.Count() != 4 || Convert.ToInt16(PacketNumber) < 1 || Convert.ToInt16(PacketNumber) > 1440)
            {
                return false;
            }

            return true;

        }

        public bool Tokenize()
        {
            char[] separators = { ',' };
            TokenizedDatagram = Datagram.Split(separators);   
            return Validate();
            
        }
    }
}