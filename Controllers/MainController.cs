﻿using System.Web.Mvc;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.IO;
using BExIS.IO.Transform.Output;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.PMM.Models;
using Telerik.Web.Mvc;
using BExIS.Pmm.Entities;
using System.Web;
using BExIS.Pmm.Model;
using System.Text;

namespace BExIS.Modules.Pmm.UI.Controllers
{
    public class MainController : Controller
    {
        private BExIS.Pmm.Model.Plotchart helper;
        public MainController()
        {
            helper = new BExIS.Pmm.Model.Plotchart();
        }

        public ActionResult Index()
        {

            PlotChartViewModel plotviewmodel = new PlotChartViewModel();
            plotviewmodel.plotList = helper.GetPlots();
            return View(plotviewmodel);
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
            plotviewmodel.ImageSource = helper.ProducePlot(plotviewmodel.selectedPlot, 1, false);


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