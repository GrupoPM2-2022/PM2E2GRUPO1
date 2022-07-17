using Plugin.AudioRecorder;
using Plugin.Media;
using Plugin.Media.Abstractions;
using PM2E2GRUPO1.Controller;
using PM2E2GRUPO1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PM2E2GRUPO1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateSite : ContentPage
    {
        byte[] Image;
        private AudioRecorderService audioRecorderService = new AudioRecorderService()
        {
            StopRecordingOnSilence = false,
            StopRecordingAfterTimeout = false
        };

        private AudioPlayer audioPlayer = new AudioPlayer();

        private bool reproducir = false;
        MediaFile FileFoto = null;
        public UpdateSite()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            getLatitudeAndLongitude();
        }


        private async void btnGrabar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Microphone>();
                var status2 = await Permissions.RequestAsync<Permissions.StorageRead>();
                var status3 = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (status != PermissionStatus.Granted & status2 != PermissionStatus.Granted & status3 != PermissionStatus.Granted)
                {
                    return; // si no tiene los permisos no avanza
                }

                if (audioRecorderService.IsRecording)
                {
                    await audioRecorderService.StopRecording();


                    audioPlayer.Play(audioRecorderService.GetAudioFilePath());

                    txtMessage.Text = "No esta grabando";
                    btnGrabar.Text = "Grabar audio";

                    reproducir = true;
                }
                else
                {
                    await audioRecorderService.StartRecording();


                    txtMessage.Text = "Esta grabando";

                    btnGrabar.Text = "Dejar de Grabar";

                    //reproducir = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alerta", ex.Message, "OK");
            }
        }

        private async void btnActualizar_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtDescriptionE.Text) || txtDescriptionE.Text.Length > 250)
            {
                Message("Aviso", "Debe escribir una breve Description");
                return;
            }

            try
            {
                var idSitio = (string.IsNullOrEmpty(txtIdE.Text)) ? 0 : int.Parse(txtIdE.Text);
                
                var sitio = new Sitio()
                {
                    Id = idSitio,
                    Latitude = double.Parse(txtLatitudeE.Text),
                    Longitude = double.Parse(txtLongitudeE.Text),
                    Description = txtDescriptionE.Text,
                    Image = Image,
                    AudioFile = ConvertAudioToByteArray()
                    //pathImage = FileFoto.Path
                };

                var result = await SitioController.UpdateSitio(sitio);

                if (result)
                {
                    Message("Aviso", "Sitio Actualizado correctamente");
                    
                }
                else
                {
                    Message("Aviso", "No se pudo Actualizar el sitio");
                }

            }
            catch (Exception ex)
            {

                Message("Error: ", ex.Message);
            }
        }


        private async void getLatitudeAndLongitude()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    var localizacion = await Geolocation.GetLocationAsync();
                    txtLatitudeE.Text = Math.Round(localizacion.Latitude, 5) + "";
                    txtLongitudeE.Text = Math.Round(localizacion.Longitude, 5) + "";
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
        private async void Message(string title, string message)
        {
            await DisplayAlert(title, message, "OK");
        }



        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            bool response = await Application.Current.MainPage.DisplayAlert("Advertencia", "Seleccione el tipo de imagen que desea", "Camara", "Galeria");

            if (response)
                GetImageFromCamera();
            else
                GetImageFromGallery();

        }

        private async void GetImageFromCamera()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status == PermissionStatus.Granted)
            {
                try
                {
                    FileFoto = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        PhotoSize = PhotoSize.Medium,
                    });

                    if (FileFoto == null)
                        return;

                    imgFotoE.Source = ImageSource.FromStream(() => { return FileFoto.GetStream(); });
                    Image = File.ReadAllBytes(FileFoto.Path);
                }
                catch (Exception)
                {
                    Message("Advertencia", "Se produjo un error al tomar la fotografia.");
                }
            }
            else
            {
                await Permissions.RequestAsync<Permissions.Camera>();
            }
        }

        private async void GetImageFromGallery()
        {
            try
            {
                if (CrossMedia.Current.IsPickPhotoSupported)
                {
                    var FileFoto = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Medium,
                    });
                    if (FileFoto == null)
                        return;

                    imgFotoE.Source = ImageSource.FromStream(() => { return FileFoto.GetStream(); });
                    Image = File.ReadAllBytes(FileFoto.Path);
                }
                else
                {
                    Message("Advertencia", "Se produjo un error al seleccionar la imagen");
                }
            }
            catch (Exception)
            {
                Message("Advertencia", "Se produjo un error al seleccionar la imagen");
            }

        }

        public byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private Byte[] ConvertAudioToByteArray()
        {
            Stream audioFile = audioRecorderService.GetAudioFileStream();

            //var mStream = new MemoryStream(File.ReadAllBytes(audioRecorderService.GetAudioFilePath()));
            //var mStream = (MemoryStream)audioFile;

            Byte[] bytes = ReadFully(audioFile);
            return bytes;
        }

    }


}