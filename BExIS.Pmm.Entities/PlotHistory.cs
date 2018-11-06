using System.Collections.Generic;
using Vaiona.Entities.Common;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using System;

namespace BExIS.Pmm.Entities
{
    public class PlotHistory : BaseEntity
    {
        public PlotHistory()
        {
            Geometries = new List<GeometryInformation>();
            parser = new WKTReader();
        }
        private string _GeometryText;
        private WKTReader parser;
        public virtual long LogedId { set; get; }
        public virtual string Action { set; get; }
        public virtual DateTime LogTime { set; get; }
        public virtual string PlotId { set; get; }
        public virtual string PlotType { set; get; }
        public virtual string Latitude { set; get; }
        public virtual string Longitude { set; get; }
        public virtual ICollection<GeometryInformation> Geometries { set; get; }
        //public virtual PlotArea Area { set; get; }
        public virtual string GeometryType { set; get; }
        public virtual string GeometryText {
            set {
                _GeometryText = value;
                Geometry = parser.Read(value);
            }
            get { return _GeometryText; }
        }
        public virtual IGeometry Geometry { set; get; }
        public virtual string Coordinate { set; get; }
        public virtual string CoordinateType { set; get; }
        public virtual byte Status { set; get; }
        public virtual string RefrencePoint { set; get; }
    }
}
