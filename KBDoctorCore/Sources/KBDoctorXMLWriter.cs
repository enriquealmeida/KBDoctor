using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Artech.Architecture.UI.Framework.Services;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    class KBDoctorXMLWriter: XmlTextWriter
    {
        public KBDoctorXMLWriter(string archivo, Encoding enc): base(archivo, enc)
        {
        }

        public void AddHeader(string title)
        {
            //Salvo JS a directorio de trabajo
            //WriteJScripttoDir();

            WriteStartElement("html");
            WriteAttributeString("xmlns", "http://www.w3.org/1999/xhtml");

            WriteStartElement("head");
            WriteElementString("title", title);
           
            WriteStartElement("style");
                WriteAttributeString("type", "text/css");
                WriteString("h2 {font-family: Tahoma; color: Navy;}");
                WriteString("table {border-width: thin; border-style: solid; font-family: Tahoma; font-size: 10pt;}");
                WriteString("th {background-color: Gray;   color: White;}");
            WriteEndElement();   // style
  
            WriteRaw(@"<script type='text/javascript' src='jquery_latest.js'></script> ");
            WriteRaw(@"<script type='text/javascript' src='jquery_tablesorter.js'></script>");
            WriteRaw(@"<script type='text/javascript'> $(function() { $('table').tablesorter() } ); </script>");   
            
            WriteEndElement();   // head

            WriteStartElement("body");
            AddTitle(title);
       
            WriteStartElement("table");
            WriteAttributeString("width", "100%");
            WriteAttributeString("style","BORDER-COLLAPSE: collapse");
            WriteAttributeString("borderColor","#cdaaad");
            WriteAttributeString("cellSpacing","0");
            WriteAttributeString("cellPadding","0");
            WriteAttributeString("border","1");
        }

        public void AddFooter()
        {

            WriteEndElement();   // tbody
            WriteEndElement();   // table
            WriteEndElement();   // body
            WriteEndElement();   // html
        }

        public void AddTableFooterOnly()
        {  
            WriteEndElement();
        }

        public void AddTableHeader(string[] titles)
        {
            WriteStartElement("thead");
            WriteStartElement("tr");
            foreach (string s in titles)
            {
                WriteElementString("th", s);
            }
            WriteEndElement();   // tr
            WriteEndElement(); // thead
           
            WriteStartElement("tbody");
            WriteAttributeString("style","{vertical-align: top}");
           
        }

        public void AddTableData(string[] datos)
        {
            WriteStartElement("tr");
            foreach (string s in datos)
            {
                WriteStartElement("td");
                WriteRaw(s);
                WriteEndElement();  // td
            }
            WriteEndElement();   // tr
        }

        public void AddTitle(string title)
        { 
            WriteElementString("h2", title); 
        }

       /* private static void WriteJScripttoDir()
        {
            IKBService kbserv = UIServices.KB;
            string outputFile = kbserv.CurrentKB.UserDirectory + @"\jquery_latest.js";
            File.WriteAllText(outputFile, StringResources.jquery_latest);
            
            outputFile = kbserv.CurrentKB.UserDirectory + @"\jquery_tablesorter.js";
            File.WriteAllText(outputFile, StringResources.jquery_tablesorter);

        }*/
    }
}
