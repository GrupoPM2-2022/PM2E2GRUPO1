using Newtonsoft.Json;
using PM2E2GRUPO1.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PM2E2GRUPO1.Controller
{
    public class SitioController
    {
        private static readonly string URL_SITIOS = "https://dennisdomain.com/microservices/examen2p/api/sitios/";
        private static HttpClient client = new HttpClient();

        public async static Task<List<Sitio>> GetAllSite()
        {
            List<Sitio> listBooks = new List<Sitio>();
            try
            {
                var uri = new Uri(URL_SITIOS);
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    listBooks = JsonConvert.DeserializeObject<List<Sitio>>(content);
                    return listBooks;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return listBooks;
        }

        public async static Task<bool> DeleteSite(string id)
        {
            try
            {
                var uri = new Uri(URL_SITIOS + "?id=" + id);
                var result = await client.DeleteAsync(uri);
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async static Task<bool> CreateSite(Sitio sitio)
        {
            try
            {
                Uri requestUri = new Uri(URL_SITIOS);
                var jsonObject = JsonConvert.SerializeObject(sitio);
                var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }


        public async static Task<bool> UpdateSitio(Sitio sitio)
        {
            try
            {
                Uri requestUri = new Uri(URL_SITIOS + "?id=" + sitio.Id);
                var jsonObject = JsonConvert.SerializeObject(sitio);
                var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

    }
}
