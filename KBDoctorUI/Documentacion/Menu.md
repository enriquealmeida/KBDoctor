# Menú publicado de KBDoctor

KBDoctor es una extensión de GeneXus. El menú que ve el usuario sale de `KBDoctorUI/KBDoctor.package`, embebido en el ensamblado. Se inserta en **Tools > KBDoctor**.

Los textos del menú salen de `KBDoctorUI/StringResources.resx`, con la misma clave que el id del comando. Casi todas las opciones quedan deshabilitadas hasta que hay una KB abierta. **Help** y **About KBDoctor** están siempre habilitadas.

`GeneXusPackage.package` conserva un menú anterior y no se embebe en el proyecto. No es el menú que se publica.

`Build object and references` aparece dos veces: en **Fix / Cleanup > Objects** y en **Reports & Inventory > Objects**. Es el mismo comando.

---

## Diagnostics

Informes de problemas. No modifican la KB, salvo donde se indica.

### Attributes

| Opción | Id | Qué hace |
| --- | --- | --- |
| Attributes without domain | `AttWithNoDomain` | Lista atributos sin dominio. |
| Attributes with incomplete description/title | `AttWithoutDescription` | Lista atributos con descripción o título incompletos. |
| Attributes used in one transaction only | `AttInOneTrnOnly` | Lista atributos usados en una sola transacción. |
| Where is this attribute updated? | `AttUpdated` | Muestra dónde se actualiza un atributo. |
| CHAR attributes that should be VARCHAR | `AttCharToVarchar` | Lista atributos CHAR candidatos a pasar a VARCHAR. |
| VARCHAR attributes that should be CHAR | `AttVarcharToChar` | Lista atributos VARCHAR candidatos a pasar a CHAR. |
| Key attributes using VARCHAR | `AttKeyVarchar` | Lista claves definidas como VARCHAR. |

### Tables

| Opción | Id | Qué hace |
| --- | --- | --- |
| Tables with incomplete description | `TablesWithNoDescription` | Lista tablas con descripción incompleta. |
| Subtype groups with incomplete description | `GroupWithNoDescription` | Lista grupos de subtipos con descripción incompleta. |
| Indexes with unreferenced attributes | `IndexWithNotRefAtt` | Lista índices que incluyen atributos no referenciados. |

### Objects

| Opción | Id | Qué hace |
| --- | --- | --- |
| Unreferenced objects in user modules | `ListUnreferencedObjectsInUserModules` | Lista objetos de módulos de usuario que nadie referencia. |
| Parm rules without in/out/inout | `ObjectsWithoutInOut` | Lista reglas `parm` sin `in:`, `out:` o `inout:`. |
| Commit call tree | `TreeCommit` | Arma el árbol de llamadas relacionado con `commit`. |
| Procedures updating attributes | `ObjectsUpdateAttribute` | Lista procedimientos que actualizan atributos. |
| Review objects | `ReviewObjects` | Revisa los objetos elegidos y estima deuda técnica en minutos. |
| Variables not based on attributes | `VariablesNotBasedOnAttributes` | Lista variables que no se basan en un atributo. |

### Navigation

| Opción | Id | Qué hace |
| --- | --- | --- |
| Objects with warnings/errors | `ListObjWarningsErrors` | Lista objetos con warnings o errores de navegación. |
| Similar navigations | `ListObjSimilarNavigation` | Lista navegaciones parecidas entre objetos. |

### Database

| Opción | Id | Qué hace |
| --- | --- | --- |
| Attributes with Nullable = Compatible | `ListTableWithAttributeNullableCompatible` | Lista atributos con `Nullable = Compatible`. |
| Compare nullable: GeneXus vs database | `ScriptToCompareNULLABLE_GXvsDB` | Genera un script para comparar nulabilidad entre GeneXus y la base. |
| Compare data types: GeneXus vs database | `ScriptToCompareNULLABLE_GXvsDB2` | Genera un script para comparar tipos de datos entre GeneXus y la base. |

### Modules

| Opción | Id | Qué hace |
| --- | --- | --- |
| Objects using tables from another module | `ListObjectsWithTableInOtherModule` | Lista objetos que usan tablas de un módulo distinto al suyo. |

### UI / Frontend

| Opción | Id | Qué hace |
| --- | --- | --- |
| Web forms using Abstract Editor | `WebFormToAbstractEditor` | Lista web forms que usan el editor abstracto. |
| Improve control classes | `ImproveControlClasses` | Revisa clases de controles para proponer mejoras. |
| Audit Web Panels for Smooth UX | `AuditWebPanelsSmoothUX` | Audita Web Panels respecto de Smooth UX. |

---

## Fix / Cleanup

Acciones que cambian objetos, atributos o propiedades de la KB.

### Attributes

| Opción | Id | Qué hace |
| --- | --- | --- |
| Replace domain | `ReplaceDomain` | Reemplaza el dominio de atributos. |

### Objects

| Opción | Id | Qué hace |
| --- | --- | --- |
| Build object and references | `BuildObjectAndReferences` | Especifica o compila un objeto y sus referencias. |
| Fix variables not based on attribute/domain | `FixVariablesNotBasedInAttributesOrDomain` | Asigna atributo o dominio a variables que no lo tienen. |

### Modules

| Opción | Id | Qué hace |
| --- | --- | --- |
| Build module objects and references | `BuildModule` | Compila los objetos de un módulo y sus referencias. |
| Move objects to modules from file | `MoveObjectsToModulesFromFile` | Mueve objetos a módulos según un archivo. |

### UI / Frontend

| Opción | Id | Qué hace |
| --- | --- | --- |
| Convert safe Web Panels to Smooth UX | `ConvertSafeWebPanelsToSmoothUX` | Convierte a Smooth UX los Web Panels que el análisis marca como seguros. |

### KB cleanup

| Opción | Id | Qué hace |
| --- | --- | --- |
| Remove attributes without table | `AttWithoutBaseTable` | Borra atributos que no tienen tabla base. |
| Remove unreferenced objects | `ObjectsNotCalled` | Borra objetos que no están referenciados. |
| Remove unused variables | `CleanVarsNotUsed` | Borra variables no usadas. |
| Removable transactions | `RemovableTransactions` | Detecta transacciones que se pueden sacar. |
| Initialize and clean objects | `CleanObjects` | Limpia los objetos que se seleccionan. |
| Reset Win forms | `ResetWINForm` | Regenera los Win forms. |
| Replace Nullable = Compatible | `ReplaceNullCompatible` | Cambia atributos con `Nullable = Compatible`. |
| Change Commit on Exit | `ChangeCommitOnExit` | Cambia la propiedad Commit on Exit. |

---

## Reports & Inventory

Inventarios de la KB. No modifican objetos.

### Attributes

| Opción | Id | Qué hace |
| --- | --- | --- |
| Attributes | `ListAttributes` | Inventario de atributos. |
| Formula attributes | `AttFormula` | Lista atributos fórmula. |

### Domains

| Opción | Id | Qué hace |
| --- | --- | --- |
| Domains | `ListDomain` | Inventario de dominios. |

### Tables

| Opción | Id | Qué hace |
| --- | --- | --- |
| Tables | `ListTables` | Inventario de tablas. |
| Table / transaction relation | `TableTransaction` | Relaciona cada tabla con su transacción. |
| Table access: read/insert/update/delete | `TableUpdate` | Muestra en qué objetos se lee, inserta, actualiza o borra cada tabla. |
| Table width | `TablesWidth` | Informa el ancho de las tablas. |

### Objects

| Opción | Id | Qué hace |
| --- | --- | --- |
| Objects | `ListObj` | Inventario de objetos. |
| KB interfaces | `KBInterfaces` | Lista las interfaces de la KB. |
| Build object and references | `BuildObjectAndReferences` | El mismo comando de Fix / Cleanup > Objects. |
| Objects by property | `BuildObjectWithProperty` | Lista objetos según el valor de una propiedad. |
| Web object properties | `ListWebObjectsProperties` | Lista propiedades de objetos web. |

### Modules

| Opción | Id | Qué hace |
| --- | --- | --- |
| Module issues | `ListModules` | Lista problemas de módulos (`ListModulesErrors`). |
| Tables by module | `ListTablesInModules` | Lista las tablas de cada módulo. |
| Public API objects | `ListAPIObjects` | Lista objetos expuestos como API. |
| Module dependencies | `ModuleDependencies` | Lista dependencias entre módulos. |

---

## Compare & Snapshots

### Objects

| Opción | Id | Qué hace |
| --- | --- | --- |
| Create object text snapshot | `CalculateCheckSum` | Guarda una foto del texto de los objetos. |
| Compare last object snapshots | `CompareLastOBJDirectory` | Compara las dos últimas fotos de objetos. |
| Open object snapshots folder | `OpenFolderObjComparerNavigation` | Abre la carpeta de fotos de objetos. |

### Navigations

| Opción | Id | Qué hace |
| --- | --- | --- |
| Create navigation snapshot | `PrepareComparerNavigations` | Guarda una foto de las navegaciones. |
| Compare last navigation snapshots | `CompareLastNVGDirectory` | Compara las dos últimas fotos de navegación. |
| Open navigation snapshots folder | `OpenFolderComparerNavigation` | Abre la carpeta de fotos de navegación. |

---

## Modularization

| Opción | Id | Qué hace |
| --- | --- | --- |
| Recommended module | `RecomendedModule` | Sugiere el módulo de cada objeto. |
| Make objects called from workflow public | `MakeWorkflowObjectsPublic` | Marca como públicos los objetos llamados desde el workflow. |
| Generated objects not reachable from deployment units | `GeneratedObjectsNotReachableFromDeploymentUnits` | Lista objetos generados que ninguna deployment unit alcanza. |
| Split main object | `SplitMainObject` | Parte un objeto main. |
| Move transactions to folders | `MoveTransactions` | Mueve transacciones a carpetas. |
| Generate KB graph | `GenerateGraph` | Genera el grafo de la KB. |
| Apply external modularization | `ApplyExternalModularization` | Aplica una modularización calculada fuera de GeneXus. |
| Add modularization info to documentation | `AddModularizationInfo` | Escribe información de modularización en la documentación de los objetos. |
| Detect module mavericks | `DetectMavericks` | Detecta objetos difíciles de ubicar en un módulo. |

---

## Statistics

| Opción | Id | Qué hace |
| --- | --- | --- |
| Object complexity index | `ObjectsRefactoringCandidates` | Exporta un CSV con el índice de complejidad de los objetos. |
| Table access count | `CountTableAccess` | Cuenta los accesos a cada tabla. |
| Module statistics | `ListModulesStatistics` | Estadísticas por módulo. |
| Modularization quality | `ListModularizationQuality` | Mide la calidad de la modularización. |
| Generated by pattern without dynamism | `CountGeneratedByPattern` | Cuenta objetos generados por pattern. El texto del menú habla de “without dynamism”; el comando que hace ese análisis (`GeneratedByPatternWithoutDynamism`) no está en el menú. |

---

## Utilities

| Opción | Id | Qué hace |
| --- | --- | --- |
| Review configuration | `EditReviewObjects` | Edita la configuración de la revisión de objetos. |
| Search and replace in objects | `SearchAndReplace` | Busca y reemplaza texto en objetos. |
| Generate location.xml | `GenerateLocationXML` | Genera `location.xml`. |
| Copy environment | `CopyEnvironment` | Copia un environment de la KB. |
| Rename attributes and tables | `RenameAttributesAndTables` | Renombra atributos y tablas. |
| Object properties | `ListProperties` | Lista propiedades de objetos. |
| Dynamic combo usage | `ListDynamicCombo` | Lista el uso de dynamic combos. |

---

## Migration

| Opción | Id | Qué hace |
| --- | --- | --- |
| Green screen to web migration | `ObjectMigration` | Apoya la migración de objetos de pantalla verde a web. |

---

## Help

| Opción | Id | Qué hace |
| --- | --- | --- |
| Help | `HelpKBDoctor` | Abre la página de la wiki de GeneXus (página 26679). Siempre habilitado. |
| Last reports | `ListLastReports` | Abre los últimos reportes generados. |
| About KBDoctor | `AboutKBDoctor` | Muestra el acerca de. Siempre habilitado. |

---

## Labs

Bloque experimental, publicado dentro del grupo `LABS` de `KBDoctor.package`. No hay código que lo oculte: aparece en el menú junto con el resto.

Varias opciones de este bloque no tienen entrada en `StringResources.resx`. En ese caso GeneXus muestra el id del comando.

| Opción en el menú | Id | Qué hace |
| --- | --- | --- |
| `GenerateTrnFromTables` | `GenerateTrnFromTables` | Genera transacciones a partir de tablas. Sin texto en recursos. |
| Add IN: to parametrs without IN:/OUT:/INOUT: | `AddINParmRule` | Agrega `IN:` a parámetros que no tienen dirección. |
| Generate SDT Sources | `ProcedureSDT` | Genera procedimientos fuente de un SDT. |
| Generate Get/Set Procedures | `ProcedureGetSet` | Genera procedimientos Get/Set. |
| `CreateDeployUnits` | `CreateDeployUnits` | Crea deployment units. Sin texto en recursos. |
| Generate Simple Trn from Not Generate Transactions | `GenerateSimpleTransactionFromNotGeneratedTransactions` | Genera una transacción simple desde transacciones no generadas. |
| `RenameVariables` | `RenameVariables` | Renombra variables. Sin texto en recursos. |
| Objects with variables not used | `ObjectsWithVarsNotUsed` | Lista objetos con variables sin usar. |
| `GenerateTrnFromTables2` | `GenerateTrnFromTables2` | Segunda variante de generación de transacciones desde tablas. Sin texto en recursos. |
| Description attribute without unique index | `AttDescWithoutUniqueIndex` | Lista atributos descriptor sin índice único. |
| New - Attribute not instanciated | `TableInsertNew` | Lista `New` donde un atributo no se instancia. |
| Tables used by mains | `MainTableUsed` | Lista tablas usadas por objetos main. |
| List all object dependencies | `ObjectsReferenced` | Lista las dependencias de los objetos. |
| With parm() and Commit on exit=Yes | `ObjectsWithCommitOnExit` | Lista objetos con `parm()` y Commit on Exit = Yes. |
| List Commit On Exit | `ListCommitOnExit` | Lista el valor de Commit on Exit. |
| Generated for WIN and WEB | `ObjectsWINWEB` | Lista objetos generados para Win y Web. |
| Webpanels and Transactions called by Procedures | `ListProcedureCallWebpanelTransaction` | Lista Web Panels y transacciones llamados por procedimientos. |
| Objects with legacy code | `ObjectsLegacyCode` | Lista objetos con código legado. |
| Mark Public Objects | `MarkPublicObjects` | Marca objetos como públicos. |
| Mark unreachable objects with GenerateObject=FALSE | `ObjectsNotReacheable` | Pone `GenerateObject = FALSE` en objetos inalcanzables. |
| Clean my KB as much as possible | `CleanKBAsMuchAsPossible` | Encadena limpiezas sobre la KB. |
| List tables/attributes using domain | `ListTableAttributesUsingDomain` | Lista tablas y atributos que usan un dominio. |
| Anonimize KB | `ObjectsWithConstants` | Trabaja sobre constantes para anonimizar la KB. |
| Assign Type Comparer | `AssignTypeComparer` | Compara tipos en asignaciones. |
| Parameter type comparer | `ParameterTypeComparer` | Compara tipos de parámetros. |
| `ObjectsWithRuleOld` | `ObjectsWithRuleOld` | Busca reglas con sintaxis vieja. Sin texto en recursos. |
| Empty Conditional Blocks | `EmptyConditionalBlocks` | Lista bloques condicionales vacíos. |
| For Each without When None | `ForEachsWithoutWhenNone` | Lista `For Each` sin `When None`. |
| News without When Duplicate | `NewsWithoutWhenDuplicate` | Lista `New` sin `When Duplicate`. |
| Constants in code | `ConstantsInCode` | Lista constantes escritas en el código. |
| `ReviewCommits` | `ReviewCommits` | Revisa commits. Sin texto en recursos. |
| `GenerateRESTCalls` | `GenerateRESTCalls` | Genera llamadas REST a partir de procedimientos elegidos. Sin texto en recursos. |
| `SDTsWithDateInWS` | `SDTsWithDateInWS` | Lista SDT con fecha usados en web services. Sin texto en recursos. |
| `GenerateSDTDataLoad` | `GenerateSDTDataLoad` | Genera la carga de datos de SDT elegidos. Sin texto en recursos. |
| Generate DP from Table | `GenerateDPfromTable` | El handler llama a `TablesHelper.GenerateTrnFromTables2()`, el mismo método que `GenerateTrnFromTables2`. |
| Objects With Attribute/Domain as Output | `AttributeAsOutput` | Lista objetos que devuelven un atributo o dominio en el `parm`. |
| `CheckVariableUsages` | `CheckVariableUsages` | Revisa usos de variables. Sin texto en recursos. |
| Generate scripts to check data | `GenerateSQLScripts` | Genera scripts SQL para controlar datos. |

### Themes

Submenú de Labs. **Theme classes not used** y **Object theme classes not used** están siempre habilitadas, aunque no haya KB abierta.

| Opción en el menú | Id | Qué hace |
| --- | --- | --- |
| Class not in Theme and Class not used | `ClassNotInTheme` | Lista clases que no están en el theme y clases no usadas. |
| Classes Used | `ClassUsed` | Lista clases usadas. |
| `ObjThemeClassesNotUsed` | `ObjThemeClassesNotUsed` | Clases de theme no usadas por objeto. Sin texto en recursos. |
| `ThemeClassesNotUsed` | `ThemeClassesNotUsed` | Clases de theme no usadas. Sin texto en recursos. |

---

## Entradas fuera de Tools

El mismo paquete publica tres comandos en otras superficies de GeneXus. El texto de los tres es “Review Object by KBDoctor” o “Review Objects by KBDoctor”.

| Dónde aparece | Id | Qué hace |
| --- | --- | --- |
| Grupo de commit de GeneXus | `PreprocessPendingObjects` | Revisa los objetos pendientes del commit. |
| Menú contextual de un objeto de la KB | `ReviewObject` | Revisa el objeto seleccionado. Solo se habilita con un objeto seleccionado. |
| Menú contextual de módulo o carpeta | `ReviewModuleOrFolder` | Revisa los objetos del módulo o la carpeta. Solo se habilita con un módulo o una carpeta seleccionados. |

---

## Comandos registrados que no están en el menú

Están definidos en `KBDoctor.package` y tienen handler, pero no se publican en Tools ni en los menús contextuales. Los reportes HTML los disparan como acciones (asignar dominio, abrir objeto, borrar, aplicar un reemplazo).

`ApplyReplaceDomain`, `ApplyAttUpdated`, `RemoveIndexAttribute`, `SelectObjectsUpdateAttribute`, `ApplyObjectsUpdateAttribute`, `ObjectsWithVarNotBasedOnAtt`, `ChangeLegacyCode`, `EditLegacyCodeToReplace`, `GeneratedByPatternWithoutDynamism`, `AssignDomainToAttribute`, `AssignDescriptionToAttribute`, `ApplyAttributeText`, `AssignTitleToAttribute`, `AssignColumnTitleToAttribute`, `ListAttribute`, `AddDescriptorIndex`, `RemoveObject`, `RemoveUnreferencedObjectsInUserModules`, `OpenObject`, `SetObjectPropertyText`, `ApplyObjectPropertyText`, `AssignAttributeToVariable`, `AssignDomainToVariable`, `AssignAttributeOrDomainToVariable`, `RunResponsiveSmoothAction`, `ApplySearchAndReplace`, `ApplyCopyEnvironment`, `AssignDescriptionToTable`, `ApplyTableText`, `UDPCallables`, `CheckBldObjects`.

`UDPCallables` está registrado y su handler no hace nada: el cuerpo está comentado.
