using Artech.Architecture.Common.Cache;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Location;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language;
using Artech.Architecture.Language.Parser;
using Artech.Common.Collections;
using Artech.Genexus.Common;
using Artech.Genexus.Common.AST;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Genexus.Common.Types;

using Artech.Udm.Framework;
using Artech.Udm.Framework.References;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Artech.Genexus.Common.Objects;
using Artech.Common.Diagnostics;
using System.Windows.Forms;

namespace Concepto.Packages.KBDoctor
{
    public class AttributeTree 
    {
        private CallTree m_MyDependencies;
        private Dictionary<EntityKey, bool> m_ParsedObjects;
        private HashSet<Artech.Genexus.Common.Objects.Attribute> m_AnalyzedAttributes;
        private HashSet<Artech.Genexus.Common.Objects.Attribute> m_AttributesToAnalyze;
        private KBModel m_Model;
        private string WorkName;

        public AttributeTree(KBModel Model) : this(Model, new List<Artech.Genexus.Common.Objects.Attribute>()) { }
        public AttributeTree(KBModel Model, IEnumerable<Artech.Genexus.Common.Objects.Attribute> Attributes) : base()
        {
            this.m_Model = Model;
            Reset();
            ToAnalyze(Attributes);
            if (!m_AttributesToAnalyze.IsEmpty())
                WorkName = m_AttributesToAnalyze.First().Name;
        }

        public void Reset()
        {
            m_AttributesToAnalyze = new HashSet<Artech.Genexus.Common.Objects.Attribute>();
            m_AnalyzedAttributes = new HashSet<Artech.Genexus.Common.Objects.Attribute>();
           // m_MyDependencies = new Dependencies(Artech.Genexus.Common.Version.Model, this);
            m_ParsedObjects = new Dictionary<EntityKey, bool>();
            WorkName = "";
        }

        public void ToAnalyze(IEnumerable<Artech.Genexus.Common.Objects.Attribute> Attributes)
        {
            foreach (Artech.Genexus.Common.Objects.Attribute Attribute in Attributes)
                ToAnalyze(Attribute);
        }

        public void ToAnalyze(Artech.Genexus.Common.Objects.Attribute Attribute)
        {
            lock (m_AttributesToAnalyze)
            {
                if (m_AttributesToAnalyze.Contains(Attribute))
                    return;
                m_AttributesToAnalyze.Add(Attribute);
            }
        }

        public IEnumerable<Artech.Genexus.Common.Objects.Attribute> Attributes
        {
            get
            {
                return m_AnalyzedAttributes;
            }
        }

        public bool CancelRequested { get; private set; }
        public bool IsWorkComplete { get; private set; }

        public  bool Task(params object[] parameters)
        {
            List<Artech.Genexus.Common.Objects.Attribute> TempList;
            lock (m_AttributesToAnalyze)
            {
                TempList = new List<Artech.Genexus.Common.Objects.Attribute>(m_AttributesToAnalyze);
                m_AttributesToAnalyze.Clear();
            }
            using (new ScopedModelObjectCache())
            {
                foreach (Artech.Genexus.Common.Objects.Attribute Attribute in TempList)
                {
                    NewAttribute(Attribute);
                    if (CancelRequested)
                        goto cancel;
                }
            }
            cancel:
            return true;
        }

        private void NewAttribute(Artech.Genexus.Common.Objects.Attribute Attribute)
        {
            EventArgs eventArgs = new EventArgs(Attribute);
            StartAttributeAnalysis(this, eventArgs);
            if (!m_AnalyzedAttributes.Contains(Attribute))
            {

                string Section = string.Format("Tree for {0}", Attribute.Name);
                try
                {
                    KBDoctorOutput.StartSection(Section);
                    Analyze(m_MyDependencies.AddItem(Attribute, this), Attribute.Model, m_MyDependencies, m_ParsedObjects); //, this);
                    m_AnalyzedAttributes.Add(Attribute);
                    KBDoctorOutput.EndSection(Section);
                }
                catch (Exception e)
                {
                    KBDoctorOutput.InternalError(Section, e);
                }
            }
            EndAttributeAnalysis(this, eventArgs);
        }

        public class EventArgs : System.EventArgs
        {
            public Artech.Genexus.Common.Objects.Attribute Attribute { get; private set; }
            public EventArgs(Artech.Genexus.Common.Objects.Attribute attribute)
            {
                Attribute = attribute;
            }
        }

        public delegate void AttributeTreeEvent(AttributeTree sender, EventArgs e);
        public event AttributeTreeEvent EndAttributeAnalysis;
        public event AttributeTreeEvent StartAttributeAnalysis;

      /*  public class Analyzer : K2BThreading
        {
            private KBModel m_Model;
            private Dependency m_Dependency;
            public Dependency Dependency { get { return m_Dependency; } set { m_Dependency = value; } }
            public KBModel Model { get { return m_Model; } set { m_Model = value; } }

            public bool CancelRequested { get; private set; }

            public Analyzer(Dependency Member, KBModel model) : base(true, Member.Name)
            {
                m_Model = model;
                m_Dependency = Member;
            }

            public  bool Task(params object[] parameters)
            {
                using (new ScopedModelObjectCache())
                {
                    Analyze(m_Dependency, m_Model, m_Dependency.AttributeTree.m_MyDependencies, m_Dependency.AttributeTree.m_ParsedObjects);//, this);
                    return !CancelRequested;
                }
            }
        }
        */
        private static void Analyze(Dependency m_Dependency, KBModel m_Model, CallTree m_Dependencies, Dictionary<EntityKey, bool> m_ParsedObjects) //, K2BThreading Worker)
        {
            if (m_Dependency.KBObject == null)
                return;

         //   if (Worker != null && Worker.CancelRequested)
         //       goto end;

            ParseObject(m_Dependency.KBObject.Key, m_Model, m_Dependencies, m_ParsedObjects, m_Dependency.AnalyzeCallers);

            if (!m_Dependency.AnalyzeCallers)
                return;

            bool ReferencesAnalyzed;
            m_ParsedObjects.TryGetValue(m_Dependency.KBObject.Key, out ReferencesAnalyzed);
            if (ReferencesAnalyzed)
                return;
            m_ParsedObjects[m_Dependency.KBObject.Key] = true;

            //K2BStatusBar.Message(string.Format("Analyzing references to {1} {0}.", m_Dependency.KBObject.QualifiedName.ToString(), m_Dependency.KBObject.TypeDescriptor.Name));
            foreach (EntityReference item in m_Dependency.KBObject.GetReferencesTo())
            {
                ParseObject(item.From, m_Model, m_Dependencies, m_ParsedObjects, true);
              //  if (Worker != null && Worker.CancelRequested)
              //      goto end;
            }

            end:
           // K2BStatusBar.Clear();
            return;
        }

        private const int MaxParsedObjects = 10000;
        private static void ParseObject(EntityKey entityKey, KBModel model, CallTree dependencies, Dictionary<EntityKey, bool> parsedObjects, bool AnalyzeCallers)
        {
            if (parsedObjects.ContainsKey(entityKey))
                return;
            parsedObjects[entityKey] = !AnalyzeCallers;

            if (entityKey.Type != typeof(Procedure).GUID && entityKey.Type != typeof(Transaction).GUID && entityKey.Type != typeof(Artech.Genexus.Common.Objects.Attribute).GUID)
                return;

            KBObject KBObject = KBObject.Get(model, entityKey);
            if (KBObject == null)
                return;

            if (parsedObjects.Count > MaxParsedObjects)
            {
                KBDoctorOutput.Error(string.Format("{1} {0} not analyzed. Limit of {2} objects scanned exceeded.", KBObject.QualifiedName.ToString(), KBObject.TypeDescriptor.Name, MaxParsedObjects));
                return;
            }

            //K2BStatusBar.Message(string.Format("Parsing {1} {0}.", KBObject.QualifiedName.ToString(), KBObject.TypeDescriptor.Name));
            if (KBObject is Artech.Genexus.Common.Objects.Attribute)
                ParseAttribute((Artech.Genexus.Common.Objects.Attribute)KBObject, model, dependencies);
            else
                foreach (KBObjectPart part in KBObject.Parts)
                {
                   // K2BSourceBase sb;
                    /*
                    if (part is RulesPart)
                    {
                        sb = new K2BRules(new K2BObject(KBObject), (RulesPart)part);
                        if (sb.isValid)
                            ParseRules(sb.AstRoot, model, dependencies);
                        K2BOutput.Messages(sb.Messages);
                    }
                    else if (part is SourcePart)
                    {
                        sb = new K2BSourceBase(new K2BObject(KBObject), (SourcePart)part);
                        if (sb.isValid)
                            ParseSource(sb.AstRoot, model, dependencies);
                        K2BOutput.Messages(sb.Messages);
                    }
                    */
                }
        }

        private void DumpAllToFile(CallTree Dependencies)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CallTree));
            TextWriter writer = new StreamWriter(@"c:\temp\kk.xml");
            serializer.Serialize(writer, Dependencies);
            writer.Close();
        }

        private void DumpAllDependencies()
        {
         /*   foreach (Dependency member in m_MyDependencies.Items)
            {
                K2BOutput.StartSection(string.Format("Dependencies for {0}", member.Name));
                DumpDependencies(member, m_Model, 0, new List<SourcePosition>(), new List<Dependency.Identifier>(), false);
                K2BOutput.EndSection();
            }
            */
        }

        private void DumpDependencies(IEnumerable<Artech.Genexus.Common.Objects.Attribute> Attributes, bool RecursiveSearch)
        {
           /* foreach (Artech.Genexus.Common.Objects.Attribute Attribute in Attributes)
            {
                try
                {
                    K2BOutput.StartSection(string.Format("Dependencies for {0}", Attribute.Name));
                    Dependency Member = m_MyDependencies.Get(Attribute);
                    DumpDependencies(Member, m_Model, 0, new List<SourcePosition>(), new List<Dependency.Identifier>(), RecursiveSearch);
                    K2BOutput.EndSection();
                    if (CancelRequested)
                        break;
                }
                catch (Exception e)
                {
                    K2BOutput.InternalError(string.Format("Dependencies for {0}", Attribute.Name), e);
                }
            }*/
        }

        [Serializable, XmlRoot(ElementName = "TrackChanges")]
        public class TrackChanges
        {
            private System.Collections.ObjectModel.Collection<string> m_Attributes = new System.Collections.ObjectModel.Collection<string>();

            [XmlArray(ElementName = "Attributes")]
            [XmlArrayItem(ElementName = "Attribute")]
            public string[] AttributeNames
            {
                get { return m_Attributes.ToArray(); }
                set { m_Attributes = new System.Collections.ObjectModel.Collection<string>(value); }
            }

            [XmlAttribute(AttributeName = "OutputFile")]
            public string OutputFile;

            public void Add(string name)
            {
                m_Attributes.Add(name);
            }
        }
/*
        public class SaveToFile : K2BThreading
        {
            string m_FileName;
            KBModel m_Model;
            IEnumerable<AttributeTree> m_AttributeTreeList;
            public SaveToFile(IEnumerable<AttributeTree> List, string fileName, KBModel model) : base(true, fileName)
            {
                this.m_AttributeTreeList = List;
                this.m_FileName = fileName;
                this.m_Model = model;
            }

            public override bool Task(params object[] parameters)
            {
                bool result = true;
                KBDoctorOutput.Message(string.Format("Saving changes tree to \"{0}\".", m_FileName));
                XMLLineageRoot root = new XMLLineageRoot(m_Model);

                using (new ScopedModelObjectCache())
                {
                    foreach (AttributeTree at in m_AttributeTreeList)
                    {
                        if (!at.IsWorkComplete)
                        {
                            KBDoctorOutput.Error("Please try again when evaluation of all nodes is complete.");
                            result = false;
                            break;
                        }
                       /* try
                        {
                            foreach (Dependency d in ScanDependencies(at.Attributes, at.m_MyDependencies, at.m_ParsedObjects, at, m_Model, true))
                                root.AddDependency(d);
                        }
                        catch (Exception e)
                        {
                            result = false;
                            K2BOutput.InternalError(e);
                        }
                        
                    }

                }
                if (result)
                {
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(XMLLineageRoot));
                        TextWriter writer = new StreamWriter(m_FileName);
                        serializer.Serialize(writer, root);
                        writer.Close();
                        KBDoctorOutput.Message(string.Format("Changes tree saved to \"{0}\".", m_FileName));
                    }
                    catch (Exception e)
                    {
                        KBDoctorOutput.InternalError("Task", e);
                    }
                }
                return result;
            }
        }
    */

        public IEnumerable<KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>>> ScanDependencies(KBModel model)
        {
            foreach (Artech.Genexus.Common.Objects.Attribute Attribute in Attributes)
            {
                Dependency Member = m_MyDependencies.Get(Attribute);
                foreach (KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>> kvp in ScanDependencies(model, Member))
                    yield return kvp;
            }
        }

        public IEnumerable<KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>>> ScanDependencies(KBModel model, Artech.Genexus.Common.Objects.Attribute Attribute)
        {
            return ScanDependencies(model, m_MyDependencies.Get(Attribute));
        }

        public static IEnumerable<KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>>> ScanDependencies(KBModel model, Dependency Member)
        {
            Analyze(Member, model, Member.AttributeTree.m_MyDependencies, Member.AttributeTree.m_ParsedObjects);//, null);
            return Member.DependsOn;
        }

     /*   private static IEnumerable<Dependency> ScanDependencies(IEnumerable<Artech.Genexus.Common.Objects.Attribute> Attributes, Dependencies dependencies, Dictionary<EntityKey, bool> parsedObjects, K2BThreading backgroundWorker, KBModel model, bool RecursirveSearch)
        {
            foreach (Artech.Genexus.Common.Objects.Attribute Attribute in Attributes)
            {
                Dependency Member = dependencies.Get(Attribute);
                foreach (Dependency d in ScanDependencies(Member, model, dependencies, parsedObjects, new List<Dependency.Identifier>(), backgroundWorker, RecursirveSearch))
                {
                    yield return d;
                    if (backgroundWorker != null && backgroundWorker.CancelRequested)
                        break;
                }
            }
        }
        
        private static IEnumerable<Dependency> ScanDependencies(Dependency Member, KBModel model, Dependencies dependencies, Dictionary<EntityKey, bool> parsedObjects, List<Dependency.Identifier> Visited, K2BThreading backgroundWorker, bool RecursiveSearch)
        {
            if (RecursiveSearch)
                Analyze(Member, model, dependencies, parsedObjects);//, backgroundWorker);

            yield return Member;

            if (Visited.Contains(Member.Id) || (backgroundWorker != null && backgroundWorker.CancelRequested))
                yield break;
            Visited.Add(Member.Id);

            foreach (KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>> dMember in Member.DependsOn)
                foreach (Dependency d in ScanDependencies(dMember.Key, model, dependencies, parsedObjects, Visited, backgroundWorker, RecursiveSearch))
                    yield return d;
        }
        */

        private void DumpDependencies(Dependency Member, KBModel model, int Indent, IEnumerable<Artech.Common.Location.IPosition> References, List<Dependency.Identifier> Visited, bool RecursiveSearch)
        {
            if (RecursiveSearch)
                Analyze(Member, model, m_MyDependencies, m_ParsedObjects);//, this);

            KBDoctorOutput.Message(new string('\t', Indent) + Member.Type + " " + Member.Name);
            //foreach (Artech.Common.Location.IPosition kbop in References)
            //    K2BOutput.Message(new OutputMessage(new Message(Artech.K2B.Entity.Common.Messages.Unknown, new string('\t', Indent + 1)), kbop));

            if (Visited.Contains(Member.Id) || CancelRequested)
                return;

            if (Indent > 50)
            {
                //K2BOutput.Message(new OutputError(new Message(tity.Common.Messages.Unknown, "Lineage nesting exceeds supported value of 50 levels.")));
                return;
            }

            List<Dependency.Identifier> NewVisited = new List<Dependency.Identifier>(Visited);
            NewVisited.Add(Member.Id);

            bool IsStack = Visited.FirstThat(x => x.Type == Dependency.Types.ProgramParameter) != null;
            foreach (KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>> dMember in Member.DependsOn)
            {
                if (IsStack && Member.Type == Dependency.Types.ProgramParameter && Visited.Find(x => x.KBObject == dMember.Key.KBObject) == null)
                    continue;

                DumpDependencies(dMember.Key, model, Indent + 1, dMember.Value, NewVisited, RecursiveSearch);
            }
        }

        private static void ParseAttribute(Artech.Genexus.Common.Objects.Attribute Attribute, KBModel model, CallTree dependencies)
        {
            Dependency leftItem = dependencies.AddItem(new Dependency(Attribute, dependencies.m_AttributeTree));
            if (Attribute.Formula != null)
            {
              //  IParserEngine2 Parser = new Artech.Architecture.Language.Services.ILanguageService as IParserEngine2;

                var Parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;

                ParserInfo FormulaParser = new ParserInfo(model, ParserType.ExpressionFormula);
                if (Parser.Validate(FormulaParser, Attribute.Formula.ToString()))
                {
                    KBObjectPart x = new LayoutPart(Attribute);

                    AbstractNode FormulaNodes = ASTNodeFactory.Create(Parser.Structure, x, null, FormulaParser);

                    ParserInfo AttributeParser = new ParserInfo(model, ParserType.ArithmethicExpression);
                    if (Parser.Validate(AttributeParser, Attribute.Name))
                    {
                        AbstractNode AttributeNodes = ASTNodeFactory.Create(Parser.Structure, x, null, AttributeParser);
                        ParseFormula(FormulaNodes, AttributeNodes, model, dependencies);
                    }
                }
            }

            string AttributeNameLC = Attribute.Name.ToLower();
            foreach (DataView dv in DataView.GetAll(model))
                foreach (DataViewAttribute dva in dv.DataViewStructure.Attributes)
                    if (string.Compare(dva.ExternalName, Attribute.Name, true) == 0)
                    {
                        Dependency rightItem = dependencies.AddItem(dva.Attribute, dependencies.m_AttributeTree);
                        dependencies.AddDependency(leftItem, rightItem, new KBObjectPosition(dv.DataViewStructure));
                    }
        }
        /*
        public static void ParseSource(AbstractNode AbstractNode, KBModel model, Dependencies dependencies)
        {
            if (AbstractNode == null || AbstractNode.Node == null)
                return;

            switch (AbstractNode.Node.Token)
            {
                case (int)TokensIds.DTCLL: // Call object
                    CallStatement(AbstractNode, model, dependencies);
                    break;
                case (int)TokensIds.DTASG: // X = Y (Assignment)
                case (int)TokensIds.DTAPL: // X += Y (Assignment plus)
                case (int)TokensIds.DTAMI: // X -= Y (Assignment minus)
                    AssignmentStatement((AssignmentNode)AbstractNode, model, dependencies);
                    break;

                case (int)TokensIds.DTFIN: // for &x in collection
                    {
                        CommandBlockNode CmdBlock = (CommandBlockNode)AbstractNode;
                        VariableNameNode VariableNode = (VariableNameNode)CmdBlock.LineParameters[0];
                        AbstractNode CollectionNode = CmdBlock.LineParameters[1].Children.First();

                        Artech.Genexus.Common.Variable Variable = VariableNode.Variable;

                        if (CollectionNode is VariableNameNode || CollectionNode is ObjectPropertyNode)
                        {
                            ExpandStructureDependencies(GetStructureTypeReference(VariableNode, model), model, dependencies, CollectionNode, GetNodeText(VariableNode), Dependency.Types.Variable, VariableNode.Part.KBObject, GetNodeText(CollectionNode), Dependency.Types.Variable, VariableNode.Part.KBObject);
                            ExpandStructureDependencies(GetStructureTypeReference(VariableNode, model), model, dependencies, VariableNode, GetNodeText(CollectionNode), Dependency.Types.Variable, VariableNode.Part.KBObject, GetNodeText(VariableNode), Dependency.Types.Variable, VariableNode.Part.KBObject);
                        }
                        else
                        {
                            // This code tries to avoid sending a warning when the GetMessages() method is invoked in a for/in loop
                            string FunctionName = null;
                            if (CollectionNode.ChildrenCount > 1)
                            {
                                CollectionNode = CollectionNode.Children.Skip(1).First();
                                while (CollectionNode is ObjectPropertyNode || CollectionNode is ObjectMethodNode)
                                    CollectionNode = CollectionNode.Children.First();
                                if (CollectionNode is FunctionNode)
                                    FunctionName = ((FunctionNode)CollectionNode).FunctionName;
                                else if (CollectionNode is VariableNameNode)
                                {
                                    Artech.Genexus.Common.Variable CollectionVariable = ((VariableNameNode)CollectionNode).Variable;
                                    if (CollectionVariable != null && CollectionVariable.Type == eDBType.GX_BUSCOMP)
                                    {
                                        if (CollectionNode.Children.Skip(1).First() is FunctionFunctionNode)
                                            FunctionName = ((FunctionNode)CollectionNode.Children.Skip(1).First()).FunctionName;
                                    }
                                }
                            }
                            if (FunctionName != "getmessages")
                                KBDoctorOutput.Warning(string.Format("Do not know how to handle collection '{1}' in '{0}' code block.", CmdBlock.Name, CollectionNode.Text), new SourcePosition(CollectionNode.Part, CollectionNode.Node.Row, CollectionNode.Node.CharPosition));
                        }

                        foreach (AbstractNode child in AbstractNode.Children)
                            ParseSource(child, model, dependencies);
                        break;
                    }

                default:
                    foreach (AbstractNode child in AbstractNode.Children)
                        ParseSource(child, model, dependencies);
                    break;
            }
        }
        */
        
        public static void ParseSource2(AbstractNode AbstractNode, KBModel model, CallTree calltree)
        {
            if (AbstractNode == null || AbstractNode.Node == null)
                return;

            switch (AbstractNode.Node.Token)
            {
                case (int)TokensIds.DTCLL: // Call object
                    CallStatement(AbstractNode, model, calltree);
                    break;
                case (int)TokensIds.DTASG: // X = Y (Assignment)
                case (int)TokensIds.DTAPL: // X += Y (Assignment plus)
                case (int)TokensIds.DTAMI: // X -= Y (Assignment minus)
                    AssignmentStatement((AssignmentNode)AbstractNode, model, calltree);
                    break;

                case (int)TokensIds.DTFIN: // for &x in collection
                    {
                        CommandBlockNode CmdBlock = (CommandBlockNode)AbstractNode;
                        VariableNameNode VariableNode = (VariableNameNode)CmdBlock.LineParameters[0];
                        AbstractNode CollectionNode = CmdBlock.LineParameters[1].Children.First();

                        Artech.Genexus.Common.Variable Variable = VariableNode.Variable;

                        if (CollectionNode is VariableNameNode || CollectionNode is ObjectPropertyNode)
                        {
                            ExpandStructureDependencies(GetStructureTypeReference(VariableNode, model), model, calltree, CollectionNode, GetNodeText(VariableNode), Dependency.Types.Variable, VariableNode.Part.KBObject, GetNodeText(CollectionNode), Dependency.Types.Variable, VariableNode.Part.KBObject);
                            ExpandStructureDependencies(GetStructureTypeReference(VariableNode, model), model, calltree, VariableNode, GetNodeText(CollectionNode), Dependency.Types.Variable, VariableNode.Part.KBObject, GetNodeText(VariableNode), Dependency.Types.Variable, VariableNode.Part.KBObject);
                        }
                        else
                        {
                            // This code tries to avoid sending a warning when the GetMessages() method is invoked in a for/in loop
                            string FunctionName = null;
                            if (CollectionNode.ChildrenCount > 1)
                            {
                                CollectionNode = CollectionNode.Children.Skip(1).First();
                                while (CollectionNode is ObjectPropertyNode || CollectionNode is ObjectMethodNode)
                                    CollectionNode = CollectionNode.Children.First();
                                if (CollectionNode is FunctionNode)
                                    FunctionName = ((FunctionNode)CollectionNode).FunctionName;
                                else if (CollectionNode is VariableNameNode)
                                {
                                    Artech.Genexus.Common.Variable CollectionVariable = ((VariableNameNode)CollectionNode).Variable;
                                    if (CollectionVariable != null && CollectionVariable.Type == eDBType.GX_BUSCOMP)
                                    {
                                        if (CollectionNode.Children.Skip(1).First() is FunctionNode)
                                            FunctionName = ((FunctionNode)CollectionNode.Children.Skip(1).First()).FunctionName;
                                    }
                                }
                            }
                            if (FunctionName != "getmessages")
                                KBDoctorOutput.Warning(string.Format("Do not know how to handle collection '{1}' in '{0}' code block.", CmdBlock.Name, CollectionNode.Text), new SourcePosition(CollectionNode.Part, CollectionNode.Node.Row, CollectionNode.Node.CharPosition));
                        }

                        foreach (AbstractNode child in AbstractNode.Children)
                            ParseSource2(child, model, calltree);
                        break;
                    }

                default:
                    foreach (AbstractNode child in AbstractNode.Children)
                        ParseSource2(child, model, calltree);
                    break;
            }
        }
        
        private static void CallStatement(AbstractNode AbstractNode, KBModel model, CallTree calltree)
        {
            bool CallVariable = false;
            KBObject program = null;
            IEnumerable<AbstractNode> parameters = null;

            KBDoctorOutput.StartSection("CallStatement");
            if (AbstractNode is FunctionNode)
            {
                if (((FunctionNode)AbstractNode).FunctionName == "call")
                {
                    if (AbstractNode.Children.First().Node.Tag is KBObject)
                    {
                        // Call(pgmname[,params])
                        program = (KBObject)AbstractNode.Children.First().Node.Tag;
                        parameters = AbstractNode.Children.Skip(1);
                    }
                    else if (AbstractNode.Children.First() is VariableNameNode)
                        CallVariable = true;
                }
                else
                {
                    // PgmName([params])
                    program = (KBObject)(((FunctionNode)AbstractNode).Element.Name.Tag);
                    parameters = AbstractNode.Children;
                }
            }
            else if (AbstractNode is AssignmentNode || AbstractNode is ObjectMethodNode)
            {
                // Module1.Module2...XXXX([params])
                // Module1.Module2...XXXX.Call([params])
                // XXXX.Call([params])
                AbstractNode stmt = AbstractNode.Children.Skip(1).First();
                while (stmt is ObjectPropertyNode || stmt is ObjectMethodNode)
                {
                    if (stmt.Children.Skip(1).First().Text.ToLower() == "call")
                    {
                        parameters = stmt.Children.Skip(1).First().Children;
                        stmt = stmt.Children.First();
                    }
                    else
                        stmt = stmt.Children.Skip(1).First();
                }
                if (stmt is FunctionNode)
                {
                    FunctionNode FunctionNode = (FunctionNode)stmt;
                    if (FunctionNode.FunctionName == "call")
                    {
                        AbstractNode pgm = AbstractNode.Children.First();
                        while (pgm is ObjectPropertyNode || pgm is ObjectMethodNode)
                        {
                            if (pgm.Children.Skip(1).First().Text.ToLower() == "call")
                            {
                                parameters = pgm.Children.Skip(1).First().Children;
                                pgm = pgm.Children.First();
                            }
                            else
                                pgm = pgm.Children.Skip(1).First();
                        }
                        if (pgm is UnknownNode)
                            program = (KBObject)(((UnknownNode)pgm).Element.Tag);
                        else if (pgm is AttributeNameNode)
                            program = (KBObject)(((AttributeNameNode)pgm).Node.Tag);
                    }
                    else
                        program = (KBObject)FunctionNode.Element.Name.Tag;
                    parameters = stmt.Children;
                }
                else if (stmt is AttributeNameNode)
                    program = (KBObject)(((AttributeNameNode)stmt).Node.Tag);
                else if (stmt is UnknownNode)
                    program = (KBObject)(((UnknownNode)stmt).Node.Tag);
            }
            else if (AbstractNode is RuleNode)
            {
                program = (KBObject)(((RuleNode)AbstractNode).Element.Name.Tag);
                parameters = AbstractNode.Children;
            }

            if (program != null && parameters != null)
            {
                List<Signature> signatures = new List<Signature>();
                if (!SourceHelper.LoadSignature(program.Parts.Get<RulesPart>(), signatures) || signatures.Count < 1)
                {
                    //K2BOutput.Message(new OutputWarning(new Message(string.Format("Cannot get parameter definition for {0} or no parameter definition found.", program.GetFullName())), new SourcePosition(AbstractNode.Part, AbstractNode.Node.Row, AbstractNode.Node.CharPosition)));
                    signatures = new List<Signature>();
                    signatures.Add(new Signature(program, new RuleDefinition("")));
                }
                int parmNo = 0;
                foreach (AbstractNode parm in parameters)
                {
                    parmNo++;
                    if (Dependency.DependableNode(parm))
                    {
                        AbstractNode ReferenceNode = LeftMostNode(parm);
                        Dependency.Types ReferenceNodeType = (Dependency.Types)ReferenceNode.Node.Token;

                        SourcePosition SourcePosition = new SourcePosition(ReferenceNode.Part, ReferenceNode.Node.Row, ReferenceNode.Node.CharPosition);

                        RuleDefinition.ParameterAccess Accessor = RuleDefinition.ParameterAccess.PARM_INOUT;
                        if (signatures[0].ParametersCount >= parmNo)
                            Accessor = signatures[0].Parameters.Skip(parmNo - 1).First().Accessor;

                        if (Accessor == RuleDefinition.ParameterAccess.PARM_IN)
                            ExpandStructureDependencies(parm, model, calltree, parmNo.ToString(), Dependency.Types.ProgramParameter, program, GetNodeText(parm), ReferenceNodeType, parm.Part.KBObject);
                        else if (Accessor == RuleDefinition.ParameterAccess.PARM_OUT)
                            ExpandStructureDependencies(parm, model, calltree, GetNodeText(parm), ReferenceNodeType, parm.Part.KBObject, parmNo.ToString(), Dependency.Types.ProgramParameter, program);
                        else
                        {
                            ExpandStructureDependencies(parm, model, calltree, parmNo.ToString(), Dependency.Types.ProgramParameter, program, GetNodeText(parm), ReferenceNodeType, parm.Part.KBObject);
                            ExpandStructureDependencies(parm, model, calltree, GetNodeText(parm), ReferenceNodeType, parm.Part.KBObject, parmNo.ToString(), Dependency.Types.ProgramParameter, program);
                        }
                    }
                }
            }
            else if (!CallVariable)
                KBDoctorOutput.Warning(string.Format("Unhandled call format: {0}.", AbstractNode.Text), new SourcePosition(AbstractNode.Part, AbstractNode.Node.Row, AbstractNode.Node.CharPosition));
        }
        /*
        private static void CallStatement2(AbstractNode AbstractNode, KBModel model, CallTree calltree)
        {
            bool CallVariable = false;
            KBObject program = null;
            IEnumerable<AbstractNode> parameters = null;

            KBDoctorOutput.StartSection("CallStatement");
            if (AbstractNode is FunctionNode)
            {
                if (((FunctionNode)AbstractNode).FunctionName == "call")
                {
                    if (AbstractNode.Children.First().Node.Tag is KBObject)
                    {
                        // Call(pgmname[,params])
                        program = (KBObject)AbstractNode.Children.First().Node.Tag;
                        parameters = AbstractNode.Children.Skip(1);
                    }
                    else if (AbstractNode.Children.First() is VariableNameNode)
                        CallVariable = true;
                }
                else
                {
                    // PgmName([params])
                    program = (KBObject)(((FunctionNode)AbstractNode).Element.Name.Tag);
                    parameters = AbstractNode.Children;
                }
            }
            else if (AbstractNode is AssignmentNode || AbstractNode is ObjectMethodNode)
            {
                // Module1.Module2...XXXX([params])
                // Module1.Module2...XXXX.Call([params])
                // XXXX.Call([params])
                AbstractNode stmt = AbstractNode.Children.Skip(1).First();
                while (stmt is ObjectPropertyNode || stmt is ObjectMethodNode)
                {
                    if (stmt.Children.Skip(1).First().Text.ToLower() == "call")
                    {
                        parameters = stmt.Children.Skip(1).First().Children;
                        stmt = stmt.Children.First();
                    }
                    else
                        stmt = stmt.Children.Skip(1).First();
                }
                if (stmt is FunctionNode)
                {
                    FunctionNode FunctionNode = (FunctionNode)stmt;
                    if (FunctionNode.FunctionName == "call")
                    {
                        AbstractNode pgm = AbstractNode.Children.First();
                        while (pgm is ObjectPropertyNode || pgm is ObjectMethodNode)
                        {
                            if (pgm.Children.Skip(1).First().Text.ToLower() == "call")
                            {
                                parameters = pgm.Children.Skip(1).First().Children;
                                pgm = pgm.Children.First();
                            }
                            else
                                pgm = pgm.Children.Skip(1).First();
                        }
                        if (pgm is UnknownNode)
                            program = (KBObject)(((UnknownNode)pgm).Element.Tag);
                        else if (pgm is AttributeNameNode)
                            program = (KBObject)(((AttributeNameNode)pgm).Node.Tag);
                    }
                    else
                        program = (KBObject)FunctionNode.Element.Name.Tag;
                    parameters = stmt.Children;
                }
                else if (stmt is AttributeNameNode)
                    program = (KBObject)(((AttributeNameNode)stmt).Node.Tag);
                else if (stmt is UnknownNode)
                    program = (KBObject)(((UnknownNode)stmt).Node.Tag);
            }
            else if (AbstractNode is RuleNode)
            {
                program = (KBObject)(((RuleNode)AbstractNode).Element.Name.Tag);
                parameters = AbstractNode.Children;
            }

            if (program != null && parameters != null)
            {
                List<Signature> signatures = new List<Signature>();
                if (!SourceHelper.LoadSignature(program.Parts.Get<RulesPart>(), signatures) || signatures.Count < 1)
                {
                    //K2BOutput.Message(new OutputWarning(new Message(string.Format("Cannot get parameter definition for {0} or no parameter definition found.", program.GetFullName())), new SourcePosition(AbstractNode.Part, AbstractNode.Node.Row, AbstractNode.Node.CharPosition)));
                    signatures = new List<Signature>();
                    signatures.Add(new Signature(program, new RuleDefinition("")));
                }
                int parmNo = 0;
                foreach (AbstractNode parm in parameters)
                {
                    parmNo++;
                    if (Dependency.DependableNode(parm))
                    {
                        AbstractNode ReferenceNode = LeftMostNode(parm);
                        Dependency.Types ReferenceNodeType = (Dependency.Types)ReferenceNode.Node.Token;

                        SourcePosition SourcePosition = new SourcePosition(ReferenceNode.Part, ReferenceNode.Node.Row, ReferenceNode.Node.CharPosition);

                        RuleDefinition.ParameterAccess Accessor = RuleDefinition.ParameterAccess.PARM_INOUT;
                        if (signatures[0].ParametersCount >= parmNo)
                            Accessor = signatures[0].Parameters.Skip(parmNo - 1).First().Accessor;
                        
                        if (Accessor == RuleDefinition.ParameterAccess.PARM_IN)
                            ExpandStructureDependencies(parm, model, dependencies, parmNo.ToString(), Dependency.Types.ProgramParameter, program, GetNodeText(parm), ReferenceNodeType, parm.Part.KBObject);
                        else if (Accessor == RuleDefinition.ParameterAccess.PARM_OUT)
                            ExpandStructureDependencies(parm, model, dependencies, GetNodeText(parm), ReferenceNodeType, parm.Part.KBObject, parmNo.ToString(), Dependency.Types.ProgramParameter, program);
                        else
                        {
                            ExpandStructureDependencies(parm, model, dependencies, parmNo.ToString(), Dependency.Types.ProgramParameter, program, GetNodeText(parm), ReferenceNodeType, parm.Part.KBObject);
                            ExpandStructureDependencies(parm, model, dependencies, GetNodeText(parm), ReferenceNodeType, parm.Part.KBObject, parmNo.ToString(), Dependency.Types.ProgramParameter, program);
                        }
                        
                    }
                }
            }
            else if (!CallVariable)
                KBDoctorOutput.Warning(string.Format("Unhandled call format: {0}.", AbstractNode.Text), new SourcePosition(AbstractNode.Part, AbstractNode.Node.Row, AbstractNode.Node.CharPosition));
        }
        */

        private static void ParseFormula(AbstractNode AbstractNode, AbstractNode AttributeNode, KBModel model, CallTree dependencies)
        {
            AssignmentStatement(AttributeNode, AbstractNode, model, dependencies);
        }

        private static void ParseRules(AbstractNode AbstractNode, KBModel model, CallTree dependencies)
        {
            if (AbstractNode == null)
                return;

            if (AbstractNode is AssignmentNode)
            {
                AssignmentNode assgn = (AssignmentNode)AbstractNode;
                if (assgn.Left.Node == null)
                {
                    AbstractNode SecondChild = assgn.Right.Children.Skip(1).First();
                    if (SecondChild is FunctionNode && ((FunctionNode)SecondChild).FunctionName == "call")
                    {
                        CallStatement(AbstractNode, model, dependencies);
                        return;
                    }
                }
                AssignmentStatement((AssignmentNode)AbstractNode, model, dependencies);
                return;
            }
            else if (AbstractNode is RuleNode)
            {
                RuleNode RuleNode = (RuleNode)AbstractNode;
                if (RuleNode.RuleName == "parm")
                {
                    int parmNo = 0;
                    foreach (AbstractNode parm in RuleNode.Parameters)
                    {
                        parmNo++;
                        foreach (AbstractNode item in GetTokens(parm))
                        {
                            if (!Dependency.DependableNode(item))
                                continue;

                            string prefix = RuleNode.Element.Parameters.Skip(parmNo - 1).First().ToString().ToLower();
                            if (prefix.Contains("in:"))
                                ExpandStructureDependencies(item, model, dependencies, GetNodeText(item), Dependency.Types.Variable, item.Part.KBObject, parmNo.ToString(), Dependency.Types.ProgramParameter, item.Part.KBObject);
                            else if (prefix.Contains("out:"))
                                ExpandStructureDependencies(item, model, dependencies, parmNo.ToString(), Dependency.Types.ProgramParameter, item.Part.KBObject, GetNodeText(item), Dependency.Types.Variable, item.Part.KBObject);
                            else
                            {
                                ExpandStructureDependencies(item, model, dependencies, GetNodeText(item), Dependency.Types.Variable, item.Part.KBObject, parmNo.ToString(), Dependency.Types.ProgramParameter, item.Part.KBObject);
                                ExpandStructureDependencies(item, model, dependencies, parmNo.ToString(), Dependency.Types.ProgramParameter, item.Part.KBObject, GetNodeText(item), Dependency.Types.Variable, item.Part.KBObject);
                            }
                        }
                    }
                    return;
                }
                else if (RuleNode.RuleName == "serial")
                {
                    AbstractNode Serialized = RuleNode.Children.First();
                    AbstractNode LastNo = RuleNode.Children.Skip(1).First();
                    AbstractNode Increment = RuleNode.Children.Skip(2).First();
                    ExpandStructureDependencies(LastNo, model, dependencies, GetNodeText(Serialized), Dependency.NodeType(Serialized), Serialized.Part.KBObject, GetNodeText(LastNo), Dependency.NodeType(LastNo), Serialized.Part.KBObject);
                    ExpandStructureDependencies(Increment, model, dependencies, GetNodeText(LastNo), Dependency.NodeType(LastNo), Serialized.Part.KBObject, GetNodeText(Increment), Dependency.NodeType(Increment), LastNo.Part.KBObject);
                    return;
                }
                else if (RuleNode.RuleName == "add" || RuleNode.RuleName == "subtract")
                {
                    AbstractNode Value = RuleNode.Children.First();
                    AbstractNode Total = RuleNode.Children.Skip(1).First();
                    ExpandStructureDependencies(Value, model, dependencies, GetNodeText(Total), Dependency.NodeType(Total), Total.Part.KBObject, GetNodeText(Value), Dependency.NodeType(Value), Value.Part.KBObject);
                    return;
                }
                else if (RuleNode.RuleName == "default")
                {
                    AbstractNode Attribute = RuleNode.Children.First();
                    AbstractNode Default = RuleNode.Children.Skip(1).First();
                    foreach (AbstractNode value in GetTokens(Default))
                        if (Dependency.DependableNode(value))
                            ExpandStructureDependencies(value, model, dependencies, GetNodeText(Attribute), Dependency.NodeType(Attribute), Attribute.Part.KBObject, GetNodeText(value), Dependency.NodeType(value), value.Part.KBObject);
                    return;
                }
                else if (RuleNode.Element.Name.Tag != null)
                {
                    CallStatement(RuleNode, model, dependencies);
                    return;
                }
            }
            else if (AbstractNode is ObjectMethodNode)
            {
                ObjectMethodNode ObjectMethodNode = (ObjectMethodNode)AbstractNode;
                if (ObjectMethodNode.Node.Code == ExpressionType.Function && ObjectMethodNode.Function.FunctionName == "call")
                {
                    CallStatement(ObjectMethodNode, model, dependencies);
                    return;
                }
                else if (ObjectMethodNode.Node.Code == ExpressionType.ProgramName)
                {
                    CallStatement(ObjectMethodNode, model, dependencies);
                    return;
                }
            }

            foreach (AbstractNode child in AbstractNode.Children)
                ParseRules(child, model, dependencies);
        }

        private static void AssignmentStatement(AbstractNode LeftNode, AbstractNode RightNode, KBModel model, CallTree dependencies)
        {
            Dependency.Types LeftType;
            StructureTypeReference LeftStructureTypeReference = GetStructureTypeReference(LeftNode, model, out LeftType);

            foreach (KeyValuePair<Dependency.Types, string> left in GetLeftNode(LeftNode))
            {
                if (Dependency.DependableNode((int)left.Key))
                {
                    Dependency leftItem = dependencies.AddItem(new Dependency(left.Value, left.Key, LeftNode.Part.KBObject, dependencies.m_AttributeTree));
                    foreach (AbstractNode right in GetTokens(RightNode))
                    {
                        if (Dependency.DependableNode(right))
                        {
                            AbstractNode ReferenceNode = LeftMostNode(right);

                            Dependency rightItem = dependencies.AddItem(new Dependency(GetNodeText(right), (Dependency.Types)ReferenceNode.Node.Token, ReferenceNode.Part.KBObject, dependencies.m_AttributeTree));
                            dependencies.AddDependency(leftItem, rightItem, new SourcePosition(ReferenceNode.Part, ReferenceNode.Node.Row, ReferenceNode.Node.CharPosition));
                        }
                        UDPAssignment(right, LeftNode, LeftStructureTypeReference, model, dependencies);
                    }
                }
                else if (left.Key == Dependency.Types.Program) { }
                else if (left.Key == Dependency.Types.Property) { }
                else { }
            }

            if (RightNode is FunctionNode)
            {
                if (RightNode.Text.ToLower() == "add")
                {
                    // Método Add de una collection
                    AbstractNode Parameter = RemoveMethod(RightNode.Children.First());
                    ExpandStructureDependencies(LeftStructureTypeReference, model, dependencies, Parameter, GetNodeText(LeftNode), Dependency.Types.Variable, LeftNode.Part.KBObject, GetNodeText(Parameter), Dependency.Types.Variable, LeftNode.Part.KBObject);
                }
            }
            else if (RightNode is CommandLineNode)
            {
                // Cae aquí: new()
            }
            else if (LeftStructureTypeReference != null && (RightNode is ObjectPropertyNode || RightNode is ObjectMethodNode))
                ExpandStructureDependencies(LeftStructureTypeReference, model, dependencies, RightNode, GetNodeText(LeftNode), LeftType, LeftNode.Part.KBObject, GetNodeText(RightNode), Dependency.Types.Variable, LeftNode.Part.KBObject);
        }

        private static void UDPAssignment(AbstractNode RightNode, AbstractNode LeftNode, StructureTypeReference LeftStructureTypeReference, KBModel model, CallTree dependencies)
        {
            AbstractNode UdpObjectNode = RightNode;
            IEnumerable<AbstractNode> UdpParameters = RightNode.Children;

            KBObject UdpObject = null;
            if (UdpObjectNode is FunctionNode)
                UdpObject = (KBObject)((FunctionNode)UdpObjectNode).Element.Name.Tag;

            if (RightNode is ObjectMethodNode && RightNode.Children.Skip(1).First().Text.ToLower() == "udp")
            {
                UdpObjectNode = RightNode.Children.First();
                UdpParameters = RightNode.Children.Skip(1).First().Children;
                if (UdpObjectNode is AttributeNameNode)
                    UdpObject = (KBObject)((AttributeNameNode)UdpObjectNode).Element.Tag;
                else if (UdpObjectNode is ObjectPropertyNode)
                    UdpObject = (KBObject)((UnknownNode)UdpObjectNode.Children.Skip(1).First()).Element.Tag;
                //else
                   // K2BOutput.Message(new OutputError(new Message(string.Format("Unhandled udp format: {0}.", RightNode.Text)), new SourcePosition(RightNode.Part, RightNode.Node.Row, RightNode.Node.CharPosition)));
            }

            if (UdpObject != null)
            {
                if (UdpObjectNode.Node.Code == ExpressionType.ProgramName)
                {
                    // Objeto llamado como UDP pero sin la palabra UDP (Ej: XXX([parameters])
                    int ParmNo = 0;
                    foreach (AbstractNode parm in UdpParameters)
                    {
                        ParmNo++;
                        foreach (AbstractNode node in GetTokens(parm))
                        {
                            if (Dependency.DependableNode(node))
                                ExpandStructureDependencies(node, model, dependencies, ParmNo.ToString(), Dependency.Types.UdpParameter, UdpObject, GetNodeText(node), Dependency.Types.Variable, node.Part.KBObject);
                        }
                    }
                    ParmNo++;
                    ExpandStructureDependencies(LeftStructureTypeReference, model, dependencies, UdpObjectNode, GetNodeText(LeftNode), (Dependency.Types)LeftMostNode(LeftNode).Node.Token, LeftNode.Part.KBObject, ParmNo.ToString(), Dependency.Types.UdpParameter, UdpObject);
                }
            }
        }

        private static void AssignmentStatement(AssignmentNode assgn, KBModel model, CallTree dependencies)
        {
            AbstractNode LeftNode, RightNode;
            if (assgn.Left.Node == null)
            {
                // Cuando se invoca un método se procesa como si fuera: Objeto = Metodo
                LeftNode = assgn.Right.Children.First();
                RightNode = assgn.Right.Children.Skip(1).First();
            }
            else
            {
                LeftNode = assgn.Left;
                RightNode = assgn.Right;
            }
            AssignmentStatement(LeftNode, RightNode, model, dependencies);
        }

        private static StructureTypeReference GetStructureTypeReference(AbstractNode node, KBModel model)
        {
            Dependency.Types type;
            return GetStructureTypeReference(node, model, out type);
        }

        public static StructureTypeReference GetStructureTypeReference(AbstractNode node, KBModel model, out Dependency.Types type)
        {
            type = Dependency.Types.Variable;
            if (node is VariableNameNode && ((VariableNameNode)node).Variable != null)
            {
                AttCustomType CustomType = (((VariableNameNode)node).Variable).GetPropertyValue<AttCustomType>(Artech.Genexus.Common.Properties.ATT.DataType);
                if (CustomType.TypeCategory.Prefix == "ext")
                    return null;

                StructureTypeReference StructureReference = StructureTypeReference.DeserializeFromString(CustomType.Guid);
                return StructureReference;
            }
            else if (node is ObjectPropertyNode || node is ObjectMethodNode)
            {
                if (LeftMostNode(node) is AttributeNameNode)
                    return null;

                StructureTypeReference parentRef = GetStructureTypeReference(node.Children.First(), model, out type);
                if (node is ObjectMethodNode)
                    return parentRef;

                string ChildText = node.Children.Skip(1).First().Text;
                foreach (Artech.Common.Helpers.Structure.IStructureItem item in GetStructureSubStructures(parentRef, model))
                {
                    if (item.Name == ChildText)
                        if (item is SDTLevel)
                            return new StructureTypeReference(((SDTLevel)item).ItemEntity.Type, ((SDTLevel)item).ItemEntity.Id);
                        else if (item is TransactionLevel)
                        {
                            TransactionLevel TrnLvl = (TransactionLevel)item;
                            return new StructureTypeReference(TrnLvl.Transaction.Key.Type, TrnLvl.Transaction.Key.Id, StructureInfoProvider.GetLevelPrimaryKey(TrnLvl));
                        }
                        else if (item is SDTItem)
                            return null;
                        else if (item is TransactionAttribute)
                            return null;
                        else if (item is Artech.Genexus.Common.Parts.ExternalObject.ExternalObjectProperty)
                            return null;
                        else
                            return null;
                }
                return null;
            }
            return null;
        }

        private static string GetNodeText(AbstractNode node)
        {
            return GetNodeText(node, "");
        }

        private static string GetNodeText(AbstractNode node, string suffix)
        {
            // No se usa node.Text porque en el texto vienen comentarios del código.
            if (node is ObjectPropertyNode)
            {
                ObjectPropertyNode opn = (ObjectPropertyNode)node;
                return GetNodeText(opn.Target, "." + opn.PropertyName + suffix);
            }
            else if (node is ObjectMethodNode)
            {
                // The objective is to remove the method reference from the expression
                AbstractNode an = node.Children.First();
                while (an is ObjectMethodNode)
                    an = an.Children.First();
                return an.Text + suffix;
            }
            else if (node is VariableNameNode)
                return ((VariableNameNode)node).Text + suffix;
            else if (node is AttributeNameNode)
                return ((AttributeNameNode)node).Text + suffix;
            else
            {

            }
            return node.Text + suffix;
        }

        private static AbstractNode RemoveMethod(AbstractNode Token)
        {
            while (Token is ObjectMethodNode)
                Token = Token.Children.First();
            return Token;
        }

        private static void ExpandStructureDependencies(AbstractNode Node, KBModel model, CallTree dependencies, string left, Dependency.Types? leftType, KBObject leftObject, string right, Dependency.Types? rightType, KBObject rightObject)
        {
            ExpandStructureDependencies(GetStructureTypeReference(Node, model), model, dependencies, left, leftType, leftObject, right, rightType, rightObject, Node.Part, Node.Node.Row, Node.Node.CharPosition);
        }

        private static void ExpandStructureDependencies(StructureTypeReference StructureReference, KBModel model, CallTree dependencies, AbstractNode Node, string left, Dependency.Types? leftType, KBObject leftObject, string right, Dependency.Types? rightType, KBObject rightObject)
        {
            ExpandStructureDependencies(StructureReference, model, dependencies, left, leftType, leftObject, right, rightType, rightObject, Node.Part, Node.Node.Row, Node.Node.CharPosition);
        }

        private static void ExpandStructureDependencies(StructureTypeReference StructureReference, KBModel model, CallTree dependencies, string left, Dependency.Types? leftType, KBObject leftObject, string right, Dependency.Types? rightType, KBObject rightObject, KBObjectPart part, int row, int col)
        {
            Dependency leftItem = dependencies.AddItem(new Dependency(left, leftType, leftObject, dependencies.m_AttributeTree));
            Dependency rightItem = dependencies.AddItem(new Dependency(right, rightType, rightObject, dependencies.m_AttributeTree));
            dependencies.AddDependency(leftItem, rightItem, new SourcePosition(part, row, col));

            foreach (string itemMember in GetStructureMembers(StructureReference, model))
            {
                leftItem = dependencies.AddItem(new Dependency(left + "." + itemMember, leftType, leftObject, dependencies.m_AttributeTree));
                rightItem = dependencies.AddItem(new Dependency(right + "." + itemMember, rightType, rightObject, dependencies.m_AttributeTree));
                dependencies.AddDependency(leftItem, rightItem, new SourcePosition(part, row, col));
            }
        }

        private static IEnumerable<string> GetStructureMembers(StructureTypeReference StructureReference, KBModel model)
        {
            foreach (Artech.Common.Helpers.Structure.IStructureItem item in GetStructureSubStructures(StructureReference, model))
                foreach (string subItem in GetStructureMembers(item, ""))
                    yield return subItem;
        }

        private static IEnumerable<string> GetStructureMembers(Artech.Common.Helpers.Structure.IStructureItem item, string prefix)
        {
            if (item.IsLeafItem)
                yield return prefix + item.Name;
            else
                foreach (Artech.Common.Helpers.Structure.IStructureItem subItem in item.GetAllItems<Artech.Common.Helpers.Structure.IStructureItem>())
                    if (item != subItem)
                        foreach (string member in GetStructureMembers(subItem, item.Name + "."))
                            yield return member;
        }

        public static IEnumerable<Artech.Common.Helpers.Structure.IStructureItem> GetStructureSubStructures(StructureTypeReference StructureReference, KBModel model)
        {
            if (StructureReference == null)
                yield break;

            if (StructureReference.Key.Type == typeof(Transaction).GUID || StructureReference.Key.Type == typeof(SDT).GUID || StructureReference.Key.Type == typeof(ExternalObject).GUID)
            {
                KBObject kbobject = KBObject.Get(model, StructureReference.Key);
                if (kbobject is SDT)
                    foreach (Artech.Common.Helpers.Structure.IStructureItem item in ((SDT)kbobject).SDTStructure.Root.Items)
                        yield return item;
                else if (kbobject is Transaction)
                {
                    Transaction trn = (Transaction)kbobject;
                    TransactionLevel Level;
                    if (StructureReference.AdditionalInfo == null)
                        Level = trn.Structure.Root;
                    else
                        Level = trn.Structure.Root.GetLevelByKey(StructureReference.AdditionalInfo);

                    foreach (Artech.Common.Helpers.Structure.IStructureItem ta in Level.GetItems<Artech.Common.Helpers.Structure.IStructureItem>())
                        yield return ta;
                }
                else if (kbobject is ExternalObject)
                {
                    ExternalObject xobj = (ExternalObject)kbobject;
                    foreach (object item in xobj.EXOStructure.Items)
                    {
                        Artech.Common.Helpers.Structure.IStructureItem x = item as Artech.Common.Helpers.Structure.IStructureItem;
                        if (x != null)
                            yield return x;
                    }
                }
            }
            else
            {
                // SDT substructure. El código para cargar su estructura lo saqué descompilando el código de GX.
                SDTLevel level = Artech.Genexus.Common.Objects.Sdt.SDTLoader.GetItem(model, StructureReference.Key) as SDTLevel;
                if (level != null)
                    foreach (Artech.Common.Helpers.Structure.IStructureItem item in level.Items)
                        yield return item;
            }
        }

        private IEnumerable<string> GetStructureMembers(SDTLevel StructureItem)
        {
            return GetStructureMembers(StructureItem, "");
        }

        private IEnumerable<string> GetStructureMembers(SDTLevel StructureItem, string Prefix)
        {
            foreach (Artech.Common.Helpers.Structure.IStructureItem item in StructureItem.Items)
            {
                if (item is SDTItem)
                    yield return Prefix + item.Name;
                else if (item is SDTLevel)
                {
                    StructureTypeReference str = new StructureTypeReference(((SDTLevel)item).ItemEntity.Type, ((SDTLevel)item).ItemEntity.Id);

                    foreach (string member in GetStructureMembers((SDTLevel)item, Prefix + item.Name + "."))
                        yield return member;
                }
            }
        }

        private static IEnumerable<KeyValuePair<Dependency.Types, string>> GetLeftNode(AbstractNode node)
        {
            AbstractNode LeftNode = LeftMostNode(node);
            if (LeftNode is VariableNameNode && LeftNode.Node.Token == (int)TokensIds.FCT)
                LeftNode.Node.Token = (int)TokensIds.FUV;
            if (LeftNode is AttributeNameNode && ((AttributeNameNode)LeftNode).Element.Tag is Artech.Genexus.Common.Objects.Attribute)
                LeftNode.Node.Token = (int)TokensIds.FNA;

            yield return new KeyValuePair<Dependency.Types, string>((Dependency.Types)LeftNode.Node.Token, GetNodeText(node));
        }

        private static IEnumerable<AbstractNode> GetTokens(AbstractNode node)
        {
            if (node.Node == null)
                yield break;

            if (node is ObjectPropertyNode && LeftMostNode(node).Node.Token == (int)TokensIds.FCT)
            {
                LeftMostNode(node).Node.Token = (int)TokensIds.FUV;
                yield return node;
            }
            else if (node is ObjectMethodNode)
            {
                AbstractNode an = null;
                if (node.ChildrenCount > 1 && (an = node.Children.Skip(1).First()) is FunctionNode && ((FunctionNode)an).FunctionName == "udp")
                    // ProgramName.udp(<params>)
                    yield return node;
                else
                {
                    // The objective is to remove the method reference from the expression
                    an = node.Children.First();
                    while (an is ObjectMethodNode)
                        an = an.Children.First();

                    foreach (AbstractNode item in GetTokens(an))
                        yield return item;
                }
            }
            else if (node is FunctionNode && ((FunctionNode)node).Element.Name.Tag != null)
                // Call a objeto como funcion
                yield return node;
            else
            {
                if (node.ChildrenCount == 0)
                {
                    if (node is AttributeNameNode && node.Node.Token == (int)TokensIds.FCT)
                        node.Node.Token = (int)TokensIds.FNA;
                    if (node is VariableNameNode && node.Node.Token != (int)TokensIds.FUV)
                        node.Node.Token = (int)TokensIds.FUV;
                    yield return node;
                }
                else
                    foreach (AbstractNode token in node.Children)
                        foreach (AbstractNode item in GetTokens(token))
                            yield return item;
            }
        }


        private static AbstractNode LeftMostNode(AbstractNode Token)
        {
            if (Token is FunctionNode)
                return Token;
            if (Token.ChildrenCount == 0)
                return Token;
            return LeftMostNode(Token.Children.First());
        }

        public class CallTree
        {
            private KBModel m_Model;
            public readonly AttributeTree m_AttributeTree;

            public CallTree(KBModel model, AttributeTree AttributeTree)
            {
                m_Model = model;
                this.m_AttributeTree = AttributeTree;
            }

            private Dictionary<Dependency.Identifier, Dependency> m_Items = new Dictionary<Dependency.Identifier, Dependency>();
            public IEnumerable<Dependency> Items { get { return m_Items.Values; } }

            public Dependency AddItem(Artech.Genexus.Common.Objects.Attribute Attribute, AttributeTree AttributeTree)
            {
                return AddItem(new Dependency(Attribute, AttributeTree));
            }

            public Dependency AddItem(Dependency Item)
            {
                Dependency actualItem = Get(Item.Id);
                if (actualItem != null)
                    return actualItem;
                m_Items[Item.Id] = Item;
                return Item;
            }

            public void AddDependency(Dependency Item, Dependency ItemDependsOn, Artech.Common.Location.IPosition Position)
            {
                AddItem(Item).AddDependency(ItemDependsOn, Position);
            }

            public Dependency Get(Artech.Genexus.Common.Objects.Attribute kbobject)
            {
                return Get(new Dependency.Identifier(kbobject.Name, Dependency.Types.Attribute, kbobject));
            }

            public Dependency Get(Dependency.Identifier Id)
            {
                Dependency Item;
                if (m_Items.TryGetValue(Id, out Item))
                    return Item;
                return null;
            }
        }

        [XmlRoot(ElementName = "LineageRoot")]
        public class XMLLineageRoot
        {
            internal KBModel m_Model;

            [XmlArray(ElementName = "Items")]
            [XmlArrayItem(ElementName = "Item")]
            public XMLItem[] ItemsXML
            {
                get
                {
                    List<XMLItem> Tables = new List<XMLItem>();
                    foreach (Dependency d in Items)
                        Tables.Add(new XMLItem(d, this));
                    return Tables.ToArray();
                }
                set {; }
            }

            [XmlArray(ElementName = "Tables")]
            [XmlArrayItem(ElementName = "Table")]
            public XMLTable[] Tables
            {
                get
                {
                    Dictionary<int, XMLTable> Tables = new Dictionary<int, XMLTable>();
                    foreach (XMLObject d in Objects)
                        if (d.Table != null && !Tables.ContainsKey(d.Table.Id))
                            Tables[d.Table.Id] = new XMLTable(d.Table);
                    return Tables.Values.ToArray();
                }
                set {; }
            }

            [XmlArray(ElementName = "Objects")]
            [XmlArrayItem(ElementName = "Object")]
            public XMLObject[] Objects
            {
                get
                {
                    Dictionary<Guid, XMLObject> XMLObjects = new Dictionary<Guid, XMLObject>();
                    foreach (Dependency d in Items)
                    {
                        AddObject(XMLObjects, d.Attribute);
                        foreach (KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>> Dependency in d.DependsOn)
                        {
                            AddObject(XMLObjects, Dependency.Key.KBObject);
                            foreach (Artech.Common.Location.IPosition pos in Dependency.Value)
                            {
                                if (pos is IKBObjectPosition)
                                    AddObject(XMLObjects, KBObject.Get(m_Model, ((IKBObjectPosition)pos).Object));
                            }
                        }
                    }
                    return XMLObjects.Values.ToArray();
                }
                set {; }
            }

            private Dictionary<Dependency.Identifier, Dependency> m_Items = new Dictionary<Dependency.Identifier, Dependency>();
            public IEnumerable<Dependency> Items
            {
                get
                {
                    return m_Items.Values;
                }
            }

            private static void AddObject(Dictionary<Guid, XMLObject> XMLObjects, KBObject KBObject)
            {
                if (KBObject != null && !XMLObjects.ContainsKey(KBObject.Guid))
                    XMLObjects[KBObject.Guid] = new XMLObject(KBObject);
            }

            private XMLLineageRoot() { }
            public XMLLineageRoot(KBModel model)
            {
                m_Model = model;
            }

            public void AddDependency(Dependency d)
            {
                if (!m_Items.ContainsKey(d.Id))
                    m_Items[d.Id] = d;
            }
        }

        public class XMLItem
        {
            private Dependency m_Dependency;
            private XMLLineageRoot m_Root;
            private Artech.Genexus.Common.Objects.Attribute m_Attribute;
            private Artech.Genexus.Common.Objects.Attribute Attribute
            {
                get
                {
                    if (m_Attribute == null)
                        if (Type == Dependency.Types.Attribute)
                            m_Attribute = Artech.Genexus.Common.Objects.Attribute.Get(m_Root.m_Model, m_Dependency.Id.Name);
                    return m_Attribute;
                }
                set {; }
            }

            [XmlAttribute]
            public Dependency.Types Type { get { return (Dependency.Types)m_Dependency.Id.Type; } set {; } }

            [XmlAttribute]
            public string Name { get { return m_Dependency.Id.Name; } set {; } }

            [XmlAttribute(AttributeName = "Object")]
            public string ObjectName
            {
                get
                {
                    if (Type == Dependency.Types.Variable)
                        return m_Dependency.Id.KBObject.QualifiedName.ToString();
                    if (Type == Dependency.Types.ProgramParameter)
                        return m_Dependency.Id.KBObject.QualifiedName.ToString();
                    return null;
                }
                set {; }
            }

            [XmlArray(ElementName = "DependensOn")]
            [XmlArrayItem(ElementName = "Item")]
            public XMLDependsOnItem[] DependensOn
            {
                get
                {
                    int i = 0;
                    XMLDependsOnItem[] arr = new XMLDependsOnItem[m_Dependency.DependsOnCount];
                    foreach (KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>> Dependency in m_Dependency.DependsOn)
                    {
                        XMLDependsOnItem Item = new XMLDependsOnItem(m_Root.m_Model);
                        Item.Type = (Dependency.Types)Dependency.Key.Type;
                        Item.Name = Dependency.Key.Name;
                        Item.ObjectName = Dependency.Key.ObjectName;
                        Item.Module = Dependency.Key.Module;
                        Item.References = Dependency.Value;

                        arr[i] = Item;
                        i++;
                    }
                    return arr;
                }
                set {; }
            }

            private XMLItem()
            {
                // Do not remove. Serializer requires it.
            }

            public XMLItem(Dependency dependency, XMLLineageRoot root)
            {
                m_Dependency = dependency;
                m_Root = root;
            }
        }

        public class XMLDependsOnItem
        {
            [XmlAttribute]
            public Dependency.Types Type;

            [XmlAttribute]
            public string Name;

            [XmlAttribute(AttributeName = "Object")]
            public string ObjectName;

            [XmlAttribute(AttributeName = "Module")]
            public string Module;

            [XmlIgnore]
            public HashSet<Artech.Common.Location.IPosition> References;

            [XmlArray(ElementName = "References")]
            [XmlArrayItem(ElementName = "Object")]
            public XMLReference[] ReferencesArr
            {
                get
                {
                    int i = 0;
                    XMLReference[] arr = new XMLReference[References.Count()];
                    foreach (Artech.Common.Location.IPosition pos in References)
                    {
                        if (pos is IKBObjectPosition)
                        {
                            IKBObjectPosition ObjPos = (IKBObjectPosition)pos;
                            KBObject kbo = KBObject.Get(m_Model, ObjPos.Object);
                            if (kbo != null)
                            {
                                XMLReference op = new XMLReference(kbo);
                                op.PartGUID = ObjPos.Part;
                                if (ObjPos is SourcePosition)
                                {
                                    SourcePosition spos = (SourcePosition)ObjPos;
                                    op.Row = spos.Line;
                                    op.Col = spos.Char;
                                }
                                else { }

                                arr[i] = op;
                                i++;
                            }
                        }
                        else if (pos is KBObjectPropertyPosition)
                        {
                            KBObjectPropertyPosition ObjPos = (KBObjectPropertyPosition)pos;
                            KBObject kbo = KBObject.Get(m_Model, ObjPos.ObjectKey);
                            if (kbo != null)
                            {
                                XMLReference op = new XMLReference(kbo);
                                op.Property = ObjPos.PropertyName;

                                arr[i] = op;
                                i++;
                            }
                        }
                        else { }
                    }
                    return arr;
                }
                set {; }
            }

            private KBModel m_Model;
            private XMLDependsOnItem()
            {
            }
            public XMLDependsOnItem(KBModel kbmodel)
            {
                m_Model = kbmodel;
            }

            public class XMLReference
            {
                [XmlAttribute]
                public string Type
                {
                    get { return m_KBObject.TypeDescriptor.Name; }
                    set {; }
                }

                [XmlAttribute]
                public string Name
                {
                    get { return m_KBObject.Name; }
                    set {; }
                }

                [XmlAttribute]
                public string Module
                {
                    get
                    {
                        if (m_KBObject is Artech.Genexus.Common.Objects.Attribute)
                            return null;
                        return m_KBObject.Module.QualifiedName.ToString();
                    }
                    set {; }
                }

                [XmlIgnore]
                public Guid PartGUID;

                [XmlAttribute(AttributeName = "Part")]
                public string ObjectPartName
                {
                    get
                    {
                        if (!PartGUID.Equals(Guid.Empty))
                        {
                            KBObjectPartDescriptor PartDescriptor = KBObjectPartDescriptor.Get(PartGUID);
                            if (PartDescriptor != null)
                                return PartDescriptor.Description;
                        }
                        return null;
                    }
                    set {; }
                }

                [XmlAttribute]
                public string Property;

                private int m_Row;
                [XmlAttribute(AttributeName = "Row")]
                public int Row { get { return m_Row; } set { m_Row = value; RowSpecified = true; } }

                [XmlIgnore]
                public bool RowSpecified;

                private int m_Col;
                [XmlAttribute(AttributeName = "Col")]
                public int Col { get { return m_Col; } set { m_Col = value; ColSpecified = true; } }

                [XmlIgnore]
                public bool ColSpecified;

                private XMLReference()
                {
                }

                private KBModel m_Model { get { return m_KBObject.Model; } }
                private KBObject m_KBObject;
                public XMLReference(KBObject kbobject)
                {
                    m_KBObject = kbobject;
                }
            }
        }

        public class XMLTable
        {
            private Table m_table;

            public XMLTable(Table table)
            {
                m_table = table;
            }

            private XMLTable() { }

            [XmlAttribute(AttributeName = "Name")]
            public string Name
            {
                get { return m_table.Name; }
                set {; }
            }

            [XmlAttribute]
            public string Description
            {
                get { return m_table.Description; }
                set {; }
            }

            /*       private string m_Module;
                   [XmlAttribute]
                   public string Module
                   {
                       get
                       {
                           if (m_Module == null)
                               m_Module = m_table.BestAssociatedTransaction.Module.QualifiedName.ToString();
                           return m_Module;
                       }
                       set { m_Module = value; }
                   }

                   [XmlAttribute]
                   public string GUID
                   {
                       get { return m_table.Guid.ToString(); }
                       set {; }
                   }

                   bool m_DataViewInfoLoaded = false;
                   DataView m_DataView;
                   DataViewStructurePlatform m_DataViewPlatform;
                   DataView DataView
                   {
                       get
                       {
                           if (m_DataViewInfoLoaded)
                               return m_DataView;

                           foreach (EntityReference current in m_table.GetReferencesTo())
                           {
                               if (current.From.Type != typeof(DataView).GUID)
                                   continue;

                               DataView dv = DataView.Get(m_table.Model, current.From.Id);
                               if (dv == null)
                                   continue;

                               DataStoreCategoryReference dvDataStore = dv.GetPropertyValue<DataStoreCategoryReference>(Artech.Genexus.Common.Properties.XFL.DataStore);
                               foreach (DataViewStructurePlatform item in dv.DataViewStructure.Platforms)
                               {
                                   if (dvDataStore.Definition.Dbms == item.Dbms)
                                   {
                                       m_DataViewPlatform = item;
                                       break;
                                   }
                               }
                               if (m_DataViewPlatform != null)
                               {
                                   m_DataView = dv;
                                   break;
                               }
                           }
                           m_DataViewInfoLoaded = true;
                           return m_DataView;
                       }
                   }
                   
            [XmlAttribute]
            public string ExternalName
            {
                get
                {
                    if (DataView != null)
                        return m_DataViewPlatform.Properties.GetPropertyValue<string>(Artech.Genexus.Common.Properties.DV_SQLSERVER.Name);
                    return null;
                }
                set {; }
            }
            
            [XmlAttribute]
            public string ExternalDescription
            {
                get
                {
                    if (DataView == null)
                        return null;

                    return Table.Get(DataView.Model, ExternalName)?.Description;
                }
                set {; }
            }*/
        }

        public class XMLObject
        {
            private KBObject m_KBObject;
            public XMLObject(KBObject kbobject)
            {
                m_KBObject = kbobject;
                
            }
            private XMLObject() { }

            [XmlIgnore]
            public KBObject KBObject
            {
                get { return m_KBObject; }
            }

            [XmlIgnore]
            public Artech.Genexus.Common.Objects.Attribute Attribute
            {
                get { return m_KBObject as Artech.Genexus.Common.Objects.Attribute; }
            }

            private Table m_Table;
            [XmlIgnore]
            public Table Table
            {
                get
                {
                    if (m_Table == null && Attribute != null)
                    {
                        foreach (EntityReference or in Attribute.GetReferencesTo())
                            if (or.From.Type == typeof(Table).GUID)
                            {
                                Table Table = Artech.Genexus.Common.Objects.Table.Get(Attribute.Model, or.From.Id);
                                if (Table != null)
                                {
                                    m_Table = Table;
                                    break;
                                }
                            }
                    }
                    return m_Table;
                }
            }

            [XmlAttribute]
            public string Type
            {
                get { return m_KBObject.TypeDescriptor.Name; }
                set {; }
            }

            [XmlAttribute]
            public string Name
            {
                get { return m_KBObject.Name; }
                set {; }
            }

            [XmlAttribute]
            public string Module
            {
                get
                {
                    if (m_KBObject is Artech.Genexus.Common.Objects.Attribute)
                        return null;
                    return m_KBObject.Module.QualifiedName.ToString();
                }
                set {; }
            }

            [XmlAttribute]
            public string Description
            {
                get { return m_KBObject.Description; }
                set {; }
            }

            [XmlAttribute]
            public string GUID
            {
                get { return m_KBObject.Guid.ToString(); }
                set {; }
            }

            [XmlAttribute(AttributeName = "Table")]
            public string TableName
            {
                get
                {
                    if (Table != null)
                        return Table.Name;
                    return null;
                }
                set {; }
            }

            [XmlAttribute]
            public string ExternalName
            {
                get
                {
                    if (!(m_KBObject is Artech.Genexus.Common.Objects.Attribute))
                        return null;

                    foreach (EntityReference current in m_KBObject.GetReferencesTo())
                    {
                        if (current.From.Type != typeof(DataView).GUID)
                            continue;

                        DataView dv = DataView.Get(m_KBObject.Model, current.From.Id);
                        if (dv == null)
                            continue;

                        foreach (DataViewAttribute dva in dv.DataViewStructure.Attributes)
                            if (string.Compare(dva.InternalName, m_KBObject.Name, true) == 0)
                                return dva.ExternalName;
                    }
                    return null;
                }
                set {; }
            }
        }

        public class Dependency
        {
            public Artech.Genexus.Common.Objects.Attribute Attribute { get { return KBObject as Artech.Genexus.Common.Objects.Attribute; } }
            public Identifier Id { get; set; }
            public Types? Type { get { return Id.Type; } }
            public string Name { get { return Id.Name; } }
            public KBObject KBObject { get { return Id.KBObject; } }
            public bool AnalyzeCallers { get { if (Attribute == null && DependsOnCount > 0) return false; return Id.AnalyzeCallers; } }
            public string ObjectName
            {
                get
                {
                    if (Type == Types.Variable)
                        return Id.KBObject.Name;
                    if (Type == Types.ProgramParameter)
                        return Id.KBObject.Name;
                    if (Type == Types.Attribute)
                        return null;
                    return null;
                }
                set {; }
            }

            public readonly AttributeTree AttributeTree;
            public string Module
            {
                get
                {
                    if (Type == Types.Variable)
                        return Id.KBObject.Module.QualifiedName.ToString();
                    if (Type == Types.ProgramParameter)
                        return Id.KBObject.Module.QualifiedName.ToString();
                    return null;
                }
            }

            private KBModel m_Model { get { return Id.KBObject.Model; } }

            public Dependency(Artech.Genexus.Common.Objects.Attribute attribute, AttributeTree AttributeTree) : this(attribute.Name, Types.Attribute, attribute, AttributeTree)
            {
            }

            public Dependency(string Name, Types? Type, KBObject KBObject, AttributeTree AttributeTree)
            {
                Id = new Identifier(Name, Type, KBObject);
                m_DependsOn = new Dictionary<Dependency, HashSet<Artech.Common.Location.IPosition>>();
                this.AttributeTree = AttributeTree;
            }

            private bool isEnumerating = false;
            private Dictionary<Dependency, HashSet<Artech.Common.Location.IPosition>> m_DependsOn;
            public IEnumerable<KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>>> DependsOn
            {
                get
                {
                    isEnumerating = true;
                    foreach (KeyValuePair<Dependency, HashSet<Artech.Common.Location.IPosition>> item in m_DependsOn)
                        yield return item;
                    isEnumerating = false;
                }
            }

            public int DependsOnCount
            {
                get
                {
                    return m_DependsOn.Count;
                }
            }

            public static bool DependableNode(AbstractNode item)
            {
                if (item == null)
                    return false;
                AbstractNode LeftMost = LeftMostNode(item);
                if (LeftMost.Node == null)
                    return false;
                return DependableNode(LeftMost.Node.Token);
            }

            public static bool DependableNode(int token)
            {
                return token == (int)TokensIds.FNA || token == (int)TokensIds.FUV || token == (int)TokensIds.FNC;
            }

            public static Types? NodeType(AbstractNode node)
            {
                AbstractNode leftMostNode = LeftMostNode(node);

                if (Enum.IsDefined(typeof(Types), leftMostNode.Node.Token))
                    return (Dependency.Types)leftMostNode.Node.Token;
                return null;
            }

            public void AddDependency(Dependency itemDependsOn, Artech.Common.Location.IPosition position)
            {
                if (Equals(Id, itemDependsOn.Id))
                    return;

                if (!m_DependsOn.ContainsKey(itemDependsOn))
                {
                    if (isEnumerating)
                    {
                        //K2BOutput.Warning($"Dependency may not be analyzed: {this.ToString()}->{itemDependsOn.ToString()}", position);
                        return;
                    }
                    else
                        m_DependsOn[itemDependsOn] = new HashSet<Artech.Common.Location.IPosition>();
                }

                if (position == null)
                    return;

                HashSet<Artech.Common.Location.IPosition> references = m_DependsOn[itemDependsOn];
                if (!references.Contains(position))
                    references.Add(position);
            }

            public override bool Equals(object obj)
            {
                if (obj is Dependency)
                    return this.Id.Equals(((Dependency)obj).Id);
                return false;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public override string ToString()
            {
                return Name + ":" + Type.ToString();
            }

            public class Identifier
            {
                public string Name { get; private set; }
                public Types? Type { get; private set; }
                public KBObject KBObject { get; private set; }
                private string lcName;
                private bool IsUdpReference = false;
                public bool AnalyzeCallers
                {
                    get
                    {
                        if (Type == Types.Attribute)
                            return true;
                        if (Type == Types.ProgramParameter && !IsUdpReference)
                            return true;
                        return false;
                    }
                }

                private Identifier() { } // Required for XML serialization. DO NOT use.

                public Identifier(string Name, Types? Type, KBObject KBObject)
                {
                    if (Type == Types.UdpParameter)
                    {
                        IsUdpReference = true;
                        Type = Types.ProgramParameter;
                    }
                    this.Type = Type;

                    if (this.Type == Types.Variable)
                    {
                        this.Name = Name + " in " + KBObject.QualifiedName.ToString();
                        this.KBObject = KBObject;
                    }
                    else if (this.Type == Types.ProgramParameter)
                    {
                        this.Name = Name + " of " + KBObject.QualifiedName.ToString();
                        this.KBObject = KBObject;
                    }
                    else if (this.Type == Types.Attribute)
                    {
                        this.Name = Name;
                        this.KBObject = Artech.Genexus.Common.Objects.Attribute.Get(KBObject.Model, Name);
                    }
                    else
                    {
                        this.Name = Name;
                        this.KBObject = null;
                    }
                    this.lcName = this.Name.ToLower();
                }

                public override string ToString()
                {
                    return Type.ToString() + ":" + Name;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is Identifier))
                        return false;
                    Identifier objMember = (Identifier)obj;
                    return objMember.lcName == lcName && objMember.Type == Type;
                }

                public override int GetHashCode()
                {
                    return new { Name, Type }.GetHashCode();
                }
            }

            public enum Types
            {
                Attribute = 2,
                Constant = 3,
                Variable = 23,
                Program = 28,
                Property = 29,

                ProgramParameter = 10000,
                UdpParameter,
            }
        }
    }

    public class CallTree
    {
    }
}
