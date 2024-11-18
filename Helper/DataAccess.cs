using BExIS.Modules.Pmm.UI.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace BExIS.Modules.Pmm.UI.Helper
{
    public class DataAccess
    {
        /// <summary>
        /// Get lanu data
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns>Data table with comp dataset depents on dataset id.</returns>
        public static DataTable GetData(string datasetId, long structureId, ServerInformation serverInformation)
        {
            string link = serverInformation.ServerName + "/api/data/" + datasetId;
            HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
            request.Headers.Add("Authorization", "Bearer " + serverInformation.Token);
            // request.ContentType = "application/json";

            DataStructureObject dataStructureObject = GetDataStructure(structureId, serverInformation);

            DataTable data = new DataTable();
            foreach (var variable in dataStructureObject.Variables)
            {
                DataColumn col = new DataColumn(variable.Label);
                col.DataType = System.Type.GetType("System." + variable.SystemType);
                col.AllowDBNull = true;
                data.Columns.Add(col);
            }

            string a = "";
            try
            {
                // Get response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {

                        var records = csvReader.GetRecords<dynamic>();

                        foreach (var r in records)
                        {

                            var l = Enumerable.ToList(r);

                            DataRow dr = data.NewRow();
                            String[] row = new String[4];
                            for (int j = 0; j < data.Columns.Count; j++)
                            {
                                a = l[j].Value;
                                if (String.IsNullOrEmpty(l[j].Value) || l[j].Value == "NA")
                                    dr[data.Columns[j].ColumnName] = DBNull.Value;
                                else
                                    dr[data.Columns[j].ColumnName] = l[j].Value;

                            }

                            data.Rows.Add(dr);
                        }

                        //JavaScriptSerializer js = new JavaScriptSerializer();
                        //var objText = reader.ReadToEnd();
                        //lanuData = (DataTable)JsonConvert.DeserializeObject(objText, (typeof(DataTable)));

                        response.Close();
                    }
                }
            }
            catch (Exception e)
            {
                string t = a;
            }


            return data;
        }

        public static DataStructureObject GetDataStructure(long structId, ServerInformation serverInformation)
        {
            string link = serverInformation.ServerName + "/api/structures/" + structId;
            HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
            request.Headers.Add("Authorization", "Bearer " + serverInformation.Token);

            DataStructureObject dataStructureObject = new DataStructureObject();

            try
            {
                // Get response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var objText = reader.ReadToEnd();
                        dataStructureObject = Newtonsoft.Json.JsonConvert.DeserializeObject<DataStructureObject>(objText);
                    }
                }
            }
            catch (Exception e)
            {
                string error = "Not data" + e.InnerException;
            }

            return dataStructureObject;
        }

        /// <summary>
        /// Get information abbout dataset
        /// 
        /// </summary>
        /// <returns>Information like version, title etc</returns>
        public static DatasetObject GetDatasetInfo(string datasetId, ServerInformation serverInformation)
        {
            string link = serverInformation.ServerName + "/api/dataset/" + datasetId;
            HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
            request.Headers.Add("Authorization", "Bearer " + serverInformation.Token);

            DatasetObject datasetObject = new DatasetObject();

            try
            {
                // Get response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var objText = reader.ReadToEnd();
                        datasetObject = Newtonsoft.Json.JsonConvert.DeserializeObject<DatasetObject>(objText);
                    }
                }
            }
            catch (Exception e)
            {
                string error = "Not data" + e.InnerException;
            }

            return datasetObject;
        }
    }
}