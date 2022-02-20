using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
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

namespace Concepto.Packages.KBDoctor.Modularization
{
    public partial class Modularization : Form
    {
        public Modularization()
        {
            InitializeComponent();
        }

        private void buttonLoadTables_Click(object sender, EventArgs e)

        {
            IKBService kbserv = UIServices.KB;
            KBModel kbmodel = kbserv.CurrentModel;
            //Create a New DataTable to store the Data
            DataTable Tables = new DataTable("Tables");
            //Create the Columns in the DataTable
            DataColumn c0 = new DataColumn("Name");
            DataColumn c1 = new DataColumn("Module");
            System.Type typeInt32 = System.Type.GetType("System.Int32");
            DataColumn c2 = new DataColumn("#Updates", typeInt32);

            DataColumn c3 = new DataColumn("#Readers", typeInt32);
            DataColumn c4 = new DataColumn("#Inserts", typeInt32);
            DataColumn c5 = new DataColumn("#Total", typeInt32);
            //Add the Created Columns to the Datatable

            Tables.Columns.Add(c0);
            Tables.Columns.Add(c1);
            Tables.Columns.Add(c2);
            Tables.Columns.Add(c3);
            Tables.Columns.Add(c4);
            Tables.Columns.Add(c5);
            //Create 3 rows
            DataRow row;

            foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
            {
                if (obj is Table)
                {
                    row = Tables.NewRow();
                    row["Name"] = obj.Name;
                    row["Module"] = ModulesHelper.ObjectModule(obj);

                    IList<KBObject> updaters = (from r in kbmodel.GetReferencesTo(obj.Key, LinkType.UsedObject)
                                                where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                                where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo)
                                                select kbmodel.Objects.Get(r.From)).ToList();


                    row["#Updates"] = updaters.Count;

                    IList<KBObject> readers = (from r in kbmodel.GetReferencesTo(obj.Key, LinkType.UsedObject)
                                               where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                               where ReferenceTypeInfo.HasReadAccess(r.LinkTypeInfo)
                                               select kbmodel.Objects.Get(r.From)).ToList();

                    row["#Readers"] = readers.Count;

                    IList<KBObject> inserts = (from r in kbmodel.GetReferencesTo(obj.Key, LinkType.UsedObject)
                                               where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                               where ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo)
                                               select kbmodel.Objects.Get(r.From)).ToList();
                    row["#Inserts"] = inserts.Count;
                    row["#Total"] = updaters.Count + readers.Count + inserts.Count;
                    Tables.Rows.Add(row);
                }
            }

            DataGridViewColumn c6 = dataGridTables.Columns.GetLastColumn(DataGridViewElementStates.Frozen,DataGridViewElementStates.None);
            dataGridTables.DataSource = Tables;
            dataGridTables.Sort(c6,ListSortDirection.Descending);
        }
    }
}
