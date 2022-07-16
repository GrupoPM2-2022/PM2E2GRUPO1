using PM2E2GRUPO1.Models;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using Path = System.IO.Path;

namespace PM2E2GRUPO1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        Sitio Sitio = null;

        public MapPage(Sitio sitio)
        {
            InitializeComponent();

            Sitio = sitio;
        }


        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {

                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();


                if (status == PermissionStatus.Granted)
                {

                    var localizacion = await Geolocation.GetLocationAsync();

                    if (localizacion != null)
                    {

                        var pin = new Pin()
                        {
                            Type = PinType.SavedPin,
                            Position = new Position(Sitio.Latitude, Sitio.Longitude),
                            Label = "Description",
                            Address = Sitio.Description

                        };


                        mapa.Pins.Add(pin);

                        //mapa.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(localizacion.Latitude, localizacion.Longitude), Distance.FromMeters(100)));


                        mapa.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(Sitio.Latitude, Sitio.Longitude), Distance.FromMeters(100)));

                    }

                }
                else
                {
                    await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Equals("Location services are not enabled on device."))
                {

                    Message("Error", "Servicio de localizacion no encendido");
                }
                else
                {
                    Message("Error", e.Message);

                }
            }

        }


        private async void btnShare_Clicked(object sender, EventArgs e)
        {


            try
            {
                //"/storage/emulated/0/Android/data/com.companyname.pm2e10280/files/Pictures/MisUbicaciones/IMG_2022061…"
                //folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Sitio.nameImage);

                // if(!File.Exists(Path.GetFileName(Sitio.pathImage)))
                //   File.WriteAllBytes(Sitio.pathImage, Sitio.Image);

                //var Image = new ShareFile(Sitio.pathImage);


                //if (Image == null)
                //{
                //    Message("Aviso", "No se pudo compartir la imagen");
                //    return;
                //}


                //await Share.RequestAsync(new ShareFileRequest
                //{
                //    Title = Sitio.Description,
                //    File = Image
                //});

            }
            catch (Exception ex)
            {

                Message("Error: ", ex.Message);
            }

        }


        private async void Message(string title, string message)
        {
            await DisplayAlert(title, message, "OK");
        }


    }
}