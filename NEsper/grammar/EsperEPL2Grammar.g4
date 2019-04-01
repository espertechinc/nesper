grammar EsperEPL2Grammar;

options {
	language=CSharp;
}

@namespace { com.espertech.esper.epl.generated }
@header {
  using System;
  using System.Collections.Generic;
}

@members {
	// provide nice error messages
	private System.Collections.Generic.Stack<string> paraphrases =
		new System.Collections.Generic.Stack<string>();

	// static information initialized once
	private static System.Collections.Generic.IDictionary<int, string> lexerTokenParaphrases =
		       new System.Collections.Generic.Dictionary<int, string>();
	private static System.Collections.Generic.IDictionary<int, string> parserTokenParaphrases =
			   new System.Collections.Generic.Dictionary<int, string>();
	private static System.Collections.Generic.ISet<string> parserKeywordSet =
			   new System.Collections.Generic.HashSet<string>();
	private static System.Collections.Generic.ISet<int> afterScriptTokens =
			   new System.Collections.Generic.HashSet<int>();

	private static readonly Object _iLock = new Object();

	public System.Collections.Generic.Stack<string> GetParaphrases()
	{
		return paraphrases;
	}

	public System.Collections.Generic.ISet<string> GetKeywords()
	{
		GetParserTokenParaphrases();
		return parserKeywordSet;
	}

	public static System.Collections.Generic.IDictionary<int, string> GetLexerTokenParaphrases()
	{
		lock(_iLock)
		{
			if (lexerTokenParaphrases.Count == 0)
			{
				lexerTokenParaphrases[IDENT] = "an identifier";
				lexerTokenParaphrases[FOLLOWED_BY] = "an followed-by '->'";
				lexerTokenParaphrases[EQUALS] = "an equals '='";
				lexerTokenParaphrases[SQL_NE] = "a sql-style not equals '<>'";
				lexerTokenParaphrases[QUESTION] = "a questionmark '?'";
				lexerTokenParaphrases[LPAREN] = "an opening parenthesis '('";
				lexerTokenParaphrases[RPAREN] = "a closing parenthesis ')'";
				lexerTokenParaphrases[LBRACK] = "a left angle bracket '['";
				lexerTokenParaphrases[RBRACK] = "a right angle bracket ']'";
				lexerTokenParaphrases[LCURLY] = "a left curly bracket '{'";
				lexerTokenParaphrases[RCURLY] = "a right curly bracket '}'";
				lexerTokenParaphrases[COLON] = "a colon ':'";
				lexerTokenParaphrases[COMMA] = "a comma ','";
				lexerTokenParaphrases[EQUAL] = "an equals compare '=='";
				lexerTokenParaphrases[LNOT] = "a not '!'";
				lexerTokenParaphrases[BNOT] = "a binary not '~'";
				lexerTokenParaphrases[NOT_EQUAL] = "a not equals '!='";
				lexerTokenParaphrases[DIV] = "a division operator '\'";
				lexerTokenParaphrases[DIV_ASSIGN] = "a division assign '/='";
				lexerTokenParaphrases[PLUS] = "a plus operator '+'";
				lexerTokenParaphrases[PLUS_ASSIGN] = "a plus assign '+='";
				lexerTokenParaphrases[INC] = "an increment operator '++'";
				lexerTokenParaphrases[MINUS] = "a minus '-'";
				lexerTokenParaphrases[MINUS_ASSIGN] = "a minus assign '-='";
				lexerTokenParaphrases[DEC] = "a decrement operator '--'";
				lexerTokenParaphrases[STAR] = "a star '*'";
				lexerTokenParaphrases[STAR_ASSIGN] = "a star assign '*='";
				lexerTokenParaphrases[MOD] = "a modulo";
				lexerTokenParaphrases[MOD_ASSIGN] = "a modulo assign";
				lexerTokenParaphrases[GE] = "a greater equals '>='";
				lexerTokenParaphrases[GT] = "a greater then '>'";
				lexerTokenParaphrases[LE] = "a less equals '<='";
				lexerTokenParaphrases[LT] = "a lesser then '<'";
				lexerTokenParaphrases[BXOR] = "a binary xor '^'";
				lexerTokenParaphrases[BXOR_ASSIGN] = "a binary xor assign '^='";
				lexerTokenParaphrases[BOR] = "a binary or '|'";
				lexerTokenParaphrases[BOR_ASSIGN] = "a binary or assign '|='";
				lexerTokenParaphrases[LOR] = "a logical or '||'";
				lexerTokenParaphrases[BAND] = "a binary and '&'";
				lexerTokenParaphrases[BAND_ASSIGN] = "a binary and assign '&='";
				lexerTokenParaphrases[LAND] = "a logical and '&&'";
				lexerTokenParaphrases[SEMI] = "a semicolon ';'";
				lexerTokenParaphrases[DOT] = "a dot '.'";
			}
		}

		return lexerTokenParaphrases;
	}

	public static System.Collections.Generic.IDictionary<int, string> GetParserTokenParaphrases()
	{
		lock(_iLock)
		{
			if (parserTokenParaphrases.Count == 0)
			{
				parserTokenParaphrases[CREATE] = "'create'";
				parserTokenParaphrases[WINDOW] = "'window'";
				parserTokenParaphrases[IN_SET] = "'in'";
				parserTokenParaphrases[BETWEEN] = "'between'";
				parserTokenParaphrases[LIKE] = "'like'";
				parserTokenParaphrases[REGEXP] = "'regexp'";
				parserTokenParaphrases[ESCAPE] = "'escape'";
				parserTokenParaphrases[OR_EXPR] = "'or'";
				parserTokenParaphrases[AND_EXPR] = "'and'";
				parserTokenParaphrases[NOT_EXPR] = "'not'";
				parserTokenParaphrases[EVERY_EXPR] = "'every'";
				parserTokenParaphrases[EVERY_DISTINCT_EXPR] = "'every-distinct'";
				parserTokenParaphrases[WHERE] = "'where'";
				parserTokenParaphrases[AS] = "'as'";
				parserTokenParaphrases[SUM] = "'sum'";
				parserTokenParaphrases[AVG] = "'avg'";
				parserTokenParaphrases[MAX] = "'max'";
				parserTokenParaphrases[MIN] = "'min'";
				parserTokenParaphrases[COALESCE] = "'coalesce'";
				parserTokenParaphrases[MEDIAN] = "'median'";
				parserTokenParaphrases[STDDEV] = "'stddev'";
				parserTokenParaphrases[AVEDEV] = "'avedev'";
				parserTokenParaphrases[COUNT] = "'count'";
				parserTokenParaphrases[SELECT] = "'select'";
				parserTokenParaphrases[CASE] = "'case'";
				parserTokenParaphrases[ELSE] = "'else'";
				parserTokenParaphrases[WHEN] = "'when'";
				parserTokenParaphrases[THEN] = "'then'";
				parserTokenParaphrases[END] = "'end'";
				parserTokenParaphrases[FROM] = "'from'";
				parserTokenParaphrases[OUTER] = "'outer'";
				parserTokenParaphrases[INNER] = "'inner'";
				parserTokenParaphrases[JOIN] = "'join'";
				parserTokenParaphrases[LEFT] = "'left'";
				parserTokenParaphrases[RIGHT] = "'right'";
				parserTokenParaphrases[FULL] = "'full'";
				parserTokenParaphrases[ON] = "'on'";
				parserTokenParaphrases[IS] = "'is'";
				parserTokenParaphrases[BY] = "'by'";
				parserTokenParaphrases[GROUP] = "'group'";
				parserTokenParaphrases[HAVING] = "'having'";
				parserTokenParaphrases[ALL] = "'all'";
				parserTokenParaphrases[ANY] = "'any'";
				parserTokenParaphrases[SOME] = "'some'";
				parserTokenParaphrases[OUTPUT] = "'output'";
				parserTokenParaphrases[EVENTS] = "'events'";
				parserTokenParaphrases[FIRST] = "'first'";
				parserTokenParaphrases[LAST] = "'last'";
				parserTokenParaphrases[INSERT] = "'insert'";
				parserTokenParaphrases[INTO] = "'into'";
				parserTokenParaphrases[ORDER] = "'order'";
				parserTokenParaphrases[ASC] = "'asc'";
				parserTokenParaphrases[DESC] = "'desc'";
				parserTokenParaphrases[RSTREAM] = "'rstream'";
				parserTokenParaphrases[ISTREAM] = "'istream'";
				parserTokenParaphrases[IRSTREAM] = "'irstream'";
				parserTokenParaphrases[SCHEMA] = "'schema'";
				parserTokenParaphrases[UNIDIRECTIONAL] = "'unidirectional'";
				parserTokenParaphrases[RETAINUNION] = "'retain-union'";
				parserTokenParaphrases[RETAININTERSECTION] = "'retain-intersection'";
				parserTokenParaphrases[PATTERN] = "'pattern'";
				parserTokenParaphrases[SQL] = "'sql'";
				parserTokenParaphrases[METADATASQL] = "'metadatasql'";
				parserTokenParaphrases[PREVIOUS] = "'prev'";
				parserTokenParaphrases[PREVIOUSTAIL] = "'prevtail'";
				parserTokenParaphrases[PREVIOUSCOUNT] = "'prevcount'";
				parserTokenParaphrases[PREVIOUSWINDOW] = "'prevwindow'";
				parserTokenParaphrases[PRIOR] = "'prior'";
				parserTokenParaphrases[EXISTS] = "'exists'";
				parserTokenParaphrases[WEEKDAY] = "'weekday'";
				parserTokenParaphrases[LW] = "'lastweekday'";
				parserTokenParaphrases[INSTANCEOF] = "'instanceof'";
				parserTokenParaphrases[TYPEOF] = "'typeof'";
				parserTokenParaphrases[CAST] = "'cast'";
				parserTokenParaphrases[CURRENT_TIMESTAMP] = "'current_timestamp'";
				parserTokenParaphrases[DELETE] = "'delete'";
				parserTokenParaphrases[DISTINCT] = "'distinct'";
				parserTokenParaphrases[SNAPSHOT] = "'snapshot'";
				parserTokenParaphrases[SET] = "'set'";
				parserTokenParaphrases[VARIABLE] = "'variable'";
				parserTokenParaphrases[TABLE] = "'table'";
				parserTokenParaphrases[INDEX] = "'index'";
				parserTokenParaphrases[UNTIL] = "'until'";
				parserTokenParaphrases[AT] = "'at'";
				parserTokenParaphrases[TIMEPERIOD_YEAR] = "'year'";
				parserTokenParaphrases[TIMEPERIOD_YEARS] = "'years'";
				parserTokenParaphrases[TIMEPERIOD_MONTH] = "'month'";
				parserTokenParaphrases[TIMEPERIOD_MONTHS] = "'months'";
				parserTokenParaphrases[TIMEPERIOD_WEEK] = "'week'";
				parserTokenParaphrases[TIMEPERIOD_WEEKS] = "'weeks'";
				parserTokenParaphrases[TIMEPERIOD_DAY] = "'day'";
				parserTokenParaphrases[TIMEPERIOD_DAYS] = "'days'";
				parserTokenParaphrases[TIMEPERIOD_HOUR] = "'hour'";
				parserTokenParaphrases[TIMEPERIOD_HOURS] = "'hours'";
				parserTokenParaphrases[TIMEPERIOD_MINUTE] = "'minute'";
				parserTokenParaphrases[TIMEPERIOD_MINUTES] = "'minutes'";
				parserTokenParaphrases[TIMEPERIOD_SEC] = "'sec'";
				parserTokenParaphrases[TIMEPERIOD_SECOND] = "'second'";
				parserTokenParaphrases[TIMEPERIOD_SECONDS] = "'seconds'";
				parserTokenParaphrases[TIMEPERIOD_MILLISEC] = "'msec'";
				parserTokenParaphrases[TIMEPERIOD_MILLISECOND] = "'millisecond'";
				parserTokenParaphrases[TIMEPERIOD_MILLISECONDS] = "'milliseconds'";
				parserTokenParaphrases[TIMEPERIOD_MICROSEC] = "'usec'";
				parserTokenParaphrases[TIMEPERIOD_MICROSECOND] = "'microsecond'";
				parserTokenParaphrases[TIMEPERIOD_MICROSECONDS] = "'microseconds'";
				parserTokenParaphrases[BOOLEAN_TRUE] = "'true'";
				parserTokenParaphrases[BOOLEAN_FALSE] = "'false'";
				parserTokenParaphrases[VALUE_NULL] = "'null'";
				parserTokenParaphrases[ROW_LIMIT_EXPR] = "'limit'";
				parserTokenParaphrases[OFFSET] = "'offset'";
				parserTokenParaphrases[UPDATE] = "'update'";
				parserTokenParaphrases[MATCH_RECOGNIZE] = "'match_recognize'";
				parserTokenParaphrases[MEASURES] = "'measures'";
				parserTokenParaphrases[DEFINE] = "'define'";
				parserTokenParaphrases[PARTITION] = "'partition'";
				parserTokenParaphrases[MATCHES] = "'matches'";
				parserTokenParaphrases[AFTER] = "'after'";
				parserTokenParaphrases[FOR] = "'for'";
				parserTokenParaphrases[WHILE] = "'while'";
				parserTokenParaphrases[MERGE] = "'merge'";
				parserTokenParaphrases[MATCHED] = "'matched'";
				parserTokenParaphrases[CONTEXT] = "'context'";
				parserTokenParaphrases[START] = "'start'";
				parserTokenParaphrases[END] = "'end'";
				parserTokenParaphrases[INITIATED] = "'initiated'";
				parserTokenParaphrases[TERMINATED] = "'terminated'";
				parserTokenParaphrases[USING] = "'using'";
				parserTokenParaphrases[EXPRESSIONDECL] = "'expression'";
				parserTokenParaphrases[NEWKW] = "'new'";
				parserTokenParaphrases[DATAFLOW] = "'dataflow'";
				parserTokenParaphrases[VALUES] = "'values'";
				parserTokenParaphrases[CUBE] = "'cube'";
				parserTokenParaphrases[ROLLUP] = "'rollup'";
				parserTokenParaphrases[GROUPING] = "'grouping'";
				parserTokenParaphrases[GROUPING_ID] = "'grouping_id'";
				parserTokenParaphrases[SETS] = "'sets'";

				parserKeywordSet = new HashSet<string>(
					parserTokenParaphrases.Values);
			}
		}

		return parserTokenParaphrases;
	}

	public static System.Collections.Generic.ISet<int> GetAfterScriptTokens()
	{
		if (afterScriptTokens.Count == 0)
		{
			afterScriptTokens.Add(CREATE);
			afterScriptTokens.Add(EXPRESSIONDECL);
			afterScriptTokens.Add(SELECT);
			afterScriptTokens.Add(INSERT);
			afterScriptTokens.Add(ON);
			afterScriptTokens.Add(DELETE);
			afterScriptTokens.Add(UPDATE);
			afterScriptTokens.Add(ATCHAR);
		}

		return afterScriptTokens;
	}
}

//----------------------------------------------------------------------------
// Start Rules
//----------------------------------------------------------------------------
startPatternExpressionRule : (annotationEnum | expressionDecl)* patternExpression EOF;

startEPLExpressionRule : (annotationEnum | expressionDecl)* eplExpression EOF;

startEventPropertyRule : eventProperty EOF;

startJsonValueRule : jsonvalue EOF;

//----------------------------------------------------------------------------
// Expression Declaration
//----------------------------------------------------------------------------
expressionDecl : EXPRESSIONDECL classIdentifier? (array=LBRACK RBRACK)? typeExpressionAnnotation? expressionDialect? name=IDENT (LPAREN columnList? RPAREN)? (alias=IDENT FOR)? expressionDef;

expressionDialect : d=IDENT COLON;

expressionDef :	LCURLY expressionLambdaDecl? expression RCURLY
		| LBRACK stringconstant RBRACK
		;

expressionLambdaDecl : (i=IDENT | (LPAREN columnList RPAREN)) (GOES | FOLLOWED_BY);

expressionTypeAnno : ATCHAR n=IDENT (LPAREN v=IDENT RPAREN);

//----------------------------------------------------------------------------
// Annotations
//----------------------------------------------------------------------------
annotationEnum : ATCHAR classIdentifier ( '(' ( elementValuePairsEnum | elementValueEnum )? ')' )?;

elementValuePairsEnum : elementValuePairEnum (COMMA elementValuePairEnum)*;

elementValuePairEnum : keywordAllowedIdent '=' elementValueEnum;

elementValueEnum : annotationEnum
		| elementValueArrayEnum
		| constant
		| v=IDENT
		| classIdentifier
    		;

elementValueArrayEnum : '{' (elementValueEnum (',' elementValueEnum)*)? (',')? '}';

//----------------------------------------------------------------------------
// EPL expression
//----------------------------------------------------------------------------
eplExpression : contextExpr?
		(selectExpr
		| createWindowExpr
		| createIndexExpr
		| createVariableExpr
		| createTableExpr
		| createSchemaExpr
		| createContextExpr
		| createExpressionExpr
		| onExpr
		| updateExpr
		| createDataflow
		| fafDelete
		| fafUpdate
		| fafInsert) forExpr?
		;

contextExpr : CONTEXT i=IDENT;

selectExpr :    (INTO intoTableExpr)?
		(INSERT insertIntoExpr)?
		SELECT selectClause
		(FROM fromClause)?
		matchRecog?
		(WHERE whereClause)?
		(GROUP BY groupByListExpr)?
		(HAVING havingClause)?
		(OUTPUT outputLimit)?
		(ORDER BY orderByListExpr)?
		(ROW_LIMIT_EXPR rowLimit)?
		;

onExpr : ON onStreamExpr
	(onDeleteExpr | onSelectExpr (onSelectInsertExpr+ outputClauseInsert?)? | onSetExpr | onUpdateExpr | onMergeExpr)
	;

onStreamExpr : (eventFilterExpression | patternInclusionExpression) (AS i=IDENT | i=IDENT)?;

updateExpr : UPDATE ISTREAM updateDetails;

updateDetails :	classIdentifier (AS i=IDENT | i=IDENT)? SET onSetAssignmentList (WHERE whereClause)?;

onMergeExpr : MERGE INTO? n=IDENT (AS i=IDENT | i=IDENT)? (WHERE whereClause)? mergeItem+;

mergeItem : (mergeMatched | mergeUnmatched);

mergeMatched : WHEN MATCHED (AND_EXPR expression)? mergeMatchedItem+;

mergeMatchedItem : THEN (
		  ( u=UPDATE SET onSetAssignmentList) (WHERE whereClause)?
		  | d=DELETE (WHERE whereClause)?
		  | mergeInsert
		  )
		  ;

mergeUnmatched : WHEN NOT_EXPR MATCHED (AND_EXPR expression)? mergeUnmatchedItem+;

mergeUnmatchedItem : THEN mergeInsert;

mergeInsert : INSERT (INTO classIdentifier)? (LPAREN columnList RPAREN)? SELECT selectionList (WHERE whereClause)?;

onSelectExpr
@init  { paraphrases.Push("on-select clause"); }
@after { paraphrases.Pop(); }
		: (INSERT insertIntoExpr)?
		SELECT (AND_EXPR? d=DELETE)? DISTINCT? selectionList
		onExprFrom?
		(WHERE whereClause)?
		(GROUP BY groupByListExpr)?
		(HAVING havingClause)?
		(ORDER BY orderByListExpr)?
		(ROW_LIMIT_EXPR rowLimit)?
		;

onUpdateExpr
@init  { paraphrases.Push("on-update clause"); }
@after { paraphrases.Pop(); }
		: UPDATE n=IDENT (AS i=IDENT | i=IDENT)? SET onSetAssignmentList (WHERE whereClause)?;

onSelectInsertExpr
@init  { paraphrases.Push("on-select-insert clause"); }
@after { paraphrases.Pop(); }
		: INSERT insertIntoExpr SELECT selectionList onSelectInsertFromClause? (WHERE whereClause)?;

onSelectInsertFromClause
		: FROM propertyExpression (AS i=IDENT | i=IDENT)?;

outputClauseInsert : OUTPUT (f=FIRST | a=ALL);

onDeleteExpr
@init  { paraphrases.Push("on-delete clause"); }
@after { paraphrases.Pop(); }
		: DELETE onExprFrom (WHERE whereClause)?;

onSetExpr
@init  { paraphrases.Push("on-set clause"); }
@after { paraphrases.Pop(); }
		: SET onSetAssignmentList;

onSetAssignmentList : onSetAssignment (COMMA onSetAssignment)*;

onSetAssignment : eventProperty EQUALS expression | expression;

onExprFrom : FROM n=IDENT (AS i=IDENT | i=IDENT)?;

createWindowExpr : CREATE WINDOW i=IDENT viewExpressions? (ru=RETAINUNION|ri=RETAININTERSECTION)? AS?
		  (
		  	createWindowExprModelAfter
		  |   	LPAREN createColumnList RPAREN
		  )
		  (i1=INSERT (WHERE expression)? )?;

createWindowExprModelAfter : (SELECT createSelectionList FROM)? classIdentifier;

createIndexExpr : CREATE (u=IDENT)? INDEX n=IDENT ON w=IDENT LPAREN createIndexColumnList RPAREN;

createIndexColumnList : createIndexColumn (COMMA createIndexColumn)*;

createIndexColumn : (expression | LPAREN i=expressionList? RPAREN) (t=IDENT (LPAREN p=expressionList? RPAREN)? )?;	

createVariableExpr : CREATE c=IDENT? VARIABLE classIdentifier (arr=LBRACK p=IDENT? RBRACK)? n=IDENT (EQUALS expression)?;

createTableExpr : CREATE TABLE n=IDENT AS? LPAREN createTableColumnList RPAREN;

createTableColumnList : createTableColumn (COMMA createTableColumn)*;

createTableColumn : n=IDENT (createTableColumnPlain | builtinFunc | libFunction) p=IDENT? k=IDENT? (typeExpressionAnnotation | annotationEnum)*;

createTableColumnPlain : classIdentifier (b=LBRACK p=IDENT? RBRACK)?;

createColumnList
@init  { paraphrases.Push("column list"); }
@after { paraphrases.Pop(); }
		: createColumnListElement (COMMA createColumnListElement)*;

createColumnListElement : classIdentifier (VALUE_NULL | (classIdentifier (b=LBRACK p=IDENT? RBRACK)?)) ;

createSelectionList
@init  { paraphrases.Push("select clause"); }
@after { paraphrases.Pop(); }
		: createSelectionListElement (COMMA createSelectionListElement)* ;

createSelectionListElement : s=STAR
			     | eventProperty (AS i=IDENT)?
			     | constant AS i=IDENT;

createSchemaExpr : CREATE keyword=IDENT? createSchemaDef;

createSchemaDef : SCHEMA name=IDENT AS?
		  (
			variantList
		  |   	LPAREN createColumnList? RPAREN
		  ) createSchemaQual*;

fafDelete : DELETE FROM classIdentifier (AS i=IDENT | i=IDENT)? (WHERE whereClause)?;

fafUpdate : UPDATE updateDetails;

fafInsert : INSERT insertIntoExpr VALUES LPAREN expressionList RPAREN;

createDataflow : CREATE DATAFLOW name=IDENT AS? gopList;

gopList : gop gop*;

gop : annotationEnum* (opName=IDENT | s=SELECT) gopParams? gopOut? LCURLY gopDetail? COMMA? RCURLY
                | createSchemaExpr COMMA;

gopParams : LPAREN gopParamsItemList RPAREN;

gopParamsItemList : gopParamsItem (COMMA gopParamsItem)*;

gopParamsItem :	(n=classIdentifier | gopParamsItemMany) gopParamsItemAs?;

gopParamsItemMany : LPAREN classIdentifier (COMMA classIdentifier) RPAREN;

gopParamsItemAs : AS a=IDENT;

gopOut : FOLLOWED_BY gopOutItem (COMMA gopOutItem)*;

gopOutItem : n=classIdentifier gopOutTypeList?;

gopOutTypeList : LT gopOutTypeParam (COMMA gopOutTypeParam)* GT;

gopOutTypeParam : (gopOutTypeItem | q=QUESTION);

gopOutTypeItem : classIdentifier gopOutTypeList?;

gopDetail : gopConfig (COMMA gopConfig)*;

gopConfig : SELECT (COLON|EQUALS) LPAREN selectExpr RPAREN
                | n=IDENT (COLON|EQUALS) (expression | jsonobject | jsonarray);

createContextExpr : CREATE CONTEXT name=IDENT AS? createContextDetail;

createExpressionExpr : CREATE expressionDecl;

createContextDetail : createContextChoice
                | contextContextNested COMMA contextContextNested (COMMA contextContextNested)*;

contextContextNested : CONTEXT name=IDENT AS? createContextChoice;

createContextChoice : START (ATCHAR i=IDENT | r1=createContextRangePoint) (END r2=createContextRangePoint)?
		| INITIATED (BY)? createContextDistinct? (ATCHAR i=IDENT AND_EXPR)? r1=createContextRangePoint (TERMINATED (BY)? r2=createContextRangePoint)?
		| PARTITION (BY)? createContextPartitionItem (COMMA createContextPartitionItem)*
		| createContextGroupItem (COMMA createContextGroupItem)* FROM eventFilterExpression
		| COALESCE (BY)? createContextCoalesceItem (COMMA createContextCoalesceItem)* g=IDENT number (p=IDENT)?;

createContextDistinct :	DISTINCT LPAREN expressionList? RPAREN;

createContextRangePoint : createContextFilter
                | patternInclusionExpression (ATCHAR i=IDENT)?
                | crontabLimitParameterSet
                | AFTER timePeriod;

createContextFilter : eventFilterExpression (AS? i=IDENT)?;

createContextPartitionItem : eventProperty ((AND_EXPR|COMMA) eventProperty)* FROM eventFilterExpression;

createContextCoalesceItem : libFunctionNoClass FROM eventFilterExpression;

createContextGroupItem : GROUP BY? expression AS i=IDENT;

createSchemaQual : i=IDENT columnList;

variantList : variantListElement (COMMA variantListElement)*;

variantListElement : STAR
                | classIdentifier;


intoTableExpr
@init  { paraphrases.Push("into-table clause"); }
@after { paraphrases.Pop(); }
		: TABLE i=IDENT;

insertIntoExpr
@init  { paraphrases.Push("insert-into clause"); }
@after { paraphrases.Pop(); }
		: (i=ISTREAM | r=RSTREAM | ir=IRSTREAM)? INTO classIdentifier (LPAREN columnList? RPAREN)?;

columnList : IDENT (COMMA IDENT)*;

fromClause
@init  { paraphrases.Push("from clause"); }
@after { paraphrases.Pop(); }
		: streamExpression (regularJoin | outerJoinList);

regularJoin : (COMMA streamExpression)*;

outerJoinList :	outerJoin (outerJoin)*;

outerJoin
@init  { paraphrases.Push("outer join"); }
@after { paraphrases.Pop(); }
		: (
	          ((tl=LEFT|tr=RIGHT|tf=FULL) OUTER)?
	          | (i=INNER)
	        ) JOIN streamExpression outerJoinIdent?;

outerJoinIdent : ON outerJoinIdentPair (AND_EXPR outerJoinIdentPair)*;

outerJoinIdentPair : eventProperty EQUALS eventProperty ;

whereClause
@init  { paraphrases.Push("where clause"); }
@after { paraphrases.Pop(); }
		: evalOrExpression;

selectClause
@init  { paraphrases.Push("select clause"); }
@after { paraphrases.Pop(); }
		: (s=RSTREAM | s=ISTREAM | s=IRSTREAM)? d=DISTINCT? selectionList;

selectionList :	selectionListElement (COMMA selectionListElement)*;

selectionListElement : s=STAR
                | streamSelector
                | selectionListElementExpr;

selectionListElementExpr : expression selectionListElementAnno? (AS? keywordAllowedIdent)?;

selectionListElementAnno : ATCHAR i=IDENT;

streamSelector : s=IDENT DOT STAR (AS i=IDENT)?;

streamExpression : (eventFilterExpression | patternInclusionExpression | databaseJoinExpression | methodJoinExpression )
		viewExpressions? (AS i=IDENT | i=IDENT)? (u=UNIDIRECTIONAL)? (ru=RETAINUNION|ri=RETAININTERSECTION)?;

forExpr : FOR i=IDENT (LPAREN expressionList? RPAREN)?;

patternInclusionExpression : PATTERN annotationEnum* LBRACK patternExpression RBRACK;

databaseJoinExpression
@init  { paraphrases.Push("relational data join"); }
@after { paraphrases.Pop(); }
		: SQL COLON i=IDENT LBRACK (s=STRING_LITERAL | s=QUOTED_STRING_LITERAL) (METADATASQL (s2=STRING_LITERAL | s2=QUOTED_STRING_LITERAL))? RBRACK;

methodJoinExpression
@init  { paraphrases.Push("method invocation join"); }
@after { paraphrases.Pop(); }
		: i=IDENT COLON classIdentifier (LPAREN expressionList? RPAREN)? typeExpressionAnnotation?;

viewExpressions
@init  { paraphrases.Push("view specifications"); }
@after { paraphrases.Pop(); }
		: (DOT viewExpressionWNamespace (DOT viewExpressionWNamespace)*)
		| (HASHCHAR viewExpressionOptNamespace (HASHCHAR viewExpressionOptNamespace)*);

viewExpressionWNamespace : ns=IDENT COLON viewWParameters;

viewExpressionOptNamespace : (ns=IDENT COLON)? viewWParameters;

viewWParameters : (i=IDENT|m=MERGE) (LPAREN expressionWithTimeList? RPAREN)?;

groupByListExpr
@init  { paraphrases.Push("group-by clause"); }
@after { paraphrases.Pop(); }
		: groupByListChoice (COMMA groupByListChoice)*;

groupByListChoice : e1=expression | groupByCubeOrRollup | groupByGroupingSets;

groupByCubeOrRollup : (CUBE | ROLLUP) LPAREN groupByCombinableExpr (COMMA groupByCombinableExpr)* RPAREN;

groupByGroupingSets : GROUPING SETS LPAREN groupBySetsChoice (COMMA groupBySetsChoice)* RPAREN;

groupBySetsChoice : groupByCubeOrRollup | groupByCombinableExpr;

groupByCombinableExpr : e1=expression | LPAREN (expression (COMMA expression)*)? RPAREN;

orderByListExpr
@init  { paraphrases.Push("order by clause"); }
@after { paraphrases.Pop(); }
		: orderByListElement (COMMA orderByListElement)*;

orderByListElement
		: expression (a=ASC|d=DESC)?;

havingClause
@init  { paraphrases.Push("having clause"); }
@after { paraphrases.Pop(); }
		: evalOrExpression;

outputLimit
@init  { paraphrases.Push("output rate clause"); }
@after { paraphrases.Pop(); }
		: outputLimitAfter?
 	       (k=ALL|k=FIRST|k=LAST|k=SNAPSHOT)?
	        (
	          ( ev=EVERY_EXPR
		    (
		      timePeriod
		    | (number | i=IDENT) (e=EVENTS)
		    )
		  )
		  |
		  ( at=AT crontabLimitParameterSet)
		  |
		  ( wh=WHEN expression (THEN onSetExpr)? )
		  |
		  ( t=WHEN TERMINATED (AND_EXPR expression)? (THEN onSetExpr)? )
		  |
	        )
	        outputLimitAndTerm?;

outputLimitAndTerm : AND_EXPR WHEN TERMINATED (AND_EXPR expression)? (THEN onSetExpr)?;

outputLimitAfter : a=AFTER (timePeriod | number EVENTS);

rowLimit
@init  { paraphrases.Push("row limit clause"); }
@after { paraphrases.Pop(); }
		: (n1=numberconstant | i1=IDENT) ((c=COMMA | o=OFFSET) (n2=numberconstant | i2=IDENT))?;

crontabLimitParameterSet : LPAREN expressionWithTimeList RPAREN;

whenClause : (WHEN expression THEN expression);

elseClause : (ELSE expression);

//----------------------------------------------------------------------------
// Match recognize
//----------------------------------------------------------------------------
//
// Lowest precedence is listed first, order is (highest to lowest):
// Single-character-ERE duplication * + ? {m,n}
// Concatenation
// Anchoring ^ $
// Alternation  |
//
matchRecog : MATCH_RECOGNIZE LPAREN matchRecogPartitionBy? matchRecogMeasures matchRecogMatchesSelection? matchRecogMatchesAfterSkip? matchRecogPattern
		matchRecogMatchesInterval? matchRecogDefine? RPAREN ;

matchRecogPartitionBy : PARTITION BY expression (COMMA expression)*;

matchRecogMeasures : MEASURES matchRecogMeasureItem (COMMA matchRecogMeasureItem)*;

matchRecogMeasureItem : expression (AS (i=IDENT)? )?;

matchRecogMatchesSelection : ALL MATCHES;

matchRecogPattern : PATTERN LPAREN matchRecogPatternAlteration RPAREN;

matchRecogMatchesAfterSkip : AFTER i1=keywordAllowedIdent i2=keywordAllowedIdent i3=keywordAllowedIdent i4=keywordAllowedIdent i5=keywordAllowedIdent;

matchRecogMatchesInterval : i=IDENT timePeriod (OR_EXPR t=TERMINATED)?;

matchRecogPatternAlteration : matchRecogPatternConcat (o=BOR matchRecogPatternConcat)*;

matchRecogPatternConcat : matchRecogPatternUnary+;

matchRecogPatternUnary : matchRecogPatternPermute | matchRecogPatternNested | matchRecogPatternAtom;

matchRecogPatternNested : LPAREN matchRecogPatternAlteration RPAREN (s=STAR | s=PLUS | s=QUESTION)? matchRecogPatternRepeat?;

matchRecogPatternPermute : MATCH_RECOGNIZE_PERMUTE LPAREN matchRecogPatternAlteration (COMMA matchRecogPatternAlteration)* RPAREN;

matchRecogPatternAtom :	i=IDENT ((s=STAR | s=PLUS | s=QUESTION) (reluctant=QUESTION)? )? matchRecogPatternRepeat?;

matchRecogPatternRepeat : LCURLY e1=expression? comma=COMMA? e2=expression? RCURLY;

matchRecogDefine : DEFINE matchRecogDefineItem (COMMA matchRecogDefineItem)*;

matchRecogDefineItem : i=IDENT AS expression;

//----------------------------------------------------------------------------
// Expression
//----------------------------------------------------------------------------
expression : caseExpression;

caseExpression : { paraphrases.Push("case expression"); }  CASE whenClause+ elseClause? END { paraphrases.Pop(); }
		| { paraphrases.Push("case expression"); }  CASE expression whenClause+ elseClause? END { paraphrases.Pop(); }
		| evalOrExpression;

evalOrExpression : evalAndExpression (op=OR_EXPR evalAndExpression)*;

evalAndExpression : bitWiseExpression (op=AND_EXPR bitWiseExpression)*;

bitWiseExpression : negatedExpression ( (BAND|BOR|BXOR) negatedExpression)* ;

negatedExpression : evalEqualsExpression
		| NOT_EXPR evalEqualsExpression;

evalEqualsExpression : evalRelationalExpression (
			    (eq=EQUALS
			      |  is=IS
			      |  isnot=IS NOT_EXPR
			      |  sqlne=SQL_NE
			      |  ne=NOT_EQUAL
			     )
		       (
			evalRelationalExpression
			|  (a=ANY | a=SOME | a=ALL) ( (LPAREN expressionList? RPAREN) | subSelectGroupExpression )
		       )
		     )*;

evalRelationalExpression : concatenationExpr (
			(
			  (
			    (r=LT|r=GT|r=LE|r=GE)
			    	(
			    	  concatenationExpr
			    	  | (g=ANY | g=SOME | g=ALL) ( (LPAREN expressionList? RPAREN) | subSelectGroupExpression )
			    	)

			  )*
			)
			| (n=NOT_EXPR)?
			(
				// Represent the optional NOT prefix using the token type by
				// testing 'n' and setting the token type accordingly.
				(in=IN_SET
					  (l=LPAREN | l=LBRACK) expression	// brackets are for inclusive/exclusive
						(
							( col=COLON (expression) )		// range
							|
							( (COMMA expression)* )		// list of values
						)
					  (r=RPAREN | r=RBRACK)
					)
				| inset=IN_SET inSubSelectQuery
				| between=BETWEEN betweenList
				| like=LIKE concatenationExpr (ESCAPE stringconstant)?
				| regex=REGEXP concatenationExpr
			)
		);

inSubSelectQuery : subQueryExpr;

concatenationExpr : additiveExpression ( c=LOR additiveExpression ( LOR additiveExpression)* )?;

additiveExpression : multiplyExpression ( (PLUS|MINUS) multiplyExpression )*;

multiplyExpression : unaryExpression ( (STAR|DIV|MOD) unaryExpression )*;

unaryExpression : MINUS eventProperty
		| constant
		| substitutionCanChain
		| inner=LPAREN expression RPAREN chainedFunction?
		| builtinFunc
		| eventPropertyOrLibFunction
		| arrayExpression
		| rowSubSelectExpression
		| existsSubSelectExpression
		| NEWKW LCURLY newAssign (COMMA newAssign)* RCURLY
		| NEWKW classIdentifier LPAREN (expression (COMMA expression)*)? RPAREN chainedFunction?
		| b=IDENT LBRACK expression (COMMA expression)* RBRACK chainedFunction?
		| jsonobject
		;

substitutionCanChain : substitution chainedFunction?;

chainedFunction : d=DOT libFunctionNoClass (d=DOT libFunctionNoClass)*;

newAssign : eventProperty (EQUALS expression)?;

rowSubSelectExpression : subQueryExpr chainedFunction?;

subSelectGroupExpression : subQueryExpr;

existsSubSelectExpression : EXISTS subQueryExpr;

subQueryExpr
@init  { paraphrases.Push("subquery"); }
@after { paraphrases.Pop(); }
		: LPAREN  SELECT DISTINCT? selectionList FROM subSelectFilterExpr (WHERE whereClause)? (GROUP BY groupByListExpr)? (HAVING havingClause)? RPAREN;

subSelectFilterExpr
@init  { paraphrases.Push("subquery filter specification"); }
@after { paraphrases.Pop(); }
		: eventFilterExpression viewExpressions? (AS i=IDENT | i=IDENT)? (ru=RETAINUNION|ri=RETAININTERSECTION)?;

arrayExpression : LCURLY (expression (COMMA expression)* )? RCURLY chainedFunction?;

builtinFunc : SUM LPAREN (ALL | DISTINCT)? expressionListWithNamed RPAREN   			#builtin_sum
		| AVG LPAREN (ALL | DISTINCT)? expressionListWithNamed RPAREN			#builtin_avg
		| COUNT LPAREN (a=ALL | d=DISTINCT)? expressionListWithNamed RPAREN		#builtin_cnt
		| MEDIAN LPAREN (ALL | DISTINCT)? expressionListWithNamed RPAREN		#builtin_median
		| STDDEV LPAREN (ALL | DISTINCT)? expressionListWithNamed RPAREN		#builtin_stddev
		| AVEDEV LPAREN (ALL | DISTINCT)? expressionListWithNamed RPAREN		#builtin_avedev
		| firstLastWindowAggregation							#builtin_firstlastwindow
		| COALESCE LPAREN expression COMMA expression (COMMA expression)* RPAREN	#builtin_coalesce
		| PREVIOUS LPAREN expression (COMMA expression)? RPAREN chainedFunction?	#builtin_prev
		| PREVIOUSTAIL LPAREN expression (COMMA expression)? RPAREN chainedFunction?	#builtin_prevtail
		| PREVIOUSCOUNT LPAREN expression RPAREN					#builtin_prevcount
		| PREVIOUSWINDOW LPAREN expression RPAREN chainedFunction?			#builtin_prevwindow
		| PRIOR LPAREN expression COMMA eventProperty RPAREN				#builtin_prior
		| GROUPING LPAREN expression RPAREN						#builtin_grouping
		| GROUPING_ID LPAREN expressionList RPAREN					#builtin_groupingid
		// MIN and MAX can also be "Math.min" static function and "min(price)" aggregation function and "min(a, b, c...)" built-in function
		// therefore handled in code via libFunction as below
		| INSTANCEOF LPAREN expression COMMA classIdentifier (COMMA classIdentifier)* RPAREN	#builtin_instanceof
		| TYPEOF LPAREN expression RPAREN							#builtin_typeof
		| CAST LPAREN expression (COMMA | AS) classIdentifier (COMMA expressionNamedParameter)? RPAREN chainedFunction?	#builtin_cast
		| EXISTS LPAREN eventProperty RPAREN						#builtin_exists
		| CURRENT_TIMESTAMP (LPAREN RPAREN)? chainedFunction?				#builtin_currts
		| ISTREAM LPAREN RPAREN								#builtin_istream
		;

firstLastWindowAggregation : (q=FIRST | q=LAST | q=WINDOW) LPAREN expressionListWithNamed? RPAREN chainedFunction?;

eventPropertyOrLibFunction : eventProperty | libFunction;

libFunction: libFunctionWithClass (DOT libFunctionNoClass)*;

libFunctionWithClass : ((classIdentifier DOT funcIdentInner) | funcIdentTop) (l=LPAREN libFunctionArgs? RPAREN)?;

libFunctionNoClass : funcIdentChained (l=LPAREN libFunctionArgs? RPAREN)?;

funcIdentTop : escapableIdent
		| MAX
		| MIN;

funcIdentInner : escapableIdent
		| LAST
		| FIRST
		| WINDOW;

funcIdentChained : escapableIdent
		| LAST
		| FIRST
		| WINDOW
		| MAX
		| MIN
		| WHERE
		| SET
		| AFTER
		| BETWEEN;

libFunctionArgs : (ALL | DISTINCT)? libFunctionArgItem (COMMA libFunctionArgItem)*;

libFunctionArgItem : expressionLambdaDecl? expressionWithNamed;

betweenList : concatenationExpr AND_EXPR concatenationExpr;

//----------------------------------------------------------------------------
// Pattern event expressions / event pattern operators
//   Operators are: followed-by (->), or, and, not, every, where
//   Lowest precedence is listed first, order is (lowest to highest):  ->, or, and, not/every, within.
//   On the atomic level an expression has filters, and observer-statements.
//----------------------------------------------------------------------------
patternExpression
@init  { paraphrases.Push("pattern expression"); }
@after { paraphrases.Pop(); }
		: followedByExpression;

followedByExpression : orExpression (followedByRepeat)*;

followedByRepeat : (f=FOLLOWED_BY | (g=FOLLOWMAX_BEGIN expression FOLLOWMAX_END)) orExpression;

orExpression : andExpression (o=OR_EXPR andExpression)*;

andExpression :	matchUntilExpression (a=AND_EXPR matchUntilExpression)*;

matchUntilExpression : (r=matchUntilRange)? qualifyExpression (UNTIL until=qualifyExpression)?;

qualifyExpression : ((e=EVERY_EXPR | n=NOT_EXPR | d=EVERY_DISTINCT_EXPR distinctExpressionList) matchUntilRange? )? guardPostFix;

guardPostFix : (atomicExpression | l=LPAREN patternExpression RPAREN) ((wh=WHERE guardWhereExpression) | (wi=WHILE guardWhileExpression))?;

distinctExpressionList : LPAREN distinctExpressionAtom (COMMA distinctExpressionAtom)* RPAREN;

distinctExpressionAtom : expressionWithTime;

atomicExpression : observerExpression | patternFilterExpression;

observerExpression : ns=IDENT COLON (nm=IDENT | a=AT) LPAREN expressionListWithNamedWithTime? RPAREN;

guardWhereExpression : IDENT COLON IDENT LPAREN (expressionWithTimeList)? RPAREN;

guardWhileExpression : LPAREN expression RPAREN;

// syntax is [a:b] or [:b] or [a:] or [a]
matchUntilRange : LBRACK ( low=expression (c1=COLON high=expression?)? | c2=COLON upper=expression) RBRACK;

//----------------------------------------------------------------------------
// Filter expressions
//   Operators are the usual bunch =, <, >, =<, >=
//	 Ranges such as 'property in [a,b]' are allowed and ([ and )] distinguish open/closed range endpoints
//----------------------------------------------------------------------------
eventFilterExpression
@init  { paraphrases.Push("filter specification"); }
@after { paraphrases.Pop(); }
    :   (i=IDENT EQUALS)? classIdentifier (LPAREN expressionList? RPAREN)? propertyExpression?;

propertyExpression : propertyExpressionAtomic (propertyExpressionAtomic)*;

propertyExpressionAtomic : LBRACK propertyExpressionSelect? expression typeExpressionAnnotation? (AS n=IDENT)? (WHERE where=expression)? RBRACK;

propertyExpressionSelect : SELECT propertySelectionList FROM;

propertySelectionList : propertySelectionListElement (COMMA propertySelectionListElement)*;

propertySelectionListElement : s=STAR
		| propertyStreamSelector
		| expression (AS keywordAllowedIdent)?;

propertyStreamSelector : s=IDENT DOT STAR (AS i=IDENT)?;

typeExpressionAnnotation : ATCHAR n=IDENT (LPAREN v=IDENT RPAREN);

patternFilterExpression
@init  { paraphrases.Push("filter specification"); }
@after { paraphrases.Pop(); }
    		: (i=IDENT EQUALS)? classIdentifier (LPAREN expressionList? RPAREN)? propertyExpression? patternFilterAnnotation?;

patternFilterAnnotation : ATCHAR i=IDENT (LPAREN number RPAREN)?;

classIdentifier : i1=escapableStr (DOT i2=escapableStr)*;

slashIdentifier : (d=DIV)? i1=escapableStr (DIV i2=escapableStr)*;

expressionListWithNamed : expressionWithNamed (COMMA expressionWithNamed)*;

expressionListWithNamedWithTime : expressionWithNamedWithTime (COMMA expressionWithNamedWithTime)*;

expressionWithNamed : expressionNamedParameter | expressionWithTime;

expressionWithNamedWithTime : expressionNamedParameterWithTime | expressionWithTimeInclLast;

expressionNamedParameter : IDENT COLON (expression | LPAREN expressionList? RPAREN);

expressionNamedParameterWithTime : IDENT COLON (expressionWithTime | LPAREN expressionWithTimeList? RPAREN);

expressionList : expression (COMMA expression)*;

expressionWithTimeList : expressionWithTimeInclLast (COMMA expressionWithTimeInclLast)*;

expressionWithTime : lastWeekdayOperand
		| timePeriod
		| expressionQualifyable
		| rangeOperand
		| frequencyOperand
		| lastOperator
		| weekDayOperator
		| numericParameterList
		| STAR
		| propertyStreamSelector
		;

expressionWithTimeInclLast : lastOperand
		| expressionWithTime
		;

expressionQualifyable : expression (a=ASC|d=DESC|s=TIMEPERIOD_SECONDS|s=TIMEPERIOD_SECOND|s=TIMEPERIOD_SEC)?;

lastWeekdayOperand : LW;

lastOperand : LAST;

frequencyOperand : STAR DIV (number|i=IDENT|substitution);

rangeOperand : (n1=number|i1=IDENT|s1=substitution) COLON (n2=number|i2=IDENT|s2=substitution);

lastOperator : (number|i=IDENT|substitution) LAST;

weekDayOperator : (number|i=IDENT|substitution) WEEKDAY;

numericParameterList : LBRACK numericListParameter (COMMA numericListParameter)* RBRACK;

numericListParameter : rangeOperand
		| frequencyOperand
		| numberconstant;

eventProperty : eventPropertyAtomic (DOT eventPropertyAtomic)*;

eventPropertyAtomic : eventPropertyIdent (
			lb=LBRACK ni=number RBRACK (q=QUESTION)?
			|
			lp=LPAREN (s=STRING_LITERAL | s=QUOTED_STRING_LITERAL) RPAREN (q=QUESTION)?
			|
			q1=QUESTION
			)?;

eventPropertyIdent : ipi=keywordAllowedIdent (ESCAPECHAR DOT ipi2=keywordAllowedIdent?)*;

keywordAllowedIdent : i1=IDENT
		| i2=TICKED_STRING_LITERAL
		| AT
		| COUNT
		| ESCAPE
		| EVERY_EXPR
		| SCHEMA
		| SUM
		| AVG
		| MAX
		| MIN
		| COALESCE
		| MEDIAN
		| STDDEV
		| AVEDEV
		| EVENTS
		| FIRST
		| LAST
		| WHILE
		| MERGE
		| MATCHED
		| UNIDIRECTIONAL
		| RETAINUNION
		| RETAININTERSECTION
		| UNTIL
		| PATTERN
		| SQL
		| METADATASQL
		| PREVIOUS
		| PREVIOUSTAIL
		| PRIOR
		| WEEKDAY
		| LW
		| INSTANCEOF
		| TYPEOF
		| CAST
		| SNAPSHOT
		| VARIABLE
		| TABLE
		| INDEX
		| WINDOW
		| LEFT
		| RIGHT
		| OUTER
		| FULL
		| JOIN
		| DEFINE
		| PARTITION
		| MATCHES
		| CONTEXT
		| FOR
		| USING;

escapableStr : i1=IDENT | i2=EVENTS | i3=TICKED_STRING_LITERAL;

escapableIdent : IDENT | t=TICKED_STRING_LITERAL;

timePeriod : (	yearPart monthPart? weekPart? dayPart? hourPart? minutePart? secondPart? millisecondPart? microsecondPart?
		| monthPart weekPart? dayPart? hourPart? minutePart? secondPart? millisecondPart? microsecondPart?
		| weekPart dayPart? hourPart? minutePart? secondPart? millisecondPart? microsecondPart?
		| dayPart hourPart? minutePart? secondPart? millisecondPart? microsecondPart?
		| hourPart minutePart? secondPart? millisecondPart? microsecondPart?
		| minutePart secondPart? millisecondPart? microsecondPart?
		| secondPart millisecondPart? microsecondPart?
		| millisecondPart microsecondPart?
		| microsecondPart
		);

yearPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_YEARS | TIMEPERIOD_YEAR);

monthPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_MONTHS | TIMEPERIOD_MONTH);

weekPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_WEEKS | TIMEPERIOD_WEEK);

dayPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_DAYS | TIMEPERIOD_DAY);

hourPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_HOURS | TIMEPERIOD_HOUR);

minutePart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_MINUTES | TIMEPERIOD_MINUTE | MIN);

secondPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_SECONDS | TIMEPERIOD_SECOND | TIMEPERIOD_SEC);

millisecondPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_MILLISECONDS | TIMEPERIOD_MILLISECOND | TIMEPERIOD_MILLISEC);

microsecondPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_MICROSECONDS | TIMEPERIOD_MICROSECOND | TIMEPERIOD_MICROSEC);

number : IntegerLiteral | FloatingPointLiteral;

substitution : q=QUESTION (COLON slashIdentifier)?;

constant : numberconstant
		| stringconstant
		| t=BOOLEAN_TRUE
		| f=BOOLEAN_FALSE
		| nu=VALUE_NULL;

numberconstant : (m=MINUS | p=PLUS)? number;

stringconstant : sl=STRING_LITERAL
		| qsl=QUOTED_STRING_LITERAL;

//----------------------------------------------------------------------------
// JSON
//----------------------------------------------------------------------------
jsonvalue : constant
		| jsonobject
		| jsonarray;

jsonobject : LCURLY jsonmembers RCURLY;

jsonarray : LBRACK jsonelements? RBRACK;

jsonelements : jsonvalue (COMMA jsonvalue)* (COMMA)?;

jsonmembers : jsonpair (COMMA jsonpair)* (COMMA)?;

jsonpair : (stringconstant | keywordAllowedIdent) COLON jsonvalue;

//----------------------------------------------------------------------------
// LEXER
//----------------------------------------------------------------------------

// Tokens
CREATE:'create';
WINDOW:'window';
IN_SET:'in';
BETWEEN:'between';
LIKE:'like';
REGEXP:'regexp';
ESCAPE:'escape';
OR_EXPR:'or';
AND_EXPR:'and';
NOT_EXPR:'not';
EVERY_EXPR:'every';
EVERY_DISTINCT_EXPR:'every-distinct';
WHERE:'where';
AS:'as';
SUM:'sum';
AVG:'avg';
MAX:'max';
MIN:'min';
COALESCE:'coalesce';
MEDIAN:'median';
STDDEV:'stddev';
AVEDEV:'avedev';
COUNT:'count';
SELECT:'select';
CASE:'case';
ELSE:'else';
WHEN:'when';
THEN:'then';
END:'end';
FROM:'from';
OUTER:'outer';
INNER:'inner';
JOIN:'join';
LEFT:'left';
RIGHT:'right';
FULL:'full';
ON:'on';
IS:'is';
BY:'by';
GROUP:'group';
HAVING:'having';
DISTINCT:'distinct';
ALL:'all';
ANY:'any';
SOME:'some';
OUTPUT:'output';
EVENTS:'events';
FIRST:'first';
LAST:'last';
INSERT:'insert';
INTO:'into';
VALUES:'values';
ORDER:'order';
ASC:'asc';
DESC:'desc';
RSTREAM:'rstream';
ISTREAM:'istream';
IRSTREAM:'irstream';
SCHEMA:'schema';
UNIDIRECTIONAL:'unidirectional';
RETAINUNION:'retain-union';
RETAININTERSECTION:'retain-intersection';
PATTERN:'pattern';
SQL:'sql';
METADATASQL:'metadatasql';
PREVIOUS:'prev';
PREVIOUSTAIL:'prevtail';
PREVIOUSCOUNT:'prevcount';
PREVIOUSWINDOW:'prevwindow';
PRIOR:'prior';
EXISTS:'exists';
WEEKDAY:'weekday';
LW:'lastweekday';
INSTANCEOF:'instanceof';
TYPEOF:'typeof';
CAST:'cast';
CURRENT_TIMESTAMP:'current_timestamp';
DELETE:'delete';
SNAPSHOT:'snapshot';
SET:'set';
VARIABLE:'variable';
TABLE:'table';
UNTIL:'until';
AT:'at';
INDEX:'index';
TIMEPERIOD_YEAR:'year';
TIMEPERIOD_YEARS:'years';
TIMEPERIOD_MONTH:'month';
TIMEPERIOD_MONTHS:'months';
TIMEPERIOD_WEEK:'week';
TIMEPERIOD_WEEKS:'weeks';
TIMEPERIOD_DAY:'day';
TIMEPERIOD_DAYS:'days';
TIMEPERIOD_HOUR:'hour';
TIMEPERIOD_HOURS:'hours';
TIMEPERIOD_MINUTE:'minute';
TIMEPERIOD_MINUTES:'minutes';
TIMEPERIOD_SEC:'sec';
TIMEPERIOD_SECOND:'second';
TIMEPERIOD_SECONDS:'seconds';
TIMEPERIOD_MILLISEC:'msec';
TIMEPERIOD_MILLISECOND:'millisecond';
TIMEPERIOD_MILLISECONDS:'milliseconds';
TIMEPERIOD_MICROSEC:'usec';
TIMEPERIOD_MICROSECOND:'microsecond';
TIMEPERIOD_MICROSECONDS:'microseconds';
BOOLEAN_TRUE:'true';
BOOLEAN_FALSE:'false';
VALUE_NULL:'null';
ROW_LIMIT_EXPR:'limit';
OFFSET:'offset';
UPDATE:'update';
MATCH_RECOGNIZE:'match_recognize';
MATCH_RECOGNIZE_PERMUTE:'match_recognize_permute';
MEASURES:'measures';
DEFINE:'define';
PARTITION:'partition';
MATCHES:'matches';
AFTER:'after';
FOR:'for';
WHILE:'while';
USING:'using';
MERGE:'merge';
MATCHED:'matched';
EXPRESSIONDECL:'expression';
NEWKW:'new';
START:'start';
CONTEXT:'context';
INITIATED:'initiated';
TERMINATED:'terminated';
DATAFLOW:'dataflow';
CUBE:'cube';
ROLLUP:'rollup';
GROUPING:'grouping';
GROUPING_ID:'grouping_id';
SETS:'sets';

// Operators
FOLLOWMAX_BEGIN : '-[';
FOLLOWMAX_END   : ']>';
FOLLOWED_BY 	: '->';
GOES 		: '=>';
EQUALS 		: '=';
SQL_NE 		: '<>';
QUESTION 	: '?';
LPAREN 		: '(';
RPAREN 		: ')';
LBRACK 		: '[';
RBRACK 		: ']';
LCURLY 		: '{';
RCURLY 		: '}';
COLON 		: ':';
COMMA 		: ',';
EQUAL 		: '==';
LNOT 		: '!';
BNOT 		: '~';
NOT_EQUAL 	: '!=';
DIV 		: '/';
DIV_ASSIGN 	: '/=';
PLUS 		: '+';
PLUS_ASSIGN	: '+=';
INC 		: '++';
MINUS 		: '-';
MINUS_ASSIGN 	: '-=';
DEC 		: '--';
STAR 		: '*';
STAR_ASSIGN 	: '*=';
MOD 		: '%';
MOD_ASSIGN 	: '%=';
GE 		: '>=';
GT 		: '>';
LE 		: '<=';
LT 		: '<';
BXOR 		: '^';
BXOR_ASSIGN 	: '^=';
BOR		: '|';
BOR_ASSIGN 	: '|=';
LOR		: '||';
BAND 		: '&';
BAND_ASSIGN 	: '&=';
LAND 		: '&&';
SEMI 		: ';';
DOT 		: '.';
NUM_LONG	: '\u18FF';  // assign bogus unicode characters so the token exists
NUM_DOUBLE	: '\u18FE';
NUM_FLOAT	: '\u18FD';
ESCAPECHAR	: '\\';
ESCAPEBACKTICK	: '`';
ATCHAR		: '@';
HASHCHAR	: '#';

// Whitespace -- ignored
WS	:	(	' '
		|	'\t'
		|	'\f'
			// handle newlines
		|	(
				'\r'    // Macintosh
			|	'\n'    // Unix (the right way)
			)
		)+
		-> channel(HIDDEN)
	;

// Single-line comments
SL_COMMENT
	:	'//'
		(~('\n'|'\r'))* ('\n'|'\r'('\n')?)?
		-> channel(HIDDEN)
	;

// multiple-line comments
ML_COMMENT
    	:   	'/*' (.)*? '*/'
		-> channel(HIDDEN)
    	;

TICKED_STRING_LITERAL
    :   '`' ( EscapeSequence | ~('`'|'\\') )* '`'
    ;

QUOTED_STRING_LITERAL
    :   '\'' ( EscapeSequence | ~('\''|'\\') )* '\''
    ;

STRING_LITERAL
    :  '"' ( EscapeSequence | ~('\\'|'"') )* '"'
    ;

fragment
EscapeSequence	:	'\\'
		(	'n'
		|	'r'
		|	't'
		|	'b'
		|	'f'
		|	'"'
		|	'\''
		|	'\\'
		|	UnicodeEscape
		|	OctalEscape
		|	. // unknown, leave as it is
		)
    ;

// an identifier.  Note that testLiterals is set to true!  This means
// that after we match the rule, we look in the literals table to see
// if it's a literal or really an identifer
IDENT
	:	('a'..'z'|'_') ('a'..'z'|'_'|'$'|'0'..'9')*
	;

IntegerLiteral
    :   DecimalIntegerLiteral
    |   HexIntegerLiteral
    |   OctalIntegerLiteral
    |   BinaryIntegerLiteral
    ;

FloatingPointLiteral
    :   DecimalFloatingPointLiteral
    |   HexadecimalFloatingPointLiteral
    ;

fragment
OctalEscape
    :   '\\' ('0'..'3') ('0'..'7') ('0'..'7')
    |   '\\' ('0'..'7') ('0'..'7')
    |   '\\' ('0'..'7')
    ;

fragment
UnicodeEscape
    :   '\\' 'u' HexDigit HexDigit HexDigit HexDigit
    ;

fragment
DecimalIntegerLiteral
    :   DecimalNumeral IntegerTypeSuffix?
    ;

fragment
HexIntegerLiteral
    :   HexNumeral IntegerTypeSuffix?
    ;

fragment
OctalIntegerLiteral
    :   OctalNumeral IntegerTypeSuffix?
    ;

fragment
BinaryIntegerLiteral
    :   BinaryNumeral IntegerTypeSuffix?
    ;

fragment
IntegerTypeSuffix
    :   [lL]
    ;

fragment
DecimalNumeral
    :   '0'
    |   ('0')* NonZeroDigit (Digits? | Underscores Digits)
    ;

fragment
Digits
    :   Digit (DigitOrUnderscore* Digit)?
    ;

fragment
Digit
    :   '0'
    |   NonZeroDigit
    ;

fragment
NonZeroDigit
    :   [1-9]
    ;

fragment
DigitOrUnderscore
    :   Digit
    |   '_'
    ;

fragment
Underscores
    :   '_'+
    ;

fragment
HexNumeral
    :   '0' [xX] HexDigits
    ;

fragment
HexDigits
    :   HexDigit (HexDigitOrUnderscore* HexDigit)?
    ;

fragment
HexDigit
    :   [0-9a-fA-F]
    ;

fragment
HexDigitOrUnderscore
    :   HexDigit
    |   '_'
    ;

fragment
OctalNumeral
    :   '0' Underscores? OctalDigits
    ;

fragment
OctalDigits
    :   OctalDigit (OctalDigitOrUnderscore* OctalDigit)?
    ;

fragment
OctalDigit
    :   [0-7]
    ;

fragment
OctalDigitOrUnderscore
    :   OctalDigit
    |   '_'
    ;

fragment
BinaryNumeral
    :   '0' [bB] BinaryDigits
    ;

fragment
BinaryDigits
    :   BinaryDigit (BinaryDigitOrUnderscore* BinaryDigit)?
    ;

fragment
BinaryDigit
    :   [01]
    ;

fragment
BinaryDigitOrUnderscore
    :   BinaryDigit
    |   '_'
    ;

fragment
DecimalFloatingPointLiteral
    :   Digits '.' Digits? ExponentPart? FloatTypeSuffix?
    |   '.' Digits ExponentPart? FloatTypeSuffix?
    |   Digits ExponentPart FloatTypeSuffix?
    |   Digits FloatTypeSuffix
    ;

fragment
ExponentPart
    :   ExponentIndicator SignedInteger
    ;

fragment
ExponentIndicator
    :   [eE]
    ;

fragment
SignedInteger
    :   Sign? Digits
    ;

fragment
Sign
    :   [+-]
    ;

fragment
FloatTypeSuffix
    :   [fFdDm]
    ;

fragment
HexadecimalFloatingPointLiteral
    :   HexSignificand BinaryExponent FloatTypeSuffix?
    ;

fragment
HexSignificand
    :   HexNumeral '.'?
    |   '0' [xX] HexDigits? '.' HexDigits
    ;

fragment
BinaryExponent
    :   BinaryExponentIndicator SignedInteger
    ;

fragment
BinaryExponentIndicator
    :   [pP]
    ;
