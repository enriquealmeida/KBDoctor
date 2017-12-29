using Artech.Architecture.Common.Events;
using Artech.Architecture.Common.Services;
using Artech.Architecture.BL.Framework.Packages;
using Artech.Common.Diagnostics;
using Artech.Architecture.Common.Packages;
using Microsoft.Practices.CompositeUI.EventBroker;
using System;
using System.Configuration;
using Concepto.Packages.KBDoctor;


namespace Concepto.Packages.KBDoctorValidator
{

    public class KBDoctorValidatorClass : AbstractPackage, IGxPackageBL

    {
        public override string Name
        {
            get { return "KBDoctorValidator"; }
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            CommonServices.Output.Add(new OutputError("KBDoctorValidator was loaded!!", MessageLevel.Information));
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationManager.RefreshSection("userSettings");
            config.Save(ConfigurationSaveMode.Full);
        }


        [EventSubscription(ArchitectureEvents.BeforeSaveKBObject)]
        public void OnBeforeSaveKBObject(object sender, KBObjectCancelEventArgs args)
        {
            if (args.KBObject != null)

            { var days = (DateTime.Now - args.KBObject.Timestamp).TotalDays;

                if (Math.Abs(days) < 10)
                    {
                   

                        if (KBDoctor2.Default.ValidateINOUTParm && Function2.ValidateINOUTinParm(args.KBObject))
                        {
                            args.Cancel = true;
                            args.CancelMessage += args.CancelMessage.Contains("KBDoctor") ? Environment.NewLine : "KBDoctor:";
                            args.CancelMessage += "Missing IN:,OUT:, INOUT: in parm rule.";
                        }

                    string input = Function2.ObjectSourceUpper(args.KBObject);

                    if (Function2.MaxNestLevel(input) > KBDoctor2.Default.MaxNestLevel)
                    {
                        args.Cancel = true;
                        args.CancelMessage += args.CancelMessage.Contains("KBDoctor") ? Environment.NewLine : "KBDoctor:";
                        args.CancelMessage += "CODE too complex. Simplify your code. Nest level > " + KBDoctor2.Default.MaxNestLevel.ToString();
                    }


                    if (Function2.ComplexityLevel(input) > KBDoctor2.Default.ComplexityLevel)
                    {
                        args.Cancel = true;
                        args.CancelMessage += args.CancelMessage.Contains("KBDoctor") ? Environment.NewLine : "KBDoctor:";
                        args.CancelMessage += "CODE Too complex. Complexity index > " + KBDoctor2.Default.ComplexityLevel.ToString();
                    }

                    if (Function2.MaxCodeBlock(input) > KBDoctor2.Default.MaxCodeBlcok)
                    {
                        args.Cancel = true;
                        args.CancelMessage += args.CancelMessage.Contains("KBDoctor") ? Environment.NewLine : "KBDoctor:";
                        args.CancelMessage += "CODE Too complex. Block of code must be less than " + KBDoctor2.Default.MaxCodeBlcok.ToString() + " lines";
                    }
                }
            }
        }
    }
}






