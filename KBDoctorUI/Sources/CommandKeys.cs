using Artech.Common.Framework.Commands;

namespace Concepto.Packages.KBDoctor
{
    static class CommandKeys
    {
        private static CommandKey Key(string id)
        {
            return new CommandKey(Package.guid, id);
        }

        // Attributes and domains
        public static CommandKey ListAttributes { get; } = Key("ListAttributes");
        public static CommandKey ListAttribute { get; } = Key("ListAttribute");
        public static CommandKey AttWithNoDomain { get; } = Key("AttWithNoDomain");
        public static CommandKey AttWithoutDescription { get; } = Key("AttWithoutDescription");
        public static CommandKey AttInOneTrnOnly { get; } = Key("AttInOneTrnOnly");
        public static CommandKey AttFormula { get; } = Key("AttFormula");
        public static CommandKey AttUpdated { get; } = Key("AttUpdated");
        public static CommandKey AttCharToVarchar { get; } = Key("AttCharToVarchar");
        public static CommandKey AttVarcharToChar { get; } = Key("AttVarcharToChar");
        public static CommandKey AttKeyVarchar { get; } = Key("AttKeyVarchar");
        public static CommandKey AttDescWithoutUniqueIndex { get; } = Key("AttDescWithoutUniqueIndex");
        public static CommandKey AttWithoutBaseTable { get; } = Key("AttWithoutBaseTable");
        public static CommandKey ReplaceDomain { get; } = Key("ReplaceDomain");
        public static CommandKey ApplyReplaceDomain { get; } = Key("ApplyReplaceDomain");
        public static CommandKey ListDomain { get; } = Key("ListDomain");
        public static CommandKey ApplyAttUpdated { get; } = Key("ApplyAttUpdated");

        public static CommandKey AssignDomainToAttribute { get; } = Key("AssignDomainToAttribute");
        public static CommandKey AssignDescriptionToAttribute { get; } = Key("AssignDescriptionToAttribute");
        public static CommandKey ApplyAttributeText { get; } = Key("ApplyAttributeText");
        public static CommandKey AssignTitleToAttribute { get; } = Key("AssignTitleToAttribute");
        public static CommandKey AssignColumnTitleToAttribute { get; } = Key("AssignColumnTitleToAttribute");
        public static CommandKey AddDescriptorIndex { get; } = Key("AddDescriptorIndex");

        // Tables and indexes
        public static CommandKey ListTables { get; } = Key("ListTables");
        public static CommandKey TblWihNoDescription { get; } = Key("TablesWithNoDescription");
        public static CommandKey GrpWihNoDescription { get; } = Key("GroupWithNoDescription");
        public static CommandKey TblWidth { get; } = Key("TablesWidth");
        public static CommandKey TblTableTransaction { get; } = Key("TableTransaction");
        public static CommandKey TblGenerateSimpleTransactionFromNotGeneratedTransactions { get; } = Key("GenerateSimpleTransactionFromNotGeneratedTransactions");
        public static CommandKey TblScriptToCompareNULLABLE_GXvsDB { get; } = Key("ScriptToCompareNULLABLE_GXvsDB");
        public static CommandKey TblScriptToCompareNULLABLE_GXvsDB2 { get; } = Key("ScriptToCompareNULLABLE_GXvsDB2");
        public static CommandKey TblListTableWithAttributeNullableCompatible { get; } = Key("ListTableWithAttributeNullableCompatible");
        public static CommandKey TblTableUpdate { get; } = Key("TableUpdate");
        public static CommandKey TblTableInsertNew { get; } = Key("TableInsertNew");
        public static CommandKey GenerateTrnFromTables { get; } = Key("GenerateTrnFromTables");
        public static CommandKey GenerateTrnFromTables2 { get; } = Key("GenerateTrnFromTables2");
        public static CommandKey ListTablesInModules { get; } = Key("ListTablesInModules");
        public static CommandKey ListObjectsWithTableInOtherModule { get; } = Key("ListObjectsWithTableInOtherModule");

        public static CommandKey AssignDescriptionToTable { get; } = Key("AssignDescriptionToTable");
        public static CommandKey ApplyTableText { get; } = Key("ApplyTableText");
        public static CommandKey IndexWithNotRefAtt { get; } = Key("IndexWithNotRefAtt");
        public static CommandKey RemoveIndexAttribute { get; } = Key("RemoveIndexAttribute");

        // Objects
        public static CommandKey ListObj { get; } = Key("ListObj");
        public static CommandKey ListUnreferencedObjectsInUserModules { get; } = Key("ListUnreferencedObjectsInUserModules");
        public static CommandKey ObjNotReacheable { get; } = Key("ObjectsNotReacheable");
        public static CommandKey ObjectsNotCalled { get; } = Key("ObjectsNotCalled");
        public static CommandKey ObjectsWithoutInOut { get; } = Key("ObjectsWithoutInOut");
        public static CommandKey ObjectsMainCalled { get; } = Key("ObjectsMainCalled");
        public static CommandKey ObjectsReferenced { get; } = Key("ObjectsReferenced");
        public static CommandKey ObjectsWithVarNotBasedOnAtt { get; } = Key("ObjectsWithVarNotBasedOnAtt");
        public static CommandKey ObjectsWithVarsNotUsed { get; } = Key("ObjectsWithVarsNotUsed");
        public static CommandKey ObjectsWithCommitOnExit { get; } = Key("ObjectsWithCommitOnExit");
        public static CommandKey ListCommitOnExit { get; } = Key("ListCommitOnExit");
        public static CommandKey ObjectsUpdateAttribute { get; } = Key("ObjectsUpdateAttribute");
        public static CommandKey SelectObjectsUpdateAttribute { get; } = Key("SelectObjectsUpdateAttribute");
        public static CommandKey ApplyObjectsUpdateAttribute { get; } = Key("ApplyObjectsUpdateAttribute");
        public static CommandKey ObjectsLegacyCode { get; } = Key("ObjectsLegacyCode");
        public static CommandKey ObjectsRefactoringCandidates { get; } = Key("ObjectsRefactoringCandidates");
        public static CommandKey ObjectsWithConstants { get; } = Key("ObjectsWithConstants");
        public static CommandKey ObjectsWINWEB { get; } = Key("ObjectsWINWEB");
        public static CommandKey ListProcedureCallWebpanelTransaction { get; } = Key("ListProcedureCallWebpanelTransaction");
        public static CommandKey BuildObjectAndReferences { get; } = Key("BuildObjectAndReferences");
        public static CommandKey BuildObjectWithProperty { get; } = Key("BuildObjectWithProperty");
        public static CommandKey ObjectMigration { get; } = Key("ObjectMigration");
        public static CommandKey KBInterfaces { get; } = Key("KBInterfaces");
        public static CommandKey ListWebObjectsProperties { get; } = Key("ListWebObjectsProperties");
        public static CommandKey ListProperties { get; } = Key("ListProperties");
        public static CommandKey ListDynamicCombo { get; } = Key("ListDynamicCombo");
        public static CommandKey MainTableUsed { get; } = Key("MainTableUsed");
        public static CommandKey RemovableTransactions { get; } = Key("RemovableTransactions");
        public static CommandKey AttributeAsOutput { get; } = Key("AttributeAsOutput");

        // Object actions
        public static CommandKey RemoveObject { get; } = Key("RemoveObject");
        public static CommandKey RemoveUnreferencedObjectsInUserModules { get; } = Key("RemoveUnreferencedObjectsInUserModules");
        public static CommandKey OpenObject { get; } = Key("OpenObject");
        public static CommandKey SetObjectPropertyText { get; } = Key("SetObjectPropertyText");
        public static CommandKey ApplyObjectPropertyText { get; } = Key("ApplyObjectPropertyText");
        public static CommandKey AssignAttributeToVariable { get; } = Key("AssignAttributeToVariable");
        public static CommandKey AssignDomainToVariable { get; } = Key("AssignDomainToVariable");
        public static CommandKey AssignAttributeOrDomainToVariable { get; } = Key("AssignAttributeOrDomainToVariable");

        // Cleanup and UI
        public static CommandKey CleanVarsNotUsed { get; } = Key("CleanVarsNotUsed");
        public static CommandKey CleanObjects { get; } = Key("CleanObjects");
        public static CommandKey CleanKBAsMuchAsPossible { get; } = Key("CleanKBAsMuchAsPossible");
        public static CommandKey FixVariablesNotBasedInAttributesOrDomain { get; } = Key("FixVariablesNotBasedInAttributesOrDomain");
        public static CommandKey ResetWINForm { get; } = Key("ResetWINForm");
        public static CommandKey ChangeCommitOnExit { get; } = Key("ChangeCommitOnExit");
        public static CommandKey ReplaceNullCompatible { get; } = Key("ReplaceNullCompatible");
        public static CommandKey WebFormToAbstractEditor { get; } = Key("WebFormToAbstractEditor");
        public static CommandKey ImproveControlClasses { get; } = Key("ImproveControlClasses");
        public static CommandKey AuditWebPanelsSmoothUX { get; } = Key("AuditWebPanelsSmoothUX");
        public static CommandKey ConvertSafeWebPanelsToSmoothUX { get; } = Key("ConvertSafeWebPanelsToSmoothUX");
        public static CommandKey RunResponsiveSmoothAction { get; } = Key("RunResponsiveSmoothAction");

        // Navigation and comparisons
        public static CommandKey PrepareComparerNavigations { get; } = Key("PrepareComparerNavigations");
        public static CommandKey OpenFolderComparerNavigation { get; } = Key("OpenFolderComparerNavigation");
        public static CommandKey OpenFolderObjComparerNavigation { get; } = Key("OpenFolderObjComparerNavigation");
        public static CommandKey CompareLastNVGDirectory { get; } = Key("CompareLastNVGDirectory");
        public static CommandKey CompareLastOBJDirectory { get; } = Key("CompareLastOBJDirectory");
        public static CommandKey CalculateCheckSum { get; } = Key("CalculateCheckSum");
        public static CommandKey ListObjWarningsErrors { get; } = Key("ListObjWarningsErrors");
        public static CommandKey ListObjSimilarNavigation { get; } = Key("ListObjSimilarNavigation");

        // Modularization
        public static CommandKey BuildModule { get; } = Key("BuildModule");
        public static CommandKey GenerateGraph { get; } = Key("GenerateGraph");
        public static CommandKey ListModules { get; } = Key("ListModules");
        public static CommandKey ListModulesStatistics { get; } = Key("ListModulesStatistics");
        public static CommandKey MoveTransactions { get; } = Key("MoveTransactions");
        public static CommandKey ModuleDependencies { get; } = Key("ModuleDependencies");
        public static CommandKey ListAPIObjects { get; } = Key("ListAPIObjects");
        public static CommandKey RecomendedModule { get; } = Key("RecomendedModule");
        public static CommandKey ListModularizationQuality { get; } = Key("ListModularizationQuality");
        public static CommandKey AddModularizationInfo { get; } = Key("AddModularizationInfo");
        public static CommandKey ApplyExternalModularization { get; } = Key("ApplyExternalModularization");
        public static CommandKey DetectMavericks { get; } = Key("DetectMavericks");
        public static CommandKey SplitMainObject { get; } = Key("SplitMainObject");
        public static CommandKey MakeWorkflowObjectsPublic { get; } = Key("MakeWorkflowObjectsPublic");
        public static CommandKey GeneratedObjectsNotReachableFromDeploymentUnits { get; } = Key("GeneratedObjectsNotReachableFromDeploymentUnits");
        public static CommandKey MoveObjectsToModulesFromFile { get; } = Key("MoveObjectsToModulesFromFile");

        // Review
        public static CommandKey PreprocessPendingObjects { get; } = Key("PreprocessPendingObjects");
        public static CommandKey ReviewObjects { get; } = Key("ReviewObjects");
        public static CommandKey ReviewModuleOrFolder { get; } = Key("ReviewModuleOrFolder");
        public static CommandKey ReviewObject { get; } = Key("ReviewObject");
        public static CommandKey EditReviewObjects { get; } = Key("EditReviewObjects");

        // Utilities and lab checks
        public static CommandKey SearchAndReplace { get; } = Key("SearchAndReplace");
        public static CommandKey ApplySearchAndReplace { get; } = Key("ApplySearchAndReplace");
        public static CommandKey GenerateLocationXML { get; } = Key("GenerateLocationXML");
        public static CommandKey CopyEnvironment { get; } = Key("CopyEnvironment");
        public static CommandKey ApplyCopyEnvironment { get; } = Key("ApplyCopyEnvironment");
        public static CommandKey RenameAttributesAndTables { get; } = Key("RenameAttributesAndTables");
        public static CommandKey RenameVariables { get; } = Key("RenameVariables");
        public static CommandKey CountTableAccess { get; } = Key("CountTableAccess");
        public static CommandKey CountGeneratedByPattern { get; } = Key("CountGeneratedByPattern");
        public static CommandKey GeneratedByPatternWithoutDynamism { get; } = Key("GeneratedByPatternWithoutDynamism");
        public static CommandKey GenerateSQLScripts { get; } = Key("GenerateSQLScripts");
        public static CommandKey GenerateDPfromTable { get; } = Key("GenerateDPfromTable");
        public static CommandKey GenerateRESTCalls { get; } = Key("GenerateRESTCalls");
        public static CommandKey SDTsWithDateInWS { get; } = Key("SDTsWithDateInWS");
        public static CommandKey GenerateSDTDataLoad { get; } = Key("GenerateSDTDataLoad");
        public static CommandKey CreateDeployUnits { get; } = Key("CreateDeployUnits");
        public static CommandKey MarkPublicObjects { get; } = Key("MarkPublicObjects");
        public static CommandKey UDPCallables { get; } = Key("UDPCallables");
        public static CommandKey CheckBldObjects { get; } = Key("CheckBldObjects");
        public static CommandKey CheckVariableUsages { get; } = Key("CheckVariableUsages");
        public static CommandKey VariablesNotBasedOnAttributes { get; } = Key("VariablesNotBasedOnAttributes");
        public static CommandKey AssignTypeComparer { get; } = Key("AssignTypeComparer");
        public static CommandKey ParameterTypeComparer { get; } = Key("ParameterTypeComparer");
        public static CommandKey ObjectsWithRuleOld { get; } = Key("ObjectsWithRuleOld");
        public static CommandKey EmptyConditionalBlocks { get; } = Key("EmptyConditionalBlocks");
        public static CommandKey NewsWithoutWhenDuplicate { get; } = Key("NewsWithoutWhenDuplicate");
        public static CommandKey ForEachsWithoutWhenNone { get; } = Key("ForEachsWithoutWhenNone");
        public static CommandKey ConstantsInCode { get; } = Key("ConstantsInCode");
        public static CommandKey ReviewCommits { get; } = Key("ReviewCommits");

        // Themes
        public static CommandKey ClassNotInTheme { get; } = Key("ClassNotInTheme");
        public static CommandKey ClassUsed { get; } = Key("ClassUsed");
        public static CommandKey ThemeClassesNotUsed { get; } = Key("ThemeClassesNotUsed");
        public static CommandKey ObjThemeClassesNotUsed { get; } = Key("ObjThemeClassesNotUsed");

        // Help
        public static CommandKey AboutKBDoctor { get; } = Key("AboutKBDoctor");
        public static CommandKey HelpKBDoctor { get; } = Key("HelpKBDoctor");
        public static CommandKey ListLastReports { get; } = Key("ListLastReports");

        // Labs
        public static CommandKey AddINParmRule { get; } = Key("AddINParmRule");
        public static CommandKey ListTableAttributesUsingDomain { get; } = Key("ListTableAttributesUsingDomain");
        public static CommandKey ProcedureSDT { get; } = Key("ProcedureSDT");
        public static CommandKey ProcedureGetSet { get; } = Key("ProcedureGetSet");
        public static CommandKey ChangeLegacyCode { get; } = Key("ChangeLegacyCode");
        public static CommandKey EditLegacyCodeToReplace { get; } = Key("EditLegacyCodeToReplace");
        public static CommandKey TreeCommit { get; } = Key("TreeCommit");
    }
}
