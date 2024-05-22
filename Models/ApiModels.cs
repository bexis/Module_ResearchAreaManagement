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
        public string Token { get; set; }

    }

    /// <summary>
    /// Class to store dataset information receive via api
    /// 
    /// </summary>
    /// <returns></returns>
    public class DataStructureObject
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string inUse { get; set; }
        public string Structured { get; set; }
        public List<Variables> Variables { get; set; }
    }

    public class Variables
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string isOptional { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }
        public string SystemType { get; set; }
        public string AttributeName { get; set; }
        public string AttributeDescription { get; set; }
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
