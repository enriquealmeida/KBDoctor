<?xml version='1.0' encoding='iso-8859-1'?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">



<!-- FORMULAS TEMPLATES -->

<!-- AGGREGATE SELECT TEMPLATE -->
<xsl:template match="AggSelFormulas">
	<xsl:if test="Formula">
	-- Formulas:
	<xsl:for-each select="Formula">
	<xsl:sort select="FormulaAttri/Attribute/AttriName"/>
		<xsl:choose>
			<xsl:when test="FormulaType='aggsel'">
		<xsl:text>Navigation to evaluate: </xsl:text ><xsl:apply-templates select="FormulaAttri"/>
				<xsl:if test="FormulaExpression">
				Formula: <xsl:apply-templates select="FormulaExpression"/>
				</xsl:if>				
				<xsl:call-template name="FormulaWhereMultiple"/>
				<xsl:if test="FormulaGivenAttris">
					Given:<xsl:apply-templates select="FormulaGivenAttris"/>
				</xsl:if>
				<xsl:if test="FormulaIndex">
					Index:<xsl:value-of select="FormulaIndex"/>
				</xsl:if>
				<xsl:if test="FormulaGroupByAttris">
					Group by:<xsl:apply-templates select="FormulaGroupByAttris"/>
				</xsl:if>						
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>Navigation to evaluate: </xsl:text><xsl:value-of select="FormulaName"/>
				<xsl:value-of select="FormulaType"/><xsl:apply-templates select="FormulaOutputAttri"/>
				Where: <xsl:apply-templates select="FormulaWhere"/>
				Given: <xsl:apply-templates select="FormulaGivenAttris"/>
				Index: <xsl:value-of select="FormulaIndex"/>
				Start From: <xsl:apply-templates select="StartFrom"/>
				Loop While: <xsl:apply-templates select="LoopWhile"/>
				Returning: <xsl:apply-templates select="FormulaReturnAttri"/>
			</xsl:otherwise>
		</xsl:choose>	
				Returning: <xsl:apply-templates select="FormulaReturnAttri"/>
		<xsl:apply-templates select="NavigationTree"/>
		<xsl:call-template name="NewLine"/>
	</xsl:for-each>
	</xsl:if>
</xsl:template>
<!-- END AGGREGATE SELECT TEMPLATE -->

<!-- VERTICAL FORMULAS-->
<xsl:template name="FormulaWhereMultiple">
		<xsl:for-each select="FormulaWhere">
			<xsl:choose>
				<xsl:when test="position() = 1">
					Where:<xsl:call-template name="ProcessList"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="ProcessList"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>		
</xsl:template>
<xsl:template match="FormulaWhere">
	<xsl:call-template name="ProcessList"/>
</xsl:template>

<xsl:template match="VerticalFormulas">
	<xsl:if test="VerticalFormulaGroup">
	Formulas:
	<xsl:for-each select="VerticalFormulaGroup">
		Navigation to evaluate: <xsl:apply-templates select="FormulasInGroup"/>
		<xsl:apply-templates select="NavigationTree"/>
	</xsl:for-each>
	</xsl:if>	
</xsl:template>

<xsl:template match="FormulaOutputAttri">
	<xsl:call-template name="ProcessList">
		<xsl:with-param name="Sep">,</xsl:with-param>
	</xsl:call-template>
</xsl:template>

<xsl:template match="FormulasInGroup">
	<xsl:call-template name="ProcessList">
	</xsl:call-template>
</xsl:template>

<!-- REDUNDANTFORMULAS -->
<xsl:template match="RedundantFormulas">
	Redundant Formulas:
	<xsl:apply-templates select="FormulaToUpdate"/>
</xsl:template>

<xsl:template match="FormulaToUpdate">
	<xsl:apply-templates/>
</xsl:template>


</xsl:stylesheet>
