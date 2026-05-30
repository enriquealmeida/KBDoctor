using System;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Services;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.CustomTypes;

using Artech.Common.Diagnostics;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.Language.Services;
using Artech.Architecture.Language.Parser;
using Artech.Architecture.Common;
using Artech.Architecture.Common.Services;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.BL.Framework.Services;
using GeneXus.Server.Contracts;
using Artech.Architecture.Language.ComponentModel;

using Artech.Udm.Framework;
using Concepto.Packages.KBDoctorCore.Sources;
using System.Threading;
using Artech.Genexus.Common.Parts.WebForm;
using API = Concepto.Packages.KBDoctorCore.Sources.API;
using Concepto.Packages.KBDoctor.Sources;

using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.Form;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Packages.Patterns.Objects;
using Artech.Patterns.WorkWithDevices.Helpers;
using Artech.Patterns.WorkWithDevices;
using Artech.Common.Helpers.Reflection;


namespace Concepto.Packages.KBDoctor
{
    partial class ObjectsHelper
    {
        public static void CalculateCheckSum()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            SpecificationListHelper helper = new SpecificationListHelper(kbserv.CurrentModel.Environment.TargetModel);

            string title = "KBDoctor - Generate objects in text format";
            KBDoctorOutput.StartSection(title);

            string fechahora = String.Format("{0:yyyy-MM-dd-HHmm}", DateTime.Now);
            string newDir = KBDoctorHelper.ObjComparerDirectory(kbserv) + @"\OBJ-" + fechahora + @"\";
            Directory.CreateDirectory(newDir);

            StringCollection objReferenced = new StringCollection();


            int iObj = 0;
            foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
            {
                if (!(obj is Domain | obj is Artech.Genexus.Common.Objects.Theme | obj is DataView | obj is Index | obj is KBCategory | obj is DataProvider | obj is Artech.Genexus.Common.Objects.Menubar | obj is DataView | obj is Diagram | obj is Folder | obj is Image |
                    obj is ExternalObject | obj is ThemeClass | obj is ThemeColor | obj is DataViewIndex | obj is Artech.Architecture.Common.Objects.Module  | obj is Artech.Genexus.Common.Objects.DesignSystem | obj is Artech.Genexus.Common.Objects.Group))
                {
                    iObj += 1;
                    if ((iObj % 200) == 0)
                    {
                        KBDoctorOutput.Message( obj.GetFullName());
                    }
                    WriteObjectToJsonFile(obj, newDir);
                }
            }

            bool success = true;
            KBDoctorOutput.EndSection(title, success);
        }

        public static void GenerateLocationXML()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            SpecificationListHelper helper = new SpecificationListHelper(kbserv.CurrentModel.Environment.TargetModel);
            string title = "KBDoctor - Genenrate Location.xml";

            string outputFile = kbserv.CurrentKB.UserDirectory + @"\Location.xml";
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            KBDoctorOutput.StartSection(title);
            KBDoctorOutput.Message( "Generate Location.xml template in " + outputFile);

            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.WriteStartElement("GXLocations");

            KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");
            foreach (ExternalObject eobj in kbserv.CurrentModel.GetObjects<ExternalObject>())
            {
                if (eobj.GetPropertyValueString(Properties.EXO.Type) == "WSDL")
                {

                    writer.WriteStartElement("GXLocation");
                    string locationName = eobj.QualifiedName.ToString().Replace(".", "_");
                    writer.WriteAttributeString("name", locationName);
                    writer.WriteStartElement("Common");

                    writer.WriteElementString("Host", "www.server.com");
                    writer.WriteElementString("Port", "80");
                    writer.WriteElementString("BaseUrl", "/baseUrl/");
                    writer.WriteElementString("Secure", "");
                    writer.WriteElementString("Timeout", "60");
                    writer.WriteElementString("CancelOnError", "1");

                    writer.WriteElementString("Proxyserverhost", "www.proxy.com");
                    writer.WriteElementString("Proxyserverport", "80");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("HTTP");
                    writer.WriteStartElement("Authentication");
                    writer.WriteElementString("Authenticationmethod", "0=Basic;1:Digest,2:NTML,3:Kerberos");
                    writer.WriteElementString("Authenticationrealm", "domain");
                    writer.WriteElementString("Authenticationuser", "user");
                    writer.WriteElementString("Authenticationpassword", "pass");
                    writer.WriteEndElement();
                    writer.WriteStartElement("Proxyauthentication");
                    writer.WriteElementString("Proxyauthenticationmethod", "0=Basic;1:Digest,2:NTML,3:Kerberos");
                    writer.WriteElementString("Proxyauthenticationrealm", "proxydomain");
                    writer.WriteElementString("Proxyauthenticationuser", "proxyuser");
                    writer.WriteElementString("Proxyauthenticationpassword", "proxypass");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

            }
            writer.WriteEndElement();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            KBDoctorOutput.EndSection(title, success);

        }

        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
        private static void WriteObjectToTextFile(KBObject obj, string newDir)
        {
            string name = Regex.Replace(obj.GetFullName(), "[':/\\\\ ]", "_");
            string FileName = newDir + name + ".json";

            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(FileName))
                {
                    file.WriteLine($"Object: {obj.Name} \nType: {obj.TypeName} \nDescription : {obj.Description} \nModule: {obj.Module.Name} \n");

                    RulesPart rp = obj.Parts.Get<RulesPart>();
                    if (rp != null)
                    {
                        file.WriteLine("=== RULES ===");
                        file.WriteLine(rp.Source);
                    }

                    switch (obj.TypeDescriptor.Name)
                    {
                        case "Attribute":
                            HandleAttribute(obj, file);
                            break;
                        case "Procedure":
                            HandleProcedure(obj, file);
                            break;
                        case "Transaction":
                            HandleTransaction(obj, file);
                            break;
                        case "WorkPanel":
                            HandleWorkPanel((WorkPanel)obj, file);
                            break;
                        case "WebPanel":
                            HandleWebPanel((WebPanel)obj, file);
                            break;
                        case "WebComponent":
                            HandleWebComponent((WebPanel)obj, file);
                            break;
                        case "Table":
                            HandleTable((Table)obj, file);
                            break;
                        case "SDT":
                            HandleSDT((Artech.Genexus.Common.Objects.SDT)obj, file);
                            break;
                        default:
                            break;
                    }
                    HandleReferences(obj, file);
                }
            }

            catch (IOException e)
            {
                Console.WriteLine("Error al escribir en el archivo: " + e.Message);
            }

        }

        private static void HandleReferences(KBObject objRef, StreamWriter file)
        {
            file.WriteLine("==== Referencias ==========");
            foreach (EntityReference r in objRef.GetReferencesTo())
            {
                KBObject obj = KBObject.Get(objRef.Model, r.From);

                if ((obj != null) && (Functions.isRunable(obj)) && (obj != objRef))
                {
                    file.WriteLine(obj.Name + " hace referencia a " + objRef.Name + " en una relacion de tipo  " + r.ReferenceType.ToString() + " " + r.LinkType.ToString() + " " + r.LinkTypeInfo.ToString());
                    file.WriteLine(objRef.Name + " es referenciado por " + objRef.Name + " en una relacion de tipo  " + r.ReferenceType.ToString() + " " + r.LinkType.ToString() + " " + r.LinkTypeInfo.ToString() );

                }
            }
        }


        private static void HandleAttribute(KBObject obj, StreamWriter file)
        {
            Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)obj;

            file.WriteLine("DataType: " + Utility.FormattedTypeAttribute(att));
            file.WriteLine(att.Formula?.ToString() ?? "");
        }

        private static void HandleProcedure(KBObject obj, StreamWriter file)
        {
            ProcedurePart pp = obj.Parts.Get<ProcedurePart>();
            if (pp != null)
            {
                file.WriteLine("=== PROCEDURE SOURCE ===");
                file.WriteLine(pp.Source);
            }
        }

        private static void HandleTransaction(KBObject obj, StreamWriter file)
        {
            StructurePart sp = obj.Parts.Get<StructurePart>();
            if (sp != null)
            {
                file.WriteLine("=== STRUCTURE ===");
                file.WriteLine(sp.ToString());
            }

            HandleEventsPart(obj.Parts.Get<EventsPart>(), file);
        }

        private static void HandleWorkPanel(WorkPanel obj, StreamWriter file)
        {
            HandleEventsPart(obj.Parts.Get<EventsPart>(), file);
        }

        private static void HandleWebPanel(WebPanel obj, StreamWriter file)
        {
            HandleEventsPart(obj.Parts.Get<EventsPart>(), file);
        }

        private static void HandleWebComponent(WebPanel obj, StreamWriter file)
        {
            HandleEventsPart(obj.Parts.Get<EventsPart>(), file);
        }

        private static void HandleTable(Table obj, StreamWriter file)
        {
            file.WriteLine("=== TABLE STRUCTURE ===");
            foreach (TableAttribute attr in obj.TableStructure.Attributes)
            {
                StringBuilder line = new StringBuilder();
                line.Append(attr.IsKey ? "*" : " ");
                line.Append(attr.Name).Append("  ").Append(attr.GetPropertiesObject().GetPropertyValueString("DataTypeString"));
                line.Append("-").Append(attr.GetPropertiesObject().GetPropertyValueString("Formula"));
                line.Append(" DataType:  " + Utility.FormattedTypeAttribute(attr));

                if (attr.IsExternalRedundant)
                    line.Append(" External_Redundant");

                line.Append(" Null=").Append(attr.IsNullable);
                if (attr.IsRedundant)
                    line.Append(" Redundant");

                file.WriteLine(line.ToString());
            }
        }

        private static void HandleSDT(Artech.Genexus.Common.Objects.SDT obj, StreamWriter file)
        {
            if (obj != null)
            {
                file.WriteLine("===SDT STRUCTURE ===");
                ListStructure(obj.SDTStructure.Root, 0, file);
            }
        }

        private static void HandleEventsPart(EventsPart ep, StreamWriter file)
        {
            if (ep != null)
            {
                file.WriteLine("=== EVENTS SOURCE ===");
                file.WriteLine(ep.Source);
            }
        }




private static void WriteObjectToJsonFile(KBObject obj, string newDir)
    {
        string name = Regex.Replace(obj.GetFullName(), "[':/\\\\ ]", "_");
        string FileName = newDir + name + ".json";

        try
        {
            using (StreamWriter file = new StreamWriter(FileName))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                writer.Formatting = Formatting.Indented; // Para una mejor legibilidad del archivo JSON

                // Crear un objeto an�nimo con toda la informaci�n necesaria
                var objData = new
                {
                    ObjectName = obj.Name,
                    Type = obj.TypeName,
                    Description = obj.Description,
                    Module = obj.Module.Name,
                    Rules = GetRulesPart(obj),
                    References = GetReferences(obj),
                    SpecificData = GetSpecificData(obj),
                    Parts = GetParts(obj)
                };

                // Serializar y escribir en el archivo
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, objData);
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("Error al escribir en el archivo: " + e.Message);
        }
    }

        private static object GetParts(KBObject obj)
        {
            foreach (var parte in obj.Parts)
            {
                // Aqu� puedes acceder a las propiedades de cada parte
                return  parte.Name;

            }
            return null;
        }

        private static object GetReferences(KBObject obj)
        {

            foreach (EntityReference r in obj.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(obj.Model, r.To);

                if ((obj != null) && (Functions.isRunable(obj)) && (obj != objRef))
                {
                    return new
                    {
                        Title = "Referencia a",
                        ObjetoReferenciado = objRef.Name,
                        TipoObjetoReferenciado = objRef.TypeDescriptor,
                        TipoReferencia = r.ReferenceType.ToString(),
                        TipoLink = r.LinkTypeInfo.ToString(),
                        TipoLinkInfo = r.LinkTypeInfo.ToString()
                    };
                }

            }
            return null;
        }
        private static object GetSpecificData(KBObject obj)
        {
            switch (obj.TypeDescriptor.Name)
        {
            case "Table":
                return GetTableData((Table)obj);
            case "SDT":
                return GetSDTData((Artech.Genexus.Common.Objects.SDT)obj);

            default:
                return null;
        }
    }

        private static object GetSDTData(Artech.Genexus.Common.Objects.SDT obj)
        {
            if (obj != null)
            {
                return new
                {
                    Title = "Structure",
                    Source = "" //ListStructure(obj.SDTStructure.Root, 0, file)
                };
            }
            return null;
        }

        private static object GetTableData(Table obj)
        {
            StringBuilder line = new StringBuilder();
            foreach (TableAttribute attr in obj.TableStructure.Attributes)
            {

                line.Append(attr.IsKey ? "*" : " ");
                line.Append(attr.Name).Append("  ").Append(attr.GetPropertiesObject().GetPropertyValueString("DataTypeString"));
                line.Append("-").Append(attr.GetPropertiesObject().GetPropertyValueString("Formula"));
                line.Append(" DataType:  " + Utility.FormattedTypeAttribute(attr));

                if (attr.IsExternalRedundant)
                    line.Append(" External_Redundant");

                line.Append(" Null=").Append(attr.IsNullable);
                if (attr.IsRedundant)
                    line.Append(" Redundant");

            }
            return new { line };
        }

        // Asumo que ya tienes funciones como GetAttributeData, GetProcedureData, etc., que
        // devuelven datos espec�ficos formateados para ser parte del objeto JSON.
        // Similar a los m�todos HandleAttribute, HandleProcedure, etc., pero retornando
        // objetos o estructuras en lugar de escribir directamente a un archivo.

        // Nota: Deber�s implementar m�todos como GetRulesPart, GetReferences, y otros para cada tipo de objeto,
        // que deber�n devolver la informaci�n correspondiente en formato adecuado para ser serializada a JSON.
        private static object GetRulesPart(KBObject obj)
        {
            RulesPart rp = obj.Parts.Get<RulesPart>();
            if (rp != null)
            {
                return new
                {
                    Title = "RULES",
                    Source = rp.Source
                };
            }

            return null;
        }


        internal static void ListCommitOnExit()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            bool commitOnExit;
            string title = "KBDoctor - Commit on Exit = Yes";
            string objNameLink;

            KBDoctorOutput.StartSection(title);
            try
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter("CommitOnExit.txt");
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Name", "Description", "UpdateDB?" });

                foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
                {
                    ICallableObject callableObject = obj as ICallableObject;
                    if (callableObject != null && isGenerated(obj))
                    {
                        KBDoctorOutput.Message("Processing " + obj.TypeDescriptor.Name + " " + obj.Name);

                            object aux = obj.GetPropertyValue("CommitOnExit");
                            commitOnExit = ((aux != null) && (aux.ToString() == "Yes"));
                        string objName = obj.Name;
                        string updateDB = "";
                        bool updateDBbool = CleanKBHelper.ObjectUpdateDB(obj);

                        if (commitOnExit && obj.IsPropertyDefault("CommitOnExit") && isGenerated(obj))
                            {

                                if (updateDBbool)
                                {
                                updateDB = "YES";
                                }

                                objNameLink = Functions.linkObject(obj);
                                writer.AddTableData(new string[] { obj.TypeDescriptor.Name, objNameLink, obj.Description, updateDB });
                            }
                        if (obj is Procedure || obj is Transaction)
                        {
                            file.WriteLine(obj.TypeDescriptor.Name + ":" + obj.Name + "  CommitOnExit: " + commitOnExit.ToString() + "  UpdateDB: " + updateDBbool.ToString());
                        }
                    }
                }

                writer.AddFooter();
                writer.Close();
                file.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);

                bool success = true;
                KBDoctorOutput.EndSection(title, success);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }

        }


        private static void ListStructure(SDTLevel level, int tabs, System.IO.StreamWriter file)
        {
            WriteTabs(tabs, file);
            file.Write(level.Name);
            if (level.IsCollection)
                file.Write(", collection: {0}", level.CollectionItemName);
            file.WriteLine();

            foreach (var childItem in level.GetItems<SDTItem>())
                ListItem(childItem, tabs + 1, file);
            foreach (var childLevel in level.GetItems<SDTLevel>())
                ListStructure(childLevel, tabs + 1, file);
        }

        private static void ListItem(SDTItem item, int tabs, System.IO.StreamWriter file)
        {
            WriteTabs(tabs, file);
            string dataType = Utility.ReturnFormattedType(item.Type, item.Length, item.Decimals, item.Signed);
            file.WriteLine("{0}, {1}, {2} {3}", item.Name, dataType, item.Description, (item.IsCollection ? ", collection " + item.CollectionItemName : ""));
        }

        private static void WriteTabs(int tabs, System.IO.StreamWriter file)
        {
            while (tabs-- > 0)
                file.Write('\t');
        }

        private static void WriteCopyObject(IOutputService output, KBObject obj, StringCollection tableOperation, KBObjectCollection objMarked, string mainstr, string Dircopia)
        {
            IKBService kbserv = UIServices.KB;
            string linea = String.Format(@"XCOPY /y/d {0}bin\{1}.dll %{2}%\bin\", Dircopia, obj.Name, mainstr);

            if (!(obj is Procedure) && !(obj is WorkPanel) && !(tableOperation.Contains(linea)))
            {
                KBDoctorOutput.Message( linea);
                KBDoctorOutput.Message( String.Format(@"XCOPY /y/d {0}{1}.js  %{2}%\ ", Dircopia, obj.Name, mainstr));
                tableOperation.Add(linea);
            }

            objMarked.Add(obj);

            foreach (EntityReference reference in obj.GetReferences())
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.To);

                if ((objRef != null) && !objMarked.Contains(objRef))
                {
                    if (!Utility.IsMain(objRef))
                    {
                        if (objRef is Transaction || objRef is WorkPanel || objRef is WebPanel || objRef is Artech.Genexus.Common.Objects.Menubar || objRef is Procedure || objRef is DataProvider || objRef is DataSelector)
                            WriteCopyObject(output, objRef, tableOperation, objMarked, mainstr, Dircopia);
                    }
                    else
                        return;

                }


            }
        }

        public static void ObjectMigration()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;



            foreach (WorkPanel obj in WorkPanel.GetAll(kbserv.CurrentModel))
            {
                string objName = obj.Name.ToUpper();
                if  (objName == "WINV501")
                      {
                    CreateNewWebPanel(obj);
                }
            }



        }
        public static void CreateNewWebPanel(WorkPanel workPanel)
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            KBModel kbModel = kbserv.CurrentModel;

            WebPanel newWebPanel = new WebPanel(kbModel);
            newWebPanel.Name = workPanel.Name + "_v18";

            // Copy Variables
            VariablesPart workPanelVariables = workPanel.Parts.Get<VariablesPart>();
            if (workPanelVariables != null)
            {
                foreach (Variable variable in workPanelVariables.Variables)
                {
                    if (!variable.IsStandard)
                    {
                        newWebPanel.Variables.Add(variable);
                    }
                }
            }

            // Copy Conditions
            ConditionsPart workPanelConditions = workPanel.Parts.Get<ConditionsPart>();
            if (workPanelConditions != null)
            {
                newWebPanel.Conditions.Source = workPanelConditions.Source;
            }

            // Copy Rules
            RulesPart workPanelRules = workPanel.Parts.Get<RulesPart>();
            if (workPanelRules != null)
            {
                newWebPanel.Rules.Source = workPanelRules.Source;
            }

            // Copy Events
            EventsPart workPanelEvents = workPanel.Parts.Get<EventsPart>();
            if (workPanelEvents != null)
            {
                newWebPanel.Events.Source = workPanelEvents.Source;
            }
            // Copy Help
            HelpPart workPanelHelp = workPanel.Parts.Get<HelpPart>();
            if (workPanelHelp != null)
            {
                newWebPanel.Help.HtmlContent = workPanelHelp.HtmlContent;
            }

            // Copy Documentation
            DocumentationPart workPanelDocumentation = workPanel.Parts.Get<DocumentationPart>();
            if (workPanelDocumentation != null)
            {
                newWebPanel.Documentation.Page = workPanelDocumentation.Page;
            }

            // Process Form
            WinFormPart winForm = workPanel.Parts.Get<WinFormPart>();
            WebFormPart webForm = newWebPanel.Parts.Get<WebFormPart>();
            if (winForm != null && webForm != null)
            {
                ProcessFormDefinitions(winForm.MyDocument, output, webForm);
            }

            try
            {
                newWebPanel.Save();
                output.AddLine("WebPanel created successfully: " + newWebPanel.Name);
            }
            catch (Exception e)
            {
                output.AddErrorLine("Error saving WebPanel: " + e.Message);

                newWebPanel.Name = workPanel.Name + "_v18_error";
                newWebPanel.Events.Source = "/* " + workPanelEvents.Source + " */";
                newWebPanel.Rules.Source = "/*" +  workPanelRules.Source + " */";

                try { newWebPanel.Save();
                    output.AddLine("WebPanel created successfully: " + newWebPanel.Name);
                }
                catch (Exception ex) { output.AddErrorLine("Error saving second WebPanel: " + ex.Message); }
            }
        }


        // TODO: Sin referencias textuales encontradas; revisar si se usa manualmente antes de eliminar.
        public static void CreateNewPanel(WorkPanel workPanel)
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            KBModel kbModel = kbserv.CurrentModel;

            SDPanel newPanel = new SDPanel(kbModel);
            PatternInstanceElement panel = newPanel.PatternPart.PanelElement;



            newPanel.Name = workPanel.Name + "_Pv18";
            var variablesPart = WorkWithDevicesSources.GetVariablesPartForPanel(panel);


            // Copy Variables
            VariablesPart workPanelVariables = workPanel.Parts.Get<VariablesPart>();
            if (workPanelVariables != null)
            {
                foreach (Variable variable in workPanelVariables.Variables)
                {
                    if (!variable.IsStandard)
                    {
                        variablesPart.Add(variable);
                    }
                }
            }


            // Copy Events
            var eventsSource = panel.Attributes.GetPropertyValue<string>(InstanceAttributes.Panel.Events);


            EventsPart workPanelEvents = workPanel.Parts.Get<EventsPart>();
            if (workPanelEvents != null)
            {
                panel.Attributes.SetPropertyValue(InstanceAttributes.Panel.Events, workPanelEvents.ToString());
            }
            try
            {
                newPanel.Save();
                output.AddLine("WebPanel created successfully: " + newPanel.Name);
            }
            catch (Exception e)
            {
                output.AddErrorLine("Error saving WebPanel: " + e.Message);
            }

        }


        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
        private static string ProcessRulesSource(string source)
        {
            source = source.Replace("search(", "//search(");
            return source;
        }

        private static void ProcessFormDefinitions(FormDocument formDocument, IOutputService output, WebFormPart webForm)
        {

            foreach (var definition in formDocument.Definitions
                .Where(def => def.Value.FormClass.ToString() == "Text"))
            {
                var sortedElements = definition.Value.Canvas.Elements
                    .OrderBy(e => e.Properties.GetPropertyValue<int>("Top"))
                    .ThenBy(e => e.Properties.GetPropertyValue<int>("Left"));

                foreach (var element in sortedElements)
                {
                    ProcessElement(element, output, webForm);
                }
            }

        }

        private static void ProcessElement(FormElement element, IOutputService output, WebFormPart webForm)
        {
            // List of property names to ignore
            var ignoredProperties = new HashSet<string>
           {
               "Font", "ForeColorText", "FormType", "FromDefault", "FromStyle",
               "LinesFont", "LinesForeColorText", "TitleFont", "TitleForeColorText"
           };

            // Process the element here
            output.AddLine($"Element: {element.Name} {element.Type.ToString()}");

            // Iterate through all properties of the element and display their content
            foreach (var pt in element.Properties.GetPropertiesDescriptors())
            {
                if (!pt.IsDefaultValue && !ignoredProperties.Contains(pt.Name))
                {
                    output.AddLine($"      Property: {pt.Name} = {pt.Value}");
                }
            }

            // Recursively process child elements
            foreach (FormElement childElement in element.Children)
            {
                ProcessElement(childElement, output, webForm);
            }
        }
        public static void ObjectsLegacyCode()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string[] legacyCode = new string[] { "Object", "Description", "Type", " call", " udp", " create", ".false", ".true", "new", "defined", "delete", ".and.", ".or.", ".not.", ".like." };
            string titulo = "KBDoctor - Objects - Legacy Code";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, titulo);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);

                KBDoctorOutput.StartSection(titulo);
                writer.AddHeader(titulo);
                writer.AddTableHeader(legacyCode);
                int objWithLegacyCode = 0;

                foreach (KBObject obj in Utility.EditableObjects(UIServices.KB.CurrentModel.Objects.GetAll()))
                {

                    if (obj is Transaction || obj is WebPanel || obj is Procedure || obj is WorkPanel)
                    {

                        if (isGenerated(obj))
                        {
                            KBDoctorOutput.Message( obj.Name);

                            string source = ObjectSource(obj);
                            source = Functions.RemoveEmptyLines(source);
                            string sourceWOComments = Functions.ExtractComments(source);
                            bool hasLegacyCode = false;
                            string[] data = new string[legacyCode.Length];
                            data[0] = Functions.linkObject(obj);
                            data[1] = obj.Description;
                            data[2] = obj.TypeDescriptor.Name;

                            for (int i = 3; i < legacyCode.Length; i++) //empieza en la columan 3 para saltear nombre de objeto, descripcion y tipo
                            {
                                if (sourceWOComments.Contains(legacyCode[i].ToUpper()))
                                {
                                    data[i] = "   **  ";
                                    hasLegacyCode = true;
                                }
                                else
                                {
                                    data[i] = "";
                                }
                            }
                            if (hasLegacyCode)
                            {
                                writer.AddTableData(data);
                                objWithLegacyCode += 1;
                            }
                        }
                    }
                }

                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                KBDoctorOutput.EndSection(titulo, success);
                Functions.AddLineSummary(titulo + ".txt", objWithLegacyCode.ToString());
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(titulo, success);
            }

        }

        public static void EditLegacyCodeToReplace()
        {
            string filename = CreateFileWithTextToTeplace();
            Process.Start("notepad.exe", filename);

        }

        public static void EditReviewObjects()
        {
            API.InitializeIniFile(UIServices.KB.CurrentKB);
            string filename = UIServices.KB.CurrentKB.UserDirectory + "\\KBDoctor.ini";
            Process.Start("notepad.exe", filename);

        }
        private static string CreateFileWithTextToTeplace()
        {
            IKBService kbserv = UIServices.KB;

            string filename = kbserv.CurrentKB.UserDirectory + @"\Replace.txt";
            if (!File.Exists(filename))
            {
                File.WriteAllText(filename, Comparer.Replace);
            }

            return filename;
        }

        public static void ChangeLegacyCode()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;

            Regex[] re4 = new Regex[9]
                {
                new Regex(@"([\b]*)([^.])([c][a][l][l][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]", RegexOptions.Compiled | RegexOptions.IgnoreCase ),
                new Regex(@"([\b]*)([^.])([u][d][p][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]" , RegexOptions.Compiled | RegexOptions.IgnoreCase ),
                new Regex(@"([\b]*)([^.])([c][r][e][a][t][e][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]" , RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"([\b]*)([^.])([l][i][n][k][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                new Regex(@"([\b]*)([^.])([c][a][l][l][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]", RegexOptions.Compiled | RegexOptions.IgnoreCase ),
                new Regex(@"([\b]*)([^.])([u][d][p][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]" , RegexOptions.Compiled | RegexOptions.IgnoreCase ),
                new Regex(@"([\b]*)([^.])([c][r][e][a][t][e][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]" , RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"([\b]*)([^.])([l][i][n][k][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                new Regex(@"^([c][a][l][l][(])([\s]*[a-z][0-9a-z_]+)(,|\))", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline)
                };
            string[] re4Replace = new string[9]
                {
                @"$1$2$4.$3",@"$1$2$4.$3",@"$1$2$4.$3",@"$1$2$4.$3",
                @"$1$2$4.$3)",  @"$1$2$4.$3)",  @"$1$2$4.$3)",  @"$1$2$4.$3)",
                @"$2.$1"
                };

            string[] lines = System.IO.File.ReadAllLines(CreateFileWithTextToTeplace());
            string[,] leg = new string[100, 2];
            int j = 0;
            foreach (string line in lines)
            {
                if (!line.StartsWith("#") && line.Contains("|"))
                {
                    string[] cam = line.Split('|');

                    leg[j, 0] = cam[0];
                    leg[j, 1] = cam[1];
                    j += 1;
                }
            }

            string titulo = "KBDoctor - Change code to improve readability";
            KBDoctorOutput.StartSection(titulo);

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            KBModel kbModel = UIServices.KB.CurrentModel;

            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WorkPanel>());

            foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                if (obj is Transaction || obj is WebPanel || obj is Procedure || obj is WorkPanel)
                {
                    if (isGenerated(obj) && !isGeneratedbyPattern(obj))
                    {
                        string source = ObjectSource(obj);
                        string newSource = source;

                        KBDoctorOutput.Message( "Object " + obj.Name);

                        string callers = ChangeUDPCallWhenNecesary(obj);
                        //Cambio expresiones regulares
                        for (int i = 0; i < 9; i++)
                        {
                            newSource = re4[i].Replace(newSource, re4Replace[i]);
                        }

                        //Solo cambio si NO tiene codigo nativo. Cambiar codigo nativo puede traer lios.
                        if (!(newSource.ToLower().Contains("java") || newSource.ToLower().Contains("csharp")))
                        {
                            for (int i = 0; i < (leg.Length) / 2; i++)
                            {
                                newSource = newSource.Replace(leg[i, 0], leg[i, 1], StringComparison.InvariantCultureIgnoreCase);
                            }

                            if (source != newSource)
                            {
                                try
                                {
                                    KBDoctorOutput.Message( "..Saving.." + obj.Name);
                                    SaveNewSource(obj, newSource);
                                }
                                catch (Exception e)
                                {
                                    KBDoctorOutput.Error(e.Message);
                                    KBDoctorOutput.Error("========= newsource ===============");
                                    KBDoctorOutput.Message( newSource);
                                    success = false;
                                };

                            }

                        }
                    }
                }
            }

            KBDoctorOutput.EndSection(titulo, success);

        }

        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
        private static string ReplaceLegacyCode(string newSource, string original, string changeto)
        {
            newSource = newSource.Replace(original, changeto, StringComparison.CurrentCultureIgnoreCase);
            return newSource;
        }

        private static void SaveNewSource(KBObject obj, string newSource)
        {

            if (obj is Procedure)
            {
                Procedure p = (Procedure)obj;
                p.ProcedurePart.Source = newSource;
                p.Save();

            }
            if (obj is WebPanel)
            {
                WebPanel p = (WebPanel)obj;
                p.Events.Source = newSource;
                p.Save();
            }
            if (obj is Transaction)
            {
                Transaction p = (Transaction)obj;
                p.Events.Source = newSource;
                p.Save();
            }
            if (obj is WorkPanel)
            {
                WorkPanel p = (WorkPanel)obj;
                p.Events.Source = newSource;
                p.Save();
            }
        }

        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
        private static string ReplaceOneLegacy(string myString, string v)
        {
            int from, to;
            string newString = "";

            from = myString.ToLower().IndexOf(v);
            to = from + v.Length;
            string aux = myString.Substring(to, myString.Length - to).Trim();
            aux = aux.Replace("(", "");
            string objeto = "";
            foreach (var token in aux.Split(new char[] { ',', ')', ' ' }))
            {
                objeto = token;
                break;
            }
            if (objeto != "" && objeto.Substring(0, 1) != "&") //Call Dinamico si esta llamando a una variable
            {
                aux = aux.Substring(objeto.Length, aux.Length - objeto.Length);
                if (aux != "" && aux.Substring(0, 1) == ",")
                {
                    aux = aux.Substring(1, aux.Length - 1);
                }

                newString = myString.Substring(0, from) + " " + objeto + "." + v.Trim() + "(" + aux;
            }

            return newString;

        }

    }
}
