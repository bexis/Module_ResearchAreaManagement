using BExIS.Dlm.Services;
using BExIS.Pmm.Entities;
using BExIS.Pmm.Services;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BExIS.Pmm.Model
{
    public class Plotchart
    {
        /// <summary>
        /// add a new plot
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="geometrytype"></param>
        /// <param name="coordinatetype"></param>
        /// <param name="name"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="referencePoint"></param>
        /// <returns>new plot</returns>
        public Plot AddPlot(string coordinate, string geometrytype, string coordinatetype, string name, String latitude, String longitude, string referencePoint = "")
        {
            if (!checkGeometry(geometrytype, coordinatetype, coordinate))
                return null;

            double[] bb = { Convert.ToDouble(longitude), Convert.ToDouble(latitude) };
            String geometryText = calCoordd(geometrytype, coordinate, bb, coordinatetype, referencePoint);

            using (PlotManager pManager = new PlotManager())
            using (PlotHistoryManager pHManager = new PlotHistoryManager())
            {
                Plot plot = pManager.Create(name, "", latitude, longitude, new List<GeometryInformation>(), coordinate, coordinatetype, geometrytype, geometryText);
                pHManager.Create(plot.PlotId, plot.PlotType, plot.Latitude, plot.Longitude, plot.Coordinate, plot.CoordinateType, plot.GeometryType, plot.GeometryText, plot.Id, "Create", DateTime.Now);

                return plot;
            }
        }

        /// <summary>
        /// update a existing plot
        /// </summary>
        /// <param name="plotid"></param>
        /// <param name="coordinate"></param>
        /// <param name="geometrytype"></param>
        /// <param name="coordinatetype"></param>
        /// <param name="name"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="referencePoint"></param>
        /// <returns>updated plot</returns>
        public Plot UpdatePlot(long plotid, string coordinate, string geometrytype, string coordinatetype, string name, String latitude, String longitude, string referencePoint = "")
        {
            using (PlotManager pManager = new PlotManager())
            using (PlotHistoryManager pHManager = new PlotHistoryManager())
            using (GeometryManager gManager = new GeometryManager())
            {
                Plot plot = pManager.Repo.Get(x => x.Id == plotid).First();
                bool locationChanged = false;

                if (!checkGeometry(geometrytype, coordinatetype, coordinate))
                    return null;

                double[] bb = { Convert.ToDouble(longitude), Convert.ToDouble(latitude) };
                String geometryText = calCoordd(geometrytype, coordinate, bb, coordinatetype, referencePoint);

                if (plot.Latitude != latitude || plot.Longitude != longitude)
                    locationChanged = true;

                plot.Coordinate = coordinate;
                plot.CoordinateType = coordinatetype;
                plot.GeometryText = geometryText;
                plot.GeometryType = geometrytype;
                plot.Latitude = latitude;
                plot.Longitude = longitude;
                plot.PlotId = name;
                plot.PlotType = "";

                Plot Plot = pManager.Update(plot);
                if (locationChanged)
                {
                    foreach (var geom in Plot.Geometries)
                    {
                        geom.GeometryText = calCoordd(geom.GeometryType, geom.Coordinate, bb, geom.CoordinateType, geom.ReferencePoint);
                        gManager.Update(geom);
                    }
                }

                pHManager.Create(plot.PlotId, plot.PlotType, plot.Latitude, plot.Longitude, plot.Coordinate, plot.CoordinateType, plot.GeometryType, plot.GeometryText, plot.Id, "Update", DateTime.Now);

                return plot;
            }
        }

        /// <summary>
        /// change the status of a plot to delete
        /// </summary>
        /// <param name="plotid"></param>
        /// <returns>updated plot</returns>
        public Plot DeletePlot(long plotid)
        {
            using (PlotManager pManager = new PlotManager())
            using (PlotHistoryManager pHManager = new PlotHistoryManager())
            {
                Plot plot = pManager.Repo.Get(x => x.Id == plotid).First();
                plot.Status = (byte)(plot.Status == 3 ? 1 : 3);
                Plot Plot = pManager.Update(plot);

                string action = "";
                if (plot.Status == 1) action = "Undelete";
                if (plot.Status == 3) action = "Delete";
                pHManager.Create(plot.PlotId, plot.PlotType, plot.Latitude, plot.Longitude, plot.Coordinate, plot.CoordinateType, plot.GeometryType, plot.GeometryText, plot.Id, action, DateTime.Now);

                return plot;
            }
        }

        /// <summary>
        /// change the status of a plot to archived
        /// </summary>
        /// <param name="plotid"></param>
        /// <returns>updated plot</returns>
        public Plot ArchivePlot(long plotid)
        {
            using (PlotManager pManager = new PlotManager())
            using (PlotHistoryManager pHManager = new PlotHistoryManager())
            {
                Plot plot = pManager.Repo.Get(x => x.Id == plotid).First();
                plot.Status = (byte)(plot.Status == 2 ? 1 : 2);
                Plot Plot = pManager.Update(plot);

                string action = "";
                if (plot.Status == 1) action = "Unarchive";
                if (plot.Status == 2) action = "Archive";
                pHManager.Create(plot.PlotId, plot.PlotType, plot.Latitude, plot.Longitude, plot.Coordinate, plot.CoordinateType, plot.GeometryType, plot.GeometryText, plot.Id, action, DateTime.Now);

                return plot;
            }
        }

        /// <summary>
        /// check plot name for duplicatation
        /// </summary>
        /// <param name="action"></param>
        /// <param name="name"></param>
        /// <param name="plotId"></param>
        /// <returns>Message for validity of the name</returns>
        public String CheckDuplicatePlotName(String action, String name, long plotId)
        {
            String message = "valid";
            using (PlotManager pManager = new PlotManager())
            {
                if (action == "create")
                {
                    if (pManager.Repo.Get(x => x.PlotId == name && x.Status == 1).Count() != 0)
                        message = "Active Plot with a same name already exist";
                }
                else if (action == "update")
                    if (pManager.Repo.Get(x => x.PlotId == name && x.Status == 1 && x.Id != plotId).Count() != 0)
                        message = "Active Plot with a same name already exist";

                return message;
            }
        }

        /// <summary>
        /// add a new subplot
        /// </summary>
        /// <param name="plotid"></param>
        /// <param name="coordinate"></param>
        /// <param name="geometrytype"></param>
        /// <param name="coordinatetype"></param>
        /// <param name="color"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="referencePoint"></param>
        /// <returns>new subplot</returns>
        public GeometryInformation AddGeometry(long plotid, string coordinate, string geometrytype, string coordinatetype, string color, string name, string description, string referencePoint = "")
        {
            Plot plot = GetPlot(plotid);

            if (!checkGeometry(geometrytype, coordinatetype, coordinate))
                return null;

            double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };
            String geometryText = calCoordd(geometrytype, coordinate, bb, coordinatetype, referencePoint);

            using (GeometryManager gManager = new GeometryManager())
            using (GeometryHistoryManager gHManager = new GeometryHistoryManager())
            {
                GeometryInformation geometry = gManager.Create(plot.Id, coordinate, geometrytype, coordinatetype, color, geometryText, plot, name, description);
                gHManager.Create(geometry.PlotId, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.GeometryText, geometry.Name, geometry.Description, geometry.Id, "Create", DateTime.Now);

                return geometry;
            }
        }

        /// <summary>
        /// update a existing subplot
        /// </summary>
        /// <param name="geometryId"></param>
        /// <param name="coordinate"></param>
        /// <param name="geometrytype"></param>
        /// <param name="coordinatetype"></param>
        /// <param name="color"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="referencePoint"></param>
        /// <returns>updated subplot</returns>
        public GeometryInformation UpdateGeometry(long geometryId, string coordinate, string geometrytype, string coordinatetype, string color, string name, string description, string referencePoint = "")
        {
            using (GeometryManager gManager = new GeometryManager())
            using (GeometryHistoryManager gHManager = new GeometryHistoryManager())
            {
                GeometryInformation geom = gManager.Repo.Get(x => x.Id == geometryId).First();

                if (!checkGeometry(geometrytype, coordinatetype, coordinate))
                    return null;

                double[] bb = { Convert.ToDouble(geom.Plot.Longitude), Convert.ToDouble(geom.Plot.Latitude) };
                String geometryText = calCoordd(geometrytype, coordinate, bb, coordinatetype, referencePoint);

                geom.GeometryText = geometryText;
                geom.Coordinate = coordinate;
                geom.GeometryType = geometrytype;
                geom.CoordinateType = coordinatetype;
                geom.Color = color;
                geom.Name = name;
                geom.Description = description;

                GeometryInformation geometry = gManager.Update(geom);
                gHManager.Create(geometry.PlotId, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.GeometryText, geometry.Name, geometry.Description, geometry.Id, "Update", DateTime.Now);

                return geometry;
            }
        }

        /// <summary>
        /// change the status of a subplot to archived
        /// </summary>
        /// <param name="geometryId"></param>
        /// <returns>updated subplot</returns>
        public GeometryInformation ArchiveGeometry(long geometryId)
        {
            using (GeometryManager gManager = new GeometryManager())
            using (GeometryHistoryManager gHManager = new GeometryHistoryManager())
            {
                GeometryInformation geom = gManager.Repo.Get(x => x.Id == geometryId).First();
                geom.Status = (byte)(geom.Status == 2 ? 1 : 2);

                GeometryInformation geometry = gManager.Update(geom);

                string action = "";
                if (geom.Status == 1) action = "Unarchive";
                if (geom.Status == 2) action = "Archive";
                gHManager.Create(geometry.PlotId, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.GeometryText, geometry.Name, geometry.Description, geometry.Id, action, DateTime.Now);

                return geometry;
            }
        }

        /// <summary>
        /// change the status of a subplot to deleted
        /// </summary>
        /// <param name="geometryId"></param>
        /// <returns>updated subplot</returns>
        public bool DeleteGeometry(long geometryId)
        {
            using (GeometryManager gManager = new GeometryManager())
            using (GeometryHistoryManager gHManager = new GeometryHistoryManager())
            {
                GeometryInformation geom = gManager.Repo.Get(x => x.Id == geometryId).First();
                geom.Status = (byte)(geom.Status == 3 ? 1 : 3);

                bool delete = gManager.Delete(geom);

                string action = "";
                if (geom.Status == 1) action = "Undelete";
                if (geom.Status == 3) action = "Delete";
                gHManager.Create(geom.PlotId, geom.Coordinate, geom.GeometryType, geom.CoordinateType, geom.Color, geom.GeometryText, geom.Name, geom.Description, geom.Id, action, DateTime.Now);

                return delete;
            }
        }

        /// <summary>
        /// get list of plots
        /// </summary>
        /// <returns>plots list</returns>
        public IList<Plot> GetPlots()
        {
            using (PlotManager pcManager = new PlotManager())
            {
                IList<Plot> plots = pcManager.Repo.Get();

                return plots;
            }
        }


        /// <summary>
        /// get list of plots
        /// </summary>
        /// <returns>plots list</returns>
        public IList<Plot> GetPlotsOld()
        {
            using (PlotManager pcManager = new PlotManager())
            {
                IList<Plot> plots = pcManager.Repo.Query().Where(x => !x.PlotId.Contains("-")).ToList(); // get all plot names without a "-" -> "old" plots

                return plots;
            }
        }

        /// <summary>
        /// get list of plots
        /// </summary>
        /// <returns>plots list</returns>
        public IList<Plot> GetPlotsNew()
        {
            using (PlotManager pcManager = new PlotManager())
            {
                IList<Plot> plots = pcManager.Repo.Query().Where(x => x.PlotId.Contains("-")).ToList(); // get all plot names with a "-" -> new experiments
                return plots;
            }
        }

        /// <summary>
        /// get a plot by the plot name
        /// </summary>
        /// <param name="plotid"></param>
        /// <returns>plot</returns>
        public Plot GetPlot(string plotid)
        {
            using (PlotManager pcManager = new PlotManager())
            {
                Plot plot = pcManager.Repo.Get(x => x.PlotId == plotid).ElementAtOrDefault(0);
                if (plot != null)
                {
                    pcManager.Repo.LoadIfNot(plot.Geometries); // Load Geoemetries
                }
                return plot;
            }
        }

        /// <summary>
        /// get a plot by plot id
        /// </summary>
        /// <param name="plotid"></param>
        /// <returns>plot</returns>
        public Plot GetPlot(long plotid)
        {
            Plot returnPlot = new Plot(); // workaround helping var to force geometries are loaded
            using (PlotManager pcManager = new PlotManager())
            {
                Plot plot = pcManager.Repo.Get(x => x.Id == plotid).ElementAtOrDefault(0);
                if (plot != null)
                {
                    pcManager.Repo.LoadIfNot(plot.Geometries); // Load Geoemetries
                }

                // Workaround to force Geoemtries are loaded
                returnPlot = plot;
                if (plot.Geometries != null && plot.Geometries.Count() >0)
                    returnPlot.Geometries = plot.Geometries;

                return returnPlot;
            }
        }

        /// <summary>
        /// get a plot
        /// </summary>
        /// <param name="plotid"></param>
        /// <param name="deactivePlot">load deactive subplots</param>
        /// <param name="beyondPlot">load subplots which are outside of the plot border</param>
        /// <returns></returns>
        public Plot GetPlot(long plotid, Boolean deactivePlot, Boolean beyondPlot)
        {
            using (PlotManager pcManager = new PlotManager())
            {
                Plot plot = pcManager.Repo.Get(x => x.Id == plotid).ElementAtOrDefault(0);
                if (plot == null)
                {
                    plot = new Plot();
                    //plot.Geometries = new List<GeometryInformation>();
                    //if (plot != null)
                    //{
                    //    pcManager.Repo.LoadIfNot(plot.Geometries); // Load Geoemetries
                    //}
                    return plot;
                }
                if (plot != null)
                {
                    pcManager.Repo.LoadIfNot(plot.Geometries); // Load Geoemetries
                }

                double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };
                IGeometry area = plot.Geometry;
                List<GeometryInformation> removeList = new List<GeometryInformation>();
                foreach (var geom in plot.Geometries)
                {
                    if (!deactivePlot && geom.Status != 1)
                    {
                        removeList.Add(geom);
                        continue;
                    }
                    if (!beyondPlot)
                    {
                        if (!area.Envelope.Contains(geom.Geometry.Envelope) && plot.PlotId != "AEG42" && plot.PlotId != "HEG31")
                            removeList.Add(geom);
                    }
                }
                foreach (var geom in removeList)
                    plot.Geometries.Remove(geom);

                return plot;
            }
        }

        /// <summary>
        /// produce plot image
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="zoom"></param>
        /// <param name="deactiveGeometries">load deactive subplots</param>
        /// <param name="beyondPlot">load subplots which are outside of the plot border</param>
        /// <param name="gridSize"></param>
        /// <returns>image</returns>
        public String ProducePlot(Plot plot, int zoom = 1, bool deactiveGeometries = true, bool beyondPlot = false, int gridSize = 5)
        {
            SharpMap.Map myMap;
            using (var stream = new MemoryStream())
            {
                if (plot.PlotId == "AEG42" || plot.PlotId == "HEG31")
                {
                    beyondPlot = true;
                }
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                myMap = plot != null ? InitializeMap(new Size(3000, 3000), plot, zoom, deactiveGeometries, beyondPlot, gridSize) : null;
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht

                Image mapImage = CreateMap(myMap); //mapImage.Save("file.png", ImageFormat.Png);
                string mimeType = "image/jpg";/* Get mime type somehow (e.g. "image/png") */;

                ImageConverter _imageConverter = new ImageConverter();

                if (mapImage == null)
                    return String.Format("data:image/png;base64,{0}", Convert.ToBase64String(stream.ToArray()));
                mapImage.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                var img = String.Format("data:image/png;base64,{0}", Convert.ToBase64String(stream.ToArray()));
                return img;
            }
        }

        /// <summary>
        /// produce sharpmap map and visualize all the plot's infromation
        /// </summary>
        /// <param name="outputsize"></param>
        /// <param name="plot"></param>
        /// <param name="zoom"></param>
        /// <param name="deactiveGeometries">load deactive subplots</param>
        /// <param name="beyondPlot">load subplots which are outside of the plot border</param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        private SharpMap.Map InitializeMap(Size outputsize, Plot plot, int zoom = 1, bool deactiveGeometries = true, bool beyondPlot = false, int gridSize = 5)
        {
            //Initialize a new map of size 'imagesize'
            SharpMap.Map map = new SharpMap.Map(outputsize);
            map.SRID = 54004;
            map.BackColor = Color.White;
            ProjNet.CoordinateSystems.CoordinateSystemFactory csFact = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
            GeoAPI.CoordinateSystems.ICoordinateSystem webmercator = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;

            ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctFact = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
            SharpMap.Layers.VectorLayer borderLayer = new SharpMap.Layers.VectorLayer("Border");

            double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };
            IGeometry area = plot.Geometry.Buffer(0.00005); // Add buffer to given plot area to avoid border is not shown, if it is outside of the standard plot area
            List<IGeometry> borderGeometries = new List<IGeometry>();
            borderGeometries.Add(area);
            borderLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(borderGeometries);
            borderLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
            borderLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);


            if (!beyondPlot)
                map = AddGridToMap(map, plot, zoom, beyondPlot, gridSize);

            plot.Geometries = plot.Geometries.OrderByDescending(p => p.Geometry.Length).ToList();

            foreach (var geometry in plot.Geometries)
            {
                //check to ignore deactive geometries
                if (!deactiveGeometries && geometry.Status == 2)
                    continue;

                SharpMap.Layers.VectorLayer plotLayer = new SharpMap.Layers.VectorLayer(geometry.Id.ToString());

                double disss = geometry.Geometry.Envelope.Distance(plot.Geometry.Envelope);

                List<IGeometry> geometries = new List<IGeometry>();
                geometries.Add(geometry.Geometry);
                var dd = new SharpMap.Data.FeatureDataTable();
                dd.Columns.Add("Label");
                SharpMap.Data.FeatureDataRow newRow = dd.NewRow();
                newRow.Geometry = geometry.Geometry;

                plotLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(geometries);

                //newRow["Label"] = Math.Round((plotLayer.Envelope.MaxX - (Convert.ToDouble(plot.Longitude))) * 67000) + "," + Math.Round((plotLayer.Envelope.MaxY - (Convert.ToDouble(plot.Latitude))) * 108800) + "\t \n";
                
                if(geometry.GeometryType.Equals("rectangle"))
                {
                    string[] xy = geometry.Coordinate.Split(',');
                    newRow["Label"] = xy[2] + "," + xy[3];
                }

                if (geometry.GeometryType.Equals("polygon"))
                {
                    string[] tmpXY = geometry.Coordinate.Split(new[] { "),(" }, StringSplitOptions.None);
                    string[] x = tmpXY[0].Split(',');
                    string[] y = tmpXY[1].Split(',');
                    newRow["Label"] = x[2] + "," + y[2];
                }
                if (geometry.GeometryType.Equals("linestring"))
                {
                    string[] tmpXY = geometry.Coordinate.Split(new[] { "),(" }, StringSplitOptions.None);
                    string[] x = tmpXY[0].Split(',');
                    string[] y = tmpXY[1].Split(',');

                    newRow["Label"] = x[0].Substring(1) + "," + y[0];
                }

                if (geometry.GeometryType.Equals("circle"))
                {
                    string origin = geometry.Coordinate.Substring(0);
                    origin = origin.TrimEnd(',');
                    newRow["Label"] = origin;
                }

                plotLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
                plotLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);

                dd.Rows.Clear(); 
                dd.Rows.Add(newRow);
                plotLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(dd);

                SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels")
                {
                    DataSource = plotLayer.DataSource,
                    Enabled = true,
                    LabelColumn = "Label",
                    MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                    LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                    CoordinateTransformation = plotLayer.CoordinateTransformation,
                    PriorityColumn = "Label",

                    Style = new SharpMap.Styles.LabelStyle()
                    {
                        Font = new Font(FontFamily.GenericSerif, 40),
                        HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Right,
                        VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                        CollisionDetection = true,
                        Enabled = true,
                    }
                };

                layLabel.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top;
                layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left;
                layLabel.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

                layLabel.Style.Offset = (new PointF((float)plotLayer.Envelope.MaxX, (float)plotLayer.Envelope.MaxY));

                String borderColor = "#000000";
                if (geometry.GeometryType.ToLower() == "linestring")
                    borderColor = geometry.Color;
                Pen pen = new Pen(ColorTranslator.FromHtml(RGBAToArgb(borderColor)), geometry.LineWidth);
                pen.Width = geometry.LineWidth > 1 ? geometry.LineWidth * 3 : geometry.LineWidth;
                float[] dashValues = { 8, 8 };
                int transparency = 150;
                if (geometry.Status == 2)
                {
                    pen.DashPattern = dashValues;
                    transparency = 0;
                    plotLayer.Style.Fill = new SolidBrush(Color.FromArgb(Int32.Parse(RGBAToArgb(geometry.Color).Replace("#", ""), NumberStyles.HexNumber)));
                }

                int argb = Int32.Parse(RGBAToArgb(geometry.Color).Replace("#", ""), NumberStyles.HexNumber);
                Color clr = Color.FromArgb(argb);
                plotLayer.Style.Fill = new SolidBrush(clr);
                plotLayer.Style.Outline = pen;
                plotLayer.Style.EnableOutline = true;

                if (!beyondPlot && beyondBorderCheck(borderLayer, plotLayer))
                    continue;

                map.Layers.Add(plotLayer);
                map.Layers.Add(layLabel);
            }


            map.ZoomToExtents();

            if (beyondPlot)
            {
                map = AddGridToMap(map, plot, zoom, beyondPlot, gridSize);
                for (int i = 0; i < map.Layers.Count - (gridSize * 2 * 2 - 1); i++)
                {
                    if (i % 2 == 0)
                        continue;
                    SharpMap.Layers.LabelLayer layer = map.Layers.ElementAt(i) as SharpMap.Layers.LabelLayer;
                    layer.Style.Offset = map.WorldToImage(new Coordinate((double)layer.Style.Offset.X, (double)layer.Style.Offset.Y), false);
                    var centroid = map.WorldToImage(layer.Envelope.Centre.CoordinateValue);
                    layer.Style.Offset = new PointF(layer.Style.Offset.X - centroid.X, layer.Style.Offset.Y - centroid.Y);
                    map.Layers[i] = layer; map.Layers.ResetItem(i);
                }
                for (int i = map.Layers.Count - (gridSize * 2 * 2 - 1); i < map.Layers.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        SharpMap.Layers.LabelLayer layer = map.Layers.ElementAt(i) as SharpMap.Layers.LabelLayer;
                        layer.Style.Offset = map.WorldToImage(new Coordinate((double)layer.Style.Offset.X, (double)layer.Style.Offset.Y), false);
                        var centroid = map.WorldToImage(layer.Envelope.Centre.CoordinateValue);
                        layer.Style.Offset = new PointF(layer.Style.Offset.X - centroid.X, layer.Style.Offset.Y - centroid.Y);
                        map.Layers[i] = layer; map.Layers.ResetItem(i);

                    }
                }
            }
            else
            {

                for (int i = 0; i < gridSize * 2 * 2; i++)
                {
                    if (i % 2 == 1)
                    {
                        SharpMap.Layers.LabelLayer layer = map.Layers.ElementAt(i) as SharpMap.Layers.LabelLayer;
                        layer.Style.Offset = map.WorldToImage(new Coordinate((double)layer.Style.Offset.X, (double)layer.Style.Offset.Y), false);
                        var centroid = map.WorldToImage(layer.Envelope.Centre.CoordinateValue);
                        layer.Style.Offset = new PointF(layer.Style.Offset.X - centroid.X - 30, layer.Style.Offset.Y - centroid.Y + 100);
                        map.Layers[i] = layer; map.Layers.ResetItem(i);
                    }
                }

                for (int i = gridSize * 2 * 2 - 1; i < map.Layers.Count; i++)
                {
                    if (i % 2 == 0)
                        continue;
                    SharpMap.Layers.LabelLayer layer = map.Layers.ElementAt(i) as SharpMap.Layers.LabelLayer;
                    layer.Style.Offset = map.WorldToImage(new Coordinate((double)layer.Style.Offset.X, (double)layer.Style.Offset.Y), false);
                    var centroid = map.WorldToImage(layer.Envelope.Centre.CoordinateValue);
                    layer.Style.Offset = new PointF(layer.Style.Offset.X - centroid.X, layer.Style.Offset.Y - centroid.Y);
                    map.Layers[i] = layer; map.Layers.ResetItem(i);
                }
            }
            map.ZoomToExtents();

            return map;
        }
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
        /// <summary>
        /// convert rgb colors to Argb
        /// </summary>
        /// <param name="color"></param>
        /// <returns>Argb color</returns>
        private String RGBAToArgb(String color)
        {
            if (color.Length == 9)
                color = "#" + color.Substring(7,2) + color.Substring(1, 6);
            return color;
        }

        /// <summary>
        /// Generate a plot section to store in a PDF file
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="zoom"></param>
        /// <param name="deactiveGeometries"></param>
        /// <param name="beyondPlot"></param>
        /// <param name="gridSize"></param>
        /// <param name="legend"></param>
        /// <returns></returns>
        private String generatePlotDiv(Plot plot, int zoom = 1, bool deactiveGeometries = true, bool beyondPlot = false, int gridSize = 5, Boolean legend = false)
        {
            double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };
            IGeometry area = plot.Geometry;//SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(calCoordd("rectangle", (plot.Area.X1) + "," + (plot.Area.Y1) + "," + (plot.Area.X3) + "," + (plot.Area.Y3), bb));
            String image = ProducePlot(plot, zoom, deactiveGeometries, beyondPlot, gridSize);
            String Div = "<div><center><image width='900px' height='900px' src='" + image + "'>";
            if (!legend)
            {
                Div += "</center></div>";
                return Div;
            }
            else
            {
                Div += "<table style='font-size:small;border:1px solid black;'><tr><td>Name</td><td>Geometry Type</td><td>Coordinate</td><td>Coordinate Type</td><td>Color</td><td>Description</td></tr>";
                foreach (var geometry in plot.Geometries)
                {
                    if (geometry.Color.Length < 9)
                        geometry.Color += "AA";
                    if (!deactiveGeometries && geometry.Status != 1)
                        continue;
                    if (!beyondPlot)
                    {
                        if (!area.Envelope.Contains(geometry.Geometry))
                            continue;
                    }
                    var color = Color.FromArgb(Int32.Parse(RGBAToArgb(geometry.Color).Replace("#", ""), NumberStyles.HexNumber));
                    Div += "<tr><td>" + geometry.Name + "</td><td>" + geometry.GeometryType + "</td><td>" + geometry.Coordinate + "</td><td>" + geometry.CoordinateType + "</td><td style='width:100px;background-color:rgba(" + color.R + "," + color.G + "," + color.B + "," + color.A + ")'></td><td>" + geometry.Description + "</td></tr>";
                }
                Div += "</table></center></div>";
            }
            return Div;
        }

        /// <summary>
        /// Generate PDF to export a list of plots
        /// </summary>
        /// <param name="plotList"></param>
        /// <param name="zoom"></param>
        /// <param name="deactiveGeometries"></param>
        /// <param name="beyondPlot"></param>
        /// <param name="gridSize"></param>
        /// <param name="legend"></param>
        /// <returns>PDF file</returns>
        public Byte[] generatePDF(List<Plot> plotList, int zoom = 1, bool deactiveGeometries = true, bool beyondPlot = false, int gridSize = 5, Boolean legend = false)
        {
            String Html = "<html><head><style>tr:nth-child(odd) {  background-color: #afafaf;} tr:nth-child(even) {  background-color: #f1f1f1;}</style></head><body >";
            foreach(var plot in plotList)
            {
                Html += generatePlotDiv(GetPlot(plot.Id), zoom, deactiveGeometries, beyondPlot, gridSize, legend);
                Html += "<div style='page-break-after:always'></div>";
            }
            Html += "</body></html>";
            
            var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
            var pdfBytes = htmlToPdf.GeneratePdf(Html);
            return pdfBytes;
        }

        /// <summary>
        /// check if a subplot is in a plot border or not
        /// </summary>
        /// <param name="borderLayer"></param>
        /// <param name="plotLayer"></param>
        /// <returns>true or false</returns>
        private Boolean beyondBorderCheck(SharpMap.Layers.VectorLayer borderLayer, SharpMap.Layers.VectorLayer plotLayer)
        {
            Boolean returnValue = true;
            if (borderLayer.Envelope.Contains(plotLayer.Envelope))
                returnValue = false;
            return returnValue;
        }

        /// <summary>
        /// Creates the map, inserts it into the cache and sets the ImageButton Url
        /// </summary>
        private Image CreateMap(SharpMap.Map myMap)
        {
            Image img= null;
            try
            {
                img = myMap.GetMap();
            } catch (Exception e)
            {
                Console.WriteLine("PlotChart: Can not generate the from the plotchart information");
            }
            return img;
        }

        /// <summary>
        /// Add grid to the map
        /// </summary>
        /// <param name="map"></param>
        /// <param name="plot"></param>
        /// <param name="zoom"></param>
        /// <param name="beyondPlot"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        private SharpMap.Map AddGridToMap(SharpMap.Map map, Plot plot, int zoom, bool beyondPlot = false, int gridSize = 5)
        {
            GeoAPI.CoordinateSystems.ICoordinateSystem webmercator = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;

            ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctFact = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();


            SharpMap.Layers.VectorLayer areaLayer = new SharpMap.Layers.VectorLayer("area");
            double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };

            float X1 = (float)Math.Round(((plot.Geometry.EnvelopeInternal.MinX - bb[0]) * 100000)); //plot.Area.X1 * 1.2f;
            X1 = Convert.ToInt32(X1) / 10 * 10;
            float X2 = (float)Math.Round(((plot.Geometry.EnvelopeInternal.MaxX - bb[0]) * 100000)); //plot.Area.X3 * 1.2f;
            X2 = Convert.ToInt32(X2) / 10 * 10;
            float Y1 = X1; //plot.Area.Y1 * 1.2f;
            float Y2 = X2; //plot.Area.Y3 * 1.2f;
            if (beyondPlot)
            {
                X1 = (float)Math.Round(-((float)(map.Envelope.Width) / 2)); X2 = (float)Math.Round(((float)(map.Envelope.Width) / 2));
                Y1 = (float)Math.Round(-((float)(map.Envelope.Height) / 2)); Y2 = (float)Math.Round(((float)(map.Envelope.Height) / 2));

            }

            IGeometry area = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(calCoordd("rectangle", (X1) + "," + (Y1) + "," + (X2) + "," + (Y2), bb));
            List<IGeometry> areaGeometries = new List<IGeometry>();
            areaGeometries.Add(area);
            areaLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(areaGeometries);
            areaLayer.SRID = 54004;
            areaLayer.TargetSRID = 54004;
            areaLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
            areaLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);

            using (var dd = new SharpMap.Data.FeatureDataTable())
            using (var borderFdt = new SharpMap.Data.FeatureDataTable())

            {

                dd.Columns.Add("Label");
                SharpMap.Data.FeatureDataRow newRow = dd.NewRow();
                newRow.Geometry = area;
                newRow["Label"] = "";
                dd.Rows.Clear(); dd.Rows.Add(newRow); areaLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(dd);
                SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels")
                {
                    DataSource = areaLayer.DataSource,
                    Enabled = true,
                    LabelColumn = "Label",
                    MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                    LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                    CoordinateTransformation = areaLayer.CoordinateTransformation,
                    PriorityColumn = "Label",

                    Style = new SharpMap.Styles.LabelStyle()
                    {
                        Font = new Font(FontFamily.GenericSerif, 50, FontStyle.Bold),
                        HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                        VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                        CollisionDetection = true,
                        Enabled = true,
                    }
                };

                layLabel.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top;
                layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
                layLabel.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

                layLabel.Style.Offset = (new PointF((float)areaLayer.Envelope.Centre.X, (float)areaLayer.Envelope.MaxY));


                areaLayer.Style.Fill = new SolidBrush(Color.FromArgb(0, ColorTranslator.FromHtml("#000000")));
                areaLayer.Style.Outline = Pens.Black;
                areaLayer.Style.EnableOutline = true;
                map.Layers.Add(areaLayer);
                map.Layers.Add(layLabel);

                map.ZoomToExtents();

                float XI = (X2 - (X1)) / gridSize;
                float YI = (Y2 - (Y1)) / gridSize;
                float XS = X1 > X2 ? X1 : X2;// Math.Abs(X1);
                float YS = Y1 > Y2 ? Y1 : Y2;// Math.Abs(Y1);

                SharpMap.Layers.VectorLayer borderLayer = new SharpMap.Layers.VectorLayer("area");
                List<IGeometry> frameGeometry = new List<IGeometry>();
                IGeometry test = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(calCoordd("rectangle", (X1 * 1.1) + "," + (Y1 * 1.1) + "," + (X2 * 1.1) + "," + (Y2 * 1.1), bb));
                frameGeometry.Add(test);
                borderLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(frameGeometry);
                borderLayer.Style.Fill = Brushes.Transparent;
                borderLayer.Style.Outline = Pens.Transparent;
                borderLayer.Style.EnableOutline = false;
                borderLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
                borderLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);

                borderFdt.Columns.Add("Label");
                SharpMap.Data.FeatureDataRow newRowBorder = borderFdt.NewRow();
                newRowBorder.Geometry = test;
                newRowBorder["Label"] = "Plot " + plot.PlotId + "\t \n";
                borderFdt.Rows.Clear(); borderFdt.Rows.Add(newRowBorder); borderLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(borderFdt);
                SharpMap.Layers.LabelLayer layLabelBorder = new SharpMap.Layers.LabelLayer("Country labels")
                {
                    DataSource = borderLayer.DataSource,
                    Enabled = true,
                    LabelColumn = "Label",
                    MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                    LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                    CoordinateTransformation = borderLayer.CoordinateTransformation,
                    PriorityColumn = "Label",

                    Style = new SharpMap.Styles.LabelStyle()
                    {
                        Font = new Font(FontFamily.GenericSerif, 60, FontStyle.Bold),
                        HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                        VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                        CollisionDetection = true,
                        Enabled = true,

                    }
                };

                layLabelBorder.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top;
                layLabelBorder.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
                layLabelBorder.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

                layLabelBorder.Style.Offset = (new PointF((float)borderLayer.Envelope.Centre.X, (float)borderLayer.Envelope.MaxY));

                map.Layers.Add(borderLayer);
                map.Layers.Add(layLabelBorder);

                var max = (int)((X2 - (X1)) / gridSize);
                for (int i = 0; i < max - 1; i++)
                {
                    XS -= gridSize;
                    YS -= gridSize;
                    SharpMap.Layers.VectorLayer gridxLayer = new SharpMap.Layers.VectorLayer("gridx" + i);
                    SharpMap.Layers.VectorLayer gridyLayer = new SharpMap.Layers.VectorLayer("gridy" + i);
                    IGeometry xline = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(calCoordd("linestring", "(" + X2 + "," + X1 + "),(" + YS + "," + YS + ")", bb));
                    IGeometry yline = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(calCoordd("linestring", "(" + XS + "," + XS + "),(" + Y2 + "," + Y1 + ")", bb));

                    List<IGeometry> gridXgeometries = new List<IGeometry>();
                    gridXgeometries.Add(xline);
                    List<IGeometry> gridYgeometries = new List<IGeometry>();
                    gridYgeometries.Add(yline);

                    gridxLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(gridXgeometries);
                    gridxLayer.Style.Fill = new SolidBrush(Color.FromArgb(50, ColorTranslator.FromHtml("#101010")));
                    gridxLayer.Style.Outline = new Pen((Color.FromArgb(50, ColorTranslator.FromHtml("#101010")))); ;
                    gridxLayer.Style.EnableOutline = true;
                    gridxLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
                    gridxLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);
                    using (var fdtX = new SharpMap.Data.FeatureDataTable())
                    {
                        fdtX.Columns.Add("Label");
                        SharpMap.Data.FeatureDataRow newFdtXRow = fdtX.NewRow();
                        newFdtXRow.Geometry = xline;
                        newFdtXRow["Label"] = Math.Round(XS, 2) + "\t \n";
                        fdtX.Rows.Clear(); fdtX.Rows.Add(newFdtXRow); gridxLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(fdtX);
                    }
                    SharpMap.Layers.LabelLayer layerLabelX = new SharpMap.Layers.LabelLayer("Country labels")
                    {
                        DataSource = gridxLayer.DataSource,
                        Enabled = true,
                        LabelColumn = "Label",
                        MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                        LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                        CoordinateTransformation = gridxLayer.CoordinateTransformation,
                        PriorityColumn = "Label",

                        Style = new SharpMap.Styles.LabelStyle()
                        {
                            Font = new Font(FontFamily.GenericSerif, 50, FontStyle.Bold),
                            HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                            VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                            CollisionDetection = true,
                            Enabled = true,
                        }
                    };

                    layerLabelX.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Middle;
                    layerLabelX.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left;
                    layerLabelX.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

                    layerLabelX.Style.Offset = (new PointF((float)gridxLayer.Envelope.MinX, (float)gridxLayer.Envelope.Centre.Y));

                    map.Layers.Add(gridxLayer);
                    map.Layers.Add(layerLabelX);

                    gridyLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(gridYgeometries);
                    gridyLayer.Style.Fill = new SolidBrush(Color.FromArgb(50, ColorTranslator.FromHtml("#101010")));
                    gridyLayer.Style.Outline = new Pen((Color.FromArgb(50, ColorTranslator.FromHtml("#101010"))));
                    gridyLayer.Style.EnableOutline = true;
                    gridyLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
                    gridyLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);
                    using (var fdtY = new SharpMap.Data.FeatureDataTable())
                    {
                        fdtY.Columns.Add("Label");
                        SharpMap.Data.FeatureDataRow newFdtYRow = fdtY.NewRow();
                        newFdtYRow.Geometry = yline;
                        newFdtYRow["Label"] = Math.Round(YS, 2) + "\t \n"; ;
                        fdtY.Rows.Clear(); fdtY.Rows.Add(newFdtYRow); gridyLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(fdtY);
                    }
                    SharpMap.Layers.LabelLayer layerLabelY = new SharpMap.Layers.LabelLayer("Country labels")
                    {
                        DataSource = gridyLayer.DataSource,
                        Enabled = true,
                        LabelColumn = "Label",
                        MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                        LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                        CoordinateTransformation = gridyLayer.CoordinateTransformation,
                        PriorityColumn = "Label",

                        Style = new SharpMap.Styles.LabelStyle()
                        {
                            Font = new Font(FontFamily.GenericSerif, 50, FontStyle.Bold),
                            HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                            VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                            CollisionDetection = true,
                            Enabled = true,
                        }
                    };

                    layerLabelY.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Bottom;
                    layerLabelY.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Right;
                    layerLabelY.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

                    layerLabelY.Style.Offset = (new PointF((float)gridyLayer.Envelope.MinX, (float)gridyLayer.Envelope.MinY));

                    map.Layers.Add(gridyLayer);
                    map.Layers.Add(layerLabelY);
                }

                map.ZoomToExtents();
                return map;
            }
        }

        /// <summary>
        /// check the validity of the coordinate to define a plot or a subplot
        /// </summary>
        /// <param name="geometryType"></param>
        /// <param name="coordType"></param>
        /// <param name="geometry"></param>
        /// <returns>true or false</returns>
        public bool checkGeometry(String geometryType, String coordType, String geometry)
        {
            //Holds regular expression
            String regGeometry = "";

            //xy - Coordinates
            if (coordType == "xy")
            {
                if (geometryType == "rectangle")
                {
                    //x1.a1,y1.b1,x2.a2,y2.b2
                    regGeometry = "^-?\\d+(\\.\\d+)?,-?\\d+(\\.\\d+)?,-?\\d+(\\.\\d+)?,-?\\d+(\\.\\d+)?$";
                }
                else if (geometryType == "linestring")
                {
                    //(x1.a1,...xn.an),(y1.b1,...,yn.bn)
                    regGeometry = @"^\((-?\d+(\.\d+)?\,?)+\),\((-?\d+(\.\d+)?\,?)+\)$";
                }
                else if (geometryType == "circle")
                {
                    //(x1.a1,...xn.an),(y1.b1,...,yn.bn)
                    regGeometry = @"^-?\d+(\.\d+)?\,-?\d+(\.\d+)?,\d*(\.\d+)?$";
                }
                if (geometryType == "polygon")
                {
                    //(x1.a1,...xn.an),(y1.b1,...,yn.bn)
                    regGeometry = @"^\(-?\d{1,}(\.\d{1,}){0,1}(,-?\d{1,}(\.\d{1,}){0,1})+\),\(-?\d{1,}(\.\d{1,}){0,1}(,-?\d{1,}(\.\d{1,}){0,1})+\)$";
                }
                else if (geometryType == "error_rect")
                {
                    regGeometry = "false";
                }
            }
            //polar - Coordinates
            else if (coordType == "polar")
            {
                if (geometryType == "rectangle")
                {
                    regGeometry = "^\\(-?\\d+(\\.\\d+)*,-?\\d+(\\.\\d+)*\\),\\(-?\\d+(\\.\\d+)*,-?\\d+(\\.\\d+)*\\)$";
                }
                else if (geometryType == "linestring")
                {
                    regGeometry = "^\\(-?\\d+(\\.\\d+)*,-?\\d+(\\.\\d+)*\\)(,\\(-?\\d+(\\.\\d+)*,-?\\d+(\\.\\d+)*\\))*$";
                }
                else if (geometryType == "polygon")
                {
                    regGeometry = @"^\(-?\d+(\.\d+)*,-?\d+(\.\d+)*\)(,\(-?\d+(\.\d+)*,-?\d+(\.\d+)*\))*$";
                }
                else if (geometryType == "circle")
                {
                    regGeometry = "^\\((-?\\d+(\\.\\d+)?\\,?)+\\),(-?\\d+(\\.\\d+)?\\,?)+$";
                }
                else if (geometryType == "error_rect")
                {
                    regGeometry = "^\\(-?\\d+(\\.\\d+)*,-?\\d+(\\.\\d+)*\\),\\(-?\\d+(\\.\\d+)*,-?\\d+(\\.\\d+)*,-?\\d+(\\.\\d+)*,-?\\d+(\\.\\d+)*\\)$";
                }
            }

            //Delete white-space
            geometry = geometry.Replace(" ", "");

            //Test regEx
            if (Regex.IsMatch(geometry, regGeometry))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// produce WKT(well knows text) to visualize a plot or subplot geometry
        /// </summary>
        /// <param name="geomType"></param>
        /// <param name="coord"></param>
        /// <param name="aa"></param>
        /// <param name="coordType"></param>
        /// <param name="refPoint">it have to be filled when the coordType is polar</param>
        /// <returns></returns>
        public static string calCoordd(string geomType, string coord, double[] aa, string coordType = "xy", string refPoint = "")
        {
            double[,] toReturn = null;
            bool isLinestring = false;
            if (coordType.Equals("xy") && geomType.ToLower().Equals("rectangle"))
            {

                string[] words = coord.ToString().Split(',');
                double[] myDoubles = Array.ConvertAll(words, double.Parse);

                double[] pointCoordProj1 = { CalcWithWgs84.ComputeLon(aa[0], aa[1], myDoubles[0]), CalcWithWgs84.ComputeLat(aa[1], myDoubles[1]) };
                double[] pointCoordProj2 = { CalcWithWgs84.ComputeLon(aa[0], aa[1], myDoubles[0]), CalcWithWgs84.ComputeLat(aa[1], myDoubles[3]) };
                double[] pointCoordProj3 = { CalcWithWgs84.ComputeLon(aa[0], aa[1], myDoubles[2]), CalcWithWgs84.ComputeLat(aa[1], myDoubles[3]) };
                double[] pointCoordProj4 = { CalcWithWgs84.ComputeLon(aa[0], aa[1], myDoubles[2]), CalcWithWgs84.ComputeLat(aa[1], myDoubles[1]) };
                toReturn = new double[5, 2];

                toReturn[0, 0] = pointCoordProj1[0];
                toReturn[0, 1] = pointCoordProj1[1];

                toReturn[1, 0] = pointCoordProj2[0];
                toReturn[1, 1] = pointCoordProj2[1];

                toReturn[2, 0] = pointCoordProj3[0];
                toReturn[2, 1] = pointCoordProj3[1];

                toReturn[3, 0] = pointCoordProj4[0];
                toReturn[3, 1] = pointCoordProj4[1];

                toReturn[4, 0] = pointCoordProj1[0];
                toReturn[4, 1] = pointCoordProj1[1];
            }
            else if (coordType.Equals("xy") && geomType.ToLower().Equals("linestring"))
            {
                Console.WriteLine("linestring");
                isLinestring = true;
                string[] words = coord.ToString().Replace("(", "").Replace(")", "").Split(',');
                double[] myDoubles = Array.ConvertAll(words, double.Parse);
                int noOfPoints = myDoubles.Length / 2;
                toReturn = new double[noOfPoints, 2];
                for (int i = 0; i < noOfPoints; i++)
                {
                    toReturn[i, 0] = CalcWithWgs84.ComputeLon(aa[0], aa[1], myDoubles[i]);
                    toReturn[i, 1] = CalcWithWgs84.ComputeLat(aa[1], myDoubles[i + noOfPoints]);
                }
            }
            else if (coordType.Equals("xy") && geomType.ToLower().Equals("polygon"))
            {
                Console.WriteLine("polygon");
                string[] words = coord.ToString().Replace("(", "").Replace(")", "").Split(',');
                double[] myDoubles = Array.ConvertAll(words, double.Parse);
                int noOfPoints = (myDoubles.Length / 2) + 1;
                toReturn = new double[noOfPoints, 2];
                for (int i = 0; i < noOfPoints - 1; i++)
                {
                    toReturn[i, 0] = CalcWithWgs84.ComputeLon(aa[0], aa[1], myDoubles[i]);
                    toReturn[i, 1] = CalcWithWgs84.ComputeLat(aa[1], myDoubles[i + noOfPoints - 1]);
                }
                toReturn[noOfPoints - 1, 0] = toReturn[0, 0];
                toReturn[noOfPoints - 1, 1] = toReturn[0, 1];
            }
            else if (coordType.Equals("xy") && geomType.ToLower().Equals("circle"))
            {
                Console.WriteLine("circle");
                int n = 36;
                string[] words = coord.ToString().Replace("(", "").Replace(")", "").Split(',');
                double[] myDoubles = Array.ConvertAll(words, double.Parse);
                toReturn = new double[n + 1, 2];
                for (int i = 0; i < n; i++)
                {
                    toReturn[i, 0] = CalcWithWgs84.ComputeLon(aa[0], aa[1], myDoubles[0] + myDoubles[2] * Math.Sin((2 * Math.PI / n) * i));
                    toReturn[i, 1] = CalcWithWgs84.ComputeLat(aa[1], myDoubles[1] + myDoubles[2] * Math.Cos((2 * Math.PI / n) * i));

                }
                toReturn[n, 0] = toReturn[0, 0];
                toReturn[n, 1] = toReturn[0, 1];
            }
            else if (coordType.Equals("polar") && geomType.ToLower().Equals("circle"))
            {
                Console.WriteLine("polar and circle");
                int n = 36;
                string[] words = coord.ToString().Replace("(", "").Replace(")", "").Split(',');
                double[] myDoubles = Array.ConvertAll(words, double.Parse);
                string[] words1 = refPoint.ToString().Replace("(", "").Replace(")", "").Split(',');
                double[] partialRefPoint = Array.ConvertAll(words1, double.Parse);
                toReturn = new double[n + 1, 2];
                for (int i = 0; i < n; i++)
                {
                    toReturn[i, 0] = CalcWithWgs84.ComputeLon(aa[0], aa[1], partialRefPoint[0] + myDoubles[1] * Math.Sin(0.9 * myDoubles[0]) + myDoubles[2] * Math.Sin((2 * Math.PI / n) * i));
                    toReturn[i, 1] = CalcWithWgs84.ComputeLat(aa[1], partialRefPoint[1] + myDoubles[1] * Math.Cos(0.9 * myDoubles[0]) + myDoubles[2] * Math.Cos((2 * Math.PI / n) * i));
                }
                toReturn[n, 0] = toReturn[0, 0];
                toReturn[n, 1] = toReturn[0, 1];
            }
            else if (coordType.Equals("polar") && geomType.ToLower().Equals("polygon"))
            {
                Console.WriteLine("polygon and polar");
                string[] words = coord.ToString().Replace("(", "").Replace(")", "").Split(',');
                double[] myDoubles = Array.ConvertAll(words, double.Parse);
                int n = myDoubles.Length / 2;
                string[] words1 = refPoint.ToString().Replace("(", "").Replace(")", "").Split(',');
                double[] partialRefPoint = Array.ConvertAll(words1, double.Parse);
                toReturn = new double[n + 1, 2];
                for (int i = 0; i < n; i++)
                {
                    toReturn[i, 0] = CalcWithWgs84.ComputeLon(aa[0], aa[1], partialRefPoint[0] + myDoubles[i * 2 + 1] * Math.Sin(0.9 * myDoubles[i * 2] * Math.PI / 180));
                    toReturn[i, 1] = CalcWithWgs84.ComputeLat(aa[1], partialRefPoint[1] + myDoubles[i * 2 + 1] * Math.Cos(0.9 * myDoubles[i * 2] * Math.PI / 180));
                }
                toReturn[n, 0] = toReturn[0, 0];
                toReturn[n, 1] = toReturn[0, 1];
            }
            else
            {
                return null;
            }
            string strToReturn = "";
            for (int i = 0; i < toReturn.Length / 2; i++)
            {
                if (i == 0)
                {
                    strToReturn = strToReturn + toReturn[i, 0].ToString() + ' ' + toReturn[i, 1].ToString() + ' ';
                }
                else
                    strToReturn = strToReturn + ',' + toReturn[i, 0].ToString() + ' ' + toReturn[i, 1].ToString() + ' ';
            }

            strToReturn = strToReturn.Trim();

            if (isLinestring)
            {
                strToReturn = "linestring(" + strToReturn + ")";
            }
            else
            {
                strToReturn = "POLYGON ( ( " + strToReturn + " ) )";
            }
            return strToReturn;
        }


    }
}