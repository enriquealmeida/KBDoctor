using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework.References;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    public static class Tables
    {
        public sealed class TableSummary
        {
            public Table Table { get; set; }
            public string ModuleName { get; set; }
            public string IsPublic { get; set; }
            public int KeyCount { get; set; }
            public int KeyWidth { get; set; }
            public int VariableWidth { get; set; }
            public int FixedWidth { get; set; }
            public int TotalWidth { get; set; }
            public string CacheLevel { get; set; }
        }

        public sealed class TableDescriptionIssue
        {
            public Table Table { get; set; }
        }

        public sealed class GroupDescriptionIssue
        {
            public Group Group { get; set; }
        }

        public sealed class TableTransactionRelation
        {
            public Table Table { get; set; }
            public IList<Transaction> GeneratedTransactions { get; set; }
            public IList<Transaction> NotGeneratedTransactions { get; set; }
            public bool HasMixedGeneration { get; set; }
        }

        public static IEnumerable<TableSummary> GetTableSummaries(KBModel model)
        {
            foreach (Table table in Table.GetAll(model))
            {
                int keyCount = 0;
                int keyWidth = 0;
                int variableWidth = 0;
                int fixedWidth = 0;
                int totalWidth = 0;

                foreach (TableAttribute attribute in table.TableStructure.Attributes)
                {
                    if (attribute.IsKey)
                    {
                        keyCount++;
                        keyWidth += attribute.Attribute.Length;
                    }

                    totalWidth += attribute.Attribute.Length;
                    if (attribute.Attribute.Type == eDBType.LONGVARCHAR || attribute.Attribute.Type == eDBType.VARCHAR)
                    {
                        variableWidth += attribute.Attribute.Length;
                    }
                    else
                    {
                        fixedWidth += attribute.Attribute.Length;
                    }
                }

                yield return new TableSummary
                {
                    Table = table,
                    ModuleName = TableModule(model, table).Name,
                    IsPublic = table.IsPublic ? "Yes" : string.Empty,
                    KeyCount = keyCount,
                    KeyWidth = keyWidth,
                    VariableWidth = variableWidth,
                    FixedWidth = fixedWidth,
                    TotalWidth = totalWidth,
                    CacheLevel = table.GetPropertyValueString("CACHE_LEVEL")
                };
            }
        }

        public static IEnumerable<TableDescriptionIssue> GetTablesWithoutDescription(KBModel model)
        {
            foreach (Table table in Table.GetAll(model))
            {
                if (table.Name == table.Description.Replace(" ", string.Empty))
                {
                    yield return new TableDescriptionIssue { Table = table };
                }
            }
        }

        public static IEnumerable<GroupDescriptionIssue> GetGroupsWithoutDescription(KBModel model)
        {
            foreach (Group group in Group.GetAll(model))
            {
                if (group.Name == group.Description.Replace(" ", string.Empty) || group.Name == string.Empty)
                {
                    yield return new GroupDescriptionIssue { Group = group };
                }
            }
        }

        public static IEnumerable<TableTransactionRelation> GetTableTransactionRelations(KBModel model)
        {
            foreach (Table table in Table.GetAll(model))
            {
                List<Transaction> generatedTransactions = new List<Transaction>();
                List<Transaction> notGeneratedTransactions = new List<Transaction>();

                foreach (Transaction transaction in table.AssociatedTransactions)
                {
                    if (transaction.GetPropertyValue<bool>(global::Artech.Genexus.Common.Properties.TRN.GenerateObject))
                    {
                        generatedTransactions.Add(transaction);
                    }
                    else
                    {
                        notGeneratedTransactions.Add(transaction);
                    }
                }

                yield return new TableTransactionRelation
                {
                    Table = table,
                    GeneratedTransactions = generatedTransactions,
                    NotGeneratedTransactions = notGeneratedTransactions,
                    HasMixedGeneration = generatedTransactions.Count > 0 && notGeneratedTransactions.Count > 0
                };
            }
        }

        public static Table TableOfAttribute(Artech.Genexus.Common.Objects.Attribute attribute)
        {
            foreach (EntityReference reference in attribute.GetReferencesTo(attribute.Model.Id))
            {
                KBObject tableObject = KBObject.Get(attribute.Model, reference.From);
                if (tableObject is Table table)
                {
                    return table;
                }
            }

            return null;
        }

        public static Module TableModule(KBModel model, Table table)
        {
            return GenexusBLServices.Tables.GetBestAssociatedTransaction(model, table.Key).Module;
        }

        public static string ShortName(int length, string name)
        {
            if (name.Length > length)
            {
                return name.Substring(0, length);
            }

            return name;
        }

        public static string KeyList(Table table, int attributeNameLength)
        {
            string tableKey = string.Empty;
            string comma = string.Empty;

            foreach (TableAttribute attribute in table.TableStructure.PrimaryKey)
            {
                tableKey += comma + ShortName(attributeNameLength, attribute.Name);
                comma = ",";
            }

            return tableKey;
        }
    }
}
