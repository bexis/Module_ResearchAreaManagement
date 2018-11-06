using BExIS.Pmm.Entities;
using BExIS.Pmm.Services;
using GeoAPI.Geometries;
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using BExIS.Dlm.Services;
using System.Text;

namespace BExIS.Pmm.Model
{
    public class ImportExport
    {
        public List<ImportPlotObject> PlotBatchImport(Stream inputFile)
        {
            Plotchart plotChart = new Plotchart();
            Plot output;
            List<ImportPlotObject> plotList = new List<ImportPlotObject>();

            using (var reader = new StreamReader(inputFile, Encoding.UTF8))
            {
                int index = 0;
                while (!reader.EndOfStream)
                {
                    
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    Plot plot = new Plot();
                    try
                    {
                        plot.PlotId = values[1];
                        plot.GeometryType = values[2];
                        plot.Coordinate = values[3];
                        plot.CoordinateType = values[4];
                        plot.Latitude = values[5];
                        plot.Longitude = values[6];
                        plot.Status = Convert.ToByte(values[7]);
                        plot.Id = values[0] == "" ? -1 : Convert.ToInt32(values[0]);
                    } catch
                    {
                        plot = null;
                    }
                     if (plot == null || plot.Id == -1)
                    {
                        if (plot != null && (plot.Status != 1 || plotChart.CheckDuplicatePlotName("create", plot.PlotId, 0) == "valid"))
                            output = plotChart.AddPlot(plot.Coordinate, plot.GeometryType, plot.CoordinateType, plot.PlotId, plot.Latitude, plot.Longitude);
                        else
                            output = null;
                        if (output == null)
                            plotList.Add(new ImportPlotObject(index, plot, "Insert", false));
                        else
                            plotList.Add(new ImportPlotObject(index, plot, "Insert", true));
                    }
                    else
                    {
                        if (plot.Status != 1 || plotChart.CheckDuplicatePlotName("update", plot.PlotId, plot.Id) == "valid")
                            output = plotChart.UpdatePlot(plot.Id, plot.Coordinate, plot.GeometryType, plot.CoordinateType, plot.PlotId, plot.Latitude, plot.Longitude);
                        else
                            output = null;
                        
                        if (output == null)
                            plotList.Add(new ImportPlotObject(index, plot, "Update", false));
                        else
                            plotList.Add(new ImportPlotObject(index, plot, "Update", true));
                    }
                    index++;
                }
            }
            return plotList;
        }

        public List<ImportGeometryObject> SubPlotBatchImport(Stream inputFile)
        {
            Plotchart plotChart = new Plotchart();
            GeometryInformation output;
            List<ImportGeometryObject> subPlotList = new List<ImportGeometryObject>();

            using (var reader = new StreamReader(inputFile, Encoding.UTF8))
            {
                int index = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    GeometryInformation geometry = new GeometryInformation();
                    geometry.Id = values[0] == "" ? -1 : Convert.ToInt32(values[0]);
                    geometry.Name = values[1];
                    geometry.GeometryType = values[2];
                    geometry.Coordinate = values[3];
                    geometry.CoordinateType = values[4];
                    geometry.LineWidth = Convert.ToByte(values[5]);
                    geometry.Color = values[6];
                    geometry.Description = values[7];
                    geometry.Status = Convert.ToByte(values[8]);
                    String plotId = values[9];////////////////////////////// some problem for plotid

                    if (geometry.Id == -1)
                    {
                        output = plotChart.AddGeometry(geometry.PlotId, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.Name, geometry.Description);
                        if (output == null)
                            subPlotList.Add(new ImportGeometryObject(index, geometry, "Insert", false));
                        else
                            subPlotList.Add(new ImportGeometryObject(index, geometry, "Insert", true));
                    }
                    else
                    {
                        output = plotChart.UpdateGeometry(geometry.Id, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.Name, geometry.Description);
                        if (output == null)
                            subPlotList.Add(new ImportGeometryObject(index, geometry, "Update", false));
                        else
                            subPlotList.Add(new ImportGeometryObject(index, geometry, "Update", true));
                    }
                    index++;
                }
            }
            return subPlotList;
        }

        public String ExportAllPlots()
        {
            String output = "";
            PlotManager pManager = new PlotManager();
            List<Plot> plotList = new List<Plot>();
            plotList = pManager.Repo.Get().ToList();
            foreach (var plot in plotList)
            {
                String Line = plot.Id + ";" + plot.PlotId + ";" + plot.GeometryType + ";" + plot.Coordinate + ";" + plot.CoordinateType + ";" + plot.Latitude + ";" + plot.Longitude + ";" + plot.Status + "\n";
                output += Line;
            }
            return output;
        }

        public String ExportAllGeometries()
        {
            String output = "";
            GeometryManager gManager = new GeometryManager();
            List<GeometryInformation> geometryList = new List<GeometryInformation>();
            geometryList = gManager.Repo.Get().ToList();
            foreach (var geometry in geometryList)
            {
                String line = geometry.Id + ";" + geometry.Name + ";" + geometry.GeometryType + ";" + geometry.Coordinate + ";" + geometry.CoordinateType + ";" + geometry.LineWidth + ";" + geometry.Color + ";" + geometry.Description + ";" + geometry.Status + ";" + geometry.Plot.Id + "\n";
                output += line;
            }
            return output;
        }

        public String ExportPlotGeometries(long id)
        {
            String output = "";
            Plotchart plotChart = new Plotchart();
            Plot plot = plotChart.GetPlot(id);
            foreach (var geometry in plot.Geometries)
            {
                String line = geometry.Id + ";" + geometry.Name + ";" + geometry.GeometryType + ";" + geometry.Coordinate + ";" + geometry.CoordinateType + ";" + geometry.LineWidth + ";" + geometry.Color + ";" + geometry.Description + ";" + geometry.Status + ";" + geometry.Plot.Id + "\n";
                output += line;
            }
            return output;
        }

    }
}