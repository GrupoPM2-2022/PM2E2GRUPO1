using System;
using System.Collections.Generic;
using System.Text;

namespace PM2E2GRUPO1.Models
{
    public class Sitio
    {
        public int id { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public String descripcion { get; set; }
        //public String pathImage { get; set; }
        public Byte[] image { get; set; }
        public Byte[] audioFile { get; set; }
    }
}
