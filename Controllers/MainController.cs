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
using BExIS.Modules.PMM.UI.Helper;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Modules.Pmm.UI.Controllers
{
    public class MainController : Controller
    {
        private BExIS.Pmm.Model.Plotchart helper;
        public MainController()
        {
            helper = new BExIS.Pmm.Model.Plotchart();
        }

        //temp here, delete soon
        public ActionResult ExploInfo()
        {
            return View("Temp_EPInformtion");
        }

        /// <summary>
        /// load plot view form normal users
        /// </summary>
        /// <returns>plot view</returns>
     /*   public ActionResult Index()
        {

            PlotChartViewModel plotviewmodel = new PlotChartViewModel();
            plotviewmodel.plotList = helper.GetPlots();
            return View(plotviewmodel);
        }*/

        /// <summary>
        /// load plots' list for a grid
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
        /// load subplot view for noram users
        /// </summary>
        /// <param name="plotid"></param>
        /// <returns>subplot view</returns>
        // GET: PlotChart
        public ActionResult SubPlots(long? plotid)//String plotid, int zoom)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Plot Charts", this.Session.GetTenant());

            var defaultPlotId = Helper.Settings.get("DefaultPlotId").ToString();
            


            PlotChartViewModel plotviewmodel = new PlotChartViewModel();
            //var plotList = helper.GetPlotsOld();
            plotviewmodel.grasslandPlotlist = helper.GetGrasslandPlots().ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter()).ToList();
            ViewData["DefaultPlotID"] = plotviewmodel.grasslandPlotlist.First().Id;
            plotviewmodel.forestPlotlist = helper.GetForestPlots().ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter()).ToList();

            //var plotListNew = helper.GetPlotsNew();
            plotviewmodel.foxPlotlist = helper.GetFoxPlots().ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter()).ToList();

            plotviewmodel.arablelandPlotlist = helper.GetArablelandPlots().ToList().OrderBy(x => x.PlotId, new BExIS.Modules.PMM.UI.Helper.NaturalSorter()).ToList();


            plotviewmodel.selectedPlot = null;
            var list_plotlist = plotviewmodel.grasslandPlotlist.ToList();
            if (plotid != null && list_plotlist.Count > 0 && list_plotlist.First(x => x.Id == plotid) != null)
                plotviewmodel.selectedPlot = plotid != null ? list_plotlist.First(x => x.Id == plotid) : list_plotlist.First();

            if (plotviewmodel.selectedPlot == null)
                //plotviewmodel.selectedPlot = plotviewmodel.grasslandPlotlist.Where(a => a.Id == Convert.ToInt64(defaultPlotId)).FirstOrDefault();
                plotviewmodel.selectedPlot = plotviewmodel.grasslandPlotlist.First();


            plotviewmodel.ImageSource = helper.ProducePlot(helper.GetPlot(plotviewmodel.selectedPlot.Id), 1, false);

            return View(plotviewmodel);
        }

        /// <summary>
        /// load plotchart infromation
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
        /// export a plot to PDF
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

        /*public ActionResult ImportInformation()
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
        }*/

        /// <summary>
        /// export plots to a CSV file
        /// </summary>
        /// <returns>CSV file</returns>
        public ActionResult ExportAllPlots()
        {
            return File(Encoding.ASCII.GetBytes(ImportExport.ExportAllPlots()), "text/csv", "AllPlotList_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
        }

        /// <summary>
        /// export all subplots to a CSV file
        /// </summary>
        /// <returns>CSV file</returns>
        public ActionResult ExportAllGeometries()
        {
            return File(Encoding.ASCII.GetBytes(ImportExport.ExportAllGeometries()), "text/csv", "AllSubplotList_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
        }

        /// <summary>
        /// export subplots of a plot to a CSV file
        /// </summary>
        /// <param name="id"></param>
        /// <returns>CSV file</returns>
        public ActionResult ExportPlot(long id)
        {
            Plotchart plotChart = new Plotchart();
            string plotid = plotChart.GetPlot(id).PlotId;
            return File(Encoding.ASCII.GetBytes(ImportExport.ExportPlotGeometries(id)), "text/csv", plotid + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
        }
    }
}