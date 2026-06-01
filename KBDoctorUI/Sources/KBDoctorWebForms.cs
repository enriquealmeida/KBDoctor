using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Artech.Architecture.UI.Framework.Services;

namespace Concepto.Packages.KBDoctor
{
    static class KBDoctorWebForms
    {
        private const string PackageCommandPrefix = "gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;";

        public sealed class DomainOption
        {
            public DomainOption(string name, string type, int length, int decimals)
            {
                Name = name;
                Type = type;
                Length = length;
                Decimals = decimals;
            }

            public string Name { get; private set; }
            public string Type { get; private set; }
            public int Length { get; private set; }
            public int Decimals { get; private set; }
        }

        public static void ShowTextInput(string title, string message, string command, IDictionary<string, string> fixedParameters, string valueParameterName, string currentValue)
        {
            string outputFile = Functions.CreateOutputFile(UIServices.KB, title);
            KBDoctorOutput.StartSection(title);

            StringBuilder html = new StringBuilder();
            html.AppendLine("<!doctype html>");
            html.AppendLine("<html><head><meta charset=\"utf-8\"><title>" + Html(title) + "</title>");
            html.AppendLine("<style>");
            html.AppendLine("body{font-family:Tahoma,Arial,sans-serif;margin:24px;color:#1f2933}label{display:block;margin-bottom:8px;font-weight:bold}input{width:640px;max-width:95%;padding:7px;border:1px solid #888}button{margin-top:12px;padding:7px 14px;background:#064f8f;color:white;border:0;cursor:pointer}.panel{max-width:760px}.hint{color:#555;font-size:12px}");
            html.AppendLine("</style></head><body>");
            html.AppendLine("<div class=\"panel\">");
            html.AppendLine("<h2>" + Html(title) + "</h2>");
            html.AppendLine("<label for=\"kbdoctorValue\">" + Html(message) + "</label>");
            html.AppendLine("<input id=\"kbdoctorValue\" type=\"text\" value=\"" + HtmlAttribute(currentValue) + "\" autofocus>");
            html.AppendLine("<br><button type=\"button\" onclick=\"applyValue()\">Apply</button>");
            html.AppendLine("<p class=\"hint\">This form calls a KBDoctor command in GeneXus.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<script>");
            html.AppendLine("function applyValue(){");
            html.AppendLine("var href='" + JavaScript(PackageCommandPrefix + command) + "';");
            foreach (KeyValuePair<string, string> parameter in fixedParameters)
            {
                html.AppendLine("href+='&" + JavaScript(Uri.EscapeDataString(parameter.Key)) + "=" + JavaScript(Uri.EscapeDataString(parameter.Value ?? string.Empty)) + "';");
            }

            html.AppendLine("href+='&" + JavaScript(Uri.EscapeDataString(valueParameterName)) + "='+encodeURIComponent(document.getElementById('kbdoctorValue').value);");
            html.AppendLine("window.location.href=href;");
            html.AppendLine("}");
            html.AppendLine("</script></body></html>");

            File.WriteAllText(outputFile, html.ToString(), Encoding.UTF8);
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            KBDoctorOutput.EndSection(title, true);
        }

        public static void ShowSearchReplace(string title)
        {
            string outputFile = Functions.CreateOutputFile(UIServices.KB, title);
            KBDoctorOutput.StartSection(title);

            StringBuilder html = StartHtml(title);
            html.AppendLine("<label for=\"findValue\">Find</label>");
            html.AppendLine("<input id=\"findValue\" type=\"text\" autofocus>");
            html.AppendLine("<label for=\"replaceValue\" style=\"margin-top:12px\">Replace with</label>");
            html.AppendLine("<input id=\"replaceValue\" type=\"text\">");
            html.AppendLine("<br><button type=\"button\" onclick=\"applyValue()\">Apply</button>");
            html.AppendLine("<script>");
            html.AppendLine("function applyValue(){");
            html.AppendLine("var href='" + JavaScript(PackageCommandPrefix + "ApplySearchAndReplace") + "';");
            html.AppendLine("href+='&find='+encodeURIComponent(document.getElementById('findValue').value);");
            html.AppendLine("href+='&replace='+encodeURIComponent(document.getElementById('replaceValue').value);");
            html.AppendLine("window.location.href=href;");
            html.AppendLine("}");
            html.AppendLine("</script>");
            EndHtml(html);

            File.WriteAllText(outputFile, html.ToString(), Encoding.UTF8);
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            KBDoctorOutput.EndSection(title, true);
        }

        public static void ShowDomainReplacement(string title, IEnumerable<DomainOption> domainOptions)
        {
            string outputFile = Functions.CreateOutputFile(UIServices.KB, title);
            KBDoctorOutput.StartSection(title);

            List<DomainOption> domains = domainOptions.OrderBy(domain => domain.Name).ToList();
            StringBuilder html = StartHtml(title);
            html.AppendLine("<label for=\"sourceDomain\">Original domain</label>");
            AppendSelect(html, "sourceDomain", domains.Select(domain => domain.Name));
            html.AppendLine("<div id=\"sourceDomainInfo\" class=\"domain-info\"></div>");
            html.AppendLine("<label for=\"targetDomain\" style=\"margin-top:12px\">Replace with</label>");
            html.AppendLine("<select id=\"targetDomain\"></select>");
            html.AppendLine("<div id=\"targetDomainInfo\" class=\"domain-info\"></div>");
            html.AppendLine("<br><button type=\"button\" onclick=\"applyValue()\">Apply</button>");
            html.AppendLine("<script>");
            html.AppendLine("var domains=[");
            foreach (DomainOption domain in domains)
            {
                html.Append("{name:'").Append(JavaScript(domain.Name)).Append("',type:'").Append(JavaScript(domain.Type)).Append("',length:").Append(domain.Length).Append(",decimals:").Append(domain.Decimals).AppendLine("},");
            }
            html.AppendLine("];");
            html.AppendLine("function normalizeType(value){return (value||'').toUpperCase().replace(/[\\s_]/g,'');}");
            html.AppendLine("function isChar(type){return type==='CHAR'||type==='CHARACTER';}");
            html.AppendLine("function isVarChar(type){return type==='VARCHAR';}");
            html.AppendLine("function isLongVarChar(type){return type==='LONGVARCHAR';}");
            html.AppendLine("function isCompatible(source,target){");
            html.AppendLine("if(!source||!target||source.name===target.name){return false;}");
            html.AppendLine("var sourceType=normalizeType(source.type);");
            html.AppendLine("var targetType=normalizeType(target.type);");
            html.AppendLine("if(sourceType==='DATE'){return targetType==='DATE'||targetType==='DATETIME';}");
            html.AppendLine("if(isChar(sourceType)){return (isChar(targetType)||isVarChar(targetType))&&Math.abs((target.length||0)-(source.length||0))<=15;}");
            html.AppendLine("if(sourceType==='BOOLEAN'){return targetType==='BOOLEAN'||(isChar(targetType)&&(target.length||0)===1);}");
            html.AppendLine("if(isLongVarChar(sourceType)){return isLongVarChar(targetType)||(isVarChar(targetType)&&(target.length||0)>=1024);}");
            html.AppendLine("return sourceType===targetType;");
            html.AppendLine("}");
            html.AppendLine("function findDomain(name){for(var i=0;i<domains.length;i++){if(domains[i].name===name){return domains[i];}}return null;}");
            html.AppendLine("function describe(domain){if(!domain){return 'No compatible domains available.';}return 'Type: '+domain.type+' | Length: '+domain.length+' | Precision: '+domain.decimals;}");
            html.AppendLine("function fillTargets(){");
            html.AppendLine("var source=findDomain(document.getElementById('sourceDomain').value);");
            html.AppendLine("var targetSelect=document.getElementById('targetDomain');");
            html.AppendLine("targetSelect.innerHTML='';");
            html.AppendLine("document.getElementById('sourceDomainInfo').innerText=describe(source);");
            html.AppendLine("for(var i=0;i<domains.length;i++){if(isCompatible(source,domains[i])){var option=document.createElement('option');option.value=domains[i].name;option.text=domains[i].name;targetSelect.add(option);}}");
            html.AppendLine("updateTargetInfo();");
            html.AppendLine("}");
            html.AppendLine("function updateTargetInfo(){document.getElementById('targetDomainInfo').innerText=describe(findDomain(document.getElementById('targetDomain').value));}");
            html.AppendLine("function applyValue(){");
            html.AppendLine("if(!document.getElementById('targetDomain').value){return;}");
            html.AppendLine("var href='" + JavaScript(PackageCommandPrefix + "ApplyReplaceDomain") + "';");
            html.AppendLine("href+='&originalDomain='+encodeURIComponent(document.getElementById('sourceDomain').value);");
            html.AppendLine("href+='&destDomain='+encodeURIComponent(document.getElementById('targetDomain').value);");
            html.AppendLine("window.location.href=href;");
            html.AppendLine("}");
            html.AppendLine("document.getElementById('sourceDomain').onchange=fillTargets;");
            html.AppendLine("document.getElementById('targetDomain').onchange=updateTargetInfo;");
            html.AppendLine("fillTargets();");
            html.AppendLine("</script>");
            EndHtml(html);

            File.WriteAllText(outputFile, html.ToString(), Encoding.UTF8);
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            KBDoctorOutput.EndSection(title, true);
        }

        public static void ShowTableAttributeSelection(string title, string command, IDictionary<string, IList<string>> tableAttributes)
        {
            string outputFile = Functions.CreateOutputFile(UIServices.KB, title);
            KBDoctorOutput.StartSection(title);

            StringBuilder html = StartHtml(title);
            html.AppendLine("<label for=\"tableName\">Table</label>");
            AppendSelect(html, "tableName", tableAttributes.Keys.OrderBy(name => name));
            html.AppendLine("<label for=\"attributeName\" style=\"margin-top:12px\">Attribute</label>");
            html.AppendLine("<select id=\"attributeName\"></select>");
            html.AppendLine("<br><button type=\"button\" onclick=\"applyValue()\">Apply</button>");
            html.AppendLine("<script>");
            html.AppendLine("var tableAttributes={};");
            foreach (KeyValuePair<string, IList<string>> pair in tableAttributes)
            {
                html.Append("tableAttributes['").Append(JavaScript(pair.Key)).Append("']=[");
                html.Append(string.Join(",", pair.Value.OrderBy(name => name).Select(name => "'" + JavaScript(name) + "'").ToArray()));
                html.AppendLine("];");
            }
            html.AppendLine("function fillAttributes(){var t=document.getElementById('tableName').value;var a=document.getElementById('attributeName');a.innerHTML='';(tableAttributes[t]||[]).forEach(function(name){var o=document.createElement('option');o.value=name;o.text=name;a.add(o);});}");
            html.AppendLine("function applyValue(){");
            html.AppendLine("var href='" + JavaScript(PackageCommandPrefix + command) + "';");
            html.AppendLine("href+='&tblName='+encodeURIComponent(document.getElementById('tableName').value);");
            html.AppendLine("href+='&attName='+encodeURIComponent(document.getElementById('attributeName').value);");
            html.AppendLine("window.location.href=href;");
            html.AppendLine("}");
            html.AppendLine("document.getElementById('tableName').onchange=fillAttributes;fillAttributes();");
            html.AppendLine("</script>");
            EndHtml(html);

            File.WriteAllText(outputFile, html.ToString(), Encoding.UTF8);
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            KBDoctorOutput.EndSection(title, true);
        }

        public static void ShowActionMenu(string title, IEnumerable<KeyValuePair<string, string>> actions)
        {
            string outputFile = Functions.CreateOutputFile(UIServices.KB, title);
            KBDoctorOutput.StartSection(title);

            StringBuilder html = StartHtml(title);
            html.AppendLine("<div class=\"actions\">");
            foreach (KeyValuePair<string, string> action in actions)
            {
                html.AppendLine("<p>" + Functions.CommandLink("RunResponsiveSmoothAction", Html(action.Value), "action", action.Key) + "</p>");
            }
            html.AppendLine("</div>");
            EndHtml(html);

            File.WriteAllText(outputFile, html.ToString(), Encoding.UTF8);
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            KBDoctorOutput.EndSection(title, true);
        }

        public static void ShowAbout(Assembly assembly)
        {
            string title = "KBDoctor - About";
            string outputFile = Functions.CreateOutputFile(UIServices.KB, title);
            KBDoctorOutput.StartSection(title);

            AssemblyName assemblyName = assembly.GetName();
            object[] descriptions = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            string description = descriptions.Length > 0 ? ((AssemblyDescriptionAttribute)descriptions[0]).Description : string.Empty;

            StringBuilder html = new StringBuilder();
            html.AppendLine("<!doctype html>");
            html.AppendLine("<html><head><meta charset=\"utf-8\"><title>" + Html(title) + "</title>");
            html.AppendLine("<style>body{font-family:Tahoma,Arial,sans-serif;margin:24px;color:#1f2933}dt{font-weight:bold;margin-top:12px}dd{margin-left:0}</style>");
            html.AppendLine("</head><body>");
            html.AppendLine("<h2>KBDoctor</h2>");
            html.AppendLine("<dl>");
            html.AppendLine("<dt>Assembly</dt><dd>" + Html(assemblyName.Name) + "</dd>");
            html.AppendLine("<dt>Version</dt><dd>" + Html(assemblyName.Version.ToString()) + "</dd>");
            if (!string.IsNullOrEmpty(description))
            {
                html.AppendLine("<dt>Description</dt><dd>" + Html(description) + "</dd>");
            }
            html.AppendLine("</dl>");
            html.AppendLine("</body></html>");

            File.WriteAllText(outputFile, html.ToString(), Encoding.UTF8);
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            KBDoctorOutput.EndSection(title, true);
        }

        public static Dictionary<string, string> GetCommandParameters(object[] parameters)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (object item in parameters)
            {
                Dictionary<string, string> dictionary = item as Dictionary<string, string>;
                if (dictionary == null)
                {
                    continue;
                }

                foreach (KeyValuePair<string, string> pair in dictionary)
                {
                    result[pair.Key] = pair.Value;
                }
            }

            return result;
        }

        public static string GetParameter(object[] parameters, string name)
        {
            Dictionary<string, string> values = GetCommandParameters(parameters);
            string value;
            return values.TryGetValue(name, out value) ? value : string.Empty;
        }

        private static StringBuilder StartHtml(string title)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!doctype html>");
            html.AppendLine("<html><head><meta charset=\"utf-8\"><title>" + Html(title) + "</title>");
            html.AppendLine("<style>");
            html.AppendLine("body{font-family:Tahoma,Arial,sans-serif;margin:24px;color:#1f2933}label{display:block;margin-bottom:8px;font-weight:bold}input,select{width:640px;max-width:95%;padding:7px;border:1px solid #888}button{margin-top:12px;padding:7px 14px;background:#064f8f;color:white;border:0;cursor:pointer}.panel{max-width:760px}.hint,.domain-info{color:#555;font-size:12px}.domain-info{margin-top:5px}.actions a{display:inline-block;padding:8px 12px;background:#064f8f;color:white;text-decoration:none;margin-bottom:4px}");
            html.AppendLine("</style></head><body>");
            html.AppendLine("<div class=\"panel\">");
            html.AppendLine("<h2>" + Html(title) + "</h2>");
            return html;
        }

        private static void EndHtml(StringBuilder html)
        {
            html.AppendLine("</div></body></html>");
        }

        private static void AppendSelect(StringBuilder html, string id, IEnumerable<string> values)
        {
            html.Append("<select id=\"").Append(HtmlAttribute(id)).AppendLine("\">");
            foreach (string value in values)
            {
                html.Append("<option value=\"").Append(HtmlAttribute(value)).Append("\">").Append(Html(value)).AppendLine("</option>");
            }
            html.AppendLine("</select>");
        }

        private static string Html(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string HtmlAttribute(string value)
        {
            return Html(value).Replace("\"", "&quot;");
        }

        private static string JavaScript(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }
}
