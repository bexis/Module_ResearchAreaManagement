using System.Collections.Generic;

namespace BExIS.Pmm.Entities
{
    public class ImportPlotObject
    {
        public ImportPlotObject(int index, Plot plot, string action, bool status)
        {
            Index = index;
            Plot = plot;
            Action = action;
            Status = status;
        }
        public int Index { set; get; }
        public Plot Plot { set; get; }
        public string Action { set; get; }
        public bool Status { set; get; }
    }
}
