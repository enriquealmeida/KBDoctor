using System.Collections.Generic;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Selection;
using Artech.Genexus.Common.Objects;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class KBDoctorSelection
    {
        public static List<KBObject> SelectProcedureWebPanelTransaction()
        {
            SelectObjectOptions options = new SelectObjectOptions();
            options.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            options.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            options.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Transaction>());
            options.MultipleSelection = true;

            return SelectObjects(options);
        }

        private static List<KBObject> SelectObjects(SelectObjectOptions options)
        {
            List<KBObject> selectedObjects = new List<KBObject>();

            foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(options))
            {
                if (Utility.IsUserEditableObject(obj))
                {
                    selectedObjects.Add(obj);
                }
            }

            return selectedObjects;
        }
    }
}
