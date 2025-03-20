using System;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace TaxiAAtics.Controls
{
    public class NotifySystem
    {
        private string jsonData;
        private GoogleCredential credential;

        public NotifySystem()
        {
            // Cargar el archivo de credenciales una sola vez en el constructor
            LoadCredentials();
        }

        private void LoadCredentials()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "TaxiAAtics.Resources.datacd.json"; // Dirección del recurso incrustado

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new FileNotFoundException("El archivo de credenciales no se encontró como recurso incrustado.");

                    using (var reader = new StreamReader(stream))
                    {
                        // Leer el archivo de credenciales una sola vez
                        jsonData = reader.ReadToEnd();
                    }

                    // Crear credenciales de Google una sola vez
                    credential = GoogleCredential.FromJson(jsonData).CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar las credenciales: {ex.Message}");
            }
        }

        public async Task SendNotificationByTokenAsync(string Token, string Titulo, string Mensaje)
        {
            try
            {
                var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var message = new
                    {
                        message = new
                        {
                            token = Token,
                            notification = new
                            {
                                body = Mensaje,
                                title = Titulo
                            }
                        }
                    };

                    string jsonBody = JsonConvert.SerializeObject(message);
                    StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

                    string pID = "cedyc-taxi-campeche";
                    string url = $"https://fcm.googleapis.com/v1/projects/{pID}/messages:send";
                    HttpResponseMessage responseMessage = await client.PostAsync(url, content);

                    var isSucc = responseMessage.IsSuccessStatusCode;
                    string rC = await responseMessage.Content.ReadAsStringAsync();

                    Console.WriteLine($"Éxito: {isSucc}, respuesta: {rC}");
                    if (isSucc)
                    {
                        await Application.Current.MainPage.DisplayAlert("Sistema", "La notificacion ha sido enviada con exito", "Aceptar");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Sistema", "Ocurrio un problema al enviar la notificacion", "Aceptar");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar la notificación: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Sistema", $"La notificacion fue procesada con respues {ex.Message}", "Aceptar");

            }
        }
    }
    //public class NotifySystem
    //{
    //       private StreamReader reader;
    //       private string jsonData;

    //       [UnconditionalSuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file", Justification = "<Pending>")]
    //       public async Task SendNotificationByTokenAsync(string Token, string Titulo, string Mensaje)
    //       {
    //           try
    //           {
    //               // Obtén el archivo taatcs.json desde los recursos incrustados
    //               var assembly = Assembly.GetExecutingAssembly();
    //               var resourceName = "TaxiAAtics.Resources.datacd.json"; // direccion del NameSpace para acceso al archivo de la app

    //               // Cargar el archivo como flujo de datos
    //               using (var stream = assembly.GetManifestResourceStream(resourceName))
    //               {
    //                   if (stream == null)
    //                       throw new FileNotFoundException("El archivo de credenciales no se encontró como recurso incrustado.");

    //                   // Crear una ruta temporal para almacenarlo en la memoria
    //                   var tempFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "datacd.json");

    //                   // Escribir el flujo del archivo al disco (solo si necesitas la ruta local)
    //                   using (var fileStream = File.Create(tempFilePath))
    //                   {
    //                       stream.CopyTo(fileStream);
    //                   }

    //                   // Establecer la variable de entorno con la ruta local
    //                   //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempFilePath);
    //                   reader = new StreamReader(tempFilePath);
    //                   jsonData = reader.ReadToEnd();


    //               }

    //               var credential = GoogleCredential.FromJson(jsonData).CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

    //               var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

    //               using (HttpClient client = new())
    //               {
    //                   client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    //                   var message = new
    //                   {
    //                       message = new
    //                       {
    //                           token = Token,
    //                           notification = new
    //                           {
    //                               body = Mensaje,
    //                               title = Titulo
    //                           }
    //                       }
    //                   };
    //                   string jsonBody = JsonConvert.SerializeObject(message);
    //                   StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

    //                   string pID = "cedyc-taxi-campeche";
    //                   string url = $"https://fcm.googleapis.com/v1/projects/{pID}/messages:send";
    //                   HttpResponseMessage responseMessage = await client.PostAsync(url, content);
    //                   var isSucc = responseMessage.IsSuccessStatusCode;
    //                   string rC = await responseMessage.Content.ReadAsStringAsync();

    //                   Console.WriteLine($"Exito: {isSucc}, respuesta: {rC}");
    //               }
    //           }
    //           catch (Exception ex)
    //           {
    //               Console.WriteLine($"Error: {ex.Message}");
    //           }


    //       }
    //   }
}

