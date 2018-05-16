using Artech.Architecture.Common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Helpers;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Packages;
using Artech.Architecture.Common.Services;
using Artech.Udm.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using Artech.Common.Helpers;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    public static class CodigoGX { 
        public static KBObject ObjectHelperGet(KBModel model, string name)
        {
            ResolveResult resolveResult;
            if (model.Objects.ResolveName(model.RootModule, null, name, out resolveResult))
            {
                return resolveResult.Match;
            }
            HandleError(resolveResult);
            return null;
        }

        public static KBObject ObjectHelperGet(KBModel model, Guid type, string name)
        {
            ResolveResult resolveResult;
            if (model.Objects.ResolveName(model.RootModule, new Guid?(type), name, out resolveResult))
            {
                return resolveResult.Match;
            }
            HandleError(resolveResult);
            return null;
        }

        public static KBObject ObjectHelperGet(KBModel model, string name, bool checkPrefix)
        {
            if (checkPrefix)
            {
                int num = name.IndexOf(':');
                if (num > 0)
                {
                    string arg_22_0 = name.Substring(0, num);
                    string name2 = name.Substring(num + 1);
                    KBObjectDescriptor kBObjectDescriptor = KBObjectDescriptor.Get(arg_22_0);
                    if (kBObjectDescriptor != null)
                    {
                        return ObjectHelperGet(model, kBObjectDescriptor.Id, name2);
                    }
                }
            }
            return ObjectHelperGet(model, name);
        }

        public static IEnumerable<KBObject> GetGXLObjects(KBModel model, string gxlFileName)
        {
            GxlDocumentHelper gxlDocumentHelper = new GxlDocumentHelper(gxlFileName);
            foreach (XmlNode current in gxlDocumentHelper.GetObjects())
            {
                KBObject kBObject = null;
                Guid objectGuid = gxlDocumentHelper.GetObjectGuid(current);
                Guid guid;
                string name;
                if (objectGuid != Guid.Empty)
                {
                    kBObject = model.Objects.Get(objectGuid);
                }
                else if (gxlDocumentHelper.GetObjectTypeName(current, out guid, out name))
                {
                    if (guid != Guid.Empty)
                    {
                        QualifiedName qname = new QualifiedName(guid,name);
                        model.Objects.Get(guid,qname);
                        kBObject = ObjectHelperGet(model, guid, name);
                    }
                    else
                    {
                        kBObject = ObjectHelperGet(model, name);
                    }
                }
                if (kBObject != null)
                {
                    yield return kBObject;
                }
            }
            IEnumerator<XmlNode> enumerator = null;
            yield break;
            yield break;
        }

        public static void HandleError(ResolveResult result)
        {
            CommonServices.Output.AddWarningLine(result.ErrorMessage);
        }

        public static IEnumerable<KBObject> GetObjects(KBModel model, string objects)
        {
            Dictionary<EntityKey, bool> dictionary = new Dictionary<EntityKey, bool>();
            string[] array = objects.Split(new char[]
            {
                    ';'
            });
            for (int i = 0; i < array.Length; i++)
            {
                string text = array[i];
                if (text[0] == '@')
                {
                    foreach (KBObject current in GetGXLObjects(model, text.Substring(1)))
                    {
                        if (!dictionary.ContainsKey(current.Key))
                        {
                            dictionary[current.Key] = true;
                            yield return current;
                        }
                    }
                    IEnumerator<KBObject> enumerator = null;
                }
                else
                {
                    string text2 = null;
                    string text3 = text;
                    int num = text.IndexOf(':');
                    if (num > 0)
                    {
                        text2 = text.Substring(0, num);
                        text3 = text.Substring(num + 1);
                    }
                    KBObject kBObject = null;
                    string[] array2 = text3.Split(new char[]
                    {
                            ','
                    });
                    for (int j = 0; j < array2.Length; j++)
                    {
                        string name = array2[j];
                        if (text2 != null)
                        {
                            KBObjectDescriptor kBObjectDescriptor = KBObjectDescriptor.Get(text2);
                            if (kBObjectDescriptor != null)
                            {
                                kBObject = ObjectHelperGet(model, kBObjectDescriptor.Id, name);
                            }
                        }
                        else
                        {
                            kBObject = ObjectHelperGet(model, name);
                        }
                        if (kBObject != null && !dictionary.ContainsKey(kBObject.Key))
                        {
                            dictionary[kBObject.Key] = true;
                            yield return kBObject;
                        }
                    }
                    array2 = null;
                    text2 = null;
                    kBObject = null;
                }
                text = null;
            }
            array = null;
            yield break;
            yield break;
        }
    }
}

