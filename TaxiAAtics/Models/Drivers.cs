using System;
using Google.Cloud.Firestore;

namespace TaxiAAtics.Models
{
    [FirestoreData]
    public class Drivers
    {
        [FirestoreProperty]
        public bool Activo { get; set; }
        [FirestoreProperty]

        public string Asociacion { get; set; }
        [FirestoreProperty]

        public string Clave { get; set; }
        [FirestoreProperty]

        public string IdIET { get; set; }
        [FirestoreProperty]

        public string Nombre { get; set; }
        [FirestoreProperty]

        public string Placas { get; set; }
        [FirestoreProperty]

        public bool PrimerSesion { get; set; }
        [FirestoreProperty]

        public bool SociosConductores { get; set; }
        [FirestoreProperty]

        public string UrlFoto { get; set; }
        [FirestoreProperty]

        public string NumeroEconomico { get; set; }
        [FirestoreProperty]

        public string Modelo { get; set; }
        [FirestoreProperty]

        public string Marca { get; set; }
        [FirestoreProperty]

        public string Correo { get; set; }
        [FirestoreProperty]

        public string UIDD { get; set; }
        [FirestoreProperty]
        public bool EnEspera { get; set; }
        [FirestoreProperty]
        public bool EnServicio { get; set; }
        [FirestoreProperty]
        public string NotifyID { get; set; }
        [FirestoreProperty]
        public bool Restringido { get; set; }
        [FirestoreProperty]
        public bool SesionActiva { get; set; }
        [FirestoreProperty]
        public int NumRestric { get; set; }
        [FirestoreProperty]
        public string HID { get; set; }
        [FirestoreProperty]

        public string Localidad { get; set; }
    }
}

