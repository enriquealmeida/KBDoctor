using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;

namespace Concepto.Packages.KBDoctor
{
    static class EnvironmentHelper
    {
        private const string Title = "KBDoctor - Copy environment";

        public static void ShowCopyEnvironment()
        {
            KBModel designModel = UIServices.KB.CurrentKB.DesignModel;
            IList<string> environmentNames = GetEnvironmentModels(designModel)
                .Select(model => model.Name)
                .OrderBy(name => name)
                .ToList();

            KBDoctorWebForms.ShowCopyEnvironment(Title, environmentNames);
        }

        public static void CopyEnvironment(object[] parameters)
        {
            KBDoctorOutput.StartSection(Title);
            IOutputService output = KBDoctorHelper.SelectOutput();

            try
            {
                output.AddLine("CopyEnvironment handler started. Parameter items: " + (parameters == null ? "null" : parameters.Length.ToString()));
                string sourceEnvironmentName = KBDoctorWebForms.GetParameter(parameters, "sourceEnvironment").Trim();
                string newEnvironmentName = KBDoctorWebForms.GetParameter(parameters, "newEnvironment").Trim();
                output.AddLine("Copy requested: " + sourceEnvironmentName + " -> " + newEnvironmentName);

                if (string.IsNullOrEmpty(sourceEnvironmentName))
                {
                    output.AddErrorLine("Source environment is required.");
                    KBDoctorOutput.EndSection(Title, false);
                    return;
                }

                if (string.IsNullOrEmpty(newEnvironmentName))
                {
                    output.AddErrorLine("New environment name is required.");
                    KBDoctorOutput.EndSection(Title, false);
                    return;
                }

                KnowledgeBase kb = UIServices.KB.CurrentKB;
                KBVersion activeVersion = KBVersion.GetActive(kb);
                if (activeVersion == null)
                    throw new InvalidOperationException("The KB has no active version.");

                List<KBModel> environmentModels = GetEnvironmentModels(kb.DesignModel).ToList();

                KBModel sourceEnvironment = environmentModels.FirstOrDefault(model =>
                    string.Equals(model.Name, sourceEnvironmentName, StringComparison.OrdinalIgnoreCase));

                if (sourceEnvironment == null)
                {
                    output.AddErrorLine("Source environment not found: " + sourceEnvironmentName);
                    KBDoctorOutput.EndSection(Title, false);
                    return;
                }

                if (environmentModels.Any(model => string.Equals(model.Name, newEnvironmentName, StringComparison.OrdinalIgnoreCase)))
                {
                    output.AddErrorLine("An environment with the destination name already exists: " + newEnvironmentName);
                    KBDoctorOutput.EndSection(Title, false);
                    return;
                }

                string targetPath = SanitizeTargetPath(newEnvironmentName);
                if (string.IsNullOrEmpty(targetPath))
                {
                    output.AddErrorLine("The new environment name does not contain valid TargetPath characters.");
                    KBDoctorOutput.EndSection(Title, false);
                    return;
                }

                KBModel newEnvironment = kb.CreateModel(newEnvironmentName);
                if (newEnvironment == null)
                    throw new InvalidOperationException("GeneXus did not create the environment model.");

                newEnvironment.CopyConfiguration(sourceEnvironment, true);
                newEnvironment.Name = newEnvironmentName;
                newEnvironment.Guid = Guid.NewGuid();
                newEnvironment.Type = sourceEnvironment.Type;
                newEnvironment.ParentModel = activeVersion.Model;
                newEnvironment.Description = sourceEnvironment.Description;
                newEnvironment.TargetPath = targetPath;
                if (!string.Equals(newEnvironment.Name, newEnvironmentName, StringComparison.Ordinal) ||
                    !string.Equals(newEnvironment.TargetPath, targetPath, StringComparison.Ordinal))
                    throw new InvalidOperationException("The new environment name or TargetPath changed before the first save.");
                output.AddLine("Saving new environment: " + newEnvironment.Name + ", TargetPath: " + newEnvironment.TargetPath);
                newEnvironment.Save();

                KBModel savedEnvironment = GetEnvironmentModels(kb.DesignModel).FirstOrDefault(model =>
                    model.Guid == newEnvironment.Guid &&
                    string.Equals(model.Name, newEnvironmentName, StringComparison.OrdinalIgnoreCase));
                if (savedEnvironment == null)
                    throw new InvalidOperationException("The model was saved, but it is not listed among the KB environments. Model GUID: " + newEnvironment.Guid);

                output.AddLine("Environment copied successfully.");
                output.AddLine("Source: " + sourceEnvironment.Name);
                output.AddLine("New environment: " + savedEnvironment.Name);
                output.AddLine("TargetPath: " + savedEnvironment.TargetPath);
                KBDoctorOutput.EndSection(Title, true);
            }
            catch (Exception exception)
            {
                output.AddErrorLine("Could not copy the environment.");
                output.AddErrorLine(exception.ToString());
                KBDoctorOutput.EndSection(Title, false);
            }
        }

        private static IEnumerable<KBModel> GetEnvironmentModels(KBModel designModel)
        {
            return KBEnvironment.GetPrototypeModels(designModel)
                .Where(model => model != null)
                .Where(model => !string.IsNullOrEmpty(model.Name));
        }

        private static string SanitizeTargetPath(string environmentName)
        {
            HashSet<char> invalidCharacters = new HashSet<char>(
                Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()));
            invalidCharacters.Add('.');

            return new string((environmentName ?? string.Empty)
                .Where(character => !invalidCharacters.Contains(character))
                .ToArray())
                .Trim();
        }
    }
}
