using BExIS.App.Bootstrap.Attributes;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.PMM.UI.Controllers
{

    public class PlotOutController : ApiController
    {

        // GET: api/Plot
        /// <summary>
        /// Get a list of all plots
        /// </summary>
        /// <returns>List of plots with geometry</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Plot")]
      //  [HttpGet]
        public HttpResponseMessage Get()
        {
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(token, "plot", "");
        }


        // GET: api/Subplot
        /// <summary>
        /// Get a list of all subplots
        /// </summary>
        /// <returns>List of subplots with geometry</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Subplot")]
        public HttpResponseMessage GetSubplots()
        {
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(token, "subplot", "");
        }


        // GET: api/Subplot/{id}
        /// <summary>
        /// Get subplots of one plot
        /// </summary>
        /// <returns>List of all subplots in one plot with geometry</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Subplot/{id}")]
        public HttpResponseMessage GetSubplots(string id)
        {
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(token, "subplot", id);
        }


        // GET: api/Subplot/grouped/{id}
        /// <summary>
        /// Get subplots of one explo (A, S, H)
        /// </summary>
        /// <returns>List of all subplots in one explo with geometry</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Subplot/grouped/{id}")]
        public HttpResponseMessage GetSubplotsGrouped(string id)
        {
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(token, "subplot_grouped", id);
        }


        private HttpResponseMessage getData( string token, string type, string id)
        {

            using (UserManager userManager = new UserManager())
            {
             
                // check token
                if (String.IsNullOrEmpty(token))
                {
                    var request = Request.CreateResponse();
                    request.Content = new StringContent("Bearer token not exist.");

                    return request;
                }
                

                // get user from token
                User user = ControllerContext.RouteData.Values["user"] as User;

                if (user != null)
                {
                    
                    // check permission fror user
                    if (checkPermission(new Tuple<string, string, string>("PMM", "Main", "*"), user))
                    {
                        // get data related to API call
                        var data = "";
                        if (type == "plot")
                        {
                            data = getPlots();
                        }
                        else if (type == "subplot")
                        {
                            data = getSubplots(id);
                        }

                        else if (type == "subplot_grouped")
                        {
                            data = getSubplotsGrouped(id);
                        }
                        

                        // return result as JSON
                        var response = Request.CreateResponse(HttpStatusCode.OK);
                        response.Content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        return response;
                    }
                    else // has rights?
                    {
                        var request = Request.CreateResponse();
                        request.Content = new StringContent("User has no read right.");

                        return request;
                    }
                }
                else
                {
                    var request = Request.CreateResponse();
                    request.Content = new StringContent("User is not available.");

                    return request;
                }
            }
        }


        // get a list of all plots
        private string getPlots()
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("SELECT jsonb_build_object( 'type', 'FeatureCollection', 'features', jsonb_agg(features.feature)) FROM ( SELECT jsonb_build_object('type', 'Feature', 'id', id, 'geometry', ST_AsGeoJSON(geometrytext::geometry)::jsonb,'properties', to_jsonb(inputs) - 'id' - 'geometrytext') AS feature FROM(SELECT * FROM pmm_plot) inputs) features; "));
            

            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                {
                    var result = uow.ExecuteScalar(mvBuilder.ToString());
                    return (string)result;
                }
            }
            catch
            {
                return "";
            }
        }

        // get list of subplots (all or for one explo)
        private string getSubplots(string id)
        {

            StringBuilder mvBuilder = new StringBuilder();

            if (id.Length > 0)
            {
                mvBuilder.AppendLine(string.Format("SELECT jsonb_build_object('type', 'FeatureCollection','features', jsonb_agg(features.feature)) FROM( SELECT jsonb_build_object('type', 'Feature','id', pmm_geometryinformation_id,'geometry', ST_AsGeoJSON(geometrytext::geometry)::jsonb, 'properties', to_jsonb(inputs) - 'pmm_geometryinformation_id' - 'geometrytext') AS feature FROM(SELECT t.id as pmm_geometryinformation_id, geometryid, t.geometrytype, t.geometrytext, color, linewidth, t.status, t.coordinate, t.coordinatetype, t.name, t.description, t.referencepoint, t.plotid FROM pmm_geometryinformation t left join pmm_plot on pmm_plot.id = t.plotid where pmm_plot.plotid = '" + id + "' order by  ST_Perimeter(t.geometrytext) DESC) inputs) features; "));
            }
            else
            {
                mvBuilder.AppendLine(string.Format("SELECT jsonb_build_object('type', 'FeatureCollection','features', jsonb_agg(features.feature)) FROM( SELECT jsonb_build_object('type', 'Feature','id', pmm_geometryinformation_id,'geometry', ST_AsGeoJSON(geometrytext::geometry)::jsonb, 'properties', to_jsonb(inputs) - 'pmm_geometryinformation_id' - 'geometrytext') AS feature FROM(SELECT t.id as pmm_geometryinformation_id, geometryid, t.geometrytype, t.geometrytext, color, linewidth, t.status, t.coordinate, t.coordinatetype, t.name, t.description, t.referencepoint, t.plotid FROM pmm_geometryinformation t left join pmm_plot on pmm_plot.id = t.plotid  order by  ST_Perimeter(t.geometrytext) DESC) inputs) features; "));
            }

            // execute the statement
            try
                {
                    using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                    {
                        var result = uow.ExecuteScalar(mvBuilder.ToString());
                        return (string)result;
                    }
                }
                catch
                {
                    return "";
                }

        }

        // get all subplots grouped by explo (A, H, S)
        private string getSubplotsGrouped(string id)
        {
            var allowed_id = new string[] { "A", "H", "S" };
            if (allowed_id.Contains(id))
            {

                StringBuilder mvBuilder = new StringBuilder();
                mvBuilder.AppendLine(string.Format("SELECT jsonb_build_object('type', 'FeatureCollection','features', jsonb_agg(features.feature)) FROM( SELECT jsonb_build_object('type', 'Feature','id', pmm_geometryinformation_id,'geometry', ST_AsGeoJSON(geometrytext::geometry)::jsonb, 'properties', to_jsonb(inputs) - 'pmm_geometryinformation_id' - 'geometrytext') AS feature FROM(SELECT t.id as pmm_geometryinformation_id, geometryid, t.geometrytype, t.geometrytext, color, linewidth, t.status, t.coordinate, t.coordinatetype, t.name, t.description, t.referencepoint, t.plotid FROM pmm_geometryinformation t left join pmm_plot on pmm_plot.id = t.plotid where pmm_plot.plotid ilike '" + id + "%' order by  ST_Perimeter(t.geometrytext) DESC) inputs) features; "));
                // execute the statement
                try
                {
                    using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                    {
                        var result = uow.ExecuteScalar(mvBuilder.ToString());
                        return (string)result;
                    }
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        // check permission 
        protected bool checkPermission(Tuple<string, string, string> LandingPage, User user)
        {
            var featurePermissionManager = new FeaturePermissionManager();
            var operationManager = new OperationManager();
            var userManager = new UserManager();

            try
            {
                var areaName = LandingPage.Item1;
                if (areaName == "")
                {
                    areaName = "shell";
                }
                var controllerName = LandingPage.Item2;
                var actionName = LandingPage.Item3;

  
                var operation = operationManager.Find(areaName, controllerName, "*");

                var feature = operation.Feature;
                if (feature == null) return true;

 

                if (featurePermissionManager.HasAccessAsync(user.Id, feature.Id).Result)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                featurePermissionManager.Dispose();
                operationManager.Dispose();
                userManager.Dispose();
            }
        }
    }






}
