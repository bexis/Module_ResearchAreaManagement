using BExIS.Pmm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.PMM.Models
{
    public class PlotChartViewModel
    {
        public string ImageSource { set; get; }
        public Plot selectedPlot { set; get; }
        public IOrderedEnumerable<Plot> plotlist { get; set; }
        public IOrderedEnumerable<Plot> plotlistNew { get; internal set; }
        public List<string> plotlist_new { set; get; }
        public IList<int> gridsize { set; get; }
        public IList<String> geometryType { set; get; }
        public IList<String> colorList { set; get; }
        public IList<String> coordinateType { set; get; }
        public Boolean isAdmin { set; get; }
        public string allPlots { set; get; }
    }
}