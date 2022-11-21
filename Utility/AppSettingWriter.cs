using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Ecm.Utility
{
    public class AppSettingWriter
    {
        private string _configFileName;
        private XmlDocument _configDocument = new XmlDocument();

        public AppSettingWriter(string configFile)
        {
            _configFileName = configFile;
            _configDocument.Load(_configFileName);
        }

        public void SetValue(string key, string value)
        {
            string xpath = string.Format("/configuration/connectionStrings/add[@name=\"{0}\"]", key);
            XmlNode node = _configDocument.SelectSingleNode(xpath);

            if (node == null)
            {
                XmlElement element = _configDocument.CreateElement("add");
                element.SetAttribute("name", key);
                element.SetAttribute("connectionString", value);

                xpath = "/configuration/connectionStrings";
                XmlNode root = _configDocument.DocumentElement.SelectSingleNode(xpath);

                root.AppendChild((XmlNode)element);
            }
            else
            {
                node.Attributes.GetNamedItem("connectionString").Value = value;
            }

            _configDocument.Save(_configFileName);
        }

    }

}