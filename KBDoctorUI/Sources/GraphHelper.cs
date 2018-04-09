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
using LouvainCommunityPL;
using System.Diagnostics;
using Artech.Udm.Framework;
using Artech.Genexus.Common.Services;
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
            /*
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
            */
            Check = "KB Object Edges txt";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".txt";
            GenerateKBObjectEdgesTxt(Name, FileName);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });
            /*
            Check = "KB Module Graph";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".gexf";
            GenerateKBModuleGraph(Name, FileName);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });
            */
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
                return ((Functions.isRunable(obj) && ObjectsHelper.isGenerated(obj)) || (obj is Table) || (obj is ExternalObject));
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
                
                if ((Functions.isRunable(obj) && ObjectsHelper.isGenerated(obj) ) || (obj is Table ))
                {

                    objName = NombreNodo(obj);
                    string modulename = ModulesHelper.ObjectModuleName(obj);

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
                                String edge = " source='" + objRefName + "' target='" + objName + "' weight= '1.0' ";
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
                scriptFile.WriteLine("                     <edge id=" + i.ToString() + s  + " />  ");
                i += 1;
            };
            scriptFile.WriteLine("      </edges>");
            scriptFile.WriteLine("  </graph>");
            scriptFile.WriteLine("</gexf>");
            scriptFile.Close();
        }

        private static void GenerateMDGGraph(string name, string fileName)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            StringCollection aristas = new StringCollection();
            output.AddLine("Generating MDG " + name);

            string objName = "";
            StringCollection nodos = new StringCollection();
            foreach (KBObject obj in model.Objects.GetAll())
            {

                bool includedInGraph = (Functions.isRunable(obj) && ObjectsHelper.isGenerated(obj)) || (obj is Table);
                if (includedInGraph)
                {

                    objName = NombreNodo(obj);
                    string modulename = ModulesHelper.ObjectModuleName(obj);

                    if (!nodos.Contains(objName))
                    {
                        scriptFile.WriteLine("          <node id='" + objName + "' label='" + objName + "' >");
                        scriptFile.WriteLine("              <attvalues>  <attvalue for='0' value = '" + modulename + "' /> </attvalues>");
                        scriptFile.WriteLine("          </node>");
                        nodos.Add(objName);
                    }

                    foreach (EntityReference r in obj.GetReferencesTo())
                    {
                        KBObject objRef = KBObject.Get(obj.Model, r.From);
                        if ((objRef != null) && (Functions.isRunable(objRef) || objRef is Table))

                        {
                            string objRefName = NombreNodo(objRef);
                            if (objName != objRefName)
                            {
                                String edge = " source='" + objRefName + "' target='" + objName + "' weight= '1.0' ";
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
                scriptFile.WriteLine("                     <edge id=" + i.ToString() + s + " />  ");
                i += 1;
            };
            scriptFile.WriteLine("      </edges>");
            scriptFile.WriteLine("  </graph>");
            scriptFile.WriteLine("</gexf>");
            scriptFile.Close();
        }


        private static void GenerateKBObjectEdgesTxt(string name, string fileName)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            Graph g = new Graph();

            Module rootModule = kbserv.CurrentModel.GetDesignModel().RootModule;

            IOutputService output = CommonServices.Output;
            StringCollection aristas = new StringCollection();
            output.AddLine("Generating " + name);

            Dictionary<int, int> initialpartition = new Dictionary<int, int>();

            // Dictionary<string, Tuple<int,string>> dictionary = new Dictionary<string, Tuple<int,string>>();
            Dictionary<string, int> NameToId = new Dictionary<string, int>();
            Dictionary<string, string> NameToModule = new Dictionary<string, string>();
            Dictionary<int, string> IdToName = new Dictionary<int, string>();
            Dictionary<int, string> IdToModule = new Dictionary<int, string>();
            Dictionary<int, EntityKey> IdToKey = new Dictionary<int, EntityKey>();

            int objId = 0;

            foreach (KBObject obj in model.Objects.GetAll())
            {
                if (Functions.hasModule(obj) || (obj is Module))
                {
                    string objName = NombreNodo(obj);
                    string modulename = ModulesHelper.ObjectModuleName(obj);

                    try
                    {
                        objId += 1;
                        NameToId.Add(objName, objId);
                        NameToModule.Add(objName, modulename);
                        IdToName.Add(objId, objName);
                        IdToModule.Add(objId, modulename);
                        IdToKey.Add(objId, obj.Key);
                    }
                    catch (Exception e)
                    {// output.AddWarningLine("Can't add : " + objName + " Exception: " + e.Message + " " + e.InnerException);                   
                    };
                }
            }
 

            foreach (KBObject obj in model.Objects.GetAll())
            {

                string objName = "";

                if (Functions.hasModule(obj) || (obj is Module)) //((Functions.isRunable(obj) && ObjectsHelper.isGenerated(obj)) || (obj is Table))
                {
                    /*
                    objName = NombreNodo(obj);
                    string modulename = ModulesHelper.ObjectModuleName(obj);

                    try
                    {
                        objId += 1;
                        NameToId.Add(objName, objId);
                        NameToModule.Add(objName, modulename);
                        IdToName.Add(objId, objName);
                        IdToModule.Add(objId, modulename);
                        IdToKey.Add(objId, obj.Key);
                    }
                    catch (Exception e) { //output.AddWarningLine("Can't add : " + objName); 
                    };
                    */

                    //Tomo las referencias que no sean tablas. 
                    foreach (EntityReference r in obj.GetReferencesTo())
                    {
                        KBObject objRef = KBObject.Get(obj.Model, r.From);
                        if ((objRef != null) && (Functions.isRunable(objRef)) || (objRef is Table))

                        {
                            string objRefName = NombreNodo(objRef);
                            if (objName != objRefName)
                            {
                                int weight = ReferenceWeight(objRef,obj);
                                String edge = objRefName + " " + objName;
                               
                                if (!aristas.Contains(edge))
                                {
                                    aristas.Add(edge);
                                    GraboArista(g, NameToId, objRefName,objName, weight);
                                }
                                  
                            }
                        }
                    }



                }

                
            };

           
            foreach (int node in g.Nodes)
            {
                string moduleName = IdToModule[node];
                int moduleId = NameToId[moduleName];
                initialpartition.Add(node, moduleId);
            }

            output.AddLine("Before automatic modularization. TurboMQ = " + TurboMQ(g, initialpartition).ToString());

            //Empiezo modularizacion
            Stopwatch stopwatch = new Stopwatch();
           // stopwatch.Restart();
            Dictionary<int, int> partition = Community.BestPartition(g);
            output.AddLine("BestPartition: "+ stopwatch.Elapsed );
            var communities = new Dictionary<int, List<int>>();
            foreach (var kvp in partition)
            {
                List<int> nodeset;
                if (!communities.TryGetValue(kvp.Value, out nodeset))
                {
                    nodeset = communities[kvp.Value] = new List<int>();
                }
                nodeset.Add(kvp.Key);
            //    output.AddLine(kvp.Key.ToString() +"  "+kvp.Value);
            }
            output.AddLine(communities.Count + " modules found");
            Dictionary<string, int> modu = new Dictionary<string, int>();
            int counter = 0;
            foreach (var kvp in communities)
            {
                output.AddLine(String.Format("module {0}: {1} objects", counter, kvp.Value.Count));
                foreach (var objid in kvp.Value)
                {
                    var objname = IdToName[objid];
                    int cantidad = 0;
                    // output.AddLine("Module :" + counter.ToString() + " " + objname);
                    string pareja = IdToModule[objid] + " " + counter.ToString() ;
                    if (modu.ContainsKey(pareja))
                        modu[pareja] = modu[pareja] + 1;
                    else
                        modu.Add(pareja, 1);
                }
                

                var sortedDict = from entry in modu orderby entry.Value descending select entry;

                //Cantidad de modulo nuevo y modulo viejo. 
                foreach (KeyValuePair<string, int> entry in sortedDict)
                {
                 //   output.AddLine(entry.Key + " " + entry.Value.ToString());
                    Module m = new Module(model);
                    m.Name = entry.Key.Replace(" ", "_") + string.Format("_{0:yyyy_MM_dd_hh_mm_ss}",DateTime.Now);
                    output.AddLine(m.Name);
                    m.Module= kbserv.CurrentModel.GetDesignModel().RootModule;
                    m.Save();

                    foreach (var objid in kvp.Value)
                    {
                        KBObject objToChange = KBObject.Get(model, IdToKey[objid]);
                        if (objToChange != null)
                        {
                            if (objToChange is Table)
                            {
                                try
                                {
                                    KBObject trnBest = GenexusBLServices.Tables.GetBestAssociatedTransaction(model, objToChange.Key);
                                    trnBest.Module = m;
                                    trnBest.Save();
                                }
                                catch (Exception e) { output.AddErrorLine(objToChange.Name +  e.Message); }
                            }
                            else
                            {
                                try
                                {
                                    objToChange.Module = m;
                                    objToChange.Save();
                                }
                                catch (Exception e) { output.AddErrorLine(objToChange.Name + e.Message); }
                            }
                        }

                    }

                    break;
                }
                counter++;
                modu.Clear();
                

            }
           
        }

        private static Double TurboMQ(Graph g, Dictionary<int,int> partition)
        {
            return 1.0;
        }

        private static void GraboArista(Graph g,  Dictionary<string, int> NameToId, string objName, string objRefName, int weight)
        {

            int id1 = 99999;
            int id2 = 99999;

            try
            {
                id1 = NameToId[objName];
                id2 = NameToId[objRefName];

                g.AddEdge(id1, id2, weight);
            }
            catch (Exception e) {  };
        }

        private static int ReferenceWeight(KBObject obj, KBObject objRef)
        {
            if (objRef is Table)
                return 10;
            else
                return 1; 
        }

        private static string NombreNodo(KBObject obj)
        {
            string objName = ""; 
            if (obj != null)
            {
                objName = obj.Name + ":" + obj.TypeDescriptor.Name;
                if (obj.GetPropertyValue<bool>(KBObjectProperties.IsGeneratedObject))
                {

                    PatternDefinition pattern;
                    if (InstanceManager.IsInstanceObject(obj, out pattern))
                    {
                        // objName = obj.Parent.Name +":" + obj.Parent.TypeDescriptor.Name;
                        objName = obj.Parent.Name.Replace("WorkWithPlus", "");
                        objName = objName.Replace("WorkWith", "");
                        objName = objName + ":Transaction";
                    }
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
                            if ((objRef != null) && (Functions.isRunable(objRef) || objRef is Table) && modulename != ModulesHelper.ObjectModuleName(objRef))

                            {
                                String edge = "          <edge id='XXXX' source='" + modulename + "' target='" + ModulesHelper.ObjectModuleName(objRef) + "' />  ";
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
