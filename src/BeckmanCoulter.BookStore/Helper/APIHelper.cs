using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeckmanCoulter.BookStore.Helper
{
    public class ApiHelper
    {
        public HttpClient SpecifyClient;
        public async Task<string> GetHttpMethodAsync(string url, Dictionary<string, string> headers, bool tokenRequired = false)
        {
            var httpClient = new HttpClient();
            this.SpecifyClient = httpClient;
            Uri uri = new Uri(url);

            var requestMsg = new HttpRequestMessage(HttpMethod.Get, uri);
            BuildRequestHeaders(requestMsg, headers, tokenRequired);

            try
            {
                return await SendRequestAsync(httpClient, requestMsg);
            }
            catch (Exception ex)
            {
                //To Do handle other possible exceptions
            }

            return null;
        }

        private void BuildRequestHeaders(HttpRequestMessage requestMsg, Dictionary<string, string> headers, bool tokenRequired)
        {
            if (headers != null && headers.Count > 0)
            {
                foreach (var item in headers)
                {
                    requestMsg.Headers.Add(item.Key, item.Value);
                }
            }

            if (tokenRequired)
            {
                //To Do --add bearer token here   
            }

        }

        private async Task<string> SendRequestAsync(HttpClient httpClient, HttpRequestMessage requestMsg)
        {
            HttpResponseMessage response = await httpClient.SendAsync(requestMsg,HttpCompletionOption.ResponseContentRead);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else
            {
                return null;
            }

        }
    }
}
