using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Diagnostics;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Common.Collections;
using Concepto.Packages.KBDoctorCore.Sources;
using System.Collections.Specialized;
using System.ComponentModel;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects.Themes;

namespace Concepto.Packages.KBDoctor
{
    class ThemeHelper
    {

        public static void ClassNotInTheme()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Class not in Theme";

            string outputFile = Functions.CreateOutputFile(kbserv, title);


            //IOutputService output = CommonServices.Output;
            KBDoctorOutput.StartSection(title);

            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Object", "Class", "Error" });

            //Cargo todas las clases de todos los theme de la KB. 
            StringCollection ThemeClasses = LoadThemeClasses();

            StringCollection UsedClasses = LoadUsedClasses();


            foreach (string sd in UsedClasses)
            {
                if (!ThemeClasses.Contains(sd))
                {
                    writer.AddTableData(new string[] { "", sd, "Application Class not in theme" });
                    KBDoctorOutput.Message( "Application Class not in theme " + sd);

                }
                else
                {
                    ThemeClasses.Remove(sd);
                }
            }


            writer.AddTableData(new string[] { "-----------------", "--------------", "---" });
            foreach (string ss in ThemeClasses)
                if (!UsedClasses.Contains(ss))
                {
                    writer.AddTableData(new string[] { "", ss, "Class not referenced" });
                    KBDoctorOutput.Message("Class not referenced in application " + ss);
                }
            writer.AddTableData(new string[] { "-------", "-----------------", "--------------" });
            writer.AddFooter();
            writer.Close();
            KBDoctorOutput.EndSection(title,true);

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            
        }


        private static bool VeoSiClassEstaContenidaEnAlgunaClassDelTheme(StringCollection ThemeClasses, string miclstr)
        {
            bool classEstaEnElTheme = false;
            try
            {

                foreach (string thmcls in ThemeClasses)
                {
                    if (thmcls.Contains(miclstr))
                    {
                        classEstaEnElTheme = true;
                        break;
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(); };
            return classEstaEnElTheme;
        }

        private static StringCollection LoadThemeClasses()
        {
            StringCollection ThemeClasses = new StringCollection();
            foreach (Theme thm in Theme.GetAll(UIServices.KB.CurrentModel))
            {
                KBDoctorOutput.Message( "Procesing theme .." + thm.Name);
                ThemeStylesPart part = thm.Parts.Get<ThemeStylesPart>();
                foreach (Artech.Genexus.Common.Objects.Themes.ThemeStyle thmclass in part.GetAllStyles())
                {
                    string thmstr = thmclass.Name;
                    if (!ThemeClasses.Contains(thmstr) && (!(thmstr.Contains("Dragging") || thmstr.Contains("AcceptDrag") || thmstr.Contains("NoAcceptDrag")))) //Excluyo clases especiales
                    {
                        ThemeClasses.Add(thmstr.ToLower());
                    }
                }
            }
            return ThemeClasses;
        }

        public static void LoadAndCheckUsedClasses(IKBService kbserv, IOutputService output, StringCollection UsedClasses, StringCollection ThemeClasses, KBDoctorXMLWriter writer)
        {

            int cant = 0;

            foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
            {
                if ((cant % 100) == 0)
                {
                    KBDoctorOutput.Message( "Procesing.." + cant.ToString() + " objects ");
                }
                cant += 1;
                if (((obj is Transaction) || (obj is WebPanel)) && (obj.GetPropertyValue<bool>(Properties.TRN.GenerateObject)))
                {
                    WebFormPart webForm = obj.Parts.Get<WebFormPart>();
                    foreach (IWebTag tag in WebFormHelper.EnumerateWebTag(webForm))
                    {
                        if (tag.Properties != null)
                        {
                            PropertyDescriptor prop = tag.Properties.GetPropertyDescriptorByDisplayName("Class");
                            if (prop != null)
                            {
                                //arreglar acan cancela con la Evo3. 
                                ThemeClassReferenceList miclasslist = new ThemeClassReferenceList();
                                //    try
                                //    {
                                // miclasslist = (ThemeClassReferenceList)prop.GetValue(new object());
                                //  }
                                // catch (Exception e) {
                                //     KBDoctorOutput.Error("LoadAndCheckUsedClasses:" + e.Message + " " + e.InnerException);
                                //     throw e;
                                // };
                                foreach (ThemeClass miclass in miclasslist.GetThemeClasses(obj.Model))
                                {
                                    if (miclass != null)
                                    {
                                        string miclstr = miclass.Name.ToLower();
                                        if (!UsedClasses.Contains(miclstr))
                                        {
                                            UsedClasses.Add(miclstr);
                                        }
                                        if (!ThemeClasses.Contains(miclstr))
                                        {
                                            bool classEstaEnElTheme = VeoSiClassEstaContenidaEnAlgunaClassDelTheme(ThemeClasses, miclstr);
                                            if (!classEstaEnElTheme)
                                            {
                                                string objName = obj.Name;
                                                KBDoctorOutput.Message( " Object : " + obj.Name + " reference class " + miclstr + " which not exist in Theme");
                                                string objNameLink = Functions.linkObject(obj);
                                                writer.AddTableData(new string[] { objNameLink, miclstr, " does not exist in theme" });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public static StringCollection LoadUsedClasses()
        {

            StringCollection UsedClasses = new StringCollection();
            KBModel model = UIServices.KB.CurrentModel;

            foreach (ThemeClass themeClass in ThemeClass.GetAll(model))
            {
                KBDoctorOutput.Message("Class:" + themeClass.Name);
                int cant = 0; 
                foreach (EntityReference entityRef in themeClass.GetReferencesTo()) {
                   // KBDoctorOutput.Message("ObjRef: From:" + entityRef.From.Type.ToString() + " To: " + entityRef.To.Type.ToString());
                    KBObject objRefTo = KBObject.Get(model, entityRef.To);
                    KBObject objRefFrom = KBObject.Get(model, entityRef.From);

                    
                    if (objRefFrom != null && !(objRefFrom is Theme) && !(objRefFrom is ThemeClass))
                    {
                        KBDoctorOutput.Message("ObjRefFrom: :" + objRefFrom.Name + " Type : " + objRefFrom.TypeName);
                        cant++;
                    }
                    
                }
                KBDoctorOutput.Message("Class Name: :" + themeClass.Name + " Referencias: " + cant.ToString());
            }


            return UsedClasses;

            

           
            
        }


        /*
        public static void LoadUsedClasses(ollection UsedClasses)
        {

            int cant = 0;

            foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
            {
                if ((cant % 100) == 0)
                {
                    KBDoctorOutput.Message( "Procesing.." + cant.ToString() + " objects ");

                }
                cant += 1;
                if (((obj is Transaction) || (obj is WebPanel) || obj is ThemeClass) && (obj.GetPropertyValue<bool>(Properties.TRN.GenerateObject)))
                {
                    WebFormPart webForm = obj.Parts.Get<WebFormPart>();

                    KBDoctorOutput.Message( " Object : " + obj.Name);

                    foreach (IWebTag tag in WebFormHelper.EnumerateWebTag(webForm))
                    {
                        if (tag.Properties != null)
                        {

                            PropertyDescriptor prop = tag.Properties.GetPropertyDescriptorByDisplayName("Class");
                            if (prop != null)
                            {

                                ThemeClassReferenceList miclasslist = new ThemeClassReferenceList();
                                try
                                {
                                    miclasslist = (ThemeClassReferenceList)prop.GetValue(new object());
                                }
                                catch (Exception e) { Console.WriteLine(e.InnerException); };

                                foreach (ThemeClass miclass in miclasslist.GetThemeClasses(obj.Model))
                                {
                                    if (miclass != null)
                                    {
                                        string miclstr = miclass.Root.Description + '-' + miclass.Name.ToLower();
                                        if (!UsedClasses.Contains(miclstr))
                                        {
                                            UsedClasses.Add(miclstr);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        */
        public static void ClassUsed()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Used Classes";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);
                KBModel model = UIServices.KB.CurrentModel;

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Class", "#References", "External" });

                foreach (ThemeClass themeClass in ThemeClass.GetAll(model))
                {
                   // KBDoctorOutput.Message("Class:" + themeClass.Name);
                    int cant = 0;
                    foreach (EntityReference entityRef in themeClass.GetReferencesTo())
                    {
                        // KBDoctorOutput.Message("ObjRef: From:" + entityRef.From.Type.ToString() + " To: " + entityRef.To.Type.ToString());
                        KBObject objRefTo = KBObject.Get(model, entityRef.To);
                        KBObject objRefFrom = KBObject.Get(model, entityRef.From);


                        if (objRefFrom != null && !(objRefFrom is Theme) && !(objRefFrom is ThemeClass))
                        {
                  //          KBDoctorOutput.Message("ObjRefFrom: :" + objRefFrom.Name + " Type : " + objRefFrom.TypeName);
                            cant++;
                        }

                    }
                    writer.AddTableData(new string[] { themeClass.Name, cant.ToString() , themeClass.ExternalClass.ToString()});
                    
                    KBDoctorOutput.Message("Class Name: :" + themeClass.Name + " Referencias: " + cant.ToString());
                }



                writer.AddFooter();
                writer.Close();
                KBDoctorOutput.EndSection( title, true);

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

    }
}
