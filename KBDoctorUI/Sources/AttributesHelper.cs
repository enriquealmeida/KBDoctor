using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Diagnostics;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework.References;
using System.Linq;
using Artech.Udm.Framework;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class AttributesHelper
    {
        public static void ListAttWithoutDescription()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Attributes with incomplete description";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);


            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Table", "Attribute", "Description", "Data type", "Title", "Column Title" });
            string description;
            string titlesuggested;
            string columnTitle;
            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
            {
                description = a.Description;
                titlesuggested = a.Title;
                columnTitle = a.ColumnTitle;
                string Picture = Functions.ReturnPicture(a);

                if ((a.Description.Replace(" ", "") == a.Name) || (a.Title == a.Description) || (a.ColumnTitle == a.Description))
                {
                    string attNameLink = Functions.linkObject(a); // "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + a.Guid.ToString() + "\">" + a.Name + "</a>";

                    if (a.Title == a.Description)
                    {
                        titlesuggested = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignTitleToAttribute&attName=" + a.Name + "\">" + a.Title + "</a>";
                     
                    }

                    if (a.Description.Replace(" ", "") == a.Name)
                    {
                        description = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignDescriptionToAttribute&attName=" + a.Name + "\">" + a.Description + "</a>";
                        Table t = TablesHelper.TableOfAttribute(a);
                        writer.AddTableData(new string[] {Functions.linkObject(t), attNameLink, description, Picture, titlesuggested, columnTitle });

                    }

                    if (a.ColumnTitle == a.Description)
                    {
                        columnTitle = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignColumnTitleToAttribute&attName=" + a.Name + "\">" + a.ColumnTitle + "</a>";
                    }
      
 //                    writer.AddTableData(new string[] { attNameLink, description, Picture, titlesuggested, columnTitle });
                }

            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        public static void AttFormula()
        {
            IKBService kbserv = UIServices.KB;


            string title = "KBDoctor - Attributes Formula";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);


            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Attribute", "Description", "DataType", "Formula", "Tables", "Redundant in Tables", "#References" });
            string description;
            string titlesuggested;
            string columnTitle;
            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
            {
                description = a.Description;
                titlesuggested = a.Title;
                columnTitle = a.ColumnTitle;
                Formula formula = a.Formula;
                if (formula == null)
                {
                    //  output.AddLine(a.Name);
                }
                else {

                    output.AddLine("Formula " + a.Name);
                    string Picture = Functions.ReturnPicture(a);
                    string attNameLink = Functions.linkObject(a); 
                    string redundantInTables = "";
                    string tables = "";
                    GetTablesAttIsRedundant(kbserv.CurrentModel, a, out tables, out redundantInTables);

                    int Count = 0;
                    foreach (EntityReference entityRef in kbserv.CurrentModel.GetReferencesTo(a.Key))
                    {
                        KBObject objRef = KBObject.Get(kbserv.CurrentModel,entityRef.From);
                        if (objRef != null)
                        {
                            if (!(objRef is Table) && !(objRef is Transaction))
                                Count += 1 ;
                        }

                    }
                    if (Count==0)  //Solo es referido por transacciones o por tablas

                        
                       foreach (EntityReference entityRef in kbserv.CurrentModel.GetReferencesTo(a.Key))
                          {
                           KBObject objRef = KBObject.Get(kbserv.CurrentModel, entityRef.From);
                           if (objRef != null)
                           {
                              if (objRef is Transaction)
                               
                                foreach (KBObjectPart part in objRef.Parts)
                                {
                                    if (!(part is StructurePart))
                                        foreach (EntityReference ref2 in part.GetPartReferences())
                                        {
                                            if (ref2.To.Id == a.Id)
                                                Count += 1;
                                        }
                                }
                            }
                        }


                        writer.AddTableData(new string[] { attNameLink, description, Picture, formula.ToString(), tables, redundantInTables, Count.ToString() });
                }

            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }



   

        static void GetTablesAttIsRedundant(KBModel model, Artech.Genexus.Common.Objects.Attribute att, out string tables, out string tablesRedundant)
        {
            tables = "";
            tablesRedundant = "";
            if (att.Formula == null)      // attribute is not a formula
            {
                // return tables;
            }
            else
                //  get all references to the given ATT
                foreach (EntityReference entityRef in model.GetReferencesTo(att.Key))
                {
                    if (entityRef.From.Type == ObjClass.Table)
                    {     // Get the references from the tables
                        Table table = model.Objects.Get(entityRef.From.Type, entityRef.From.Id) as Table;
                        if (table != null)      // should be an assert()
                        {
                            // Get the TableAttribute (info of the att in the table)
                            Artech.Genexus.Common.Parts.TableAttribute tblAtt = table.TableStructure.GetAttribute(att.Key);
                            if (tblAtt != null && tblAtt.IsFormula && tblAtt.IsRedundant) // IsFormula is not strictly necessary, can be asked outside of the loop (seee above)
                                tablesRedundant += table.Name + " ";
                            else
                                tables += table.Name + " ";
                        }
                    }
                }
            //return tables;
        }



        public static void ListWithoutDomain()
        {
            IKBService kbserv = UIServices.KB;
            Dictionary<string, string> myDict = new Dictionary<string, string>();

            string title = "KBDoctor - Attributes without domain";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);


            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Attribute", "Description", "Data type", "Suggested Domains" });

            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
            {
                string Picture = Functions.ReturnPicture(a);
                bool isSubtype = Functions.AttIsSubtype(a);
                if ((a.DomainBasedOn == null) && !isSubtype)
                {
                    // search for domains with the same data type
                    output.AddLine("Procesing " + a.Name);
                    string suggestedDomains = "";
                    string value = "";
                    //busco el 
                    if (myDict.TryGetValue(Picture, out value))
                    {
                        suggestedDomains = value;
                    }
                    else
                    {
                        suggestedDomains = SuggestedDomains(kbserv, a);
                    }
                    string attNameLink = Functions.linkObject(a); // "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + a.Guid.ToString() + "\">" + a.Name + "</a>";
                    writer.AddTableData(new string[] { attNameLink, a.Description, Picture, suggestedDomains });
                }
            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        private static string SuggestedDomains(IKBService kbserv, Artech.Genexus.Common.Objects.Attribute a)
        {
            string suggestedDomains = "";

            foreach (Domain d in Domain.GetAll(kbserv.CurrentModel))
            {
                if ((a.Type == d.Type) && (a.Length == d.Length) && (a.Decimals == d.Decimals) && (a.Signed == d.Signed))
                {
                    if (suggestedDomains != "")
                    {
                        suggestedDomains += ", ";
                    }
                    suggestedDomains += "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignDomainToAttribute&attName=" + a.Name + "&domainName=" + d.Name + "\">" + d.Name + "</a>";
                }

            }

            return suggestedDomains;
        }



        public static void ListCharToVarchar()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Attributes Char that shoud be Varchar";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);

            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Attribute", "Description", "Data type", "Domain" });

            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
            {
                string Picture = Functions.ReturnPicture(a);
                if ((a.Type == Artech.Genexus.Common.eDBType.CHARACTER) && (a.Length > 35) && !Functions.AttIsSubtype(a))
                {
                    string domLink = DomainLinkFromAttribute(a);

                    string attNameLink = "";
                    attNameLink = Functions.linkObject(a); //"<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + a.Guid.ToString() + "\">" + a.Name + "</a>";

                    writer.AddTableData(new string[] { attNameLink, a.Description, Picture, domLink });
                }
            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        private static string DomainLinkFromAttribute(Artech.Genexus.Common.Objects.Attribute a)
        {
            string domLink = "";
            if (a.DomainBasedOn != null)
            {
                KBObject dom = Domain.Get(UIServices.KB.CurrentModel, a.DomainKey);
                domLink = Functions.linkObject(dom); //"<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + dom.Guid.ToString() + "\">" + dom.Name + "</a>";
            }
            else
            {
                domLink = "";
            }
            return domLink;
        }

        public static void ListVarcharToChar()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Attributes Varchar that shoud be Char";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);


            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Attribute", "Description", "Data type", "Domain" });

            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
            {
                string Picture = Functions.ReturnPicture(a);
                if (((a.Type == Artech.Genexus.Common.eDBType.VARCHAR) || (a.Type == Artech.Genexus.Common.eDBType.LONGVARCHAR)) && (a.Length <= 25) && !Functions.AttIsSubtype(a))
                {
                    string domLink = DomainLinkFromAttribute(a);

                    string attNameLink = "";

                    attNameLink = Functions.linkObject(a); //"<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + a.Guid.ToString() + "\">" + a.Name + "</a>";
                    writer.AddTableData(new string[] { attNameLink, a.Description, Picture, domLink });
                }
            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        internal static void ListAttributes()
        {
            IKBService kbserv = UIServices.KB;
           // Dictionary<string, string> myDict = new Dictionary<string, string>();

            string title = "KBDoctor - List Attributes";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);


            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Attribute", "Description", "Data type", "Domain", "Subtype" , "Title", "Column Title", "Contextual", "IsFormula"});

            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
            {
                string Picture = Functions.ReturnPicture(a);
                string domlink = a.DomainBasedOn == null ? " ": Functions.linkObject(a.DomainBasedOn);
                string superTypeName = a.SuperTypeKey==null? " ": a.SuperType.Name;
                output.AddLine("Procesing " + a.Name);
                string isFormula = a.Formula == null ? "" : "*";
                writer.AddTableData(new string[] { Functions.linkObject(a), a.Description, Picture, domlink ,  superTypeName, a.Title, a.ColumnTitle, a.ContextualTitleProperty,isFormula});
            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);
        }

        public static void ListKeyVarchar()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Attributes Varchar that is Primary Key in some table";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);

            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Attribute", "Description", "Data type", "Domain", "Tables" });

            foreach (Table t in Table.GetAll(kbserv.CurrentModel))
            {
                output.AddLine("Processing... " + t.Name);
                string objNameLink = Functions.linkObject(t); //"<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + t.Guid.ToString() + "\">" + t.Name + "</a>";


                foreach (TableAttribute attr in t.TableStructure.PrimaryKey)
                {
                    if ((attr.Attribute.Type == Artech.Genexus.Common.eDBType.VARCHAR) || (attr.Attribute.Type == Artech.Genexus.Common.eDBType.LONGVARCHAR))
                    {
                        output.AddLine("Processing " + attr.Name);
                        if (!Functions.AttIsSubtype(attr))
                        {
                            string domLink = DomainLinkFromAttribute(attr);
                            string attNameLink = Functions.linkObject(attr); //"<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + attr.Guid.ToString() + "\">" + attr.Name + "</a>";
                            string Picture = Functions.ReturnPicture(attr);
                            writer.AddTableData(new string[] { attNameLink, attr.Attribute.Description, Picture, domLink, t.Name });
                        }
                    }
                }
            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }



        public static void ListAttDescWithoutUniqueIndex()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Descriptor attribute without unique index";
            string outputFile = Functions.CreateOutputFile(kbserv, title);


            IOutputService output = CommonServices.Output;
            output.StartSection(title);

            string tabla = "";
            string atributo = "";
            string add = "";


            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Table", "Description", "Attribute", "Data Type", "" });

            // voy borrando todos los atributos que estan en alguna tabla
            foreach (Table t in Table.GetAll(kbserv.CurrentModel))
            {

                int idAttDesc = t.TableStructure.DescriptionAttribute.Id;
                bool existeIndice = false;

                tabla = t.Name + " - " + t.Description;
                atributo = t.TableStructure.DescriptionAttribute.Name;

                foreach (TableIndex index in t.TableIndexes.Indexes)
                {
                    int nroAtributos = 0;
                    bool esta = false;
                    foreach (EntityReference reference in index.Index.GetReferences(LinkType.UsedObject))
                    {
                        if (!existeIndice)
                        {
                            KBObject objRef = KBObject.Get(kbserv.CurrentModel, reference.To);
                            if (objRef is Artech.Genexus.Common.Objects.Attribute)
                            {
                                if (objRef.Id == idAttDesc)
                                    esta = true;
                                nroAtributos = nroAtributos + 1;
                            }
                            if ((esta) && (nroAtributos == 1))
                                existeIndice = true;
                        }
                    }

                }
                if (!existeIndice)
                {
                    add = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AddDescriptorIndex&tabName=" + t.Name + "\">Add index</a>";
                    writer.AddTableData(new string[] { Functions.linkObject((KBObject)t), t.Description, atributo, Functions.ReturnPicture(t.TableStructure.DescriptionAttribute.Attribute), add });
                }


            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        public static void AssignDomainToAttribute(object[] parameters)
        {
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;

                string attName = "";
                string domainName = "";

                foreach (string s in dic.Values)
                {
                    switch (cant)
                    {
                        case 1:
                            attName = s;
                            break;
                        case 2:
                            domainName = s;
                            break;
                        default:
                            break;
                    }
                    cant++;

                    if ((attName != "") && (domainName != ""))
                    {
                        Artech.Genexus.Common.Objects.Attribute a = Artech.Genexus.Common.Objects.Attribute.Get(UIServices.KB.CurrentModel, attName);
                        Domain d = Functions.DomainByName(domainName);

                        a.DomainBasedOn = d;
                        a.Save();
                    }
                }
            }
        }

        public static void AssignDescriptionToAttribute(object[] parameters, int descriptionToSet)
        {
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;

                string attName = "";
                string mensaje = "";
                PromptDescription pd;
                DialogResult dr;

                foreach (string s in dic.Values)
                {
                    if (cant == 1)
                    {
                        attName = s;
                        switch (descriptionToSet)
                        {
                            case 0:
                                mensaje = "Insert description for attribute " + attName;
                                break;
                            case 1:
                                mensaje = "Insert title for attribute " + attName;
                                break;
                            case 2:
                                mensaje = "Insert column title for attribute " + attName;
                                break;
                        }

                        pd = new PromptDescription(mensaje);
                        dr = pd.ShowDialog();

                        if (dr == DialogResult.OK)
                        {
                            Artech.Genexus.Common.Objects.Attribute a = Artech.Genexus.Common.Objects.Attribute.Get(UIServices.KB.CurrentModel, attName);
                            switch (descriptionToSet)
                            {
                                case 0:
                                    a.Description = pd.Description;
                                    break;
                                case 1:
                                    a.Title = pd.Description;
                                    break;
                                case 2:
                                    a.ColumnTitle = pd.Description;
                                    break;
                            }
                            a.Save();
                        }
                    }

                    cant++;
                }
            }
        }


        public static void AddDescriptorIndex(object[] parameters)
        {
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;

                string tabName = "";

                foreach (string s in dic.Values)
                {
                    if (cant == 1)
                    {
                        tabName = s;
                    }

                    cant++;
                }



                //   mensaje = "Insert a name for the index:";
                //   pd = new PromptDescription(mensaje);
                //   dr = pd.ShowDialog();



                //if (dr == DialogResult.OK)
                //{
                IKBService kbserv = UIServices.KB;

                Artech.Genexus.Common.Objects.Index i = Artech.Genexus.Common.Objects.Index.Create(kbserv.CurrentModel);
                try
                {
                    Table t = Table.Get(kbserv.CurrentModel, tabName);
                    if (t != null)
                    {
                        Artech.Genexus.Common.Objects.Attribute descAtt = t.TableStructure.DescriptionAttribute.Attribute;
                        if (descAtt != null)

                        {
                            i.Name = t.Name + "_" + descAtt.Name;
                            i.IndexType = Artech.Genexus.Common.IndexType.Unique;
                            i.Source = IndexSource.User;
                            i.Description = "Descriptor index";

                            IndexMember idxMember = new IndexMember(i.IndexStructure);
                            idxMember.Attribute = t.TableStructure.DescriptionAttribute.Attribute;
                            idxMember.Order = IndexOrder.Ascending; // o IndexOrder.Descending 
                            i.IndexStructure.Members.Add(idxMember);

                            // Agregarlo a la tabla 
                            t.TableIndexes.Indexes.Add(new TableIndex(t.TableIndexes, i));
                            t.Save();
                            MessageBox.Show("Index " + i.Name + " was successfully created.");
                        }

                    }
                }
                catch (GxException gxe)
                {
                    MessageBox.Show(gxe.Message, "Could not create index", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                //}
            }
        }

        public static void AttInOneTrnOnly()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Attributes in Transaction only";
            string outputFile = Functions.CreateOutputFile(kbserv, title);


            IOutputService output = CommonServices.Output;
            output.StartSection(title);


            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Attribute", "can delete", "Description", "Data type", "Tables", "Transactions" });

            // grabo todos los atributos en una colección
            output.AddLine("Loading attributes..");
            List<Artech.Genexus.Common.Objects.Attribute> attTodos = new List<Artech.Genexus.Common.Objects.Attribute>();
            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
            {
                attTodos.Add(a);
            }

            output.AddLine("Procesiong objects..");

            foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
            {
                // output.AddLine("Procesing .. " + obj.Name);

                if ((!(obj is Transaction) && !(obj is Table) && !(obj is SDT)) || obj.GetPropertyValue<bool>("idISBUSINESSCOMPONENT"))
                {
                    foreach (EntityReference reference in obj.GetReferences(LinkType.UsedObject))
                    {
                        KBObject objRef = KBObject.Get(kbserv.CurrentModel, reference.To);
                        if (objRef is Artech.Genexus.Common.Objects.Attribute)
                        {
                            Artech.Genexus.Common.Objects.Attribute a = (Artech.Genexus.Common.Objects.Attribute)objRef;
                            attTodos.Remove(a);
                        }
                    }
                }
            }


            foreach (Artech.Genexus.Common.Objects.Attribute a in attTodos)
            {
                output.AddLine("Procesing .. " + a.Name);

                string attNameLink = Functions.linkObject(a); // "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + a.Guid.ToString() + "\">" + a.Name + "</a>";
                string Picture = Functions.ReturnPicture(a);
                string table = TableOfAtt(a);
                bool canDelete;
                string trns = TransactionsOfAtt(a, out canDelete);
                string strCanDelete = canDelete ? "Yes" : "";
                writer.AddTableData(new string[] { attNameLink, strCanDelete, a.Description, Picture, table, trns });

            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        public static void ReplaceDomain()
        {
            IKBService kbserv = UIServices.KB;

            bool success = true;
            string title = "KBDoctor - Replace domain ";
            IOutputService output = CommonServices.Output;


            ReplaceDomain rd = new ReplaceDomain();
            DialogResult dr = new DialogResult();
            dr = rd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                output.StartSection(title);
                Domain od = Functions.DomainByName(rd.originalDomainName);
                Domain ud = Functions.DomainByName(rd.destDomainName);
                if (od != null && ud != null)
                {

                    foreach (EntityReference reference in od.GetReferencesTo()) // LinkType.UsedObject))
                    {
                        KBObject objRef = KBObject.Get(UIServices.KB.CurrentModel, reference.From);
                        output.AddLine("Procesing " + objRef.Name);
                        if (objRef is Artech.Genexus.Common.Objects.Attribute)
                        {

                            Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)objRef;
                            att.DomainBasedOn = ud;
                            att.Save();
                        }
                        else
                        {
                            VariablesPart vp = objRef.Parts.Get<VariablesPart>();
                            if (vp != null)
                            {
                                foreach (Variable v in vp.Variables)
                                {
                                    if (v.DomainBasedOn == od && !v.IsStandard)
                                    {
                                        v.DomainBasedOn = ud;
                                    }
                                }
                                objRef.Save();
                            }
                            else
                            {
                                output.AddLine("Replace " + od.Name + " domain manually in object " + objRef.Name);
                                success = false;
                            }


                        }
                    }
                }
                output.EndSection(title,success);
            }

            
        }

        public static void ListDomain()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Domains";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);


            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Domain", "Description", "Data type", "Att references", "Other References" });
            string description;
            string titlesuggested;
            string columnTitle;
            foreach (Domain d in Domain.GetAll(kbserv.CurrentModel))
            {
                description = d.Description;
                string Picture = Functions.ReturnPictureDomain(d);
                int attReferences = 0;
                int otherReferences = 0;
                foreach (EntityReference r in d.GetReferencesTo())
                {
                    KBObject objRef = KBObject.Get(UIServices.KB.CurrentModel, r.From);
                    if (objRef is Artech.Genexus.Common.Objects.Attribute)
                        attReferences += 1;
                    else
                        otherReferences += 1;
                }
                writer.AddTableData(new string[] { Functions.linkObject(d), description, Picture, attReferences.ToString(), otherReferences.ToString() });
            }
            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);
        }


        internal static Artech.Genexus.Common.Objects.Attribute Att2SubType(Artech.Genexus.Common.Objects.Attribute a)
        {
            IKBService kbserv = UIServices.KB;
            EntityKey superTypeKey = a.SuperTypeKey;
            Artech.Genexus.Common.Objects.Attribute superType = a.GetPropertyValue<Artech.Genexus.Common.Objects.Attribute>("SuperType");
            if (superTypeKey != null && (superType == null || superType.Key != superTypeKey))
            {
                superType = Artech.Genexus.Common.Objects.Attribute.Get(kbserv.CurrentModel, superTypeKey.Id);
            }
            return superType;
        }

        public static string TableOfAtt(Artech.Genexus.Common.Objects.Attribute att)
        {
            IKBService kbserv = UIServices.KB;
            string tables = "";
            foreach (EntityReference entityRef in kbserv.CurrentModel.GetReferencesTo(att.Key))
            {
                if (entityRef.From.Type == ObjClass.Table)
                {     // Get the references from the tables
                    Table table = kbserv.CurrentModel.Objects.Get(entityRef.From.Type, entityRef.From.Id) as Table;
                    if (table != null)      // should be an assert()
                    {
                        tables += table.Name + " ";
                    }
                }
            }
            return tables;
        }

        public static string TransactionsOfAtt(Artech.Genexus.Common.Objects.Attribute att, out bool canDelete)
        {
            IKBService kbserv = UIServices.KB;
            string transactions = "";
            canDelete = true;
            foreach (EntityReference entityRef in kbserv.CurrentModel.GetReferencesTo(att.Key))
            {
                if (entityRef.From.Type == ObjClass.Transaction)
                {     // Get the references from the tables
                    Transaction Trn = kbserv.CurrentModel.Objects.Get(entityRef.From.Type, entityRef.From.Id) as Transaction;
                    if (Trn != null)      // should be an assert()
                    {
                        transactions += Trn.Name + " ";
                        if (Trn.GetPropertyValue<bool>(Properties.TRN.GenerateObject))
                            canDelete = false;
                    }
                }
            }
            return transactions;
        }





    }
}
