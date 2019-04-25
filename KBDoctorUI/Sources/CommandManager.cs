using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using System.Windows.Forms;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Common.Framework.Selection;
using Artech.Architecture.UI.Framework.Controls;
using Concepto.Packages.KBDoctorCore;
using Infragistics.Win.UltraWinGrid;
using static Artech.Architecture.Common.Objects.KnowledgeBase;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Common.Descriptors;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    class CommandManager : CommandDelegator
    {
        public CommandManager()
        {
            // Atributos
            AddCommand(CommandKeys.AttWithNoDomain, new ExecHandler(ExecAttWithNoDomain), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListAttributes, new ExecHandler(ExecListAttributes), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttWithoutDescription, new ExecHandler(ExecAttWithoutDescription), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttCharToVarchar, new ExecHandler(ExecAttCharToVarchar), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttVarcharToChar, new ExecHandler(ExecAttVarcharToChar), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttKeyVarchar, new ExecHandler(ExecAttKeyVarchar), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttDescWithoutUniqueIndex, new ExecHandler(ExecAttDescWithoutUniqueIndex), new QueryHandler(QueryKBDoctor));
        //    AddCommand(CommandKeys.AttNotReferenced, new ExecHandler(ExecAttNotReferenced), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttWithoutBaseTable, new ExecHandler(ExecAttWithoutBaseTable), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttInOneTrnOnly, new ExecHandler(ExecAttInOneTrnOnly), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttFormula, new ExecHandler(ExecAttFormula), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ReplaceDomain, new ExecHandler(ExecReplaceDomain), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListDomain, new ExecHandler(ExecListDomain), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AttUpdated, new ExecHandler(ExecAttUpdated), new QueryHandler(QueryKBDoctor));
            // Acciones sobre atributos
            AddCommand(CommandKeys.AssignDomainToAttribute, new ExecHandler(ExecAssignDomainToAttribute), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AssignDescriptionToAttribute, new ExecHandler(ExecAssignDescriptionToAttribute), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AssignTitleToAttribute, new ExecHandler(ExecAssignTitleToAttribute), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AssignColumnTitleToAttribute, new ExecHandler(ExecAssignColumnTitleToAttribute), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AddDescriptorIndex, new ExecHandler(ExecAddDescriptorIndex), new QueryHandler(QueryKBDoctor));
            // Tablas
            AddCommand(CommandKeys.TblWihNoDescription, new ExecHandler(ExecTblWihNoDescription), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.GrpWihNoDescription, new ExecHandler(ExecGrpWihNoDescription), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.TblWidth, new ExecHandler(ExecTblWidth), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListTables, new ExecHandler(ExecListTables), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.TblTableTransaction, new ExecHandler(ExecTblTableTransaction), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.TblGenerateSimpleTransactionFromNotGeneratedTransactions, new ExecHandler(ExecTblGenerateSimpleTransactionFromNotGeneratedTransactions), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.TblTableUpdate, new ExecHandler(ExecTblTableUpdate), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.TblTableInsertNew, new ExecHandler(ExecTblTableInsertNew), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.GenerateTrnFromTables, new ExecHandler(ExecGenerateTrnFromTables), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.GenerateTrnFromTables2, new ExecHandler(ExecGenerateTrnFromTables2), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListTablesInModules, new ExecHandler(ExecListTablesInModules), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListObjectsWithTableInOtherModule, new ExecHandler(ExecListObjectsWithTableInOtherModule), new QueryHandler(QueryKBDoctor));

            // Acciones sobre tablas
            AddCommand(CommandKeys.AssignDescriptionToTable, new ExecHandler(ExecAssignDescriptionToTable), new QueryHandler(QueryKBDoctor));
            // Indices
            AddCommand(CommandKeys.IndexWithNotRefAtt, new ExecHandler(ExecIndexWithNotRefAtt), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.RemoveIndexAttribute, new ExecHandler(ExecRremoveIndexAttribute), new QueryHandler(QueryKBDoctor));
            // Objetos
            AddCommand(CommandKeys.ObjNotReacheable, new ExecHandler(ExecObjNotReacheable), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ObjectsWithoutInOut, new ExecHandler(ExecObjectsWithoutInOut), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ObjectsMainCalled, new ExecHandler(ExecObjectsMainsCalled), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ObjectsReferenced, new ExecHandler(ExecObjectsReferenced), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ObjectsWithVarNotBasedOnAtt, new ExecHandler(ExecObjectsWithVarNotBasedOnAtt), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListDynamicCombo, new ExecHandler(ExecListDynamicCombo), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.RenameVariables, new ExecHandler(ExecRenameVariables), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.BuildModule, new ExecHandler(ExecBuildModule), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AssignTypeComparer, new ExecHandler(ExecAssignTypeComparer), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ParameterTypeComparer, new ExecHandler(ExecParametersTypeComparer), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.EmptyConditionalBlocks, new ExecHandler(ExecEmptyConditionalBlock), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.NewsWithoutWhenDuplicate, new ExecHandler(ExecNewsWithoutWhenDuplicate), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ForEachsWithoutWhenNone, new ExecHandler(ExecForEachsWithoutWhenNone), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ConstantsInCode, new ExecHandler(ExecConstantsInCode), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ReviewCommits, new ExecHandler(ExecReviewCommits), new QueryHandler(QueryKBDoctor));

            //      AddCommand(CommandKeys.BuildModuleContext, new ExecHandler(ExecBuildModuleContext), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.BuildObjectAndReferences, new ExecHandler(ExecBuildObjectAndReferences), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.BuildObjectWithProperty, new ExecHandler(ExecBuildObjectWithProperty), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.ObjectsNotCalled, new ExecHandler(ExecObjectsNotCalled), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ObjectsWithCommitOnExit, new ExecHandler(ExecObjectsWithCommitOnExit), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ObjectsWithVarsNotUsed, new ExecHandler(ExecObjectsWithVarsNotUsed), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ResetWINForm, new ExecHandler(ExecResetWINForm), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ObjectsComplex, new ExecHandler(ExecObjectsComplex), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.ObjectsUpdateAttribute, new ExecHandler(ExecProceduresThatUpdatesAttributes), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.ChangeCommitOnExit, new ExecHandler(ExecChangeCommitOnExit), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.TreeCommit, new ExecHandler(ExecTreeCommit), new QueryHandler(QueryKBDoctor));
            
            AddCommand(CommandKeys.ObjectsLegacyCode, new ExecHandler(ExecObjectsLegacyCode), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ChangeLegacyCode, new ExecHandler(ExecChangeLegacyCode), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.EditLegacyCodeToReplace, new ExecHandler(ExecEditLegacyCodeToReplace), new QueryHandler(QueryKBDoctor));


            AddCommand(CommandKeys.ObjectsRefactoringCandidates, new ExecHandler(ExecObjectsRefactoringCandidates), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.CountTableAccess, new ExecHandler(ExecCountTableAccess), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.ObjectsWithConstants, new ExecHandler(ExecObjectsWithConstants), new QueryHandler(QueryKBDoctor));
           
            AddCommand(CommandKeys.FixVariablesNotBasedInAttributesOrDomain, new ExecHandler(ExecFixVariablesNotBasedInAttributesOrDomain), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.KBInterfaces, new ExecHandler(ExecKBInterfaces), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ObjectsWINWEB, new ExecHandler(ExecObjectsWINWEB), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListProcedureCallWebpanelTransaction, new ExecHandler(ExecListProcedureCallWebpanelTransaction), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.MainTableUsed, new ExecHandler(ExecMainTableUsed), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.RemovableTransactions, new ExecHandler(ExecRemovableTransactions), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.GenerateSQLScripts, new ExecHandler(ExecGenerateSQLScripts), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.GenerateGraph, new ExecHandler(ExecGenerateGraph), new QueryHandler(QueryKBDoctor));

            // Acciones sobre objetos
            AddCommand(CommandKeys.RemoveObject, new ExecHandler(ExecRemoveObject), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.OpenObject, new ExecHandler(ExecOpenKBObject), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AssignAttributeToVariable, new ExecHandler(ExecAssignAttributeToVariable), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.AssignDomainToVariable, new ExecHandler(ExecAssignDomainToVariable), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.CleanVarsNotUsed, new ExecHandler(ExecCleanVarsNotUsed), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.AddINParmRule, new ExecHandler(ExecAddINParmRule), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListTableAttributesUsingDomain, new ExecHandler(ExecListTableAttributesUsingDomain), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.CleanObjects, new ExecHandler(ExecCleanObjects), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.CleanKBAsMuchAsPossible, new ExecHandler(ExecCleanKB), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ProcedureSDT, new ExecHandler(ExecProcedureSDT), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ProcedureGetSet, new ExecHandler(ExecProcedureGetSet), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.SearchAndReplace, new ExecHandler(ExecSearchAndReplace), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ClassNotInTheme, new ExecHandler(ExecClassNotInTheme), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ClassUsed, new ExecHandler(ExecClassUsed), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListClassUsed, new ExecHandler(ExecClassUsed), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.PrepareComparerNavigation, new ExecHandler(ExecPrepareComparerNavigation), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.OpenFolderComparerNavigation, new ExecHandler(ExecOpenFolderComparerNavigation), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.OpenFolderObjComparerNavigation, new ExecHandler(ExecOpenFolderObjComparerNavigation), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.CompareLastNVGDirectory, new ExecHandler(ExecCompareLastNVGDirectory), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.CompareLastOBJDirectory, new ExecHandler(ExecCompareLastOBJDirectory), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.ListLastReports, new ExecHandler(ExecListLastReports), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.PreprocessPendingObjects, new ExecHandler(ExecPreprocessPendingObjects), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.ReviewObjects, new ExecHandler(ExecReviewObjects), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ReviewModuleOrFolder, new ExecHandler(ExecReviewModuleOrFolder), new QueryHandler(QueryIsModuleOrFolderSelected));
            AddCommand(CommandKeys.ReviewObject, new ExecHandler(ExecReviewObject), new QueryHandler(QueryIsKBObjectSelected));
            AddCommand(CommandKeys.EditReviewObjects, new ExecHandler(ExecEditReviewObjects), new QueryHandler(QueryKBDoctor));


            AddCommand(CommandKeys.AboutKBDoctor, new ExecHandler(ExecAboutKBDoctor), new QueryHandler(QueryKBDoctorNoKB));
            AddCommand(CommandKeys.HelpKBDoctor, new ExecHandler(ExecHelpKBDoctor), new QueryHandler(QueryKBDoctorNoKB));
            AddCommand(CommandKeys.ThemeClassesNotUsed, new ExecHandler(ExecThemeClassesNotUsed), new QueryHandler(QueryKBDoctorNoKB));


            //Labs
            AddCommand(CommandKeys.RenameAttributesAndTables, new ExecHandler(ExecRenameAttributesAndTables), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.CountGeneratedByPattern, new ExecHandler(ExecCountGeneratedByPattern), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ReplaceNullCompatible, new ExecHandler(ExecReplaceNullCompatible), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListObj, new ExecHandler(ExecListObj), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.CreateDeployUnits, new ExecHandler(ExecCreateDeployUnits), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.MarkPublicObjects, new ExecHandler(ExecMarkPublicObjects), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListModules, new ExecHandler(ExecListModules), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListModulesStatistics, new ExecHandler(ExecListModulesStatistics), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListModularizationQuality, new ExecHandler(ExecListModularizationQuality), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.MoveTransactions, new ExecHandler(ExecMoveTransactions), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ModuleDependencies, new ExecHandler(ExecModuleDependencies), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.CalculateCheckSum, new ExecHandler(ExecCalculateCheckSum), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.GenerateLocationXML, new ExecHandler(ExecGenerateLocationXML), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.ListObjWarningsErrors, new ExecHandler(ExecListObjWarningsErrors), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListObjSimilarNavigation, new ExecHandler(ExecListObjSimilarNavigation), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ListAPIObjects, new ExecHandler(ExecListAPIObjects), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.RecomendedModule, new ExecHandler(ExecRecomendedModule), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.ApplyExternalModularization, new ExecHandler(ExecApplyExternalModularization), new QueryHandler(QueryKBDoctor));

            AddCommand(CommandKeys.SplitMainObject, new ExecHandler(ExecSplitMainObject), new QueryHandler(QueryKBDoctor));
            AddCommand(CommandKeys.UDPCallables, new ExecHandler(ExecUDPCallables), new QueryHandler(QueryKBDoctor));

        }

       
        #region Atributos

        public bool ExecAttWithoutDescription(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListAttWithoutDescription));
            t.Start();
            return true;
        }

        public bool ExecAttWithNoDomain(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListWithoutDomain));
            t.Start();
            return true;
        }

        public bool ExecListAttributes(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListAttributes));
            t.Start();
            return true;
        }

        public bool ExecAttInOneTrnOnly(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(AttributesHelper.AttInOneTrnOnly));
            t.Start();
            return true;
        }

        public bool ExecAttCharToVarchar(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListCharToVarchar));
            t.Start();
            return true;
        }

        public bool ExecAttVarcharToChar(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListVarcharToChar));
            t.Start();
            return true;
        }

        public bool ExecAttKeyVarchar(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(AttributesHelper.ListKeyVarchar));
            t.Start();
            return true;
        }

        public bool ExecAttDescWithoutUniqueIndex(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            // Atributo sin tabla. Abrir objetos que referencien a att sin tablas y que permita corregirlo.
            Thread t = new Thread(new ThreadStart(CleanKBHelper.RemoveAttributeWithoutTable));
            t.Start();
            return true;
        }

        public bool ExecAttFormula(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(AttributesHelper.AttFormula));
            t.Start();
            return true;
        }

        public bool ExecReplaceDomain(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            AttributesHelper.ReplaceDomain();
            return true;
        }

        public bool ExecListDomain(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            AttributesHelper.ListDomain();
            return true;
        }

        public bool ExecAttUpdated(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            NavigationHelper.AttUpdated();
            return true;
        }
        #endregion

        #region Acciones sobre atributos
        public bool ExecAssignDomainToAttribute(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            AttributesHelper.AssignDomainToAttribute(cmdData.Parameters);
            return true;
        }

        public bool ExecAssignDescriptionToAttribute(CommandData cmdData)
        {

            AttributesHelper.AssignDescriptionToAttribute(cmdData.Parameters, 0);
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            AttributesHelper.AddDescriptorIndex(cmdData.Parameters);
            return true;
        }

        #endregion

        #region Tablas
        public bool ExecTblWihNoDescription(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(TablesHelper.ListWithoutDescription));
            t.Start();
            return true;
        }

        public bool ExecGrpWihNoDescription(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(TablesHelper.ListGroupWithoutDescription));
            t.Start();
            return true;
        }

        public bool ExecTblWidth(CommandData cmdData)
        {

            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTablesWidth));
            t.Start();
            return true;
        }

        public bool ExecListTables(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");

            Thread t = new Thread(new ThreadStart(TablesHelper.ListTables));
            t.Start();
            return true;
        }


        public bool ExecTblTableTransaction(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTableTransaction));
            t.Start();
            return true;
        }

        public bool ExecTblGenerateSimpleTransactionFromNotGeneratedTransactions(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(TablesHelper.GenterateSimpleTransactionFromNotGeneratedTransaction));
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTableUpdate));
            t.Start();
            return true;
        }

        public bool ExecGenerateTrnFromTables(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            TablesHelper.GenerateTrnFromTables();
            return true;
        }

        public bool ExecGenerateTrnFromTables2(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
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

            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ModulesHelper.ListObjectsWithTableInOtherModule();
            return true;
        }


        public bool ExecModuleDependencies(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ModulesHelper.ModuleDependencies();
            return true;
        }

        public bool ExecTblTableInsertNew(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(TablesHelper.ListTableInsertNew));
            t.Start();
            return true;
        }
        #endregion

        public bool ExecRenameAttributesAndTables(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(CleanKBHelper.RenameAttributesAndTables));
            t.Start();
            return true;
        }


        #region Acciones sobre tablas
        public bool ExecAssignDescriptionToTable(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            TablesHelper.AssignDescriptionToTable(cmdData.Parameters);
            return true;
        }
        #endregion

        #region Indices
        public bool ExecIndexWithNotRefAtt(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(ObjectsHelper.IndexWithNotRefAtt));
            t.Start();
            return true;
        }

        public bool ExecRremoveIndexAttribute(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.RemoveIndexAttribute(cmdData.Parameters);
            return true;
        }
        #endregion

        #region Objetos

        public bool ExecPreprocessPendingObjects(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread thread = new Thread(() => PreprocessPendingObjects(cmdData));
            thread.Start();
            return true;
        }

        public bool ExecParametersTypeComparer(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ParametersTypeComparer();
            return true;
        }

        private static void ParametersTypeComparer()
        {
            
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Transaction>());
            selectObjectOption.MultipleSelection = true;

            List<KBObject> objs = (List<KBObject>)UIServices.SelectObjectDialog.SelectObjects(selectObjectOption);
            int cant = 0;
            Thread thread = new Thread(() => ParametersTypeComparer(UIServices.KB.CurrentKB, objs, out cant));
            thread.Start();
        }

        private static void ParametersTypeComparer(KnowledgeBase KB, List<KBObject> objs, out int cant)
        {
            cant = 0;  
            KBDoctorOutput.StartSection("KBDoctor - Parameters Type Comparer");
            string recommendations = "";
            API.ParametersTypeComparer(UIServices.KB.CurrentKB, objs, ref recommendations, out cant);
            KBDoctorOutput.EndSection("KBDoctor - Parameters Type Comparer");
        }

        public bool ExecEmptyConditionalBlock(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            EmptyConditionalBlock();
            return true;
        }

        public bool ExecNewsWithoutWhenDuplicate(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            NewsWithoutWhenDuplicate();
            return true;
        }

        public bool ExecConstantsInCode(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ConstantsInCode();
            return true;
        }

        public bool ExecReviewCommits(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ReviewCommits();
            return true;
        }

        private static void ConstantsInCode()
        {
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Transaction>());
            selectObjectOption.MultipleSelection = true;

            List<KBObject> objs = (List<KBObject>)UIServices.SelectObjectDialog.SelectObjects(selectObjectOption);

            Thread thread = new Thread(() => ConstantsInCode(UIServices.KB.CurrentKB, objs));
            thread.Start();
        }

        private static void ConstantsInCode(KnowledgeBase KB, List<KBObject> objs)
        {
            int cant;
            KBDoctorOutput.StartSection("KBDoctor - Verify empty conditional blocks");
            API.ConstantsInCode(UIServices.KB.CurrentKB, objs, out cant);
            KBDoctorOutput.EndSection("KBDoctor - Verify empty conditionals blocks");
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
            catch(Exception e)
            {
                success = false;
                error_message = e.Message;
                KBDoctorOutput.InternalError(error_message, e);
            }
            KBDoctorOutput.EndSection("KBDoctor - Review commits", success);
        }

        public bool ExecForEachsWithoutWhenNone(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ForEachsWithoutWhenNone();
            return true;
        }


        private static void EmptyConditionalBlock()
        {
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Transaction>());
            selectObjectOption.MultipleSelection = true;

            List<KBObject> objs = (List<KBObject>)UIServices.SelectObjectDialog.SelectObjects(selectObjectOption);

            Thread thread = new Thread(() => EmptyConditionalBlock(UIServices.KB.CurrentKB, objs));
            thread.Start();
        }

        private static void NewsWithoutWhenDuplicate()
        {
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Transaction>());
            selectObjectOption.MultipleSelection = true;

            List<KBObject> objs = (List<KBObject>)UIServices.SelectObjectDialog.SelectObjects(selectObjectOption);

            Thread thread = new Thread(() => NewsWithoutWhenDuplicate(UIServices.KB.CurrentKB, objs));
            thread.Start();
        }

        private static void ForEachsWithoutWhenNone()
        {
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Transaction>());
            selectObjectOption.MultipleSelection = true;

            List<KBObject> objs = (List<KBObject>)UIServices.SelectObjectDialog.SelectObjects(selectObjectOption);

            Thread thread = new Thread(() => ForEachsWithoutWhenNone(UIServices.KB.CurrentKB, objs));
            thread.Start();
        }



        private static void ForEachsWithoutWhenNone(KnowledgeBase KB, List<KBObject> objs)
        {
            KBDoctorOutput.StartSection("KBDoctor - Verify empty conditional blocks");
            API.ForEachsWithoutWhenNone(UIServices.KB.CurrentKB, objs);
            KBDoctorOutput.EndSection("KBDoctor - Verify empty conditionals blocks");
        }

        private static void NewsWithoutWhenDuplicate(KnowledgeBase KB, List<KBObject> objs)
        {
            KBDoctorOutput.StartSection("KBDoctor - Verify empty conditional blocks");
            API.NewsWithoutWhenDuplicate(UIServices.KB.CurrentKB, objs);
            KBDoctorOutput.EndSection("KBDoctor - Verify empty conditionals blocks");
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            SelectedRowsCollection selrows = cmdData.Context as SelectedRowsCollection;
            List<KBObjectHistory> kbohList = GetGenericHistoryObjects(selrows);
            IKBService kbserv = UIServices.KB;


            string title = "KBDoctor - Review Objects";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Problems", "Technical Debt (min)" });
                List<string[]> lineswriter = new List<string[]>(); ;


                KBModel model = UIServices.KB.CurrentModel;
                List<KBObject> selectedObjects = new List<KBObject>();
                foreach (KBObjectHistory kboh in kbohList)
                {
                    KBObject obj = model.Objects.Get(kboh.Key);
                    if (obj != null)
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ReviewObjects();
            return true;
        }

        private static void ReviewObjects()
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = kbserv.CurrentModel;

            

            string title = "KBDoctor - Review Objects";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Problems" , "Technical debt (min)" });

                List<KBObject> selectedObjects = new List<KBObject>();

                foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
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

        public bool ExecReviewObject(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread thread = new Thread(() => ReviewObject(cmdData));
            thread.Start();
            return true;
        }

        private static void ReviewObject(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            string title = "KBDoctor - Review Objects";
            IKBService kbserv = UIServices.KB;
            try
            {
                double cant = 0;
                double cantSum = 0;
                string outputFile = Functions.CreateOutputFile(kbserv, title);
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
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
                string outputFile = Functions.CreateOutputFile(kbserv, title);
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            return (from UltraGridRow row in rows
                    where !row.IsGroupByRow // Quitamos las rows de grupo.
                    select (KBObjectHistory)row.Cells["KBObjectHistory"].Value).ToList();
        }

        public bool ExecObjNotReacheable(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.Unreachables));
            t.Start();
            return true;
        }

        public bool ExecObjectsWithoutInOut(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ParmWOInOut));
            t.Start();
            return true;
        }

        public bool ExecObjectsMainsCalled(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsMainsCalled));
            t.Start();
            return true;
        }

        public bool ExecObjectsReferenced(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            CleanKBHelper.ObjectsReferenced();
            return true;
        }

        public bool ExecObjectsWithVarNotBasedOnAtt(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsWithVarNotBasedOnAtt));
            t.Start();
            return true;
        }
        public bool ExecListDynamicCombo(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(BadSmells.ListDynamicCombo));
            t.Start();
            return true;
        }

        public bool ExecBuildModule(CommandData cmdData)
        {
           IOutputService output = CommonServices.Output;
           output.SelectOutput("KBDoctor");
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            //Hace el Build with this only de los objetos de un modulo y de los que lo referencian
            ObjectsHelper.BuildObjectAndReferences();
            return true;
        }

        public bool ExecBuildObjectWithProperty(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            //Hace el Build with this only de los objetos de un modulo y de los que lo referencian
            ObjectsHelper.BuildObjectWithProperty();
            return true;
        }

        public bool ExecRenameVariables(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            // Cambiar las variables para que se basen en atributos o dominios.
            Thread t = new Thread(new ThreadStart(CleanKBHelper.RenameVariables));
            t.Start();
            return true;
        }

        public bool ExecProceduresThatUpdatesAttributes(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsUpdatingAttributes));
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            return true;
        }

        public bool ExecObjectsNotCalled(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjetNotCalled));
            t.Start();
            return true;
        }

        public bool ExecObjectsWithCommitOnExit(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsWithParmAndCommitOnExit));
            t.Start();
            return true;
        }

        public bool ExecObjectsWithVarsNotUsed(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsWithVarsNotUsed));
            t.Start();
            return true;
        }

        public bool ExecResetWINForm(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ResetWINForm));
            t.Start();
            return true;
        }

        public bool ExecListProcedureCallWebpanelTransaction(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ListProcedureCallWebPanelTransaction));
            t.Start();
            return true;
        }

        public bool ExecObjectsComplex(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsComplex));
            t.Start();
            return true;
        }

        public bool ExecObjectsLegacyCode(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsLegacyCode));
            t.Start();
            return true;
        }

        public bool ExecEditLegacyCodeToReplace(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.EditLegacyCodeToReplace();
            return true;
        }

        public bool ExecEditReviewObjects(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.EditReviewObjects();
            return true;
        }


        public bool ExecChangeLegacyCode(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.ChangeLegacyCode();
            return true;
        }

        public bool ExecChangeCommitOnExit(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.ChangeCommitOnExit();
            return true;
        }

        public bool ExecTreeCommit(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.TreeCommit();
            return true;
        }

        public bool ExecObjectsRefactoringCandidates(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            //  Thread t = new Thread(new ThreadStart(ObjectsHelper.ObjectsRefactoringCandidates));
            Thread t = new Thread(new ThreadStart(ObjectsHelper.GenerateCSV_ObjectsRefactoring));
            t.Start();
            return true;
        }

        public bool ExecCountTableAccess(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.CountTableAccess));
            t.Start();
            return true;
        }

        public bool ExecObjectsWithConstants(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.FixVariablesNotBasedInAttributesOrDomain();
            return true;
        }

        public bool ExecObjectsWINWEB(CommandData cmdData)
        {

            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(KbStats.ObjectsWINWEB));
            t.Start();
            return true;
        }

        public bool ExecMainTableUsed(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.MainTableUsed));
            t.Start();
            return true;
        }
        public bool ExecRemovableTransactions(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(ObjectsHelper.RemovableTransactions));
            t.Start();
            return true;
        }

        public bool ExecGenerateSQLScripts(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            TablesHelper.GenerateSQLScripts();
            return true;
        }
        public bool ExecGenerateGraph(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(GraphHelper.GenerateGraph));
            t.Start();
            return true;
        }

        public bool ExecKBInterfaces(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
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

        public bool ExecOpenKBObject(CommandData cmdData)
        {
            ObjectsHelper.OpenObject(cmdData.Parameters);

            return true;
        }

        public bool ExecAssignAttributeToVariable(CommandData cmdData)
        {
            ObjectsHelper.AssignAttributeToVariable(cmdData.Parameters);
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
        #endregion

        #region Acerca de
        public bool ExecAboutKBDoctor(CommandData cmdData)
        {
            Assembly assem = this.GetType().Assembly;
            object[] atributos = assem.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
            using (Form aboutBox = new AboutBox1())
            {
                aboutBox.ShowDialog();
            }
            return true;
        }

        public bool ExecHelpKBDoctor(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
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

            KBDoctorCore.Sources.API.CleanKBObjects(kbModel, UIServices.SelectObjectDialog.SelectObjects(selectObjectOption), output);
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


        public bool ExecPrepareComparerNavigation(CommandData cmdData)
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            KBDoctorCore.Sources.API.PrepareCompareNavigations(kbserv.CurrentKB, output);
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

        public bool ExecListClassUsed(CommandData cmdData)
        {
            ThemeHelper.ClassUsed();
            return true;
        }
        #endregion

        public bool ExecCountGeneratedByPattern(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            KbStats.CountGeneratedByPattern();
            return true;
        }

        public bool ExecReplaceNullCompatible(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            AttributesHelper.ReplaceNullsCompatible();
            return true;
        }

        public bool ExecListObj(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            Thread t = new Thread(new ThreadStart(KbStats.ListObj));
            t.Start();
            return true;
        }

        public bool ExecCreateDeployUnits(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.CreateDeployUnits();
            return true;
        }

        public bool ExecMarkPublicObjects(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ModulesHelper.MarkPublicObjects();
            return true;
        }

        public bool ExecListModules(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
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
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ModulesHelper.ListModulesStatistics();
            return true;
        }

        public bool ExecListModularizationQuality(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ModulesHelper.ListModularizationQuality();
            return true;
        }
        public bool ExecMoveTransactions(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ModulesHelper.MoveTransactions();
            return true;
        }

        public bool ExecCalculateCheckSum(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.CalculateCheckSum();
            return true;
        }

        public bool ExecGenerateLocationXML(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.GenerateLocationXML();
            return true;
        }

        public bool ExecListLastReports(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            KbStats.ListLastReports();
            return true;
        }

        public bool ExecListObjWarningsErrors(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            NavigationHelper.ListObjWarningsErrors();
            return true;
        }

        public bool ExecListObjSimilarNavigation(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            NavigationHelper.ListObjSimilarNavigation();
            return true;
        }

        public bool ExecListAPIObjects(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.ListAPIObjects();
            return true;
        }
        public bool ExecRecomendedModule(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ModulesHelper.RecomendedModule();
            return true;
        }

        public bool ExecApplyExternalModularization(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ModulesHelper.ApplyExternalModularization();
            return true;
        }
        public bool ExecSplitMainObject(CommandData cmdData)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            ObjectsHelper.SplitMainObject();
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
                        list.Add(current as KBObject);
                    }
                }
            }
            else
            {
                list.Add(data.Context as KBObject);
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
