using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace BExIS.Pmm.Entities
{
    public class GeometryInformationHistory : BaseEntity
    {
        public GeometryInformationHistory()
        {
            parser = new WKTReader();
        }
        private string _GeometryText;
        private WKTReader parser;
        public virtual long LogedId { set; get; }
        public virtual String Action { set; get; }
        public virtual DateTime LogTime { set; get; }
        public virtual int GeometryId { set; get; }
        public virtual string GeometryText
        {
            set
            {
                _GeometryText = value;
                Geometry = parser.Read(value);
            }
            get { return _GeometryText; }
        }
        public virtual string GeometryType { set; get; }
        public virtual IGeometry Geometry { set; get; }
        public virtual string Color { set; get; }
        public virtual int LineWidth { set; get; }
        public virtual byte Status { set; get; }
        public virtual string Coordinate { set; get; }
        public virtual string CoordinateType { set; get; }
        public virtual string Name { set; get; }
        public virtual string Description { set; get; }
        public virtual long PlotId { set; get; }
        public virtual string ReferencePoint { set; get; }
    }
}
