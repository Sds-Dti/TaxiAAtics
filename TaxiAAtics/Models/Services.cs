using System;
using Google.Cloud.Firestore;

namespace TaxiAAtics.Models
{
    [FirestoreData]

    public class Services
    {
        [FirestoreProperty]
        public string Asignado { get; set; }
        [FirestoreProperty]
        public string Estado { get; set; }
        [FirestoreProperty]
        public Timestamp? Creado { get; set; }
        [FirestoreProperty]
        public string DireccionDest { get; set; }
        [FirestoreProperty]
        public string DireccionUsr { get; set; }
        [FirestoreProperty]
        public List<string> DistanciaPre { get; set; }
        [FirestoreProperty]
        public GeoPoint GeoDestino { get; set; }
        [FirestoreProperty]
        public GeoPoint GeoUsr { get; set; }
        [FirestoreProperty]
        public string Motivo_cancelacion { get; set; }
        [FirestoreProperty]
        public string NoID { get; set; }
        [FirestoreProperty]
        public string Nombre { get; set; }
        [FirestoreProperty]
        public string idService { get; set; }
        [FirestoreProperty]
        public string uuid { get; set; }
        [FirestoreProperty]
        public string Conductor { get; set; }
        [FirestoreProperty]
        public string Referencia { get; set; }
        public List<string> Excluidos { get; set; }
        [FirestoreProperty]
        public int RegionId { get; set; }
        [FirestoreProperty]
        public string TotalPagado { get; set; }
    }
}

