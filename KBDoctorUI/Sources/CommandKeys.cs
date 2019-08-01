using System;
using System.Collections.Generic;
using System.Text;
using Artech.Common.Framework.Commands;

namespace Concepto.Packages.KBDoctor
{
    static class CommandKeys
    {
        // Atributos
        private static CommandKey attWithNoDomain = new CommandKey(Package.guid, "AttWithNoDomain");
        private static CommandKey listAttributes = new CommandKey(Package.guid, "ListAttributes");
        private static CommandKey attWithoutDescription = new CommandKey(Package.guid, "AttWithoutDescription");
        private static CommandKey attCharToVarchar = new CommandKey(Package.guid, "AttCharToVarchar");
        private static CommandKey attVarcharToChar = new CommandKey(Package.guid, "AttVarcharToChar");
        private static CommandKey attKeyVarchar = new CommandKey(Package.guid, "AttKeyVarchar");
        private static CommandKey attDescWithoutUniqueIndex = new CommandKey(Package.guid, "AttDescWithoutUniqueIndex");
        private static CommandKey attNotReferenced = new CommandKey(Package.guid, "AttNotReferenced");
        private static CommandKey attWithoutBaseTable = new CommandKey(Package.guid, "AttWithoutBaseTable");
        private static CommandKey attInOneTrnOnly = new CommandKey(Package.guid, "AttInOneTrnOnly");
        private static CommandKey attFormula = new CommandKey(Package.guid, "AttFormula");
        private static CommandKey replaceDomain = new CommandKey(Package.guid, "ReplaceDomain");
        private static CommandKey listDomain = new CommandKey(Package.guid, "ListDomain");
        private static CommandKey attUpdated = new CommandKey(Package.guid, "AttUpdated");
       
        public static CommandKey AttWithNoDomain { get { return attWithNoDomain; } }
        public static CommandKey ListAttributes { get { return listAttributes; } }
        public static CommandKey AttWithoutDescription { get { return attWithoutDescription; } }
        public static CommandKey AttCharToVarchar { get { return attCharToVarchar; } }
        public static CommandKey AttVarcharToChar { get { return attVarcharToChar; } }
        public static CommandKey AttKeyVarchar { get { return attKeyVarchar; } }
        public static CommandKey AttDescWithoutUniqueIndex { get { return attDescWithoutUniqueIndex; } }
        public static CommandKey AttNotReferenced { get { return attNotReferenced; } }
        public static CommandKey AttWithoutBaseTable { get { return attWithoutBaseTable; } }
        public static CommandKey AttInOneTrnOnly { get { return attInOneTrnOnly; } }
        public static CommandKey AttFormula { get { return attFormula; } }
        public static CommandKey ReplaceDomain { get { return replaceDomain; } }
        public static CommandKey ListDomain { get { return listDomain; } }
        public static CommandKey AttUpdated { get { return attUpdated; } }


        // Acciones sobre atributos
        private static CommandKey assignDomainToAttribute = new CommandKey(Package.guid, "AssignDomainToAttribute");
        private static CommandKey assignDescriptionToAttribute = new CommandKey(Package.guid, "AssignDescriptionToAttribute");
        private static CommandKey assignTitleToAttribute = new CommandKey(Package.guid, "AssignTitleToAttribute");
        private static CommandKey assignColumnTitleToAttribute = new CommandKey(Package.guid, "AssignColumnTitleToAttribute");
        private static CommandKey listAttribute = new CommandKey(Package.guid, "ListAttribute");
        private static CommandKey addDescriptorIndex = new CommandKey(Package.guid, "AddDescriptorIndex");
  
        public static CommandKey AssignDomainToAttribute { get { return assignDomainToAttribute; } }
        public static CommandKey AssignDescriptionToAttribute { get { return assignDescriptionToAttribute; } }
        public static CommandKey AssignTitleToAttribute { get { return assignTitleToAttribute; } }
        public static CommandKey AssignColumnTitleToAttribute { get { return assignColumnTitleToAttribute; } }
        public static CommandKey ListAttribute { get { return listAttribute; } }
        public static CommandKey AddDescriptorIndex { get { return addDescriptorIndex; } }
        

        // Tablas
        private static CommandKey tblWihNoDescription = new CommandKey(Package.guid, "TablesWithNoDescription");
        public static CommandKey TblWihNoDescription { get { return tblWihNoDescription; } }
        
        private static CommandKey grpWihNoDescription = new CommandKey(Package.guid, "GroupWithNoDescription");
        public static CommandKey GrpWihNoDescription { get { return grpWihNoDescription; } }
        
        private static CommandKey tblWidth            = new CommandKey(Package.guid, "TablesWidth");
        public static CommandKey TblWidth { get { return tblWidth; } }

        private static CommandKey listTables = new CommandKey(Package.guid, "ListTables");
        public static CommandKey ListTables { get { return listTables; } }

        private static CommandKey tblTableTransaction = new CommandKey(Package.guid, "TableTransaction");
        public static CommandKey TblTableTransaction { get { return tblTableTransaction; } }

        private static CommandKey tblGenerateSimpleTransactionFromNotGeneratedTransactions = new CommandKey(Package.guid, "GenerateSimpleTransactionFromNotGeneratedTransactions");
        public static CommandKey TblGenerateSimpleTransactionFromNotGeneratedTransactions { get { return tblGenerateSimpleTransactionFromNotGeneratedTransactions; } }

        private static CommandKey tblTableUpdate      = new CommandKey(Package.guid, "TableUpdate");
        public static CommandKey TblTableUpdate { get { return tblTableUpdate; } }

        private static CommandKey tblTableInsertNew = new CommandKey(Package.guid, "TableInsertNew");
        public static CommandKey TblTableInsertNew { get { return tblTableInsertNew; } }

        private static CommandKey tblGenerateTrnFromTables = new CommandKey(Package.guid, "GenerateTrnFromTables");
        public static CommandKey GenerateTrnFromTables { get { return tblGenerateTrnFromTables; } }

        private static CommandKey tblGenerateTrnFromTables2 = new CommandKey(Package.guid, "GenerateTrnFromTables2");
        public static CommandKey GenerateTrnFromTables2 { get { return tblGenerateTrnFromTables2; } }

        private static CommandKey tblListTablesInModules = new CommandKey(Package.guid, "ListTablesInModules");
        public static CommandKey ListTablesInModules { get { return tblListTablesInModules; } }

        private static CommandKey tblListObjectsWithTableInOtherModule = new CommandKey(Package.guid, "ListObjectsWithTableInOtherModule");
        public static CommandKey ListObjectsWithTableInOtherModule { get { return tblListObjectsWithTableInOtherModule; } }

        // Acciones sobre tablas
        private static CommandKey assignDescriptionToTable = new CommandKey(Package.guid, "AssignDescriptionToTable");

        public static CommandKey AssignDescriptionToTable { get { return assignDescriptionToTable; } }

        // Indices
        private static CommandKey indexWithNotRefAtt = new CommandKey(Package.guid, "IndexWithNotRefAtt");
        private static CommandKey removeIndexAttribute = new CommandKey(Package.guid, "RemoveIndexAttribute");

        public static CommandKey IndexWithNotRefAtt { get { return indexWithNotRefAtt; } }
        public static CommandKey RemoveIndexAttribute { get { return removeIndexAttribute; } }
        

        // Objetos
        private static CommandKey objNotReacheable = new CommandKey(Package.guid, "ObjectsNotReacheable");
        private static CommandKey objectsWithoutInOut = new CommandKey(Package.guid, "ObjectsWithoutInOut");
        private static CommandKey objectsMainCalled = new CommandKey(Package.guid, "ObjectsMainCalled");
        private static CommandKey objectsReferenced = new CommandKey(Package.guid, "ObjectsReferenced");
        private static CommandKey objectsWithVarNotBasedOnAtt = new CommandKey(Package.guid, "ObjectsWithVarNotBasedOnAtt");
        private static CommandKey renameVariables = new CommandKey(Package.guid, "RenameVariables");
        private static CommandKey buildModule = new CommandKey(Package.guid, "BuildModule");
        private static CommandKey buildModuleContext = new CommandKey(Package.guid, "BuildModuleContext");
        private static CommandKey buildObjectAndReferences = new CommandKey(Package.guid, "BuildObjectAndReferences");
        private static CommandKey buildObjectWithProperty = new CommandKey(Package.guid, "BuildObjectWithProperty");
        private static CommandKey objectsNotCalled = new CommandKey(Package.guid, "ObjectsNotCalled");
        private static CommandKey objectsWithCommitOnExit = new CommandKey(Package.guid, "ObjectsWithCommitOnExit");
        private static CommandKey objectsWithVarsNotUsed = new CommandKey(Package.guid, "ObjectsWithVarsNotUsed");
        private static CommandKey listDynamicCombo = new CommandKey(Package.guid, "ListDynamicCombo");

        private static CommandKey cleanVarsNotUsed = new CommandKey(Package.guid, "CleanVarsNotUsed");
        private static CommandKey objectsComplex = new CommandKey(Package.guid, "ObjectsComplex");
        private static CommandKey objectsLegacyCode = new CommandKey(Package.guid, "ObjectsLegacyCode");
        private static CommandKey changeLegacyCode = new CommandKey(Package.guid, "ChangeLegacyCode");

        private static CommandKey changeCommitOnExit = new CommandKey(Package.guid, "ChangeCommitOnExit");
        private static CommandKey treeCommit = new CommandKey(Package.guid, "TreeCommit");
        private static CommandKey editLegacyCodeToReplace = new CommandKey(Package.guid, "EditLegacyCodeToReplace");
        private static CommandKey editReviewObjects = new CommandKey(Package.guid, "EditReviewObjects");

        private static CommandKey objectsRefactoringCandidates = new CommandKey(Package.guid, "ObjectsRefactoringCandidates");
        private static CommandKey countTableAccess = new CommandKey(Package.guid, "CountTableAccess");
        private static CommandKey objectsWithConstants = new CommandKey(Package.guid, "ObjectsWithConstants");
        private static CommandKey fixVariablesNotBasedInAttributesOrDomain = new CommandKey(Package.guid, "FixVariablesNotBasedInAttributesOrDomain");

        private static CommandKey objectsWIWEB = new CommandKey(Package.guid, "ObjectsWINWEB");
        private static CommandKey listProcedureCallWebpanelTransaction = new CommandKey(Package.guid, "ListProcedureCallWebpanelTransaction");
        
        private static CommandKey mainTableUsed = new CommandKey(Package.guid, "MainTableUsed");
        private static CommandKey removableTransactions = new CommandKey(Package.guid, "RemovableTransactions");
        private static CommandKey generateSQLScripts  = new CommandKey(Package.guid, "GenerateSQLScripts");
        private static CommandKey generateDPfromTable  = new CommandKey(Package.guid, "GenerateDPfromTable");

        private static CommandKey generateGraph = new CommandKey(Package.guid, "GenerateGraph");
        private static CommandKey themeClassesNotUsed = new CommandKey(Package.guid, "ThemeClassesNotUsed");
        private static CommandKey parameterTypeComparer = new CommandKey(Package.guid, "ParameterTypeComparer");
        private static CommandKey objectsWithRuleOld = new CommandKey(Package.guid, "ObjectsWithRuleOld");
        private static CommandKey emptyConditionalBlocks = new CommandKey(Package.guid, "EmptyConditionalBlocks");
        private static CommandKey newsWithoutWhenDuplicate = new CommandKey(Package.guid, "NewsWithoutWhenDuplicate");
        private static CommandKey forEachsWithoutWhenNone = new CommandKey(Package.guid, "ForEachsWithoutWhenNone");
        private static CommandKey constantsInCode = new CommandKey(Package.guid, "ConstantsInCode");
        private static CommandKey reviewCommits = new CommandKey(Package.guid, "ReviewCommits");
        private static CommandKey generateRESTCalls = new CommandKey(Package.guid, "GenerateRESTCalls");
        private static CommandKey sDTsWithDateInWS = new CommandKey(Package.guid, "SDTsWithDateInWS");
        private static CommandKey generateSDTDataLoad = new CommandKey(Package.guid, "GenerateSDTDataLoad");
        

        public static CommandKey ObjNotReacheable { get { return objNotReacheable; } }
        public static CommandKey ObjectsWithoutInOut { get { return objectsWithoutInOut; } }
        public static CommandKey ObjectsMainCalled { get { return objectsMainCalled; } }
        public static CommandKey ObjectsReferenced { get { return objectsReferenced; } }
        public static CommandKey ObjectsWithVarNotBasedOnAtt { get { return objectsWithVarNotBasedOnAtt; } }
        public static CommandKey ListDynamicCombo { get { return listDynamicCombo; } }
        public static CommandKey RenameVariables { get { return renameVariables; } }
        public static CommandKey BuildModule { get { return buildModule; } }

        public static CommandKey BuildModuleContext { get { return buildModuleContext; } }
        public static CommandKey BuildObjectAndReferences { get { return buildObjectAndReferences; } }
        public static CommandKey BuildObjectWithProperty { get { return buildObjectWithProperty; } }
        public static CommandKey ObjectsNotCalled { get { return objectsNotCalled; } }
        public static CommandKey ObjectsWithCommitOnExit { get { return objectsWithCommitOnExit; } }
        public static CommandKey ObjectsWithVarsNotUsed { get { return objectsWithVarsNotUsed; } }

        public static CommandKey CleanVarsNotUsed { get { return cleanVarsNotUsed; } }
        public static CommandKey ObjectsComplex { get { return objectsComplex; } }
        public static CommandKey ObjectsLegacyCode { get { return objectsLegacyCode; } }
        public static CommandKey ChangeLegacyCode { get { return changeLegacyCode; } }
        public static CommandKey ChangeCommitOnExit { get { return changeCommitOnExit; } }
        public static CommandKey TreeCommit { get { return treeCommit; } }
        public static CommandKey EditLegacyCodeToReplace { get { return editLegacyCodeToReplace; } }
        public static CommandKey EditReviewObjects { get { return editReviewObjects; } }
        public static CommandKey ObjectsRefactoringCandidates { get { return objectsRefactoringCandidates; } }

        public static CommandKey CountTableAccess { get { return countTableAccess; } }

        public static CommandKey ObjectsWithConstants { get { return objectsWithConstants; } }

        private static CommandKey objectsUpdateAttribute = new CommandKey(Package.guid, "ObjectsUpdateAttribute");
        public static CommandKey ObjectsUpdateAttribute { get { return objectsUpdateAttribute; } }

        public static CommandKey FixVariablesNotBasedInAttributesOrDomain { get { return fixVariablesNotBasedInAttributesOrDomain; } }
        public static CommandKey ObjectsWINWEB { get { return objectsWIWEB; } }
        public static CommandKey ListProcedureCallWebpanelTransaction { get { return listProcedureCallWebpanelTransaction; } }
        public static CommandKey MainTableUsed { get { return mainTableUsed; } }
        public static CommandKey RemovableTransactions { get { return removableTransactions; } }
        public static CommandKey GenerateSQLScripts { get { return generateSQLScripts; } }
        public static CommandKey GenerateDPfromTable { get { return generateDPfromTable; } }

        public static CommandKey GenerateGraph { get { return generateGraph; } }
        public static CommandKey ThemeClassesNotUsed { get { return themeClassesNotUsed; } }
        public static CommandKey ParameterTypeComparer { get { return parameterTypeComparer; } }
        public static CommandKey ObjectsWithRuleOld { get { return objectsWithRuleOld; } }
        public static CommandKey EmptyConditionalBlocks { get { return emptyConditionalBlocks; } }
        public static CommandKey NewsWithoutWhenDuplicate { get { return newsWithoutWhenDuplicate; } }
        public static CommandKey ForEachsWithoutWhenNone { get { return forEachsWithoutWhenNone; } }
        public static CommandKey ConstantsInCode { get { return constantsInCode; } }
        public static CommandKey ReviewCommits { get { return reviewCommits; } }
        public static CommandKey GenerateRESTCalls { get { return generateRESTCalls; } }
        public static CommandKey SDTsWithDateInWS { get { return sDTsWithDateInWS; } }
        public static CommandKey GenerateSDTDataLoad { get { return generateSDTDataLoad; } }
        

        // Acciones sobre objetos
        private static CommandKey removeObject = new CommandKey(Package.guid, "RemoveObject");
        private static CommandKey openObject = new CommandKey(Package.guid, "OpenObject");
        private static CommandKey assignDomainToVariable = new CommandKey(Package.guid, "AssignDomainToVariable");
        private static CommandKey assignAttributeToVariable = new CommandKey(Package.guid, "AssignAttributeToVariable");
        private static CommandKey renameAttributesAndTables =  new CommandKey(Package.guid, "RenameAttributesAndTables");


        public static CommandKey RemoveObject { get { return removeObject; } }
        public static CommandKey OpenObject { get { return openObject; } }
        public static CommandKey AssignDomainToVariable { get { return assignDomainToVariable; } }
        public static CommandKey AssignAttributeToVariable { get { return assignAttributeToVariable; } }

        public static CommandKey RenameAttributesAndTables { get { return renameAttributesAndTables; } }

        private static CommandKey preprocessPendingObjects = new CommandKey(Package.guid, "PreprocessPendingObjects");
        public static CommandKey PreprocessPendingObjects { get { return preprocessPendingObjects; } }

        private static CommandKey reviewObjects = new CommandKey(Package.guid, "ReviewObjects");
        public static CommandKey ReviewObjects { get { return reviewObjects; } }

        private static CommandKey reviewObject = new CommandKey(Package.guid, "ReviewObject");
        public static CommandKey ReviewObject { get { return reviewObject; } }

        private static CommandKey reviewModuleOrFolder = new CommandKey(Package.guid, "ReviewModuleOrFolder");
        public static CommandKey ReviewModuleOrFolder { get { return reviewModuleOrFolder; } }

        private static CommandKey assignTypeComparer = new CommandKey(Package.guid, "AssignTypeComparer");
        public static CommandKey AssignTypeComparer { get { return assignTypeComparer; } }
        //Surgery
        private static CommandKey procedureSDT = new CommandKey(Package.guid, "ProcedureSDT");
        public static CommandKey ProcedureSDT { get { return procedureSDT; } }

        private static CommandKey procedureGetSet = new CommandKey(Package.guid, "ProcedureGetSet");
        public static CommandKey ProcedureGetSet { get { return procedureGetSet; } }

        private static CommandKey addINPramRule = new CommandKey(Package.guid, "AddINParmRule");
        public static CommandKey AddINParmRule { get { return addINPramRule; } }

        private static CommandKey listTableAttributesUsingDomain = new CommandKey(Package.guid, "ListTableAttributesUsingDomain");
        public static CommandKey ListTableAttributesUsingDomain { get { return listTableAttributesUsingDomain; } }

        private static CommandKey cleanKBAsMuchAsPossible = new CommandKey(Package.guid, "CleanKBAsMuchAsPossible");
        public static CommandKey CleanKBAsMuchAsPossible { get { return cleanKBAsMuchAsPossible; } }

        private static CommandKey cleanObjects = new CommandKey(Package.guid, "CleanObjects");
        public static CommandKey CleanObjects { get { return cleanObjects; } }

        private static CommandKey resetWINForm = new CommandKey(Package.guid, "ResetWINForm");
        public static CommandKey ResetWINForm { get { return resetWINForm; } }

        private static CommandKey searchAndReplace = new CommandKey(Package.guid, "SearchAndReplace");
        public static CommandKey SearchAndReplace { get { return searchAndReplace; } }

        private static CommandKey classNotInTheme = new CommandKey(Package.guid, "ClassNotInTheme");
        public static CommandKey ClassNotInTheme { get { return classNotInTheme; } }

        private static CommandKey classUsed = new CommandKey(Package.guid, "ClassUsed");
        public static CommandKey ClassUsed { get { return classUsed; } }

        private static CommandKey listClassUsed = new CommandKey(Package.guid, "ListClassUsed");
        public static CommandKey ListClassUsed { get { return listClassUsed; } }

        private static CommandKey historyGXServer = new CommandKey(Package.guid, "HistoryGXServer");
        public static CommandKey HistoryGXServer { get { return historyGXServer; } }

        //Stats
        private static CommandKey countGeneratedByPattern = new CommandKey(Package.guid, "CountGeneratedByPattern");
        public static CommandKey CountGeneratedByPattern { get { return countGeneratedByPattern; } }

        private static CommandKey replaceNullCompatible = new CommandKey(Package.guid, "ReplaceNullCompatible");
        public static CommandKey ReplaceNullCompatible { get { return replaceNullCompatible; } }

        private static CommandKey listObj = new CommandKey(Package.guid, "ListObj");
        public static CommandKey ListObj { get { return listObj; } }

        private static CommandKey createDeployUnits = new CommandKey(Package.guid, "CreateDeployUnits");
        public static CommandKey CreateDeployUnits { get { return createDeployUnits; } }

        private static CommandKey markPublicObjects = new CommandKey(Package.guid, "MarkPublicObjects");
        public static CommandKey MarkPublicObjects { get { return markPublicObjects; } }


        private static CommandKey listModules = new CommandKey(Package.guid, "ListModules");
        public static CommandKey ListModules { get { return listModules; } }

        private static CommandKey listModulesStatistics = new CommandKey(Package.guid, "ListModulesStatistics");
        public static CommandKey ListModulesStatistics { get { return listModulesStatistics; } }

        private static CommandKey listModularizationQuality = new CommandKey(Package.guid, "ListModularizationQuality");
        public static CommandKey ListModularizationQuality { get { return listModularizationQuality; } }

        private static CommandKey detectMavericks = new CommandKey(Package.guid, "DetectMavericks");
        public static CommandKey DetectMavericks { get { return detectMavericks; } }

        private static CommandKey moveTransactions = new CommandKey(Package.guid, "MoveTransactions");
        public static CommandKey MoveTransactions { get { return moveTransactions; } }

        private static CommandKey moduleDependencies = new CommandKey(Package.guid, "ModuleDependencies");
        public static CommandKey ModuleDependencies { get { return moduleDependencies; } }

        private static CommandKey calculateCheckSum = new CommandKey(Package.guid, "CalculateCheckSum");
        public static CommandKey CalculateCheckSum { get { return calculateCheckSum; } }

        private static CommandKey generateLocationXML = new CommandKey(Package.guid, "GenerateLocationXML");
        public static CommandKey GenerateLocationXML { get { return generateLocationXML; } }

        private static CommandKey listObjWarningsErrors = new CommandKey(Package.guid, "ListObjWarningsErrors");
        public static CommandKey ListObjWarningsErrors { get { return listObjWarningsErrors; } }

        private static CommandKey listObjSimilarNavigation = new CommandKey(Package.guid, "ListObjSimilarNavigation");
        public static CommandKey ListObjSimilarNavigation { get { return listObjSimilarNavigation; } }

        private static CommandKey listAPIObjects = new CommandKey(Package.guid, "ListAPIObjects");
        public static CommandKey ListAPIObjects { get { return listAPIObjects; } }

        private static CommandKey recomendedModule = new CommandKey(Package.guid, "RecomendedModule");
        public static CommandKey RecomendedModule { get { return recomendedModule; } }

        private static CommandKey applyExternalModularization = new CommandKey(Package.guid, "ApplyExternalModularization");
        public static CommandKey ApplyExternalModularization { get { return applyExternalModularization; } }

        private static CommandKey splitMainObject = new CommandKey(Package.guid, "SplitMainObject");
        public static CommandKey SplitMainObject { get { return splitMainObject; } }

        private static CommandKey prepareComparerNavigation = new CommandKey(Package.guid, "PrepareComparerNavigation");
        public static CommandKey PrepareComparerNavigation { get { return prepareComparerNavigation; } }

        private static CommandKey kbInterfaces = new CommandKey(Package.guid, "KBInterfaces");
        public static CommandKey  KBInterfaces   { get { return kbInterfaces; } }

        private static CommandKey compareLastNVGDirectory = new CommandKey(Package.guid, "CompareLastNVGDirectory");
        public static CommandKey CompareLastNVGDirectory { get { return compareLastNVGDirectory; } }

        private static CommandKey openFolderComparerNavigation = new CommandKey(Package.guid, "OpenFolderComparerNavigation");
        public static CommandKey OpenFolderComparerNavigation { get { return openFolderComparerNavigation; } }

        private static CommandKey openFolderObjComparerNavigation = new CommandKey(Package.guid, "OpenFolderObjComparerNavigation");
        public static CommandKey OpenFolderObjComparerNavigation { get { return openFolderObjComparerNavigation; } }

        private static CommandKey compareLastOBJDirectory = new CommandKey(Package.guid, "CompareLastOBJDirectory");
        public static CommandKey CompareLastOBJDirectory { get { return compareLastOBJDirectory; } }

        private static CommandKey listLastReports = new CommandKey(Package.guid, "ListLastReports");
        public static CommandKey UDPCallables { get { return udpCallables; } }

        private static CommandKey udpCallables = new CommandKey(Package.guid, "UDPCallables");
        public static CommandKey ListLastReports { get { return listLastReports; } }
        // Acerca de
        private static CommandKey aboutKBDoctor = new CommandKey(Package.guid, "AboutKBDoctor");
        public static CommandKey AboutKBDoctor { get { return aboutKBDoctor; } }

        private static CommandKey helpKBDoctor = new CommandKey(Package.guid, "HelpKBDoctor");
        public static CommandKey HelpKBDoctor { get { return helpKBDoctor; } }
    }
}
