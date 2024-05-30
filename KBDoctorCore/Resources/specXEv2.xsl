<?xml version='1.0'?>
<xsl:stylesheet version="1.0"
              	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <xsl:text>Text Format Navigation </xsl:text>
    <xsl:apply-templates select="ObjectSpec"/>
  </xsl:template>
  <!-- ***** INCLUDE.XML (COMIENZA) -->
  <xsl:variable name="Product" select="'GeneXus'" />
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
    <xsl:param name="ID"/>
    <xsl:param name="Title"/>
    <xsl:param name="RowTemplate"/>
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
				<xsl:sort select="./Table/TableName"/>
				12/01/05 Para que los prompt se listen ordenados 
				<xsl:apply-templates select="." mode="TableRow"/>
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
    <xsl:variable name="Classes">
      <Classes>
        <Class Id='0'>
          <Name>Transaction</Name>
        </Class>
        <Class Id='1'>
          <Name>Procedure</Name>
        </Class>
        <Class Id='2'>
          <Name>Report</Name>
        </Class>
        <Class Id='3'>
          <Name>Menu</Name>
        </Class>
        <Class Id='4'>
          <Name>Work Panel</Name>
        </Class>
        <Class Id='5'>
          <Name>Attribute</Name>
        </Class>
        <Class Id='-5'>
          <Name>Formula</Name>
        </Class>
        <Class Id='6'>
          <Name>DataView</Name>
        </Class>
        <Class Id='7'>
          <Name>Table</Name>
        </Class>
        <Class Id='8'>
          <Name>Folder</Name>
        </Class>
        <Class Id='9'>
          <Name>Model</Name>
        </Class>
        <Class Id='10'>
          <Name>Group</Name>
        </Class>
        <Class Id='11'>
          <Name>Domain</Name>
        </Class>
        <Class Id='12'>
          <Name>Prompt</Name>
        </Class>
        <Class Id='13'>
          <Name>Web Panel</Name>
        </Class>
        <Class Id='14'>
          <Name>External Program</Name>
        </Class>
        <Class Id='17'>
          <Name>Menu Bar</Name>
        </Class>
        <Class Id='18'>
          <Name>Style Transaction</Name>
        </Class>
        <Class Id='19'>
          <Name>Style Procedure</Name>
        </Class>
        <Class Id='20'>
          <Name>Style Report</Name>
        </Class>
        <Class Id='21'>
          <Name>Style Work Panel</Name>
        </Class>
        <Class Id='22'>
          <Name>Style Prompt</Name>
        </Class>
        <Class Id='23'>
          <Name>Style Web Panel</Name>
        </Class>
        <Class Id='24'>
          <Name>Style MenuBar</Name>
        </Class>
        <Class Id='25'>
          <Name>Theme</Name>
        </Class>
        <Class Id='26'>
          <Name>Structured Data Type</Name>
        </Class>
      </Classes>
    </xsl:variable>
    <xsl:value-of select="$Classes/Classes/Class[@Id=$Class]/Name"/>
  </xsl:template>

  <xsl:template match="GenId">
    <xsl:call-template name="GetImgGenerator">
      <xsl:with-param name="GenId">
        <xsl:value-of select="."/>
      </xsl:with-param>
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
      <xsl:with-param name="Type">
        <xsl:value-of select="$Type"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:if test="$Length >= 0">
      <xsl:if test="$Type = 'N' or $Type = 'C' or $Type = 'V'">
        <xsl:text>(</xsl:text>
        <xsl:value-of select="$Length"/>
        <xsl:if test="$Decimals > 0 and $Type ='N'">
          <xsl:text>.</xsl:text>
          <xsl:value-of select="$Decimals"/>
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
      <xsl:otherwise>
        <xsl:value-of select="$Type"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- TOKEN TEMPLATE -->
  <xsl:template match="Token">
    <xsl:value-of select="."/>
  </xsl:template>
  <!-- VARIABLE TEMPLATE -->
  <xsl:template match="Variable">
    <xsl:value-of select="VarName"/>
  </xsl:template>
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
    <xsl:param name="Name"/>
    <xsl:param name="Value"/>
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
      <xsl:text>!! </xsl:text>
      <xsl:value-of select="MsgCode"/>
      <xsl:text>: </xsl:text>
      <xsl:apply-templates select="Message"/>
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
      <xsl:text>!! </xsl:text>
      <xsl:value-of select="MsgCode"/>
      <xsl:text>: </xsl:text>
      <xsl:apply-templates select="Message"/>
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
    <xsl:param name="Sep"/>
    <xsl:for-each select="Token|Attribute|Variable|Object|Table|Parameter|SubtypeGroup">
      <!--		<xsl:sort select="AttriName"/>   -->
      <xsl:choose>
        <xsl:when test="text()[.=$Sep]"/>
        <xsl:when test="starts-with(text(),$Sep) and string-length($Sep) > 0">
          <xsl:value-of select="$Sep"/>
          <xsl:value-of select="substring-after(text(),$Sep)"/>
        </xsl:when>
        <xsl:when test="starts-with(text(),';')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),'.')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),')')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),' ')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="position() != 1">
          <xsl:value-of select="$Sep"/>
          <xsl:text> </xsl:text>
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="."/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="ProcessListOrdered">
    <xsl:param name="Sep"/>
    <xsl:for-each select="Attribute|Variable|Object|Table|Parameter|SubtypeGroups">
      <xsl:sort select="AttriName"/>
      <xsl:choose>
        <xsl:when test="text()[.=$Sep]"/>
        <xsl:when test="starts-with(text(),$Sep) and string-length($Sep) > 0">
          <xsl:value-of select="$Sep"/>
          <xsl:value-of select="substring-after(text(),$Sep)"/>
        </xsl:when>
        <xsl:when test="starts-with(text(),';')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),'.')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),')')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),' ')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="position() != 1">
          <xsl:value-of select="$Sep"/>
          <xsl:text> </xsl:text>
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="."/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>
  <!--13/02/06 ProcessLista para separar las constraint que incluyen AND y comparar con 7.5 -->
  <xsl:template name="ProcessListConstraint">
    <xsl:param name="Sep"/>
    <xsl:for-each select="Token|Attribute|Variable|Object|Table|Parameter|SubtypeGroup">
      		<xsl:sort select="AttriName"/>   
      <xsl:choose>
        <xsl:when test="text()[.=$Sep]"/>
        <xsl:when test="starts-with(text(),$Sep) and string-length($Sep) > 0">
          <xsl:value-of select="$Sep"/>
          <xsl:value-of select="substring-after(text(),$Sep)"/>
        </xsl:when>
        <xsl:when test="starts-with(text(),';')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),'.')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),')')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),' ')">
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:when test="starts-with(text(),'and')">
          <xsl:call-template name="NewLine2"/>
        </xsl:when>
        <xsl:when test="position() != 1">
          <xsl:value-of select="$Sep"/>
          <xsl:text> </xsl:text>
          <xsl:apply-templates select="."/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="."/>
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
      <xsl:apply-templates select="."/>
    </xsl:for-each>
  </xsl:template>

  <!--  **************************************** INCLUDE.XML  (FIN)  -->


  <!-- *********** FORMULAS INICIO ****** -->
  <!-- FORMULAS TEMPLATES -->

  <!-- AGGREGATE SELECT TEMPLATE -->
  <xsl:template match="AggSelFormulas">
    <xsl:if test="Formula">
      -- Formulas:
      <xsl:for-each select="Formula">
        <xsl:sort select="FormulaAttri/Attribute/AttriName"/>
        <xsl:choose>
          <xsl:when test="FormulaType='aggsel'">
            <xsl:text>Navigation to evaluate: </xsl:text >
            <xsl:apply-templates select="FormulaAttri"/>
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

  <!-- *********** FORMULAS FIN ****** -->

  <!-- *********** genexusSpecifc90.xsl ini ****** -->
  <!-- DYNAMIC COMBO NAVIGATION -->
  <xsl:template match="DynamicCombo" mode="TableRow">
    <xsl:value-of select="DynamicComboName"/>:
    <xsl:apply-templates select="Table" mode="icon"/>
    <xsl:value-of select="DynamicComboDesc"/>
    <xsl:value-of select="DynamicComboSorted"/>
    <xsl:value-of select="DynamicComboValue"/>
  </xsl:template>

  <xsl:template match="DynamicCombos">
    <xsl:call-template name="GenerateTable">
      <xsl:with-param name="ID">DynamicCombos</xsl:with-param>
      <xsl:with-param name="Title">Dynamic Combos</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template match="Suggests">
    <xsl:call-template name="GenerateTable">
      <xsl:with-param name="ID">Suggests</xsl:with-param>
      <xsl:with-param name="Title">Suggestions</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template match="Suggest" mode="TableRow">
    <xsl:text>
	Control: </xsl:text>
    <xsl:apply-templates select="ControlName"/>
    <xsl:text>
	Suggest Type: </xsl:text>
    <xsl:value-of select="SuggestType"/>
    <xsl:text>
	Navigation:
	</xsl:text>
    <xsl:apply-templates select="Navigation"/>
  </xsl:template>

  <!-- MENU BAR -->
  <xsl:template match="MenuBar">
    <xsl:apply-templates select="MenuBarOption"/>
  </xsl:template>

  <xsl:template match="MenuBarOption">
    <xsl:value-of select="Name"/>
    <xsl:text>   </xsl:text>
    <xsl:value-of select="Event"/>
    <ol>
      <xsl:apply-templates select="MenuBarOption"/>
    </ol>
  </xsl:template>
  <!-- END MENUBAR -->

  <xsl:template match="MenuOption" mode="TableRow">
    <xsl:value-of select="OptionNumber"/>
    <xsl:apply-templates select="OptionCls"/>
    <xsl:value-of select="OptionName"/>
    <xsl:value-of select="OptionDesc"/>
    <xsl:value-of select="OptionCall"/>
  </xsl:template>

  <xsl:template match="MenuOptions">
    <xsl:call-template name="GenerateTable">
      <xsl:with-param name="ID">MenuOptions</xsl:with-param>
      <xsl:with-param name="Title">Menu Options</xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="Prompts">
    <xsl:call-template name="GenerateTable">
      <xsl:with-param name="ID">Prompts</xsl:with-param>
      <xsl:with-param name="Title">Prompts</xsl:with-param>
    </xsl:call-template>
    -- End Prompts --
  </xsl:template>

  <xsl:template match="Prompt" mode="TableRow">
    <xsl:call-template name="NewLine"/>
    <xsl:apply-templates select="Table" mode="icon"/>
    <xsl:text> </xsl:text>
    <xsl:if test="Object">
      <xsl:apply-templates select="Object" mode="icon"/>
      <xsl:if test="PromptType">
        <xsl:apply-templates select="PromptType"/>
      </xsl:if>
    </xsl:if>

    <xsl:text> </xsl:text>
    <xsl:if test="ProgramName">
      <xsl:value-of select="ProgramName"/>
      <xsl:if test="PromptType">
        <xsl:apply-templates select="PromptType"/>
      </xsl:if>
    </xsl:if>

    <xsl:text> </xsl:text>
    <xsl:apply-templates select="InputParameters"/>
    <xsl:text> </xsl:text>
    <xsl:apply-templates select="OutputParameters"/>
  </xsl:template>



  <!-- PROMPT TYPE TEMPLATE -->
  <xsl:template match="PromptType">
    <xsl:if test="text()[.='User']">
      <xsl:call-template name="RenderImage">
        <xsl:with-param name="Id">User</xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Prompt">
    <xsl:apply-templates/>
  </xsl:template>

  <!-- *********** genexusSpecifc90.xsl FIN ****** -->





  <!-- FROM FORMULA TEMPLATE -->
  <xsl:template match="FromFormula|FromValue">
    <xsl:apply-templates select="*"/>
  </xsl:template>
  <!-- RULES TEMPLATES -->
  <!-- RULES-->
  <xsl:template match="StandAloneRules|StandAloneWithModeRules|BaseTableRule|AfterConfirmRules|AfterInsertRules|AfterUpdateRules|BeforeConfirmRules|BeforeInsertRules|BeforeUpdateRules|BeforeDeleteRules|BeforeTrnRules">
    <xsl:apply-templates select="Action"/>
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
        <xsl:text> </xsl:text>
        <xsl:apply-templates select="FormulaExpression"/>
      </xsl:when>
      <xsl:when test="ActionType[.='ReadTable' or .='ReadBaseTable' or .='ReadCKey']">
        <xsl:text>
					READ  </xsl:text>
        <xsl:apply-templates select="Table"/>
        <xsl:if test="ActionType[.='ReadCKey']">
          <xsl:text> UNIQUE</xsl:text>
        </xsl:if>
        <xsl:if test="JoinType[.='Outer']">
          <xsl:text> allowing nulls</xsl:text>
        </xsl:if>
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
        <xsl:if test="Condition/*">
          IF <xsl:apply-templates select="Condition"/>
        </xsl:if>
      </xsl:when>
      <xsl:when test="ActionType[.='SubType']">
        <xsl:apply-templates select="SubtypeAction"/>
      </xsl:when>
      <xsl:when test="ActionType[.='VerticalFormulas']">

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
    Join location: <xsl:choose>
      <xsl:when test="text()[.='1']">Server</xsl:when>
      <xsl:otherwise>Client</xsl:otherwise>
    </xsl:choose>
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
      <xsl:if test="Table/TableName">
        <xsl:apply-templates select="Table"/>.
      </xsl:if>
      <xsl:apply-templates select="AttributeFrom"/>
      <xsl:text>; </xsl:text>
    </xsl:for-each>
  </xsl:template>
  <!-- END JOIN CONDITIONS -->
  <!-- END RULE TEMPLATES -->
  <xsl:template match="BaseTable">
    <xsl:apply-templates select="Table"/>
  </xsl:template>
  <xsl:template match="Generator">
    <xsl:apply-templates select="GenId"/>
  </xsl:template>
  <!--  Standa-rd Templates  -->
  
	<xsl:template match="ObjClsName">
		<xsl:variable name="Classes"
            			select="document('classes.xml')"/>
		<xsl:variable name="ClassName">
			<xsl:value-of select="."/>
		</xsl:variable>
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
    Name: <xsl:apply-templates select="Object"
                  mode="icon"/>
    Description : <xsl:value-of select="Object/ObjDesc"/>
    <xsl:if test="Result!='genreq'">
      Status: <xsl:apply-templates select="Result"/>
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
      Parameters:	<xsl:apply-templates select="Parameters"/>
      <xsl:text>
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
        <xsl:apply-templates select="Object"
                   					mode="xRef"/>
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
    <xsl:apply-templates select="NavigationTree/Table"
                   			mode="Tree"/>
    <xsl:if test="NavigationConditions/*">
      Where <xsl:apply-templates select="NavigationConditions"/>
    </xsl:if>
    Order <xsl:apply-templates select="NavigationOrder"/>
  </xsl:template>
  <xsl:template match="NavigationConditions">
    <xsl:for-each select="Condition">
      <xsl:if test="position() != 1"> And </xsl:if>
      <xsl:call-template name="ProcessList"/>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="NavigationOrder">
    <xsl:apply-templates select="Order"/>
    <xsl:apply-templates select="IndexName"/>
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
    <xsl:apply-templates select="Table"
                   			mode="Tree"/>
  </xsl:template>
  <xsl:template match="KeyAttributes">
    <xsl:text>(</xsl:text>
    <xsl:call-template name="ProcessList">
      <xsl:with-param name="Sep">,</xsl:with-param>
    </xsl:call-template>
    <xsl:text>)</xsl:text>
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
      <xsl:when test="Type[.='Aggregate']">
        <xsl:apply-templates select="Expression"/>
      </xsl:when>
      <xsl:when test="Type[.='InsertWSubSelect']">Insert with subselect</xsl:when>
      <xsl:when test="Type[.='Update']">Update</xsl:when>
      <xsl:when test="Type[.='FirstRows']">
        First <xsl:value-of select="MaxRows"/> record(s)
      </xsl:when>
      <xsl:when test="Type[.='ServerPaging']">Server Paging</xsl:when>
      <xsl:otherwise>Unknown optimization</xsl:otherwise>
    </xsl:choose>
    <xsl:call-template name="NewLine"/>
  </xsl:template>
  <xsl:template match="Unique">
    Unique:
    <xsl:call-template name="ProcessList"/>
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
        <xsl:apply-templates select="Order"/>
        <xsl:text> </xsl:text>
        <xsl:choose>
          <xsl:when test="Condition/*">
            <xsl:text>WHEN </xsl:text>
            <xsl:apply-templates select="Condition"/>
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
          <xsl:attribute name="id">
            gx<xsl:value-of select="BaseTable/Table/TableId"/>
          </xsl:attribute>
          <xsl:call-template name="TableHeader">
            <xsl:with-param name="id">
              gx<xsl:value-of select="BaseTable/Table/TableId"/>
            </xsl:with-param>
            <xsl:with-param name="title">
              <xsl:apply-templates select="LevelType"/>
              <xsl:text> </xsl:text>
              <xsl:apply-templates select="BaseTable"/>
              <xsl:text> </xsl:text>
              <xsl:apply-templates select="LevelBeginRow"/>
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
        Order:	<xsl:apply-templates select="Order"/>
        <xsl:text> </xsl:text>
        <xsl:call-template name="UsedIndex"/>
      </xsl:if>
    </xsl:if>
    <xsl:if test="Unique">
      <xsl:apply-templates select="Unique"/>
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
    <xsl:if test="AfterAttributeRules/Action|AfterAttributeRules/NonTriggeredActions">
      <xsl:call-template name="TitledRules">
        <xsl:with-param name="Title">After Attribute Rules</xsl:with-param>
        <xsl:with-param name="Node">AfterAttributeRules</xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <xsl:apply-templates select="Rules"/>
    <xsl:if test="BeforeConfirmRules/Action|BeforeConfirmRules/NonTriggeredActions">
      <xsl:call-template name="TitledRules">
        <xsl:with-param name="Title">Before Validate Rules</xsl:with-param>
        <xsl:with-param name="Node">BeforeConfirmRules</xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="AfterConfirmRules/Action|AfterConfirmRules/NonTriggeredActions">
      <xsl:call-template name="TitledRules">
        <xsl:with-param name="Title">After Validate Rules</xsl:with-param>
        <xsl:with-param name="Node">AfterConfirmRules</xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="AfterDisplayRules/Action|AfterDisplayRules/NonTriggeredActions">
      <xsl:call-template name="TitledRules">
        <xsl:with-param name="Title">After Display Rules</xsl:with-param>
        <xsl:with-param name="Node">AfterDisplayRules</xsl:with-param>
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
    <xsl:if test="BeforeTrnRules/Action|BeforeTrnRules/NonTriggeredActions">
      <xsl:call-template name="TitledRules">
        <xsl:with-param name="Title">Before Complete Rules</xsl:with-param>
        <xsl:with-param name="Node">BeforeTrnRules</xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="AfterTrnRules/Action|AfterTrnRules/NonTriggeredActions">
      <xsl:call-template name="TitledRules">
        <xsl:with-param name="Title">After Complete Rules</xsl:with-param>
        <xsl:with-param name="Node">AfterTrnRules</xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="AfterLevelRules/Action|AfterLevelRules/NonTriggeredActions">
      <xsl:call-template name="TitledRules">
        <xsl:with-param name="Title">After Level Rules</xsl:with-param>
        <xsl:with-param name="Node">AfterLevelRules</xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="Event">
      <xsl:apply-templates select="Event"/>
    </xsl:if>
    <xsl:if test="CALL">
      <xsl:text>   </xsl:text>
      <xsl:apply-templates select="CALL"/>
    </xsl:if>
    <xsl:if test="SUBMIT">
      <xsl:text>   </xsl:text>
      <xsl:apply-templates select="SUBMIT"/>
    </xsl:if>
    <xsl:if test="Binding">
      <xsl:text>   </xsl:text>
      <xsl:apply-templates select="Binding"/>
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
        <xsl:apply-templates select="."/>
        <xsl:apply-templates select="KeyAttributes"/>
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
  <xsl:template match="AttriType|AttriOldType"
            		mode="Properties">
    <xsl:for-each select="Properties/Property">
      <xsl:apply-templates select="."
                   				mode="Attribute"/>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="Property"
            		mode="Attribute">
    <xsl:if test="Value[.='Yes']">
      <xsl:value-of select="Name"/> = <xsl:value-of select="Value"/>
    </xsl:if>
  </xsl:template>
  <xsl:template match="AttriType|AttriOldType">
    <xsl:call-template name="PrintType">
      <xsl:with-param name="Type">
        <xsl:value-of select="DataType"/>
      </xsl:with-param>
      <xsl:with-param name="Length">
        <xsl:value-of select="Presicion"/>
      </xsl:with-param>
      <xsl:with-param name="Decimals">
        <xsl:value-of select="Scale"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template match="FormulaName">
    <xsl:call-template name="ProcessList"/>
    <xsl:if test="Attribute|Variable"> = </xsl:if>
  </xsl:template>
  <xsl:template match="ConditionalConstraint">
    <xsl:apply-templates select="Constraint"/>
    <xsl:text> WHEN </xsl:text>
    <xsl:apply-templates select="Condition"/>
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
      <xsl:text>[</xsl:text>
      <xsl:value-of select="."/>
      <xsl:text>]</xsl:text>
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
      <xsl:otherwise>
        <xsl:value-of select="."/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- END STATUS TEMPLATE -->
  <xsl:template match="OptimizedWhere">
    <xsl:if test="../LevelType != 'Break'">
      Navigation filters:
      Start from: <xsl:apply-templates select="StartFrom"/>
      Loop while: <xsl:apply-templates select="LoopWhile"/>
      <xsl:call-template name="NewLine"/>
    </xsl:if>
  </xsl:template>
  <!-- ENDFOREACH -->
  <xsl:template match="NonOptimizedWhere">
    <xsl:if test="Condition or ConditionalConstraint">
      Constraints:
      <xsl:for-each select="Condition|ConditionalConstraint">
        <xsl:apply-templates select="."/>
        <xsl:call-template name="NewLine2"/>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <xsl:template match="StartFrom">
    <xsl:for-each select="Condition">
      <xsl:call-template name="ProcessList"/>
      <xsl:text>  </xsl:text>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="LoopWhile">
    <xsl:for-each select="Condition">
      <xsl:call-template name="ProcessList"/>
      <xsl:text>  </xsl:text>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="FormulaGivenAttris">
    <xsl:apply-templates/>
  </xsl:template>
  <xsl:template match="Formulas">
    <xsl:apply-templates/>
  </xsl:template>
  <xsl:template match="DynamicLoads">
    <xsl:apply-templates/>
  </xsl:template>
  <!-- 03/01/06 cambio para mostrar las tablas ordenadas por nombre en cada nivel.  -->
  <xsl:template match="Tables">
    <xsl:for-each select="Table">
      <xsl:sort select="TableName"/>
      <xsl:apply-templates select="."
                   				mode="Tree"/>
    </xsl:for-each>
  </xsl:template>
  <!-- tableSTOUPDATE TEMPLATE -->
  <xsl:template match="TablesToUpdate">
    <xsl:for-each select="TableToUpdate">
      <xsl:text>
				* </xsl:text>
      <xsl:apply-templates select="."/>
    </xsl:for-each>
    <xsl:call-template name="NewLine0"/>
  </xsl:template>
  <xsl:template match="TableToUpdate">
    <xsl:apply-templates select="TableAction"/>
    <xsl:apply-templates select="Table"/>
    <xsl:if test="AttrisToUpdate/Attribute">
      <xsl:text> (</xsl:text>
      <xsl:apply-templates select="AttrisToUpdate"/>
      <xsl:text>)</xsl:text>
    </xsl:if>
    <xsl:if test="UpdateRedundancyCall">
      <xsl:call-template name="NewLine"/>
      <xsl:text>Update Redundancy:</xsl:text>
      <xsl:apply-templates select="UpdateRedundancyCall"/>
    </xsl:if>
  </xsl:template>
  <xsl:template match="UpdateRedundancyCall">
    <xsl:if test="ProgramName">
      CALL ( <xsl:value-of select="ProgramName"/>,<xsl:value-of select="Parameters"/>	)
    </xsl:if>
    <xsl:if test="Token">
      CALL ( <xsl:value-of select="Token"/>,<xsl:value-of select="Parameters"/>	)
    </xsl:if>
  </xsl:template>
  <xsl:template match="TableAction">
    <xsl:choose>
      <xsl:when test="text()[.='insert']">Insert into </xsl:when>
      <xsl:when test="text()[.='update']">Update </xsl:when>
      <xsl:when test="text()[.='delete']">Delete from </xsl:when>
      <xsl:otherwise/>
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
  <xsl:template match="TableName">
    <xsl:apply-templates/>
  </xsl:template>
  <!-- SYNCHRONIZE TEMPLATE -->
  <xsl:template match="Synchronize">
    Load into <xsl:value-of select="text()"/>
  </xsl:template>
  <!-- EVENT TEMPLATE -->
  <xsl:template match="Event">
    <xsl:if test="ImplicitForEach|Level|CALL|SUBMIT|Binding">
      <xsl:call-template name="NewLine"/>
      <xsl:choose>
        <xsl:when test="EventType[text()='Subrutine']">Subroutine</xsl:when>
        <xsl:otherwise>Event </xsl:otherwise>
      </xsl:choose>
      <xsl:value-of select="EventName"/>
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
        <xsl:apply-templates select="ProgramName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="ProcessList">
          <xsl:with-param name="Sep"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>.Call( <xsl:apply-templates select="Parameters"/>)
    <xsl:if test="IF/*">
      IF
      <xsl:apply-templates select="IF"/>
    </xsl:if>
  </xsl:template>
  <xsl:template match="ProgramName">
    <xsl:call-template name="ProcessList">
      <xsl:with-param name="Sep"/>
    </xsl:call-template>
  </xsl:template>
  <xsl:template match="SUBMIT">
    <xsl:choose>
      <xsl:when test="ProgramName">
        <xsl:value-of select="ProgramName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="ProcessList">
          <xsl:with-param name="Sep"/>
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
      <xsl:apply-templates select="."/>
      <xsl:text> </xsl:text>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="Table"
            		mode="Tree">
    <!-- Join type -->
    <xsl:choose>
      <xsl:when test="JoinType[.='Outer']">~</xsl:when>
      <xsl:otherwise>=</xsl:otherwise>
    </xsl:choose>
    <xsl:apply-templates select="TableName"/>
    <xsl:apply-templates select="KeyAttributes"/>
    <xsl:if test="Into/Attribute">
      <xsl:text>
				INTO </xsl:text>
      <xsl:apply-templates select="Into"/>
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
