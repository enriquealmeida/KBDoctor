<?xml version='1.0'?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <xsl:output method="text"/>


<xsl:template match="/">
	<xsl:text>Text Format Navigation for GX X Evolution 1- Version 1.0</xsl:text>
	<xsl:apply-templates select="ObjectSpec"/>
</xsl:template>

<xsl:include href="include.xsl"/>
<xsl:include href="formulas.xsl"/>
<xsl:include href="genexusSpecific90.xsl"/>



<!-- FROM FORMULA TEMPLATE -->
<xsl:template match="FromFormula|FromValue">
	<xsl:apply-templates select="*"/>
</xsl:template>


<!-- RULES TEMPLATES -->

<!-- RULES-->
<xsl:template match="StandAloneRules|StandAloneWithModeRules|BaseTableRule|AfterConfirmRules|AfterInsertRules|AfterUpdateRules|BeforeConfirmRules|BeforeInsertRules|BeforeUpdateRules|BeforeDeleteRules|BeforeTrnRules">
	<xsl:apply-templates select="Action"></xsl:apply-templates>    
</xsl:template>

<xsl:template match="AfterDeleteRules|AfterTrnRules|AfterLevelRules|NotIncludedRules">
	<xsl:apply-templates select="Action"/>
</xsl:template>

<xsl:template match="Rules">
	
		<xsl:apply-templates select="Action"/>
	<xsl:if test="NonTriggeredActions/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">No Triggered Actions</xsl:with-param>
			<xsl:with-param name="Node">NonTriggeredActions</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
</xsl:template>

<xsl:template match="NonTriggeredActions">
	<xsl:apply-templates/>
</xsl:template>
<!-- END RULES -->


<!-- ACTION TEMPLATE -->
<xsl:template match="Action">
	<xsl:choose>
		<xsl:when test="ActionType[.='Formula']">
				<xsl:apply-templates select="FormulaName"/>
				<xsl:text> </xsl:text><xsl:apply-templates select="FormulaExpression"/>
		</xsl:when>
		<xsl:when test="ActionType[.='ReadTable' or .='ReadBaseTable' or .='ReadCKey']">
				<xsl:text>
		READ  </xsl:text>
			<xsl:apply-templates select="Table"/>
				<xsl:if test="ActionType[.='ReadCKey']"><xsl:text> UNIQUE</xsl:text></xsl:if>
					<xsl:if test="JoinType[.='Outer']"><xsl:text> allowing nulls</xsl:text></xsl:if>
		<xsl:text> 
			WHERE </xsl:text>
				<xsl:apply-templates select="JoinConditions"/>
				<xsl:if test="Into/Attribute">
				<xsl:text>
			INTO </xsl:text>
				<xsl:apply-templates select="Into"/>
				</xsl:if>
		</xsl:when>
		<xsl:when test="ActionType[.='BusinessRule']">
				<xsl:choose>
					<xsl:when test="CALL">
						<xsl:apply-templates select="CALL"/>
					</xsl:when>
					<xsl:when test="SUBMIT">
						<xsl:apply-templates select="SUBMIT"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:choose>
							<xsl:when test="RuleType[.='ErrorRule']">
								Error( <xsl:apply-templates select="Expression"/>)
							</xsl:when>
							<xsl:when test="RuleType[.='MsgRule']">
								Msg( <xsl:apply-templates select="Expression"/>)
							</xsl:when>
							<xsl:when test="RuleType[.='NoacceptRule']">
								NoAccept( <xsl:apply-templates select="Parameters"/>)
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="."/>(<xsl:apply-templates select="Parameters"/>)
							</xsl:otherwise>
						</xsl:choose>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="Condition/*">IF <xsl:apply-templates select="Condition"/></xsl:if>
		</xsl:when>
		<xsl:when test="ActionType[.='SubType']">
					<xsl:apply-templates select="SubtypeAction"/>
		</xsl:when>
		<xsl:when test="ActionType[.='VerticalFormulas']">
					<xsl:if test="not($Product!='Deklarit')">VERTICAL</xsl:if>
				FORMULAS:
				<xsl:apply-templates select="VerticalFormulasToCalc"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:apply-templates/>
		</xsl:otherwise>
	</xsl:choose>
	<xsl:call-template name="NewLine"/>
</xsl:template>

<!-- SUBTYPEACTION TEMPLATE -->
<xsl:template match="ActionType"/>

<xsl:template match="DynamicLoad">
FILL <xsl:apply-templates select="ControlName"/> with <xsl:apply-templates select="CodeAttributes"/>, <xsl:apply-templates select="DescriptionAttributes"/> in <xsl:apply-templates select="Navigation"/> 
<xsl:call-template name="NewLine"/>
</xsl:template>

<xsl:template match="HideCode">
FIND <xsl:apply-templates select="CodeAttributes"/> with <xsl:apply-templates select="DescriptionAttributes"/> in <xsl:apply-templates select="Navigation"/> 
</xsl:template>

<xsl:template match="SubtypeAction">
<xsl:value-of select="Supertype"/> = <xsl:value-of select="Subtype"/>
</xsl:template>

<xsl:template match="JoinLocation">
	Join location: <xsl:choose><xsl:when test="text()[.='1']">Server</xsl:when><xsl:otherwise>Client</xsl:otherwise></xsl:choose>
	<xsl:call-template name="NewLine"/>
</xsl:template>

<!-- RULE TYPE TEMPLATE -->
<xsl:template match="RuleType">
	<xsl:choose>
		<xsl:when test="text()[.='ErrorRule']">
			Error(<xsl:apply-templates select="../Parameters"/>)
		</xsl:when>
		<xsl:when test="text()[.='NoacceptRule']">
			NoAccept(<xsl:apply-templates select="../Parameters"/>)
		</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="."/>(<xsl:apply-templates select="../Parameters"/>)
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<!-- JOIN CONDITIONS -->
<xsl:template match="JoinConditions">
	<xsl:for-each select="JoinCondition">
		<xsl:apply-templates select="../Table"/>.<xsl:apply-templates select="AttributeTo"/>
		<xsl:text> = </xsl:text>
		<xsl:if test="Table/TableName"><xsl:apply-templates select="Table"/>.</xsl:if>
		<xsl:apply-templates select="AttributeFrom"/><xsl:text>; </xsl:text>
	</xsl:for-each>
</xsl:template>


<!-- END JOIN CONDITIONS -->

<!-- END RULE TEMPLATES -->



<xsl:template match="BaseTable">
	<xsl:apply-templates select="Table"/>
</xsl:template>

<xsl:template match="Generator"><xsl:apply-templates select="GenId"/></xsl:template>	

<!--  Standard Templates  -->




<xsl:template match="ObjClsName">
	<xsl:variable name="Classes" select="document('classes.xml')"/>
	<xsl:variable name="ClassName"><xsl:value-of select="."/></xsl:variable>
	<xsl:choose>
		<xsl:when test="$Classes//Class[Name=$ClassName]/MapedName">
			<xsl:value-of select="$Classes//Class[Name=$ClassName]/MapedName"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="."/>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="MainInfo">
	  
	Name: <xsl:apply-templates select="Object" mode="icon"/>
  	Description : <xsl:value-of select="Object/ObjDesc"/>
	<xsl:if test="Result!='genreq'">
     <!-- 	Status: <xsl:apply-templates select="Result"/>  -->
	</xsl:if>
    
    <xsl:if test="$Product='Deklarit'">
	<xsl:if test="Parameters/*">
	Parameters:	<xsl:apply-templates select="Parameters"/>
	</xsl:if>
   </xsl:if>
  <xsl:if test="not($Product='Deklarit')">
	
	<xsl:if test="OutputDevices">
	Output Devices: <xsl:apply-templates select="OutputDevices"/>
	</xsl:if>
  </xsl:if>

</xsl:template>

<xsl:template name="Enviroment">
	Environment: <xsl:apply-templates select="Generator"/>
	Spec. Version: 	<xsl:apply-templates select="SpecVersion"/>
	<xsl:if test="FormClass">
	Form Class: <xsl:value-of select="FormClass"/>
	</xsl:if>
	<xsl:if test="Object/ObjPgmName">
	Program Name: <xsl:value-of select="Object/ObjPgmName"/>
	</xsl:if>
	<xsl:if test="CallProtocol">
	Call Protocol: <xsl:value-of select="CallProtocol"/>
	</xsl:if>
	<xsl:if test="Parameters">
	Parameters:	<xsl:apply-templates select="Parameters"/><xsl:text>
	
	</xsl:text>
	</xsl:if>
</xsl:template>

<xsl:template name="Levels">
<!-- BEGIN LEVELS -->
<xsl:if test="Levels/Level">    
	<xsl:call-template name="TableHeaderSubMain">
		<xsl:with-param name="id">level</xsl:with-param>
		<xsl:with-param name="title">Levels</xsl:with-param>
	</xsl:call-template>
	<xsl:apply-templates select="Levels"/>
</xsl:if>
<!-- END LEVELS -->
</xsl:template>


<xsl:template match="ObjectSpec">
	<xsl:call-template name="TableHeaderMain">
		<xsl:with-param name="id">Obj</xsl:with-param>
		<xsl:with-param name="title">
			<xsl:apply-templates select="Object/ObjClsName"/>
			<xsl:text> </xsl:text>
			<xsl:apply-templates select="Object" mode="xRef"/>
			<xsl:text> Navigation Report</xsl:text>
		</xsl:with-param>
	</xsl:call-template>

	<xsl:call-template name="MainInfo"/>
	<xsl:if test="not($Product='Deklarit')">
		<xsl:call-template name="Enviroment"/>
	</xsl:if>
	<xsl:if test="Errors/Error">
		<xsl:apply-templates select="Errors"/>
	</xsl:if>
	<xsl:if test="Warnings/Warning">
		<xsl:apply-templates select="Warnings"/>
	</xsl:if>
	<xsl:if test="NotIncludedRules/Action">
		<xsl:text>Rules not included</xsl:text>
		<xsl:call-template name="NewLine"/>
		<xsl:apply-templates select="NotIncludedRules"/>
	</xsl:if>
			<xsl:apply-templates select="StandAloneRules"/>

	<xsl:call-template name="Levels"/>

	<xsl:if test="not($Product='Deklarit')">
		<xsl:if test="MenuBar">
			<xsl:apply-templates select="MenuBar"/>
		</xsl:if>
		<xsl:if test="MenuOptions">
			<xsl:apply-templates select="MenuOptions"/>
		</xsl:if>
	</xsl:if>

	<xsl:if test="not($Product='Deklarit')">
				<xsl:if test="Event/*">
					<xsl:apply-templates select="Event"/>
				</xsl:if>
			</xsl:if>

			<xsl:if test="ImplicitForEach">
				<xsl:for-each select="ImplicitForEach">
						<xsl:apply-templates select="."/>
				</xsl:for-each>
			</xsl:if>

			<xsl:if test="not($Product='Deklarit')">
				<xsl:if test="Prompts">
				<xsl:apply-templates select="Prompts"/>
				</xsl:if>

				<xsl:if test="DynamicCombos/DynamicCombo">
						<xsl:apply-templates select="DynamicCombos"/>
				</xsl:if>
				<xsl:if test="Suggests/Suggest">
					<xsl:apply-templates select="Suggests"/>
				</xsl:if>
			</xsl:if>

</xsl:template>


<xsl:template match="SpecVersion">
	<xsl:value-of select="."/>
</xsl:template>

<xsl:template match="AttriName">
	<xsl:if test="position() != 1">, </xsl:if>
	<xsl:apply-templates/>
</xsl:template>

<!-- Generic Navigation -->
<xsl:template match="Navigation">
	<xsl:apply-templates select="NavigationTree/Table" mode="Tree"/>
	<xsl:if test="NavigationConditions/*">Where <xsl:apply-templates select="NavigationConditions"/></xsl:if>
	Order <xsl:apply-templates select="NavigationOrder"/>
</xsl:template>

<xsl:template match="NavigationConditions">
<xsl:for-each select="Condition"><xsl:if test="position() != 1"> And </xsl:if><xsl:call-template name="ProcessList"/></xsl:for-each>
</xsl:template>

<xsl:template match="NavigationOrder">
	<xsl:apply-templates select="Order"/><xsl:apply-templates select="IndexName"/>
</xsl:template>

<xsl:template match="Order">
	<xsl:choose> 
		<xsl:when test="*">
			<xsl:call-template name="ProcessList"/>
		</xsl:when>
		<xsl:otherwise>None</xsl:otherwise>
	</xsl:choose> 
</xsl:template>

<xsl:template match="IndexName">
	Index: <xsl:value-of select="."/>
</xsl:template>

<xsl:template match="NavigationTree">
	<xsl:call-template name="NewLine"/>
	<xsl:apply-templates select="Table" mode="Tree"/>	
</xsl:template>

<xsl:template match="KeyAttributes">
	<xsl:text>(</xsl:text><xsl:call-template name="ProcessList"><xsl:with-param name="Sep">,</xsl:with-param></xsl:call-template><xsl:text>)</xsl:text>
</xsl:template>

<!-- LEVEL TEMPLATE -->

<xsl:template match="Levels">
	<xsl:for-each select="Level">
		<xsl:apply-templates select="."/>
	</xsl:for-each>
</xsl:template>

<xsl:template match="Optimizations">
	<xsl:if test="Optimization">
	Optimizations:	<xsl:for-each select="Optimization">
				<xsl:apply-templates select="."/>
			</xsl:for-each>
	</xsl:if>
</xsl:template>

<xsl:template match="Optimization">
	<xsl:choose>
		<xsl:when test="Type[.='Delete']">Delete</xsl:when>
		<xsl:when test="Type[.='Aggregate']"><xsl:apply-templates select="Expression"/> </xsl:when>
		<xsl:when test="Type[.='InsertWSubSelect']">Insert with subselect</xsl:when>
		<xsl:when test="Type[.='Update']">Update</xsl:when>
		<xsl:when test="Type[.='FirstRows']">First <xsl:value-of select="MaxRows"/> record(s)</xsl:when>
		<xsl:when test="Type[.='ServerPaging']">Server Paging</xsl:when>
		<xsl:otherwise>Unknown optimization</xsl:otherwise>
	</xsl:choose><xsl:call-template name="NewLine"/>
</xsl:template>

<xsl:template match="LevelOptions">
	Options:
		<xsl:for-each select="LevelOption">
			<xsl:if test="position() != 1">, </xsl:if>
			<xsl:apply-templates select="."/>
		</xsl:for-each>
</xsl:template>

<xsl:template match="ConditionalOrders">
<xsl:if test="ConditionalOrder">
	Order: 
	<xsl:for-each select="ConditionalOrder">
		<xsl:apply-templates select="Order"/><xsl:text> </xsl:text>
		<xsl:choose> 
			<xsl:when test="Condition/*">
				<xsl:text>WHEN </xsl:text><xsl:apply-templates select="Condition"/>							
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>OTHERWISE </xsl:text>
			</xsl:otherwise>
		</xsl:choose> 
		<xsl:call-template name="UsedIndex"/>
	</xsl:for-each>
</xsl:if>
</xsl:template>

<xsl:template match="LevelType">
	<xsl:choose>
	<xsl:when test="text()[.='For First']">For First</xsl:when>
	<xsl:when test="text()[.='New']">New</xsl:when>
	<xsl:when test="text()[.='XNew']">XNew</xsl:when>
	<xsl:when test="text()[.='For Each']">For Each</xsl:when>
	<xsl:when test="text()[.='Break']">Break</xsl:when>
	<xsl:when test="text()[.='XFor Each']">XFor Each</xsl:when>
	<xsl:otherwise>Level</xsl:otherwise>
	</xsl:choose>
</xsl:template>


<xsl:template match="LevelBeginRow">
	<xsl:if test="$Product!='Deklarit'">
	 <xsl:text>  </xsl:text>(Line: <xsl:value-of select="."/>)
	</xsl:if>
</xsl:template>

<!-- LEVEL OR FOREACH -->
<xsl:template match="Level|ImplicitForEach">
	<xsl:if test="BaseTable">
		<xsl:choose>
			<xsl:when test="not(../ImplicitForEach)">
				<xsl:attribute name="id">gx<xsl:value-of select="BaseTable/Table/TableId"/></xsl:attribute>
					<xsl:call-template name="TableHeader">
						<xsl:with-param name="id">gx<xsl:value-of select="BaseTable/Table/TableId"/></xsl:with-param>
						<xsl:with-param name="title">
							<xsl:apply-templates select="LevelType"/> 
							<xsl:text> </xsl:text><xsl:apply-templates select="BaseTable"/> 
							<xsl:text> </xsl:text><xsl:apply-templates select="LevelBeginRow"/> 
						</xsl:with-param>
					</xsl:call-template>
		<xsl:call-template name="NewLine"/>
		<xsl:call-template name="LevelInfo"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="LevelInfo"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:if>
	<xsl:if test="LevelType[text()='Undefined']">
		<xsl:apply-templates select="Event"/>
	</xsl:if>
</xsl:template>

<xsl:template name="LevelInfo">
	<xsl:if test="SubordinateTo">
		<xsl:apply-templates select="SubordinateTo"/>
	</xsl:if>
	<xsl:if test="LevelType[text()!='New']">
	<xsl:if test="Order">
	Order:	<xsl:apply-templates select="Order"/><xsl:text> </xsl:text>			
							<xsl:call-template name="UsedIndex"/>
	</xsl:if>
	</xsl:if>
	<xsl:if test="LevelOptions/*">
		<xsl:apply-templates select="LevelOptions"/>
	</xsl:if>
	<xsl:if test="ConditionalOrders">
		<xsl:apply-templates select="ConditionalOrders"/>
	</xsl:if>
	<xsl:apply-templates select="OptimizedWhere"/>
	<xsl:apply-templates select="NonOptimizedWhere"/>
	<xsl:if test="JoinLocation">
		<xsl:apply-templates select="JoinLocation"/>
	</xsl:if>
	<xsl:apply-templates select="Optimizations"/>
	<xsl:apply-templates select="StandAloneRules"/>
	<xsl:apply-templates select="BaseTableRule"/>
	<xsl:apply-templates select="StandAloneWithModeRules"/>
	<xsl:apply-templates select="//RedundantFormulas"/>
	<xsl:if test="AfterAttributeRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">After Attribute Rules</xsl:with-param>
			<xsl:with-param name="Node">AfterAttributeRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:apply-templates select="Rules"/>
	<xsl:if test="BeforeConfirmRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">Before Validate Rules</xsl:with-param>
			<xsl:with-param name="Node">BeforeConfirmRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="AfterConfirmRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">After Validate Rules</xsl:with-param>
			<xsl:with-param name="Node">AfterConfirmRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:apply-templates select="NavigationTree"/>
	<xsl:apply-templates select="TablesToUpdate"/>
	<xsl:apply-templates select="DynamicLoads"/>
	<xsl:apply-templates select="Formulas"/>
	<xsl:if test="BeforeInsertRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">Before Insert Rules</xsl:with-param>
			<xsl:with-param name="Node">BeforeInsertRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="AfterInsertRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">After Insert Rules</xsl:with-param>
			<xsl:with-param name="Node">AfterInsertRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="BeforeUpdateRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">Before Update Rules</xsl:with-param>
			<xsl:with-param name="Node">BeforeUpdateRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="AfterUpdateRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">After Update Rules</xsl:with-param>
			<xsl:with-param name="Node">AfterUpdateRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="BeforeDeleteRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">Before Delete Rules</xsl:with-param>
			<xsl:with-param name="Node">BeforeDeleteRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="AfterDeleteRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">After Delete Rules</xsl:with-param>
			<xsl:with-param name="Node">AfterDeleteRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:apply-templates select="TablesToControlOnDelete"/>
	<xsl:apply-templates select="Levels"/>
	
		
			<xsl:for-each select="Level">
				<xsl:apply-templates select="."/>
			</xsl:for-each>
		
	
	<xsl:if test="BeforeTrnRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">Before Complete Rules</xsl:with-param>
			<xsl:with-param name="Node">BeforeTrnRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="AfterTrnRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">After Complete Rules</xsl:with-param>
			<xsl:with-param name="Node">AfterTrnRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="AfterLevelRules/Action">
		<xsl:call-template name="TitledRules">
			<xsl:with-param name="Title">After Level Rules</xsl:with-param>
			<xsl:with-param name="Node">AfterLevelRules</xsl:with-param>
		</xsl:call-template>
	</xsl:if>
	<xsl:if test="Event">
		<xsl:apply-templates select="Event"/>
	</xsl:if>
	<xsl:if test="CALL">
		<xsl:text>   </xsl:text><xsl:apply-templates select="CALL"/>
	</xsl:if>
	<xsl:if test="SUBMIT">
		<xsl:text>   </xsl:text><xsl:apply-templates select="SUBMIT"/>
	</xsl:if>
	<xsl:if test="Binding">
		<xsl:text>   </xsl:text><xsl:apply-templates select="Binding"/>
	</xsl:if>   
</xsl:template>

<!-- END LEVEL TEMPLATE -->


<!-- tableS TO UPDATE ON DELETE -->

<xsl:template match="TablesToControlOnDelete">
	<xsl:if test="Table/TableName">
		<xsl:call-template name="NewLine"/>
	-- Referential integrity controls on delete:
		<xsl:call-template name="NewLine"/>
		<xsl:for-each select="Table">
			<xsl:sort select="TableName"/>
			<xsl:apply-templates select="."/> <xsl:apply-templates select="KeyAttributes"/>
			<xsl:call-template name="NewLine"/> 
</xsl:for-each>

</xsl:if>
</xsl:template>

<!-- END TALBES TO UPDATE ON DELETE -->

<xsl:template name="UsedIndex">
	<!-- Display index information only when an order has been specified -->
	<xsl:if test="Order/*">
		<xsl:choose>
			<xsl:when test="IndexName">
				<xsl:apply-templates select="IndexName"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>! No index</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:call-template name="NewLine"/>
	</xsl:if>
</xsl:template>



<xsl:template match="InputParameters|OutputParameters">
	<xsl:for-each select="Attribute|Variable">
		<xsl:if test="position() != 1">, </xsl:if>
		<xsl:apply-templates select="."/>
	</xsl:for-each>
</xsl:template>

<xsl:template match="AttriType|AttriOldType" mode="Properties">
	<xsl:for-each select="Properties/Property">
		<xsl:apply-templates select="." mode="Attribute"/>
	</xsl:for-each>
</xsl:template>

<xsl:template match="Property" mode="Attribute">
	<xsl:if test="Value[.='Yes']">
		<xsl:value-of select="Name"/> = <xsl:value-of select="Value"/>
	</xsl:if>
</xsl:template>

<xsl:template match="AttriType|AttriOldType">
	<xsl:call-template name="PrintType">
		<xsl:with-param name="Type"><xsl:value-of select="DataType"/></xsl:with-param>
		<xsl:with-param name="Length"><xsl:value-of select="Presicion"/></xsl:with-param>
		<xsl:with-param name="Decimals"><xsl:value-of select="Scale"/></xsl:with-param>
	</xsl:call-template>
</xsl:template>

<xsl:template match="FormulaName">
	<xsl:call-template name="ProcessList"/><xsl:if test="Attribute|Variable"> = </xsl:if>
</xsl:template>

<xsl:template match="IF|Condition|Constraint|Expression|FormulaExpression|CodeAttributes|DescriptionAttributes|ControlName|VerticalFormulasToCalc">
	<xsl:call-template name="ProcessList"/>
</xsl:template>

<!-- <xsl:template match="Parameters|RedundantAttris|RedundantAttrisToUpdate|FromAttrisToUpdate|AttrisToUpdate"> -->
<xsl:template match="Parameters|RedundantAttris|RedundantAttrisToUpdate|FromAttrisToUpdate">
	<xsl:call-template name="ProcessList">
		<xsl:with-param name="Sep">,</xsl:with-param>
	</xsl:call-template>
</xsl:template>

<!-- 17/12/05 Agregado por PM para procesar lista ordenada alfabeticamente --> 
<xsl:template match="AttrisToUpdate|Into">
	<xsl:call-template name="ProcessListOrdered">
		<xsl:with-param name="Sep">,</xsl:with-param>
	</xsl:call-template>
</xsl:template>
<xsl:template match="Parameter">
	<xsl:if test="not($Product='Deklarit')">
		<xsl:apply-templates select="IO"/>
	</xsl:if>
	<xsl:apply-templates select="Token"/>
	<xsl:apply-templates select="Attribute"/>
	<xsl:apply-templates select="Variable"/>
</xsl:template>

<xsl:template match="IO">
	<xsl:if test="text()[.!='inout']">
		<xsl:text>[</xsl:text><xsl:value-of select="."/><xsl:text>]</xsl:text>
	</xsl:if>
</xsl:template>

<!-- END PROMPTS -->

<!-- FOR EACH -->



<!-- STATUS TEMPLATE -->
<xsl:template match="Result">
<xsl:choose>
	<xsl:when test="text()[.='genreq']">Generation is required</xsl:when>
	<xsl:when test="text()[.='specfailed']">Specification Failed</xsl:when>
	<xsl:when test="text()[.='nogenreq']">No Generation Required</xsl:when>
	<xsl:when test="text()[.='nogenspc']">No Specification Required</xsl:when>
	<xsl:when test="text()[.='nospcreq']">No Specification Required</xsl:when>
	<xsl:otherwise><xsl:value-of select="."/></xsl:otherwise>
</xsl:choose>
</xsl:template>



<!-- END STATUS TEMPLATE -->
<xsl:template match="OptimizedWhere">
	<xsl:if test="../LevelType != 'Break'">
	Navigation filters: 
		Start from: <xsl:apply-templates select="StartFrom"/>
		Loop while: <xsl:apply-templates select="LoopWhile"/><xsl:call-template name="NewLine"/>
	</xsl:if>
</xsl:template>

<!-- ENDFOREACH -->

<xsl:template match="NonOptimizedWhere">
<xsl:if test="Condition or ConditionalConstraint">
	Constraints:
					<xsl:for-each select="Condition">
							<xsl:call-template name="ProcessList"/>
					</xsl:for-each>
					<xsl:for-each select="ConditionalConstraint">
							<xsl:apply-templates select="Constraint"/>
							<xsl:text> WHEN </xsl:text>
							<xsl:apply-templates select="Condition"/>
					</xsl:for-each>
</xsl:if>
</xsl:template>

<xsl:template match="StartFrom">
		<xsl:for-each select="Condition">
			<xsl:call-template name="ProcessList"/><xsl:text>  </xsl:text>
		</xsl:for-each>
</xsl:template>

<xsl:template match="LoopWhile">
		<xsl:for-each select="Condition">
			<xsl:call-template name="ProcessList"/><xsl:text>  </xsl:text>
		</xsl:for-each>
</xsl:template>

<xsl:template match="FormulaGivenAttris"> <xsl:apply-templates/></xsl:template>

<xsl:template match="Formulas"><xsl:apply-templates/></xsl:template>

<xsl:template match="DynamicLoads"><xsl:apply-templates/></xsl:template>

<!-- 03/01/06 cambio para mostrar las tablas ordenadas por nombre en cada nivel.  -->
<xsl:template match="Tables">
	<xsl:for-each select="Table">
	<xsl:sort select="TableName"/>
		<xsl:apply-templates select="." mode="Tree"/>
	</xsl:for-each>	
</xsl:template>
<!-- tableSTOUPDATE TEMPLATE -->
<xsl:template match="TablesToUpdate">
		<xsl:for-each select="TableToUpdate">
			<xsl:text>
			* </xsl:text><xsl:apply-templates select="."/>
		</xsl:for-each>
		<xsl:call-template name="NewLine0"/>
</xsl:template>

<xsl:template match="TableToUpdate">
	<xsl:apply-templates select="TableAction"/>
	<xsl:apply-templates select="Table"/>
	<xsl:if test="AttrisToUpdate/Attribute">
		<xsl:text> (</xsl:text><xsl:apply-templates select="AttrisToUpdate"/><xsl:text>)</xsl:text>
	</xsl:if>
	<xsl:if test="UpdateRedundancyCall">
		<xsl:call-template name="NewLine"/>
		<xsl:text>Update Redundancy:</xsl:text> 
			<xsl:apply-templates select="UpdateRedundancyCall"/>
	</xsl:if>
</xsl:template>

<xsl:template match="UpdateRedundancyCall">
	CALL ( <xsl:value-of select="ProgramName"/>,<xsl:value-of select="Parameters"/>	)
</xsl:template>

<xsl:template match="TableAction">
	<xsl:choose>
		<xsl:when test="text()[.='insert']">Insert into </xsl:when>
		<xsl:when test="text()[.='update']">Update </xsl:when>
		<xsl:when test="text()[.='delete']">Delete from </xsl:when>
		<xsl:otherwise></xsl:otherwise>
	</xsl:choose>
</xsl:template>

<!--
<xsl:template match="AttrisToUpdate">
	<xsl:for-each select="Attribute">
		<xsl:if test="position() != 1">, </xsl:if>
		<xsl:apply-templates select="."/>
	</xsl:for-each>
</xsl:template>
-->
<xsl:template match="TableName"><xsl:apply-templates/></xsl:template>

<!-- EVENT TEMPLATE -->
<xsl:template match="Event">
	<xsl:if test="ImplicitForEach|Level|CALL|SUBMIT|Binding">
		<xsl:call-template name="NewLine"/>  
		<xsl:choose><xsl:when test="EventType[text()='Subrutine']">Subroutine</xsl:when><xsl:otherwise>Event </xsl:otherwise></xsl:choose><xsl:value-of select="EventName"/>
			<xsl:call-template name="NewLine"/>
			<xsl:for-each select="ImplicitForEach|Level|CALL|SUBMIT|Binding">
				<xsl:apply-templates select="."/>
			</xsl:for-each>
	</xsl:if>
</xsl:template>
<!-- END EVENT TEMPLATE -->

<xsl:template match="EventName">
</xsl:template>

<xsl:template match="Binding">
	Collection: <xsl:call-template name="ProcessList"/>
</xsl:template>

<xsl:template match="LoadMethod">
	<xsl:if test="text()[.='Auto']">
		Load command or method automatically added.
	</xsl:if>
</xsl:template>

<!-- CALL/SUBMIT TEMPLATE -->
<xsl:template match="CALL">
	<xsl:choose>
		<xsl:when test="ProgramName">
			<xsl:value-of select="ProgramName"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:call-template name="ProcessList">
				<xsl:with-param name="Sep"></xsl:with-param>
			</xsl:call-template>
		</xsl:otherwise>
	</xsl:choose>.Call( <xsl:apply-templates select="Parameters"/>)
	<xsl:if test="IF/*">
		IF
		<xsl:apply-templates select="IF"/>
	</xsl:if>
</xsl:template>

<xsl:template match="SUBMIT">
	<xsl:choose>
		<xsl:when test="ProgramName">
			<xsl:value-of select="ProgramName"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:call-template name="ProcessList">
				<xsl:with-param name="Sep"></xsl:with-param>
			</xsl:call-template>
		</xsl:otherwise>
	</xsl:choose>.Submit( <xsl:apply-templates select="Parameters"/>)
	<xsl:if test="IF/*">
		IF
		<xsl:apply-templates select="IF"/>
	</xsl:if>
</xsl:template>
<!-- END CALL TEMPLATE -->

<xsl:template name="VAttributes">
		<xsl:for-each select="Attributes/Attribute">
			<xsl:apply-templates select="."/><xsl:text> </xsl:text>
		</xsl:for-each>
</xsl:template>


<xsl:template match="Table" mode="Tree">
		<!-- Join type -->
		<xsl:choose>
			<xsl:when test="JoinType[.='Outer']">~</xsl:when>
			<xsl:otherwise>=</xsl:otherwise>
		</xsl:choose>
			<xsl:apply-templates select="TableName"/>
		<xsl:apply-templates select="KeyAttributes"/>
		<xsl:if test="Into/Attribute">
		<xsl:text>
			INTO </xsl:text><xsl:apply-templates select="Into"/>
		</xsl:if>
		<xsl:call-template name="NewLine"/>   
		<xsl:apply-templates select="Tables"/>
</xsl:template>

<xsl:template name='NewLine0'>
<xsl:text>
</xsl:text>
</xsl:template>

<xsl:template name='NewLine'>
<xsl:text>
	</xsl:text>
</xsl:template>

<xsl:template name='NewLine2'>
<xsl:text>
		</xsl:text>
</xsl:template>

<xsl:template name='Blank'>
<xsl:text> </xsl:text>
</xsl:template>
</xsl:stylesheet>
