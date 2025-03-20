using System;
using Google.Cloud.Firestore;
using System.Reflection;

namespace TaxiAAtics.Controls
{
    public class FirestoreService
    {
        private FirestoreDb _db;

        public FirestoreService()
        {
            // Obtén el archivo taatcs.json desde los recursos incrustados
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TaxiAAtics.Resources.datacd.json"; // direccion del NameSpace para acceso al archivo de la app

            // Cargar el archivo como flujo de datos
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("El archivo de credenciales no se encontró como recurso incrustado.");

                // Crear una ruta temporal para almacenarlo en la memoria
                var tempFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "datacd.json");

                // Escribir el flujo del archivo al disco (solo si necesitas la ruta local)
                using (var fileStream = File.Create(tempFilePath))
                {
                    stream.CopyTo(fileStream);
                }

                // Establecer la variable de entorno con la ruta local
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempFilePath);
            }

            // Inicializa la conexión a Firestore
            _db = FirestoreDb.Create("cedyc-taxi-campeche");
        }

        public async Task GetData()
        {
            var collection = _db.Collection("SystemB");
            var snapshot = await collection.GetSnapshotAsync();

            foreach (var document in snapshot.Documents)
            {
                Console.WriteLine($"Document data: {document.ToDictionary()}");
            }
        }

        public async void Set(string Data, string ID)
        {
            var collection = _db.Collection("SystemB");
            await collection.Document(ID).SetAsync(new { DataTest = Data });
        }

        public CollectionReference GetCollection(string collectionName)
        {
            return _db.Collection(collectionName);
        }
    }

}

