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

namespace BExIS.Modules.Pmm.UI.Controllers
{
    public class MainAdminController : Controller
    {
        private BExIS.Pmm.Model.Plotchart helper;
        public MainAdminController()
        {
            helper = new BExIS.Pmm.Model.Plotchart();
        }

        public ActionResult Index()
        {

            PlotChartViewModel plotviewmodel = new PlotChartViewModel();
            plotviewmodel.plotList = helper.GetPlots();
            plotviewmodel.isAdmin = true;
            plotviewmodel.allPlots = "," + String.Join(",", plotviewmodel.plotList.Select(x => x.Id.ToString()).ToArray());
            return View(plotviewmodel);
        }

        [HttpPost]
        public ActionResult _newPlot(string coordinate, string geometrytype, string coordinatetype, string name, String latitude, String longitude, string refrencePoint = "")
        {
            String message = "valid";
            message = helper.CheckDuplicatePlotName("create", name, 0);
            if (message != "valid")
                return Json(message);
            Plot result = helper.AddPlot(coordinate, geometrytype, coordinatetype, name, latitude, longitude, refrencePoint);
            return Json(message);
        }


        [HttpPost]
        public ActionResult _updatePlot(int plotid, string coordinate, string geometrytype, string coordinatetype, string name, String latitude, String longitude, string refrencePoint = "")
        {
            String message = "valid";
            message = helper.CheckDuplicatePlotName("update", name, plotid);
            if (message != "valid")
                return Json(message);
            Plot result = helper.UpdatePlot(plotid, coordinate, geometrytype, coordinatetype, name, latitude, longitude, refrencePoint);
            return Json(message);
        }

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

        // GET: PlotChart
        public ActionResult SubPlots(long? plotid)//String plotid, int zoom)
        {
            PlotChartViewModel plotviewmodel = new PlotChartViewModel();
            plotviewmodel.plotList = helper.GetPlots();
            plotviewmodel.selectedPlot = null;
            if (plotid != null && plotviewmodel.plotList.Count > 0 && plotviewmodel.plotList.First(x => x.Id == plotid) != null)
                plotviewmodel.selectedPlot = plotid != null ? plotviewmodel.plotList.First(x => x.Id == plotid) : plotviewmodel.plotList.First();
            plotviewmodel.ImageSource = helper.ProducePlot( plotviewmodel.selectedPlot , 1, false);
            

            return View(plotviewmodel);
        }

        public ActionResult _getPlotChart(long id, Boolean deactivePlot = false, Boolean beyondPlot = false, int gridSize = 5)
        {
            return Json(helper.ProducePlot(helper.GetPlot(id), 1, deactivePlot, beyondPlot, gridSize));
        }

        public ActionResult GetPDF(long id, Boolean deactivePlot = false, Boolean beyondPlot = false, int gridSize = 5)
        {
            List<Plot> plotList = new List<Plot>();
            plotList.Add(helper.GetPlot(id));
            return File(helper.generatePDF(plotList, 1, deactivePlot, beyondPlot, gridSize), "application/pdf", "filename.pdf");
        }

        public ActionResult GetPDFBatch(String ids, Boolean deactivePlot = false, Boolean beyondPlot = false, int gridSize = 5, Boolean legend = false)
        {
            List<Plot> plotList = new List<Plot>();
            String[] idList = ids.Split(',');
            foreach(String id in idList) {
                if (id.Equals(""))
                    continue;
                plotList.Add(helper.GetPlot(Convert.ToInt64(id)));
            }
            return File(helper.generatePDF(plotList, 1, deactivePlot, beyondPlot, gridSize, legend), "application/pdf", "filename.pdf");
        }

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

        [HttpPost]
        public ActionResult _newGeometry(long plotid,string coordinate, string geometrytype, string coordinatetype, string color, string name, string description, string refrencePoint = "")
        {
            GeometryInformation result = helper.AddGeometry(plotid, coordinate, geometrytype, coordinatetype, color, name, description, refrencePoint);
            return Json(result != null);
        }


        [HttpPost]
        public ActionResult _updateGeometry(string plotid, string coordinate, string geometrytype, string coordinatetype, string color, long geometryId, string name, string description, string refrencePoint = "")
        {
            GeometryInformation result = helper.UpdateGeometry(geometryId, coordinate, geometrytype, coordinatetype, color, name, description, refrencePoint);
            return Json(result != null);
        }

        [HttpPost]
        public ActionResult _archiveGeometry(long geometryid)
        {
            helper.ArchiveGeometry(geometryid);
            return Json(true);
        }

        [HttpPost]
        public ActionResult _deleteGeometry(long geometryid)
        {
            helper.DeleteGeometry(geometryid);
            return Json(true);
        }

        public ActionResult ImportInformation()
        {
            return View();
        }

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
                    if (type == "Plot")
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

            }

            // Redirect to a view showing the result of the form submission.
            return View();
        }

        public ActionResult ExportAllPlots()
        {
            ImportExport importExport = new ImportExport();
            //byte[] csvData = importExport.ExportAllPlots();
            return File(Encoding.ASCII.GetBytes(importExport.ExportAllPlots()), "text/csv", "PlotList.csv");
        }

        public ActionResult ExportAllGeometries()
        {
            ImportExport importExport = new ImportExport();
            //byte[] csvData = importExport.ExportAllPlots();
            return File(Encoding.ASCII.GetBytes(importExport.ExportAllGeometries()), "text/csv", "SubplotList.csv");
        }

        public ActionResult ExportPlot(long id)
        {
            ImportExport importExport = new ImportExport();
            return File(Encoding.ASCII.GetBytes(importExport.ExportPlotGeometries(id)), "text/csv", "SubplotList.csv");
        }
    }
}