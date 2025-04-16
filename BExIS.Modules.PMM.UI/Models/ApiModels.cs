using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.Transform.Output;
using System.Collections.Generic;

namespace BExIS.Modules.Pmm.UI.Models
{
    /// <summary>
    /// Class to store server information to access data via API
    /// 
    /// </summary>
    /// <returns></returns>
    public class ServerInformation
    {
        public string ServerName { get; set; }
        public string UsernamePassword { get; set; }

    }

    /// <summary>
    /// Class to store dataset information receive via api
    /// 
    /// </summary>
    /// <returns></returns>
    public class DataStructureObject
    {
        public int id { get; set; }
        public string title { get; set; }
        public string desciption { get; set; }
        public bool inUse { get; set; }
        public List<Variable> variables { get; set; }
    }

    public class Constraint
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }

    public class Unit
    {
        public int id { get; set; }
        public string name { get; set; }
        public string abbrevation { get; set; }
        public string description { get; set; }
        public Dimension dimension { get; set; }
        public string measurementSystem { get; set; }
    }

    public class Dimension
    {
        public string name { get; set; }
        public string description { get; set; }
        public string specification { get; set; }
    }

    public class Variable
    {
        public int id { get; set; }
        public string label { get; set; }
        public string description { get; set; }
        public bool isOptional { get; set; }
        public string dataType { get; set; }
        public string systemType { get; set; }
        public string displayPattern { get; set; }
        public Unit unit { get; set; }
        public List<object> missingValues { get; set; }
        public Template template { get; set; }
        public List<object> meanings { get; set; }
        public List<Constraint> constraints { get; set; }
    }


    /// <summary>
    /// Class to store dataset information receive via api
    /// 
    /// </summary>
    /// <returns></returns>
    public class DatasetObject
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string VersionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DataStructureId { get; set; }
        public string MetadataStructureId { get; set; }
        public AdditionalInformations AdditionalInformations { get; set; }
        public DatasetObject()
        {
            AdditionalInformations = new AdditionalInformations();
        }
    }



    /// <summary>
    /// Store AdditionalInformations for Dataset Object
    /// 
    /// </summary>
    /// <returns></returns>
    public class AdditionalInformations
    {
        public string Title { get; set; }

    }


    public class PostApiPlotCount
    {
        public string[] plots { get; set; }
    }


    public class Feature
    {
        public int id { get; set; }
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<List<List<double>>> coordinates { get; set; }
    }

    public class Properties
    {
        public object extra { get; set; }
        public string plotid { get; set; }
        public int status { get; set; }
        public string latitude { get; set; }
        public string plottype { get; set; }
        public string longitude { get; set; }
        public int versionno { get; set; }
        public string coordinate { get; set; }
        public string geometrytype { get; set; }
        public string coordinatetype { get; set; }
        public string referencepoint { get; set; }
        public string plotType { get; set; }
        public string habitat { get; set; }
    }

    public class Plots
    {
        public string type { get; set; }
        public List<Feature> features { get; set; }
    }



}
