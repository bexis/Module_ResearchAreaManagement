using BExIS.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.Tcd.Helpers
{
    public class ServerInformation
    {
        private const string sourceFile = "serverInformation.xml";
        private XmlDocument requirementXmlDocument = null;
        private String serverAddress = "";

        public String ServerAddress
        {
            get { return serverAddress; }
        }

        public ServerInformation()
        {
            requirementXmlDocument = new XmlDocument();
            Load();
        }

        private void Load()
        {
            try
            {

                string filepath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("TCD"), sourceFile);

                if (FileHelper.FileExist(filepath))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(filepath);
                    requirementXmlDocument = xmlDoc;
                    XmlNodeList serverNodes = requirementXmlDocument.GetElementsByTagName("server");

                    foreach (XmlNode child in serverNodes)
                    {
                        serverAddress = child.SelectSingleNode("address").InnerText;
                   }
                }
            } catch(Exception e)
            {
                serverAddress = "";
            }
            
        }

    }
}
