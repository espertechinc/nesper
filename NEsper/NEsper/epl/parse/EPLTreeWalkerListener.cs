///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events.property;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.guard;
using com.espertech.esper.plugin;
using com.espertech.esper.rowregex;
using com.espertech.esper.schedule;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Called during the walks of a EPL expression AST tree as specified in the grammar file.
    /// Constructs filter and view specifications etc.
    /// </summary>
    public class EPLTreeWalkerListener : IEsperEPL2GrammarListener
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private static readonly ISet<int> EVENT_FILTER_WALK_EXCEPTIONS_RECURSIVE = new HashSet<int>();
        private static readonly ISet<int> WHERE_CLAUSE_WALK_EXCEPTIONS_RECURSIVE = new HashSet<int>();
        private static readonly ISet<int> EVENT_PROPERTY_WALK_EXCEPTIONS_PARENT = new HashSet<int>();
        private static readonly ISet<int> SELECT_EXPRELE_WALK_EXCEPTIONS_RECURSIVE = new HashSet<int>();

        static EPLTreeWalkerListener()
        {
            EVENT_FILTER_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_createContextDetail);
            EVENT_FILTER_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_createContextFilter);
            EVENT_FILTER_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_createContextPartitionItem);
            EVENT_FILTER_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_createContextCoalesceItem);

            WHERE_CLAUSE_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_patternExpression);
            WHERE_CLAUSE_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_mergeMatchedItem);
            WHERE_CLAUSE_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_mergeInsert);
            WHERE_CLAUSE_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_updateDetails);
            WHERE_CLAUSE_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_onSetExpr);
            WHERE_CLAUSE_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_onUpdateExpr);

            EVENT_PROPERTY_WALK_EXCEPTIONS_PARENT.Add(EsperEPL2GrammarParser.RULE_newAssign);
            EVENT_PROPERTY_WALK_EXCEPTIONS_PARENT.Add(EsperEPL2GrammarParser.RULE_createContextPartitionItem);
            EVENT_PROPERTY_WALK_EXCEPTIONS_PARENT.Add(EsperEPL2GrammarParser.RULE_createContextDetail);
            EVENT_PROPERTY_WALK_EXCEPTIONS_PARENT.Add(EsperEPL2GrammarParser.RULE_createContextFilter);
            EVENT_PROPERTY_WALK_EXCEPTIONS_PARENT.Add(EsperEPL2GrammarParser.RULE_createContextCoalesceItem);

            SELECT_EXPRELE_WALK_EXCEPTIONS_RECURSIVE.Add(EsperEPL2GrammarParser.RULE_mergeInsert);
        }

        private readonly Stack<IDictionary<ITree, ExprNode>> _astExprNodeMapStack;
        private readonly IDictionary<ITree, EvalFactoryNode> _astPatternNodeMap = new LinkedHashMap<ITree, EvalFactoryNode>();
        private readonly IDictionary<ITree, RowRegexExprNode> _astRowRegexNodeMap = new HashMap<ITree, RowRegexExprNode>();
        private readonly IDictionary<ITree, Object> _astGopNodeMap = new HashMap<ITree, Object>();
        private readonly IDictionary<ITree, StatementSpecRaw> _astStatementSpecMap = new HashMap<ITree, StatementSpecRaw>();
        private readonly IList<ViewSpec> _viewSpecs = new List<ViewSpec>();
        private readonly Stack<StatementSpecRaw> _statementSpecStack;
        private readonly CommonTokenStream _tokenStream;
        private readonly EngineImportService _engineImportService;
        private readonly VariableService _variableService;
        private readonly TimeProvider _timeProvider;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly SelectClauseStreamSelectorEnum _defaultStreamSelector;
        private readonly string _engineURI;
        private readonly ConfigurationInformation _configurationInformation;
        private readonly SchedulingService _schedulingService;
        private readonly PatternNodeFactory _patternNodeFactory;
        private readonly ContextManagementService _contextManagementService;
        private readonly IList<string> _scriptBodies;
        private readonly ExprDeclaredService _exprDeclaredService;
        private readonly IList<ExpressionScriptProvided> _scriptExpressions;
        private readonly ExpressionDeclDesc _expressionDeclarations;
        private readonly TableService _tableService;
        // private holding areas for accumulated INFO
        private IDictionary<ITree, ExprNode> _astExprNodeMap = new LinkedHashMap<ITree, ExprNode>();
        private IDictionary<StatementSpecRaw, OnTriggerSplitStreamFromClause> _onTriggerSplitPropertyEvals;
        private readonly LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory> _plugInAggregations = new LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory>();
        private FilterSpecRaw _filterSpec;
        // AST Walk result
        private readonly IList<ExprSubstitutionNode> _substitutionParamNodes = new List<ExprSubstitutionNode>();
        private StatementSpecRaw _statementSpec;
        private IList<SelectClauseElementRaw> _propertySelectRaw;
        private PropertyEvalSpec _propertyEvalSpec;
        private IList<OnTriggerMergeMatched> _mergeMatcheds;
        private IList<OnTriggerMergeAction> _mergeActions;
        private ContextDescriptor _contextDescriptor;

        private readonly IContainer _container;

        public EPLTreeWalkerListener(
            IContainer container,
            CommonTokenStream tokenStream,
            EngineImportService engineImportService,
            VariableService variableService,
            SchedulingService schedulingService,
            SelectClauseStreamSelectorEnum? defaultStreamSelector,
            string engineURI,
            ConfigurationInformation configurationInformation,
            PatternNodeFactory patternNodeFactory,
            ContextManagementService contextManagementService,
            IList<string> scriptBodies,
            ExprDeclaredService exprDeclaredService,
            TableService tableService)
        {
            _container = container;
            _tokenStream = tokenStream;
            _engineImportService = engineImportService;
            _variableService = variableService;
            _timeProvider = schedulingService;
            _patternNodeFactory = patternNodeFactory;
            _exprEvaluatorContext = new ExprEvaluatorContextTimeOnly(container, _timeProvider);
            _engineURI = engineURI;
            _configurationInformation = configurationInformation;
            _schedulingService = schedulingService;
            _contextManagementService = contextManagementService;
            _scriptBodies = scriptBodies;
            _exprDeclaredService = exprDeclaredService;
            _tableService = tableService;

            if (defaultStreamSelector == null) {
                throw ASTWalkException.From("Default stream selector is null");
            }

            _defaultStreamSelector = defaultStreamSelector.Value;
            _statementSpec = new StatementSpecRaw(defaultStreamSelector.Value);
            _statementSpecStack = new Stack<StatementSpecRaw>();
            _astExprNodeMapStack = new Stack<IDictionary<ITree, ExprNode>>();
    
            // statement-global items
            _expressionDeclarations = new ExpressionDeclDesc();
            _statementSpec.ExpressionDeclDesc = _expressionDeclarations;
            _scriptExpressions = new List<ExpressionScriptProvided>();
            _statementSpec.ScriptExpressions = _scriptExpressions;
        }
    
        /// <summary>
        /// Pushes a statement into the stack, creating a new empty statement to fill in.
        /// The leave node method for lookup statements pops from the stack.
        /// The leave node method for lookup statements pops from the stack.
        /// </summary>
        private void PushStatementContext() {
            _statementSpecStack.Push(_statementSpec);
            _astExprNodeMapStack.Push(_astExprNodeMap);
    
            _statementSpec = new StatementSpecRaw(_defaultStreamSelector);
            _astExprNodeMap = new HashMap<ITree, ExprNode>();
        }
    
        private void PopStatementContext(IParseTree ctx) {
            var currentSpec = _statementSpec;
            _statementSpec = _statementSpecStack.Pop();
            if (currentSpec.HasVariables) {
                _statementSpec.HasVariables = true;
            }
            ASTTableExprHelper.AddTableExpressionReference(_statementSpec, currentSpec.TableExpressions);
            if (currentSpec.ReferencedVariables != null) {
                foreach (var var in currentSpec.ReferencedVariables) {
                    ASTExprHelper.AddVariableReference(_statementSpec, var);
                }
            }
            _astExprNodeMap = _astExprNodeMapStack.Pop();
            _astStatementSpecMap.Put(ctx, currentSpec);
        }

        public StatementSpecRaw StatementSpec
        {
            get { return _statementSpec; }
        }

        public void ExitContextExpr(EsperEPL2GrammarParser.ContextExprContext ctx) {
            var contextName = ctx.i.Text;
            _statementSpec.OptionalContextName = contextName;
            _contextDescriptor = _contextManagementService.GetContextDescriptor(contextName);
        }
    
        public void ExitEvalRelationalExpression(EsperEPL2GrammarParser.EvalRelationalExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var isNot = ctx.n != null;
            ExprNode exprNode;
            if (ctx.like != null) {
                exprNode = new ExprLikeNode(isNot);
            } else if (ctx.@in != null && ctx.col != null) { // range
                var isLowInclude = ctx.LBRACK() != null;
                var isHighInclude = ctx.RBRACK() != null;
                exprNode = new ExprBetweenNodeImpl(isLowInclude, isHighInclude, isNot);
            } else if (ctx.@in != null) {
                exprNode = new ExprInNodeImpl(isNot);
            } else if (ctx.inSubSelectQuery() != null) {
                var currentSpec = _astStatementSpecMap.Delete(ctx.inSubSelectQuery().subQueryExpr());
                exprNode = new ExprSubselectInNode(currentSpec, isNot);
            } else if (ctx.between != null) {
                exprNode = new ExprBetweenNodeImpl(true, true, isNot);
            } else if (ctx.regex != null) {
                exprNode = new ExprRegexpNode(isNot);
            } else if (ctx.r != null) {
                RelationalOpEnum relationalOpEnum;
                switch (ctx.r.Type) {
                    case EsperEPL2GrammarLexer.LT:
                        relationalOpEnum = RelationalOpEnum.LT;
                        break;
                    case EsperEPL2GrammarLexer.GT:
                        relationalOpEnum = RelationalOpEnum.GT;
                        break;
                    case EsperEPL2GrammarLexer.LE:
                        relationalOpEnum = RelationalOpEnum.LE;
                        break;
                    case EsperEPL2GrammarLexer.GE:
                        relationalOpEnum = RelationalOpEnum.GE;
                        break;
                    default:
                        throw ASTWalkException.From("Encountered unrecognized node type " + ctx.r.Type, _tokenStream, ctx);
                }
    
                var isAll = ctx.g != null && ctx.g.Type == EsperEPL2GrammarLexer.ALL;
                var isAny = ctx.g != null && (ctx.g.Type == EsperEPL2GrammarLexer.ANY || ctx.g.Type == EsperEPL2GrammarLexer.SOME);
    
                if (isAll || isAny) {
                    if (ctx.subSelectGroupExpression() != null && !ctx.subSelectGroupExpression().IsEmpty()) {
                        StatementSpecRaw currentSpec = _astStatementSpecMap.Delete(ctx.subSelectGroupExpression()[0].subQueryExpr());
                        exprNode = new ExprSubselectAllSomeAnyNode(currentSpec, false, isAll, relationalOpEnum);
                    } else {
                        exprNode = new ExprRelationalOpAllAnyNode(relationalOpEnum, isAll);
                    }
                } else {
                    exprNode = new ExprRelationalOpNodeImpl(relationalOpEnum);
                }
            } else {
                throw ASTWalkException.From("Encountered unrecognized relational op", _tokenStream, ctx);
            }
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
            if (ctx.like != null && ctx.stringconstant() != null) {
                exprNode.AddChildNode(new ExprConstantNodeImpl(ASTConstantHelper.Parse(ctx.stringconstant())));
            }
        }
    
        public void ExitLibFunction(EsperEPL2GrammarParser.LibFunctionContext ctx) {
            ASTLibFunctionHelper.HandleLibFunc(
                _container,
                _tokenStream, ctx, 
                _configurationInformation, 
                _engineImportService, 
                _astExprNodeMap, 
                _plugInAggregations, 
                _engineURI, 
                _expressionDeclarations, 
                _exprDeclaredService, 
                _scriptExpressions, 
                _contextDescriptor, 
                _tableService, 
                _statementSpec, 
                _variableService);
        }
    
        public void ExitMatchRecog(EsperEPL2GrammarParser.MatchRecogContext ctx) {
            var allMatches = ctx.matchRecogMatchesSelection() != null && ctx.matchRecogMatchesSelection().ALL() != null;
            if (ctx.matchRecogMatchesAfterSkip() != null) {
                var skip = ASTMatchRecognizeHelper.ParseSkip(_tokenStream, ctx.matchRecogMatchesAfterSkip());
                _statementSpec.MatchRecognizeSpec.Skip.Skip = skip;
            }
    
            if (ctx.matchRecogMatchesInterval() != null) {
                if (!ctx.matchRecogMatchesInterval().i.Text.ToLowerInvariant().Equals("interval")) {
                    throw ASTWalkException.From("Invalid interval-clause within match-recognize, expecting keyword INTERVAL", _tokenStream, ctx.matchRecogMatchesInterval());
                }
                var expression = ASTExprHelper.ExprCollectSubNodes(ctx.matchRecogMatchesInterval().timePeriod(), 0, _astExprNodeMap)[0];
                var timePeriodExpr = (ExprTimePeriod) expression;
                var orTerminated = ctx.matchRecogMatchesInterval().TERMINATED() != null;
                _statementSpec.MatchRecognizeSpec.Interval = new MatchRecognizeInterval(timePeriodExpr, orTerminated);
            }
    
            _statementSpec.MatchRecognizeSpec.IsAllMatches = allMatches;
        }
    
        public void ExitMatchRecogPartitionBy(EsperEPL2GrammarParser.MatchRecogPartitionByContext ctx) {
            if (_statementSpec.MatchRecognizeSpec == null) {
                _statementSpec.MatchRecognizeSpec = new MatchRecognizeSpec();
            }
            var nodes = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap);
            _statementSpec.MatchRecognizeSpec.PartitionByExpressions.AddAll(nodes);
        }
    
        public void ExitMergeMatchedItem(EsperEPL2GrammarParser.MergeMatchedItemContext ctx) {
            if (_mergeActions == null) {
                _mergeActions = new List<OnTriggerMergeAction>();
            }
            ExprNode whereCond = null;
            if (ctx.whereClause() != null) {
                whereCond = ASTExprHelper.ExprCollectSubNodes(ctx.whereClause(), 0, _astExprNodeMap)[0];
            }
            if (ctx.d != null) {
                _mergeActions.Add(new OnTriggerMergeActionDelete(whereCond));
            }
            if (ctx.u != null) {
                var sets = ASTExprHelper.GetOnTriggerSetAssignments(ctx.onSetAssignmentList(), _astExprNodeMap);
                _mergeActions.Add(new OnTriggerMergeActionUpdate(whereCond, sets));
            }
            if (ctx.mergeInsert() != null) {
                HandleMergeInsert(ctx.mergeInsert());
            }
        }
    
        public void EnterSubQueryExpr(EsperEPL2GrammarParser.SubQueryExprContext ctx) {
            PushStatementContext();
        }
    
        public void ExitSubQueryExpr(EsperEPL2GrammarParser.SubQueryExprContext ctx) {
            PopStatementContext(ctx);
        }
    
        public void ExitMatchRecogDefineItem(EsperEPL2GrammarParser.MatchRecogDefineItemContext ctx) {
            var first = ctx.i.Text;
            var exprNode = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap)[0];
            _statementSpec.MatchRecognizeSpec.Defines.Add(new MatchRecognizeDefineItem(first, exprNode));
        }
    
        public void ExitMergeUnmatchedItem(EsperEPL2GrammarParser.MergeUnmatchedItemContext ctx) {
            if (_mergeActions == null) {
                _mergeActions = new List<OnTriggerMergeAction>();
            }
            HandleMergeInsert(ctx.mergeInsert());
        }
    
        public void ExitHavingClause(EsperEPL2GrammarParser.HavingClauseContext ctx) {
            if (_astExprNodeMap.Count != 1) {
                throw new IllegalStateException("Having clause generated zero or more then one expression nodes");
            }
            _statementSpec.HavingExprRootNode = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap)[0];
            _astExprNodeMap.Clear();
        }
    
        public void ExitMatchRecogMeasureItem(EsperEPL2GrammarParser.MatchRecogMeasureItemContext ctx) {
            if (_statementSpec.MatchRecognizeSpec == null) {
                _statementSpec.MatchRecognizeSpec = new MatchRecognizeSpec();
            }
            var exprNode = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap)[0];
            var name = ctx.i != null ? ctx.i.Text : null;
            _statementSpec.MatchRecognizeSpec.AddMeasureItem(new MatchRecognizeMeasureItem(exprNode, name));
        }
    
        public void ExitObserverExpression(EsperEPL2GrammarParser.ObserverExpressionContext ctx) {
            var objectNamespace = ctx.ns.Text;
            var objectName = ctx.a != null ? ctx.a.Text : ctx.nm.Text;
            var obsParameters = ASTExprHelper.ExprCollectSubNodes(ctx, 2, _astExprNodeMap);
    
            var observerSpec = new PatternObserverSpec(objectNamespace, objectName, obsParameters);
            var observerNode = _patternNodeFactory.MakeObserverNode(observerSpec);
            ASTExprHelper.PatternCollectAddSubnodesAddParentNode(observerNode, ctx, _astPatternNodeMap);
        }
    
        public void ExitMatchRecogPatternNested(EsperEPL2GrammarParser.MatchRecogPatternNestedContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var type = RegexNFATypeEnum.SINGLE;
            if (ctx.s != null) {
                type = RegexNFATypeEnumExtensions.FromString(ctx.s.Text, null);
            }
            var repeat = ASTMatchRecognizeHelper.walkOptionalRepeat(ctx.matchRecogPatternRepeat(), _astExprNodeMap);
            var nestedNode = new RowRegexExprNodeNested(type, repeat);
            ASTExprHelper.RegExCollectAddSubNodesAddParentNode(nestedNode, ctx, _astRowRegexNodeMap);
        }
    
        public void ExitMatchRecogPatternPermute(EsperEPL2GrammarParser.MatchRecogPatternPermuteContext ctx) {
            var permuteNode = new RowRegexExprNodePermute();
            ASTExprHelper.RegExCollectAddSubNodesAddParentNode(permuteNode, ctx, _astRowRegexNodeMap);
        }
    
        public void ExitEvalOrExpression(EsperEPL2GrammarParser.EvalOrExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var or = new ExprOrNode();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(or, ctx, _astExprNodeMap);
        }
    
        public void ExitTimePeriod(EsperEPL2GrammarParser.TimePeriodContext ctx)
        {
            var timeNode = ASTExprHelper.TimePeriodGetExprAllParams(
                ctx,
                _astExprNodeMap,
                _variableService,
                _statementSpec,
                _configurationInformation,
                _engineImportService.TimeAbacus,
                _container.Resolve<ILockManager>());
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(timeNode, ctx, _astExprNodeMap);
        }
    
        public void ExitSelectionListElementExpr(EsperEPL2GrammarParser.SelectionListElementExprContext ctx) {
            ExprNode exprNode;
            if (ASTUtil.IsRecursiveParentRule(ctx, SELECT_EXPRELE_WALK_EXCEPTIONS_RECURSIVE)) {
                exprNode = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap)[0];
            } else {
                if ((_astExprNodeMap.Count > 1) || ((_astExprNodeMap.IsEmpty()))) {
                    throw ASTWalkException.From("Unexpected AST tree contains zero or more then 1 child element for root", _tokenStream, ctx);
                }
                exprNode = _astExprNodeMap.Values.First();
                _astExprNodeMap.Clear();
            }
    
            // Get list element name
            string optionalName = null;
            if (ctx.keywordAllowedIdent() != null) {
                optionalName = ctx.keywordAllowedIdent().GetText();
            }
    
            var eventsAnnotation = false;
            if (ctx.selectionListElementAnno() != null) {
                var annotation = ctx.selectionListElementAnno().i.Text.ToLowerInvariant();
                if (annotation.Equals("eventbean") || annotation.Equals("eventbean")) {
                    eventsAnnotation = true;
                } else {
                    throw ASTWalkException.From("Failed to recognize select-expression annotation '" + annotation + "', expected 'eventbean'", _tokenStream, ctx);
                }
            }
    
            // Add as selection element
            _statementSpec.SelectClauseSpec.Add(new SelectClauseExprRawSpec(exprNode, optionalName, eventsAnnotation));
        }
    
        public void ExitEventFilterExpression(EsperEPL2GrammarParser.EventFilterExpressionContext ctx) {
            if (ASTUtil.IsRecursiveParentRule(ctx, EVENT_FILTER_WALK_EXCEPTIONS_RECURSIVE)) {
                return;
            }
    
            // for event streams we keep the filter spec around for use when the stream definition is completed
            _filterSpec = ASTFilterSpecHelper.WalkFilterSpec(ctx, _propertyEvalSpec, _astExprNodeMap);
    
            // set property eval to null
            _propertyEvalSpec = null;
    
            _astExprNodeMap.Clear();
        }
    
        public void ExitMatchRecogPatternConcat(EsperEPL2GrammarParser.MatchRecogPatternConcatContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var concatNode = new RowRegexExprNodeConcatenation();
            ASTExprHelper.RegExCollectAddSubNodesAddParentNode(concatNode, ctx, _astRowRegexNodeMap);
        }
    
        public void ExitNumberconstant(EsperEPL2GrammarParser.NumberconstantContext ctx) {
            // if the parent is constant, don't need an expression
            if (ctx.Parent.RuleIndex == EsperEPL2GrammarParser.RULE_constant) {
                return;
            }
            var constantNode = new ExprConstantNodeImpl(ASTConstantHelper.Parse(ctx));
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(constantNode, ctx, _astExprNodeMap);
        }
    
        public void ExitMatchRecogPattern(EsperEPL2GrammarParser.MatchRecogPatternContext ctx) {
            var exprNode = ASTExprHelper.RegExGetRemoveTopNode(ctx, _astRowRegexNodeMap);
            if (exprNode == null) {
                throw new IllegalStateException("Expression node for AST node not found");
            }
            _statementSpec.MatchRecognizeSpec.Pattern = exprNode;
        }
    
        public void ExitWhereClause(EsperEPL2GrammarParser.WhereClauseContext ctx) {
            if (ctx.Parent.RuleIndex != EsperEPL2GrammarParser.RULE_subQueryExpr &&
                    ASTUtil.IsRecursiveParentRule(ctx, WHERE_CLAUSE_WALK_EXCEPTIONS_RECURSIVE)) { // ignore pattern
                return;
            }
            if (_astExprNodeMap.Count != 1) {
                throw new IllegalStateException("Where clause generated zero or more then one expression nodes");
            }
    
            // Just assign the single root ExprNode not consumed yet
            _statementSpec.FilterExprRootNode = _astExprNodeMap.Values.First();
            _astExprNodeMap.Clear();
        }
    
        public void ExitMatchRecogPatternAtom(EsperEPL2GrammarParser.MatchRecogPatternAtomContext ctx) {
            var first = ctx.i.Text;
            var type = RegexNFATypeEnum.SINGLE;
            if (ctx.reluctant != null && ctx.s != null) {
                type = RegexNFATypeEnumExtensions.FromString(ctx.s.Text, ctx.reluctant.Text);
            } else if (ctx.s != null) {
                type = RegexNFATypeEnumExtensions.FromString(ctx.s.Text, null);
            }
    
            var repeat = ASTMatchRecognizeHelper.walkOptionalRepeat(ctx.matchRecogPatternRepeat(), _astExprNodeMap);
            var item = new RowRegexExprNodeAtom(first, type, repeat);
            ASTExprHelper.RegExCollectAddSubNodesAddParentNode(item, ctx, _astRowRegexNodeMap);
        }
    
        public void ExitUpdateExpr(EsperEPL2GrammarParser.UpdateExprContext ctx) {
            var updctx = ctx.updateDetails();
            var eventTypeName = ASTUtil.UnescapeClassIdent(updctx.classIdentifier());
            var streamSpec = new FilterStreamSpecRaw(new FilterSpecRaw(eventTypeName, Collections.GetEmptyList<ExprNode>(), null), ViewSpec.EMPTY_VIEWSPEC_ARRAY, eventTypeName, StreamSpecOptions.DEFAULT);
            _statementSpec.StreamSpecs.Add(streamSpec);
            var optionalStreamName = updctx.i != null ? updctx.i.Text : null;
            var assignments = ASTExprHelper.GetOnTriggerSetAssignments(updctx.onSetAssignmentList(), _astExprNodeMap);
            var whereClause = updctx.WHERE() != null ? ASTExprHelper.ExprCollectSubNodes(updctx.whereClause(), 0, _astExprNodeMap)[0] : null;
            _statementSpec.UpdateDesc = new UpdateDesc(optionalStreamName, assignments, whereClause);
        }
    
        public void ExitFrequencyOperand(EsperEPL2GrammarParser.FrequencyOperandContext ctx) {
            var exprNode = new ExprNumberSetFrequency();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
            ASTExprHelper.AddOptionalNumber(exprNode, ctx.number());
            ASTExprHelper.AddOptionalSimpleProperty(exprNode, ctx.i, _variableService, _statementSpec);
        }
    
        public void ExitCreateDataflow(EsperEPL2GrammarParser.CreateDataflowContext ctx) {
            var graphDesc = ASTGraphHelper.WalkCreateDataFlow(ctx, _astGopNodeMap, _engineImportService);
            _statementSpec.CreateDataFlowDesc = graphDesc;
        }
    
        public void ExitInsertIntoExpr(EsperEPL2GrammarParser.InsertIntoExprContext ctx) {
            var selector = SelectClauseStreamSelectorEnum.ISTREAM_ONLY;
            if (ctx.r != null) {
                selector = SelectClauseStreamSelectorEnum.RSTREAM_ONLY;
            } else if (ctx.ir != null) {
                selector = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            }
    
            // type name
            var eventTypeName = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            var insertIntoDesc = new InsertIntoDesc(selector, eventTypeName);
    
            // optional columns
            if (ctx.columnList() != null) {
                for (var i = 0; i < ctx.columnList().ChildCount; i++) {
                    var node = ctx.columnList().GetChild(i);
                    if (ASTUtil.IsTerminatedOfType(node, EsperEPL2GrammarLexer.IDENT)) {
                        insertIntoDesc.Add(node.GetText());
                    }
                }
            }
    
            _statementSpec.InsertIntoDesc = insertIntoDesc;
        }
    
        public void ExitCreateVariableExpr(EsperEPL2GrammarParser.CreateVariableExprContext ctx) {
    
            var constant = false;
            if (ctx.c != null) {
                var text = ctx.c.Text;
                if (text.Equals("constant") || text.Equals("const")) {
                    constant = true;
                } else {
                    throw new EPException("Expected 'constant' or 'const' keyword after create for create-variable syntax but encountered '" + text + "'");
                }
            }
    
            var variableType = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            var variableName = ctx.n.Text;
    
            var array = ctx.arr != null;
            var arrayOfPrimitive = ASTCreateSchemaHelper.ValidateIsPrimitiveArray(ctx.p);
    
            ExprNode assignment = null;
            if (ctx.EQUALS() != null) {
                assignment = ASTExprHelper.ExprCollectSubNodes(ctx.expression(), 0, _astExprNodeMap)[0];
            }
    
            var desc = new CreateVariableDesc(variableType, variableName, assignment, constant, array, arrayOfPrimitive);
            _statementSpec.CreateVariableDesc = desc;
        }
    
        public void ExitOnStreamExpr(EsperEPL2GrammarParser.OnStreamExprContext ctx) {
            var streamAsName = ctx.i != null ? ctx.i.Text : null;
    
            // get stream to use (pattern or filter)
            StreamSpecRaw streamSpec;
            if (ctx.eventFilterExpression() != null) {
                streamSpec = new FilterStreamSpecRaw(_filterSpec, ViewSpec.EMPTY_VIEWSPEC_ARRAY, streamAsName, StreamSpecOptions.DEFAULT);
            } else if (ctx.patternInclusionExpression() != null) {
                if ((_astPatternNodeMap.Count > 1) || ((_astPatternNodeMap.IsEmpty()))) {
                    throw ASTWalkException.From("Unexpected AST tree contains zero or more then 1 child elements for root");
                }
                // Get expression node sub-tree from the AST nodes placed so far
                var evalNode = _astPatternNodeMap.Values.First();
                var flags = GetPatternFlags(ctx.patternInclusionExpression().annotationEnum());
                streamSpec = new PatternStreamSpecRaw(evalNode, _viewSpecs.ToArray(), streamAsName, StreamSpecOptions.DEFAULT, flags.IsSuppressSameEventMatches, flags.IsDiscardPartialsOnMatch);
                _astPatternNodeMap.Clear();
            } else {
                throw new IllegalStateException("Invalid AST type node, cannot map to stream specification");
            }
            _statementSpec.StreamSpecs.Add(streamSpec);
        }
    
        public void ExitOnSelectInsertFromClause(EsperEPL2GrammarParser.OnSelectInsertFromClauseContext ctx) {
            if (_onTriggerSplitPropertyEvals == null) {
                _onTriggerSplitPropertyEvals = new Dictionary<StatementSpecRaw, OnTriggerSplitStreamFromClause>();
            }
            _onTriggerSplitPropertyEvals.Put(_statementSpec, new OnTriggerSplitStreamFromClause(_propertyEvalSpec, ctx.i == null ? null : ctx.i.Text));
            _propertyEvalSpec = null;
        }
    
        public void ExitPropertyExpressionAtomic(EsperEPL2GrammarParser.PropertyExpressionAtomicContext ctx) {
            // initialize if not set
            if (_propertyEvalSpec == null) {
                _propertyEvalSpec = new PropertyEvalSpec();
            }
    
            // get select clause
            var optionalSelectClause = new SelectClauseSpecRaw();
            if (_propertySelectRaw != null) {
                optionalSelectClause.SelectExprList.AddAll(_propertySelectRaw);
                _propertySelectRaw = null;
            }
    
            // get the splitter expression
            var splitterExpression = ASTExprHelper.ExprCollectSubNodes(ctx.expression(0), 0, _astExprNodeMap)[0];
    
            // get where-clause, if any
            var optionalWhereClause = ctx.where == null ? null : ASTExprHelper.ExprCollectSubNodes(ctx.where, 0, _astExprNodeMap)[0];
    
            var optionalAsName = ctx.n == null ? null : ctx.n.Text;
    
            string splitterEventTypeName = ASTTypeExpressionAnnoHelper.ExpectMayTypeAnno(ctx.typeExpressionAnnotation(), _tokenStream);
            var atom = new PropertyEvalAtom(splitterExpression, splitterEventTypeName, optionalAsName, optionalSelectClause, optionalWhereClause);
            _propertyEvalSpec.Add(atom);
        }
    
        public void ExitFafUpdate(EsperEPL2GrammarParser.FafUpdateContext ctx) {
            HandleFAFNamedWindowStream(ctx.updateDetails().classIdentifier(), ctx.updateDetails().i);
            var assignments = ASTExprHelper.GetOnTriggerSetAssignments(ctx.updateDetails().onSetAssignmentList(), _astExprNodeMap);
            var whereClause = ctx.updateDetails().whereClause() == null ? null : ASTExprHelper.ExprCollectSubNodes(ctx.updateDetails().whereClause(), 0, _astExprNodeMap)[0];
            _statementSpec.FilterExprRootNode = whereClause;
            _statementSpec.FireAndForgetSpec = new FireAndForgetSpecUpdate(assignments);
        }
    
        public void ExitBitWiseExpression(EsperEPL2GrammarParser.BitWiseExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            BitWiseOpEnum bitWiseOpEnum;
            var token = ASTUtil.GetAssertTerminatedTokenType(ctx.GetChild(1));
            switch (token) {
                case EsperEPL2GrammarLexer.BAND:
                    bitWiseOpEnum = BitWiseOpEnum.BAND;
                    break;
                case EsperEPL2GrammarLexer.BOR:
                    bitWiseOpEnum = BitWiseOpEnum.BOR;
                    break;
                case EsperEPL2GrammarLexer.BXOR:
                    bitWiseOpEnum = BitWiseOpEnum.BXOR;
                    break;
                default:
                    throw ASTWalkException.From("Node type " + token + " not a recognized bit wise node type", _tokenStream, ctx);
            }
    
            var bwNode = new ExprBitWiseNode(bitWiseOpEnum);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(bwNode, ctx, _astExprNodeMap);
        }
    
        public void ExitEvalEqualsExpression(EsperEPL2GrammarParser.EvalEqualsExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            ExprNode exprNode;
            var isNot = ctx.ne != null || ctx.isnot != null || ctx.sqlne != null;
            if (ctx.a == null) {
                var isIs = ctx.@is != null || ctx.isnot != null;
                exprNode = new ExprEqualsNodeImpl(isNot, isIs);
            } else {
                var isAll = ctx.a.Type == EsperEPL2GrammarLexer.ALL;
                IList<EsperEPL2GrammarParser.SubSelectGroupExpressionContext> subselect = ctx.subSelectGroupExpression();
                if (subselect != null && !subselect.IsEmpty()) {
                    StatementSpecRaw currentSpec = _astStatementSpecMap.Delete(ctx.subSelectGroupExpression()[0].subQueryExpr());
                    exprNode = new ExprSubselectAllSomeAnyNode(currentSpec, isNot, isAll, null);
                } else {
                    exprNode = new ExprEqualsAllAnyNode(isNot, isAll);
                }
            }
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
        }
    
        public void ExitGopConfig(EsperEPL2GrammarParser.GopConfigContext ctx) {
    
            Object value;
            if (ctx.SELECT() == null) {
                if (ctx.expression() != null) {
                    value = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap)[0];
                } else {
                    if (ctx.jsonarray() != null) {
                        value = new ExprConstantNodeImpl(ASTJsonHelper.WalkArray(_tokenStream, ctx.jsonarray()));
                    } else {
                        value = new ExprConstantNodeImpl(ASTJsonHelper.WalkObject(_tokenStream, ctx.jsonobject()));
                    }
                    ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap);
                }
            } else {
                var newSpec = new StatementSpecRaw(_defaultStreamSelector);
                newSpec.Annotations.AddAll(_statementSpec.Annotations);
    
                var existingSpec = _statementSpec;
                existingSpec.CreateSchemaDesc = null;
                value = existingSpec;
                existingSpec.Annotations = Collections.GetEmptyList<AnnotationDesc>();  // clearing property-level annotations
    
                _statementSpec = newSpec;
            }
            _astGopNodeMap.Put(ctx, value);
        }
    
        public void ExitCreateSelectionListElement(EsperEPL2GrammarParser.CreateSelectionListElementContext ctx) {
            if (ctx.STAR() != null) {
                _statementSpec.SelectClauseSpec.Add(new SelectClauseElementWildcard());
            } else {
                var expr = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap)[0];
                var asName = ctx.i != null ? ctx.i.Text : null;
                _statementSpec.SelectClauseSpec.Add(new SelectClauseExprRawSpec(expr, asName, false));
            }
        }
    
        public void ExitFafDelete(EsperEPL2GrammarParser.FafDeleteContext ctx) {
            HandleFAFNamedWindowStream(ctx.classIdentifier(), ctx.i);
            _statementSpec.FireAndForgetSpec = new FireAndForgetSpecDelete();
        }
    
        public void ExitConstant(EsperEPL2GrammarParser.ConstantContext ctx) {
            var constantNode = new ExprConstantNodeImpl(ASTConstantHelper.Parse(ctx.GetChild(0)));
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(constantNode, ctx, _astExprNodeMap);
        }
    
        public void ExitMergeMatched(EsperEPL2GrammarParser.MergeMatchedContext ctx) {
            HandleMergeMatchedUnmatched(ctx.expression(), true);
        }
    
        public void ExitEvalAndExpression(EsperEPL2GrammarParser.EvalAndExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var and = new ExprAndNodeImpl();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(and, ctx, _astExprNodeMap);
        }
    
        public void ExitForExpr(EsperEPL2GrammarParser.ForExprContext ctx) {
            if (_statementSpec.ForClauseSpec == null) {
                _statementSpec.ForClauseSpec = new ForClauseSpec();
            }
            var ident = ctx.i.Text;
            var expressions = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap);
            _statementSpec.ForClauseSpec.Clauses.Add(new ForClauseItemSpec(ident, expressions));
        }
    
        public void ExitExpressionQualifyable(EsperEPL2GrammarParser.ExpressionQualifyableContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            if (ctx.s != null) {
                ExprNode node = ASTExprHelper.TimePeriodGetExprJustSeconds(
                    ctx.expression(), 
                    _astExprNodeMap, 
                    _configurationInformation, 
                    _engineImportService.TimeAbacus,
                    _container.Resolve<ILockManager>());
                _astExprNodeMap.Put(ctx, node);
            } else if (ctx.a != null || ctx.d != null) {
                var isDescending = ctx.d != null;
                var node = ASTExprHelper.ExprCollectSubNodes(ctx.expression(), 0, _astExprNodeMap)[0];
                var exprNode = new ExprOrderedExpr(isDescending);
                exprNode.AddChildNode(node);
                _astExprNodeMap.Put(ctx, exprNode);
            }
        }
    
        public void ExitPropertySelectionListElement(EsperEPL2GrammarParser.PropertySelectionListElementContext ctx) {
            SelectClauseElementRaw raw;
            if (ctx.s != null) {
                raw = new SelectClauseElementWildcard();
            } else if (ctx.propertyStreamSelector() != null) {
                raw = new SelectClauseStreamRawSpec(ctx.propertyStreamSelector().s.Text,
                        ctx.propertyStreamSelector().i != null ? ctx.propertyStreamSelector().i.Text : null);
            } else {
                var exprNode = ASTExprHelper.ExprCollectSubNodes(ctx.expression(), 0, _astExprNodeMap)[0];
                var optionalName = ctx.keywordAllowedIdent() != null ? ctx.keywordAllowedIdent().GetText() : null;
                raw = new SelectClauseExprRawSpec(exprNode, optionalName, false);
            }
    
            // Add as selection element
            if (_propertySelectRaw == null) {
                _propertySelectRaw = new List<SelectClauseElementRaw>();
            }
            _propertySelectRaw.Add(raw);
        }
    
        public void ExitExpressionDecl(EsperEPL2GrammarParser.ExpressionDeclContext ctx) {
            if (ctx.Parent.RuleIndex == EsperEPL2GrammarParser.RULE_createExpressionExpr) {
                return;
            }
    
            var pair = ASTExpressionDeclHelper.WalkExpressionDecl(ctx, _scriptBodies, _astExprNodeMap, _tokenStream);
            if (pair.First != null) {
                _expressionDeclarations.Add(pair.First);
            } else {
                _scriptExpressions.Add(pair.Second);
            }
        }
    
        public void ExitSubstitutionCanChain(EsperEPL2GrammarParser.SubstitutionCanChainContext ctx) {
            if (ctx.chainedFunction() == null) {
                return;
            }
            var substitutionNode = (ExprSubstitutionNode) _astExprNodeMap.Delete(ctx.substitution());
            IList<ExprChainedSpec> chainSpec = ASTLibFunctionHelper.GetLibFuncChain(ctx.chainedFunction().libFunctionNoClass(), _astExprNodeMap);
            var exprNode = new ExprDotNodeImpl(chainSpec, _engineImportService.IsDuckType, _engineImportService.IsUdfCache);
            exprNode.AddChildNode(substitutionNode);
            _astExprNodeMap.Put(ctx, exprNode);
        }
    
        public void ExitSubstitution(EsperEPL2GrammarParser.SubstitutionContext ctx) {
            var currentSize = _substitutionParamNodes.Count;
            ExprSubstitutionNode substitutionNode;
            if (ctx.slashIdentifier() != null) {
                var name = ASTUtil.UnescapeSlashIdentifier(ctx.slashIdentifier());
                substitutionNode = new ExprSubstitutionNode(name);
            } else {
                substitutionNode = new ExprSubstitutionNode(currentSize + 1);
            }
            ASTSubstitutionHelper.ValidateNewSubstitution(_substitutionParamNodes, substitutionNode);
            _substitutionParamNodes.Add(substitutionNode);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(substitutionNode, ctx, _astExprNodeMap);
        }
    
        public void ExitWeekDayOperator(EsperEPL2GrammarParser.WeekDayOperatorContext ctx) {
            var exprNode = new ExprNumberSetCronParam(CronOperatorEnum.WEEKDAY);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
            ASTExprHelper.AddOptionalNumber(exprNode, ctx.number());
            ASTExprHelper.AddOptionalSimpleProperty(exprNode, ctx.i, _variableService, _statementSpec);
        }
    
        public void ExitLastWeekdayOperand(EsperEPL2GrammarParser.LastWeekdayOperandContext ctx) {
            var exprNode = new ExprNumberSetCronParam(CronOperatorEnum.LASTWEEKDAY);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
        }
    
        public void ExitGroupByListExpr(EsperEPL2GrammarParser.GroupByListExprContext ctx) {
            ASTGroupByHelper.WalkGroupBy(ctx, _astExprNodeMap, _statementSpec.GroupByExpressions);
            _astExprNodeMap.Clear();
        }
    
        public void ExitStreamSelector(EsperEPL2GrammarParser.StreamSelectorContext ctx) {
            var streamName = ctx.s.Text;
            var optionalName = ctx.i != null ? ctx.i.Text : null;
            _statementSpec.SelectClauseSpec.Add(new SelectClauseStreamRawSpec(streamName, optionalName));
        }
    
        public void ExitStreamExpression(EsperEPL2GrammarParser.StreamExpressionContext ctx) {
            // Determine the optional stream name
            var streamName = ctx.i != null ? ctx.i.Text : null;
    
            var isUnidirectional = ctx.UNIDIRECTIONAL() != null;
            var isRetainUnion = ctx.RETAINUNION() != null;
            var isRetainIntersection = ctx.RETAININTERSECTION() != null;
    
            // Convert to a stream specification instance
            StreamSpecRaw streamSpec;
            var options = new StreamSpecOptions(isUnidirectional, isRetainUnion, isRetainIntersection);
    
            // If the first subnode is a filter node, we have a filter stream specification
            if (ASTUtil.GetRuleIndexIfProvided(ctx.GetChild(0)) == EsperEPL2GrammarParser.RULE_eventFilterExpression) {
                streamSpec = new FilterStreamSpecRaw(_filterSpec, _viewSpecs.ToArray(), streamName, options);
            } else if (ASTUtil.GetRuleIndexIfProvided(ctx.GetChild(0)) == EsperEPL2GrammarParser.RULE_patternInclusionExpression) {
                if ((_astPatternNodeMap.Count > 1) || ((_astPatternNodeMap.IsEmpty()))) {
                    throw ASTWalkException.From("Unexpected AST tree contains zero or more then 1 child elements for root");
                }
                var pctx = (EsperEPL2GrammarParser.PatternInclusionExpressionContext) ctx.GetChild(0);
    
                // Get expression node sub-tree from the AST nodes placed so far
                EvalFactoryNode evalNode = _astPatternNodeMap.Values.First();
                var flags = GetPatternFlags(pctx.annotationEnum());
                streamSpec = new PatternStreamSpecRaw(evalNode, _viewSpecs.ToArray(), streamName, options, flags.IsSuppressSameEventMatches, flags.IsDiscardPartialsOnMatch);
                _astPatternNodeMap.Clear();
            } else if (ctx.databaseJoinExpression() != null) {
                var dbctx = ctx.databaseJoinExpression();
                var dbName = dbctx.i.Text;
                var sqlWithParams = StringValue.ParseString(dbctx.s.Text);
    
                // determine if there is variables used
                IList<PlaceholderParser.Fragment> sqlFragments;
                try {
                    sqlFragments = PlaceholderParser.ParsePlaceholder(sqlWithParams);
                    foreach (var fragment in sqlFragments) {
                        if (!(fragment is PlaceholderParser.ParameterFragment)) {
                            continue;
                        }
    
                        // Parse expression, store for substitution parameters
                        var expression = fragment.Value;
                        if (expression.ToUpperInvariant().Equals(DatabasePollingViewableFactory.SAMPLE_WHERECLAUSE_PLACEHOLDER)) {
                            continue;
                        }
    
                        if (expression.Trim().Length == 0) {
                            throw ASTWalkException.From("Missing expression within ${...} in SQL statement");
                        }
                        var toCompile = "select * from System.Object where " + expression;
                        var raw = EPAdministratorHelper.CompileEPL(
                            _container,
                            toCompile, expression, false, null,
                            SelectClauseStreamSelectorEnum.ISTREAM_ONLY,
                            _engineImportService, _variableService,
                            _schedulingService, _engineURI,
                            _configurationInformation,
                            _patternNodeFactory,
                            _contextManagementService,
                            _exprDeclaredService,
                            _tableService);
    
                        if ((raw.SubstitutionParameters != null) && (raw.SubstitutionParameters.Count > 0)) {
                            throw ASTWalkException.From("EPL substitution parameters are not allowed in SQL ${...} expressions, consider using a variable instead");
                        }
    
                        if (raw.HasVariables) {
                            _statementSpec.HasVariables = true;
                        }
    
                        // add expression
                        if (_statementSpec.SqlParameters == null) {
                            _statementSpec.SqlParameters = new Dictionary<int, IList<ExprNode>>();
                        }
                        var listExp = _statementSpec.SqlParameters.Get(_statementSpec.StreamSpecs.Count);
                        if (listExp == null) {
                            listExp = new List<ExprNode>();
                            _statementSpec.SqlParameters.Put(_statementSpec.StreamSpecs.Count, listExp);
                        }
                        listExp.Add(raw.FilterRootNode);
                    }
                } catch (PlaceholderParseException ex) {
                    Log.Warn("Failed to parse SQL text '" + sqlWithParams + "' :" + ex.Message);
                    // Let the view construction handle the validation
                }
    
                string sampleSQL = null;
                if (dbctx.s2 != null) {
                    sampleSQL = dbctx.s2.Text;
                    sampleSQL = StringValue.ParseString(sampleSQL.Trim());
                }

                streamSpec = new DBStatementStreamSpec(streamName, _viewSpecs.ToArray(), dbName, sqlWithParams, sampleSQL);
            } else if (ctx.methodJoinExpression() != null) {
                var mthctx = ctx.methodJoinExpression();
                var prefixIdent = mthctx.i.Text;
                var fullName = ASTUtil.UnescapeClassIdent(mthctx.classIdentifier());
    
                var indexDot = fullName.LastIndexOf('.');
                string classNamePart;
                string methodNamePart;
                if (indexDot == -1) {
                    classNamePart = null;
                    methodNamePart = fullName;
                } else {
                    classNamePart = fullName.Substring(0, indexDot);
                    methodNamePart = fullName.Substring(indexDot + 1);
                }
                var exprNodes = ASTExprHelper.ExprCollectSubNodes(mthctx, 0, _astExprNodeMap);
    
                if (_variableService.GetVariableMetaData(classNamePart) != null) {
                    _statementSpec.HasVariables = true;
                }
    
                string eventTypeName = ASTTypeExpressionAnnoHelper.ExpectMayTypeAnno(ctx.methodJoinExpression().typeExpressionAnnotation(), _tokenStream);

                streamSpec = new MethodStreamSpec(streamName, _viewSpecs.ToArray(), prefixIdent, classNamePart, methodNamePart, exprNodes, eventTypeName);
            } else {
                throw ASTWalkException.From("Unexpected AST child node to stream expression", _tokenStream, ctx);
            }
            _viewSpecs.Clear();
            _statementSpec.StreamSpecs.Add(streamSpec);
        }
    
        public void ExitViewExpressionWNamespace(EsperEPL2GrammarParser.ViewExpressionWNamespaceContext ctx) {
            var objectNamespace = ctx.GetChild(0).GetText();
            var objectName = ctx.viewWParameters().GetChild(0).GetText();
            var viewParameters = ASTExprHelper.ExprCollectSubNodes(ctx.viewWParameters(), 1, _astExprNodeMap);
            _viewSpecs.Add(new ViewSpec(objectNamespace, objectName, viewParameters));
        }
    
        public void ExitViewExpressionOptNamespace(EsperEPL2GrammarParser.ViewExpressionOptNamespaceContext ctx) {
            string objectNamespace = null;
            var objectName = ctx.viewWParameters().GetChild(0).GetText();
            if (ctx.ns != null) {
                objectNamespace = ctx.ns.Text;
            }
            var viewParameters = ASTExprHelper.ExprCollectSubNodes(ctx.viewWParameters(), 1, _astExprNodeMap);
            _viewSpecs.Add(new ViewSpec(objectNamespace, objectName, viewParameters));
        }
    
        public void ExitPatternFilterExpression(EsperEPL2GrammarParser.PatternFilterExpressionContext ctx) {
            string optionalPatternTagName = null;
            if (ctx.i != null) {
                optionalPatternTagName = ctx.i.Text;
            }
    
            var eventName = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
    
            var anno = ctx.patternFilterAnnotation();
            int? consumption = null;
            if (anno != null) {
                var name = ctx.patternFilterAnnotation().i.Text;
                if (!name.ToUpper().Equals("CONSUME")) {
                    throw new EPException("Unexpected pattern filter @ annotation, expecting 'consume' but received '" + name + "'");
                }
                if (anno.number() != null) {
                    consumption = ASTConstantHelper.Parse(anno.number()).AsInt();
                } else {
                    consumption = 1;
                }
            }
    
            var exprNodes = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap);
    
            var rawFilterSpec = new FilterSpecRaw(eventName, exprNodes, _propertyEvalSpec);
            _propertyEvalSpec = null;
            var filterNode = _patternNodeFactory.MakeFilterNode(rawFilterSpec, optionalPatternTagName, consumption);
            ASTExprHelper.PatternCollectAddSubnodesAddParentNode(filterNode, ctx, _astPatternNodeMap);
        }
    
        public void ExitOnSelectExpr(EsperEPL2GrammarParser.OnSelectExprContext ctx) {
            _statementSpec.SelectClauseSpec.IsDistinct = ctx.DISTINCT() != null;
        }
    
        public void ExitStartPatternExpressionRule(EsperEPL2GrammarParser.StartPatternExpressionRuleContext ctx) {
            if ((_astPatternNodeMap.Count > 1) || ((_astPatternNodeMap.IsEmpty()))) {
                throw ASTWalkException.From("Unexpected AST tree contains zero or more then 1 child elements for root");
            }
    
            // Get expression node sub-tree from the AST nodes placed so far
            var evalNode = _astPatternNodeMap.Values.First();
    
            var streamSpec = new PatternStreamSpecRaw(evalNode, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT, false, false);
            _statementSpec.StreamSpecs.Add(streamSpec);
            _statementSpec.SubstitutionParameters = _substitutionParamNodes;
    
            _astPatternNodeMap.Clear();
        }
    
        public void ExitOutputLimit(EsperEPL2GrammarParser.OutputLimitContext ctx) {
            var spec = ASTOutputLimitHelper.BuildOutputLimitSpec(_tokenStream, ctx, _astExprNodeMap, _variableService, _engineURI, _timeProvider, _exprEvaluatorContext);
            _statementSpec.OutputLimitSpec = spec;
            if (spec.VariableName != null) {
                _statementSpec.HasVariables = true;
                ASTExprHelper.AddVariableReference(_statementSpec, spec.VariableName);
            }
        }
    
        public void ExitNumericParameterList(EsperEPL2GrammarParser.NumericParameterListContext ctx) {
            var exprNode = new ExprNumberSetList();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
        }
    
        public void ExitCreateSchemaExpr(EsperEPL2GrammarParser.CreateSchemaExprContext ctx) {
            var createSchema = ASTCreateSchemaHelper.WalkCreateSchema(ctx);
            if (ctx.Parent.RuleIndex == EsperEPL2GrammarParser.RULE_eplExpression) {
                _statementSpec.StreamSpecs.Add(new FilterStreamSpecRaw(new FilterSpecRaw(typeof(Object).FullName, Collections.GetEmptyList<ExprNode>(), null), ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT));
            }
            _statementSpec.CreateSchemaDesc = createSchema;
        }
    
        public void ExitLastOperator(EsperEPL2GrammarParser.LastOperatorContext ctx) {
            var exprNode = new ExprNumberSetCronParam(CronOperatorEnum.LASTDAY);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
            ASTExprHelper.AddOptionalNumber(exprNode, ctx.number());
            ASTExprHelper.AddOptionalSimpleProperty(exprNode, ctx.i, _variableService, _statementSpec);
        }
    
        public void ExitCreateIndexExpr(EsperEPL2GrammarParser.CreateIndexExprContext ctx) {
            var desc = ASTIndexHelper.Walk(ctx, _astExprNodeMap);
            _statementSpec.CreateIndexDesc = desc;
        }
    
        public void ExitAnnotationEnum(EsperEPL2GrammarParser.AnnotationEnumContext ctx) {
            if (ctx.Parent.RuleIndex != EsperEPL2GrammarParser.RULE_startEPLExpressionRule &&
                    ctx.Parent.RuleIndex != EsperEPL2GrammarParser.RULE_startPatternExpressionRule) {
                return;
            }
    
            _statementSpec.Annotations.Add(ASTAnnotationHelper.Walk(ctx, _engineImportService));
            _astExprNodeMap.Clear();
        }
    
        public void ExitCreateContextExpr(EsperEPL2GrammarParser.CreateContextExprContext ctx) {
            var contextDesc = ASTContextHelper.WalkCreateContext(ctx, _astExprNodeMap, _astPatternNodeMap, _propertyEvalSpec, _filterSpec);
            _filterSpec = null;
            _propertyEvalSpec = null;
            _statementSpec.CreateContextDesc = contextDesc;
        }
    
        public void ExitLastOperand(EsperEPL2GrammarParser.LastOperandContext ctx) {
            var exprNode = new ExprNumberSetCronParam(CronOperatorEnum.LASTDAY);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
        }
    
        public void ExitCreateWindowExpr(EsperEPL2GrammarParser.CreateWindowExprContext ctx) {
            var windowName = ctx.i.Text;
    
            var eventName = "System.Object";
            if (ctx.createWindowExprModelAfter() != null) {
                eventName = ASTUtil.UnescapeClassIdent(ctx.createWindowExprModelAfter().classIdentifier());
            }
    
            var isRetainUnion = ctx.ru != null;
            var isRetainIntersection = ctx.ri != null;
            var streamSpecOptions = new StreamSpecOptions(false, isRetainUnion, isRetainIntersection);
    
            // handle table-create clause, i.e. (col1 type, col2 type)
            IList<ColumnDesc> colums = ASTCreateSchemaHelper.GetColTypeList(ctx.createColumnList());
    
            var isInsert = ctx.INSERT() != null;
            ExprNode insertWhereExpr = null;
            if (isInsert && ctx.expression() != null) {
                insertWhereExpr = ASTExprHelper.ExprCollectSubNodes(ctx.expression(), 0, _astExprNodeMap)[0];
            }
    
            var desc = new CreateWindowDesc(windowName, _viewSpecs, streamSpecOptions, isInsert, insertWhereExpr, colums, eventName);
            _statementSpec.CreateWindowDesc = desc;
    
            // this is good for indicating what is being selected from
            var rawFilterSpec = new FilterSpecRaw(eventName, new List<ExprNode>(), null);
            var streamSpec = new FilterStreamSpecRaw(rawFilterSpec, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, streamSpecOptions);
            _statementSpec.StreamSpecs.Add(streamSpec);
        }
    
        public void ExitCreateExpressionExpr(EsperEPL2GrammarParser.CreateExpressionExprContext ctx) {
            var pair = ASTExpressionDeclHelper.WalkExpressionDecl(ctx.expressionDecl(), _scriptBodies, _astExprNodeMap, _tokenStream);
            _statementSpec.CreateExpressionDesc = new CreateExpressionDesc(pair);
        }
    
        public void ExitRangeOperand(EsperEPL2GrammarParser.RangeOperandContext ctx) {
            var exprNode = new ExprNumberSetRange();
            _astExprNodeMap.Put(ctx, exprNode);
            if (ctx.s1 != null) {
                ASTExprHelper.ExprCollectAddSubNodes(exprNode, ctx.s1, _astExprNodeMap);
            }
            ASTExprHelper.AddOptionalNumber(exprNode, ctx.n1);
            ASTExprHelper.AddOptionalSimpleProperty(exprNode, ctx.i1, _variableService, _statementSpec);
            if (ctx.s2 != null) {
                ASTExprHelper.ExprCollectAddSubNodes(exprNode, ctx.s2, _astExprNodeMap);
            }
            ASTExprHelper.AddOptionalNumber(exprNode, ctx.n2);
            ASTExprHelper.AddOptionalSimpleProperty(exprNode, ctx.i2, _variableService, _statementSpec);
        }
    
        public void ExitRowSubSelectExpression(EsperEPL2GrammarParser.RowSubSelectExpressionContext ctx) {
            StatementSpecRaw statementSpec = _astStatementSpecMap.Delete(ctx.subQueryExpr());
            var subselectNode = new ExprSubselectRowNode(statementSpec);
            if (ctx.chainedFunction() != null) {
                HandleChainedFunction(ctx, ctx.chainedFunction(), subselectNode);
            } else {
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(subselectNode, ctx, _astExprNodeMap);
            }
        }
    
        public void ExitUnaryExpression(EsperEPL2GrammarParser.UnaryExpressionContext ctx) {
            if (ctx.inner != null && ctx.chainedFunction() != null) {
                HandleChainedFunction(ctx, ctx.chainedFunction(), null);
            }
            if (ctx.NEWKW() != null && ctx.newAssign() != null) {
                var columnNames = new List<string>();
                var expressions = new List<ExprNode>();
                IList<EsperEPL2GrammarParser.NewAssignContext> assigns = ctx.newAssign();
                foreach (var assign in assigns) {
                    var property = ASTUtil.GetPropertyName(assign.eventProperty(), 0);
                    columnNames.Add(property);
                    ExprNode expr;
                    if (assign.expression() != null) {
                        expr = ASTExprHelper.ExprCollectSubNodes(assign.expression(), 0, _astExprNodeMap)[0];
                    } else {
                        expr = new ExprIdentNodeImpl(property);
                    }
                    expressions.Add(expr);
                }
                string[] columns = columnNames.ToArray();
                var newNode = new ExprNewStructNode(columns);
                newNode.AddChildNodes(expressions);
                _astExprNodeMap.Put(ctx, newNode);
            }
            if (ctx.NEWKW() != null && ctx.classIdentifier() != null) {
                var classIdent = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
                ExprNode exprNode;
                var newNode = new ExprNewInstanceNode(classIdent);
                if (ctx.chainedFunction() != null) {
                    IList<ExprChainedSpec> chainSpec = ASTLibFunctionHelper.GetLibFuncChain(ctx.chainedFunction().libFunctionNoClass(), _astExprNodeMap);
                    var dotNode = new ExprDotNodeImpl(chainSpec, _engineImportService.IsDuckType, _engineImportService.IsUdfCache);
                    dotNode.AddChildNode(newNode);
                    exprNode = dotNode;
                } else {
                    exprNode = newNode;
                }
                ASTExprHelper.ExprCollectAddSubNodes(newNode, ctx, _astExprNodeMap);
                _astExprNodeMap.Put(ctx, exprNode);
            }
            if (ctx.b != null) {
                // handle "variable[xxx]"
                var tableName = ctx.b.Text;
                ExprNode exprNode;
                ExprTableAccessNode tableNode;
                if (ctx.chainedFunction() == null) {
                    tableNode = new ExprTableAccessNodeTopLevel(tableName);
                    exprNode = tableNode;
                } else {
                    IList<ExprChainedSpec> chainSpec = ASTLibFunctionHelper.GetLibFuncChain(ctx.chainedFunction().libFunctionNoClass(), _astExprNodeMap);
                    var pair = ASTTableExprHelper.GetTableExprChainable(_engineImportService, _plugInAggregations, _engineURI, tableName, chainSpec);
                    tableNode = pair.First;
                    if (pair.Second.IsEmpty()) {
                        exprNode = tableNode;
                    } else {
                        exprNode = new ExprDotNodeImpl(pair.Second, _engineImportService.IsDuckType, _engineImportService.IsUdfCache);
                        exprNode.AddChildNode(tableNode);
                    }
                }
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(tableNode, ctx, _astExprNodeMap);
                _astExprNodeMap.Put(ctx, exprNode);
                ASTTableExprHelper.AddTableExpressionReference(_statementSpec, tableNode);
            }
        }
    
        public void EnterOnSelectInsertExpr(EsperEPL2GrammarParser.OnSelectInsertExprContext ctx) {
            PushStatementContext();
        }
    
        public void ExitSelectClause(EsperEPL2GrammarParser.SelectClauseContext ctx) {
            SelectClauseStreamSelectorEnum selector;
            if (ctx.s != null) {
                if (ctx.s.Type == EsperEPL2GrammarLexer.RSTREAM) {
                    selector = SelectClauseStreamSelectorEnum.RSTREAM_ONLY;
                } else if (ctx.s.Type == EsperEPL2GrammarLexer.ISTREAM) {
                    selector = SelectClauseStreamSelectorEnum.ISTREAM_ONLY;
                } else if (ctx.s.Type == EsperEPL2GrammarLexer.IRSTREAM) {
                    selector = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
                } else {
                    throw ASTWalkException.From("Encountered unrecognized token type " + ctx.s.Type, _tokenStream, ctx);
                }
                _statementSpec.SelectStreamDirEnum = selector;
            }
            _statementSpec.SelectClauseSpec.IsDistinct = ctx.d != null;
        }
    
        public void ExitConcatenationExpr(EsperEPL2GrammarParser.ConcatenationExprContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var concatNode = new ExprConcatNode();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(concatNode, ctx, _astExprNodeMap);
        }
    
        public void ExitSubSelectFilterExpr(EsperEPL2GrammarParser.SubSelectFilterExprContext ctx) {
            var streamName = ctx.i != null ? ctx.i.Text : null;
            var isRetainUnion = ctx.ru != null;
            var isRetainIntersection = ctx.ri != null;
            var options = new StreamSpecOptions(false, isRetainUnion, isRetainIntersection);
            var streamSpec = new FilterStreamSpecRaw(_filterSpec, _viewSpecs.ToArray(), streamName, options);
            _viewSpecs.Clear();
            _statementSpec.StreamSpecs.Add(streamSpec);
        }
    
        public void ExitNegatedExpression(EsperEPL2GrammarParser.NegatedExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var notNode = new ExprNotNode();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(notNode, ctx, _astExprNodeMap);
        }
    
        public void ExitAdditiveExpression(EsperEPL2GrammarParser.AdditiveExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var expr = ASTExprHelper.MathGetExpr(ctx, _astExprNodeMap, _configurationInformation);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(expr, ctx, _astExprNodeMap);
        }
    
        public void ExitMultiplyExpression(EsperEPL2GrammarParser.MultiplyExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var expr = ASTExprHelper.MathGetExpr(ctx, _astExprNodeMap, _configurationInformation);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(expr, ctx, _astExprNodeMap);
        }
    
        public void ExitEventProperty(EsperEPL2GrammarParser.EventPropertyContext ctx) {
            if (EVENT_PROPERTY_WALK_EXCEPTIONS_PARENT.Contains(ctx.Parent.RuleIndex)) {
                return;
            }
    
            if (ctx.ChildCount == 0) {
                throw new IllegalStateException("EmptyFalse event property expression encountered");
            }
    
            ExprNode exprNode;
            string propertyName;
    
            // The stream name may precede the event property name, but cannot be told apart from the property name:
            //      s0.p1 could be a nested property, or could be stream 's0' and property 'p1'
    
            // A single entry means this must be the property name.
            // And a non-simple property means that it cannot be a stream name.
            if (ctx.eventPropertyAtomic().Length == 1 || PropertyParser.IsNestedPropertyWithNonSimpleLead(ctx)) {
                propertyName = ctx.GetText();
                exprNode = new ExprIdentNodeImpl(propertyName);
    
                var first = ctx.eventPropertyAtomic()[0];
    
                // test table access expression
                if (first.lb != null) {
                    string nameText = first.eventPropertyIdent().GetText();
                    if (_tableService.GetTableMetadata(nameText) != null) {
                        ExprTableAccessNode tableNode;
                        if (ctx.eventPropertyAtomic().Length == 1)
                        {
                            tableNode = new ExprTableAccessNodeTopLevel(nameText);
                        }
                        else if (ctx.eventPropertyAtomic().Length == 2)
                        {
                            var column = ctx.eventPropertyAtomic()[1].GetText();
                            tableNode = new ExprTableAccessNodeSubprop(nameText, column);
                        } else {
                            throw ASTWalkException.From("Invalid table expression '" + _tokenStream.GetText(ctx));
                        }
                        exprNode = tableNode;
                        ASTTableExprHelper.AddTableExpressionReference(_statementSpec, tableNode);
                        ASTExprHelper.AddOptionalNumber(tableNode, first.ni);
                    }
                }
    
                // test script
                if (first.lp != null) {
                    var ident = ASTUtil.EscapeDot(first.eventPropertyIdent().GetText());
                    var key = StringValue.ParseString(first.s.Text);
                    var @params = Collections.SingletonList<ExprNode>(new ExprConstantNodeImpl(key));
                    var scriptNode = ExprDeclaredHelper.GetExistsScript(GetDefaultDialect(), ident, @params, _scriptExpressions, _exprDeclaredService);
                    if (scriptNode != null) {
                        exprNode = scriptNode;
                    }
                }

                var found = ExprDeclaredHelper.GetExistsDeclaredExpr(
                    _container,
                    propertyName, 
                    Collections.GetEmptyList<ExprNode>(),
                    _expressionDeclarations.Expressions, 
                    _exprDeclaredService, 
                    _contextDescriptor);
                if (found != null) {
                    exprNode = found;
                }
            } else {
                // --> this is more then one child node, and the first child node is a simple property
                // we may have a stream name in the first simple property, or a nested property
                // i.e. 's0.p0' could mean that the event has a nested property to 's0' of name 'p0', or 's0' is the stream name
                var leadingIdentifier = ctx.GetChild(0).GetChild(0).GetText();
                var streamOrNestedPropertyName = ASTUtil.EscapeDot(leadingIdentifier);
                propertyName = ASTUtil.GetPropertyName(ctx, 2);
    
                var tableNode = ASTTableExprHelper.CheckTableNameGetExprForSubproperty(_tableService, streamOrNestedPropertyName, propertyName);
                var variableMetaData = _variableService.GetVariableMetaData(leadingIdentifier);
                if (tableNode != null) {
                    if (tableNode.Second != null) {
                        exprNode = tableNode.Second;
                    } else {
                        exprNode = tableNode.First;
                    }
                    ASTTableExprHelper.AddTableExpressionReference(_statementSpec, tableNode.First);
                } else if (variableMetaData != null) {
                    exprNode = new ExprVariableNodeImpl(variableMetaData, propertyName);
                    _statementSpec.HasVariables = true;
                    var message = VariableServiceUtil.CheckVariableContextName(_statementSpec.OptionalContextName, variableMetaData);
                    if (message != null) {
                        throw ASTWalkException.From(message);
                    }
                    ASTExprHelper.AddVariableReference(_statementSpec, variableMetaData.VariableName);
                } else if (_contextDescriptor != null && _contextDescriptor.ContextPropertyRegistry.IsContextPropertyPrefix(streamOrNestedPropertyName)) {
                    exprNode = new ExprContextPropertyNode(propertyName);
                } else {
                    exprNode = new ExprIdentNodeImpl(propertyName, streamOrNestedPropertyName);
                }
            }
    
            // handle variable
            var variableMetaDataX = _variableService.GetVariableMetaData(propertyName);
            if (variableMetaDataX != null) {
                exprNode = new ExprVariableNodeImpl(variableMetaDataX, null);
                _statementSpec.HasVariables = true;
                var message = VariableServiceUtil.CheckVariableContextName(_statementSpec.OptionalContextName, variableMetaDataX);
                if (message != null) {
                    throw ASTWalkException.From(message);
                }
                ASTExprHelper.AddVariableReference(_statementSpec, variableMetaDataX.VariableName);
            }
    
            // handle table
            var table = ASTTableExprHelper.CheckTableNameGetExprForProperty(_tableService, propertyName);
            if (table != null) {
                exprNode = table;
                ASTTableExprHelper.AddTableExpressionReference(_statementSpec, table);
            }
    
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, ctx, _astExprNodeMap);
        }
    
        public void ExitOuterJoin(EsperEPL2GrammarParser.OuterJoinContext ctx) {
            OuterJoinType joinType;
            if (ctx.i != null) {
                joinType = OuterJoinType.INNER;
            } else if (ctx.tr != null) {
                joinType = OuterJoinType.RIGHT;
            } else if (ctx.tl != null) {
                joinType = OuterJoinType.LEFT;
            } else if (ctx.tf != null) {
                joinType = OuterJoinType.FULL;
            } else {
                joinType = OuterJoinType.INNER;
            }
    
            // always starts with ON-token, so as to not produce an empty node
            ExprIdentNode left = null;
            ExprIdentNode right = null;
            ExprIdentNode[] addLeftArr = null;
            ExprIdentNode[] addRightArr = null;
    
            // get subnodes representing the on-expression, if provided
            if (ctx.outerJoinIdent() != null) {
                IList<EsperEPL2GrammarParser.OuterJoinIdentPairContext> pairs = ctx.outerJoinIdent().outerJoinIdentPair();
                IList<EsperEPL2GrammarParser.EventPropertyContext> props = pairs[0].eventProperty();
                left = ValidateOuterJoinGetIdentNode(ASTExprHelper.ExprCollectSubNodes(props[0], 0, _astExprNodeMap)[0]);
                right = ValidateOuterJoinGetIdentNode(ASTExprHelper.ExprCollectSubNodes(props[1], 0, _astExprNodeMap)[0]);
    
                if (pairs.Count > 1) {
                    var addLeft = new List<ExprIdentNode>(pairs.Count - 1);
                    var addRight = new List<ExprIdentNode>(pairs.Count - 1);
                    for (var i = 1; i < pairs.Count; i++) {
                        props = pairs[i].eventProperty();
                        var moreLeft = ValidateOuterJoinGetIdentNode(ASTExprHelper.ExprCollectSubNodes(props[0], 0, _astExprNodeMap)[0]);
                        var moreRight = ValidateOuterJoinGetIdentNode(ASTExprHelper.ExprCollectSubNodes(props[1], 0, _astExprNodeMap)[0]);
                        addLeft.Add(moreLeft);
                        addRight.Add(moreRight);
                    }
                    addLeftArr = addLeft.ToArray();
                    addRightArr = addRight.ToArray();
                }
            }
    
            var outerJoinDesc = new OuterJoinDesc(joinType, left, right, addLeftArr, addRightArr);
            _statementSpec.OuterJoinDescList.Add(outerJoinDesc);
        }
    
        public void ExitOnExpr(EsperEPL2GrammarParser.OnExprContext ctx) {
            if (ctx.onMergeExpr() != null) {
                var windowName = ctx.onMergeExpr().n.Text;
                var asName = ctx.onMergeExpr().i != null ? ctx.onMergeExpr().i.Text : null;
                var desc = new OnTriggerMergeDesc(windowName, asName, _mergeMatcheds ?? Collections.GetEmptyList<OnTriggerMergeMatched>());
                _statementSpec.OnTriggerDesc = desc;
            } else if (ctx.onSetExpr() == null) {
                var windowName = GetOnExprWindowName(ctx);
                var deleteAndSelect = ctx.onSelectExpr() != null && ctx.onSelectExpr().d != null;
                if (windowName == null) {
                    // on the statement spec, the deepest spec is the outermost
                    var splitStreams = new List<OnTriggerSplitStream>();
                    var statementSpecList = _statementSpecStack.Reverse().ToArray();

                    for (var ii = 1; ii < statementSpecList.Length; ii++) {
                        StatementSpecRaw raw = statementSpecList[ii];
                        OnTriggerSplitStreamFromClause fromClause = _onTriggerSplitPropertyEvals == null ? null : _onTriggerSplitPropertyEvals.Get(raw);
                        splitStreams.Add(new OnTriggerSplitStream(raw.InsertIntoDesc, raw.SelectClauseSpec, fromClause, raw.FilterExprRootNode));
                    }
                    OnTriggerSplitStreamFromClause fromClauseX = _onTriggerSplitPropertyEvals == null ? null : _onTriggerSplitPropertyEvals.Get(_statementSpec);
                    splitStreams.Add(new OnTriggerSplitStream(_statementSpec.InsertIntoDesc, _statementSpec.SelectClauseSpec, fromClauseX, _statementSpec.FilterExprRootNode));
                    if (!statementSpecList.IsEmpty()) {
                        _statementSpec = statementSpecList[0];
                    }
                    var isFirst = ctx.outputClauseInsert() == null || ctx.outputClauseInsert().ALL() == null;
                    _statementSpec.OnTriggerDesc = new OnTriggerSplitStreamDesc(OnTriggerType.ON_SPLITSTREAM, isFirst, splitStreams);
                    _statementSpecStack.Clear();
                } else if (ctx.onUpdateExpr() != null) {
                    var assignments = ASTExprHelper.GetOnTriggerSetAssignments(ctx.onUpdateExpr().onSetAssignmentList(), _astExprNodeMap);
                    _statementSpec.OnTriggerDesc = new OnTriggerWindowUpdateDesc(windowName.First, windowName.Second, assignments);
                    if (ctx.onUpdateExpr().whereClause() != null) {
                        _statementSpec.FilterExprRootNode = ASTExprHelper.ExprCollectSubNodes(ctx.onUpdateExpr().whereClause(), 0, _astExprNodeMap)[0];
                    }
                } else {
                    _statementSpec.OnTriggerDesc = new OnTriggerWindowDesc(windowName.First, windowName.Second, ctx.onDeleteExpr() != null ? OnTriggerType.ON_DELETE : OnTriggerType.ON_SELECT, deleteAndSelect);
                }
            } else {
                var assignments = ASTExprHelper.GetOnTriggerSetAssignments(ctx.onSetExpr().onSetAssignmentList(), _astExprNodeMap);
                _statementSpec.OnTriggerDesc = new OnTriggerSetDesc(assignments);
            }
        }
    
        public void ExitMatchRecogPatternAlteration(EsperEPL2GrammarParser.MatchRecogPatternAlterationContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var alterNode = new RowRegexExprNodeAlteration();
            ASTExprHelper.RegExCollectAddSubNodesAddParentNode(alterNode, ctx, _astRowRegexNodeMap);
        }
    
        public void ExitCaseExpression(EsperEPL2GrammarParser.CaseExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            if (_astExprNodeMap.IsEmpty()) {
                throw ASTWalkException.From("Unexpected AST tree contains zero child element for case node", _tokenStream, ctx);
            }
            if (_astExprNodeMap.Count == 1) {
                throw ASTWalkException.From("AST tree does not contain at least when node for case node", _tokenStream, ctx);
            }
    
            var caseNode = new ExprCaseNode(ctx.expression() != null);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(caseNode, ctx, _astExprNodeMap);
        }
    
        public void ExitRowLimit(EsperEPL2GrammarParser.RowLimitContext ctx) {
            var spec = ASTOutputLimitHelper.BuildRowLimitSpec(ctx);
            _statementSpec.RowLimitSpec = spec;
    
            if ((spec.NumRowsVariable != null) || (spec.OptionalOffsetVariable != null)) {
                _statementSpec.HasVariables = true;
                ASTExprHelper.AddVariableReference(_statementSpec, spec.OptionalOffsetVariable);
            }
            _astExprNodeMap.Clear();
        }
    
        public void ExitOrderByListElement(EsperEPL2GrammarParser.OrderByListElementContext ctx) {
            var exprNode = ASTExprHelper.ExprCollectSubNodes(ctx, 0, _astExprNodeMap)[0];
            _astExprNodeMap.Clear();
            var descending = ctx.d != null;
            _statementSpec.OrderByList.Add(new OrderByItem(exprNode, descending));
        }
    
        public void ExitMergeUnmatched(EsperEPL2GrammarParser.MergeUnmatchedContext ctx) {
            HandleMergeMatchedUnmatched(ctx.expression(), false);
        }
    
        public void ExitExistsSubSelectExpression(EsperEPL2GrammarParser.ExistsSubSelectExpressionContext ctx) {
            StatementSpecRaw currentSpec = _astStatementSpecMap.Delete(ctx.subQueryExpr());
            var subselectNode = new ExprSubselectExistsNode(currentSpec);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(subselectNode, ctx, _astExprNodeMap);
        }
    
        public void ExitArrayExpression(EsperEPL2GrammarParser.ArrayExpressionContext ctx) {
            var arrayNode = new ExprArrayNode();
            if (ctx.chainedFunction() != null) {
                ASTExprHelper.ExprCollectAddSubNodesExpressionCtx(arrayNode, ctx.expression(), _astExprNodeMap);
                HandleChainedFunction(ctx, ctx.chainedFunction(), arrayNode);
            } else {
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(arrayNode, ctx, _astExprNodeMap);
            }
        }
    
        public void VisitTerminal(ITerminalNode terminalNode) {
            if (terminalNode.Symbol.Type == EsperEPL2GrammarLexer.STAR) {
                var ruleIndex = ASTUtil.GetRuleIndexIfProvided(terminalNode.Parent);
                if (ruleIndex == EsperEPL2GrammarParser.RULE_selectionListElement) {
                    _statementSpec.SelectClauseSpec.Add(new SelectClauseElementWildcard());
                }
                if (ruleIndex == EsperEPL2GrammarParser.STAR || ruleIndex == EsperEPL2GrammarParser.RULE_expressionWithTime) {
                    var exprNode = new ExprWildcardImpl();
                    ASTExprHelper.ExprCollectAddSubNodesAddParentNode(exprNode, terminalNode, _astExprNodeMap);
                }
            }
        }
    
        public void ExitAndExpression(EsperEPL2GrammarParser.AndExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var andNode = _patternNodeFactory.MakeAndNode();
            ASTExprHelper.PatternCollectAddSubnodesAddParentNode(andNode, ctx, _astPatternNodeMap);
        }
    
        public void ExitFollowedByExpression(EsperEPL2GrammarParser.FollowedByExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            IList<EsperEPL2GrammarParser.FollowedByRepeatContext> repeats = ctx.followedByRepeat();
            var maxExpressions = new ExprNode[ctx.ChildCount - 1];
            for (var i = 0; i < repeats.Count; i++) {
                EsperEPL2GrammarParser.FollowedByRepeatContext repeat = repeats[i];
                if (repeat.expression() != null) {
                    maxExpressions[i] = ASTExprHelper.ExprCollectSubNodes(repeat.expression(), 0, _astExprNodeMap)[0];
                }
            }
    
            IList<ExprNode> expressions = Collections.GetEmptyList<ExprNode>();
            if (!CollectionUtil.IsAllNullArray(maxExpressions)) {
                expressions = maxExpressions; // can contain null elements as max/no-max can be mixed
            }
    
            var fbNode = _patternNodeFactory.MakeFollowedByNode(expressions, _configurationInformation.EngineDefaults.Patterns.MaxSubexpressions != null);
            ASTExprHelper.PatternCollectAddSubnodesAddParentNode(fbNode, ctx, _astPatternNodeMap);
        }
    
        public void ExitOrExpression(EsperEPL2GrammarParser.OrExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            var orNode = _patternNodeFactory.MakeOrNode();
            ASTExprHelper.PatternCollectAddSubnodesAddParentNode(orNode, ctx, _astPatternNodeMap);
        }
    
        public void ExitQualifyExpression(EsperEPL2GrammarParser.QualifyExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            if (ctx.matchUntilRange() != null) {
                var matchUntil = MakeMatchUntil(ctx.matchUntilRange(), false);
                ASTExprHelper.PatternCollectAddSubnodesAddParentNode(matchUntil, ctx.guardPostFix(), _astPatternNodeMap);
            }
    
            EvalFactoryNode theNode;
            if (ctx.e != null) {
                theNode = _patternNodeFactory.MakeEveryNode();
            } else if (ctx.n != null) {
                theNode = _patternNodeFactory.MakeNotNode();
            } else if (ctx.d != null) {
                var exprNodes = ASTExprHelper.ExprCollectSubNodes(ctx.distinctExpressionList(), 0, _astExprNodeMap);
                theNode = _patternNodeFactory.MakeEveryDistinctNode(exprNodes);
            } else {
                throw ASTWalkException.From("Failed to recognize node");
            }
            ASTExprHelper.PatternCollectAddSubnodesAddParentNode(theNode, ctx, _astPatternNodeMap);
        }
    
        public void ExitMatchUntilExpression(EsperEPL2GrammarParser.MatchUntilExpressionContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            EvalFactoryNode node;
            if (ctx.matchUntilRange() != null) {
                node = MakeMatchUntil(ctx.matchUntilRange(), ctx.until != null);
            } else {
                node = _patternNodeFactory.MakeMatchUntilNode(null, null, null);
            }
            ASTExprHelper.PatternCollectAddSubnodesAddParentNode(node, ctx, _astPatternNodeMap);
        }
    
        private EvalFactoryNode MakeMatchUntil(EsperEPL2GrammarParser.MatchUntilRangeContext range, bool hasUntil) {
            var hasRange = true;
            ExprNode low = null;
            ExprNode high = null;
            ExprNode single = null;
            var allowZeroLowerBounds = false;
    
            if (range.low != null && range.c1 != null && range.high == null) { // [expr:]
                low = ASTExprHelper.ExprCollectSubNodes(range.low, 0, _astExprNodeMap)[0];
            } else if (range.c2 != null && range.upper != null) { // [:expr]
                high = ASTExprHelper.ExprCollectSubNodes(range.upper, 0, _astExprNodeMap)[0];
            } else if (range.low != null && range.c1 == null) { // [expr]
                single = ASTExprHelper.ExprCollectSubNodes(range.low, 0, _astExprNodeMap)[0];
                hasRange = false;
            } else if (range.low != null) { // [expr:expr]
                low = ASTExprHelper.ExprCollectSubNodes(range.low, 0, _astExprNodeMap)[0];
                high = ASTExprHelper.ExprCollectSubNodes(range.high, 0, _astExprNodeMap)[0];
                allowZeroLowerBounds = true;
            }
    
            bool tightlyBound;
            if (single != null) {
                ASTMatchUntilHelper.Validate(single, single, allowZeroLowerBounds);
                tightlyBound = true;
            } else {
                tightlyBound = ASTMatchUntilHelper.Validate(low, high, allowZeroLowerBounds);
            }
            if (hasRange && !tightlyBound && !hasUntil) {
                throw ASTWalkException.From("Variable bounds repeat operator requires an until-expression");
            }
            return _patternNodeFactory.MakeMatchUntilNode(low, high, single);
        }
    
        public void ExitGuardPostFix(EsperEPL2GrammarParser.GuardPostFixContext ctx) {
            if (ctx.ChildCount < 2) {
                return;
            }
            if (ctx.guardWhereExpression() == null && ctx.guardWhileExpression() == null) { // nested
                return;
            }
            string objectNamespace;
            string objectName;
            IList<ExprNode> obsParameters;
            if (ctx.guardWhereExpression() != null) {
                objectNamespace = ctx.guardWhereExpression().GetChild(0).GetText();
                objectName = ctx.guardWhereExpression().GetChild(2).GetText();
                obsParameters = ASTExprHelper.ExprCollectSubNodes(ctx.guardWhereExpression(), 3, _astExprNodeMap);
            } else {
                objectNamespace = GuardEnum.WHILE_GUARD.GetNamespace();
                objectName = GuardEnum.WHILE_GUARD.GetName();
                obsParameters = ASTExprHelper.ExprCollectSubNodes(ctx.guardWhileExpression(), 1, _astExprNodeMap);
            }
    
            var guardSpec = new PatternGuardSpec(objectNamespace, objectName, obsParameters);
            var guardNode = _patternNodeFactory.MakeGuardNode(guardSpec);
            ASTExprHelper.PatternCollectAddSubnodesAddParentNode(guardNode, ctx, _astPatternNodeMap);
        }
    
        public void ExitBuiltin_coalesce(EsperEPL2GrammarParser.Builtin_coalesceContext ctx) {
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(new ExprCoalesceNode(), ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_typeof(EsperEPL2GrammarParser.Builtin_typeofContext ctx) {
            var typeofNode = new ExprTypeofNode();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(typeofNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_avedev(EsperEPL2GrammarParser.Builtin_avedevContext ctx) {
            var aggregateNode = new ExprAvedevNode(ctx.DISTINCT() != null);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(aggregateNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_prevcount(EsperEPL2GrammarParser.Builtin_prevcountContext ctx) {
            var previousNode = new ExprPreviousNode(ExprPreviousNodePreviousType.PREVCOUNT);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(previousNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_stddev(EsperEPL2GrammarParser.Builtin_stddevContext ctx) {
            var aggregateNode = new ExprStddevNode(ctx.DISTINCT() != null);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(aggregateNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_sum(EsperEPL2GrammarParser.Builtin_sumContext ctx) {
            var aggregateNode = new ExprSumNode(ctx.DISTINCT() != null);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(aggregateNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_exists(EsperEPL2GrammarParser.Builtin_existsContext ctx) {
            var existsNode = new ExprPropertyExistsNode();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(existsNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_prior(EsperEPL2GrammarParser.Builtin_priorContext ctx) {
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(new ExprPriorNode(), ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_instanceof(EsperEPL2GrammarParser.Builtin_instanceofContext ctx) {
            // get class identifiers
            var classes = new List<string>();
            IList<EsperEPL2GrammarParser.ClassIdentifierContext> classCtxs = ctx.classIdentifier();
            foreach (var classCtx in classCtxs) {
                classes.Add(ASTUtil.UnescapeClassIdent(classCtx));
            }
    
            string[] idents = classes.ToArray();
            var instanceofNode = new ExprInstanceofNode(idents, _container.Resolve<ILockManager>());
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(instanceofNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_currts(EsperEPL2GrammarParser.Builtin_currtsContext ctx) {
            var timeNode = new ExprTimestampNode();
            if (ctx.chainedFunction() != null) {
                HandleChainedFunction(ctx, ctx.chainedFunction(), timeNode);
            } else {
                _astExprNodeMap.Put(ctx, timeNode);
            }
        }
    
        public void ExitBuiltin_median(EsperEPL2GrammarParser.Builtin_medianContext ctx) {
            var aggregateNode = new ExprMedianNode(ctx.DISTINCT() != null);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(aggregateNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_firstlastwindow(EsperEPL2GrammarParser.Builtin_firstlastwindowContext ctx) {
            AggregationStateType? stateType = AggregationStateTypeExtensions.FromString(ctx.firstLastWindowAggregation().q.Text, true);
            var expr = new ExprAggMultiFunctionLinearAccessNode(stateType.Value);
            ASTExprHelper.ExprCollectAddSubNodes(expr, ctx.firstLastWindowAggregation().expressionListWithNamed(), _astExprNodeMap);
            if (ctx.firstLastWindowAggregation().chainedFunction() != null) {
                HandleChainedFunction(ctx, ctx.firstLastWindowAggregation().chainedFunction(), expr);
            } else {
                _astExprNodeMap.Put(ctx, expr);
            }
        }
    
        public void ExitBuiltin_avg(EsperEPL2GrammarParser.Builtin_avgContext ctx) {
            var aggregateNode = new ExprAvgNode(ctx.DISTINCT() != null);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(aggregateNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_cast(EsperEPL2GrammarParser.Builtin_castContext ctx) {
            var classIdent = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            var castNode = new ExprCastNode(classIdent);
            if (ctx.chainedFunction() != null) {
                ASTExprHelper.ExprCollectAddSubNodes(castNode, ctx.expression(), _astExprNodeMap);
                ASTExprHelper.ExprCollectAddSingle(castNode, ctx.expressionNamedParameter(), _astExprNodeMap);
                HandleChainedFunction(ctx, ctx.chainedFunction(), castNode);
            } else {
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(castNode, ctx, _astExprNodeMap);
            }
        }
    
        public void ExitBuiltin_cnt(EsperEPL2GrammarParser.Builtin_cntContext ctx) {
            var aggregateNode = new ExprCountNode(ctx.DISTINCT() != null);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(aggregateNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_prev(EsperEPL2GrammarParser.Builtin_prevContext ctx) {
            var previousNode = new ExprPreviousNode(ExprPreviousNodePreviousType.PREV);
            if (ctx.chainedFunction() != null) {
                ASTExprHelper.ExprCollectAddSubNodesExpressionCtx(previousNode, ctx.expression(), _astExprNodeMap);
                HandleChainedFunction(ctx, ctx.chainedFunction(), previousNode);
            } else {
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(previousNode, ctx, _astExprNodeMap);
            }
        }
    
        public void ExitBuiltin_istream(EsperEPL2GrammarParser.Builtin_istreamContext ctx) {
            var istreamNode = new ExprIStreamNode();
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(istreamNode, ctx, _astExprNodeMap);
        }
    
        public void ExitBuiltin_prevwindow(EsperEPL2GrammarParser.Builtin_prevwindowContext ctx) {
            var previousNode = new ExprPreviousNode(ExprPreviousNodePreviousType.PREVWINDOW);
            if (ctx.chainedFunction() != null) {
                ASTExprHelper.ExprCollectAddSubNodes(previousNode, ctx.expression(), _astExprNodeMap);
                HandleChainedFunction(ctx, ctx.chainedFunction(), previousNode);
            } else {
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(previousNode, ctx, _astExprNodeMap);
            }
        }
    
        public void ExitBuiltin_prevtail(EsperEPL2GrammarParser.Builtin_prevtailContext ctx) {
            var previousNode = new ExprPreviousNode(ExprPreviousNodePreviousType.PREVTAIL);
            if (ctx.chainedFunction() != null) {
                ASTExprHelper.ExprCollectAddSubNodesExpressionCtx(previousNode, ctx.expression(), _astExprNodeMap);
                HandleChainedFunction(ctx, ctx.chainedFunction(), previousNode);
            } else {
                ASTExprHelper.ExprCollectAddSubNodesAddParentNode(previousNode, ctx, _astExprNodeMap);
            }
        }
    
        private PatternLevelAnnotationFlags GetPatternFlags(IEnumerable<EsperEPL2GrammarParser.AnnotationEnumContext> ctxList) {
            var flags = new PatternLevelAnnotationFlags();
            if (ctxList != null) {
                foreach (var ctx in ctxList) {
                    var desc = ASTAnnotationHelper.Walk(ctx, _engineImportService);
                    PatternLevelAnnotationUtil.ValidateSetFlags(flags, desc.Name);
                }
            }
            return flags;
        }
    
        private UniformPair<string> GetOnExprWindowName(EsperEPL2GrammarParser.OnExprContext ctx) {
            if (ctx.onDeleteExpr() != null) {
                return GetOnExprWindowName(ctx.onDeleteExpr().onExprFrom());
            }
            if (ctx.onSelectExpr() != null && ctx.onSelectExpr().onExprFrom() != null) {
                return GetOnExprWindowName(ctx.onSelectExpr().onExprFrom());
            }
            if (ctx.onUpdateExpr() != null) {
                var alias = ctx.onUpdateExpr().i;
                return new UniformPair<string>(ctx.onUpdateExpr().n.Text, alias != null ? alias.Text : null);
            }
            return null;
        }
    
        private UniformPair<string> GetOnExprWindowName(EsperEPL2GrammarParser.OnExprFromContext ctx) {
            var windowName = ctx.n.Text;
            var windowStreamName = ctx.i != null ? ctx.i.Text : null;
            return new UniformPair<string>(windowName, windowStreamName);
        }
    
        private string GetDefaultDialect() {
            return _configurationInformation.EngineDefaults.Scripts.DefaultDialect;
        }
    
        private void HandleMergeMatchedUnmatched(EsperEPL2GrammarParser.ExpressionContext expression, bool b) {
            if (_mergeMatcheds == null) {
                _mergeMatcheds = new List<OnTriggerMergeMatched>();
            }
            ExprNode filterSpec = null;
            if (expression != null) {
                filterSpec = ASTExprHelper.ExprCollectSubNodes(expression, 0, _astExprNodeMap)[0];
            }
            _mergeMatcheds.Add(new OnTriggerMergeMatched(b, filterSpec, _mergeActions));
            _mergeActions = null;
        }
    
        private void HandleMergeInsert(EsperEPL2GrammarParser.MergeInsertContext mergeInsertContext) {
            ExprNode whereCond = null;
            if (mergeInsertContext.whereClause() != null) {
                whereCond = ASTExprHelper.ExprCollectSubNodes(mergeInsertContext.whereClause(), 0, _astExprNodeMap)[0];
            }
            var expressions = new List<SelectClauseElementRaw>(_statementSpec.SelectClauseSpec.SelectExprList);
            _statementSpec.SelectClauseSpec.SelectExprList.Clear();
    
            var optionalInsertName = mergeInsertContext.classIdentifier() != null ? ASTUtil.UnescapeClassIdent(mergeInsertContext.classIdentifier()) : null;
            IList<string> columsList = ASTUtil.GetIdentList(mergeInsertContext.columnList());
            _mergeActions.Add(new OnTriggerMergeActionInsert(whereCond, optionalInsertName, columsList, expressions));
        }
    
        private void HandleChainedFunction(ParserRuleContext parentCtx, EsperEPL2GrammarParser.ChainedFunctionContext chainedCtx, ExprNode childExpression) {
            IList<ExprChainedSpec> chainSpec = ASTLibFunctionHelper.GetLibFuncChain(chainedCtx.libFunctionNoClass(), _astExprNodeMap);
            var dotNode = new ExprDotNodeImpl(chainSpec, _configurationInformation.EngineDefaults.Expression.IsDuckTyping,
                    _configurationInformation.EngineDefaults.Expression.IsUdfCache);
            if (childExpression != null) {
                dotNode.AddChildNode(childExpression);
            }
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(dotNode, parentCtx, _astExprNodeMap);
        }
    
        private void HandleFAFNamedWindowStream(EsperEPL2GrammarParser.ClassIdentifierContext node, IToken i) {
            var windowName = ASTUtil.UnescapeClassIdent(node);
            var alias = i != null ? i.Text : null;
            _statementSpec.StreamSpecs.Add(new FilterStreamSpecRaw(new FilterSpecRaw(windowName, Collections.GetEmptyList<ExprNode>(), null), _viewSpecs.ToArray(), alias, StreamSpecOptions.DEFAULT));
        }
    
        public void ExitFafInsert(EsperEPL2GrammarParser.FafInsertContext ctx) {
            IList<EsperEPL2GrammarParser.ExpressionContext> valueExprs = ctx.expressionList().expression();
            foreach (var valueExpr in valueExprs) {
                var expr = ASTExprHelper.ExprCollectSubNodes(valueExpr, 0, _astExprNodeMap)[0];
                _statementSpec.SelectClauseSpec.Add(new SelectClauseExprRawSpec(expr, null, false));
            }
            _statementSpec.FireAndForgetSpec = new FireAndForgetSpecInsert(true);
        }
    
        internal void End() {
            if (_astExprNodeMap.Count > 1) {
                throw ASTWalkException.From("Unexpected AST tree contains left over child elements," +
                        " not all expression nodes have been removed from AST-to-expression nodes map");
            }
            if (_astPatternNodeMap.Count > 1) {
                throw ASTWalkException.From("Unexpected AST tree contains left over child elements," +
                        " not all pattern nodes have been removed from AST-to-pattern nodes map");
            }
    
            // detect insert-into fire-and-forget query
            if (_statementSpec.InsertIntoDesc != null && _statementSpec.StreamSpecs.IsEmpty() && _statementSpec.FireAndForgetSpec == null) {
                _statementSpec.FireAndForgetSpec = new FireAndForgetSpecInsert(false);
            }
    
            _statementSpec.SubstitutionParameters = _substitutionParamNodes;
        }
    
        public void ExitBuiltin_grouping(EsperEPL2GrammarParser.Builtin_groupingContext ctx) {
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(new ExprGroupingNode(), ctx, _astExprNodeMap);
        }
    
    
        public void ExitBuiltin_groupingid(EsperEPL2GrammarParser.Builtin_groupingidContext ctx) {
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(new ExprGroupingIdNode(), ctx, _astExprNodeMap);
        }
    
        public void ExitIntoTableExpr(EsperEPL2GrammarParser.IntoTableExprContext ctx) {
            var name = ctx.i.Text;
            _statementSpec.IntoTableSpec = new IntoTableSpec(name);
        }
    
        public void ExitCreateTableExpr(EsperEPL2GrammarParser.CreateTableExprContext ctx) {
            var tableName = ctx.n.Text;
    
            // obtain item declarations
            IList<CreateTableColumn> cols = ASTTableHelper.GetColumns(ctx.createTableColumnList().createTableColumn(), _astExprNodeMap, _engineImportService);
            _statementSpec.CreateTableDesc = new CreateTableDesc(tableName, cols);
        }
    
        public void ExitJsonobject(EsperEPL2GrammarParser.JsonobjectContext ctx) {
            var node = new ExprConstantNodeImpl(ASTJsonHelper.WalkObject(_tokenStream, ctx));
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(node, ctx, _astExprNodeMap);
        }
    
        public void ExitPropertyStreamSelector(EsperEPL2GrammarParser.PropertyStreamSelectorContext ctx) {
            var streamWildcard = ctx.s.Text;
            var node = new ExprStreamUnderlyingNodeImpl(streamWildcard, true);
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(node, ctx, _astExprNodeMap);
        }
    
        public void ExitExpressionNamedParameter(EsperEPL2GrammarParser.ExpressionNamedParameterContext ctx) {
            var named = new ExprNamedParameterNodeImpl(ctx.IDENT().GetText());
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(named, ctx, _astExprNodeMap);
        }
    
        public void ExitExpressionNamedParameterWithTime(EsperEPL2GrammarParser.ExpressionNamedParameterWithTimeContext ctx) {
            var named = new ExprNamedParameterNodeImpl(ctx.IDENT().GetText());
            ASTExprHelper.ExprCollectAddSubNodesAddParentNode(named, ctx, _astExprNodeMap);
        }
    
        private ExprIdentNode ValidateOuterJoinGetIdentNode(ExprNode exprNode) {
            if (exprNode is ExprIdentNode) {
                return (ExprIdentNode) exprNode;
            }
            if (exprNode is ExprTableAccessNodeSubprop) {
                var subprop = (ExprTableAccessNodeSubprop) exprNode;
                return new ExprIdentNodeImpl(subprop.SubpropName, subprop.TableName);
            }
            throw ASTWalkException.From("Failed to validated 'on'-keyword expressions in outer join, expected identifiers only");
        }
    
        public void EnterContextExpr(EsperEPL2GrammarParser.ContextExprContext ctx) {
        }
    
        public void EnterExpressionList(EsperEPL2GrammarParser.ExpressionListContext ctx) {
        }
    
        public void ExitExpressionList(EsperEPL2GrammarParser.ExpressionListContext ctx) {
        }
    
        public void EnterSelectionList(EsperEPL2GrammarParser.SelectionListContext ctx) {
        }
    
        public void ExitSelectionList(EsperEPL2GrammarParser.SelectionListContext ctx) {
        }
    
        public void EnterEvalRelationalExpression(EsperEPL2GrammarParser.EvalRelationalExpressionContext ctx) {
        }
    
        public void EnterPatternInclusionExpression(EsperEPL2GrammarParser.PatternInclusionExpressionContext ctx) {
        }
    
        public void ExitPatternInclusionExpression(EsperEPL2GrammarParser.PatternInclusionExpressionContext ctx) {
        }
    
        public void EnterLibFunction(EsperEPL2GrammarParser.LibFunctionContext ctx) {
        }
    
        public void EnterSelectionListElement(EsperEPL2GrammarParser.SelectionListElementContext ctx) {
        }
    
        public void ExitSelectionListElement(EsperEPL2GrammarParser.SelectionListElementContext ctx) {
        }
    
        public void EnterGopOutTypeList(EsperEPL2GrammarParser.GopOutTypeListContext ctx) {
        }
    
        public void ExitGopOutTypeList(EsperEPL2GrammarParser.GopOutTypeListContext ctx) {
        }
    
        public void EnterGopOutTypeItem(EsperEPL2GrammarParser.GopOutTypeItemContext ctx) {
        }
    
        public void ExitGopOutTypeItem(EsperEPL2GrammarParser.GopOutTypeItemContext ctx) {
        }
    
        public void EnterMatchRecog(EsperEPL2GrammarParser.MatchRecogContext ctx) {
        }
    
        public void EnterJsonmembers(EsperEPL2GrammarParser.JsonmembersContext ctx) {
        }
    
        public void ExitJsonmembers(EsperEPL2GrammarParser.JsonmembersContext ctx) {
        }
    
        public void EnterNumber(EsperEPL2GrammarParser.NumberContext ctx) {
        }
    
        public void ExitNumber(EsperEPL2GrammarParser.NumberContext ctx) {
        }
    
        public void EnterVariantList(EsperEPL2GrammarParser.VariantListContext ctx) {
        }
    
        public void ExitVariantList(EsperEPL2GrammarParser.VariantListContext ctx) {
        }
    
        public void EnterMatchRecogPartitionBy(EsperEPL2GrammarParser.MatchRecogPartitionByContext ctx) {
        }
    
        public void EnterOutputLimitAfter(EsperEPL2GrammarParser.OutputLimitAfterContext ctx) {
        }
    
        public void ExitOutputLimitAfter(EsperEPL2GrammarParser.OutputLimitAfterContext ctx) {
        }
    
        public void EnterCreateColumnList(EsperEPL2GrammarParser.CreateColumnListContext ctx) {
        }
    
        public void ExitCreateColumnList(EsperEPL2GrammarParser.CreateColumnListContext ctx) {
        }
    
        public void EnterMergeMatchedItem(EsperEPL2GrammarParser.MergeMatchedItemContext ctx) {
        }
    
        public void EnterMatchRecogMatchesSelection(EsperEPL2GrammarParser.MatchRecogMatchesSelectionContext ctx) {
        }
    
        public void ExitMatchRecogMatchesSelection(EsperEPL2GrammarParser.MatchRecogMatchesSelectionContext ctx) {
        }
    
        public void EnterClassIdentifier(EsperEPL2GrammarParser.ClassIdentifierContext ctx) {
        }
    
        public void ExitClassIdentifier(EsperEPL2GrammarParser.ClassIdentifierContext ctx) {
        }
    
        public void EnterDatabaseJoinExpression(EsperEPL2GrammarParser.DatabaseJoinExpressionContext ctx) {
        }
    
        public void ExitDatabaseJoinExpression(EsperEPL2GrammarParser.DatabaseJoinExpressionContext ctx) {
        }
    
        public void EnterMatchRecogDefineItem(EsperEPL2GrammarParser.MatchRecogDefineItemContext ctx) {
        }
    
        public void EnterLibFunctionArgs(EsperEPL2GrammarParser.LibFunctionArgsContext ctx) {
        }
    
        public void ExitLibFunctionArgs(EsperEPL2GrammarParser.LibFunctionArgsContext ctx) {
        }
    
        public void EnterMergeUnmatchedItem(EsperEPL2GrammarParser.MergeUnmatchedItemContext ctx) {
        }
    
        public void EnterHavingClause(EsperEPL2GrammarParser.HavingClauseContext ctx) {
        }
    
        public void EnterMatchRecogMeasureItem(EsperEPL2GrammarParser.MatchRecogMeasureItemContext ctx) {
        }
    
        public void EnterMatchRecogMatchesInterval(EsperEPL2GrammarParser.MatchRecogMatchesIntervalContext ctx) {
        }
    
        public void ExitMatchRecogMatchesInterval(EsperEPL2GrammarParser.MatchRecogMatchesIntervalContext ctx) {
        }
    
        public void EnterObserverExpression(EsperEPL2GrammarParser.ObserverExpressionContext ctx) {
        }
    
        public void EnterMatchRecogPatternNested(EsperEPL2GrammarParser.MatchRecogPatternNestedContext ctx) {
        }
    
        public void EnterCreateContextFilter(EsperEPL2GrammarParser.CreateContextFilterContext ctx) {
        }
    
        public void ExitCreateContextFilter(EsperEPL2GrammarParser.CreateContextFilterContext ctx) {
        }
    
        public void EnterEvalOrExpression(EsperEPL2GrammarParser.EvalOrExpressionContext ctx) {
        }
    
        public void EnterExpressionDef(EsperEPL2GrammarParser.ExpressionDefContext ctx) {
        }
    
        public void ExitExpressionDef(EsperEPL2GrammarParser.ExpressionDefContext ctx) {
        }
    
        public void EnterOutputLimitAndTerm(EsperEPL2GrammarParser.OutputLimitAndTermContext ctx) {
        }
    
        public void ExitOutputLimitAndTerm(EsperEPL2GrammarParser.OutputLimitAndTermContext ctx) {
        }
    
        public void EnterNumericListParameter(EsperEPL2GrammarParser.NumericListParameterContext ctx) {
        }
    
        public void ExitNumericListParameter(EsperEPL2GrammarParser.NumericListParameterContext ctx) {
        }
    
        public void EnterTimePeriod(EsperEPL2GrammarParser.TimePeriodContext ctx) {
        }
    
        public void EnterEventPropertyAtomic(EsperEPL2GrammarParser.EventPropertyAtomicContext ctx) {
        }
    
        public void ExitEventPropertyAtomic(EsperEPL2GrammarParser.EventPropertyAtomicContext ctx) {
        }
    
        public void EnterSubSelectGroupExpression(EsperEPL2GrammarParser.SubSelectGroupExpressionContext ctx) {
        }
    
        public void ExitSubSelectGroupExpression(EsperEPL2GrammarParser.SubSelectGroupExpressionContext ctx) {
        }
    
        public void EnterOuterJoinList(EsperEPL2GrammarParser.OuterJoinListContext ctx) {
        }
    
        public void ExitOuterJoinList(EsperEPL2GrammarParser.OuterJoinListContext ctx) {
        }
    
        public void EnterSelectionListElementExpr(EsperEPL2GrammarParser.SelectionListElementExprContext ctx) {
        }
    
        public void EnterEventFilterExpression(EsperEPL2GrammarParser.EventFilterExpressionContext ctx) {
        }
    
        public void EnterGopParamsItemList(EsperEPL2GrammarParser.GopParamsItemListContext ctx) {
        }
    
        public void ExitGopParamsItemList(EsperEPL2GrammarParser.GopParamsItemListContext ctx) {
        }
    
        public void EnterMatchRecogPatternConcat(EsperEPL2GrammarParser.MatchRecogPatternConcatContext ctx) {
        }
    
        public void EnterNumberconstant(EsperEPL2GrammarParser.NumberconstantContext ctx) {
        }
    
        public void EnterOnSetAssignment(EsperEPL2GrammarParser.OnSetAssignmentContext ctx) {
        }
    
        public void ExitOnSetAssignment(EsperEPL2GrammarParser.OnSetAssignmentContext ctx) {
        }
    
        public void EnterContextContextNested(EsperEPL2GrammarParser.ContextContextNestedContext ctx) {
        }
    
        public void ExitContextContextNested(EsperEPL2GrammarParser.ContextContextNestedContext ctx) {
        }
    
        public void EnterExpressionWithTime(EsperEPL2GrammarParser.ExpressionWithTimeContext ctx) {
        }
    
        public void ExitExpressionWithTime(EsperEPL2GrammarParser.ExpressionWithTimeContext ctx) {
        }
    
        public void EnterMatchRecogPattern(EsperEPL2GrammarParser.MatchRecogPatternContext ctx) {
        }
    
        public void EnterMergeInsert(EsperEPL2GrammarParser.MergeInsertContext ctx) {
        }
    
        public void ExitMergeInsert(EsperEPL2GrammarParser.MergeInsertContext ctx) {
        }
    
        public void EnterOrderByListExpr(EsperEPL2GrammarParser.OrderByListExprContext ctx) {
        }
    
        public void ExitOrderByListExpr(EsperEPL2GrammarParser.OrderByListExprContext ctx) {
        }
    
        public void EnterElementValuePairsEnum(EsperEPL2GrammarParser.ElementValuePairsEnumContext ctx) {
        }
    
        public void ExitElementValuePairsEnum(EsperEPL2GrammarParser.ElementValuePairsEnumContext ctx) {
        }
    
        public void EnterDistinctExpressionAtom(EsperEPL2GrammarParser.DistinctExpressionAtomContext ctx) {
        }
    
        public void ExitDistinctExpressionAtom(EsperEPL2GrammarParser.DistinctExpressionAtomContext ctx) {
        }
    
        public void EnterExpression(EsperEPL2GrammarParser.ExpressionContext ctx) {
        }
    
        public void ExitExpression(EsperEPL2GrammarParser.ExpressionContext ctx) {
        }
    
        public void EnterWhereClause(EsperEPL2GrammarParser.WhereClauseContext ctx) {
        }
    
        public void EnterCreateColumnListElement(EsperEPL2GrammarParser.CreateColumnListElementContext ctx) {
        }
    
        public void ExitCreateColumnListElement(EsperEPL2GrammarParser.CreateColumnListElementContext ctx) {
        }
    
        public void EnterGopList(EsperEPL2GrammarParser.GopListContext ctx) {
        }
    
        public void ExitGopList(EsperEPL2GrammarParser.GopListContext ctx) {
        }
    
        public void EnterPatternFilterAnnotation(EsperEPL2GrammarParser.PatternFilterAnnotationContext ctx) {
        }
    
        public void ExitPatternFilterAnnotation(EsperEPL2GrammarParser.PatternFilterAnnotationContext ctx) {
        }
    
        public void EnterElementValueArrayEnum(EsperEPL2GrammarParser.ElementValueArrayEnumContext ctx) {
        }
    
        public void ExitElementValueArrayEnum(EsperEPL2GrammarParser.ElementValueArrayEnumContext ctx) {
        }
    
        public void EnterHourPart(EsperEPL2GrammarParser.HourPartContext ctx) {
        }
    
        public void ExitHourPart(EsperEPL2GrammarParser.HourPartContext ctx) {
        }
    
        public void EnterOnDeleteExpr(EsperEPL2GrammarParser.OnDeleteExprContext ctx) {
        }
    
        public void ExitOnDeleteExpr(EsperEPL2GrammarParser.OnDeleteExprContext ctx) {
        }
    
        public void EnterMatchRecogPatternAtom(EsperEPL2GrammarParser.MatchRecogPatternAtomContext ctx) {
        }
    
        public void EnterGopOutTypeParam(EsperEPL2GrammarParser.GopOutTypeParamContext ctx) {
        }
    
        public void ExitGopOutTypeParam(EsperEPL2GrammarParser.GopOutTypeParamContext ctx) {
        }
    
        public void EnterMergeItem(EsperEPL2GrammarParser.MergeItemContext ctx) {
        }
    
        public void ExitMergeItem(EsperEPL2GrammarParser.MergeItemContext ctx) {
        }
    
        public void EnterYearPart(EsperEPL2GrammarParser.YearPartContext ctx) {
        }
    
        public void ExitYearPart(EsperEPL2GrammarParser.YearPartContext ctx) {
        }
    
        public void EnterEventPropertyOrLibFunction(EsperEPL2GrammarParser.EventPropertyOrLibFunctionContext ctx) {
        }
    
        public void ExitEventPropertyOrLibFunction(EsperEPL2GrammarParser.EventPropertyOrLibFunctionContext ctx) {
        }
    
        public void EnterCreateDataflow(EsperEPL2GrammarParser.CreateDataflowContext ctx) {
        }
    
        public void EnterUpdateExpr(EsperEPL2GrammarParser.UpdateExprContext ctx) {
        }
    
        public void EnterFrequencyOperand(EsperEPL2GrammarParser.FrequencyOperandContext ctx) {
        }
    
        public void EnterOnSetAssignmentList(EsperEPL2GrammarParser.OnSetAssignmentListContext ctx) {
        }
    
        public void ExitOnSetAssignmentList(EsperEPL2GrammarParser.OnSetAssignmentListContext ctx) {
        }
    
        public void EnterPropertyStreamSelector(EsperEPL2GrammarParser.PropertyStreamSelectorContext ctx) {
        }
    
        public void EnterInsertIntoExpr(EsperEPL2GrammarParser.InsertIntoExprContext ctx) {
        }
    
        public void EnterCreateVariableExpr(EsperEPL2GrammarParser.CreateVariableExprContext ctx) {
        }
    
        public void EnterGopParamsItem(EsperEPL2GrammarParser.GopParamsItemContext ctx) {
        }
    
        public void ExitGopParamsItem(EsperEPL2GrammarParser.GopParamsItemContext ctx) {
        }
    
        public void EnterOnStreamExpr(EsperEPL2GrammarParser.OnStreamExprContext ctx) {
        }
    
        public void EnterPropertyExpressionAtomic(EsperEPL2GrammarParser.PropertyExpressionAtomicContext ctx) {
        }
    
        public void EnterGopDetail(EsperEPL2GrammarParser.GopDetailContext ctx) {
        }
    
        public void ExitGopDetail(EsperEPL2GrammarParser.GopDetailContext ctx) {
        }
    
        public void EnterGop(EsperEPL2GrammarParser.GopContext ctx) {
        }
    
        public void ExitGop(EsperEPL2GrammarParser.GopContext ctx) {
        }
    
        public void EnterOutputClauseInsert(EsperEPL2GrammarParser.OutputClauseInsertContext ctx) {
        }
    
        public void ExitOutputClauseInsert(EsperEPL2GrammarParser.OutputClauseInsertContext ctx) {
        }
    
        public void EnterEplExpression(EsperEPL2GrammarParser.EplExpressionContext ctx) {
        }
    
        public void ExitEplExpression(EsperEPL2GrammarParser.EplExpressionContext ctx) {
        }
    
        public void EnterOnMergeExpr(EsperEPL2GrammarParser.OnMergeExprContext ctx) {
        }
    
        public void ExitOnMergeExpr(EsperEPL2GrammarParser.OnMergeExprContext ctx) {
        }
    
        public void EnterFafUpdate(EsperEPL2GrammarParser.FafUpdateContext ctx) {
        }
    
        public void EnterCreateSelectionList(EsperEPL2GrammarParser.CreateSelectionListContext ctx) {
        }
    
        public void ExitCreateSelectionList(EsperEPL2GrammarParser.CreateSelectionListContext ctx) {
        }
    
        public void EnterOnSetExpr(EsperEPL2GrammarParser.OnSetExprContext ctx) {
        }
    
        public void ExitOnSetExpr(EsperEPL2GrammarParser.OnSetExprContext ctx) {
        }
    
        public void EnterBitWiseExpression(EsperEPL2GrammarParser.BitWiseExpressionContext ctx) {
        }
    
        public void EnterChainedFunction(EsperEPL2GrammarParser.ChainedFunctionContext ctx) {
        }
    
        public void ExitChainedFunction(EsperEPL2GrammarParser.ChainedFunctionContext ctx) {
        }
    
        public void EnterMatchRecogPatternUnary(EsperEPL2GrammarParser.MatchRecogPatternUnaryContext ctx) {
        }
    
        public void ExitMatchRecogPatternUnary(EsperEPL2GrammarParser.MatchRecogPatternUnaryContext ctx) {
        }
    
        public void EnterBetweenList(EsperEPL2GrammarParser.BetweenListContext ctx) {
        }
    
        public void ExitBetweenList(EsperEPL2GrammarParser.BetweenListContext ctx) {
        }
    
        public void EnterSecondPart(EsperEPL2GrammarParser.SecondPartContext ctx) {
        }
    
        public void ExitSecondPart(EsperEPL2GrammarParser.SecondPartContext ctx) {
        }
    
        public void EnterEvalEqualsExpression(EsperEPL2GrammarParser.EvalEqualsExpressionContext ctx) {
        }
    
        public void EnterGopConfig(EsperEPL2GrammarParser.GopConfigContext ctx) {
        }
    
        public void EnterMergeMatched(EsperEPL2GrammarParser.MergeMatchedContext ctx) {
        }
    
        public void EnterCreateSelectionListElement(EsperEPL2GrammarParser.CreateSelectionListElementContext ctx) {
        }
    
        public void EnterFafDelete(EsperEPL2GrammarParser.FafDeleteContext ctx) {
        }
    
        public void EnterDayPart(EsperEPL2GrammarParser.DayPartContext ctx) {
        }
    
        public void ExitDayPart(EsperEPL2GrammarParser.DayPartContext ctx) {
        }
    
        public void EnterConstant(EsperEPL2GrammarParser.ConstantContext ctx) {
        }
    
        public void EnterGopOut(EsperEPL2GrammarParser.GopOutContext ctx) {
        }
    
        public void ExitGopOut(EsperEPL2GrammarParser.GopOutContext ctx) {
        }
    
        public void EnterGuardWhereExpression(EsperEPL2GrammarParser.GuardWhereExpressionContext ctx) {
        }
    
        public void ExitGuardWhereExpression(EsperEPL2GrammarParser.GuardWhereExpressionContext ctx) {
        }
    
        public void EnterKeywordAllowedIdent(EsperEPL2GrammarParser.KeywordAllowedIdentContext ctx) {
        }
    
        public void ExitKeywordAllowedIdent(EsperEPL2GrammarParser.KeywordAllowedIdentContext ctx) {
        }
    
        public void EnterCreateContextGroupItem(EsperEPL2GrammarParser.CreateContextGroupItemContext ctx) {
        }
    
        public void ExitCreateContextGroupItem(EsperEPL2GrammarParser.CreateContextGroupItemContext ctx) {
        }
    
        public void EnterEvalAndExpression(EsperEPL2GrammarParser.EvalAndExpressionContext ctx) {
        }
    
        public void EnterMultiplyExpression(EsperEPL2GrammarParser.MultiplyExpressionContext ctx) {
        }
    
        public void EnterExpressionLambdaDecl(EsperEPL2GrammarParser.ExpressionLambdaDeclContext ctx) {
        }
    
        public void ExitExpressionLambdaDecl(EsperEPL2GrammarParser.ExpressionLambdaDeclContext ctx) {
        }
    
        public void EnterPropertyExpression(EsperEPL2GrammarParser.PropertyExpressionContext ctx) {
        }
    
        public void ExitPropertyExpression(EsperEPL2GrammarParser.PropertyExpressionContext ctx) {
        }
    
        public void EnterOuterJoinIdentPair(EsperEPL2GrammarParser.OuterJoinIdentPairContext ctx) {
        }
    
        public void ExitOuterJoinIdentPair(EsperEPL2GrammarParser.OuterJoinIdentPairContext ctx) {
        }
    
        public void EnterGopOutItem(EsperEPL2GrammarParser.GopOutItemContext ctx) {
        }
    
        public void ExitGopOutItem(EsperEPL2GrammarParser.GopOutItemContext ctx) {
        }
    
        public void EnterForExpr(EsperEPL2GrammarParser.ForExprContext ctx) {
        }
    
        public void EnterPropertyExpressionSelect(EsperEPL2GrammarParser.PropertyExpressionSelectContext ctx) {
        }
    
        public void ExitPropertyExpressionSelect(EsperEPL2GrammarParser.PropertyExpressionSelectContext ctx) {
        }
    
        public void EnterExpressionQualifyable(EsperEPL2GrammarParser.ExpressionQualifyableContext ctx) {
        }
    
        public void EnterExpressionDialect(EsperEPL2GrammarParser.ExpressionDialectContext ctx) {
        }
    
        public void ExitExpressionDialect(EsperEPL2GrammarParser.ExpressionDialectContext ctx) {
        }
    
        public void EnterStartEventPropertyRule(EsperEPL2GrammarParser.StartEventPropertyRuleContext ctx) {
        }
    
        public void ExitStartEventPropertyRule(EsperEPL2GrammarParser.StartEventPropertyRuleContext ctx) {
        }
    
        public void EnterPropertySelectionListElement(EsperEPL2GrammarParser.PropertySelectionListElementContext ctx) {
        }
    
        public void EnterExpressionDecl(EsperEPL2GrammarParser.ExpressionDeclContext ctx) {
        }
    
        public void EnterSubstitution(EsperEPL2GrammarParser.SubstitutionContext ctx) {
        }
    
        public void EnterCrontabLimitParameterSet(EsperEPL2GrammarParser.CrontabLimitParameterSetContext ctx) {
        }
    
        public void ExitCrontabLimitParameterSet(EsperEPL2GrammarParser.CrontabLimitParameterSetContext ctx) {
        }
    
        public void EnterWeekDayOperator(EsperEPL2GrammarParser.WeekDayOperatorContext ctx) {
        }
    
        public void EnterWhenClause(EsperEPL2GrammarParser.WhenClauseContext ctx) {
        }
    
        public void ExitWhenClause(EsperEPL2GrammarParser.WhenClauseContext ctx) {
        }
    
        public void EnterNewAssign(EsperEPL2GrammarParser.NewAssignContext ctx) {
        }
    
        public void ExitNewAssign(EsperEPL2GrammarParser.NewAssignContext ctx) {
        }
    
        public void EnterLastWeekdayOperand(EsperEPL2GrammarParser.LastWeekdayOperandContext ctx) {
        }
    
        public void EnterGroupByListExpr(EsperEPL2GrammarParser.GroupByListExprContext ctx) {
        }
    
        public void EnterStreamSelector(EsperEPL2GrammarParser.StreamSelectorContext ctx) {
        }
    
        public void EnterStartJsonValueRule(EsperEPL2GrammarParser.StartJsonValueRuleContext ctx) {
        }
    
        public void ExitStartJsonValueRule(EsperEPL2GrammarParser.StartJsonValueRuleContext ctx) {
        }
    
        public void EnterStreamExpression(EsperEPL2GrammarParser.StreamExpressionContext ctx) {
        }
    
        public void EnterOuterJoinIdent(EsperEPL2GrammarParser.OuterJoinIdentContext ctx) {
        }
    
        public void ExitOuterJoinIdent(EsperEPL2GrammarParser.OuterJoinIdentContext ctx) {
        }
    
        public void EnterCreateIndexColumnList(EsperEPL2GrammarParser.CreateIndexColumnListContext ctx) {
        }
    
        public void ExitCreateIndexColumnList(EsperEPL2GrammarParser.CreateIndexColumnListContext ctx) {
        }
    
        public void EnterColumnList(EsperEPL2GrammarParser.ColumnListContext ctx) {
        }
    
        public void ExitColumnList(EsperEPL2GrammarParser.ColumnListContext ctx) {
        }
    
        public void EnterPatternFilterExpression(EsperEPL2GrammarParser.PatternFilterExpressionContext ctx) {
        }
    
        public void EnterJsonpair(EsperEPL2GrammarParser.JsonpairContext ctx) {
        }
    
        public void ExitJsonpair(EsperEPL2GrammarParser.JsonpairContext ctx) {
        }
    
        public void EnterOnSelectExpr(EsperEPL2GrammarParser.OnSelectExprContext ctx) {
        }
    
        public void EnterElementValuePairEnum(EsperEPL2GrammarParser.ElementValuePairEnumContext ctx) {
        }
    
        public void ExitElementValuePairEnum(EsperEPL2GrammarParser.ElementValuePairEnumContext ctx) {
        }
    
        public void EnterStartPatternExpressionRule(EsperEPL2GrammarParser.StartPatternExpressionRuleContext ctx) {
        }
    
        public void EnterSelectionListElementAnno(EsperEPL2GrammarParser.SelectionListElementAnnoContext ctx) {
        }
    
        public void ExitSelectionListElementAnno(EsperEPL2GrammarParser.SelectionListElementAnnoContext ctx) {
        }
    
        public void EnterOutputLimit(EsperEPL2GrammarParser.OutputLimitContext ctx) {
        }
    
        public void EnterCreateContextDistinct(EsperEPL2GrammarParser.CreateContextDistinctContext ctx) {
        }
    
        public void ExitCreateContextDistinct(EsperEPL2GrammarParser.CreateContextDistinctContext ctx) {
        }
    
        public void EnterJsonelements(EsperEPL2GrammarParser.JsonelementsContext ctx) {
        }
    
        public void ExitJsonelements(EsperEPL2GrammarParser.JsonelementsContext ctx) {
        }
    
        public void EnterNumericParameterList(EsperEPL2GrammarParser.NumericParameterListContext ctx) {
        }
    
        public void EnterLibFunctionWithClass(EsperEPL2GrammarParser.LibFunctionWithClassContext ctx) {
        }
    
        public void ExitLibFunctionWithClass(EsperEPL2GrammarParser.LibFunctionWithClassContext ctx) {
        }
    
        public void EnterStringconstant(EsperEPL2GrammarParser.StringconstantContext ctx) {
        }
    
        public void ExitStringconstant(EsperEPL2GrammarParser.StringconstantContext ctx) {
        }
    
        public void EnterCreateSchemaExpr(EsperEPL2GrammarParser.CreateSchemaExprContext ctx) {
        }
    
        public void EnterElseClause(EsperEPL2GrammarParser.ElseClauseContext ctx) {
        }
    
        public void ExitElseClause(EsperEPL2GrammarParser.ElseClauseContext ctx) {
        }
    
        public void EnterGuardWhileExpression(EsperEPL2GrammarParser.GuardWhileExpressionContext ctx) {
        }
    
        public void ExitGuardWhileExpression(EsperEPL2GrammarParser.GuardWhileExpressionContext ctx) {
        }
    
        public void EnterCreateWindowExprModelAfter(EsperEPL2GrammarParser.CreateWindowExprModelAfterContext ctx) {
        }
    
        public void ExitCreateWindowExprModelAfter(EsperEPL2GrammarParser.CreateWindowExprModelAfterContext ctx) {
        }
    
        public void EnterMatchRecogMatchesAfterSkip(EsperEPL2GrammarParser.MatchRecogMatchesAfterSkipContext ctx) {
        }
    
        public void ExitMatchRecogMatchesAfterSkip(EsperEPL2GrammarParser.MatchRecogMatchesAfterSkipContext ctx) {
        }
    
        public void EnterCreateContextDetail(EsperEPL2GrammarParser.CreateContextDetailContext ctx) {
        }
    
        public void ExitCreateContextDetail(EsperEPL2GrammarParser.CreateContextDetailContext ctx) {
        }
    
        public void EnterMonthPart(EsperEPL2GrammarParser.MonthPartContext ctx) {
        }
    
        public void ExitMonthPart(EsperEPL2GrammarParser.MonthPartContext ctx) {
        }
    
        public void EnterPatternExpression(EsperEPL2GrammarParser.PatternExpressionContext ctx) {
        }
    
        public void ExitPatternExpression(EsperEPL2GrammarParser.PatternExpressionContext ctx) {
        }
    
        public void EnterLastOperator(EsperEPL2GrammarParser.LastOperatorContext ctx) {
        }
    
        public void EnterCreateSchemaDef(EsperEPL2GrammarParser.CreateSchemaDefContext ctx) {
        }
    
        public void ExitCreateSchemaDef(EsperEPL2GrammarParser.CreateSchemaDefContext ctx) {
        }
    
        public void EnterEventPropertyIdent(EsperEPL2GrammarParser.EventPropertyIdentContext ctx) {
        }
    
        public void ExitEventPropertyIdent(EsperEPL2GrammarParser.EventPropertyIdentContext ctx) {
        }
    
        public void EnterCreateIndexExpr(EsperEPL2GrammarParser.CreateIndexExprContext ctx) {
        }
    
        public void EnterAtomicExpression(EsperEPL2GrammarParser.AtomicExpressionContext ctx) {
        }
    
        public void ExitAtomicExpression(EsperEPL2GrammarParser.AtomicExpressionContext ctx) {
        }
    
        public void EnterJsonvalue(EsperEPL2GrammarParser.JsonvalueContext ctx) {
        }
    
        public void ExitJsonvalue(EsperEPL2GrammarParser.JsonvalueContext ctx) {
        }
    
        public void EnterLibFunctionNoClass(EsperEPL2GrammarParser.LibFunctionNoClassContext ctx) {
        }
    
        public void ExitLibFunctionNoClass(EsperEPL2GrammarParser.LibFunctionNoClassContext ctx) {
        }
    
        public void EnterElementValueEnum(EsperEPL2GrammarParser.ElementValueEnumContext ctx) {
        }
    
        public void ExitElementValueEnum(EsperEPL2GrammarParser.ElementValueEnumContext ctx) {
        }
    
        public void EnterOnUpdateExpr(EsperEPL2GrammarParser.OnUpdateExprContext ctx) {
        }
    
        public void ExitOnUpdateExpr(EsperEPL2GrammarParser.OnUpdateExprContext ctx) {
        }
    
        public void EnterAnnotationEnum(EsperEPL2GrammarParser.AnnotationEnumContext ctx) {
        }
    
        public void EnterCreateContextExpr(EsperEPL2GrammarParser.CreateContextExprContext ctx) {
        }
    
        public void EnterLastOperand(EsperEPL2GrammarParser.LastOperandContext ctx) {
        }
    
        public void EnterExpressionWithTimeInclLast(EsperEPL2GrammarParser.ExpressionWithTimeInclLastContext ctx) {
        }
    
        public void ExitExpressionWithTimeInclLast(EsperEPL2GrammarParser.ExpressionWithTimeInclLastContext ctx) {
        }
    
        public void EnterCreateContextPartitionItem(EsperEPL2GrammarParser.CreateContextPartitionItemContext ctx) {
        }
    
        public void ExitCreateContextPartitionItem(EsperEPL2GrammarParser.CreateContextPartitionItemContext ctx) {
        }
    
        public void EnterCreateWindowExpr(EsperEPL2GrammarParser.CreateWindowExprContext ctx) {
        }
    
        public void EnterVariantListElement(EsperEPL2GrammarParser.VariantListElementContext ctx) {
        }
    
        public void ExitVariantListElement(EsperEPL2GrammarParser.VariantListElementContext ctx) {
        }
    
        public void EnterCreateExpressionExpr(EsperEPL2GrammarParser.CreateExpressionExprContext ctx) {
        }
    
        public void EnterRangeOperand(EsperEPL2GrammarParser.RangeOperandContext ctx) {
        }
    
        public void EnterInSubSelectQuery(EsperEPL2GrammarParser.InSubSelectQueryContext ctx) {
        }
    
        public void ExitInSubSelectQuery(EsperEPL2GrammarParser.InSubSelectQueryContext ctx) {
        }
    
        public void EnterEscapableStr(EsperEPL2GrammarParser.EscapableStrContext ctx) {
        }
    
        public void ExitEscapableStr(EsperEPL2GrammarParser.EscapableStrContext ctx) {
        }
    
        public void EnterRowSubSelectExpression(EsperEPL2GrammarParser.RowSubSelectExpressionContext ctx) {
        }
    
        public void EnterUnaryExpression(EsperEPL2GrammarParser.UnaryExpressionContext ctx) {
        }
    
        public void EnterDistinctExpressionList(EsperEPL2GrammarParser.DistinctExpressionListContext ctx) {
        }
    
        public void ExitDistinctExpressionList(EsperEPL2GrammarParser.DistinctExpressionListContext ctx) {
        }
    
        public void ExitOnSelectInsertExpr(EsperEPL2GrammarParser.OnSelectInsertExprContext ctx) {
        }
    
        public void EnterSelectClause(EsperEPL2GrammarParser.SelectClauseContext ctx) {
        }
    
        public void EnterConcatenationExpr(EsperEPL2GrammarParser.ConcatenationExprContext ctx) {
        }
    
        public void EnterStartEPLExpressionRule(EsperEPL2GrammarParser.StartEPLExpressionRuleContext ctx) {
        }
    
        public void ExitStartEPLExpressionRule(EsperEPL2GrammarParser.StartEPLExpressionRuleContext ctx) {
        }
    
        public void EnterSubSelectFilterExpr(EsperEPL2GrammarParser.SubSelectFilterExprContext ctx) {
        }
    
        public void EnterCreateContextCoalesceItem(EsperEPL2GrammarParser.CreateContextCoalesceItemContext ctx) {
        }
    
        public void ExitCreateContextCoalesceItem(EsperEPL2GrammarParser.CreateContextCoalesceItemContext ctx) {
        }
    
        public void EnterMillisecondPart(EsperEPL2GrammarParser.MillisecondPartContext ctx) {
        }
    
        public void ExitMillisecondPart(EsperEPL2GrammarParser.MillisecondPartContext ctx) {
        }
    
        public void EnterMicrosecondPart(EsperEPL2GrammarParser.MicrosecondPartContext ctx) {
        }
    
        public void ExitMicrosecondPart(EsperEPL2GrammarParser.MicrosecondPartContext ctx) {
        }
    
        public void EnterOnExprFrom(EsperEPL2GrammarParser.OnExprFromContext ctx) {
        }
    
        public void ExitOnExprFrom(EsperEPL2GrammarParser.OnExprFromContext ctx) {
        }
    
        public void EnterNegatedExpression(EsperEPL2GrammarParser.NegatedExpressionContext ctx) {
        }
    
        public void EnterSelectExpr(EsperEPL2GrammarParser.SelectExprContext ctx) {
        }
    
        public void EnterMatchRecogMeasures(EsperEPL2GrammarParser.MatchRecogMeasuresContext ctx) {
        }
    
        public void ExitMatchRecogMeasures(EsperEPL2GrammarParser.MatchRecogMeasuresContext ctx) {
        }
    
        public void EnterAdditiveExpression(EsperEPL2GrammarParser.AdditiveExpressionContext ctx) {
        }
    
        public void EnterEventProperty(EsperEPL2GrammarParser.EventPropertyContext ctx) {
        }
    
        public void EnterJsonarray(EsperEPL2GrammarParser.JsonarrayContext ctx) {
        }
    
        public void ExitJsonarray(EsperEPL2GrammarParser.JsonarrayContext ctx) {
        }
    
        public void EnterJsonobject(EsperEPL2GrammarParser.JsonobjectContext ctx) {
        }
    
        public void EnterOuterJoin(EsperEPL2GrammarParser.OuterJoinContext ctx) {
        }
    
        public void EnterEscapableIdent(EsperEPL2GrammarParser.EscapableIdentContext ctx) {
        }
    
        public void ExitEscapableIdent(EsperEPL2GrammarParser.EscapableIdentContext ctx) {
        }
    
        public void EnterFromClause(EsperEPL2GrammarParser.FromClauseContext ctx) {
        }
    
        public void ExitFromClause(EsperEPL2GrammarParser.FromClauseContext ctx) {
        }
    
        public void EnterOnExpr(EsperEPL2GrammarParser.OnExprContext ctx) {
        }
    
        public void EnterGopParamsItemMany(EsperEPL2GrammarParser.GopParamsItemManyContext ctx) {
        }
    
        public void ExitGopParamsItemMany(EsperEPL2GrammarParser.GopParamsItemManyContext ctx) {
        }
    
        public void EnterPropertySelectionList(EsperEPL2GrammarParser.PropertySelectionListContext ctx) {
        }
    
        public void ExitPropertySelectionList(EsperEPL2GrammarParser.PropertySelectionListContext ctx) {
        }
    
        public void EnterWeekPart(EsperEPL2GrammarParser.WeekPartContext ctx) {
        }
    
        public void ExitWeekPart(EsperEPL2GrammarParser.WeekPartContext ctx) {
        }
    
        public void EnterMatchRecogPatternAlteration(EsperEPL2GrammarParser.MatchRecogPatternAlterationContext ctx) {
        }
    
        public void EnterGopParams(EsperEPL2GrammarParser.GopParamsContext ctx) {
        }
    
        public void ExitGopParams(EsperEPL2GrammarParser.GopParamsContext ctx) {
        }
    
        public void EnterCreateContextChoice(EsperEPL2GrammarParser.CreateContextChoiceContext ctx) {
        }
    
        public void ExitCreateContextChoice(EsperEPL2GrammarParser.CreateContextChoiceContext ctx) {
        }
    
        public void EnterCaseExpression(EsperEPL2GrammarParser.CaseExpressionContext ctx) {
        }
    
        public void EnterCreateIndexColumn(EsperEPL2GrammarParser.CreateIndexColumnContext ctx) {
        }
    
        public void ExitCreateIndexColumn(EsperEPL2GrammarParser.CreateIndexColumnContext ctx) {
        }
    
        public void EnterExpressionWithTimeList(EsperEPL2GrammarParser.ExpressionWithTimeListContext ctx) {
        }
    
        public void ExitExpressionWithTimeList(EsperEPL2GrammarParser.ExpressionWithTimeListContext ctx) {
        }
    
        public void EnterGopParamsItemAs(EsperEPL2GrammarParser.GopParamsItemAsContext ctx) {
        }
    
        public void ExitGopParamsItemAs(EsperEPL2GrammarParser.GopParamsItemAsContext ctx) {
        }
    
        public void EnterRowLimit(EsperEPL2GrammarParser.RowLimitContext ctx) {
        }
    
        public void EnterCreateSchemaQual(EsperEPL2GrammarParser.CreateSchemaQualContext ctx) {
        }
    
        public void ExitCreateSchemaQual(EsperEPL2GrammarParser.CreateSchemaQualContext ctx) {
        }
    
        public void EnterMatchUntilRange(EsperEPL2GrammarParser.MatchUntilRangeContext ctx) {
        }
    
        public void ExitMatchUntilRange(EsperEPL2GrammarParser.MatchUntilRangeContext ctx) {
        }
    
        public void EnterMatchRecogDefine(EsperEPL2GrammarParser.MatchRecogDefineContext ctx) {
        }
    
        public void ExitMatchRecogDefine(EsperEPL2GrammarParser.MatchRecogDefineContext ctx) {
        }
    
        public void EnterOrderByListElement(EsperEPL2GrammarParser.OrderByListElementContext ctx) {
        }
    
        public void EnterMinutePart(EsperEPL2GrammarParser.MinutePartContext ctx) {
        }
    
        public void ExitMinutePart(EsperEPL2GrammarParser.MinutePartContext ctx) {
        }
    
        public void EnterMergeUnmatched(EsperEPL2GrammarParser.MergeUnmatchedContext ctx) {
        }
    
        public void EnterMethodJoinExpression(EsperEPL2GrammarParser.MethodJoinExpressionContext ctx) {
        }
    
        public void ExitMethodJoinExpression(EsperEPL2GrammarParser.MethodJoinExpressionContext ctx) {
        }
    
        public void EnterExistsSubSelectExpression(EsperEPL2GrammarParser.ExistsSubSelectExpressionContext ctx) {
        }
    
        public void EnterCreateContextRangePoint(EsperEPL2GrammarParser.CreateContextRangePointContext ctx) {
        }
    
        public void ExitCreateContextRangePoint(EsperEPL2GrammarParser.CreateContextRangePointContext ctx) {
        }
    
        public void EnterLibFunctionArgItem(EsperEPL2GrammarParser.LibFunctionArgItemContext ctx) {
        }
    
        public void ExitLibFunctionArgItem(EsperEPL2GrammarParser.LibFunctionArgItemContext ctx) {
        }
    
        public void EnterRegularJoin(EsperEPL2GrammarParser.RegularJoinContext ctx) {
        }
    
        public void ExitRegularJoin(EsperEPL2GrammarParser.RegularJoinContext ctx) {
        }
    
        public void EnterUpdateDetails(EsperEPL2GrammarParser.UpdateDetailsContext ctx) {
        }
    
        public void ExitUpdateDetails(EsperEPL2GrammarParser.UpdateDetailsContext ctx) {
        }
    
        public void EnterArrayExpression(EsperEPL2GrammarParser.ArrayExpressionContext ctx) {
        }
    
        public void VisitErrorNode(IErrorNode errorNode) {
        }
    
        public void EnterEveryRule(ParserRuleContext parserRuleContext) {
        }
    
        public void ExitEveryRule(ParserRuleContext parserRuleContext) {
        }
    
        public void EnterAndExpression(EsperEPL2GrammarParser.AndExpressionContext ctx) {
        }
    
        public void EnterFollowedByRepeat(EsperEPL2GrammarParser.FollowedByRepeatContext ctx) {
        }
    
        public void ExitFollowedByRepeat(EsperEPL2GrammarParser.FollowedByRepeatContext ctx) {
        }
    
        public void EnterFollowedByExpression(EsperEPL2GrammarParser.FollowedByExpressionContext ctx) {
        }
    
        public void EnterOrExpression(EsperEPL2GrammarParser.OrExpressionContext ctx) {
        }
    
        public void EnterQualifyExpression(EsperEPL2GrammarParser.QualifyExpressionContext ctx) {
        }
    
        public void EnterMatchUntilExpression(EsperEPL2GrammarParser.MatchUntilExpressionContext ctx) {
        }
    
        public void EnterGuardPostFix(EsperEPL2GrammarParser.GuardPostFixContext ctx) {
        }
    
        public void EnterBuiltin_coalesce(EsperEPL2GrammarParser.Builtin_coalesceContext ctx) {
        }
    
        public void EnterBuiltin_typeof(EsperEPL2GrammarParser.Builtin_typeofContext ctx) {
        }
    
        public void EnterBuiltin_avedev(EsperEPL2GrammarParser.Builtin_avedevContext ctx) {
        }
    
        public void EnterBuiltin_prevcount(EsperEPL2GrammarParser.Builtin_prevcountContext ctx) {
        }
    
        public void EnterBuiltin_stddev(EsperEPL2GrammarParser.Builtin_stddevContext ctx) {
        }
    
        public void EnterBuiltin_sum(EsperEPL2GrammarParser.Builtin_sumContext ctx) {
        }
    
        public void EnterBuiltin_exists(EsperEPL2GrammarParser.Builtin_existsContext ctx) {
        }
    
        public void EnterBuiltin_prior(EsperEPL2GrammarParser.Builtin_priorContext ctx) {
        }
    
        public void EnterBuiltin_instanceof(EsperEPL2GrammarParser.Builtin_instanceofContext ctx) {
        }
    
        public void EnterBuiltin_currts(EsperEPL2GrammarParser.Builtin_currtsContext ctx) {
        }
    
        public void EnterBuiltin_median(EsperEPL2GrammarParser.Builtin_medianContext ctx) {
        }
    
        public void EnterFuncIdentChained(EsperEPL2GrammarParser.FuncIdentChainedContext ctx) {
        }
    
        public void ExitFuncIdentChained(EsperEPL2GrammarParser.FuncIdentChainedContext ctx) {
        }
    
        public void EnterFuncIdentTop(EsperEPL2GrammarParser.FuncIdentTopContext ctx) {
        }
    
        public void ExitFuncIdentTop(EsperEPL2GrammarParser.FuncIdentTopContext ctx) {
        }
    
        public void EnterBuiltin_avg(EsperEPL2GrammarParser.Builtin_avgContext ctx) {
        }
    
        public void EnterBuiltin_cast(EsperEPL2GrammarParser.Builtin_castContext ctx) {
        }
    
        public void EnterBuiltin_cnt(EsperEPL2GrammarParser.Builtin_cntContext ctx) {
        }
    
        public void EnterBuiltin_prev(EsperEPL2GrammarParser.Builtin_prevContext ctx) {
        }
    
        public void EnterBuiltin_istream(EsperEPL2GrammarParser.Builtin_istreamContext ctx) {
        }
    
        public void EnterBuiltin_prevwindow(EsperEPL2GrammarParser.Builtin_prevwindowContext ctx) {
        }
    
        public void EnterBuiltin_prevtail(EsperEPL2GrammarParser.Builtin_prevtailContext ctx) {
        }
    
        public void EnterFafInsert(EsperEPL2GrammarParser.FafInsertContext ctx) {
        }
    
        public void EnterGroupByListChoice(EsperEPL2GrammarParser.GroupByListChoiceContext ctx) {
        }
    
        public void ExitGroupByListChoice(EsperEPL2GrammarParser.GroupByListChoiceContext ctx) {
        }
    
        public void EnterGroupBySetsChoice(EsperEPL2GrammarParser.GroupBySetsChoiceContext ctx) {
        }
    
        public void ExitGroupBySetsChoice(EsperEPL2GrammarParser.GroupBySetsChoiceContext ctx) {
        }
    
        public void ExitSelectExpr(EsperEPL2GrammarParser.SelectExprContext ctx) {
        }
    
        public void EnterGroupByCubeOrRollup(EsperEPL2GrammarParser.GroupByCubeOrRollupContext ctx) {
        }
    
        public void ExitGroupByCubeOrRollup(EsperEPL2GrammarParser.GroupByCubeOrRollupContext ctx) {
        }
    
        public void EnterGroupByGroupingSets(EsperEPL2GrammarParser.GroupByGroupingSetsContext ctx) {
        }
    
        public void ExitGroupByGroupingSets(EsperEPL2GrammarParser.GroupByGroupingSetsContext ctx) {
        }
    
        public void EnterGroupByCombinableExpr(EsperEPL2GrammarParser.GroupByCombinableExprContext ctx) {
        }
    
        public void ExitGroupByCombinableExpr(EsperEPL2GrammarParser.GroupByCombinableExprContext ctx) {
        }
    
        public void EnterBuiltin_grouping(EsperEPL2GrammarParser.Builtin_groupingContext ctx) {
        }
    
        public void EnterBuiltin_groupingid(EsperEPL2GrammarParser.Builtin_groupingidContext ctx) {
        }
    
        public void EnterFuncIdentInner(EsperEPL2GrammarParser.FuncIdentInnerContext ctx) {
        }
    
        public void ExitFuncIdentInner(EsperEPL2GrammarParser.FuncIdentInnerContext ctx) {
        }
    
        public void EnterCreateTableColumnPlain(EsperEPL2GrammarParser.CreateTableColumnPlainContext ctx) {
        }
    
        public void ExitCreateTableColumnPlain(EsperEPL2GrammarParser.CreateTableColumnPlainContext ctx) {
        }
    
        public void EnterCreateTableExpr(EsperEPL2GrammarParser.CreateTableExprContext ctx) {
        }
    
        public void EnterCreateTableColumn(EsperEPL2GrammarParser.CreateTableColumnContext ctx) {
        }
    
        public void ExitCreateTableColumn(EsperEPL2GrammarParser.CreateTableColumnContext ctx) {
        }
    
        public void EnterCreateTableColumnList(EsperEPL2GrammarParser.CreateTableColumnListContext ctx) {
        }
    
        public void ExitCreateTableColumnList(EsperEPL2GrammarParser.CreateTableColumnListContext ctx) {
        }
    
        public void EnterIntoTableExpr(EsperEPL2GrammarParser.IntoTableExprContext ctx) {
        }
    
        public void EnterSubstitutionCanChain(EsperEPL2GrammarParser.SubstitutionCanChainContext ctx) {
        }
    
        public void EnterSlashIdentifier(EsperEPL2GrammarParser.SlashIdentifierContext ctx) {
        }
    
        public void ExitSlashIdentifier(EsperEPL2GrammarParser.SlashIdentifierContext ctx) {
        }
    
        public void EnterMatchRecogPatternRepeat(EsperEPL2GrammarParser.MatchRecogPatternRepeatContext ctx) {
        }
    
        public void ExitMatchRecogPatternRepeat(EsperEPL2GrammarParser.MatchRecogPatternRepeatContext ctx) {
        }
    
        public void EnterMatchRecogPatternPermute(EsperEPL2GrammarParser.MatchRecogPatternPermuteContext ctx) {
        }
    
        public void EnterExpressionListWithNamed(EsperEPL2GrammarParser.ExpressionListWithNamedContext ctx) {
        }
    
        public void ExitExpressionListWithNamed(EsperEPL2GrammarParser.ExpressionListWithNamedContext ctx) {
        }
    
        public void EnterExpressionNamedParameter(EsperEPL2GrammarParser.ExpressionNamedParameterContext ctx) {
        }
    
        public void EnterExpressionWithNamed(EsperEPL2GrammarParser.ExpressionWithNamedContext ctx) {
        }
    
        public void ExitExpressionWithNamed(EsperEPL2GrammarParser.ExpressionWithNamedContext ctx) {
        }
    
        public void EnterBuiltin_firstlastwindow(EsperEPL2GrammarParser.Builtin_firstlastwindowContext ctx) {
        }
    
        public void EnterFirstLastWindowAggregation(EsperEPL2GrammarParser.FirstLastWindowAggregationContext ctx) {
        }
    
        public void ExitFirstLastWindowAggregation(EsperEPL2GrammarParser.FirstLastWindowAggregationContext ctx) {
        }
    
        public void EnterExpressionWithNamedWithTime(EsperEPL2GrammarParser.ExpressionWithNamedWithTimeContext ctx) {
        }
    
        public void ExitExpressionWithNamedWithTime(EsperEPL2GrammarParser.ExpressionWithNamedWithTimeContext ctx) {
        }
    
        public void EnterExpressionNamedParameterWithTime(EsperEPL2GrammarParser.ExpressionNamedParameterWithTimeContext ctx) {
        }
    
        public void EnterExpressionListWithNamedWithTime(EsperEPL2GrammarParser.ExpressionListWithNamedWithTimeContext ctx) {
        }
    
        public void ExitExpressionListWithNamedWithTime(EsperEPL2GrammarParser.ExpressionListWithNamedWithTimeContext ctx) {
        }
    
        public void EnterViewExpressions(EsperEPL2GrammarParser.ViewExpressionsContext ctx) {
        }
    
        public void ExitViewExpressions(EsperEPL2GrammarParser.ViewExpressionsContext ctx) {
        }
    
        public void EnterViewWParameters(EsperEPL2GrammarParser.ViewWParametersContext ctx) {
        }
    
        public void EnterViewExpressionWNamespace(EsperEPL2GrammarParser.ViewExpressionWNamespaceContext ctx) {
        }
    
        public void ExitViewWParameters(EsperEPL2GrammarParser.ViewWParametersContext ctx) {
        }
    
        public void EnterViewExpressionOptNamespace(EsperEPL2GrammarParser.ViewExpressionOptNamespaceContext ctx) {
        }
    
        public void EnterOnSelectInsertFromClause(EsperEPL2GrammarParser.OnSelectInsertFromClauseContext ctx) {
        }
    
        public void EnterExpressionTypeAnno(EsperEPL2GrammarParser.ExpressionTypeAnnoContext ctx) {
        }
    
        public void ExitExpressionTypeAnno(EsperEPL2GrammarParser.ExpressionTypeAnnoContext ctx) {
        }
    
        public void EnterTypeExpressionAnnotation(EsperEPL2GrammarParser.TypeExpressionAnnotationContext ctx) {
        }
    
        public void ExitTypeExpressionAnnotation(EsperEPL2GrammarParser.TypeExpressionAnnotationContext ctx) {
        }
    }
} // end of namespace
