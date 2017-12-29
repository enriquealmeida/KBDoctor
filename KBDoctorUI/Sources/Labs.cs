using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework.References;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Common.Framework.Commands;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common;

using System;
using System.Collections.Generic;
using System.Text;

using Artech.Genexus.Common.Collections;
using System.Text.RegularExpressions;


using System.Linq;
//using System.Xml.Linq;
using System.Xml;
using System.IO;

//using Microsoft.XmlDiffPatch;
using System.Xml.Xsl;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.Threading;
using System.Globalization;
using Artech.Packages.Patterns.Definition;
using Artech.Packages.Patterns.Engine;
using Artech.Packages.Patterns;
using Artech.Packages.Patterns.Objects;
using Artech.Architecture.Common.Descriptors;
using Artech.Genexus.Common.Parts;
//using Artech.Genexus.Common.Resources;


namespace Concepto.Packages.KBDoctor
{
    static class Labs
    {
        public static void ReplaceNullsCompatible()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Replace attribute with Compatible with NO ";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            output.StartSection(title);

            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Object", "Description", "Attribute", "Description", "PK / FK", "Nullable" });


            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            KBModel kbModel = UIServices.KB.CurrentModel;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());

            foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                bool saveObj = false;
                Transaction trn = (Transaction)obj;
                if (trn != null)
                {
                    foreach (TransactionLevel LVL in trn.Structure.GetLevels())
                    {
                        bool isLevelRemovable = true;

                        Table TBL = LVL.AssociatedTable;

                        foreach (TransactionAttribute a in LVL.Structure.GetAttributes())
                        {
                            output.AddLine(a.Name);
                            writer.AddTableData(new string[] { Functions.linkObject(trn), trn.Description, Functions.linkObject(a), a.Attribute.Description, a.IsForeignKey.ToString(), a.IsNullable.ToString() });
                            if (!a.IsForeignKey && !a.IsKey && (a.IsNullable == TableAttribute.IsNullableValue.Compatible || a.IsNullable == TableAttribute.IsNullableValue.True))
                                {
                                a.IsNullable = TableAttribute.IsNullableValue.False;
                                saveObj = true;
                                }
                        }
                    }
                    if (saveObj)
                    {
                        output.AddLine("Saving ." + trn.Name);
                        trn.Save();
                    }
                }
            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);
        }
    }
}