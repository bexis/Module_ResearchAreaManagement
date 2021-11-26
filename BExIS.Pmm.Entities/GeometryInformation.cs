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
    public class GeometryInformation : BaseEntity
    {
        public GeometryInformation()
        {
            parser = new WKTReader();
            parser.DefaultSRID = 54012;

            parser.HandleSRID = true;
            parser.DefaultSRID = 54012;

        }
        private string _GeometryText;
        private WKTReader parser;
        public virtual int GeometryId { set; get; }
        public virtual string GeometryText
        {
            set
            {
                _GeometryText = value;
                Geometry = parser.Read(value);
                Geometry.SRID = 54012;
                
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
        public virtual Plot Plot { set; get; }

        public virtual DateTime Date { set; get; }
    }
}
