using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.WebForm;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class ResponsiveSmoothActions
    {
        public static void Run(string action)
        {
            string title = "KBDoctor - Responsive/Smooth actions";
            KBDoctorOutput.StartSection(title);
            switch (action)
            {
                case "responsiveDefaults":
                    ConvertDefaultResponsiveProperties();
                    break;
                case "smoothDefaults":
                    ConvertDefaultWebUXToSmooth();
                    break;
                case "generatedPreviousToSmooth":
                    ConvertGeneratedPreviousCompatibleToSmooth();
                    break;
                case "auditSmoothResponsive":
                    AuditSmoothResponsive();
                    break;
                case "resetDefaultMasterPage":
                    ResetDefaultMasterPageReferences();
                    break;
                case "webFormToAbstractEditor":
                    WebFormToAbstractEditor();
                    break;
                default:
                    KBDoctorOutput.Error("Unknown action: " + action);
                    KBDoctorOutput.EndSection(title, false);
                    return;
            }

            KBDoctorOutput.EndSection(title, true);
        }

        private static void WebFormToAbstractEditor()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());

            int selected = 0;
            int changed = 0;
            int alreadyAbstract = 0;
            int converted = 0;
            int notConverted = 0;
            int failed = 0;
            int reportRows = 0;
            string reportTitle = "KBDoctor - WebForm to Abstract Editor";
            string outputFile = Functions.CreateOutputFile(UIServices.KB, reportTitle);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(reportTitle);
            writer.AddTableHeader(new string[] { "Name", "Module", "MasterPage", "Style", "TypeEditor", "Conversion Result" });

            output.AddLine("Object,MasterPage Default,Style Default,Was Abstract,Is Abstract,Result");

            foreach (WebPanel webPanel in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                selected += 1;

                if (!Utility.IsUserEditableObject(webPanel))
                {
                    failed += 1;
                    output.AddLine(webPanel.Name + ",False,False,False,False,Read-only object");
                    continue;
                }

                bool wasAbstract = WebPanelFormRootIsAbstract(webPanel.WebForm);
                string masterPageBefore = SafeGetPropertyValueString(webPanel, Properties.TRN.MasterPage);
                string styleBefore = SafeGetPropertyValueString(webPanel, Properties.TRN.Theme);
                bool masterPageDefault = ResetPropertyToDefault(webPanel, Properties.TRN.MasterPage);
                bool styleDefault = ResetPropertyToDefault(webPanel, Properties.TRN.Theme);
                bool conversionChanged = false;
                string conversionMessage;

                try
                {
                    conversionChanged = TryConvertWebFormToAbstract(webPanel, out conversionMessage);
                    webPanel.Save();
                    changed += 1;
                }
                catch (Exception e)
                {
                    failed += 1;
                    output.AddErrorLine("WebForm to Abstract Editor failed for " + webPanel.Name + ": " + e.ToString());
                    output.AddLine(webPanel.Name + "," + masterPageDefault.ToString() + "," + styleDefault.ToString() + "," + wasAbstract.ToString() + ",False,Save failed: " + e.Message);
                    continue;
                }

                bool isAbstract = WebPanelFormRootIsAbstract(webPanel.WebForm);
                string result;
                if (wasAbstract)
                {
                    alreadyAbstract += 1;
                    result = "Already abstract";
                }
                else if (conversionChanged && isAbstract)
                {
                    converted += 1;
                    result = "Converted to abstract editor. " + conversionMessage;
                }
                else if (isAbstract)
                {
                    converted += 1;
                    result = "Converted to abstract editor by save/defaults. " + conversionMessage;
                }
                else
                {
                    notConverted += 1;
                    result = "Still old HTML editor. " + conversionMessage;
                }

                string masterPageAfter = SafeGetPropertyValueString(webPanel, Properties.TRN.MasterPage);
                string styleAfter = SafeGetPropertyValueString(webPanel, Properties.TRN.Theme);
                bool objectChanged = conversionChanged
                    || wasAbstract != isAbstract
                    || !string.Equals(masterPageBefore, masterPageAfter, StringComparison.Ordinal)
                    || !string.Equals(styleBefore, styleAfter, StringComparison.Ordinal);

                if (objectChanged)
                {
                    writer.AddTableData(new[]
                    {
                        Functions.linkObject(webPanel),
                        Html(webPanel.Module == null ? "" : webPanel.Module.Name),
                        Html(masterPageAfter),
                        Html(styleAfter),
                        Html((wasAbstract ? "Abstract Layout" : "HTML") + " -> " + (isAbstract ? "Abstract Layout" : "HTML")),
                        Html(result)
                    });
                    reportRows += 1;
                }

                output.AddLine(webPanel.Name + "," + masterPageDefault.ToString() + "," + styleDefault.ToString() + "," + wasAbstract.ToString() + "," + isAbstract.ToString() + "," + result);
            }

            writer.AddFooter();
            writer.Close();
            if (reportRows > 0)
            {
                output.AddLine("Report: " + outputFile);
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            else
            {
                output.AddLine("No WebPanels changed. Report was generated without rows: " + outputFile);
            }

            output.AddLine("");
            output.AddLine("Selected: " + selected.ToString()
                + ". Saved: " + changed.ToString()
                + ". Already abstract: " + alreadyAbstract.ToString()
                + ". Converted: " + converted.ToString()
                + ". Still old HTML editor: " + notConverted.ToString()
                + ". Failed: " + failed.ToString() + ".");
        }

        private static string SafeGetPropertyValueString(KBObject obj, string propertyName)
        {
            if (!obj.ContainsPropertyDefinition(propertyName))
            {
                return "";
            }

            return obj.GetPropertyValueString(propertyName);
        }

        private static string Html(string text)
        {
            return SecurityElement.Escape(text ?? "");
        }

        private static bool ResetPropertyToDefault(KBObject obj, string propertyName)
        {
            if (!obj.ContainsPropertyDefinition(propertyName))
            {
                return false;
            }

            if (!obj.IsPropertyDefault(propertyName) && obj.CanResetValue(propertyName))
            {
                obj.ResetProperty(propertyName);
            }

            return obj.IsPropertyDefault(propertyName);
        }

        private static bool TryConvertWebFormToAbstract(WebPanel webPanel, out string message)
        {
            message = "";
            WebFormPart webForm = webPanel.WebForm;
            if (webForm == null || string.IsNullOrEmpty(webForm.EditableContent))
            {
                message = "No explicit WebForm content";
                return false;
            }

            XmlDocument formDocument = new XmlDocument();
            string originalEditableContent = webForm.EditableContent;
            formDocument.LoadXml(webForm.EditableContent);
            bool resolvedStoredReferences = ResolveStoredReferencesBeforeLayoutConversion(webPanel, formDocument);
            string editableXmlBeforeConversion = formDocument.OuterXml;

            int rootFormId = MultiFormSerializer.GetRootFormId(formDocument);
            List<MultiFormSerializer.Form> forms = MultiFormSerializer.GetForms(formDocument).ToList();
            MultiFormSerializer.Form rootForm = forms.FirstOrDefault(form => form.Id == rootFormId);
            if (rootForm == null)
            {
                message = "Root form not found";
                return false;
            }

            if (rootForm.Handler.XmlTypeName == MultiForm.Layout.XmlTypeName)
            {
                message = "Root form is already layout";
                return false;
            }

            if (rootForm.Handler.XmlTypeName != MultiForm.Html.XmlTypeName)
            {
                message = "Unsupported root form type: " + rootForm.Handler.XmlTypeName;
                return false;
            }

            string rootHtmlXmlBeforeConversion = rootForm.RootElement.OuterXml;
            HashSet<string> controlNames = GetExistingControlNames(webPanel, forms);
            MultiForm_GetUniqueControlName getUniqueControlName = (baseName, _) => GetUniqueControlName(controlNames, baseName);
            XmlElement layoutRoot = MultiForm.Layout.CreateForm(getUniqueControlName);
            MultiForm.Layout.ConvertForm(webPanel, layoutRoot, rootForm.Handler, rootForm.RootElement, controlNames);
            string layoutRootXmlAfterConversion = layoutRoot.OuterXml;

            List<MultiFormSerializer.Form> newForms = new List<MultiFormSerializer.Form>();
            foreach (MultiFormSerializer.Form form in forms)
            {
                if (form.Id != rootFormId)
                {
                    newForms.Add(form);
                }
            }

            newForms.Add(new MultiFormSerializer.Form(rootFormId, MultiForm.Layout, layoutRoot));
            XmlDocument convertedDocument = MultiFormSerializer.SaveForms(rootFormId, newForms);
            int classReplacements = ApplyClassReplacements(webPanel, convertedDocument);
            int cdataTerminators = ReplaceInvalidCDataTerminators(convertedDocument);
            string convertedXml = ReplaceStoredReferenceIds(webPanel, convertedDocument.OuterXml);
            convertedXml = SanitizeEscapedCDataMarkers(convertedXml, ref cdataTerminators);
            convertedXml = SanitizeConvertedXml(webPanel, convertedXml, ref cdataTerminators);
            DumpXmlIfInvalid(webPanel, convertedXml, "Converted XML after var/att replacement");
            webForm.EditableContent = convertedXml;
            try
            {
#pragma warning disable 0612
                webForm.EditableToStored();
#pragma warning restore 0612
            }
            catch (Exception)
            {
                DumpConversionFailureContext(
                    webPanel,
                    "EditableToStored failure",
                    originalEditableContent,
                    editableXmlBeforeConversion,
                    rootHtmlXmlBeforeConversion,
                    layoutRootXmlAfterConversion,
                    convertedXml,
                    webForm.Document == null ? null : webForm.Document.OuterXml);
                throw;
            }

            message = "HTML root converted with WebLayoutHandler"
                + (resolvedStoredReferences ? " after StoredToEditable" : "")
                + (classReplacements > 0 ? ". Class replacements: " + classReplacements.ToString() : "")
                + (cdataTerminators > 0 ? ". Replaced CDATA terminators: " + cdataTerminators.ToString() : "");
            return true;
        }

        private static int ApplyClassReplacements(WebPanel webPanel, XmlDocument convertedDocument)
        {
            Dictionary<string, string> replacements = LoadClassReplacements();
            if (replacements.Count == 0)
            {
                return 0;
            }

            int changed = 0;
            ApplyClassReplacements(convertedDocument.DocumentElement, replacements, ref changed);
            if (changed > 0)
            {
                CommonServices.Output.AddLine(webPanel.Name + ": class replacements applied: " + changed.ToString());
            }

            return changed;
        }

        private static void ApplyClassReplacements(XmlNode node, Dictionary<string, string> replacements, ref int changed)
        {
            if (node == null)
            {
                return;
            }

            if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (IsClassAttribute(attribute.Name) && replacements.ContainsKey(attribute.Value))
                    {
                        attribute.Value = replacements[attribute.Value];
                        changed += 1;
                    }
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                ApplyClassReplacements(child, replacements, ref changed);
            }
        }

        private static bool IsClassAttribute(string attributeName)
        {
            return string.Equals(attributeName, "class", StringComparison.OrdinalIgnoreCase)
                || string.Equals(attributeName, "cellClass", StringComparison.OrdinalIgnoreCase)
                || string.Equals(attributeName, "rowClass", StringComparison.OrdinalIgnoreCase)
                || string.Equals(attributeName, "selectedClass", StringComparison.OrdinalIgnoreCase)
                || string.Equals(attributeName, "unselectedClass", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> LoadClassReplacements()
        {
            Dictionary<string, string> replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string filePath = Path.Combine(UIServices.KB.CurrentKB.UserDirectory, "KBDoctorWebFormClassReplacements.csv");
            if (!File.Exists(filePath))
            {
                File.WriteAllLines(filePath, new[]
                {
                    "# SourceClass,TargetClass",
                    "BotonGrande,Button"
                });
                CommonServices.Output.AddLine("Created class replacement example file: " + filePath);
            }

            foreach (string line in File.ReadAllLines(filePath))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length == 0 || trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                string[] parts = trimmedLine.Split(new[] { ',', ';' }, 2);
                if (parts.Length < 2)
                {
                    CommonServices.Output.AddWarningLine("Invalid class replacement line: " + line);
                    continue;
                }

                string sourceClass = parts[0].Trim();
                string targetClass = parts[1].Trim();
                if (sourceClass.Length == 0 || targetClass.Length == 0)
                {
                    CommonServices.Output.AddWarningLine("Invalid class replacement line: " + line);
                    continue;
                }

                replacements[sourceClass] = targetClass;
            }

            CommonServices.Output.AddLine("Loaded class replacements: " + replacements.Count.ToString() + " from " + filePath);
            return replacements;
        }

        private static string SanitizeEscapedCDataMarkers(string xml, ref int replacements)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return xml;
            }

            replacements += CountOccurrences(xml, "&amp;lt;![CDATA[");
            replacements += CountOccurrences(xml, "]]&amp;gt;");
            replacements += CountOccurrences(xml, "&lt;![CDATA[");
            replacements += CountOccurrences(xml, "]]&gt;");

            return xml
                .Replace("&amp;lt;![CDATA[", "")
                .Replace("]]&amp;gt;", "")
                .Replace("&lt;![CDATA[", "")
                .Replace("]]&gt;", "");
        }

        private static void DumpConversionFailureContext(
            WebPanel webPanel,
            string stage,
            string originalEditableContent,
            string editableXmlBeforeConversion,
            string rootHtmlXmlBeforeConversion,
            string layoutRootXmlAfterConversion,
            string convertedXml,
            string currentDocumentXml)
        {
            IOutputService output = CommonServices.Output;
            output.AddErrorLine("WebForm conversion diagnostic for " + webPanel.Name + " - " + stage);
            output.AddLine("CDATA terminator counts: original="
                + CountOccurrences(originalEditableContent, "]]>").ToString()
                + ", editableBeforeConversion=" + CountOccurrences(editableXmlBeforeConversion, "]]>").ToString()
                + ", rootHtml=" + CountOccurrences(rootHtmlXmlBeforeConversion, "]]>").ToString()
                + ", layoutRoot=" + CountOccurrences(layoutRootXmlAfterConversion, "]]>").ToString()
                + ", converted=" + CountOccurrences(convertedXml, "]]>").ToString()
                + ", currentDocument=" + CountOccurrences(currentDocumentXml, "]]>").ToString());
            DumpXml(webPanel, originalEditableContent, "Original EditableContent");
            DumpXml(webPanel, editableXmlBeforeConversion, "After StoredToEditable before conversion");
            DumpXml(webPanel, rootHtmlXmlBeforeConversion, "Root HTML form passed to ConvertForm");
            DumpXml(webPanel, layoutRootXmlAfterConversion, "Layout root returned by ConvertForm");
            DumpXml(webPanel, convertedXml, "Converted XML assigned to EditableContent");
            DumpXml(webPanel, currentDocumentXml, "Current WebFormPart.Document");
        }

        private static int CountOccurrences(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count += 1;
                index += value.Length;
            }

            return count;
        }

        private static string SanitizeConvertedXml(WebPanel webPanel, string xml, ref int cdataTerminators)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.LoadXml(xml);
            }
            catch (XmlException)
            {
                DumpXml(webPanel, xml, "Invalid converted XML before CDATA terminator sanitation");
                throw;
            }

            int replacements = ReplaceInvalidCDataTerminators(document);
            cdataTerminators += replacements;
            return replacements == 0 ? xml : document.OuterXml;
        }

        private static int ReplaceInvalidCDataTerminators(XmlDocument document)
        {
            int replacements = 0;
            ReplaceInvalidCDataTerminators(document.DocumentElement, ref replacements);
            return replacements;
        }

        private static void ReplaceInvalidCDataTerminators(XmlNode node, ref int replacements)
        {
            if (node == null)
            {
                return;
            }

            if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (ReplaceInvalidCDataTerminator(attribute))
                    {
                        replacements += 1;
                    }
                }
            }

            if (ReplaceInvalidCDataTerminator(node))
            {
                replacements += 1;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                ReplaceInvalidCDataTerminators(child, ref replacements);
            }
        }

        private static bool ReplaceInvalidCDataTerminator(XmlNode node)
        {
            if (node == null || node.Value == null || node.Value.IndexOf("]]>", StringComparison.Ordinal) < 0)
            {
                return false;
            }

            node.Value = node.Value.Replace("]]>", "]]&gt;");
            return true;
        }

        private static void DumpXmlIfInvalid(WebPanel webPanel, string xml, string stage)
        {
            try
            {
                XmlDocument validationDocument = new XmlDocument();
                validationDocument.LoadXml(xml);
            }
            catch (XmlException exception)
            {
                CommonServices.Output.AddErrorLine("Invalid XML in " + webPanel.Name + " - " + stage + ": " + exception.Message);
                DumpXml(webPanel, xml, stage);
                throw;
            }
        }

        private static void DumpXml(WebPanel webPanel, string xml, string stage)
        {
            IOutputService output = CommonServices.Output;
            output.AddLine("----- XML BEGIN " + webPanel.Name + " - " + stage + " -----");
            output.AddLine(xml ?? "<null>");
            output.AddLine("----- XML END " + webPanel.Name + " - " + stage + " -----");
        }

        private static string ReplaceStoredReferenceIds(WebPanel webPanel, string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return xml;
            }

            VariablesPart variables = webPanel.Parts.Get<VariablesPart>();
            xml = Regex.Replace(xml, @"var:(\d+)", match =>
            {
                int variableId;
                if (!int.TryParse(match.Groups[1].Value, out variableId) || variables == null)
                {
                    return match.Value;
                }

                Variable variable = variables.GetVariable(variableId);
                return variable == null ? match.Value : SecurityElement.Escape("&" + variable.Name);
            }, RegexOptions.IgnoreCase);

            KBModel model = webPanel.Model;
            xml = Regex.Replace(xml, @"att:(\d+)", match =>
            {
                int attributeId;
                if (!int.TryParse(match.Groups[1].Value, out attributeId))
                {
                    return match.Value;
                }

                Artech.Genexus.Common.Objects.Attribute attribute = Artech.Genexus.Common.Objects.Attribute.Get(model, attributeId);
                return attribute == null ? match.Value : SecurityElement.Escape(attribute.Name);
            }, RegexOptions.IgnoreCase);

            return xml;
        }

        private static bool ResolveStoredReferencesBeforeLayoutConversion(WebPanel webPanel, XmlDocument formDocument)
        {
            string xml = formDocument.OuterXml;
            if (xml.IndexOf("att:", StringComparison.OrdinalIgnoreCase) < 0 && xml.IndexOf("var:", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            WebFormEditable.StoredToEditable(webPanel, formDocument);
            return true;
        }

        private static HashSet<string> GetExistingControlNames(WebPanel webPanel, IEnumerable<MultiFormSerializer.Form> forms)
        {
            HashSet<string> controlNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (MultiFormSerializer.Form form in forms)
            {
                foreach (string controlName in form.Handler.GetControlNames(webPanel, form.RootElement))
                {
                    if (!string.IsNullOrEmpty(controlName))
                    {
                        controlNames.Add(controlName);
                    }
                }
            }

            return controlNames;
        }

        private static string GetUniqueControlName(HashSet<string> existingNames, string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                baseName = "Layout";
            }

            string candidate = baseName;
            int suffix = 1;
            while (existingNames.Contains(candidate))
            {
                candidate = baseName + suffix.ToString();
                suffix += 1;
            }

            existingNames.Add(candidate);
            return candidate;
        }

        private static void ConvertDefaultResponsiveProperties()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            WebPanelReference masterRef = (WebPanelReference)kbModel.GetPropertyValue("idMASTER_PAGE");
            ThemeWebReference themeReference = (ThemeWebReference)kbModel.GetPropertyValue(Properties.MODEL.DefaultTheme);
            string modelWebDF = kbModel.GetPropertyValueString(Properties.MODEL.WebFormDefaults_DisplayName);

            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                bool changed = false;
                string webDF = obj.GetPropertyValueString(Properties.TRN.WebFormDefaults);
                if (ObjectsHelper.isGenerated(obj))
                {
                    if (webDF == modelWebDF && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebFormDefaults_Values.ResponsiveWebDesign);
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebFormDefaults_Values.PreviousVersionsCompatible);
                        changed = true;
                    }

                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);
                    if (objmasterRef.GetFullQualifyName(kbModel) == masterRef.GetFullQualifyName(kbModel) && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.MasterPage, WebPanelReference.NoneRef);
                        obj.SetPropertyValue(Properties.TRN.MasterPage, objmasterRef);
                        changed = true;
                    }

                    ThemeWebReference objThemeRef = (ThemeWebReference)obj.GetPropertyValue(Properties.TRN.Theme);
                    if (objThemeRef.GetFullQualifiedName(kbModel) == themeReference.GetFullQualifiedName(kbModel) && obj.IsPropertyDefault(Properties.TRN.Theme))
                    {
                        obj.SetPropertyValue(Properties.TRN.Theme, themeReference);
                        changed = true;
                    }
                }

                if (changed)
                {
                    obj.Save();
                }
                output.AddLine(obj.Name);
            });
        }

        private static void ConvertDefaultWebUXToSmooth()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            string modelWebUX = kbModel.GetPropertyValueString(Properties.MODEL.WebUserExperience);

            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience);
                if (ObjectsHelper.isGenerated(obj) && webUX == modelWebUX && obj.IsPropertyDefault(Properties.TRN.WebUserExperience))
                {
                    obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.Smooth);
                    obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.PreviousVersionsCompatible);
                    obj.Save();
                }
                else
                {
                    output.AddText("NO CAMBIA WEBUX ");
                }

                output.AddLine(obj.Name);
            });
        }

        private static void ConvertGeneratedPreviousCompatibleToSmooth()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                if (ObjectsHelper.isGenerated(obj) && ObjectsHelper.isGeneratedbyPattern(obj))
                {
                    string eventList = ListOfEvents(obj);
                    output.AddLine(obj.Name + "," + obj.GetPropertyValueString(Properties.TRN.WebUserExperience) + "," + eventList);
                    if (obj.GetPropertyValueString(Properties.TRN.WebUserExperience) == "Previous versions compatible")
                    {
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.Smooth);
                        obj.Save();
                    }
                }
            });
        }

        private static void AuditSmoothResponsive()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            QualifiedName qn = new QualifiedName("DECLARACIONES.Dua.HCIMDPol");
            WebPanel webp = WebPanel.Get(kbModel, qn);
            WebPanelReference luciamasterRef = webp == null ? null : (WebPanelReference)webp.GetPropertyValue(Properties.TRN.MasterPage);

            output.AddLine("ObjectName,UX,MasterPageName,Responsive,Theme,AbstracEditor,PatternOrManual,NeedFix");
            int objTotal = 0;
            int objToFix = 0;

            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                if (ObjectsHelper.isGenerated(obj))
                {
                    objTotal += 1;
                    string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience).ToUpper();
                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);
                    string webDF = obj.GetPropertyValueString(Properties.TRN.WebFormDefaults);
                    string themeObj = obj.GetPropertyValueString(Properties.TRN.Theme);
                    WebFormPart webFormPart = obj is WebPanel ? ((WebPanel)obj).WebForm : ((Transaction)obj).WebForm;
                    bool abstracEditor = WebPanelFormRootIsAbstract(webFormPart);
                    string genPatt = ObjectsHelper.isGeneratedbyPattern(obj) ? "Pattern" : "Manual";
                    bool needFix = webUX != "SMOOTH" || objmasterRef.GetName(kbModel) != "GIAMasterPage_v1" || webDF == "Previous versions compatible" || themeObj != "GIATheme_v1" || !abstracEditor;
                    if (needFix)
                    {
                        objToFix += 1;
                    }

                    output.AddLine(obj.Name + "," + webUX + "," + objmasterRef.GetName(kbModel) + "," + webDF + "," + themeObj + "," + abstracEditor.ToString() + "," + genPatt + "," + needFix.ToString());
                    if (luciamasterRef != null && objmasterRef.GetName(kbModel) == "GIAMasterPage_v1" && !abstracEditor && genPatt == "Manual")
                    {
                        output.AddLine(">>> Cambio " + obj.Name);
                        obj.SetPropertyValue(Properties.TRN.MasterPage, luciamasterRef);
                        obj.Save();
                    }
                }
            });

            output.AddLine("   ");
            output.AddLine("#Objects:" + objTotal.ToString() + "  #Objects to Fix: " + objToFix.ToString());
        }

        private static void ResetDefaultMasterPageReferences()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            WebPanelReference masterRef = (WebPanelReference)kbModel.GetPropertyValue("idMASTER_PAGE");
            output.AddWarningLine(masterRef.GetName(kbModel));

            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                if (ObjectsHelper.isGenerated(obj))
                {
                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);
                    if (objmasterRef.GetFullQualifyName(kbModel) == masterRef.GetFullQualifyName(kbModel) && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.MasterPage, WebPanelReference.NoneRef);
                        obj.SetPropertyValue(Properties.TRN.MasterPage, objmasterRef);
                        obj.Save();

                        string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience).ToUpper();
                        if (webUX == "SMOOTH")
                        {
                            output.AddLine("ERROR SMOOTH WITH MASTERPAGE DEFAULT:" + obj.Name + "--" + objmasterRef.GetFullQualifyName(kbModel));
                        }
                    }
                }
            });
        }

        private static void ForEachTransactionAndWebPanel(KBModel kbModel, Action<KBObject> action)
        {
            foreach (KBObject obj in Utility.EditableObjects(Transaction.GetAll(kbModel)))
            {
                action(obj);
            }

            foreach (KBObject obj in Utility.EditableObjects(WebPanel.GetAll(kbModel)))
            {
                action(obj);
            }
        }

        private static string ListOfEvents(KBObject obj)
        {
            string eventsList = "";
            string source = obj.Parts.Get<EventsPart>().Source;
            string sourceWOcomments = Utility.ExtractComments(source);
            bool containLOAD = false, containREFRESH = false, containSTART = false;

            using (StringReader reader = new StringReader(sourceWOcomments))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.ToUpper().StartsWith("EVENT "))
                    {
                        eventsList += "," + line.ToUpper().Replace("EVENT ", "");
                        if (line.ToUpper().Contains("LOAD"))
                            containLOAD = true;
                        if (line.ToUpper().Contains("REFRESH"))
                            containREFRESH = true;
                        if (line.ToUpper().Contains("START"))
                            containSTART = true;
                    }
                }
            }

            return "," + containSTART.ToString() + "," + containREFRESH.ToString() + "," + containLOAD.ToString() + eventsList;
        }

        private static bool WebPanelFormRootIsAbstract(WebFormPart part)
        {
            if (part.EditableContent != null)
            {
                XmlDocument xmlDocumentWebForm = new XmlDocument();
                xmlDocumentWebForm.LoadXml(part.EditableContent);
                return xmlDocumentWebForm.SelectSingleNode("//Form[@id='" + xmlDocumentWebForm.DocumentElement?.Attributes["rootId"]?.Value + "']")?.Attributes["type"]?.Value == "layout";
            }

            return part.Model.GetPropertyValue<Properties.MODEL.DefaultWebFormEditor_Enum>(Properties.MODEL.DefaultWebFormEditor) == Properties.MODEL.DefaultWebFormEditor_Enum.AbstractLayout;
        }
    }
}
