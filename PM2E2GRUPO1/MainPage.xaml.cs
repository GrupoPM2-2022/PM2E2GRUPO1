using Plugin.Media;
using Plugin.Media.Abstractions;
using PM2E2GRUPO1.Models;
using PM2E2GRUPO1.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Plugin.AudioRecorder;
using PM2E2GRUPO1.Controller;
using Acr.UserDialogs;

namespace PM2E2GRUPO1
{
    public partial class MainPage : ContentPage
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

        public MainPage()
        {
            InitializeComponent();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            getLatitudeAndLongitude();
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            bool response = await Application.Current.MainPage.DisplayAlert("Advertencia", "Seleccione el tipo de imagen que desea", "Camara", "Galeria");

            if (response)
                GetImageFromCamera();
            else
                GetImageFromGallery();
            
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

                    imgFoto.Source = ImageSource.FromStream(() => { return FileFoto.GetStream(); });
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

                    imgFoto.Source = ImageSource.FromStream(() => { return FileFoto.GetStream(); });
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

        private void btnExit_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
        }

        private async void btnAdd_Clicked(object sender, EventArgs e)
        {
            if (Image == null)
            {
                Message("Aviso", "Aun no se a tomado una foto: Presione la imagen de ejemplo para capturar una imagen");
                return;
            }

            if (string.IsNullOrEmpty(txtLatitude.Text) || string.IsNullOrEmpty(txtLongitude.Text))
            {
                Message("Aviso", "Aun no se obtiene la ubicacion");
                getLatitudeAndLongitude();
                return;
            }

            if (string.IsNullOrEmpty(txtDescription.Text) || txtDescription.Text.Length > 250)
            {
                Message("Aviso", "Debe escribir una breve Description");
                return;
            }

            if (!reproducir)
            {
                Message("Aviso", "No ha grabado ningun audio");
                return;
            }


            try
            {

                UserDialogs.Instance.ShowLoading("Guardando Sitio", MaskType.Clear);

                var sitio = new Sitio()
                {
                    Latitude = double.Parse(txtLatitude.Text),
                    Longitude = double.Parse(txtLongitude.Text),
                    Description = txtDescription.Text,
                    Image = Image,
                    AudioFile = ConvertAudioToByteArray()
                    //pathImage = FileFoto.Path
                };

                var result = await SitioController.CreateSite(sitio);

                UserDialogs.Instance.HideLoading();
                await Task.Delay(500);

                if (result)
                {
                    Message("Aviso", "Sitio agregado correctamente");
                    clearComp();
                }
                else
                {
                    Message("Aviso", "No se pudo agregar el sitio");
                }

            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                
                await Task.Delay(500);
                
                Message("Error: ", ex.Message);
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

        private async void btnList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListSite());
        }

        private async void getLatitudeAndLongitude()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    var localizacion = await Geolocation.GetLocationAsync();
                    txtLatitude.Text = Math.Round(localizacion.Latitude, 5) + "";
                    txtLongitude.Text = Math.Round(localizacion.Longitude, 5) + "";
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

        private void clearComp()
        {
            imgFoto.Source = "imgMuestra.png";
            txtDescription.Text = "";
            Image = null;
            getLatitudeAndLongitude();
        }


        #region Metodos Utiles
        private Byte[] ConvertAudioToByteArray()
        {
            Stream audioFile = audioRecorderService.GetAudioFileStream();

            //var mStream = new MemoryStream(File.ReadAllBytes(audioRecorderService.GetAudioFilePath()));
            //var mStream = (MemoryStream)audioFile;

            Byte[] bytes = ReadFully(audioFile);
            return bytes;
        }

        private Byte[] ConvertImageToByteArray()
        {
            if (FileFoto != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    Stream stream = FileFoto.GetStream();

                    stream.CopyTo(memory);

                    return memory.ToArray();
                }
            }

            return null;
        }

        private async void Message(string title, string message)
        {
            await DisplayAlert(title, message, "OK");
        }

        #endregion Metodos Utiles

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
    }
}
