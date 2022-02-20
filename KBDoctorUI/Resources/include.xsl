<?xml version='1.0' encoding='iso-8859-1'?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="html" encoding="iso-8859-1"/>

<xsl:variable name="Product" select="document('version.xml')/Product/Name"/>
<xsl:variable name="Images"  select="document('images.xml')"/>

<xsl:template name="TableHeaderMain">
<xsl:param name="title">Table Title</xsl:param>
	---- <xsl:value-of select="$title"/> ------
</xsl:template>

<xsl:template name="RelPathImage">
	<xsl:param name="Id">img</xsl:param>

</xsl:template>

<xsl:template name="PathImage">
	<xsl:param name="Id">img</xsl:param>

</xsl:template>

<xsl:template name="RenderImage">
	<xsl:param name="Id">img</xsl:param>
</xsl:template>

<xsl:template name="SeparatorRow">
	<xsl:if test="position()!=last()">
		<xsl:call-template name="EndRow"/>
	</xsl:if>
</xsl:template>

<xsl:template name="EndRow">
	---
</xsl:template>

<xsl:template name="GenerateTable">
	<xsl:param name="ID"></xsl:param>
	<xsl:param name="Title"/>
	<xsl:param name="RowTemplate"></xsl:param>
	<xsl:param name="HeaderMain">false</xsl:param>
	<xsl:param name="Subtitulo">true</xsl:param>
	<xsl:param name="image"/>
	
	<xsl:variable name="Tables" select="document('tables.xml')/Tables"/>
	<xsl:choose>
		<xsl:when test="$HeaderMain = 'true'">
			<xsl:call-template name="TableHeaderMain">
				<xsl:with-param name="title" select="$Title"/>
			</xsl:call-template>
		</xsl:when>
		<xsl:otherwise>
			<xsl:call-template name="TableHeader">
				<xsl:with-param name="title" select="$Title"/>
				<xsl:with-param name="image" select="$image"/>
				<xsl:with-param name="width">
				  <xsl:choose>
					<xsl:when test="$Tables/Table[@id = $ID]/Width">
						<xsl:value-of select="$Tables/Table[@id = $ID]/Width"/>
					</xsl:when>
					<xsl:otherwise>100%</xsl:otherwise>
				  </xsl:choose>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:otherwise>
	</xsl:choose>
	<xsl:variable name="CurrentNode" select="."/>
	<xsl:for-each select="$Tables/Table[@id = $ID]/Content/Template">
		<xsl:variable name="Template" select="."/>
		<xsl:for-each select="$CurrentNode/*[name() = $Template]">
		<xsl:sort select="./Table/TableName"/>            <!-- 12/01/05 Para que los prompt se listen ordenados -->
			<xsl:apply-templates select="." mode="TableRow" />
		</xsl:for-each>
	</xsl:for-each>
</xsl:template>

<xsl:template name="TableHeaderSubMain">
	<xsl:param name="title">Table Title</xsl:param> 
	<xsl:param name="image"/>
	<xsl:param name="width">100%</xsl:param>
	<xsl:call-template name="TableHeader">
		<xsl:with-param name="title" select="$title"/>
		<xsl:with-param name="image" select="$image"/>
		<xsl:with-param name="width" select="$width"/>
		<xsl:with-param name="tableClass">titulosTablaSubMain</xsl:with-param>
	</xsl:call-template>
</xsl:template>

<xsl:template name="TableHeader">
	<xsl:param name="title">Table Title</xsl:param>
	<xsl:param name="image"/>
	<xsl:param name="width">100%</xsl:param>
	<xsl:param name="tableClass">titulosTabla</xsl:param>
	-- <xsl:value-of select="$title"/>
</xsl:template>

<xsl:template name="closeNugget">
<xsl:param name="tdClass"/>
</xsl:template>

<xsl:template name="GxOpen">
<xsl:param name="Class"/>
<xsl:param name="Id"/>
</xsl:template>

<!-- Group TEMPLATE -->
<xsl:template match="Group">
	<xsl:value-of select="GroupName"/>
</xsl:template>

<xsl:template name="GetClsPrefix">
<xsl:param name="Class"/>
<xsl:choose>			
			<xsl:when test="$Class=0">trn</xsl:when>
			<xsl:when test="$Class=1">prc</xsl:when>
			<xsl:when test="$Class=2">rpt</xsl:when>
			<xsl:when test="$Class=3">mnu</xsl:when>
			<xsl:when test="$Class=4">wkp</xsl:when>
			<xsl:when test="$Class=5">att</xsl:when>
			<xsl:when test="$Class=6">dv</xsl:when>
			<xsl:when test="$Class=7">tbl</xsl:when>
			<xsl:when test="$Class=8">fld</xsl:when>
			<xsl:when test="$Class=9">mdl</xsl:when>
			<xsl:when test="$Class=10">grp</xsl:when>
			<xsl:when test="$Class=11">dom</xsl:when>
			<xsl:when test="$Class=12">pmt</xsl:when>
			<xsl:when test="$Class=13">wbp</xsl:when>
			<xsl:when test="$Class=17">mbr</xsl:when>
			<xsl:when test="$Class=18">strn</xsl:when>
			<xsl:when test="$Class=19">sprc</xsl:when>
			<xsl:when test="$Class=20">srpt</xsl:when>
			<xsl:when test="$Class=21">swkp</xsl:when>
			<xsl:when test="$Class=22">spmt</xsl:when>
			<xsl:when test="$Class=23">swbp</xsl:when>
			<xsl:when test="$Class=24">smbr</xsl:when>
			<xsl:when test="$Class=25">thm</xsl:when>
			<xsl:when test="$Class=26">sdt</xsl:when>
</xsl:choose>
</xsl:template>

<xsl:template name="GetClsName">
	<xsl:param name="Class"/>
	<xsl:variable name="Classes" select="document('classes.xml')"/>
	<xsl:value-of select="$Classes/Classes/Class[@Id=$Class]/Name"/>
</xsl:template>

<xsl:template match="GenId">
	<xsl:call-template name="GetImgGenerator">
		<xsl:with-param name="GenId"><xsl:value-of select="."/></xsl:with-param>
	</xsl:call-template>
</xsl:template>

<xsl:template name="GetImgGenerator">
	<xsl:param name="GenId"/>
	<xsl:choose>
		<xsl:when test="$GenId=3">Visual Basic</xsl:when>
		<xsl:when test="$GenId=4">DOS</xsl:when>
		<xsl:when test="$GenId=5">RPG</xsl:when>
		<xsl:when test="$GenId=6">Cobol</xsl:when>
		<xsl:when test="$GenId=7">Visual Basic C/S</xsl:when>
		<xsl:when test="$GenId=8">C</xsl:when>
		<xsl:when test="$GenId=9">Visual FoxPro</xsl:when>
		<xsl:when test="$GenId=10">Visual FoxPro C/S</xsl:when>
		<xsl:when test="$GenId=11">Java</xsl:when>
		<xsl:when test="$GenId=12">Java</xsl:when>
		<xsl:when test="$GenId=13">Visual Basic</xsl:when>
		<xsl:when test="$GenId=14">Visual Basic C/S</xsl:when>
		<xsl:when test="$GenId=15">C#</xsl:when>
		<xsl:when test="$GenId=18">C#</xsl:when>
		<xsl:otherwise>Design</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<!-- LinkObjectStd TEMPLATE -->
<xsl:template name="LinkObjectStd">
<xsl:param name="ShowClassAs">icon</xsl:param>
	<xsl:call-template name="LinkObject">
		<xsl:with-param name="ObjCls" select="ObjCls"/>
		<xsl:with-param name="ObjId" select="ObjId"/>
		<xsl:with-param name="ObjName" select="ObjName"/>
		<xsl:with-param name="ObjDesc" select="ObjTitle"/>
		<xsl:with-param name="ShowClassAs" select="$ShowClassAs"/>
	</xsl:call-template>
</xsl:template>

<!-- LinkObject TEMPLATE -->
<xsl:template name="LinkObject">
<xsl:param name="ObjCls"/>
<xsl:param name="ObjId"/>
<xsl:param name="ObjName"/>
<xsl:param name="ObjDesc"/>
<xsl:param name="ObjShortName"/>
<xsl:param name="ShowClassAs">icon</xsl:param>
	<xsl:value-of select="$ObjName"/>
</xsl:template>

<!-- AttriSuper TEMPLATE -->
<xsl:template match="AttriSuper">
	<xsl:if test="Group">
		<xsl:apply-templates select="Group"/>
		<xsl:text>.</xsl:text>
	</xsl:if>
	<xsl:apply-templates select="Attribute"/>
</xsl:template>

<!-- Attribute TEMPLATE -->
<xsl:template match="Attribute">
	<xsl:if test="AttriOrder[text()='Descending']">(</xsl:if>
	<xsl:value-of select="AttriName"/>
	<xsl:if test="AttriOrder[text()='Descending']">)</xsl:if>
</xsl:template>

<xsl:template name="AttTooltip">
<xsl:param name="Description"/>
</xsl:template>

<xsl:template name="PrintType">
	<xsl:param name="Type"/> 
	<xsl:param name="Length">0</xsl:param>
	<xsl:param name="Decimals">0</xsl:param>
	<xsl:call-template name="TypeName">
		<xsl:with-param name="Type"><xsl:value-of select="$Type"/></xsl:with-param>
	</xsl:call-template>
	<xsl:if test="$Length >= 0">
		<xsl:if test="$Type = 'N' or $Type = 'C' or $Type = 'V'">
			<xsl:text>(</xsl:text>
			<xsl:value-of select="$Length"/>
			<xsl:if test="$Decimals > 0 and $Type ='N'">
				<xsl:text>.</xsl:text><xsl:value-of select="$Decimals"/>
			</xsl:if>
			<xsl:text>)</xsl:text>
		</xsl:if>
	</xsl:if>
</xsl:template>

<xsl:template name="TypeName">
	<xsl:param name="Type"/>
	<xsl:choose>
		<xsl:when test="$Type = 'C'">Character </xsl:when>
		<xsl:when test="$Type = 'N'">Numeric </xsl:when>
		<xsl:when test="$Type = 'T'">Datetime </xsl:when>
		<xsl:when test="$Type = 'V'">Varchar </xsl:when>
		<xsl:when test="$Type = 'D'">Date </xsl:when>
		<xsl:when test="$Type = 'L'">Long varchar </xsl:when>
		<xsl:when test="$Type = 'O'">Blob </xsl:when>
		<xsl:when test="$Type = '1'">Boolean </xsl:when>
		<xsl:when test="$Type = 'G'">GUID </xsl:when>
		<xsl:otherwise><xsl:value-of select="$Type"/></xsl:otherwise>
	</xsl:choose>
</xsl:template>

<!-- TOKEN TEMPLATE -->
<xsl:template match="Token"><xsl:value-of select="."/></xsl:template>

<!-- VARIABLE TEMPLATE -->
<xsl:template match="Variable"><xsl:value-of select="VarName"/></xsl:template>

<!-- TABLE TEMPLATE -->
<xsl:template match="Table">
	<xsl:call-template name="GralTable">
		<xsl:with-param name="ShowClassAs">none</xsl:with-param>
	</xsl:call-template>
</xsl:template>

<xsl:template match="Table" mode="icon">
	<xsl:call-template name="GralTable">
		<xsl:with-param name="ShowClassAs">icon</xsl:with-param>
	</xsl:call-template>
</xsl:template>

<xsl:template name="GralTable">
<xsl:param name="ShowClassAs">icon</xsl:param>
		<xsl:if test="TableId">
			<xsl:call-template name="LinkObject">
				<xsl:with-param name="ObjCls">7</xsl:with-param>
				<xsl:with-param name="ObjId" select="TableId"/>
				<xsl:with-param name="ObjName" select="TableName"/>
				<xsl:with-param name="ObjDesc">
					<xsl:value-of select="Description"/>
					<xsl:value-of select="TableDesc"/>
				</xsl:with-param>
				<xsl:with-param name="ObjShortName" select="TableShortName"/>
				<xsl:with-param name="ShowClassAs" select="$ShowClassAs"/>
			</xsl:call-template>
		</xsl:if>
</xsl:template>

<!-- Subtype Group TEMPLATE -->
<xsl:template match="SubtypeGroup">
	<xsl:value-of select="SubtypeGroupName"/>	
</xsl:template>


<!-- Object TEMPLATE -->
<xsl:template match="Object">
	<xsl:call-template name="GralObject">
		<xsl:with-param name="ShowClassAs">prefix</xsl:with-param>
	</xsl:call-template>
</xsl:template>

<xsl:template match="Object" mode="xRef">
	<xsl:call-template name="GralObject">
		<xsl:with-param name="ShowClassAs">none</xsl:with-param>
	</xsl:call-template>
</xsl:template>

<xsl:template match="Object" mode="icon">
	<xsl:call-template name="GralObject">
		<xsl:with-param name="ShowClassAs">icon</xsl:with-param>
	</xsl:call-template>
</xsl:template>


<xsl:template name="GralObject">
<xsl:param name="ShowClassAs">icon</xsl:param>
	<xsl:call-template name="LinkObjectStd">
		<xsl:with-param name="ShowClassAs" select="$ShowClassAs"/>
	</xsl:call-template>
</xsl:template>


<xsl:template name="HtmlHeader">
</xsl:template>

<xsl:template name="HtmlScript">
</xsl:template>

<xsl:template name="ProperyValue">
	<xsl:param name="Name"></xsl:param>
	<xsl:param name="Value"></xsl:param>
	<xsl:value-of select="$Name"/> : <xsl:value-of select="$Value"/>
	
</xsl:template>

<xsl:template name="ChangeObject">
	<xsl:param name="ChgType">none</xsl:param>
	<xsl:choose>
		<xsl:when test="$ChgType = 'N'">
			New
			</xsl:when>
		<xsl:when test="$ChgType = 'U'">
			Upd
			</xsl:when>
		<xsl:when test="$ChgType = 'D'">
			Del
			</xsl:when>
		<xsl:otherwise/>
	</xsl:choose>
</xsl:template>

<!-- WARNINGS -->
<xsl:template match="Warnings">
	-- Warnings --
	<xsl:for-each select="Warning">
	<xsl:sort select="Message/Token"/> 
	<xsl:text>!! </xsl:text><xsl:value-of select="MsgCode"/><xsl:text>: </xsl:text><xsl:apply-templates select="Message"/>
		<xsl:apply-templates select="Location">
			<xsl:with-param name="ObjId" select="../../Object/ObjId"/>
			<xsl:with-param name="ObjCls" select="../../Object/ObjCls"/>
		</xsl:apply-templates>
	<xsl:text>
	</xsl:text>
	</xsl:for-each> 
	-- End Warnings --
	<xsl:text>
	</xsl:text>
</xsl:template>		

		
<!-- ERRORS -->
<xsl:template match="Errors">
	-- Errors --
	<xsl:for-each select="Error">
	<xsl:sort select="Message/Token"/> 
	<xsl:text>!! </xsl:text><xsl:value-of select="MsgCode"/><xsl:text>: </xsl:text><xsl:apply-templates select="Message"/>
	<xsl:apply-templates select="Location">
			<xsl:with-param name="ObjId" select="../../Object/ObjId"/>
			<xsl:with-param name="ObjCls" select="../../Object/ObjCls"/>
		</xsl:apply-templates>
	<xsl:text>
	</xsl:text>
	</xsl:for-each> 
	-- End Errors --
</xsl:template>	
<!-- END ERRORS -->

<xsl:template match="Location">
	<xsl:param name="ObjId"/>
	<xsl:param name="ObjCls"/>
	<xsl:if test="Type">
		<xsl:text> (</xsl:text>
			<xsl:if test="$ObjId != '' and $ObjCls != ''">
				<xsl:attribute name="class">gxlink</xsl:attribute>
				<xsl:attribute name="onclick">
					<xsl:text>gxopenpart(</xsl:text>
					<xsl:value-of select="$ObjCls"/>
					<xsl:text>,</xsl:text>
					<xsl:value-of select="$ObjId"/>
					<xsl:text>,'</xsl:text>
					<xsl:value-of select="Type"/>
					<xsl:text>','</xsl:text>
					<xsl:value-of select="Line"/>
					<xsl:text>')</xsl:text>
				</xsl:attribute>
			</xsl:if>
			
			<xsl:value-of select="Type"/>
			<xsl:if test="Line">
				<xsl:text>, Line: </xsl:text>
				<xsl:value-of select="Line"/>
			</xsl:if>
		<xsl:text>)</xsl:text>
	</xsl:if>
</xsl:template>

<xsl:template match="Message">
	<!-- Copied from msg.xsl -->
	<xsl:for-each select="Attribute|Subtype|Token|Variable|Table|Index|SubtypeGroup|Sp">
		<xsl:apply-templates select="."/>
	</xsl:for-each>
</xsl:template>

<xsl:template name="ProcessList">
	<xsl:param name="Sep"></xsl:param>
	<xsl:for-each select="Token|Attribute|Variable|Object|Table|Parameter|SubtypeGroup">    
<!--		<xsl:sort select="AttriName"/>   -->  
		<xsl:choose> 
			<xsl:when test="text()[.=$Sep]"></xsl:when>
			<xsl:when test="starts-with(text(),$Sep) and string-length($Sep) > 0">
				<xsl:value-of select="$Sep"/><xsl:value-of select="substring-after(text(),$Sep)"/>
			</xsl:when>
			<xsl:when test="starts-with(text(),';')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),'.')">
				<xsl:apply-templates select="." />
			</xsl:when>  
			<xsl:when test="starts-with(text(),')')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),' ')">
				<xsl:apply-templates select="." />
			</xsl:when>			
			<xsl:when test="position() != 1">
				<xsl:value-of select="$Sep"/><xsl:text> </xsl:text><xsl:apply-templates select="." />
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="." />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:for-each>
</xsl:template>


<xsl:template name="ProcessListOrdered">
	<xsl:param name="Sep"></xsl:param>
	<xsl:for-each select="Attribute|Variable|Object|Table|Parameter|SubtypeGroups">    
		<xsl:sort select="AttriName"/>     
		<xsl:choose> 
			<xsl:when test="text()[.=$Sep]"></xsl:when>
			<xsl:when test="starts-with(text(),$Sep) and string-length($Sep) > 0">
				<xsl:value-of select="$Sep"/><xsl:value-of select="substring-after(text(),$Sep)"/>
			</xsl:when>
			<xsl:when test="starts-with(text(),';')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),'.')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),')')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),' ')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="position() != 1">
				<xsl:value-of select="$Sep"/><xsl:text> </xsl:text><xsl:apply-templates select="." />
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="." />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:for-each>
</xsl:template>

<!--13/02/06 ProcessLista para separar las constraint que incluyen AND y comparar con 7.5 --> 
<xsl:template name="ProcessListConstraint">
	<xsl:param name="Sep"></xsl:param>
	<xsl:for-each select="Token|Attribute|Variable|Object|Table|Parameter|SubtypeGroup">    
<!--		<xsl:sort select="AttriName"/>   -->  
		<xsl:choose> 
			<xsl:when test="text()[.=$Sep]"></xsl:when>
			<xsl:when test="starts-with(text(),$Sep) and string-length($Sep) > 0">
				<xsl:value-of select="$Sep"/><xsl:value-of select="substring-after(text(),$Sep)"/>
			</xsl:when>
			<xsl:when test="starts-with(text(),';')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),'.')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),')')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),' ')">
				<xsl:apply-templates select="." />
			</xsl:when>
			<xsl:when test="starts-with(text(),'and')">
				<xsl:call-template name="NewLine2"/>
			</xsl:when>			
			<xsl:when test="position() != 1">
				<xsl:value-of select="$Sep"/><xsl:text> </xsl:text><xsl:apply-templates select="." />
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="." />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:for-each>
</xsl:template>

<xsl:template name="TitledRules">
	<xsl:param name="Title">Title</xsl:param>
	<xsl:param name="Node">AfterConfirmRules</xsl:param>
	-- <xsl:value-of select="$Title"/> --
	
	<xsl:variable name="CurrentNode" select="."/>
	<xsl:for-each select="$CurrentNode/*[name() = $Node]">
		<xsl:apply-templates select="."/>
	</xsl:for-each>
</xsl:template>

<!-- Token TEMPLATE -->
<xsl:template match="Token">
	<xsl:value-of select="."/>
</xsl:template>

<!-- Sp TEMPLATE -->
<xsl:template match="Sp">
	<xsl:text> </xsl:text>
</xsl:template>
<!-- FormulaDef TEMPLATE -->
<xsl:template match="FormulaDef">
	<xsl:for-each select="Token|Attribute|Object">
		<xsl:apply-templates select="." />
	</xsl:for-each>
</xsl:template>

</xsl:stylesheet>