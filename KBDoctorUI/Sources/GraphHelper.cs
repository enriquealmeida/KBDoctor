using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Packages.Patterns.Definition;
using Artech.Packages.Patterns.Engine;
using Artech.Udm.Framework.References;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class GraphHelper
    {
        public static void GenerateGraph()
        {
         

            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Generate Graph ";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            KBModel model = kbserv.CurrentKB.DesignModel;
            int ATTNAME_LEN = model.GetPropertyValue<int>("ATTNAME_LEN");
            int TBLNAME_LEN = model.GetPropertyValue<int>("TBLNAME_LEN");
            int OBJNAME_LEN = model.GetPropertyValue<int>("OBJNAME_LEN");


            IOutputService output = CommonServices.Output;
            output.StartSection(title);

            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Check", "File" });

            string Check = "";
            string Name = "";
            string FileName = "";
            
            Check = "KB Table Graph";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".gexf";
            GenerateKBTableGraph(Name, FileName); 
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });

            Check = "KB Object Graph";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".gexf";
            GenerateKBObjectGraph(Name, FileName);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });
            
            Check = "KB Module Graph";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".gexf";
            GenerateKBModuleGraph(Name, FileName);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);
        }

        private static void GenerateKBConexComponentGraph(string name)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            int i = 0;
            string fileName = name + i + ".gexf";
            KBObjectCollection visited = new KBObjectCollection();

            IEnumerable<KBObject> listObjNode = (from r in model.Objects.GetAll() where isNode(r) select model.Objects.Get(r.Key));

            foreach (KBObject obj in listObjNode)
                {
                   
                if (!visited.Contains(obj)) 
                    {
                        i += 1;
                        visited.Add(obj);
                        IOutputService output = CommonServices.Output;
                        output.AddLine("");
                        output.AddWarningLine("START Componente conexo " + i.ToString());
                        
                        ComponenteConexo(obj, visited);

                        output.AddWarningLine("FIN Componente conexo " + i.ToString());
                    }
            }




         }

        private static bool isNode(KBObject obj)
        {
            if (obj != null)
                return ((Functions.isRunable(obj) && KBDoctorCore.Sources.Utility.isGenerated(obj)) || (obj is Table) || (obj is ExternalObject));
            else
                return false;
        }

        private static void ComponenteConexo(KBObject obj, KBObjectCollection visited)
        {
           
            foreach (EntityReference r in obj.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(obj.Model, r.From);
                VisitNode(visited, obj, objRef);
            }

            foreach (EntityReference r in obj.GetReferences())
            {
                KBObject objRef = KBObject.Get(obj.Model, r.To);
                VisitNode(visited, obj, objRef);
            }
        }

        private static void VisitNode(KBObjectCollection visited, KBObject obj, KBObject objRef)
        {
            IOutputService output = CommonServices.Output;
            if (isNode(objRef))
            {
                output.AddLine( NombreNodo(obj) + ";" + NombreNodo(objRef));

                if (!visited.Contains(objRef))
                {
                    visited.Add(objRef);
                    ComponenteConexo(objRef, visited);
                    
                }
            }
        }




        private static void GenerateKBTableGraph(string name, string fileName)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            StringCollection aristas = new StringCollection();
            output.AddLine("Generating " + name);

            scriptFile.WriteLine("<?xml version = '1.0' encoding = 'UTF-8'?>");

            scriptFile.WriteLine("<gexf xmlns='http://www.gexf.net/1.2draft'  xmlns:viz='http://www.gexf.net/1.2draft/viz' ");
            scriptFile.WriteLine("     xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.gexf.net/1.2draft http://www.gexf.net/1.2draft/gexf.xsd' version = '1.2' > ");

            scriptFile.WriteLine("  <graph mode = 'static' defaultedgetype = 'directed' > ");
            scriptFile.WriteLine("<attributes class='node'> <attribute id='0' title = 'module' type = 'string' />  </attributes >");

            scriptFile.WriteLine("      <nodes>");
            foreach (Table t in Table.GetAll(model))
            {
                scriptFile.WriteLine("          <node id='" + t.Name + "' label='" + t.Name + "' >");
                scriptFile.WriteLine("              <attvalues>  <attvalue for='0' value = '" + TablesHelper.TableModule(model, t).Name + "' /> </attvalues>");
                scriptFile.WriteLine("          </node>");
                foreach (TableRelation relation in t.SuperordinatedTables)
                {
                    String edge = "          <edge id='XXXX' source='" + relation.BaseTable.Name + "' target='" + relation.RelatedTable.Name + "' />  ";
                    if (!aristas.Contains(edge))
                        aristas.Add(edge);
                }
            };
            scriptFile.WriteLine("      </nodes>");

            //Grabo las aristas
            scriptFile.WriteLine("      <edges>");

            int i = 0;
            foreach (String s in aristas)
            {
                string s2 = s.Replace("XXXX", i.ToString());
                scriptFile.WriteLine("          " + s2);
                i += 1;
            };
            scriptFile.WriteLine("      </edges>");
            scriptFile.WriteLine("  </graph>");
            scriptFile.WriteLine("</gexf>");
            scriptFile.Close();
        }

        private static void GenerateKBTableGraphSatsuma(string name, string fileName)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            StringCollection aristas = new StringCollection();
            output.AddLine("Generating " + name);
           
            scriptFile.WriteLine("      </edges>");
            scriptFile.WriteLine("  </graph>");
            scriptFile.WriteLine("</gexf>");
            scriptFile.Close();
        }

        private static void GenerateKBObjectGraph(string name, string fileName)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            StringCollection aristas = new StringCollection();
            output.AddLine("Generating " + name);

            scriptFile.WriteLine("<?xml version = '1.0' encoding = 'UTF-8'?>");

            scriptFile.WriteLine("<gexf xmlns='http://www.gexf.net/1.2draft'  xmlns:viz='http://www.gexf.net/1.2draft/viz' ");
            scriptFile.WriteLine("     xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.gexf.net/1.2draft http://www.gexf.net/1.2draft/gexf.xsd' version = '1.2' > ");

            scriptFile.WriteLine("  <graph mode = 'static' defaultedgetype = 'directed' > ");
            scriptFile.WriteLine("<attributes class='node'> <attribute id='0' title = 'module' type = 'string' />  </attributes >");

            scriptFile.WriteLine("      <nodes>");

            string objName = "";
            StringCollection nodos = new StringCollection();
            foreach (KBObject obj in model.Objects.GetAll())
            {
                
                if ((Functions.isRunable(obj) && KBDoctorCore.Sources.Utility.isGenerated(obj) ) || (obj is Table ))
                {

                    objName = NombreNodo(obj);
                    string modulename = ModulesHelper.ObjectModule(obj);

                    if (!nodos.Contains(objName))
                    {
                        scriptFile.WriteLine("          <node id='" + objName + "' label='" + objName +  "' >");
                        scriptFile.WriteLine("              <attvalues>  <attvalue for='0' value = '" + modulename + "' /> </attvalues>");
                        scriptFile.WriteLine("          </node>");
                        nodos.Add(objName);
                    }

                    foreach (EntityReference r in obj.GetReferencesTo())
                    {
                        KBObject objRef = KBObject.Get(obj.Model, r.From);
                        if ((objRef != null) && (Functions.isRunable(objRef) || objRef is Table) )

                        {
                            string objRefName = NombreNodo(objRef);
                            if (objName != objRefName)
                            {
                                String edge = "          <edge id='XXXX' source='" + objRefName + "' target='" + objName + "' />  ";
                                if (!aristas.Contains(edge))
                                    aristas.Add(edge);
                            }
                        }
                    }
                }
            };
            scriptFile.WriteLine("      </nodes>");

            //Grabo las aristas
            scriptFile.WriteLine("      <edges>");

            int i = 0;
            foreach (String s in aristas)
            {
                string s2 = s.Replace("XXXX", i.ToString());
                scriptFile.WriteLine("          " + s2);
                i += 1;
            };
            scriptFile.WriteLine("      </edges>");
            scriptFile.WriteLine("  </graph>");
            scriptFile.WriteLine("</gexf>");
            scriptFile.Close();
        }

        private static string NombreNodo(KBObject obj)
        {
            IOutputService output = CommonServices.Output;
            string objName = obj.Name + ":" + obj.TypeDescriptor.Name ;
            if (obj.GetPropertyValue<bool>(KBObjectProperties.IsGeneratedObject))
            {
            
                PatternDefinition pattern;
                if (InstanceManager.IsInstanceObject(obj, out pattern))
                {
                    objName = obj.Parent.Name +":" + obj.Parent.TypeDescriptor.Name;
                }
            }
            
            return objName ;
        }

        private static void GenerateKBModuleGraph(string name, string fileName)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            StringCollection aristas = new StringCollection();
            output.AddLine("Generating " + name);

            scriptFile.WriteLine("<?xml version = '1.0' encoding = 'UTF-8'?>");

            scriptFile.WriteLine("<gexf xmlns='http://www.gexf.net/1.2draft'  xmlns:viz='http://www.gexf.net/1.2draft/viz' ");
            scriptFile.WriteLine("     xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.gexf.net/1.2draft http://www.gexf.net/1.2draft/gexf.xsd' version = '1.2' > ");

            scriptFile.WriteLine("  <graph mode = 'static' defaultedgetype = 'directed' > ");
            scriptFile.WriteLine("<attributes class='node'> <attribute id='0' title = 'module' type = 'string' />  </attributes >");

            scriptFile.WriteLine("      <nodes>");

            foreach (Module mdl in Module.GetAll(model))

            {

                string modulename = mdl.Name;

                scriptFile.WriteLine("          <node id='" + mdl.Name + "' label='" + mdl.Description + "' >");
                scriptFile.WriteLine("              <attvalues>  <attvalue for='0' value = '" + mdl.Name + "' /> </attvalues>");
                scriptFile.WriteLine("          </node>");

                foreach (KBObject obj in mdl.GetAllMembers())
                {
                    if (obj is Procedure || obj is Table)
                    {
                        foreach (EntityReference r in obj.GetReferences())
                        {
                            KBObject objRef = KBObject.Get(obj.Model, r.To);
                            if ((objRef != null) && (Functions.isRunable(objRef) || objRef is Table) && modulename != ModulesHelper.ObjectModule(objRef))

                            {
                                String edge = "          <edge id='XXXX' source='" + modulename + "' target='" + ModulesHelper.ObjectModule(objRef) + "' />  ";
                                if (!aristas.Contains(edge))
                                    aristas.Add(edge);
                            }
                        }
                    }
                }
            }
            scriptFile.WriteLine("      </nodes>");

            //Grabo las aristas
            scriptFile.WriteLine("      <edges>");

            int i = 0;
            foreach (String s in aristas)
            {
                string s2 = s.Replace("XXXX", i.ToString());
                scriptFile.WriteLine("          " + s2);
                i += 1;
            };
            scriptFile.WriteLine("      </edges>");
            scriptFile.WriteLine("  </graph>");
            scriptFile.WriteLine("</gexf>");
            scriptFile.Close();
        }
    }


   

}
