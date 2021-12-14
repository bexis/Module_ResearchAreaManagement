using BExIS.Web.Shell.Areas.PMM.Models;
using BExIS.Pmm.Entities;
using BExIS.Pmm.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using System.IO;
using System.Text;
using BExIS.Pmm.Services;
using BExIS.Security.Services.Utilities;
using Vaiona.Utils.Cfg;
using BExIS.Security.Services.Subjects;

namespace BExIS.Modules.Pmm.UI.Controllers
{
    public class MainAdminController : Controller
    {
        private BExIS.Pmm.Model.Plotchart helper;
        public MainAdminController()
        {
            helper = new BExIS.Pmm.Model.Plotchart();
        }

        /// <summary>
        /// Load plot admin view
        /// </summary>
        /// <returns>plot admin view</returns>
        public ActionResult Index()
        {

            PlotChartViewModel plotviewmodel = new PlotChartViewModel();
            var plotList = helper.GetPlots();
            plotviewmodel.plotlist = plotList.ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter());
            plotviewmodel.isAdmin = true;
            plotviewmodel.allPlots = "," + String.Join(",", plotviewmodel.plotlist.Select(x => x.Id.ToString()).ToArray());
            return View(plotviewmodel);
        }

        /// <summary>
        /// add new plot
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="geometrytype"></param>
        /// <param name="coordinatetype"></param>
        /// <param name="name"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="referencePoint"></param>
        /// <returns>if the action was successful or not</returns>
        [HttpPost]
        public ActionResult _newPlot(string coordinate, string geometrytype, string coordinatetype, string name, String latitude, String longitude, string referencePoint = "")
        {
            String message = "valid";
            message = helper.CheckDuplicatePlotName("create", name, 0);
            if (message != "valid")
                return Json(message);
            Plot result = helper.AddPlot(coordinate, geometrytype, coordinatetype, name, latitude, longitude, referencePoint);
            return Json(message);
        }

        /// <summary>
        /// update information of a existing plot
        /// </summary>
        /// <param name="plotid"></param>
        /// <param name="coordinate"></param>
        /// <param name="geometrytype"></param>
        /// <param name="coordinatetype"></param>
        /// <param name="name"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="referencePoint"></param>
        /// <returns>if the action was successful or not</returns>
        [HttpPost]
        public ActionResult _updatePlot(int plotid, string coordinate, string geometrytype, string coordinatetype, string name, String latitude, String longitude, string referencePoint = "")
        {
            String message = "valid";
            message = helper.CheckDuplicatePlotName("update", name, plotid);
            if (message != "valid")
                return Json(message);
            Plot result = helper.UpdatePlot(plotid, coordinate, geometrytype, coordinatetype, name, latitude, longitude, referencePoint);
            return Json(message);
        }

        /// <summary>
        /// archive or unarchive a plot
        /// </summary>
        /// <param name="plotid"></param>
        /// <param name="name"></param>
        /// <returns>if the action was successful or not</returns>
        [HttpPost]
        public ActionResult _archivePlot(long plotid, string name)
        {
            String message = "valid";
            message = helper.CheckDuplicatePlotName("update", name, plotid);
            if (message != "valid")
                return Json(message);

            helper.ArchivePlot(plotid);
            return Json(message);
        }

        /// <summary>
        /// delete a plot logically(not physically, it only changes the status)
        /// </summary>
        /// <param name="plotid"></param>
        /// <param name="name"></param>
        /// <returns>if the action was successful or not</returns>
        [HttpPost]
        public ActionResult _deletePlot(long plotid, string name)
        {
            String message = "valid";
            message = helper.CheckDuplicatePlotName("update", name, plotid);
            if (message != "valid")
                return Json(message);

            helper.DeletePlot(plotid);
            return Json(message);
        }

        /// <summary>
        /// grid action to load plots' list
        /// </summary>
        /// <returns>grid model</returns>
        [GridAction]
        public ActionResult _AjaxBindingPlots()
        {
            var plots = helper.GetPlots().OrderBy(x => x.Id);
            foreach (var plot in plots.ToList())
            {
                foreach (var geom in plot.Geometries)
                {
                    geom.Plot = null;
                    geom.Geometry = null;
                }
                //if(plot.Area != null)
                //plot.Area.Plot = null;
                plot.Geometry = null;
            }
            return View(new GridModel<Plot> { Data = plots.ToList() });
        }

        /// <summary>
        /// load subplot image view
        /// </summary>
        /// <param name="plotid"></param>
        /// <returns>subplotview</returns>
        // GET: PlotChart
        public ActionResult LoadPlotchartImage(long? plotid)//String plotid, int zoom)
        {
            PlotChartViewModel plotviewmodel = new PlotChartViewModel();
            var defaultPlotId = Helper.Settings.get("DefaultPlotId").ToString();
            ViewData["DefaultPlotID"] = defaultPlotId;

            var plotList = helper.GetPlotsOld();
            plotviewmodel.plotlist = plotList.ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter());

            var plotListNew = helper.GetPlotsNew();
            plotviewmodel.plotlistNew = plotListNew.ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter());

            if (plotviewmodel.selectedPlot == null)
                plotviewmodel.selectedPlot = plotList.Where(a => a.Id == Convert.ToInt64(defaultPlotId)).FirstOrDefault();

            plotviewmodel.ImageSource = helper.ProducePlot(helper.GetPlot(plotviewmodel.selectedPlot.Id), 1, false);

            ViewBag.Title = "BExIS - Plot Maps";

            return View("PlotchartImageView", plotviewmodel);

        }

        /// <summary>
        /// load subplot view
        /// </summary>
        /// <param name="plotid"></param>
        /// <returns>subplotview</returns>
        // GET: PlotChart
        public ActionResult SubPlots(long? plotid)//String plotid, int zoom)
        {
            var defaultPlotId = Helper.Settings.get("DefaultPlotId").ToString();
            ViewData["DefaultPlotID"] = defaultPlotId;

            PlotChartViewModel plotviewmodel = new PlotChartViewModel();
            var plotList = helper.GetPlotsOld();
            plotviewmodel.plotlist = plotList.ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter());

            var plotListNew = helper.GetPlotsNew();
            plotviewmodel.plotlistNew = plotListNew.ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter());

            plotviewmodel.selectedPlot = null;
            var list_plotlist = plotviewmodel.plotlist.ToList();
            if (plotid != null && list_plotlist.Count > 0 && list_plotlist.First(x => x.Id == plotid) != null)
                plotviewmodel.selectedPlot = plotid != null ? list_plotlist.First(x => x.Id == plotid) : list_plotlist.First();


            if (plotviewmodel.selectedPlot == null)
                plotviewmodel.selectedPlot = plotList.Where(a => a.Id == Convert.ToInt64(defaultPlotId)).FirstOrDefault();

            plotviewmodel.ImageSource = helper.ProducePlot(helper.GetPlot(plotviewmodel.selectedPlot.Id), 1, false);

            ViewBag.Title = "BExIS - Plot Maps";

            return View(plotviewmodel);
        }

        /// <summary>
        /// get subplots of a plot
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deactivePlot">load deactive subplots</param>
        /// <param name="beyondPlot">load subplots which are outside of the plot border</param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public ActionResult _getPlotChart(long id, Boolean deactivePlot = false, Boolean beyondPlot = false, int gridSize = 5)
        {
            return Json(helper.ProducePlot(helper.GetPlot(id), 1, deactivePlot, beyondPlot, gridSize));
        }

        /// <summary>
        /// Export a plot to PDF
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deactivePlot">load deactive subplots</param>
        /// <param name="beyondPlot">load subplots which are outside of the plot border</param>
        /// <param name="gridSize"></param>
        /// <returns>PDF file</returns>
        public ActionResult GetPDF(long id, Boolean deactivePlot = false, Boolean beyondPlot = false, int gridSize = 5, Boolean legend = false)
        {
            List<Plot> plotList = new List<Plot>();
            plotList.Add(helper.GetPlot(id));
            DateTime date = DateTime.Now;
            legend = !legend;

            return File(helper.generatePDF(plotList, 1, deactivePlot, beyondPlot, gridSize, legend), "application/pdf", plotList[0].PlotId + "_" + date.ToString("dd_mm_yyyy") + ".pdf");
        }

        /// <summary>
        /// Export several plots to PDF
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="deactivePlot">load deactive subplots</param>
        /// <param name="beyondPlot">load subplots which are outside of the plot border</param>
        /// <param name="gridSize"></param>
        /// <param name="legend"></param>
        /// <returns>PDF file</returns>
        public ActionResult GetPDFBatch(String ids, Boolean deactivePlot = false, Boolean beyondPlot = false, int gridSize = 5, Boolean legend = false)
        {
            List<Plot> plotList = new List<Plot>();
            String[] idList = ids.Split(',');
            foreach(String id in idList) {
                if (id.Equals(""))
                    continue;
                plotList.Add(helper.GetPlot(Convert.ToInt64(id)));
            }
            legend = !legend;
            return File(helper.generatePDF(plotList, 1, deactivePlot, beyondPlot, gridSize, legend), "application/pdf", "filename.pdf");
        }

        /// <summary>
        /// load subplots' list for a grid
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deactivePlot">load deactive subplots</param>
        /// <param name="beyondPlot">load subplots which are outside of the plot border</param>
        /// <returns>grid model</returns>
        [GridAction]
        public virtual ActionResult _AjaxBinding(long id, Boolean deactivePlot, Boolean beyondPlot)
        {
            Plot plot = helper.GetPlot(id, deactivePlot, beyondPlot);
            plot.Geometries = plot.Geometries.OrderBy(x => x.Id).ToList();
            foreach (var geom in plot.Geometries)
            {
                geom.PlotId = geom.Plot.Id;
                geom.Plot = null;
                geom.Geometry = null;
            }
            return View(new GridModel<GeometryInformation> { Data = plot.Geometries.ToList() });
        }

        /// <summary>
        /// add new subplot
        /// </summary>
        /// <param name="plotid"></param>
        /// <param name="coordinate"></param>
        /// <param name="geometrytype"></param>
        /// <param name="coordinatetype"></param>
        /// <param name="color"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="referencePoint"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _newGeometry(long plotid,string coordinate, string geometrytype, string coordinatetype,int linewidth, string color, string name, string description, string referencePoint = "")
        {
            GeometryInformation result = helper.AddGeometry(plotid, coordinate, geometrytype, coordinatetype,linewidth, color, name, description, referencePoint);
            return Json(result != null);
        }

        /// <summary>
        /// update a exisiting subplot information
        /// </summary>
        /// <param name="plotid"></param>
        /// <param name="coordinate"></param>
        /// <param name="geometrytype"></param>
        /// <param name="coordinatetype"></param>
        /// <param name="color"></param>
        /// <param name="geometryId"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="referencePoint"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _updateGeometry(string plotid, string coordinate, string geometrytype, string coordinatetype, int lineWidth, string color, long geometryId, string name, string description, string referencePoint = "")
        {
            GeometryInformation result = helper.UpdateGeometry(geometryId, coordinate, geometrytype, coordinatetype, lineWidth, color, name, description, DateTime.Now, referencePoint);
            return Json(result != null);
        }

        /// <summary>
        /// change status of a existing subplot to archive
        /// </summary>8/////////////////////////////////////////////////////////////////////////////
        /// <param name="geometryid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _archiveGeometry(long geometryid)
        {
            helper.ArchiveGeometry(geometryid);
            return Json(true);
        }

        /// <summary>
        /// change status of a existing subplot to delete
        /// </summary>
        /// <param name="geometryid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _deleteGeometry(long geometryid)
        {
            bool delete = helper.DeleteGeometry(geometryid);
            return Json(delete);
        }

        /// <summary>
        /// load the view to import plot and subplots
        /// </summary>
        /// <returns></returns>
        public ActionResult ImportInformation()
        {
            return View();
        }

        /// <summary>
        /// import the data from a file to the system
        /// </summary>
        /// <param name="attachments"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ImportInformation(IEnumerable<HttpPostedFileBase> attachments, String type)
        {
            // The Name of the Upload component is "attachments"                            
            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    // Some browsers send file names with full path. We only care about the file name.
                    var fileName = Path.GetFileName(file.FileName);
                    var destinationPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                    file.SaveAs(destinationPath);
                    ImportExport importExport = new ImportExport();
                    List<ImportPlotObject> plotList = null;
                    List<ImportGeometryObject> subPlotList = null;
                    if (type == "plots")
                    {
                        plotList = importExport.PlotBatchImport(file.InputStream);
                        ViewData["Plot"] = plotList;
                    }
                    else
                    {
                        subPlotList = importExport.SubPlotBatchImport(file.InputStream);
                        ViewData["SubPlot"] = subPlotList;
                    }

                    //new StreamReader(inputFile.InputStream)
                }

                //send report Mail 
                //cearte report file
             
                string uploadType = "";
                var lines = new List<string>();

                // Create a file to write import report.

                if (ViewData["Plot"] != null)
                {
                    uploadType = "plots";
                    lines.Add("Index" + "|" + "Internal PlotId" + "|" + "PlotId" + "|" + "GeometryType" + "|" + "Coordinate" + "|" + "CoordinateType" + "|" + "Latitude" + "|" + "Longitude" + "|" + "Status");

                    List<ImportPlotObject> list = (List<ImportPlotObject>)ViewData["Plot"];
                     foreach ( var i in list)
                     {
                        lines.Add(i.Index + "|" + i.Plot.Id + "|" + i.Plot.PlotId + "|" + i.Plot.GeometryType + "|" + i.Plot.Coordinate + "|" + i.Plot.CoordinateType + "|" + i.Plot.Latitude + "|" + i.Plot.Longitude + "|" + i.UploadSuccessful);  
                     }
                          
                           
                }

                if (ViewData["SubPlot"] != null)
                {
                     uploadType = "subplots";
                     List<ImportGeometryObject> list = (List<ImportGeometryObject>)ViewData["SubPlot"];
                     lines.Add("Index" + "|" + "Action" + "|" + "GeometryId" + "|" + "GeometryName" + "|" + "GeometryType" + "|" + "Coordinate" + "|" + "CoordinateType" + "|" + "LineWidth" + "|" + "Color" + "|" + "Description" + "|" + "Status" + "|" + "PlotId" + "|" + "UploadSuccessful"); 
                     foreach (var i in list)
                     {
                         lines.Add(i.Index + "|" + i.Action + "|" + i.Geometry.Id + "|" + i.Geometry.Name + "|" + i.Geometry.GeometryType + "|" + i.Geometry.Coordinate + "|" + i.Geometry.CoordinateType + "|" + i.Geometry.LineWidth + "|" + i.Geometry.Color + "|" + i.Geometry.Description + "|" + i.Geometry.Status + "|" + i.Geometry.PlotId + "|" + i.UploadSuccessful);
                     }
                }

                FileInfo reportFile = null;
                try
                {
                    string dataPath = AppConfiguration.DataPath;
                    string storePath = Path.Combine(dataPath, "PMM", "Temp", "ImportReport_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");

                    System.IO.File.WriteAllLines(storePath, lines);
                    reportFile = new FileInfo(storePath);
                    List<FileInfo> files = new List<FileInfo>();
                    files.Add(reportFile);


                    //send mail
                    string userMail;
                    using (UserManager userManager = new UserManager())
                    {
                        userMail = userManager.FindByNameAsync(HttpContext.User.Identity.Name).Result.Email;
                    }
                    var es = new EmailService();
                    string text = "Import report as attachment.";
                    List<string> receiver = new List<string> { "bexis-sys@listserv.uni-jena.de", userMail };
                    es.Send("Plotchart " + uploadType + " import report", text, receiver, null, null, null, files);
                    files.Clear();
                    
                    
                }
                catch(Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());

                }
            }

            // Redirect to a view showing the result of the form submission.
            return View();
        }

        /// <summary>
        /// export plots to a CSV file
        /// </summary>
        /// <returns>CSV file</returns>
        public ActionResult ExportAllPlots(bool withSubplots = false)
        {
            ImportExport importExport = new ImportExport();
            return File(Encoding.ASCII.GetBytes(importExport.ExportAllPlots()), "text/csv", "AllPlotList_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
           
        }

        /// <summary>
        /// export all subplots to a CSV file
        /// </summary>
        /// <returns>CSV file</returns>
        public ActionResult ExportAllGeometries()
        {
            ImportExport importExport = new ImportExport();
            return File(Encoding.ASCII.GetBytes(importExport.ExportAllGeometries()), "text/csv", "AllSubplotList_"+ DateTime.Now.ToString("yyyy-MM-dd") +".csv");
        }

        /// <summary>
        /// export subplots of a plot to a CSV file
        /// </summary>
        /// <param name="id"></param>
        /// <returns>CSV file</returns>
        public ActionResult ExportPlot(long id)
        {
            ImportExport importExport = new ImportExport();
            Plotchart plotChart = new Plotchart();
            string plotid = plotChart.GetPlot(id).PlotId;
            return File(Encoding.ASCII.GetBytes(importExport.ExportPlotGeometries(id)), "text/csv", plotid + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
        }
    }
}