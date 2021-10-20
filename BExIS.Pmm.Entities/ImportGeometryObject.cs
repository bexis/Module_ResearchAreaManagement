using System.Collections.Generic;

namespace BExIS.Pmm.Entities
{
    public class ImportGeometryObject
    {
        public ImportGeometryObject(int index, GeometryInformation geometry, string action, bool status)
        {
            Index = index;
            Geometry = geometry;
            Action = action;
            UploadSuccessful = status;
        }
        public int Index { set; get; }
        public GeometryInformation Geometry { set; get; }
        public string Action { set; get; }
        public bool UploadSuccessful { set; get; }
    }
}
