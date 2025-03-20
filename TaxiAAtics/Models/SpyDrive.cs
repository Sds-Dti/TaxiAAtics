using System;
using Google.Cloud.Firestore;

namespace TaxiAAtics.Models
{
    [FirestoreData]

    internal class SpyDrive
    {
        [FirestoreProperty]

        public GeoPoint DriveTimer { get; set; }
        [FirestoreProperty]

        public double Rotacion { get; set; }
        [FirestoreProperty]
        public string Conductor { get; set; }

    }
}

