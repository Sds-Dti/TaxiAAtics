using TaxiAAtics.Controls;
using System.Collections.ObjectModel;
using TaxiAAtics.Models;
using System.Diagnostics;
using Microsoft.Maui.Maps;
using System.Net.NetworkInformation;

namespace TaxiAAtics;

public partial class MainPage : ContentPage
{
    private readonly Color colorSeleccionado = Color.FromArgb("#B2264E"); // Color de fondo activo
    private readonly Color colorNormal = Color.FromArgb("#fbfafb"); // Color de fondo inactivo
    private readonly Color textColorSeleccionado = Color.FromArgb("#FFFFFF"); // Texto blanco en seleccionado
    private readonly Color textColorNormal = Color.FromArgb("#000000"); // Texto negro en no seleccionado
    //
    private readonly Color colorSeleccionadoM = Color.FromArgb("#B2264E"); // Rojo oscuro (seleccionado)
    private readonly Color colorNormalM = Color.FromArgb("#222323"); // Gris oscuro (no seleccionado)
    private readonly Color textColorSeleccionadoM = Color.FromArgb("#FFFFFF"); // Blanco (texto seleccionado)
    private readonly Color textColorNormalM = Color.FromArgb("#FFFFFF"); // Blanco (texto no seleccionado)
    //
    NotifySystem Notify = new();
    // Colección para mantener las entradas de registro
    public ObservableCollection<string> LogEntries { get; set; } = new ObservableCollection<string>();
    public int TiempoAsignado { get; private set; }
    public bool PrimerAsignadoR = false;
    public bool ReasignadoR = false;

    private Dictionary<string, DateTime> _documentTimestamps = new Dictionary<string, DateTime>();
    private ObservableCollection<Services> GetServices = new ObservableCollection<Services>();
    private ObservableCollection<Drivers> GetDrivers = new ObservableCollection<Drivers>();
    FirestoreService fDb = new FirestoreService();
    private Services objC;

    public MainPage()
    {
        InitializeComponent();
        LoadSv();
        DriversList();
        MapLocked();
        UpdateSwitchColors(); // Inicializa los colores al cargar la página
        AvisoT();
        //PrimerAsignado();
        //Reasignado();
        //StartLogging();
    }

    private void AppendLog(string message)
    {
        // Agrega el mensaje a la lista de registros
        LogEntries.Add(message);

        // Auto-scroll al final
        LogView.ScrollTo(LogEntries.Count - 1, position: ScrollToPosition.End);
    }

    private async void AvisoT()
    {
        while (true)
        {
            // Mostrar el control (100% opaco)
            Aviso.Opacity = 1;
            await Task.Delay(500); // Esperar 500ms

            // Ocultar el control (0% opaco)
            Aviso.Opacity = 0;
            await Task.Delay(500); // Esperar 500ms
        }
    }
    private void MapLocked()
    {
        // Definir la región de Campeche (aproximado)
        var centroCampeche = new Location(19.8301, -90.5349); // Ubicación central de Campeche
        var distancia = Distance.FromKilometers(10); // Radio de 200 km para limitar la vista

        // Mover el mapa a la región limitada
        MapaSv.MoveToRegion(MapSpan.FromCenterAndRadius(centroCampeche, distancia));
        AppendLog("Cargando mapa...");
        AppendLog($"Ubicacion designada {centroCampeche.Latitude}, {centroCampeche.Longitude} con distancia de vista {distancia.Meters}");
        //MapaSv.VisibleRegion.

        // Monitorear el cambio en la región visible
        // ADVERTENCIA NO USAR CAUSA ERROR EN LA EJECUCION
        //Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        //{
        //    VerificarRegionVisible();
        //    return true; // Continuar ejecutando cada medio segundo
        //});
    }

    private void VerificarRegionVisible()
    {
        var visibleRegion = MapaSv.VisibleRegion;

        // Verificar si el centro de la región visible está dentro del área de Campeche
        if (!EstaDentroDeCampeche(visibleRegion.Center))
        {
            // Si está fuera, regresar a la región permitida
            MapaSv.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(19.8301, -90.5349), Distance.FromKilometers(200)));
        }
    }

    private bool EstaDentroDeCampeche(Location ubicacion)
    {
        // Calcular la distancia entre la ubicación visible y el centro de Campeche utilizando la fórmula de Haversine
        var centroCampeche = new Location(19.8301, -90.5349);

        var distancia = CalcularDistanciaHaversine(centroCampeche.Latitude, centroCampeche.Longitude, ubicacion.Latitude, ubicacion.Longitude);

        // Verificar si la distancia es menor o igual al radio de 200 km (200,000 metros)
        return distancia <= 200000;
    }

    // Fórmula de Haversine para calcular la distancia entre dos coordenadas geográficas
    private double CalcularDistanciaHaversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double radioTierra = 6371000; // Radio de la Tierra en metros

        var lat1Rad = ToRadians(lat1);
        var lon1Rad = ToRadians(lon1);
        var lat2Rad = ToRadians(lat2);
        var lon2Rad = ToRadians(lon2);

        var dLat = lat2Rad - lat1Rad;
        var dLon = lon2Rad - lon1Rad;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return radioTierra * c; // Distancia en metros
    }

    // Función para convertir grados a radianes
    private double ToRadians(double grados)
    {
        return grados * (Math.PI / 180);
    }

    private async void OnFrameTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame selectedFrame)
        {
            // Lista de los Frames y Labels a modificar
            var items = new (Frame frame, Label label)[]
            {
                    (BnDispo, LblDispo),
                    (BnEnEspe, LblEnEspe),
                    (BnEnTransi, LblEnTransi)
            };

            // Resetear todo los Frames y Labels
            foreach (var item in items)
            {
                item.frame.BackgroundColor = colorNormal;
                item.label.TextColor = textColorNormal;
            }

            // Cambiar el Frame y el texto seleccionado
            var seleccionado = items.FirstOrDefault(x => x.frame == selectedFrame);
            if (seleccionado.frame != null)
            {
                seleccionado.frame.BackgroundColor = colorSeleccionado;
                seleccionado.label.TextColor = textColorSeleccionado;
            }

            LoadListProp(seleccionado.label.Text);
            //await DisplayAlert("Alerta", seleccionado.label.Text, "Ok");
        }
    }

    private async void LoadListProp(string Tipo)
    {
        await Device.InvokeOnMainThreadAsync(() =>
        {
            if (Tipo == "Disponibles")
            {
                ServiciosLs.ItemsSource = GetServices.Where(x => x.Estado.StartsWith("disponible")).Where(x => !string.IsNullOrEmpty(x.idService))
                .Where(x => string.IsNullOrEmpty(x.Asignado));
                AppendLog("Sevicios disponibles cargados");
                //asignados
            }
            else if (Tipo == "En espera")
            {
                //    asignados
                ServiciosLs.ItemsSource = GetServices.Where(x => x.Estado.StartsWith("disponible")).Where(x => !string.IsNullOrEmpty(x.idService))
                .Where(x => !string.IsNullOrEmpty(x.Asignado));
                AppendLog("Servicios en espera cargados");
            }
            else if (Tipo == "En transito")
            {
                ServiciosLs.ItemsSource = GetServices.Where(x => x.Estado.StartsWith("enProceso") || x.Estado.StartsWith("aceptado")).Where(x => !string.IsNullOrEmpty(x.idService))
                .Where(x => !string.IsNullOrEmpty(x.Asignado));
                AppendLog("Servicios en transito cargados");
            }

        });

    }

    void Chat(System.Object sender, System.EventArgs e)
    {
    }

    private void LoadSv()
    {
        var lod = fDb.GetCollection("Servicios");

        lod.Listen(async snap =>
        {

            if (snap != null)
            {
                foreach (var change in snap.Changes)
                {
                    var dataC = change.Document.ConvertTo<Services>();

                    objC = new Services
                    {
                        Asignado = dataC.Asignado,
                        Creado = dataC.Creado,
                        DireccionDest = dataC.DireccionDest,
                        DireccionUsr = dataC.DireccionUsr,
                        DistanciaPre = dataC.DistanciaPre,
                        Estado = dataC.Estado,
                        GeoDestino = dataC.GeoDestino,
                        GeoUsr = dataC.GeoUsr,
                        idService = dataC.idService,
                        NoID = dataC.NoID,
                        Motivo_cancelacion = dataC.Motivo_cancelacion,
                        Nombre = dataC.Nombre,
                        uuid = dataC.uuid,
                        Conductor = dataC.Conductor
                    };
                    var docID = change.Document.Id;
                    var inx = change.NewIndex;
                    if (change.ChangeType.ToString() == "Added")
                    {

                        GetServices.Add(objC);
                        _documentTimestamps[docID] = DateTime.Now;
                        //ToAssig(docID);
                        Debug.WriteLine($"Agregado {objC.Creado}");
                        AppendLog($"Obteniendo datos de la base de datos, servicios nuevos {objC.Creado}");

                    }
                    else if (change.ChangeType.ToString() == "Modified")
                    {
                        Debug.WriteLine("Editado");
                        AppendLog($"Servicio modificado {objC.Creado}\n{objC.idService}");
                        //string IDC = change.Document.Id;
                        _documentTimestamps[docID] = DateTime.Now;
                        try
                        {
                            var sort = GetServices.FirstOrDefault(e => e.idService == docID);

                            if (sort != null)
                            {
                                // El servicio con el mismo ID ya está presente, reemplazarlo si es necesario
                                int index = GetServices.IndexOf(sort);
                                GetServices[index] = objC;
                            }
                            else
                            {
                                // El servicio con el mismo ID no está presente, agregarlo a la lista
                                GetServices.Insert(Convert.ToInt32(inx), objC);
                            }
                        }
                        catch (Exception exe)
                        {
                            Debug.WriteLine(exe.Message);
                        }
                    }
                    else if (change.ChangeType.ToString() == "Removed")
                    {
                        Debug.WriteLine("Eliminado");
                        AppendLog($"Servicio elimando {objC.Creado}\n{objC.idService}");

                        try
                        {
                            GetServices.RemoveAt(Convert.ToInt32(inx));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Line 121: {ex.Message}");
                            AppendLog($"Ocurrio un error inesperado en la funcion {ex.Message}");
                        }
                    }
                }
                if (GetServices.Count < 0)
                {
                    Debug.WriteLine("No hay servicios disponibles");
                }
            }
            else
            {
                Debug.WriteLine("No hay servicios disponibles");
            }



            // Actualizar la lista en la interfaz de usuario
            // No hay problema al usar esta tarea en el hilo principal ya que se esta ejecutando correctamente
#pragma warning disable CS0612 // El tipo o el miembro están obsoletos
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
            await Device.InvokeOnMainThreadAsync(() =>
            {
                ServiciosLs.ItemsSource = GetServices.Where(x => x.Estado.StartsWith("disponible")).Where(x => !string.IsNullOrEmpty(x.idService))
                .Where(x => string.IsNullOrEmpty(x.Asignado));
                AppendLog("Sevicios disponibles cargados");
                NumDispo.Text = GetServices.Where(x => x.Estado.StartsWith("disponible")).Where(x => !string.IsNullOrEmpty(x.idService))
                .Where(x => string.IsNullOrEmpty(x.Asignado)).Count().ToString();
                NumEnEspe.Text = GetServices.Where(x => x.Estado.StartsWith("disponible")).Where(x => !string.IsNullOrEmpty(x.idService))
                .Where(x => !string.IsNullOrEmpty(x.Asignado)).Count().ToString();
                NumEnTransi.Text = GetServices.Where(x => x.Estado.StartsWith("enProceso") || x.Estado.StartsWith("aceptado")).Where(x => !string.IsNullOrEmpty(x.idService))
                .Where(x => !string.IsNullOrEmpty(x.Asignado)).Count().ToString();
                LogView.ItemsSource = LogEntries;

            });
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
#pragma warning restore CS0612 // El tipo o el miembro están obsoletos
        });
    }

    void CerrarData(System.Object sender, System.EventArgs e)
    {
        DataView.IsVisible = false;
    }

    void ObtenerData(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        var rs = sender as Frame;
        var dta = rs?.BindingContext as Services;
        if (dta != null)
        {
            DataView.BindingContext = dta;
            DataView.IsVisible = true;
        }
    }

    void MapaTipo(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        if (e.Parameter is string tipoVista)
        {
            // Lista de los Frames y Labels a modificar
            var items = new (Frame frame, Label label, string tipo)[]
            {
                    (BnSate, LblSate, "Satelite"),
                    (BnDirec, LblDirec, "Direcciones")
            };

            // Resetear todos los Frames y Labels a su color original
            foreach (var item in items)
            {
                item.frame.BackgroundColor = colorNormal;
                item.label.TextColor = textColorNormal;
            }

            // Cambiar el Frame y el Label seleccionado
            var seleccionado = items.FirstOrDefault(x => x.tipo == tipoVista);
            if (seleccionado.frame != null)
            {
                seleccionado.frame.BackgroundColor = colorSeleccionado;
                seleccionado.label.TextColor = textColorSeleccionado;

                // Llamar al metodo para cambiar la vista del mapa
                CambiarVistaMapa(tipoVista);
            }
        }
    }

    private void CambiarVistaMapa(string tipoVista)
    {
        if (tipoVista == "Satelite")
        {
            // Código para cambiar el mapa a modo satélite
            MapaSv.MapType = Microsoft.Maui.Maps.MapType.Hybrid;
            Console.WriteLine("Cambiando a vista Satélite...");
            AppendLog("Cargando Satelite");
        }
        else if (tipoVista == "Direcciones")
        {
            // Código para cambiar el mapa a modo normal
            MapaSv.MapType = Microsoft.Maui.Maps.MapType.Street;
            Console.WriteLine("Cambiando a vista de Direcciones...");
            AppendLog("Cargando direcciones");
        }
    }

    void OnMenu(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        if (e.Parameter is string opcion)
        {
            // Lista de los Frames y Labels a modificar
            var items = new (Frame frame, Label label, string opcion)[]
            {
                    (BnServicios, LblServicios, "Servicios"),
                    (BnConfiguraciones, LblConfiguraciones, "Configuraciones"),
                    (BnNotificaciones, LblNotificaciones, "Notificaciones"),
                    (BnSoporte, LblSoporte, "Soporte"),
                    (BnSistema, LblSistema, "Sistema"),
                    (BnAutomatizacion, LblAutomatizacion, "Automatizacion")
            };

            // Resetear todos los Frames y Labels a su color original
            foreach (var item in items)
            {
                item.frame.BackgroundColor = colorNormalM;
                item.label.TextColor = textColorNormalM;
            }

            // Cambiar el Frame y el Label seleccionado
            var seleccionado = items.FirstOrDefault(x => x.opcion == opcion);
            if (seleccionado.frame != null)
            {
                seleccionado.frame.BackgroundColor = colorSeleccionadoM;
                seleccionado.label.TextColor = textColorSeleccionadoM;
            }

            // Lista de las vistas
            var vistas = new (Frame vista, string opcion)[]
            {
                    (VistaServicios, "Servicios"),
                    (VistaConfiguraciones, "Configuraciones"),
                    (VistaNotificaciones, "Notificaciones"),
                    (VistaSoporte, "Soporte"),
                    (VistaSistema, "Sistema"),
                    (VistaAutomatizacion, "Automatizacion")
            };

            // Ocultar todas las vistas
            foreach (var item in vistas)
            {
                item.vista.IsVisible = false;
            }

            // Mostrar solo la vista seleccionada
            var seleccionada = vistas.FirstOrDefault(x => x.opcion == opcion);
            if (seleccionada.vista != null)
            {
                seleccionada.vista.IsVisible = true;
            }
        }
    }

    void MostrarDatos(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {

    }

    async void OnStartTestClicked(System.Object sender, System.EventArgs e)
    {
        await MeasurePingAsync("google.com");
    }

    private async Task MeasurePingAsync(string host)
    {
        try
        {
            using Ping ping = new Ping();
            PingReply reply = await ping.SendPingAsync(host, 5000); // Timeout de 5000 ms

            if (reply.Status == IPStatus.Success)
            {
                ResponseTimeLabel.Text = $"{reply.RoundtripTime} ms";
                NetworkStatusLabel.Text = "Conexión Exitosa";
                NetworkStatusLabel.TextColor = Colors.Green;
            }
            else
            {
                ResponseTimeLabel.Text = "N/A";
                NetworkStatusLabel.Text = "Conexión Fallida";
                NetworkStatusLabel.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            ResponseTimeLabel.Text = "Error";
            NetworkStatusLabel.Text = $"Error: {ex.Message}";
            NetworkStatusLabel.TextColor = Colors.Red;
        }
    }

    private async void OnAsignacionToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value && TiempoAsignado > 1)
        {
            await DisplayAlert("Sistema", "La asignacion ha sido activada con exito", "Aceptar");
            PrimerAsignadoR = true;
            PrimerAsignado();
        }
        else if (e.Value && TiempoAsignado <= 0)
        {
            await DisplayAlert("Sistema", "Ocurrio un error al procesar la activacion, no hay una marca de tiempo establecida para empezar. Por favor, selecciona una", "Aceptar");
            AsignacionSwitch.IsToggled = false;
        }
        else if (!e.Value)
        {
            await DisplayAlert("Sistema", "La asignacion ha sido desactivada con exito", "Aceptar");
            PrimerAsignadoR = false;
            PrimerAsignado();
        }
    }

    private async void OnReasignacionToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value && TiempoAsignado > 1)
        {
            await DisplayAlert("Sistema", "La rasignacion ha sido activada con exito", "Aceptar");
            ReasignadoR = true;
            Reasignado();
        }
        else if (e.Value && TiempoAsignado <= 0)
        {
            await DisplayAlert("Sistema", "Ocurrio un error al procesar la activacion, no hay una marca de tiempo establecida para empezar. Por favor, selecciona una", "Aceptar");
            ReasignacionSwitch.IsToggled = false;
        }
        else if (!e.Value)
        {
            await DisplayAlert("Sistema", "La reasignacion ha sido desactivado con exito", "Aceptar");
            ReasignadoR = false;
            Reasignado();
        }
    }

    private void UpdateSwitchColors()
    {
        //// Cambia el color del pulgar y del fondo según el estado del Switch
        //AsignacionSwitch.ThumbColor = AsignacionSwitch.IsToggled ? Color.FromArgb("#B2264E") : Colors.Gray;
        //AsignacionSwitch.OnColor = AsignacionSwitch.IsToggled ? Color.FromArgb("#B2264E") : Colors.LightGray;

        //ReasignacionSwitch.ThumbColor = ReasignacionSwitch.IsToggled ? Color.FromArgb("#B2264E") : Colors.Gray;
        //ReasignacionSwitch.OnColor = ReasignacionSwitch.IsToggled ? Color.FromArgb("#B2264E") : Colors.LightGray;
    }

    void GetConductor(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        var rs = sender as Frame;
        var dta = rs?.BindingContext as Drivers;
        if (dta != null)
        {
            CDestina.Text = dta.Nombre;
            CToken.Text = dta.NotifyID;
        }
    }

    public void DriversList()
    {
        try
        {
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
            var lod = fDb.GetCollection("Conductores");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma

            lod.Listen(async snap =>
            {
                //getConductor = new List<dynamic>();

                if (snap != null)
                {
                    foreach (var change in snap.Changes)
                    {
                        var offdt = change.Document.ConvertTo<Drivers>();
                        var ondata = new Drivers
                        {
                            Activo = offdt.Activo,
                            Asociacion = offdt.Asociacion,
                            Clave = offdt.Clave,
                            Correo = offdt.Correo,
                            Placas = offdt.Placas,
                            IdIET = offdt.IdIET,
                            Marca = offdt.Marca,
                            Modelo = offdt.Modelo,
                            SociosConductores = offdt.SociosConductores,
                            Nombre = offdt.Nombre,
                            NumeroEconomico = offdt.NumeroEconomico,
                            PrimerSesion = offdt.PrimerSesion,
                            UIDD = offdt.UIDD,
                            UrlFoto = offdt.UrlFoto,
                            EnEspera = offdt.EnEspera,
                            EnServicio = offdt.EnServicio,
                            NotifyID = offdt.NotifyID,
                            Restringido = offdt.Restringido,
                            NumRestric = offdt.NumRestric,
                            SesionActiva = offdt.SesionActiva,
                            Localidad = offdt.Localidad
                        };

                        var docID = change.Document.Id;
                        var inx = change.NewIndex;

                        if (change.ChangeType.ToString() == "Added")
                        {

                            //GetDrivers.Add(ondata);

                            if (ondata.Activo)
                            {
                                GetDrivers.Add(ondata);
                                Debug.WriteLine($"Agregado {change.Document.Id} \nConductor {offdt.Nombre}");
                                AppendLog($"Agregado {change.Document.Id} \nConductor {offdt.Nombre}");
                            }


                        }
                        else if (change.ChangeType.ToString() == "Modified")
                        {
                            Debug.WriteLine("Editado");
                            try
                            {
                                var driver = GetDrivers.FirstOrDefault(e => e.UIDD == docID);
                                if (driver != null)
                                {
                                    // El conductor con el mismo ID ya está presente, borrarlo
                                    if (ondata.Activo)
                                    {
                                        GetDrivers.Remove(driver);
                                        Debug.WriteLine($"El conductor {driver.Nombre} con ID {driver.UIDD}, ha sido eliminado.");
                                        AppendLog($"El conductor {driver.Nombre} con ID {driver.UIDD}, ha sido desactivado remotamente.");
                                        // Agregar el nuevo conductor al final de la lista
                                        GetDrivers.Add(ondata);
                                        Debug.WriteLine($"El nuevo conductor {ondata.Nombre} con ID {ondata.UIDD}, ha sido agregado.");
                                        AppendLog($"El nuevo conductor {ondata.Nombre} con ID {ondata.UIDD}, se ha conectado.");
                                    }
                                    else if (!ondata.Activo)
                                    {
                                        GetDrivers.Remove(driver);
                                        Debug.WriteLine($"El conductor {ondata.Nombre} con ID {ondata.UIDD}, no esta activo, por lo cual no sera agregado a la lista.");
                                        AppendLog($"El conductor {ondata.Nombre} con ID {ondata.UIDD}, se desconecto");

                                    }
                                }
                                else
                                {
                                    GetDrivers.Add(ondata);
                                    // Manejar el caso donde no se encontró un conductor con el ID especificado
                                    Debug.WriteLine("El conductor no existe en la lista.");
                                    AppendLog("El conductor no existe en la lista.");
                                }
                            }
                            catch (Exception exe)
                            {
                                Debug.WriteLine(exe.Message);
                                AppendLog($"Ocurrio un error al procesar los datos, revisar parametros {exe.Message}");
                            }
                        }

                        else if (change.ChangeType.ToString() == "Deleted")
                        {
                            Debug.WriteLine("Eliminado");
                        }
                    }
                }

#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
#pragma warning disable CS0612 // El tipo o el miembro están obsoletos
                // Hacer una copia de la colección GetDrivers
                var driversFiltrados = new ObservableCollection<Drivers>(GetDrivers.ToList());

                await Device.InvokeOnMainThreadAsync(() =>
                {
                    // Actualizar las fuentes de datos de tus vistas con la colección filtrada
                    //DriversIn.ItemsSource = driversFiltrados.Where(x => !x.EnServicio && x.EnEspera && x.Activo);
                    Conductores.ItemsSource = driversFiltrados.Where(x => !x.EnEspera && !x.EnServicio && x.Activo);
                    //DriversOc.ItemsSource = driversFiltrados.Where(x => x.EnServicio && !x.EnEspera && x.Activo);
                    //LoadingData.IsVisible = false;
                });

                //await Device.InvokeOnMainThreadAsync(() => {
                //    DriversIn.ItemsSource = GetDrivers.Where(x => !x.EnServicio).Where(x => x.EnEspera).Where(x => x.Activo);
                //    DriversOn.ItemsSource = GetDrivers.Where(x => !x.EnEspera).Where(x=> !x.EnServicio).Where(x => x.Activo);
                //    DriversOc.ItemsSource = GetDrivers.Where(x => x.EnServicio).Where(x => !x.EnEspera).Where(x => x.Activo);
                //    LoadingData.IsVisible = false;
                //});
#pragma warning restore CS0612 // El tipo o el miembro están obsoletos
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
            });

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ocurrio un error, {GetDrivers.Count} con error en {ex.Message}");
            AppendLog($"Ocurrio un error, {GetDrivers.Count} con error en {ex.Message}");
        }

    }

    private void PrimerAsignado()
    {
        Task.Run(async () =>
        {
            while (PrimerAsignadoR)
            {
                await Task.Delay(TiempoAsignado);
                var item = GetDrivers.Count(x => x.Activo && !x.EnEspera && !x.EnServicio);
                if (item > 0)
                {
                    //CDC
                    var drvList = GetDrivers.FirstOrDefault(x => x.Activo && !x.EnEspera && !x.EnServicio && x.Localidad == "Ciudad del Carmen");
                    if (drvList != null)
                    {
                        var srvList = GetServices.FirstOrDefault(x => string.IsNullOrEmpty(x.Asignado) && x.Estado.StartsWith("disponible") && !string.IsNullOrEmpty(x.idService) && x.RegionId == 3);
                        if (srvList != null)
                        {
                            //Asignar servicio
                            srvList.Asignado = drvList.UIDD;
                            drvList.EnEspera = true;
                            srvList.Conductor = "";
                            srvList.Motivo_cancelacion = "";
                            UpDateService(srvList.idService, srvList);
                            UpdateDrive(drvList.UIDD, drvList);
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                            await Notify.SendNotificationByTokenAsync(drvList.NotifyID, "Nuevo servicio", "Hay un servicio disponible cerca de ti, abre la aplicacion");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
                            Debug.Write($"Servicio con el id {srvList.idService} fue asignado al conductor {drvList.Nombre} con el id {drvList.UIDD}\n");
                            Debug.Write($"El conductor {drvList.Nombre} se puso en espera\n");
                            var lastDrv = GetDrivers.LastOrDefault(x => x.Activo && !x.EnEspera && !x.EnServicio && x.Localidad == "Ciudad del Carmen");
                            if (lastDrv != null)
                            {
                                Debug.Write($"El ultimo conductor en la lista, que se encuentra disponible es {lastDrv.Nombre}\n");
                            }
                        }
                    }
                    //CDCamp
                    var drvListC = GetDrivers.FirstOrDefault(x => x.Activo && !x.EnEspera && !x.EnServicio && x.Localidad == "Lerma" || x.Localidad == "Chiná" || x.Localidad == "San Francisco de Campeche");
                    if (drvListC != null)
                    {
                        var srvListC = GetServices.FirstOrDefault(x => string.IsNullOrEmpty(x.Asignado) && x.Estado.StartsWith("disponible") && !string.IsNullOrEmpty(x.idService) && x.RegionId == 2);
                        if (srvListC != null)
                        {
                            //Asignar servicio
                            srvListC.Asignado = drvListC.UIDD;
                            drvListC.EnEspera = true;
                            srvListC.Conductor = "";
                            srvListC.Motivo_cancelacion = "";
                            UpDateService(srvListC.idService, srvListC);
                            UpdateDrive(drvListC.UIDD, drvListC);
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                            await Notify.SendNotificationByTokenAsync(drvListC.NotifyID, "Nuevo servicio", "Hay un servicio disponible cerca de ti, abre la aplicacion");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
                            Debug.Write($"Servicio con el id {srvListC.idService} fue asignado al conductor {drvListC.Nombre} con el id {drvListC.UIDD}\n");
                            Debug.Write($"El conductor {drvListC.Nombre} se puso en espera\n");
                            AppendLog($"El conductor {drvListC.Nombre} se puso en espera\n");

                            var lastDrv = GetDrivers.LastOrDefault(x => x.Activo && !x.EnEspera && !x.EnServicio && x.Localidad == "Lerma" || x.Localidad == "Chiná" || x.Localidad == "San Francisco de Campeche");
                            if (lastDrv != null)
                            {
                                Debug.Write($"El ultimo conductor en la lista, que se encuentra disponible es {lastDrv.Nombre}\n");
                                AppendLog($"El ultimo conductor en la lista, que se encuentra disponible es {lastDrv.Nombre}\n");

                            }
                        }
                    }
                    else
                    {
                        Debug.Write("No hay conductores disponibles\n");
                        AppendLog($"No hay conductores disponible en este momento");

                    }
                }
                else
                {
                    Debug.Write("No hay conductores activos\n");
                    AppendLog($"No hay conductores disponible en este momento");

                }

            }
        });
    }

    private void Reasignado()
    {
        Task.Run(async () =>
        {
            while (ReasignadoR)
            {
                await Task.Delay(TiempoAsignado);
                for (int i = 0; i < _documentTimestamps.Keys.Count; i++)
                {
                    var documentId = _documentTimestamps.Keys.ElementAt(i);
                    DateTime lastModified = _documentTimestamps[documentId];
                    //Tester

                    if (DateTime.Now - lastModified >= TimeSpan.FromSeconds(30))
                    {
                        // Reasignación de servicio en Ciudad del Carmen
                        var sRV_CDC = GetServices.FirstOrDefault(x => x.idService == documentId && x.Estado.StartsWith("disponible") && !string.IsNullOrWhiteSpace(x.Asignado) && x.RegionId == 3);
                        if (sRV_CDC != null)
                        {
                            var dRsV_CDC = GetDrivers.FirstOrDefault(x => x.Activo && !x.EnEspera && !x.EnServicio && !x.UIDD.Contains(sRV_CDC.Asignado) && x.Localidad == "Ciudad del Carmen");
                            var dRsVR_CDC = GetDrivers.FirstOrDefault(x => x.Activo && x.EnEspera && !x.EnServicio && x.UIDD.Contains(sRV_CDC.Asignado) && x.Localidad == "Ciudad del Carmen");
                            var dRsOf_CDC = GetDrivers.FirstOrDefault(x => !x.Activo && x.EnEspera && !x.EnServicio && x.UIDD.Contains(sRV_CDC.Asignado) && x.Localidad == "Ciudad del Carmen");
                            var dRsOfFx_CDC = GetDrivers.FirstOrDefault(x => !x.Activo && !x.EnEspera && x.EnServicio && x.UIDD.Contains(sRV_CDC.Asignado) && x.Localidad == "Ciudad del Carmen");

                            if (dRsV_CDC != null)
                            {
                                sRV_CDC.Asignado = dRsV_CDC.UIDD;
                                UpDateService(sRV_CDC.idService, sRV_CDC);
                                Debug.Write($"El conductor {dRsV_CDC.Nombre} ha sido reasignado al servicio {sRV_CDC.idService}\n");

                                dRsV_CDC.EnEspera = true;
                                UpdateDrive(dRsV_CDC.UIDD, dRsV_CDC);
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                                await Notify.SendNotificationByTokenAsync(dRsV_CDC.NotifyID, "Nuevo servicio reasignado", "Se te ha reasignado un servicio disponible.");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma

#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                                await Notify.SendNotificationByTokenAsync(sRV_CDC.NoID, "Aviso", "Se ha encontrado un nuevo conductor para tu servicio.");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
                            }
                            else
                            {
                                Debug.Write($"No se pudo reasignar el servicio {sRV_CDC.idService} a un nuevo conductor en Ciudad del Carmen\n");
                                sRV_CDC.Estado = "cancelado";
                                sRV_CDC.Asignado = "";
                                sRV_CDC.Motivo_cancelacion = "No hay conductores disponibles";
                                UpDateService(sRV_CDC.idService, sRV_CDC);
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                                await Notify.SendNotificationByTokenAsync(sRV_CDC.NoID, "Aviso", "Tu servicio ha sido cancelado por falta de conductores disponibles.");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
                            }

                            if (dRsVR_CDC != null)
                            {
                                dRsVR_CDC.EnEspera = false;
                                UpdateDrive(dRsVR_CDC.UIDD, dRsVR_CDC);
                            }
                            if (dRsOf_CDC != null)
                            {
                                dRsOf_CDC.Activo = false;
                                dRsOf_CDC.EnEspera = false;
                                dRsOf_CDC.EnServicio = false;
                                UpdateDrive(dRsOf_CDC.UIDD, dRsOf_CDC);
                            }
                            if (dRsOfFx_CDC != null)
                            {
                                dRsOfFx_CDC.Activo = false;
                                dRsOfFx_CDC.EnEspera = false;
                                dRsOfFx_CDC.EnServicio = false;
                                UpdateDrive(dRsOfFx_CDC.UIDD, dRsOfFx_CDC);
                            }
                        }

                        // Reasignación de servicio en Campeche
                        var sRV_Campeche = GetServices.FirstOrDefault(x => x.idService == documentId && x.Estado.StartsWith("disponible") && !string.IsNullOrWhiteSpace(x.Asignado) && x.RegionId == 2);

                        if (sRV_Campeche != null)
                        {
                            var dRsV_Campeche = GetDrivers.FirstOrDefault(x => x.Activo && !x.EnEspera && !x.EnServicio && !x.UIDD.Contains(sRV_Campeche.Asignado) && x.Localidad == "Lerma" || x.Localidad == "Chiná" || x.Localidad == "San Francisco de Campeche");
                            var dRsVR_Campeche = GetDrivers.FirstOrDefault(x => x.Activo && x.EnEspera && !x.EnServicio && x.UIDD.Contains(sRV_Campeche.Asignado) && x.Localidad == "Lerma" || x.Localidad == "Chiná" || x.Localidad == "San Francisco de Campeche");
                            var dRsOf_Campeche = GetDrivers.FirstOrDefault(x => !x.Activo && x.EnEspera && !x.EnServicio && x.UIDD.Contains(sRV_Campeche.Asignado) && x.Localidad == "Lerma" || x.Localidad == "Chiná" || x.Localidad == "San Francisco de Campeche");
                            var dRsOfFx_Campeche = GetDrivers.FirstOrDefault(x => !x.Activo && !x.EnEspera && x.EnServicio && x.UIDD.Contains(sRV_Campeche.Asignado) && x.Localidad == "Lerma" || x.Localidad == "Chiná" || x.Localidad == "San Francisco de Campeche");

                            if (dRsV_Campeche != null)
                            {
                                sRV_Campeche.Asignado = dRsV_Campeche.UIDD;
                                UpDateService(sRV_Campeche.idService, sRV_Campeche);
                                Debug.Write($"El conductor {dRsV_Campeche.Nombre} ha sido reasignado al servicio {sRV_Campeche.idService}\n");

                                dRsV_Campeche.EnEspera = true;
                                UpdateDrive(dRsV_Campeche.UIDD, dRsV_Campeche);
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                                await Notify.SendNotificationByTokenAsync(dRsV_Campeche.NotifyID, "Nuevo servicio reasignado", "Se te ha reasignado un servicio disponible.");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma

#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                                await Notify.SendNotificationByTokenAsync(sRV_Campeche.NoID, "Aviso", "Se ha encontrado un nuevo conductor para tu servicio.");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
                            }
                            else
                            {
                                Debug.Write($"No se pudo reasignar el servicio {sRV_Campeche.idService} a un nuevo conductor en Campeche\n");
                                sRV_Campeche.Estado = "cancelado";
                                sRV_Campeche.Asignado = "";
                                sRV_Campeche.Motivo_cancelacion = "No hay conductores disponibles";
                                UpDateService(sRV_Campeche.idService, sRV_Campeche);
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                                await Notify.SendNotificationByTokenAsync(sRV_Campeche.NoID, "Aviso", "Tu servicio ha sido cancelado por falta de conductores disponibles.");
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
                            }

                            if (dRsVR_Campeche != null)
                            {
                                dRsVR_Campeche.EnEspera = false;
                                UpdateDrive(dRsVR_Campeche.UIDD, dRsVR_Campeche);
                            }
                            if (dRsOf_Campeche != null)
                            {
                                dRsOf_Campeche.Activo = false;
                                dRsOf_Campeche.EnEspera = false;
                                dRsOf_Campeche.EnServicio = false;
                                UpdateDrive(dRsOf_Campeche.UIDD, dRsOf_Campeche);
                            }
                            if (dRsOfFx_Campeche != null)
                            {
                                dRsOfFx_Campeche.Activo = false;
                                dRsOfFx_Campeche.EnEspera = false;
                                dRsOfFx_Campeche.EnServicio = false;
                                UpdateDrive(dRsOfFx_Campeche.UIDD, dRsOfFx_Campeche);
                            }
                        }
                        else
                        {
                            Debug.Write($"No se encontró ningún servicio con este valor o ya ha sido aceptado, en proceso, cancelado o finalizado\n");
                        }
                    }

                }

            }
        });
    }

    private async void CancelService(string ID, Services Srv)
    {
        try
        {
            //Borra el servicio cancelado
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
            await fDb.GetCollection("Servicios").Document(ID).DeleteAsync();
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
            await fDb.GetCollection("ServiciosCancelados").Document(ID).SetAsync(Srv);
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private async void UpDateService(string ID, Services Srv)
    {
        try
        {
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
            await fDb.GetCollection("Servicios").Document(ID).SetAsync(Srv);
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private async void UpdateDrive(string ID, Drivers drive)
    {
        try
        {
#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
            await fDb.GetCollection("Conductores").Document(ID).SetAsync(drive);
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }

    }

    async void EnviarNt(System.Object sender, System.EventArgs e)
    {
        await Notify.SendNotificationByTokenAsync(CToken.Text, CTitulo.Text, CMensaje.Text);
        CToken.Text = string.Empty;
        CTitulo.Text = string.Empty;
        CMensaje.Text = string.Empty;
    }

    void TSegundos_Clicked(System.Object sender, System.EventArgs e)
    {
        TiempoAsignado = 30000;
        TiempoAsig.Text = "Ciclo de actualizacion de servicios 30 segundos";
        SeleccionarBoton(TSegundos);
    }

    void SSegundos_Clicked(System.Object sender, System.EventArgs e)
    {
        TiempoAsignado = 60000;
        TiempoAsig.Text = "Ciclo de actualizacion de servicios 60 segundos";

        SeleccionarBoton(SSegundos);
    }

    void NSegundos_Clicked(System.Object sender, System.EventArgs e)
    {
        TiempoAsignado = 90000;
        TiempoAsig.Text = "Ciclo de actualizacion de servicios 90 segundos";

        SeleccionarBoton(NSegundos);
    }

    void CSegundos_Clicked(System.Object sender, System.EventArgs e)
    {
        TiempoAsignado = 120000;
        TiempoAsig.Text = "Ciclo de actualizacion de servicios 120 segundos";

        SeleccionarBoton(CSegundos);
    }

    // Método para cambiar el color del botón seleccionado y restaurar los demás
    void SeleccionarBoton(Button botonSeleccionado)
    {
        // Restaurar el color de todos los botones
        TSegundos.BackgroundColor = Color.FromHex("#B2264E");
        SSegundos.BackgroundColor = Color.FromHex("#B2264E");
        NSegundos.BackgroundColor = Color.FromHex("#B2264E");
        CSegundos.BackgroundColor = Color.FromHex("#B2264E");

        // Cambiar el color del botón seleccionado
        botonSeleccionado.BackgroundColor = Color.FromHex("#007ACC"); // Color del botón seleccionado
    }
}


