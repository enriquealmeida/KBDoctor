using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Common.Framework.Selection;
using Artech.Architecture.UI.Framework.Controls;
using Infragistics.Win.UltraWinGrid;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Common.Descriptors;
//using Concepto.Packages.KBDoctorCore.Sources;
using API = Concepto.Packages.KBDoctorCore.Sources.API;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    class CommandManager : CommandDelegator
    {
        public CommandManager()
        {
            RegisterAttributeCommands();
            RegisterTableCommands();
            RegisterObjectCommands();
            RegisterObjectActionCommands();
            RegisterKbCleanupCommands();
            RegisterNavigationCompareCommands();
            RegisterModularizationCommands();
            RegisterReviewCommands();
            RegisterUtilityCommands();
            RegisterThemeCommands();
            RegisterHelpCommands();
            RegisterLabCommands();
        }

        private void Register(CommandKey key, ExecHandler execHandler)
        {
            Register(key, execHandler, new QueryHandler(QueryKBDoctor));
        }

        private void Register(CommandKey key, ExecHandler execHandler, QueryHandler queryHandler)
        {
            AddCommand(key, execHandler, queryHandler);
        }

        private void RegisterAttributeCommands()
        {
            Register(CommandKeys.ListAttributes, ExecListAttributes);
            Register(CommandKeys.ListAttribute, ExecListAttributes);
            Register(CommandKeys.AttWithNoDomain, ExecAttWithNoDomain);
            Register(CommandKeys.AttWithoutDescription, ExecAttWithoutDescription);
            Register(CommandKeys.AttInOneTrnOnly, ExecAttInOneTrnOnly);
            Register(CommandKeys.AttFormula, ExecAttFormula);
            Register(CommandKeys.AttUpdated, ExecAttUpdated);
            Register(CommandKeys.AttCharToVarchar, ExecAttCharToVarchar);
            Register(CommandKeys.AttVarcharToChar, ExecAttVarcharToChar);
            Register(CommandKeys.AttKeyVarchar, ExecAttKeyVarchar);
            Register(CommandKeys.AttDescWithoutUniqueIndex, ExecAttDescWithoutUniqueIndex);
            Register(CommandKeys.AttWithoutBaseTable, ExecAttWithoutBaseTable);
            Register(CommandKeys.ReplaceDomain, ExecReplaceDomain);
            Register(CommandKeys.ApplyReplaceDomain, ExecApplyReplaceDomain);
            Register(CommandKeys.ListDomain, ExecListDomain);
            Register(CommandKeys.ApplyAttUpdated, ExecApplyAttUpdated);

            Register(CommandKeys.AssignDomainToAttribute, ExecAssignDomainToAttribute);
            Register(CommandKeys.AssignDescriptionToAttribute, ExecAssignDescriptionToAttribute);
            Register(CommandKeys.ApplyAttributeText, ExecApplyAttributeText);
            Register(CommandKeys.AssignTitleToAttribute, ExecAssignTitleToAttribute);
            Register(CommandKeys.AssignColumnTitleToAttribute, ExecAssignColumnTitleToAttribute);
            Register(CommandKeys.AddDescriptorIndex, ExecAddDescriptorIndex);
        }

        private void RegisterTableCommands()
        {
            Register(CommandKeys.ListTables, ExecListTables);
            Register(CommandKeys.TblWihNoDescription, ExecTblWihNoDescription);
            Register(CommandKeys.GrpWihNoDescription, ExecGrpWihNoDescription);
            Register(CommandKeys.TblWidth, ExecTblWidth);
            Register(CommandKeys.TblTableTransaction, ExecTblTableTransaction);
            Register(CommandKeys.TblGenerateSimpleTransactionFromNotGeneratedTransactions, ExecTblGenerateSimpleTransactionFromNotGeneratedTransactions);
            Register(CommandKeys.TblScriptToCompareNULLABLE_GXvsDB, ExecTblScriptToCompareNULLABLE_GXvsDB);
            Register(CommandKeys.TblScriptToCompareNULLABLE_GXvsDB2, ExecTblScriptToCompareNULLABLE_GXvsDB2);
            Register(CommandKeys.TblListTableWithAttributeNullableCompatible, ExecTblListTableWithAttributeNullableCompatible);
            Register(CommandKeys.TblTableUpdate, ExecTblTableUpdate);
            Register(CommandKeys.TblTableInsertNew, ExecTblTableInsertNew);
            Register(CommandKeys.GenerateTrnFromTables, ExecGenerateTrnFromTables);
            Register(CommandKeys.GenerateTrnFromTables2, ExecGenerateTrnFromTables2);
            Register(CommandKeys.ListTablesInModules, ExecListTablesInModules);
            Register(CommandKeys.ListObjectsWithTableInOtherModule, ExecListObjectsWithTableInOtherModule);

            Register(CommandKeys.AssignDescriptionToTable, ExecAssignDescriptionToTable);
            Register(CommandKeys.ApplyTableText, ExecApplyTableText);
            Register(CommandKeys.IndexWithNotRefAtt, ExecIndexWithNotRefAtt);
            Register(CommandKeys.RemoveIndexAttribute, ExecRremoveIndexAttribute);
        }

        private void RegisterObjectCommands()
        {
            Register(CommandKeys.ListObj, ExecListObj);
            Register(CommandKeys.ListUnreferencedObjectsInUserModules, ExecListUnreferencedObjectsInUserModules);
            Register(CommandKeys.ObjNotReacheable, ExecObjNotReacheable);
            Register(CommandKeys.ObjectsNotCalled, ExecObjectsNotCalled);
            Register(CommandKeys.ObjectsWithoutInOut, ExecObjectsWithoutInOut);
            Register(CommandKeys.ObjectsMainCalled, ExecObjectsMainsCalled);
            Register(CommandKeys.ObjectsReferenced, ExecObjectsReferenced);
            Register(CommandKeys.ObjectsWithVarNotBasedOnAtt, ExecObjectsWithVarNotBasedOnAtt);
            Register(CommandKeys.ObjectsWithVarsNotUsed, ExecObjectsWithVarsNotUsed);
            Register(CommandKeys.ObjectsWithCommitOnExit, ExecObjectsWithCommitOnExit);
            Register(CommandKeys.ListCommitOnExit, ExecListCommitOnExit);
            Register(CommandKeys.ObjectsUpdateAttribute, ExecProceduresThatUpdatesAttributes);
            Register(CommandKeys.SelectObjectsUpdateAttribute, ExecSelectObjectsUpdateAttribute);
            Register(CommandKeys.ApplyObjectsUpdateAttribute, ExecApplyObjectsUpdateAttribute);
            Register(CommandKeys.ObjectsLegacyCode, ExecObjectsLegacyCode);
            Register(CommandKeys.ObjectsRefactoringCandidates, ExecObjectsRefactoringCandidates);
            Register(CommandKeys.ObjectsWithConstants, ExecObjectsWithConstants);
            Register(CommandKeys.ObjectsWINWEB, ExecObjectsWINWEB);
            Register(CommandKeys.ListProcedureCallWebpanelTransaction, ExecListProcedureCallWebpanelTransaction);
            Register(CommandKeys.BuildObjectAndReferences, ExecBuildObjectAndReferences);
            Register(CommandKeys.BuildObjectWithProperty, ExecBuildObjectWithProperty);
            Register(CommandKeys.ObjectMigration, ExecObjectMigration);
            Register(CommandKeys.KBInterfaces, ExecKBInterfaces);
            Register(CommandKeys.ListWebObjectsProperties, ExecListWebObjectsProperties);
            Register(CommandKeys.ListProperties, ExecListProperties);
            Register(CommandKeys.ListDynamicCombo, ExecListDynamicCombo);
            Register(CommandKeys.MainTableUsed, ExecMainTableUsed);
            Register(CommandKeys.RemovableTransactions, ExecRemovableTransactions);
            Register(CommandKeys.AttributeAsOutput, ExecAttributeAsOutput);
        }

        private void RegisterObjectActionCommands()
        {
            Register(CommandKeys.RemoveObject, ExecRemoveObject);
            Register(CommandKeys.RemoveUnreferencedObjectsInUserModules, ExecRemoveUnreferencedObjectsInUserModules);
            Register(CommandKeys.OpenObject, ExecOpenKBObject);
            Register(CommandKeys.SetObjectPropertyText, ExecSetObjectPropertyText);
            Register(CommandKeys.ApplyObjectPropertyText, ExecApplyObjectPropertyText);
            Register(CommandKeys.AssignAttributeToVariable, ExecAssignAttributeToVariable);
            Register(CommandKeys.AssignDomainToVariable, ExecAssignDomainToVariable);
            Register(CommandKeys.AssignAttributeOrDomainToVariable, ExecAssignAttributeOrDomainToVariable);
        }

        private void RegisterKbCleanupCommands()
        {
            Register(CommandKeys.CleanVarsNotUsed, ExecCleanVarsNotUsed);
            Register(CommandKeys.CleanObjects, ExecCleanObjects);
            Register(CommandKeys.CleanKBAsMuchAsPossible, ExecCleanKB);
            Register(CommandKeys.FixVariablesNotBasedInAttributesOrDomain, ExecFixVariablesNotBasedInAttributesOrDomain);
            Register(CommandKeys.ResetWINForm, ExecResetWINForm);
            Register(CommandKeys.ChangeCommitOnExit, ExecChangeCommitOnExit);
            Register(CommandKeys.ReplaceNullCompatible, ExecReplaceNullCompatible);
            Register(CommandKeys.WebFormToAbstractEditor, ExecWebFormToAbstractEditor);
            Register(CommandKeys.ImproveControlClasses, ExecImproveControlClasses);
            Register(CommandKeys.AuditWebPanelsSmoothUX, ExecAuditWebPanelsSmoothUX);
            Register(CommandKeys.ConvertSafeWebPanelsToSmoothUX, ExecConvertSafeWebPanelsToSmoothUX);
            Register(CommandKeys.RunResponsiveSmoothAction, ExecRunResponsiveSmoothAction);
        }

        private void RegisterNavigationCompareCommands()
        {
            Register(CommandKeys.PrepareComparerNavigations, ExecPrepareComparerNavigations);
            Register(CommandKeys.OpenFolderComparerNavigation, ExecOpenFolderComparerNavigation);
            Register(CommandKeys.OpenFolderObjComparerNavigation, ExecOpenFolderObjComparerNavigation);
            Register(CommandKeys.CompareLastNVGDirectory, ExecCompareLastNVGDirectory);
            Register(CommandKeys.CompareLastOBJDirectory, ExecCompareLastOBJDirectory);
            Register(CommandKeys.CalculateCheckSum, ExecCalculateCheckSum);
            Register(CommandKeys.ListObjWarningsErrors, ExecListObjWarningsErrors);
            Register(CommandKeys.ListObjSimilarNavigation, ExecListObjSimilarNavigation);
        }

        private void RegisterModularizationCommands()
        {
            Register(CommandKeys.BuildModule, ExecBuildModule);
            Register(CommandKeys.GenerateGraph, ExecGenerateGraph);
            Register(CommandKeys.ListModules, ExecListModules);
            Register(CommandKeys.ListModulesStatistics, ExecListModulesStatistics);
            Register(CommandKeys.MoveTransactions, ExecMoveTransactions);
            Register(CommandKeys.ModuleDependencies, ExecModuleDependencies);
            Register(CommandKeys.ListAPIObjects, ExecListAPIObjects);
            Register(CommandKeys.RecomendedModule, ExecRecomendedModule);
            Register(CommandKeys.ListModularizationQuality, ExecListModularizationQuality);
            Register(CommandKeys.AddModularizationInfo, ExecAddModularizationInfo);
            Register(CommandKeys.ApplyExternalModularization, ExecApplyExternalModularization);
            Register(CommandKeys.DetectMavericks, ExecDetectMavericks);
            Register(CommandKeys.SplitMainObject, ExecSplitMainObject);
        }

        private void RegisterReviewCommands()
        {
            Register(CommandKeys.PreprocessPendingObjects, ExecPreprocessPendingObjects);
            Register(CommandKeys.ReviewObjects, ExecReviewObjects);
            Register(CommandKeys.ReviewModuleOrFolder, ExecReviewModuleOrFolder, new QueryHandler(QueryIsModuleOrFolderSelected));
            Register(CommandKeys.ReviewObject, ExecReviewObject, new QueryHandler(QueryIsKBObjectSelected));
            Register(CommandKeys.EditReviewObjects, ExecEditReviewObjects);
        }

        private void RegisterUtilityCommands()
        {
            Register(CommandKeys.SearchAndReplace, ExecSearchAndReplace);
            Register(CommandKeys.ApplySearchAndReplace, ExecApplySearchAndReplace);
            Register(CommandKeys.GenerateLocationXML, ExecGenerateLocationXML);
            Register(CommandKeys.RenameAttributesAndTables, ExecRenameAttributesAndTables);
            Register(CommandKeys.RenameVariables, ExecRenameVariables);
            Register(CommandKeys.CountTableAccess, ExecCountTableAccess);
            Register(CommandKeys.CountGeneratedByPattern, ExecCountGeneratedByPattern);
            Register(CommandKeys.GeneratedByPatternWithoutDynamism, ExecGeneratedByPatternWithoutDynamism);
            Register(CommandKeys.GenerateSQLScripts, ExecGenerateSQLScripts);
            Register(CommandKeys.GenerateDPfromTable, ExecGenerateDPfromTable);
            Register(CommandKeys.GenerateRESTCalls, ExecGenerateRESTCalls);
            Register(CommandKeys.SDTsWithDateInWS, ExecSDTsWithDateInWS);
            Register(CommandKeys.GenerateSDTDataLoad, ExecGenerateSDTDataLoad);
            Register(CommandKeys.CreateDeployUnits, ExecCreateDeployUnits);
            Register(CommandKeys.MarkPublicObjects, ExecMarkPublicObjects);
            Register(CommandKeys.UDPCallables, ExecUDPCallables);
            Register(CommandKeys.CheckBldObjects, ExecCheckBldObjects);
            Register(CommandKeys.CheckVariableUsages, ExeCheckVariableUsages);
            Register(CommandKeys.VariablesNotBasedOnAttributes, ExecVariablesNotBasedOnAttributes);
            Register(CommandKeys.AssignTypeComparer, ExecAssignTypeComparer);
            Register(CommandKeys.ParameterTypeComparer, ExecParametersTypeComparer);
            Register(CommandKeys.ObjectsWithRuleOld, ExecObjectsWithRuleOld);
            Register(CommandKeys.EmptyConditionalBlocks, ExecEmptyConditionalBlock);
            Register(CommandKeys.NewsWithoutWhenDuplicate, ExecNewsWithoutWhenDuplicate);
            Register(CommandKeys.ForEachsWithoutWhenNone, ExecForEachsWithoutWhenNone);
            Register(CommandKeys.ConstantsInCode, ExecConstantsInCode);
            Register(CommandKeys.ReviewCommits, ExecReviewCommits);
        }

        private void RegisterThemeCommands()
        {
            Register(CommandKeys.ClassNotInTheme, ExecClassNotInTheme);
            Register(CommandKeys.ClassUsed, ExecClassUsed);
            Register(CommandKeys.ThemeClassesNotUsed, ExecThemeClassesNotUsed, new QueryHandler(QueryKBDoctorNoKB));
            Register(CommandKeys.ObjThemeClassesNotUsed, ExecObjThemeClassesNotUsed, new QueryHandler(QueryKBDoctorNoKB));
        }

        private void RegisterHelpCommands()
        {
            Register(CommandKeys.AboutKBDoctor, ExecAboutKBDoctor, new QueryHandler(QueryKBDoctorNoKB));
            Register(CommandKeys.HelpKBDoctor, ExecHelpKBDoctor, new QueryHandler(QueryKBDoctorNoKB));
            Register(CommandKeys.ListLastReports, ExecListLastReports);
        }

        private void RegisterLabCommands()
        {
            Register(CommandKeys.AddINParmRule, ExecAddINParmRule);
            Register(CommandKeys.ListTableAttributesUsingDomain, ExecListTableAttributesUsingDomain);
            Register(CommandKeys.ProcedureSDT, ExecProcedureSDT);
            Register(CommandKeys.ProcedureGetSet, ExecProcedureGetSet);
            Register(CommandKeys.ChangeLegacyCode, ExecChangeLegacyCode);
            Register(CommandKeys.EditLegacyCodeToReplace, ExecEditLegacyCodeToReplace);
            Register(CommandKeys.TreeCommit, ExecTreeCommit);
        }
        #region Atributos

        public bool ExecAttWithoutDescription(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListAttWithoutDescription));
            t.Start();
            return true;
        }

        public bool ExecAttWithNoDomain(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListWithoutDomain));
            t.Start();
            return true;
        }

        public bool ExecListAttributes(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListAttributes));
            t.Start();
            return true;
        }

        public bool ExecAttInOneTrnOnly(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.AttInOneTrnOnly));
            t.Start();
            return true;
        }

        public bool ExecAttCharToVarchar(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListCharToVarchar));
            t.Start();
            return true;
        }

        public bool ExecAttVarcharToChar(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListVarcharToChar));
            t.Start();
            return true;
        }

        public bool ExecAttKeyVarchar(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListKeyVarchar));
            t.Start();
            return true;
        }

        public bool ExecAttDescWithoutUniqueIndex(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListAttDescWithoutUniqueIndex));
            t.Start();
            return true;
        }

        /*  public bool ExecAttNotReferenced(CommandData cmdData)
          {
              // Seria listar los atributos que no son referenciado por ningun programa alcanzable. Abrir los programas que lo referencian y poder borrarlos
              MessageBox.Show("Attributes not referenced by any reachable object not implemented yet");
              return true;
          }*/

        public bool ExecAttWithoutBaseTable(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            // Atributo sin tabla. Abrir objetos que referencien a att sin tablas y que permita corregirlo.
            Thread t = new Thread(new ThreadStart(CleanKBHelper.RemoveAttributeWithoutTable));
            t.Start();
            return true;
        }

        public bool ExecDetectMavericks(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            // Atributo sin tabla. Abrir objetos que referencien a att sin tablas y que permita corregirlo.
            Thread t = new Thread(new ThreadStart(ModulesHelper.DetectMavericks));
            t.Start();
            return true;
        }

        public bool ExecAttFormula(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(AttributesHelper.AttFormula));
            t.Start();
            return true;
        }

        public bool ExecReplaceDomain(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            AttributesHelper.ReplaceDomain();
            return true;
        }

        public bool ExecApplyReplaceDomain(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            AttributesHelper.ApplyReplaceDomain(cmdData.Parameters);
            return true;
        }

        public bool ExecListDomain(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            AttributesHelper.ListDomain();
            return true;
        }

        public bool ExecAttUpdated(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            NavigationHelper.AttUpdated();
            return true;
        }

        public bool ExecApplyAttUpdated(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            NavigationHelper.ApplyAttUpdated(cmdData.Parameters);
            return true;
        }
        #endregion

        #region Acciones sobre atributos
        public bool ExecAssignDomainToAttribute(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            AttributesHelper.AssignDomainToAttribute(cmdData.Parameters);
            return true;
        }

        public bool ExecAssignDescriptionToAttribute(CommandData cmdData)
        {

            AttributesHelper.AssignDescriptionToAttribute(cmdData.Parameters, 0);
            return true;
        }

        public bool ExecApplyAttributeText(CommandData cmdData)
        {
            AttributesHelper.ApplyAttributeText(cmdData.Parameters);
            return true;
        }

        public bool ExecAssignTitleToAttribute(CommandData cmdData)
        {
            AttributesHelper.AssignDescriptionToAttribute(cmdData.Parameters, 1);
            return true;
        }

        public bool ExecAssignColumnTitleToAttribute(CommandData cmdData)
        {
            AttributesHelper.AssignDescriptionToAttribute(cmdData.Parameters, 2);
            return true;
        }



        public bool ExecAddDescriptorIndex(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            AttributesHelper.AddDescriptorIndex(cmdData.Parameters);
            return true;
        }

        #endregion

        #region Tablas
        public bool ExecTblWihNoDescription(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ListWithoutDescription));
            t.Start();
            return true;
        }

        public bool ExecGrpWihNoDescription(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ListGroupWithoutDescription));
            t.Start();
            return true;
        }

        public bool ExecTblWidth(CommandData cmdData)
        {

            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTablesWidth));
            t.Start();
            return true;
        }

        public bool ExecListTables(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();

            Thread t = new Thread(new ThreadStart(TablesHelper.ListTables));
            t.Start();
            return true;
        }


        public bool ExecTblTableTransaction(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTableTransaction));
            t.Start();
            return true;
        }

        public bool ExecTblGenerateSimpleTransactionFromNotGeneratedTransactions(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.GenterateSimpleTransactionFromNotGeneratedTransaction));
            t.Start();
            return true;
        }

        public bool ExecTblScriptToCompareNULLABLE_GXvsDB(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ScriptToCompareNULLABLE_GXvsDB));
            t.Start();
            return true;
        }

        public bool ExecTblScriptToCompareNULLABLE_GXvsDB2(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ScriptToCompareNULLABLE_GXvsDB2));
            t.Start();
            return true;
        }

        public bool ExecTblListTableWithAttributeNullableCompatible(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTableWithAttributeNullableCompatible));
            t.Start();
            return true;
        }

        /// <summary>
        /// Lista la relacion entre tablas y objetos y muestra en cuales se update/delete/insert/select
        /// </summary>
        /// <param name="cmdData"></param>
        /// <returns></returns>
        public bool ExecTblTableUpdate(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTableUpdate));
            t.Start();
            return true;
        }

        public bool ExecGenerateTrnFromTables(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            TablesHelper.GenerateTrnFromTables();
            return true;
        }

        public bool ExecGenerateTrnFromTables2(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            TablesHelper.GenerateTrnFromTables2();
            return true;
        }

        /// <summary>
        /// Lista las tablas de los modulos.
        /// </summary>
        /// <param name="cmdData"></param>
        /// <returns></returns>
        public bool ExecListTablesInModules(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.ListTableInModules();
            return true;
        }

        /// <summary>
        /// Lista las objetos con tablas en modulos diferentes al suyo.
        /// </summary>
        /// <param name="cmdData"></param>
        /// <returns></returns>
        public bool ExecListObjectsWithTableInOtherModule(CommandData cmdData)
        {

            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.ListObjectsWithTableInOtherModule();
            return true;
        }


        public bool ExecModuleDependencies(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.ModuleDependencies();
            return true;
        }

        public bool ExecTblTableInsertNew(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTableInsertNew));
            t.Start();
            return true;
        }
        #endregion

        public bool ExecRenameAttributesAndTables(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(CleanKBHelper.RenameAttributesAndTables));
            t.Start();
            return true;
        }


        #region Acciones sobre tablas
        public bool ExecAssignDescriptionToTable(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            TablesHelper.AssignDescriptionToTable(cmdData.Parameters);
            return true;
        }

        public bool ExecApplyTableText(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            TablesHelper.ApplyTableText(cmdData.Parameters);
            return true;
        }
        #endregion

        #region Indices
        public bool ExecIndexWithNotRefAtt(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(ObjectsHelper.IndexWithNotRefAtt));
            t.Start();
            return true;
        }

        public bool ExecRremoveIndexAttribute(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.RemoveIndexAttribute(cmdData.Parameters);
            return true;
        }
        #endregion

        #region Objetos

        public bool ExecPreprocessPendingObjects(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread thread = new Thread(() => PreprocessPendingObjects(cmdData));
            thread.Start();
            return true;
        }

        public bool ExecParametersTypeComparer(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ParametersTypeComparer();
            return true;
        }

        public bool ExecObjectsWithRuleOld(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsWithRuleOld();
            return true;
        }


        private static void ObjectsWithRuleOld()
        {
            List<KBObject> objs = KBDoctorSelection.SelectProcedureWebPanelTransaction();
            int cant = 0;
            string recommendations = "";
            Thread thread = new Thread(() => ObjectsWithRuleOld(UIServices.KB.CurrentKB, objs, ref recommendations ,out cant));
            thread.Start();
        }

        private static void ObjectsWithRuleOld(KnowledgeBase KB, List<KBObject> objs, ref string recommendations, out int cant)
        {
            cant = 0;
            KBDoctorOutput.StartSection("KBDoctor - Objects With Rule Old");
           // string recommendations = "";
            ObjectsWithRuleOld(UIServices.KB.CurrentKB, objs, ref recommendations, out cant);
            KBDoctorOutput.EndSection("KBDoctor - Objects With Rule Old");
        }

        private static void ParametersTypeComparer()
        {

            List<KBObject> objs = KBDoctorSelection.SelectProcedureWebPanelTransaction();
            int cant = 0;
            Thread thread = new Thread(() => ParametersTypeComparer2(UIServices.KB.CurrentKB, objs, out cant));
            thread.Start();
        }

        private static void ParametersTypeComparer2(KnowledgeBase KB, List<KBObject> objs, out int cant)
        {
            cant = 0;
            KBDoctorOutput.StartSection("KBDoctor - Parameters Type Comparer");
            string recommendations = "";
           API.ParametersTypeComparer(UIServices.KB.CurrentKB, objs,ref recommendations,  out cant);
            KBDoctorOutput.EndSection("KBDoctor - Parameters Type Comparer");
        }

        public bool ExecEmptyConditionalBlock(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            EmptyConditionalBlock();
            return true;
        }

        public bool ExecNewsWithoutWhenDuplicate(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            NewsWithoutWhenDuplicate();
            return true;
        }

        public bool ExecConstantsInCode(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ConstantsInCode();
            return true;
        }

        public bool ExecReviewCommits(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ReviewCommits();
            return true;
        }

        private static void ConstantsInCode()
        {
            List<KBObject> objs = KBDoctorSelection.SelectProcedureWebPanelTransaction();

            Thread thread = new Thread(() => ConstantsInCode(UIServices.KB.CurrentKB, objs));
            thread.Start();
        }

        private static void ConstantsInCode(KnowledgeBase KB, List<KBObject> objs)
        {
            int cant;
            KBDoctorOutput.StartSection("KBDoctor - Constants in code");
            API.ConstantsInCode(UIServices.KB.CurrentKB, objs, out cant);
            KBDoctorOutput.EndSection("KBDoctor - Constants in code");
        }

        private static void ReviewCommits()
        {
            Thread thread = new Thread(() => ReviewCommits(UIServices.KB.CurrentKB));
            thread.Start();
        }

        private static void ReviewCommits(KnowledgeBase KB)
        {
            bool success;
            string error_message = "";
            try
            {
                KBDoctorOutput.StartSection("KBDoctor - Review commits");
                DateTime FromDate = DateTime.Today.AddDays(-5);
                DateTime ToDate = DateTime.Today;
                string querystring = KBDoctorCore.Sources.Utility.GetQueryStringFromToDate(FromDate, ToDate);
                List<IKBVersionRevision> revisions_list = (List<IKBVersionRevision>)UIServices.TeamDevClient.GetRevisions(KB.DesignModel.KBVersion, querystring, 1);
                Dictionary<string, List<string[]>> review_by_user;
                success = API.ReivewCommits(UIServices.KB.CurrentKB, revisions_list, out review_by_user);


            }
            catch (Exception e)
            {
                success = false;
                error_message = e.Message;
                KBDoctorOutput.InternalError(error_message, e);
            }
            KBDoctorOutput.EndSection("KBDoctor - Review commits", success);
        }

        public bool ExecForEachsWithoutWhenNone(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ForEachsWithoutWhenNone();
            return true;
        }



        private static void EmptyConditionalBlock()
        {
            List<KBObject> objs = KBDoctorSelection.SelectProcedureWebPanelTransaction();

            Thread thread = new Thread(() => EmptyConditionalBlock(UIServices.KB.CurrentKB, objs));
            thread.Start();
        }

        private static void NewsWithoutWhenDuplicate()
        {
            List<KBObject> objs = KBDoctorSelection.SelectProcedureWebPanelTransaction();

            Thread thread = new Thread(() => NewsWithoutWhenDuplicate(UIServices.KB.CurrentKB, objs));
            thread.Start();
        }

        private static void ForEachsWithoutWhenNone()
        {
            List<KBObject> objs = KBDoctorSelection.SelectProcedureWebPanelTransaction();

            Thread thread = new Thread(() => ForEachsWithoutWhenNone(UIServices.KB.CurrentKB, objs));
            thread.Start();
        }



        private static void ForEachsWithoutWhenNone(KnowledgeBase KB, List<KBObject> objs)
        {
            KBDoctorOutput.StartSection("KBDoctor - For eachs without When None");
            API.ForEachsWithoutWhenNone(UIServices.KB.CurrentKB, objs);
            KBDoctorOutput.EndSection("KBDoctor - For eachs without When None");
        }

        private static void NewsWithoutWhenDuplicate(KnowledgeBase KB, List<KBObject> objs)
        {
            KBDoctorOutput.StartSection("KBDoctor - News without When Duplicate");
            API.NewsWithoutWhenDuplicate(UIServices.KB.CurrentKB, objs);
            KBDoctorOutput.EndSection("KBDoctor - News without When Duplicate");
        }

        private static void EmptyConditionalBlock(KnowledgeBase KB, List<KBObject> objs)
        {
            int cant;
            KBDoctorOutput.StartSection("KBDoctor - Verify empty conditional blocks");
            string recommendations = "";
            API.EmptyConditionalBlocks(UIServices.KB.CurrentKB, objs, ref recommendations, out cant);
            KBDoctorOutput.EndSection("KBDoctor - Verify empty conditionals blocks");
        }

        private static void PreprocessPendingObjects(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            SelectedRowsCollection selrows = cmdData.Context as SelectedRowsCollection;
            List<KBObjectHistory> kbohList = GetGenericHistoryObjects(selrows);
            IKBService kbserv = UIServices.KB;


            string title = "KBDoctor - Review Objects";
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Problems", "Technical Debt (min)" });
                List<string[]> lineswriter = new List<string[]>(); ;


                KBModel model = UIServices.KB.CurrentModel;
                List<KBObject> selectedObjects = new List<KBObject>();
                foreach (KBObjectHistory kboh in kbohList)
                {
                    KBObject obj = model.Objects.Get(kboh.Key);
                    if (Utility.IsUserEditableObject(obj))
                    {
                        List<KBObject> objsInContainer = new List<KBObject>();
                        if (obj is Artech.Architecture.Common.Objects.Module)
                        {
                            objsInContainer = KBDoctorCore.Sources.Utility.ModuleObjects((Artech.Architecture.Common.Objects.Module)obj);
                        }
                        else
                        {
                            if (obj is Folder)
                            {
                                objsInContainer = KBDoctorCore.Sources.Utility.FolderObjects((Folder)obj);
                            }
                            else
                            {
                                selectedObjects.Add(obj);
                            }
                        }
                        foreach (KBObject objSelected in objsInContainer)
                        {
                            if (!selectedObjects.Contains(objSelected))
                            {
                                selectedObjects.Add(objSelected);
                            }
                        }
                    }
                }
                double cant = 0;
                KBDoctorCore.Sources.API.PreProcessPendingObjects(UIServices.KB.CurrentKB, output, selectedObjects, out lineswriter, out cant);
                foreach (string[] line in lineswriter)
                {
                    writer.AddTableData(line);
                }
                writer.AddFooter();
                writer.Close();
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public bool ExecReviewObjects(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ReviewObjects();
            return true;
        }

        private static void ReviewObjects()
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = kbserv.CurrentModel;



            string title = "KBDoctor - Review Objects";
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Problems", "Technical debt (min)" });

                List<KBObject> selectedObjects = new List<KBObject>();

                foreach (KBObject obj in Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)))
                {
                    if (obj != null)
                    {
                        selectedObjects.Add(obj);
                    }
                }
                Thread thread = new Thread(() => ExecuteReviewAndShowResults(output, title, outputFile, writer, selectedObjects));
                thread.Start();
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        private static void ExecuteReviewAndShowResults(IOutputService output, string title, string outputFile, KBDoctorXMLWriter writer, List<KBObject> selectedObjects)
        {
            double cant = 0;
            double cantSum = 0;
            List<string[]> lineswriter = new List<string[]>(); ;
            API.PreProcessPendingObjects(UIServices.KB.CurrentKB, output, selectedObjects, out lineswriter, out cant);
            foreach (string[] line in lineswriter)
            {
                writer.AddTableData(line);
                cantSum += cant;
            }
            writer.AddTableData(new string[] { "Technical debt (min) Total:", "", cantSum.ToString() });
            writer.AddFooter();
            writer.Close();
            bool success = true;
            KBDoctorOutput.EndSection(title, success);
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
        }

        public bool ExecCheckBldObjects(CommandData cmdData)
        {
            Thread thread = new Thread(() => CheckBldObject());
            thread.Start();

            return true;
        }

        private static void CheckBldObject()
        {
            API.CheckBldObjects(UIServices.KB.CurrentKB);
        }

        public bool ExecReviewObject(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread thread = new Thread(() => ReviewObject(cmdData));
            thread.Start();
            return true;
        }

        public bool ExecAttributeAsOutput(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            string title = "KBDoctor - Get Objects With Attribute/Domain as Output";
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Attribute>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Domain>());
            selectObjectOption.MultipleSelection = true;
            List<KBObject> objs = Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)).ToList();
            Thread thread = new Thread(() => AttributeAsOutput(objs, title));
            thread.Start();
            return true;
        }

        private static void AttributeAsOutput(List<KBObject> objs, string title)
        {
            string outputFile = Utility.CreateOutputFile(UIServices.KB, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Object", "Description", "Parm Rule" });
            List<string[]> output_list;
            API.AttributeAsOutput(UIServices.KB.CurrentKB, objs, out output_list);

            foreach (string[] line in output_list)
            {
                writer.AddTableData(line);
            }
            writer.AddFooter();
            writer.Close();

            //KBDoctorHelper.ShowKBDoctorResults(outputFile);

        }

        public bool ExecGenerateSDTDataLoad(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<SDT>());
            selectObjectOption.MultipleSelection = true;
            List<KBObject> objs = Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)).ToList();
            Thread thread = new Thread(() => GenerateSDTDataLoad(objs));
            thread.Start();
            return true;
        }

        private static void GenerateSDTDataLoad(List<KBObject> objs)
        {
            API.GenerateSDTDataLoad(UIServices.KB.CurrentKB, objs);
        }

        public bool ExecSDTsWithDateInWS(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.MultipleSelection = true;
            List<KBObject> objs = Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)).ToList();
            Thread thread = new Thread(() => SDTsWithDateInWS(objs));
            thread.Start();
            return true;
        }

        private static void SDTsWithDateInWS(List<KBObject> objs)
        {
            API.ListSDTWithDateInWS(UIServices.KB.CurrentKB, objs);
        }

        public bool ExecGenerateRESTCalls(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.MultipleSelection = true;
            List<KBObject> objs = Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)).ToList();
            Thread thread = new Thread(() => GenerateRESTCalls(objs));
            thread.Start();
            return true;
        }

        public bool ExeCheckVariableUsages(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();

            List<KBObject> objs = KBDoctorSelection.SelectProcedureWebPanelTransaction();
            CheckVariableUsages(objs);
            return true;
        }

        public void CheckVariableUsages(List<KBObject> objs)
         {

             IOutputService output = KBDoctorHelper.SelectOutput();
             string recommendations = "";
             int cant;
             Thread thread = new Thread(() => KBDoctorCore.Sources.API.CheckVariableUsages(UIServices.KB.CurrentKB, objs, ref recommendations, out cant));
             thread.Start();

         }

        public bool ExecVariablesNotBasedOnAttributes(CommandData cmddata)
        {

            IOutputService output = KBDoctorHelper.SelectOutput();

            List<KBObject> objs = KBDoctorSelection.SelectProcedureWebPanelTransaction();
            CheckVariablesNotBasedOnAttributes(objs);
            return true;
        }

        public void CheckVariablesNotBasedOnAttributes(List<KBObject> objs)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();


            Thread thread = new Thread(() => VariablesNotBasedOnAttributes(objs));
            thread.Start();

        }

        private void VariablesNotBasedOnAttributes(List<KBObject> objs)
        {
            List<string[]> lines;
            int cant;
            string title = "KBDoctor - Variables not based on attributes with the same name";
            string outputFile = Utility.CreateOutputFile(UIServices.KB, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Object", "Var/Att Name", "Variable Type", "Attribute Type", "Variable Domain", "Attribute Domain" });
            API.VariablesNotBasedOnAttributes(UIServices.KB.CurrentKB, objs, out lines, out cant);
            foreach(string[] line in lines)
            {
                writer.AddTableData(line);
            }
            writer.AddFooter();
            writer.Close();
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
        }




            private static void GenerateRESTCalls(List<KBObject> objs)
        {
            API.GenerateRESTCalls(UIServices.KB.CurrentKB, objs);
        }

        private static void ReviewObject(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            string title = "KBDoctor - Review Objects";
            IKBService kbserv = UIServices.KB;
            try
            {
                double cant = 0;
                double cantSum = 0;
                string outputFile = Utility.CreateOutputFile(kbserv, title);
                List<string[]> lineswriter = new List<string[]>();
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Problems", "Technical Debt (min)" });
                List<KBObject> selectedObjects = GetObjects(cmdData);
                KBDoctorCore.Sources.API.PreProcessPendingObjects(UIServices.KB.CurrentKB, output, selectedObjects, out lineswriter, out cant);
                foreach (string[] line in lineswriter)
                {
                    writer.AddTableData(line);
                    cantSum += cant;
                }
                writer.AddTableData(new string[] { "Technical debt (min) Total:", "", cantSum.ToString() });
                writer.AddFooter();
                writer.Close();

                bool success = true;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public bool ExecReviewModuleOrFolder(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread thread = new Thread(() => ReviewModuleOrFolder(cmdData));
            thread.Start();
            return true;
        }

        private static void ReviewModuleOrFolder(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Review Objects";
            output.SelectOutput("KBDoctor");
            List<KBObject> selectedModulesFolders = GetObjects(cmdData);
            List<KBObject> selectedObjects = new List<KBObject>();

            List<string[]> lineswriter = new List<string[]>();

            IKBService kbserv = UIServices.KB;
            try
            {
                double cant = 0;
                string outputFile = Utility.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Problems", "Technical Debt (min)" });
                foreach (KBObject obj in selectedModulesFolders)
                {
                    List<KBObject> objsInContainer = new List<KBObject>();
                    if (obj is Artech.Architecture.Common.Objects.Module)
                    {
                        objsInContainer = KBDoctorCore.Sources.Utility.ModuleObjects((Artech.Architecture.Common.Objects.Module)obj);
                    }
                    else
                    {
                        if (obj is Folder)
                        {
                            objsInContainer = KBDoctorCore.Sources.Utility.FolderObjects((Folder)obj);
                        }
                    }
                    foreach (KBObject objSelected in objsInContainer)
                    {
                        if (!selectedObjects.Contains(objSelected))
                        {
                            selectedObjects.Add(objSelected);
                        }
                    }
                }

                double cantSum = 0;
                KBDoctorCore.Sources.API.PreProcessPendingObjects(UIServices.KB.CurrentKB, output, selectedObjects, out lineswriter, out cant);
                foreach (string[] line in lineswriter)
                {
                    writer.AddTableData(line);
                    cantSum += cant;
                }
                writer.AddTableData(new string[] { "Technical debt (min) Total:", "", cantSum.ToString() });
                writer.AddFooter();
                writer.Close();
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static List<KBObjectHistory> GetGenericHistoryObjects(SelectedRowsCollection rows)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            return (from UltraGridRow row in rows
                    where !row.IsGroupByRow // Quitamos las rows de grupo.
                    select (KBObjectHistory)row.Cells["KBObjectHistory"].Value).ToList();
        }

        public bool ExecObjNotReacheable(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.Unreachables));
            t.Start();
            return true;
        }

        public bool ExecObjectsWithoutInOut(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ParmWOInOut));
            t.Start();
            return true;
        }

        public bool ExecObjectsMainsCalled(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsMainsCalled));
            t.Start();
            return true;
        }

        public bool ExecObjectsReferenced(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            CleanKBHelper.ObjectsReferenced();
            return true;
        }

        public bool ExecObjectsWithVarNotBasedOnAtt(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsWithVarNotBasedOnAtt));
            t.Start();
            return true;
        }
        public bool ExecListDynamicCombo(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(BadSmells.ListDynamicCombo));
            t.Start();
            return true;
        }

        public bool ExecListProperties(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(BadSmells.ListNamespace));
            t.Start();
            return true;
        }

        public bool ExecBuildModule(CommandData cmdData)
        {
           IOutputService output = KBDoctorHelper.SelectOutput();
                // Hace el Build with this only de los objetos de un modulo y de los que lo referencian
           ModulesHelper.BuildModule();
            return true;
        }

  /*      public bool ExecBuildModuleContext(CommandData cmdData)
        {
            //Hace el Build with this only de los objetos de un modulo y de los que lo referencian
            //ModulesHelper.BuildModuleContext(cmdData);
            return true;
        } */

        public bool ExecBuildObjectAndReferences(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            //Hace el Build with this only de los objetos de un modulo y de los que lo referencian
            ObjectsHelper.BuildObjectAndReferences();
            return true;
        }

        public bool ExecBuildObjectWithProperty(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            //Hace el Build with this only de los objetos de un modulo y de los que lo referencian
            ObjectsHelper.BuildObjectWithProperty();
            return true;
        }

        public bool ExecRenameVariables(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(CleanKBHelper.RenameVariables));
            t.Start();
            return true;
        }

        public bool ExecProceduresThatUpdatesAttributes(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsUpdatingAttributes));
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            return true;
        }

        public bool ExecSelectObjectsUpdateAttribute(CommandData cmdData)
        {
            ObjectsHelper.SelectObjectsUpdatingAttributes();
            return true;
        }

        public bool ExecApplyObjectsUpdateAttribute(CommandData cmdData)
        {
            ObjectsHelper.ApplyObjectsUpdatingAttributes(cmdData.Parameters);
            return true;
        }

        public bool ExecObjectsNotCalled(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjetNotCalled));
            t.Start();
            return true;
        }

        public bool ExecObjectsWithCommitOnExit(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsWithParmAndCommitOnExit));
            t.Start();
            return true;
        }

        public bool ExecListCommitOnExit(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ListCommitOnExit));
            t.Start();
            return true;
        }

        public bool ExecObjectsWithVarsNotUsed(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsWithVarsNotUsed));
            t.Start();
            return true;
        }

        public bool ExecResetWINForm(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ResetWINForm));
            t.Start();
            return true;
        }

        public bool ExecWebFormToAbstractEditor(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ResponsiveSmoothActions.Run("webFormToAbstractEditor");
            return true;
        }

        public bool ExecImproveControlClasses(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            UIFrontendActions.ImproveControlClasses();
            return true;
        }

        public bool ExecAuditWebPanelsSmoothUX(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ResponsiveSmoothActions.Run("auditWebPanelsSmoothUX");
            return true;
        }

        public bool ExecConvertSafeWebPanelsToSmoothUX(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ResponsiveSmoothActions.Run("convertSafeWebPanelsToSmoothUX");
            return true;
        }

        public bool ExecRunResponsiveSmoothAction(CommandData cmdData)
        {
            ObjectsHelper.RunResponsiveSmoothAction(cmdData.Parameters);
            return true;
        }

        public bool ExecListProcedureCallWebpanelTransaction(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ListProcedureCallWebPanelTransaction));
            t.Start();
            return true;
        }

        public bool ExecObjectMigration(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectMigration));
            t.Start();
            return true;
        }

        public bool ExecObjectsLegacyCode(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsLegacyCode));
            t.Start();
            return true;
        }

        public bool ExecEditLegacyCodeToReplace(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.EditLegacyCodeToReplace();
            return true;
        }

        public bool ExecEditReviewObjects(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.EditReviewObjects();
            return true;
        }


        public bool ExecChangeLegacyCode(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.ChangeLegacyCode();
            return true;
        }

        public bool ExecChangeCommitOnExit(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.ChangeCommitOnExit();
            return true;
        }

        public bool ExecTreeCommit(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.TreeCommit();
            return true;
        }

        public bool ExecObjectsRefactoringCandidates(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            //  Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsRefactoringCandidates));
            Thread t = new Thread(new ThreadStart(ObjectsHelper.GenerateCSV_ObjectsRefactoring));
            t.Start();
            return true;
        }

        public bool ExecCountTableAccess(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.CountTableAccess));
            t.Start();
            return true;
        }

        public bool ExecObjectsWithConstants(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            //Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsWithConstants));
            ObjectsHelper.ObjectsWithConstants();
            return true;
        }

        public bool ExecUDPCallables(CommandData cmdData)
        {

            return true;
            /*
             * Comento este procedimiento para usarlo como opción nueva.
             * ObjectsHelper.ObjectsUDPCallables();
            return true;*/
        }

        public bool ExecFixVariablesNotBasedInAttributesOrDomain(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.FixVariablesNotBasedInAttributesOrDomain();
            return true;
        }

        public bool ExecObjectsWINWEB(CommandData cmdData)
        {

            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(KbStats.ObjectsWINWEB));
            t.Start();
            return true;
        }

        public bool ExecMainTableUsed(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.MainTableUsed));
            t.Start();
            return true;
        }
        public bool ExecRemovableTransactions(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(ObjectsHelper.RemovableTransactions));
            t.Start();
            return true;
        }

        public bool ExecGenerateSQLScripts(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            TablesHelper.GenerateSQLScripts();
            return true;
        }

        public bool ExecGenerateDPfromTable(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            TablesHelper.GenerateTrnFromTables2();
            return true;
        }
        public bool ExecGenerateGraph(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(GraphHelper.GenerateGraph));
            t.Start();
            return true;
        }

        public bool ExecKBInterfaces(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            // Lista objetos complejos.
            Thread t = new Thread(new ThreadStart(KbStats.KBInterfaces));
            t.Start();
            return true;
        }
        #endregion



        #region Acciones sobre objetos
        public bool ExecRemoveObject(CommandData cmdData)
        {
            ObjectsHelper.RemoveObject(cmdData.Parameters);
            return true;
        }

        public bool ExecRemoveUnreferencedObjectsInUserModules(CommandData cmdData)
        {
            ObjectsHelper.RemoveUnreferencedObjectsInUserModules();
            return true;
        }

        public bool ExecOpenKBObject(CommandData cmdData)
        {
            ObjectsHelper.OpenObject(cmdData.Parameters);

            return true;
        }

        public bool ExecSetObjectPropertyText(CommandData cmdData)
        {
            ObjectsHelper.SetObjectPropertyText(cmdData.Parameters);
            return true;
        }

        public bool ExecApplyObjectPropertyText(CommandData cmdData)
        {
            ObjectsHelper.ApplyObjectPropertyText(cmdData.Parameters);
            return true;
        }

        public bool ExecAssignAttributeToVariable(CommandData cmdData)
        {
            ObjectsHelper.AssignAttributeToVariable(cmdData.Parameters);
            return true;
        }

        public bool ExecAssignAttributeOrDomainToVariable(CommandData cmdData)
        {
            ObjectsHelper.AssignAttributeOrDomainToVariable(cmdData.Parameters);
            return true;
        }

        public bool ExecAssignDomainToVariable(CommandData cmdData)
        {
            ObjectsHelper.AssignDomainToVariable(cmdData.Parameters);
            return true;
        }

        public bool ExecCleanVarsNotUsed(CommandData cmdData)
        {
            ObjectsHelper.CleanVarsNotUsed();
            return true;
        }

        public bool ExecThemeClassesNotUsed(CommandData cmdData)
        {
            return ObjectsHelper.ThemeClassesNotUsed();
        }

        public bool ExecObjThemeClassesNotUsed(CommandData cmdData)
        {
            return ObjectsHelper.ObjThemeClassesNotUsed();
        }
        #endregion

        #region Acerca de
        public bool ExecAboutKBDoctor(CommandData cmdData)
        {
            Assembly assem = this.GetType().Assembly;
            KBDoctorWebForms.ShowAbout(assem);
            return true;
        }

        public bool ExecHelpKBDoctor(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            KBDoctorHelper.ShowKBDoctorResults("http://wiki.genexus.com/commwiki/servlet/hwikibypageid?26679");
            return true;
        }
        #endregion
        #region Limpieza KB
        public bool ExecCleanKB(CommandData cmdData)
        {

            CleanKBHelper.CleanKBAsMuchAsPossible();
            return true;
        }

        public bool ExecCleanObjects(CommandData cmdData)
        {
            KnowledgeBase kbModel = UIServices.KB.CurrentKB;
            IOutputService output = CommonServices.Output;


            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;

            List<KBObject> selectedObjects = new List<KBObject>();

            API.CleanKBObjects(kbModel, UIServices.SelectObjectDialog.SelectObjects(selectObjectOption), output);
            return true;
        }

        public bool ExecAddINParmRule(CommandData cmdData)
        {

            CleanKBHelper.AddINParmRule();
            return true;
        }

        public bool ExecListTableAttributesUsingDomain(CommandData cmdData)
        {

            CleanKBHelper.ListTableAttributesUsingDomain();
            return true;
        }
        public bool ExecProcedureSDT(CommandData cmdData)
        {

            CleanKBHelper.CreateProcedureSDT();
            return true;
        }

        public bool ExecProcedureGetSet(CommandData cmdData)
        {

            CodeGeneration.CreateProcedureGetSet();
            return true;
        }


        public bool ExecPrepareComparerNavigations(CommandData cmdData)
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            NavigationHelper.PrepareComparerNavigations(kbserv.CurrentKB,output);
            return true;
        }

        public bool ExecOpenFolderComparerNavigation(CommandData cmdData)
        {
            KbStats.OpenFolderComparerNavigation();
            return true;
        }

        public bool ExecOpenFolderObjComparerNavigation(CommandData cmdData)
        {
            KbStats.OpenFolderObjComparerNavigation();
            return true;
        }

        public bool ExecCompareLastNVGDirectory(CommandData cmdData)
        {
            KbStats.CompareLastNVGDirectory();
            return true;
        }

        public bool ExecCompareLastOBJDirectory(CommandData cmdData)
        {
            KbStats.CompareLastOBJDirectory();
            return true;
        }
        public bool ExecSearchAndReplace(CommandData cmdData)
        {
            CleanKBHelper.SearchAndReplace();
            return true;
        }

        public bool ExecApplySearchAndReplace(CommandData cmdData)
        {
            CleanKBHelper.ApplySearchAndReplace(cmdData.Parameters);
            return true;
        }

        public bool ExecClassNotInTheme(CommandData cmdData)
        {
            ThemeHelper.ClassNotInTheme();
            return true;
        }

        public bool ExecClassUsed(CommandData cmdData)
        {
            ThemeHelper.ClassUsed();
            return true;
        }

        #endregion

        public bool ExecCountGeneratedByPattern(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            KbStats.CountGeneratedByPattern();
            return true;
        }
        public bool ExecGeneratedByPatternWithoutDynamism(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            KbStats.GeneratedByPatternWithoutDynamism();
            return true;
        }

        public bool ExecReplaceNullCompatible(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            AttributesHelper.ReplaceNullsCompatible();
            return true;
        }

        public bool ExecListObj(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(KbStats.ListObj));
            t.Start();
            return true;
        }

        public bool ExecListUnreferencedObjectsInUserModules(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            Thread t = new Thread(new ThreadStart(KbStats.ListUnreferencedObjectsInUserModules));
            t.Start();
            return true;
        }

        public bool ExecCreateDeployUnits(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.CreateDeployUnits();
            return true;
        }

        public bool ExecMarkPublicObjects(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.MarkPublicObjects();
            return true;
        }

        public bool ExecListModules(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.ListModulesErrors();
            return true;
        }

        public bool ExecAssignTypeComparer(CommandData cmdData)
        {
            ObjectsHelper.AssignTypesComparer();
            return true;
        }
        public bool ExecListModulesStatistics(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.ListModulesStatistics();
            return true;
        }

        public bool ExecListModularizationQuality(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.ListModularizationQuality();
            return true;
        }

        public bool ExecAddModularizationInfo(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.AddModularizationInformationToObjects();
            return true;
        }

        public bool ExecMoveTransactions(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.MoveTransactions();
            return true;
        }

        public bool ExecCalculateCheckSum(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.CalculateCheckSum();
            return true;
        }

        public bool ExecGenerateLocationXML(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.GenerateLocationXML();
            return true;
        }

        public bool ExecListLastReports(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            KbStats.ListLastReports();
            return true;
        }

        public bool ExecListObjWarningsErrors(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            NavigationHelper.ListObjWarningsErrors();
            return true;
        }

        public bool ExecListObjSimilarNavigation(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            NavigationHelper.ListObjSimilarNavigation();
            return true;
        }

        public bool ExecListAPIObjects(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.ListAPIObjects();
            return true;
        }
        public bool ExecRecomendedModule(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.RecomendedModule();
            return true;
        }

        public bool ExecApplyExternalModularization(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ModulesHelper.ApplyExternalModularization();
            return true;
        }
        public bool ExecSplitMainObject(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.SplitMainObject();
            return true;
        }

        public bool ExecListWebObjectsProperties(CommandData cmdData)
        {
            IOutputService output = KBDoctorHelper.SelectOutput();
            ObjectsHelper.ListWebObjectsProperties();
            return true;
        }

        #region QueryKBDoctor
        private bool QueryKBDoctor(CommandData cmdData, ref CommandStatus status)
        {
            // This is where you have a chance to modify the status of
            // menu / toolbar items.
            status.State = CommandState.Disabled;

            IKBService kbserv = UIServices.KB;
            if (kbserv != null && kbserv.CurrentKB != null)
            {
                status.State = CommandState.Enabled;
            }

            // return true to indicate you already resolved the command status;
            // otherwise the framework will try with its next registered
            // command target
            return true;
        }

        private bool QueryKBDoctorNoKB(CommandData cmdData, ref CommandStatus status)
        {
            status.State = CommandState.Enabled;

            return true;
        }

        private bool QueryFolderOrObjectSelected(CommandData commandData, ref CommandStatus status)
        {
            status.State = CommandState.Enabled;
            this.QueryKBOpened(commandData, ref status);
            if (status.State == CommandState.Enabled)
            {
                this.QueryFolderSelected(commandData, ref status);
                if (status.State != CommandState.Enabled)
                {
                    this.QueryIsKBObjectSelected(commandData, ref status);
                }
            }
            return true;
        }

        private bool QueryKBOpened(CommandData commandData, ref CommandStatus status)
        {
            status.State = CommandState.Disabled;
            if (UIServices.KB != null && UIServices.KB.CurrentKB != null)
            {
                status.State = CommandState.Enabled;
            }
            return true;
        }

        private bool QueryFolderSelected(CommandData commandData, ref CommandStatus status)
        {
            bool result;
            try
            {
                this.QueryKBOpened(commandData, ref status);
                if (status.State == CommandState.Enabled)
                {
                    status.State = CommandState.Invisible;
                    ISelectionContainer selectionContainer = commandData.Context as ISelectionContainer;
                    if (selectionContainer == null || selectionContainer.SelectedObjects == null || !(selectionContainer.SelectedObject is KBObject))
                    {
                        result = true;
                        return result;
                    }
                    foreach (KBObject kBObject in selectionContainer.SelectedObjects)
                    {
                        if (kBObject.Type != typeof(Folder).GUID)
                        {
                            result = true;
                            return result;
                        }
                    }
                    status.State = CommandState.Enabled;
                }
            }
            catch
            {
                status.State = CommandState.Invisible;
            }
            result = true;
            return result;
        }

        private bool QueryIsKBObjectSelected(CommandData data, ref CommandStatus status)
        {
            this.QueryKBOpened(data, ref status);
            bool result;
            if (status.State == CommandState.Enabled)
            {
                status.State = CommandState.Invisible;
                List<KBObject> objects = CommandManager.GetObjects(data);
                if (objects.Count == 0)
                {
                    result = true;
                    return result;
                }
                foreach (KBObject current in objects)
                {
                    if (current == null || !CommandManager.CanReviewKBObject(current))
                    {
                        result = true;
                        return result;
                    }
                }
                status.State = CommandState.Enabled;
            }
            result = true;
            return result;
        }

        private static bool CanReviewKBObject(KBObject kbObj)
        {
            return kbObj.Type == typeof(Artech.Genexus.Common.Objects.Transaction).GUID || kbObj.Type == typeof(WebPanel).GUID || kbObj.Type == typeof(Procedure).GUID || kbObj.Type == typeof(DataProvider).GUID || kbObj.Type == typeof(WorkPanel).GUID || kbObj.Type == typeof(SDT).GUID;
        }

        private static List<KBObject> GetObjects(CommandData data)
        {
            List<KBObject> list = new List<KBObject>();
            ISelectionContainer selectionContainer = data.Context as ISelectionContainer;
            if (selectionContainer != null)
            {
                if (selectionContainer.SelectedObjects != null)
                {
                    foreach (object current in selectionContainer.SelectedObjects)
                    {
                        KBObject obj = current as KBObject;
                        if (Utility.IsUserEditableObject(obj))
                        {
                            list.Add(obj);
                        }
                    }
                }
            }
            else
            {
                KBObject obj = data.Context as KBObject;
                if (Utility.IsUserEditableObject(obj))
                {
                    list.Add(obj);
                }
            }
            return list;
        }

        private bool QueryIsModuleSelected(CommandData data, ref CommandStatus status)
        {
            status.State = CommandState.Invisible;
            if (UIServices.KB != null && UIServices.KB.CurrentKB != null)
            {
                IModelTree tree = data.Context as IModelTree;
                if (tree == null || !(tree.SelectedObject is KBObject))
                    return true;

                foreach (KBObject obj in tree.SelectedObjects)
                    if (obj.Type != typeof(Artech.Architecture.Common.Objects.Module).GUID)
                        return true;

                status.State = CommandState.Enabled;
            }
            return true;
        }

        private bool QueryIsModuleOrFolderSelected(CommandData data, ref CommandStatus status)
        {
            status.State = CommandState.Enabled;
            this.QueryKBOpened(data, ref status);
            if (status.State == CommandState.Enabled)
            {
                this.QueryFolderSelected(data, ref status);
                if (status.State != CommandState.Enabled)
                {
                    this.QueryIsModuleSelected(data, ref status);
                }
            }
            return true;
        }


        #endregion


    }
}
