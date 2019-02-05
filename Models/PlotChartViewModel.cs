﻿using BExIS.Pmm.Entities;
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
        public IList<Plot> plotList { set; get; }
        public IList<int> gridsize { set; get; }
        public IList<String> geometryType { set; get; }
        public IList<String> colorList { set; get; }
        public IList<String> coordinateType { set; get; }
        public Boolean isAdmin { set; get; }
        public string allPlots { set; get; }
    }
}