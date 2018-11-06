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
using GeoAPI.CoordinateSystems.Transformations;

namespace BExIS.Pmm.Model
{
    public class Plotchart
    {
        public Plot AddPlot(string coordinate, string geometrytype, string coordinatetype, string name, String latitude, String longitude, string refrencePoint = "")
        {
            if (!checkGeometry(geometrytype, coordinatetype, coordinate))
                return null;

            double[] bb = { Convert.ToDouble(longitude), Convert.ToDouble(latitude) };
            String geometryText = calCoordd(geometrytype, coordinate, bb, coordinatetype, refrencePoint);

            PlotManager pManager = new PlotManager();
            Plot plot = pManager.Create(name, "", latitude, longitude, new List<GeometryInformation>(), coordinate, coordinatetype, geometrytype, geometryText);
            PlotHistoryManager pHManager = new PlotHistoryManager();
            pHManager.Create(plot.PlotId, plot.PlotType, plot.Latitude, plot.Longitude, plot.Coordinate, plot.CoordinateType, plot.GeometryType, plot.GeometryText, plot.Id, "Create", DateTime.Now);
            
            return plot;
        }

        public Plot UpdatePlot(long plotid, string coordinate, string geometrytype, string coordinatetype, string name, String latitude, String longitude, string refrencePoint = "")
        {
            PlotManager pManager = new PlotManager();
            Plot plot = pManager.Repo.Get(x => x.Id == plotid).First();
            bool locationChanged = false;

            if (!checkGeometry(geometrytype, coordinatetype, coordinate))
                return null;

            double[] bb = { Convert.ToDouble(longitude), Convert.ToDouble(latitude) };
            String geometryText = calCoordd(geometrytype, coordinate, bb, coordinatetype, refrencePoint);

            if(plot.Latitude != latitude || plot.Longitude != longitude)
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
                GeometryManager gManager = new GeometryManager();
                foreach (var geom in Plot.Geometries)
                {
                    geom.GeometryText = calCoordd(geom.GeometryType, geom.Coordinate, bb, geom.CoordinateType, geom.RefrencePoint);
                    gManager.Update(geom);
                }
            }
            PlotHistoryManager pHManager = new PlotHistoryManager();
            pHManager.Create(plot.PlotId, plot.PlotType, plot.Latitude, plot.Longitude, plot.Coordinate, plot.CoordinateType, plot.GeometryType, plot.GeometryText, plot.Id, "Update", DateTime.Now);

            return plot;
        }

        public Plot DeletePlot(long plotid)
        {
            PlotManager pManager = new PlotManager();
            Plot plot = pManager.Repo.Get(x => x.Id == plotid).First();
            plot.Status = (byte)(plot.Status == 3 ? 1 : 3);
            Plot Plot = pManager.Update(plot);
            PlotHistoryManager pHManager = new PlotHistoryManager();
            pHManager.Create(plot.PlotId, plot.PlotType, plot.Latitude, plot.Longitude, plot.Coordinate, plot.CoordinateType, plot.GeometryType, plot.GeometryText, plot.Id, "Delete", DateTime.Now);

            return plot;
        }

        public Plot ArchivePlot(long plotid)
        {
            PlotManager pManager = new PlotManager();
            Plot plot = pManager.Repo.Get(x => x.Id == plotid).First();
            plot.Status = (byte)(plot.Status == 2 ? 1 : 2);
            Plot Plot = pManager.Update(plot);
            PlotHistoryManager pHManager = new PlotHistoryManager();
            pHManager.Create(plot.PlotId, plot.PlotType, plot.Latitude, plot.Longitude, plot.Coordinate, plot.CoordinateType, plot.GeometryType, plot.GeometryText, plot.Id, "Update", DateTime.Now);

            return plot;
        }

        public String CheckDuplicatePlotName(String action, String name, long plotId)
        {
            String message = "valid";
            PlotManager pManager = new PlotManager();
            if (action == "create")
            { 
                if (pManager.Repo.Get(x => x.PlotId == name && x.Status == 1).Count() != 0)
                    message = "Active Plot with a same name already exist";
            }
            else if(action == "update")
                if (pManager.Repo.Get(x => x.PlotId == name && x.Status == 1 && x.Id != plotId).Count() != 0)
                    message = "Active Plot with a same name already exist";

            return message;
        }

        public GeometryInformation AddGeometry(long plotid, string coordinate, string geometrytype, string coordinatetype, string color, string name, string description, string refrencePoint = "")
        {
            Plot plot = GetPlot(plotid);

            if (!checkGeometry(geometrytype, coordinatetype, coordinate))
                return null;

            double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };
            String geometryText = calCoordd(geometrytype, coordinate, bb, coordinatetype, refrencePoint);

            GeometryManager gManager = new GeometryManager();
            GeometryInformation geometry = gManager.Create(plot.Id, coordinate, geometrytype, coordinatetype, color, geometryText, plot, name, description);
            GeometryHistoryManager gHManager = new GeometryHistoryManager();
            gHManager.Create(geometry.PlotId, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.GeometryText, geometry.Name, geometry.Description, geometry.Id, "Create", DateTime.Now);

            return geometry;
        }

        public GeometryInformation UpdateGeometry(long geometryId, string coordinate, string geometrytype, string coordinatetype, string color, string name, string description, string refrencePoint = "")
        {
            GeometryManager gManager = new GeometryManager();
            GeometryInformation geom = gManager.Repo.Get(x => x.Id == geometryId).First();

            if (!checkGeometry(geometrytype, coordinatetype, coordinate))
                return null;

            double[] bb = { Convert.ToDouble(geom.Plot.Longitude), Convert.ToDouble(geom.Plot.Latitude) };
            String geometryText = calCoordd(geometrytype, coordinate, bb, coordinatetype, refrencePoint);

            geom.GeometryText = geometryText;
            geom.Coordinate = coordinate;
            geom.GeometryType = geometrytype;
            geom.CoordinateType = coordinatetype;
            geom.Color = color;
            geom.Name = name;
            geom.Description = description;

            GeometryInformation geometry = gManager.Update(geom);
            GeometryHistoryManager gHManager = new GeometryHistoryManager();
            gHManager.Create(geometry.PlotId, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.GeometryText, geometry.Name, geometry.Description, geometry.Id, "Update", DateTime.Now);

            return geometry;
        }

        public GeometryInformation ArchiveGeometry(long geometryId)
        {
            GeometryManager gManager = new GeometryManager();
            GeometryInformation geom = gManager.Repo.Get(x => x.Id == geometryId).First();
            geom.Status = (byte)(geom.Status == 2 ? 1 : 2);
            GeometryInformation geometry = gManager.Update(geom);
            GeometryHistoryManager gHManager = new GeometryHistoryManager();
            gHManager.Create(geometry.PlotId, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.GeometryText, geometry.Name, geometry.Description, geometry.Id, "Update", DateTime.Now);

            return geometry;
        }

        public GeometryInformation DeleteGeometry(long geometryId)
        {
            GeometryManager gManager = new GeometryManager();
            GeometryInformation geom = gManager.Repo.Get(x => x.Id == geometryId).First();
            geom.Status = (byte)(geom.Status == 3 ? 1 : 3);

            GeometryInformation geometry = gManager.Update(geom);
            GeometryHistoryManager gHManager = new GeometryHistoryManager();
            gHManager.Create(geometry.PlotId, geometry.Coordinate, geometry.GeometryType, geometry.CoordinateType, geometry.Color, geometry.GeometryText, geometry.Name, geometry.Description, geometry.Id, "Delete", DateTime.Now);

            return geometry;
        }

        public IList<Plot> GetPlots()
        {
            PlotManager pcManager = new PlotManager();
            IList<Plot> plots = pcManager.Repo.Get();
            
            return plots;
        }

        public Plot GetPlot(string plotid)
        {
            PlotManager pcManager = new PlotManager();
            Plot plot = pcManager.Repo.Get(x => x.PlotId == plotid).ElementAtOrDefault(0);
            return plot;
        }

        public Plot GetPlot(long plotid)
        {
            PlotManager pcManager = new PlotManager();
            Plot plot = pcManager.Repo.Get(x => x.Id == plotid).ElementAtOrDefault(0);
            return plot;
        }

        public Plot GetPlot(long plotid, Boolean deactivePlot, Boolean beyondPlot)
        {
            PlotManager pcManager = new PlotManager();
            Plot plot = pcManager.Repo.Get(x => x.Id == plotid).ElementAtOrDefault(0);
            if (plot == null)
            {
                plot = new Plot();
                plot.Geometries = new List<GeometryInformation>();
                return plot;
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
                if(!beyondPlot)
                {
                    if (!area.Envelope.Contains(geom.Geometry.Envelope))
                        removeList.Add(geom);
                }
            }
           foreach (var geom in removeList)
                plot.Geometries.Remove(geom);

            return plot;
        }

        public String ProducePlot(Plot plot, int zoom = 1, bool deactiveGeometries = true, bool beyondPlot = false, int gridSize = 5)
        {
            SharpMap.Map myMap;


            myMap = plot != null ? InitializeMap(new Size(3000, 3000), plot, zoom, deactiveGeometries, beyondPlot, gridSize) : null;

            Image mapImage = CreateMap(myMap); //mapImage.Save("file.png", ImageFormat.Png);
            string mimeType = "image/jpg";/* Get mime type somehow (e.g. "image/png") */;

            ImageConverter _imageConverter = new ImageConverter();
            //string base64 = Convert.ToBase64String((byte[])_imageConverter.ConvertTo(mapImage, typeof(byte[])));
            var stream = new MemoryStream();
            if (mapImage == null)
                return String.Format("data:image/png;base64,{0}", Convert.ToBase64String(stream.ToArray()));
            mapImage.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            var img = String.Format("data:image/png;base64,{0}", Convert.ToBase64String(stream.ToArray()));
            return img;
        }

        private SharpMap.Map InitializeMap(Size outputsize, Plot plot, int zoom = 1, bool deactiveGeometries = true, bool beyondPlot = false, int gridSize = 5)
        {
            //Initialize a new map of size 'imagesize'
            SharpMap.Map map = new SharpMap.Map(outputsize);
            map.SRID = 54004;
            map.BackColor = Color.White;
            //Plot plot = this.GetPlot(plotid);
            ProjNet.CoordinateSystems.CoordinateSystemFactory csFact = new ProjNet.CoordinateSystems.CoordinateSystemFactory();

            //string wkt_gk = "PROJCS[\"World_Mercator\", GEOGCS[\"GCS_WGS_1984\", DATUM[\"WGS_1984\", SPHEROID[\"WGS_1984\", 6378137, 298.257223563]], PRIMEM[\"Greenwich\", 0], UNIT[\"Degree\", 0.017453292519943295]], PROJECTION[\"Mercator_1SP\"], PARAMETER[\"False_Easting\", 0], PARAMETER[\"False_Northing\", 0], PARAMETER[\"Central_Meridian\", 0], PARAMETER[\"Standard_Parallel_1\", 0], UNIT[\"Meter\", 1], AUTHORITY[\"EPSG\", \"54004\"]]";
            //string wkt_gk = "PROJCS[\"Popular Visualisation CRS / Mercator\",    GEOGCS[\"Popular Visualisation CRS\",        DATUM[\"Popular_Visualisation_Datum\",            SPHEROID[\"Popular Visualisation Sphere\",6378137,0,                AUTHORITY[\"EPSG\",\"7059\"]],            TOWGS84[0,0,0,0,0,0,0],            AUTHORITY[\"EPSG\",\"6055\"]],        PRIMEM[\"Greenwich\",0,            AUTHORITY[\"EPSG\",\"8901\"]],        UNIT[\"degree\",0.01745329251994328,            AUTHORITY[\"EPSG\",\"9122\"]],        AUTHORITY[\"EPSG\",\"4055\"]],    UNIT[\"metre\",1,        AUTHORITY[\"EPSG\",\"9001\"]],    PROJECTION[\"Mercator_1SP\"],    PARAMETER[\"central_meridian\",0],    PARAMETER[\"scale_factor\",1],    PARAMETER[\"false_easting\",0],    PARAMETER[\"false_northing\",0],    AUTHORITY[\"EPSG\",\"3785\"],    AXIS[\"X\",EAST],    AXIS[\"Y\",NORTH]]";
            //string wkt_4326 = "PROJCS[\"WGS 84 / Plate Carree (deprecated)\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Equirectangular\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",0],PARAMETER[\"false_easting\",0],PARAMETER[\"false_northing\",0],AUTHORITY[\"EPSG\",\"32662\"],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH]]";
            //GeoAPI.CoordinateSystems.ICoordinateSystem World_Mercator = csFact.CreateFromWkt(wkt_gk);

            //GeoAPI.CoordinateSystems.ICoordinateSystem WGS_4326 = csFact.CreateFromWkt(wkt_4326);
            //GeoAPI.CoordinateSystems.ICoordinateSystem WGS_4326 = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
            GeoAPI.CoordinateSystems.ICoordinateSystem webmercator = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;

            ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctFact = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            //ICoordinateTransformation transform = ctFact.CreateFromCoordinateSystems(webmercator, WGS_4326);

            SharpMap.Layers.VectorLayer borderLayer = new SharpMap.Layers.VectorLayer("Border");
            double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };
            IGeometry area = plot.Geometry;
            List<IGeometry> borderGeometries = new List<IGeometry>();
            borderGeometries.Add(area);
            borderLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(borderGeometries);
            borderLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
            borderLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);


            if (!beyondPlot)
                map = AddGridToMap(map, plot, zoom, beyondPlot, gridSize);
            
            
            foreach (var geometry in plot.Geometries)
            {
                //check to ignore deactive geometries
                if (!deactiveGeometries && geometry.Status == 2)
                    continue;

                SharpMap.Layers.VectorLayer plotLayer = new SharpMap.Layers.VectorLayer(geometry.Id.ToString());

                double disss = geometry.Geometry.Envelope.Distance(plot.Geometry.Envelope);

                List<IGeometry> geometries = new List<IGeometry>();
                //geometry.Geometry = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(geometry.GeometryText);
                geometries.Add(geometry.Geometry);
                //plotLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(geometries);
                //new SharpMap.Data.Providers.GeometryProvider()
                var dd = new SharpMap.Data.FeatureDataTable();
                dd.Columns.Add("Label");
                SharpMap.Data.FeatureDataRow newRow = dd.NewRow();
                newRow.Geometry = geometry.Geometry;
                //newRow["Label"] = (plot.Latitude) + "\t \n";
                //dd.Rows.Add(new object[] { geometry.Geometry, "Salam"}); dd.AcceptChanges();
                //dd.Rows.Add(newRow);
                plotLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(geometries);
                
                //plotLayer.SRID = 3752;

                newRow["Label"] = Math.Round((plotLayer.Envelope.MaxX - (Convert.ToDouble(plot.Longitude))) * 67000) + "," + Math.Round((plotLayer.Envelope.MaxY - (Convert.ToDouble(plot.Latitude))) * 108800) + "\t \n";
                plotLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
                plotLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);
                

                dd.Rows.Clear(); dd.Rows.Add(newRow); plotLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(dd);

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
                        //ForeColor = System.Drawing.Color.Green,
                        Font = new Font(FontFamily.GenericSerif, 40),
                        //BackColor = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0)),
                        HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Right,
                        VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                        CollisionDetection = true,
                        Enabled = true,


                        //MaxVisible = 90,
                        //MinVisible = 30
                    }
                };

                layLabel.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top;
                layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left;
                layLabel.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

                layLabel.Style.Offset = (new PointF((float)plotLayer.Envelope.MaxX, (float)plotLayer.Envelope.MaxY));
                //layLabel.Style.Offset = map.WorldToImage(new Coordinate((double)layLabel.Style.Offset.X, (double)layLabel.Style.Offset.Y), false);
                //var centroid = map.WorldToImage(layLabel.Envelope.Centre.CoordinateValue);
                //layLabel.Style.Offset = new PointF(layLabel.Style.Offset.X - centroid.X, layLabel.Style.Offset.Y - centroid.Y);


                //layLabel.Style.Offset = (new PointF((float)plotLayer.Envelope.MaxX, (float)plotLayer.Envelope.MaxY));

                //Set the polygons to have a black outline
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
                //pen.Width = geometry.LineWidth;
                plotLayer.Style.Outline = pen;
                plotLayer.Style.EnableOutline = true;

                if (!beyondPlot && beyondBorderCheck(borderLayer, plotLayer))
                    continue;

                //Add the layers to the map object.
                //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
                //plotLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);
                //plotLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
                //plotLayer.CoordinateTransformation = transform;
                //plotLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, WGS_4326);

                map.Layers.Add(plotLayer);
                map.Layers.Add(layLabel);
            }


            


            ////////////////////////////////////////
            /////////////////////////////////////


            //zoom = zoom == 0 ? 1 : zoom;

            //map.ZoomToExtents();
            //map.Zoom /= zoom;

            /*for (int i = 9; i < map.Layers.Count ; i++)
            {
                if (i % 2 == 1)
                    continue;
                SharpMap.Layers.LabelLayer layer = map.Layers.ElementAt(i) as SharpMap.Layers.LabelLayer;
                layer.Style.Offset = map.WorldToImage(new Coordinate((double)layer.Style.Offset.X, (double)layer.Style.Offset.Y), false);
                var centroid = map.WorldToImage(layer.Envelope.Centre.CoordinateValue);
                layer.Style.Offset = new PointF(layer.Style.Offset.X - centroid.X, layer.Style.Offset.Y - centroid.Y);
                map.Layers[i] = layer; map.Layers.ResetItem(i);
            }*/

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

                //////////////
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


                /////////////

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

        private String RGBAToArgb(String color)
        {
            if (color.Length == 9)
                color = "#" + color.Substring(7,2) + color.Substring(1, 6);
            return color;
        }

        private String generatePlotDiv(Plot plot, int zoom = 1, bool deactiveGeometries = true, bool beyondPlot = false, int gridSize = 5, Boolean legend = false)
        {
            double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };
            IGeometry area = plot.Geometry;//SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(calCoordd("rectangle", (plot.Area.X1) + "," + (plot.Area.Y1) + "," + (plot.Area.X3) + "," + (plot.Area.Y3), bb));
            String image = ProducePlot(plot, zoom, deactiveGeometries, beyondPlot, gridSize);
            String Div = "<div><center><image width='900px' height='900px' src='" + image + "'>";
            if (legend)
            {
                Div += "</center></div>";
                return Div;
            }
            Div += "<table style='font-size:small;border:1px solid black;'><tr><td>Name</td><td>Geometry Type</td><td>Coordinate</td><td>Coordinate Type</td><td>Color</td><td>Description</td></tr>";
            foreach (var geometry in plot.Geometries)
            {
                if (geometry.Color.Length < 9)
                    geometry.Color += "AA";
                if (!deactiveGeometries && geometry.Status != 1)
                    continue;
                if (!beyondPlot)
                {
                    //IGeometry geom2 = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(geometry.GeometryText);
                    if (!area.Envelope.Contains(geometry.Geometry))
                        continue;
                }
                var color = Color.FromArgb(Int32.Parse(RGBAToArgb(geometry.Color).Replace("#", ""), NumberStyles.HexNumber));
                Div += "<tr><td>" + geometry.Name + "</td><td>" + geometry.GeometryType + "</td><td>" + geometry.Coordinate + "</td><td>" + geometry.CoordinateType + "</td><td style='width:100px;background-color:rgba(" + color.R + "," + color.G + "," + color.B + "," + color.A + ")'></td><td>" + geometry.Description + "</td></tr>";
            }
            Div += "</table></center></div>";
            return Div;
        }

        public Byte[] generatePDF(List<Plot> plotList, int zoom = 1, bool deactiveGeometries = true, bool beyondPlot = false, int gridSize = 5, Boolean legend = false)
        {
            String Html = "<html><head><style>tr:nth-child(odd) {  background-color: #afafaf;} tr:nth-child(even) {  background-color: #f1f1f1;}</style></head><body >";
            foreach(var plot in plotList)
            {
                Html += generatePlotDiv(plot, zoom, deactiveGeometries, beyondPlot, gridSize, legend);
                Html += "<div style='page-break-after:always'></div>";
            }
            Html += "</body></html>";
            
            var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
            var pdfBytes = htmlToPdf.GeneratePdf(Html);
            return pdfBytes;
        }

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
            //string imgID = SharpMap.Web.Caching.InsertIntoCache(1, img);
            //imgMap.ImageUrl = "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
            return img;
        }

        private SharpMap.Map AddGridToMap(SharpMap.Map map, Plot plot, int zoom, bool beyondPlot = false, int gridSize = 5)
        {
            //////////////////////////
            //////////////////////////
            GeoAPI.CoordinateSystems.ICoordinateSystem webmercator = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;

            ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctFact = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();


            SharpMap.Layers.VectorLayer areaLayer = new SharpMap.Layers.VectorLayer("area");
            double[] bb = { Convert.ToDouble(plot.Longitude), Convert.ToDouble(plot.Latitude) };

            

            float X1 = (float)Math.Round(((plot.Geometry.EnvelopeInternal.MinX - bb[0]) * 108800)); //plot.Area.X1 * 1.2f;
            X1 = Convert.ToInt32(X1) / gridSize * gridSize;
            float X2 = (float)Math.Round(((plot.Geometry.EnvelopeInternal.MaxX - bb[0]) * 108800)); //plot.Area.X3 * 1.2f;
            X2 = Convert.ToInt32(X2) / gridSize * gridSize;
            float Y1 = X1; //plot.Area.Y1 * 1.2f;
            float Y2 = X2; //plot.Area.Y3 * 1.2f;
            if (beyondPlot)
            {
                //map.ZoomToExtents();
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


            ////////////////////////////////////////
            var dd = new SharpMap.Data.FeatureDataTable();
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
                    //ForeColor = System.Drawing.Color.Green,
                    Font = new Font(FontFamily.GenericSerif, 50, FontStyle.Bold),
                    //BackColor = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0)),
                    HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                    VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                    CollisionDetection = true,
                    Enabled = true,


                    //MaxVisible = 90,
                    //MinVisible = 30
                }
            };

            layLabel.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top;
            layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
            layLabel.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

            layLabel.Style.Offset = (new PointF((float)areaLayer.Envelope.Centre.X, (float)areaLayer.Envelope.MaxY));






            ///////////////////////////////////////


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

            ////////////////////////////////////////
            var borderFdt = new SharpMap.Data.FeatureDataTable();
            borderFdt.Columns.Add("Label");
            SharpMap.Data.FeatureDataRow newRowBorder = borderFdt.NewRow();
            newRowBorder.Geometry = test;
            newRowBorder["Label"] = "Basistemplate " + plot.PlotId + "\t \n";
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
                    //ForeColor = System.Drawing.Color.Green,
                    Font = new Font(FontFamily.GenericSerif, 60, FontStyle.Bold),
                    //BackColor = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0)),
                    HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                    VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                    CollisionDetection = true,
                    Enabled = true,


                    //MaxVisible = 90,
                    //MinVisible = 30
                }
            };

            layLabelBorder.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top;
            layLabelBorder.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
            layLabelBorder.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

            layLabelBorder.Style.Offset = (new PointF((float)borderLayer.Envelope.Centre.X, (float)borderLayer.Envelope.MaxY));






            ///////////////////////////////////////
            map.Layers.Add(borderLayer);
            map.Layers.Add(layLabelBorder);



            for (int i = 0; i < gridSize - 1; i++)
            {
                XS -= XI;
                YS -= YI;
                SharpMap.Layers.VectorLayer gridxLayer = new SharpMap.Layers.VectorLayer("gridx" + i);
                SharpMap.Layers.VectorLayer gridyLayer = new SharpMap.Layers.VectorLayer("gridy" + i);
                IGeometry xline = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(calCoordd("linestring", "(" + X2 + "," + X1 + "),(" + YS + "," + YS+ ")", bb));
                IGeometry yline = SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse(calCoordd("linestring", "(" + XS + "," + XS + "),(" + Y2 + "," + Y1+ ")", bb));

                List<IGeometry> gridXgeometries = new List<IGeometry>();
                gridXgeometries.Add(xline);
                List<IGeometry> gridYgeometries = new List<IGeometry>();
                gridYgeometries.Add(yline);

                gridxLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(gridXgeometries);
                gridxLayer.Style.Fill = new SolidBrush(Color.FromArgb(50, ColorTranslator.FromHtml("#101010")));
                //pen.Width = geometry.LineWidth;
                gridxLayer.Style.Outline = new Pen((Color.FromArgb(50, ColorTranslator.FromHtml("#101010")))); ;
                gridxLayer.Style.EnableOutline = true;
                gridxLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
                gridxLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);

                ////////////////////////////////////////
                var fdtX = new SharpMap.Data.FeatureDataTable();
                fdtX.Columns.Add("Label");
                SharpMap.Data.FeatureDataRow newFdtXRow = fdtX.NewRow();
                newFdtXRow.Geometry = xline;
                newFdtXRow["Label"] = Math.Round( XS) + "\t \n";
                fdtX.Rows.Clear(); fdtX.Rows.Add(newFdtXRow); gridxLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(fdtX);
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
                        //ForeColor = System.Drawing.Color.Green,
                        Font = new Font(FontFamily.GenericSerif, 50, FontStyle.Bold),
                        //BackColor = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0)),
                        HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                        VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                        CollisionDetection = true,
                        Enabled = true,


                        //MaxVisible = 90,
                        //MinVisible = 30
                    }
                };

                layerLabelX.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Middle;
                layerLabelX.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left;
                layerLabelX.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

                layerLabelX.Style.Offset = (new PointF((float)gridxLayer.Envelope.MinX, (float)gridxLayer.Envelope.Centre.Y));






                ///////////////////////////////////////
                map.Layers.Add(gridxLayer);
                map.Layers.Add(layerLabelX);

                gridyLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(gridYgeometries);
                gridyLayer.Style.Fill = new SolidBrush(Color.FromArgb(50, ColorTranslator.FromHtml("#101010")));
                //pen.Width = geometry.LineWidth;
                gridyLayer.Style.Outline = new Pen((Color.FromArgb(50, ColorTranslator.FromHtml("#101010"))));
                gridyLayer.Style.EnableOutline = true;
                gridyLayer.CoordinateTransformation = ctFact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84, webmercator);
                gridyLayer.ReverseCoordinateTransformation = ctFact.CreateFromCoordinateSystems(webmercator, ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);

                ////////////////////////////////////////
                var fdtY = new SharpMap.Data.FeatureDataTable();
                fdtY.Columns.Add("Label");
                SharpMap.Data.FeatureDataRow newFdtYRow = fdtY.NewRow();
                newFdtYRow.Geometry = yline;
                newFdtYRow["Label"] = Math.Round(YS) + "\t \n"; ;
                fdtY.Rows.Clear(); fdtY.Rows.Add(newFdtYRow); gridyLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(fdtY);
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
                        //ForeColor = System.Drawing.Color.Green,
                        Font = new Font(FontFamily.GenericSerif, 50, FontStyle.Bold),
                        //BackColor = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0)),
                        HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                        VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                        CollisionDetection = true,
                        Enabled = true,


                        //MaxVisible = 90,
                        //MinVisible = 30
                    }
                };

                layerLabelY.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Bottom;
                layerLabelY.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Right;
                layerLabelY.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

                layerLabelY.Style.Offset = (new PointF((float)gridyLayer.Envelope.MinX, (float)gridyLayer.Envelope.MinY));






                ///////////////////////////////////////
                map.Layers.Add(gridyLayer);
                map.Layers.Add(layerLabelY);
            }

            //zoom = zoom == 0 ? 1 : zoom;
            map.ZoomToExtents();
            //map.ZoomToExtents();
            //map.Zoom /= zoom;

            return map;
        }

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