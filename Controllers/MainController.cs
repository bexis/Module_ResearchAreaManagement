using System.Web.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Text;
using BExIS.Tcd.Helpers;

namespace BExIS.Modules.Tcd.UI.Controllers
{
    public class MainController : Controller
    {
        private String serverAddress = "";
        public MainController()
        {
            ServerInformation serverInformation = new ServerInformation();
            serverAddress = serverInformation.ServerAddress;
        }
        // GET: TDB/Climate
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult visual_plotstation_raw()
        {
            return View();
        }

        public ActionResult visual_parameter_raw()
        {
            return View();
        }

        public ActionResult export()
        {
            return View();
        }

        public ActionResult map()
        {
            return View();
        }

        public ActionResult advanced_status()
        {
            return View();
        }

        public ActionResult status()
        {
            return View();
        }

        public ActionResult catalog()
        {
            return View();
        }

        public ActionResult map2()
        {
            return View();
        }

        public ActionResult info()
        {
            return View();
        }

        public ActionResult export_create()
        {
            return View();
        }

        public ActionResult export_sensors()
        {
            return View();
        }

        public ActionResult export_settings()
        {
            return View();
        }

        public ActionResult export_plots()
        {
            return View();
        }

        public ActionResult export_time()
        {
            return View();
        }

        public ActionResult supplement()
        {
            return View();
        }

        public ActionResult visualisation_meta()
        {
            return View();
        }

        public async Task<FileResult> ClimateData()
        {
            HttpWebResponse result = await GetClimateData();
            TransferCookiesResponse(result, this.Response);
            if (this.Response.ContentType.ToLower().Equals("application/zip; charset=UTF-8".ToLower()) || this.Response.ContentType.ToLower().Equals("application/zip;charset=utf-8".ToLower()) || this.Response.ContentType.ToLower().Equals("application/zip".ToLower()))
            {
                this.Response.AppendHeader("Content-Disposition", "attachment; filename=climate_data.zip");
                // log download in table
            }
            return File(result.GetResponseStream(), result.ContentType); ;
        }

        private Task<HttpWebResponse> GetClimateData()
        {
            return Task.Run(() =>
            {
                String a = this.Request.RawUrl;
                StreamReader reader = new StreamReader(Request.InputStream);
                string requestFromPost = Uri.UnescapeDataString(reader.ReadToEnd());
                String requestUrl = this.Request.RawUrl;
                string drequestUrl = "";
                string xxlClimateDataUrl = serverAddress;// "http://webislab16.medien.uni-weimar.de:8080/0123456789abcdef/";//WebConfigurationManager.AppSettings["xxlClimateDataUrl"];// 
                HttpWebRequest request;
                HttpWebResponse result = null;

                if (this.Request.QueryString.HasKeys())
                {

                    string v = Request.QueryString["request"];
                    if (v != null)
                    {

                        if (this.Request.HttpMethod.Equals("GET"))
                        {
                            drequestUrl = xxlClimateDataUrl + HttpUtility.UrlDecode(v);
                            NameValueCollection queryString = Request.QueryString;

                            foreach (string key in queryString.AllKeys)
                            {
                                if (!key.ToLower().Equals("request"))
                                    drequestUrl = drequestUrl + "&" + key + "=" + Request.QueryString[key];
                            }
                            request = (HttpWebRequest)WebRequest.Create(drequestUrl);
                            Uri target = new Uri(drequestUrl);
                            // Set the ContentType property of the WebRequest.
                            request.Credentials = CredentialCache.DefaultCredentials;
                            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                            TransferCookies(Request, request);
                            try
                            {
                                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                                StreamReader sr = new StreamReader(response.GetResponseStream());
                                String ax = response.ContentType;
                                result = response;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Can not fetch address: " + drequestUrl);
                            }
                        }
                        else
                        {
                            request = WebRequest.Create(xxlClimateDataUrl + HttpUtility.UrlDecode(v)) as HttpWebRequest;
                            Uri target = new Uri(xxlClimateDataUrl + HttpUtility.UrlDecode(v));

                            TransferCookies(Request, request);

                            // Set the Method property of the request to POST.
                            request.Method = "POST";
                            // Create POST data and convert it to a byte array.
                            byte[] byteArray = Encoding.UTF8.GetBytes(requestFromPost);
                            // Set the ContentType property of the WebRequest.
                            request.ContentType = "text";
                            // Set the ContentLength property of the WebRequest.
                            request.ContentLength = byteArray.Length;
                            request.Credentials = CredentialCache.DefaultCredentials;
                            // Get the request stream.
                            Stream dataStream = request.GetRequestStream();
                            // Write the data to the request stream.
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            // Close the Stream object.
                            dataStream.Close();
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            StreamReader sr = new StreamReader(response.GetResponseStream());
                            String ax = response.ContentType;
                            result = response;
                        }
                    }
                }

                // If required by the server, set the credentials.  

                // Get the response.  
                return result;
            });
        }

        private void TransferCookies(HttpRequestBase SourceHttpRequest, HttpWebRequest TargetHttpWebRequest)
        {
            HttpCookieCollection sourceCookies = SourceHttpRequest.Cookies;
            if (sourceCookies.Count > 0)
            {
                TargetHttpWebRequest.CookieContainer = new CookieContainer();
                for (int i = 1; i < sourceCookies.Count; i++)
                {
                    HttpCookie cSource = sourceCookies[i];
                    Cookie cookieTarget = new Cookie()
                    {//cSource.Name
                        Domain = TargetHttpWebRequest.RequestUri.Host,
                        Name = cSource.Name,
                        Path = "/0123456789abcdef/export",
                        Secure = cSource.Secure,
                        Value = cSource.Value
                    };
                    TargetHttpWebRequest.CookieContainer.Add(cookieTarget);
                }
            }
        }


        private void TransferCookiesResponse(HttpWebResponse SourceHttpResponse, HttpResponseBase TargetHttpWebResponse)
        {
            CookieCollection sourceCookies = SourceHttpResponse.Cookies;
            if (sourceCookies.Count > 0)
            {
                for (int i = 0; i < sourceCookies.Count; i++)
                {
                    Cookie cSource = sourceCookies[i];
                    TargetHttpWebResponse.Cookies[cSource.Name].Value = cSource.Value;
                }
            }
        }
    }
}