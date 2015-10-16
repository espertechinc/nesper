grammar EsperEPL2Grammar;

options {
	language=CSharp;
}

@namespace { com.espertech.esper.epl.generated }
@header {
  using System;
  using System.Collections.Generic;
  
  using com.espertech.esper.compat;
  using com.espertech.esper.compat.collections;
  using com.espertech.esper.compat.logging;
}

@members {
	// provide nice error messages
	private System.Collections.Generic.Stack<string> paraphrases =
		new System.Collections.Generic.Stack<string>();

	// static information initialized once
	private static System.Collections.Generic.IDictionary<int, string> lexerTokenParaphases =
		       new System.Collections.Generic.Dictionary<int, string>();
	private static System.Collections.Generic.IDictionary<int, string> parserTokenParaphases =
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
			if (lexerTokenParaphases.Count == 0)
			{
				lexerTokenParaphases.Put(IDENT, "an identifier");
				lexerTokenParaphases.Put(FOLLOWED_BY, "an followed-by '->'");
				lexerTokenParaphases.Put(EQUALS, "an equals '='");
				lexerTokenParaphases.Put(SQL_NE, "a sql-style not equals '<>'");
				lexerTokenParaphases.Put(QUESTION, "a questionmark '?'");
				lexerTokenParaphases.Put(LPAREN, "an opening parenthesis '('");
				lexerTokenParaphases.Put(RPAREN, "a closing parenthesis ')'");
				lexerTokenParaphases.Put(LBRACK, "a left angle bracket '['");
				lexerTokenParaphases.Put(RBRACK, "a right angle bracket ']'");
				lexerTokenParaphases.Put(LCURLY, "a left curly bracket '{'");
				lexerTokenParaphases.Put(RCURLY, "a right curly bracket '}'");
				lexerTokenParaphases.Put(COLON, "a colon ':'");
				lexerTokenParaphases.Put(COMMA, "a comma ','");
				lexerTokenParaphases.Put(EQUAL, "an equals compare '=='");
				lexerTokenParaphases.Put(LNOT, "a not '!'");
				lexerTokenParaphases.Put(BNOT, "a binary not '~'");
				lexerTokenParaphases.Put(NOT_EQUAL, "a not equals '!='");
				lexerTokenParaphases.Put(DIV, "a division operator '\'");
				lexerTokenParaphases.Put(DIV_ASSIGN, "a division assign '/='");
				lexerTokenParaphases.Put(PLUS, "a plus operator '+'");
				lexerTokenParaphases.Put(PLUS_ASSIGN, "a plus assign '+='");
				lexerTokenParaphases.Put(INC, "an increment operator '++'");
				lexerTokenParaphases.Put(MINUS, "a minus '-'");
				lexerTokenParaphases.Put(MINUS_ASSIGN, "a minus assign '-='");
				lexerTokenParaphases.Put(DEC, "a decrement operator '--'");
				lexerTokenParaphases.Put(STAR, "a star '*'");
				lexerTokenParaphases.Put(STAR_ASSIGN, "a star assign '*='");
				lexerTokenParaphases.Put(MOD, "a modulo");
				lexerTokenParaphases.Put(MOD_ASSIGN, "a modulo assign");
				lexerTokenParaphases.Put(GE, "a greater equals '>='");
				lexerTokenParaphases.Put(GT, "a greater then '>'");
				lexerTokenParaphases.Put(LE, "a less equals '<='");
				lexerTokenParaphases.Put(LT, "a lesser then '<'");
				lexerTokenParaphases.Put(BXOR, "a binary xor '^'");
				lexerTokenParaphases.Put(BXOR_ASSIGN, "a binary xor assign '^='");
				lexerTokenParaphases.Put(BOR, "a binary or '|'");
				lexerTokenParaphases.Put(BOR_ASSIGN, "a binary or assign '|='");
				lexerTokenParaphases.Put(LOR, "a logical or '||'");
				lexerTokenParaphases.Put(BAND, "a binary and '&'");
				lexerTokenParaphases.Put(BAND_ASSIGN, "a binary and assign '&='");
				lexerTokenParaphases.Put(LAND, "a logical and '&&'");
				lexerTokenParaphases.Put(SEMI, "a semicolon ';'");
				lexerTokenParaphases.Put(DOT, "a dot '.'");
			}
		}
		
		return lexerTokenParaphases;
	}

	public static System.Collections.Generic.IDictionary<int, string> GetParserTokenParaphrases()
	{
		lock(_iLock)
		{ 
			if (parserTokenParaphases.Count == 0)
			{
				parserTokenParaphases.Put(CREATE, "'create'");
				parserTokenParaphases.Put(WINDOW, "'window'");
				parserTokenParaphases.Put(IN_SET, "'in'");
				parserTokenParaphases.Put(BETWEEN, "'between'");
				parserTokenParaphases.Put(LIKE, "'like'");
				parserTokenParaphases.Put(REGEXP, "'regexp'");
				parserTokenParaphases.Put(ESCAPE, "'escape'");
				parserTokenParaphases.Put(OR_EXPR, "'or'");
				parserTokenParaphases.Put(AND_EXPR, "'and'");
				parserTokenParaphases.Put(NOT_EXPR, "'not'");
				parserTokenParaphases.Put(EVERY_EXPR, "'every'");
				parserTokenParaphases.Put(EVERY_DISTINCT_EXPR, "'every-distinct'");
				parserTokenParaphases.Put(WHERE, "'where'");
				parserTokenParaphases.Put(AS, "'as'");	
				parserTokenParaphases.Put(SUM, "'sum'");
				parserTokenParaphases.Put(AVG, "'avg'");
				parserTokenParaphases.Put(MAX, "'max'");
				parserTokenParaphases.Put(MIN, "'min'");
				parserTokenParaphases.Put(COALESCE, "'coalesce'");
				parserTokenParaphases.Put(MEDIAN, "'median'");
				parserTokenParaphases.Put(STDDEV, "'stddev'");
				parserTokenParaphases.Put(AVEDEV, "'avedev'");
				parserTokenParaphases.Put(COUNT, "'count'");
				parserTokenParaphases.Put(SELECT, "'select'");
				parserTokenParaphases.Put(CASE, "'case'");
				parserTokenParaphases.Put(ELSE, "'else'");
				parserTokenParaphases.Put(WHEN, "'when'");
				parserTokenParaphases.Put(THEN, "'then'");
				parserTokenParaphases.Put(END, "'end'");
				parserTokenParaphases.Put(FROM, "'from'");
				parserTokenParaphases.Put(OUTER, "'outer'");
				parserTokenParaphases.Put(INNER, "'inner'");
				parserTokenParaphases.Put(JOIN, "'join'");
				parserTokenParaphases.Put(LEFT, "'left'");
				parserTokenParaphases.Put(RIGHT, "'right'");
				parserTokenParaphases.Put(FULL, "'full'");
				parserTokenParaphases.Put(ON, "'on'");	
				parserTokenParaphases.Put(IS, "'is'");
				parserTokenParaphases.Put(BY, "'by'");
				parserTokenParaphases.Put(GROUP, "'group'");
				parserTokenParaphases.Put(HAVING, "'having'");
				parserTokenParaphases.Put(ALL, "'all'");
				parserTokenParaphases.Put(ANY, "'any'");
				parserTokenParaphases.Put(SOME, "'some'");
				parserTokenParaphases.Put(OUTPUT, "'output'");
				parserTokenParaphases.Put(EVENTS, "'events'");
				parserTokenParaphases.Put(FIRST, "'first'");
				parserTokenParaphases.Put(LAST, "'last'");
				parserTokenParaphases.Put(INSERT, "'insert'");
				parserTokenParaphases.Put(INTO, "'into'");
				parserTokenParaphases.Put(ORDER, "'order'");
				parserTokenParaphases.Put(ASC, "'asc'");
				parserTokenParaphases.Put(DESC, "'desc'");
				parserTokenParaphases.Put(RSTREAM, "'rstream'");
				parserTokenParaphases.Put(ISTREAM, "'istream'");
				parserTokenParaphases.Put(IRSTREAM, "'irstream'");
				parserTokenParaphases.Put(SCHEMA, "'schema'");
				parserTokenParaphases.Put(UNIDIRECTIONAL, "'unidirectional'");
				parserTokenParaphases.Put(RETAINUNION, "'retain-union'");
				parserTokenParaphases.Put(RETAININTERSECTION, "'retain-intersection'");
				parserTokenParaphases.Put(PATTERN, "'pattern'");
				parserTokenParaphases.Put(SQL, "'sql'");
				parserTokenParaphases.Put(METADATASQL, "'metadatasql'");
				parserTokenParaphases.Put(PREVIOUS, "'prev'");
				parserTokenParaphases.Put(PREVIOUSTAIL, "'prevtail'");
				parserTokenParaphases.Put(PREVIOUSCOUNT, "'prevcount'");
				parserTokenParaphases.Put(PREVIOUSWINDOW, "'prevwindow'");
				parserTokenParaphases.Put(PRIOR, "'prior'");
				parserTokenParaphases.Put(EXISTS, "'exists'");
				parserTokenParaphases.Put(WEEKDAY, "'weekday'");
				parserTokenParaphases.Put(LW, "'lastweekday'");
				parserTokenParaphases.Put(INSTANCEOF, "'instanceof'");
				parserTokenParaphases.Put(TYPEOF, "'typeof'");
				parserTokenParaphases.Put(CAST, "'cast'");
				parserTokenParaphases.Put(CURRENT_TIMESTAMP, "'current_timestamp'");
				parserTokenParaphases.Put(DELETE, "'delete'");
				parserTokenParaphases.Put(DISTINCT, "'distinct'");
				parserTokenParaphases.Put(SNAPSHOT, "'snapshot'");
				parserTokenParaphases.Put(SET, "'set'");
				parserTokenParaphases.Put(VARIABLE, "'variable'");
				parserTokenParaphases.Put(TABLE, "'table'");
				parserTokenParaphases.Put(INDEX, "'index'");
				parserTokenParaphases.Put(UNTIL, "'until'");
				parserTokenParaphases.Put(AT, "'at'");
				parserTokenParaphases.Put(TIMEPERIOD_YEAR, "'year'");
				parserTokenParaphases.Put(TIMEPERIOD_YEARS, "'years'");
				parserTokenParaphases.Put(TIMEPERIOD_MONTH, "'month'");
				parserTokenParaphases.Put(TIMEPERIOD_MONTHS, "'months'");
				parserTokenParaphases.Put(TIMEPERIOD_WEEK, "'week'");
				parserTokenParaphases.Put(TIMEPERIOD_WEEKS, "'weeks'");
				parserTokenParaphases.Put(TIMEPERIOD_DAY, "'day'");
				parserTokenParaphases.Put(TIMEPERIOD_DAYS, "'days'");
				parserTokenParaphases.Put(TIMEPERIOD_HOUR, "'hour'");
				parserTokenParaphases.Put(TIMEPERIOD_HOURS, "'hours'");
				parserTokenParaphases.Put(TIMEPERIOD_MINUTE, "'minute'");
				parserTokenParaphases.Put(TIMEPERIOD_MINUTES, "'minutes'");
				parserTokenParaphases.Put(TIMEPERIOD_SEC, "'sec'");
				parserTokenParaphases.Put(TIMEPERIOD_SECOND, "'second'");
				parserTokenParaphases.Put(TIMEPERIOD_SECONDS, "'seconds'");
				parserTokenParaphases.Put(TIMEPERIOD_MILLISEC, "'msec'");
				parserTokenParaphases.Put(TIMEPERIOD_MILLISECOND, "'millisecond'");
				parserTokenParaphases.Put(TIMEPERIOD_MILLISECONDS, "'milliseconds'");
				parserTokenParaphases.Put(BOOLEAN_TRUE, "'true'");
				parserTokenParaphases.Put(BOOLEAN_FALSE, "'false'");
				parserTokenParaphases.Put(VALUE_NULL, "'null'");
				parserTokenParaphases.Put(ROW_LIMIT_EXPR, "'limit'");
				parserTokenParaphases.Put(OFFSET, "'offset'");
				parserTokenParaphases.Put(UPDATE, "'update'");
				parserTokenParaphases.Put(MATCH_RECOGNIZE, "'match_recognize'");
				parserTokenParaphases.Put(MEASURES, "'measures'");
				parserTokenParaphases.Put(DEFINE, "'define'");
				parserTokenParaphases.Put(PARTITION, "'partition'");
				parserTokenParaphases.Put(MATCHES, "'matches'");
				parserTokenParaphases.Put(AFTER, "'after'");
				parserTokenParaphases.Put(FOR, "'for'");
				parserTokenParaphases.Put(WHILE, "'while'");
				parserTokenParaphases.Put(MERGE, "'merge'");
				parserTokenParaphases.Put(MATCHED, "'matched'");
				parserTokenParaphases.Put(CONTEXT, "'context'");
				parserTokenParaphases.Put(START, "'start'");
				parserTokenParaphases.Put(END, "'end'");
				parserTokenParaphases.Put(INITIATED, "'initiated'");
				parserTokenParaphases.Put(TERMINATED, "'terminated'");
				parserTokenParaphases.Put(USING, "'using'");
				parserTokenParaphases.Put(EXPRESSIONDECL, "'expression'");
				parserTokenParaphases.Put(NEWKW, "'new'");
				parserTokenParaphases.Put(DATAFLOW, "'dataflow'");
				parserTokenParaphases.Put(VALUES, "'values'");
				parserTokenParaphases.Put(CUBE, "'cube'");
				parserTokenParaphases.Put(ROLLUP, "'rollup'");
				parserTokenParaphases.Put(GROUPING, "'grouping'");
				parserTokenParaphases.Put(GROUPING_ID, "'grouping_id'");
				parserTokenParaphases.Put(SETS, "'sets'");

				parserKeywordSet = new HashSet<string>(
					parserTokenParaphases.Values);
			}
		}

		return parserTokenParaphases;
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
expressionDecl : EXPRESSIONDECL classIdentifier? (array=LBRACK RBRACK)? expressionDialect? name=IDENT (LPAREN columnList? RPAREN)? (alias=IDENT FOR)? expressionDef;

expressionDialect : d=IDENT COLON;
	
expressionDef :	LCURLY expressionLambdaDecl? expression RCURLY 		
		| LBRACK stringconstant RBRACK 
		;

expressionLambdaDecl : (i=IDENT | (LPAREN columnList RPAREN)) (GOES | FOLLOWED_BY);

//----------------------------------------------------------------------------
// Annotations
//----------------------------------------------------------------------------
annotationEnum : ATCHAR classIdentifier ( '(' ( elementValuePairsEnum | elementValueEnum )? ')' )?;
    
elementValuePairsEnum : elementValuePairEnum (COMMA elementValuePairEnum)*;
    
elementValuePairEnum : i=IDENT '=' elementValueEnum;
    
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
		: INSERT insertIntoExpr SELECT selectionList (WHERE whereClause)?;
	
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

createWindowExpr : CREATE WINDOW i=IDENT (DOT viewExpression (DOT viewExpression)*)? (ru=RETAINUNION|ri=RETAININTERSECTION)? AS? 
		  (
		  	createWindowExprModelAfter		  
		  |   	LPAREN createColumnList RPAREN
		  )		
		  (i1=INSERT (WHERE expression)? )?;

createWindowExprModelAfter : (SELECT createSelectionList FROM)? classIdentifier;
		
createIndexExpr : CREATE (u=IDENT)? INDEX n=IDENT ON w=IDENT LPAREN createIndexColumnList RPAREN;
	
createIndexColumnList : createIndexColumn (COMMA createIndexColumn)*;	

createIndexColumn : c=IDENT t=IDENT?;	

createVariableExpr : CREATE c=IDENT? VARIABLE classIdentifier (arr=LBRACK p=IDENT? RBRACK)? n=IDENT (EQUALS expression)?;

createTableExpr : CREATE TABLE n=IDENT AS? LPAREN createTableColumnList RPAREN; 

createTableColumnList : createTableColumn (COMMA createTableColumn)*;

createTableColumn : n=IDENT (createTableColumnPlain | builtinFunc | libFunction) p=IDENT? k=IDENT? (propertyExpressionAnnotation | annotationEnum)*;

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
	
createContextChoice : START (ATCHAR i=IDENT | r1=createContextRangePoint) END r2=createContextRangePoint
		| INITIATED (BY)? createContextDistinct? (ATCHAR i=IDENT AND_EXPR)? r1=createContextRangePoint TERMINATED (BY)? r2=createContextRangePoint
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
		(DOT viewExpression (DOT viewExpression)*)? (AS i=IDENT | i=IDENT)? (u=UNIDIRECTIONAL)? (ru=RETAINUNION|ri=RETAININTERSECTION)?;
		
forExpr : FOR i=IDENT (LPAREN expressionList? RPAREN)?;


patternInclusionExpression : PATTERN annotationEnum* LBRACK patternExpression RBRACK;
	
databaseJoinExpression
@init  { paraphrases.Push("relational data join"); }
@after { paraphrases.Pop(); }
		: SQL COLON i=IDENT LBRACK (s=STRING_LITERAL | s=QUOTED_STRING_LITERAL) (METADATASQL (s2=STRING_LITERAL | s2=QUOTED_STRING_LITERAL))? RBRACK;	
	
methodJoinExpression
@init  { paraphrases.Push("method invocation join"); }
@after { paraphrases.Pop(); }
    		: i=IDENT COLON classIdentifier (LPAREN expressionList? RPAREN)?;

viewExpression
@init  { paraphrases.Push("view specifications"); }
@after { paraphrases.Pop(); }
		: ns=IDENT COLON (i=IDENT|m=MERGE) LPAREN expressionWithTimeList? RPAREN;

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
		: LPAREN  SELECT DISTINCT? selectionList FROM subSelectFilterExpr (WHERE whereClause)? (GROUP BY groupByListExpr)? RPAREN;
	
subSelectFilterExpr
@init  { paraphrases.Push("subquery filter specification"); }
@after { paraphrases.Pop(); }
		: eventFilterExpression (DOT viewExpression (DOT viewExpression)*)? (AS i=IDENT | i=IDENT)? (ru=RETAINUNION|ri=RETAININTERSECTION)?;
		
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
		| PRIOR LPAREN number COMMA eventProperty RPAREN				#builtin_prior
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

propertyExpressionAtomic : LBRACK propertyExpressionSelect? expression propertyExpressionAnnotation? (AS n=IDENT)? (WHERE where=expression)? RBRACK;
       	
propertyExpressionSelect : SELECT propertySelectionList FROM;
		
propertyExpressionAnnotation : ATCHAR n=IDENT (LPAREN v=IDENT RPAREN);
	
propertySelectionList : propertySelectionListElement (COMMA propertySelectionListElement)*;

propertySelectionListElement : s=STAR
		| propertyStreamSelector
		| expression (AS keywordAllowedIdent)?;
	
propertyStreamSelector : s=IDENT DOT STAR (AS i=IDENT)?;

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

timePeriod : (	yearPart monthPart? weekPart? dayPart? hourPart? minutePart? secondPart? millisecondPart?
		| monthPart weekPart? dayPart? hourPart? minutePart? secondPart? millisecondPart?
		| weekPart dayPart? hourPart? minutePart? secondPart? millisecondPart?
		| dayPart hourPart? minutePart? secondPart? millisecondPart?
		| hourPart minutePart? secondPart? millisecondPart?
		| minutePart secondPart? millisecondPart?
		| secondPart millisecondPart?
		| millisecondPart
		);

yearPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_YEARS | TIMEPERIOD_YEAR);

monthPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_MONTHS | TIMEPERIOD_MONTH);

weekPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_WEEKS | TIMEPERIOD_WEEK);

dayPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_DAYS | TIMEPERIOD_DAY);

hourPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_HOURS | TIMEPERIOD_HOUR);

minutePart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_MINUTES | TIMEPERIOD_MINUTE | MIN);
	
secondPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_SECONDS | TIMEPERIOD_SECOND | TIMEPERIOD_SEC);
	
millisecondPart : (numberconstant|i=IDENT|substitution) (TIMEPERIOD_MILLISECONDS | TIMEPERIOD_MILLISECOND | TIMEPERIOD_MILLISEC);
	
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
    :   [fFdD]
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