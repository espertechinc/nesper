///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

using XLR8.CGLib;

namespace com.espertech.esper.epl.expression.core
{
    public static class ExprNodeUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public static readonly ExprNode[] EMPTY_EXPR_ARRAY = new ExprNode[0];
        public static readonly ExprDeclaredNode[] EMPTY_DECLARED_ARR = new ExprDeclaredNode[0];
        public static readonly ExpressionScriptProvided[] EMPTY_SCRIPTS = new ExpressionScriptProvided[0];
    
        public static bool DeepEqualsIsSubset(IList<ExprNode> subset, ExprNode[] superset) {
            foreach (var subsetNode in subset) {
                var found = false;
                foreach (var supersetNode in superset) {
                    if (DeepEquals(subsetNode, supersetNode, false)) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    return false;
                }
            }
            return true;
        }
    
        public static bool DeepEqualsIgnoreDupAndOrder(IList<ExprNode> setOne, IList<ExprNode> setTwo) {
            if ((setOne.Count == 0 && setTwo.Count != 0) || (setOne.Count != 0 && setTwo.Count == 0))
            {
                return false;
            }
    
            // find set-one expressions in set two
            var foundTwo = new bool[setTwo.Count];
            foreach (var one in setOne) {
                var found = false;
                for (var i = 0; i < setTwo.Count; i++)
                {
                    if (DeepEquals(one, setTwo[i], false)) {
                        found = true;
                        foundTwo[i] = true;
                    }
                }
                if (!found) {
                    return false;
                }
            }
    
            // find any remaining set-two expressions in set one
            for (var i = 0; i < foundTwo.Length; i++) {
                if (foundTwo[i]) {
                    continue;
                }
                foreach (var one in setOne) {
                    if (DeepEquals(one, setTwo[i], false)) {
                        break;
                    }
                }
                return false;
            }
            return true;
        }

        public static IDictionary<ExprDeclaredNode, IList<ExprDeclaredNode>> GetDeclaredExpressionCallHierarchy(
            ExprDeclaredNode[] declaredExpressions)
        {
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            var calledToCallerMap = new Dictionary<ExprDeclaredNode, IList<ExprDeclaredNode>>();
            foreach (var node in declaredExpressions)
            {
                visitor.Reset();
                node.Accept(visitor);
                foreach (var called in visitor.DeclaredExpressions)
                {
                    if (called == node)
                    {
                        continue;
                    }
                    var callers = calledToCallerMap.Get(called);
                    if (callers == null)
                    {
                        callers = new List<ExprDeclaredNode>(2);
                        calledToCallerMap.Put(called, callers);
                    }
                    callers.Add(node);
                }
                if (!calledToCallerMap.ContainsKey(node))
                {
                    calledToCallerMap.Put(node, Collections.GetEmptyList<ExprDeclaredNode>());
                }
            }
            return calledToCallerMap;
        }

        public static TextWriter ToExpressionStringMinPrecedence(this ExprNode node, TextWriter writer)
        {
            node.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            return writer;
        }

        public static String ToExpressionStringMinPrecedenceSafe(this ExprNode node)
        {
            try
            {
                return ToExpressionStringMinPrecedence(node, new StringWriter()).ToString(); ;
            }
            catch(Exception ex)
            {
                Log.Debug("Failed to render expression text: " + ex.Message, ex);
                return string.Empty;
            }
        }

        public static String[] ToExpressionStringMinPrecedenceAsArray(IList<ExprNode> nodes)
        {
            return nodes
                .Select(node => ToExpressionStringMinPrecedence(node, new StringWriter()).ToString())
                .ToArray();
        }

        public static String ToExpressionStringMinPrecedenceAsList(IList<ExprNode> nodes)
        {
            return ToExpressionStringMinPrecedenceAsList(nodes, new StringWriter()).ToString();
        }

        public static TextWriter ToExpressionStringMinPrecedenceAsList(IList<ExprNode> nodes, TextWriter writer)
        {
            String delimiter = "";
            foreach (ExprNode node in nodes)
            {
                writer.Write(delimiter);
                node.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }

            return writer;
        }

        public static Pair<string, ExprNode> CheckGetAssignmentToProp(ExprNode node)
        {
            if (!(node is ExprEqualsNode))
            {
                return null;
            }
            var equals = (ExprEqualsNode) node;
            if (!(equals.ChildNodes[0] is ExprIdentNode))
            {
                return null;
            }
            var identNode = (ExprIdentNode) equals.ChildNodes[0];
            return new Pair<string, ExprNode>(identNode.FullUnresolvedName, equals.ChildNodes[1]);
        }

        public static Pair<string, ExprNode> CheckGetAssignmentToVariableOrProp(ExprNode node)
        {
            var prop = CheckGetAssignmentToProp(node);
            if (prop != null)
            {
                return prop;
            }
            if (!(node is ExprEqualsNode))
            {
                return null;
            }
            var equals = (ExprEqualsNode) node;

            if (equals.ChildNodes[0] is ExprVariableNode)
            {
                var variableNode = (ExprVariableNode) equals.ChildNodes[0];
                return new Pair<string, ExprNode>(variableNode.VariableNameWithSubProp, equals.ChildNodes[1]);
            }
            if (equals.ChildNodes[0] is ExprTableAccessNode)
            {
                throw new ExprValidationException(
                    "Table access expression not allowed on the left hand side, please remove the table prefix");
            }
            return null;
        }

        public static void ApplyFilterExpressionsIterable(
            IEnumerable<EventBean> iterable,
            IList<ExprNode> filterExpressions,
            ExprEvaluatorContext exprEvaluatorContext,
            ICollection<EventBean> eventsInWindow)
        {
            ExprEvaluator[] evaluators = GetEvaluators(filterExpressions);
            var events = new EventBean[1];
            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);
            foreach (var theEvent in iterable)
            {
                events[0] = theEvent;
                var add = true;
                foreach (var filter in evaluators)
                {
                    var result = filter.Evaluate(evaluateParams);
                    if ((result == null) || (false.Equals(result)))
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                {
                    eventsInWindow.Add(events[0]);
                }
            }
        }

        public static void ApplyFilterExpressionIterable(
            IEnumerator<EventBean> enumerator,
            ExprEvaluator filterExpression,
            ExprEvaluatorContext exprEvaluatorContext,
            ICollection<EventBean> eventsInWindow)
        {
            var events = new EventBean[1];
            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);
            while (enumerator.MoveNext())
            {
                events[0] = enumerator.Current;
                var result = filterExpression.Evaluate(evaluateParams);
                if ((result == null) || (false.Equals(result)))
                {
                    continue;
                }
                eventsInWindow.Add(events[0]);
            }
        }

        public static ExprNode ConnectExpressionsByLogicalAnd(IList<ExprNode> nodes, ExprNode optionalAdditionalFilter)
        {
            if (nodes.IsEmpty())
            {
                return optionalAdditionalFilter;
            }
            if (optionalAdditionalFilter == null)
            {
                if (nodes.Count == 1)
                {
                    return nodes[0];
                }
                return ConnectExpressionsByLogicalAnd(nodes);
            }
            if (nodes.Count == 1)
            {
                return ConnectExpressionsByLogicalAnd(Collections.List(nodes[0], optionalAdditionalFilter));
            }
            var andNode = ConnectExpressionsByLogicalAnd(nodes);
            andNode.AddChildNode(optionalAdditionalFilter);
            return andNode;
        }

        public static ExprAndNode ConnectExpressionsByLogicalAnd(ICollection<ExprNode> nodes)
        {
            if (nodes.Count < 2)
            {
                throw new ArgumentException("Invalid empty or 1-element list of nodes");
            }
            var andNode = new ExprAndNodeImpl();
            foreach (var node in nodes)
            {
                andNode.AddChildNode(node);
            }
            return andNode;
        }

        public static ExprNode ConnectExpressionsByLogicalAndWhenNeeded(ICollection<ExprNode> nodes)
        {
            if (nodes.IsEmpty())
            {
                return null;
            }
            if (nodes.Count == 1)
            {
                return nodes.First();
            }
            return ConnectExpressionsByLogicalAnd(nodes);
        }

        /// <summary>
        /// Walk expression returning properties used.
        /// </summary>
        /// <param name="exprNode">to walk</param>
        /// <param name="visitAggregateNodes">true to visit aggregation nodes</param>
        /// <returns>list of props</returns>
        public static IList<Pair<int, string>> GetExpressionProperties(ExprNode exprNode, bool visitAggregateNodes)
        {
            var visitor = new ExprNodeIdentifierVisitor(visitAggregateNodes);
            exprNode.Accept(visitor);
            return visitor.ExprProperties;
        }

        public static bool IsConstantValueExpr(ExprNode exprNode)
        {
            var constantNode = exprNode as ExprConstantNode;
            if (constantNode == null)
            {
                return false;
            }

            return constantNode.IsConstantValue;
        }

        /// <summary>
        /// Validates the expression node subtree that has this
        /// node as root. Some of the nodes of the tree, including the
        /// root, might be replaced in the process.
        /// </summary>
        /// <param name="origin">validate origin</param>
        /// <param name="exprNode">node</param>
        /// <param name="validationContext">context</param>
        /// <exception cref="ExprValidationException">when the validation fails</exception>
        /// <returns>
        /// the root node of the validated subtree, possibly
        /// different than the root node of the unvalidated subtree
        /// </returns>
        public static ExprNode GetValidatedSubtree(
            ExprNodeOrigin origin,
            ExprNode exprNode,
            ExprValidationContext validationContext)
        {
            if (exprNode is ExprLambdaGoesNode)
            {
                return exprNode;
            }

            try
            {
                return GetValidatedSubtreeInternal(exprNode, validationContext, true);
            }
            catch (ExprValidationException ex)
            {
                try
                {
                    string text;
                    if (exprNode is ExprSubselectNode)
                    {
                        var subselect = (ExprSubselectNode) exprNode;
                        text = EPStatementStartMethodHelperSubselect.GetSubqueryInfoText(
                            subselect.SubselectNumber - 1, subselect);
                    }
                    else
                    {
                        text = ToExpressionStringMinPrecedenceSafe(exprNode);
                        if (text.Length > 40)
                        {
                            var shortened = text.Substring(0, 35);
                            text = shortened + "...(" + text.Length + " chars)";
                        }
                        text = "'" + text + "'";
                    }
                    throw new ExprValidationException(
                        "Failed to validate " +
                        origin.GetClauseName() +
                        " expression " +
                        text + ": " +
                        ex.Message, ex);
                }
                catch (ExprValidationException)
                {
                    throw;
                }
                catch (Exception rtex)
                {
                    Log.Debug("Failed to render nice validation message text: " + rtex.Message, rtex);
                }

                throw;
            }
        }

        public static void GetValidatedSubtree(
            ExprNodeOrigin origin,
            IList<ExprNode> exprNode,
            ExprValidationContext validationContext)
        {
            if (exprNode == null)
            {
                return;
            }
            for (var i = 0; i < exprNode.Count; i++)
            {
                exprNode[i] = GetValidatedSubtree(origin, exprNode[i], validationContext);
            }
        }

        public static void GetValidatedSubtree(
            ExprNodeOrigin origin,
            ExprNode[][] exprNode,
            ExprValidationContext validationContext)
        {
            if (exprNode == null)
            {
                return;
            }
            foreach (var anExprNode in exprNode)
            {
                GetValidatedSubtree(origin, anExprNode, validationContext);
            }
        }

        public static ExprNode GetValidatedAssignment(OnTriggerSetAssignment assignment, ExprValidationContext validationContext) {
            var strictAssignment = CheckGetAssignmentToVariableOrProp(assignment.Expression);
            if (strictAssignment != null) {
                var validatedRightSide = GetValidatedSubtreeInternal(strictAssignment.Second, validationContext, true);
                assignment.Expression.SetChildNode(1, validatedRightSide);
                return assignment.Expression;
            } else {
                return GetValidatedSubtreeInternal(assignment.Expression, validationContext, true);
            }
        }

        private static ExprNode GetValidatedSubtreeInternal(
            ExprNode exprNode,
            ExprValidationContext validationContext,
            bool isTopLevel)
        {
            var result = exprNode;
            if (exprNode is ExprLambdaGoesNode)
            {
                return exprNode;
            }

            for (var i = 0; i < exprNode.ChildNodes.Count; i++)
            {
                var childNode = exprNode.ChildNodes[i];
                if (childNode is ExprDeclaredOrLambdaNode)
                {
                    var node = (ExprDeclaredOrLambdaNode) childNode;
                    if (node.IsValidated)
                    {
                        continue;
                    }
                }
                var childNodeValidated = GetValidatedSubtreeInternal(childNode, validationContext, false);
                exprNode.SetChildNode(i, childNodeValidated);
            }

            try
            {
                var optionalReplacement = exprNode.Validate(validationContext);
                if (optionalReplacement != null)
                {
                    return GetValidatedSubtreeInternal(optionalReplacement, validationContext, isTopLevel);
                }
            }
            catch (ExprValidationException e)
            {
                if (exprNode is ExprIdentNode)
                {
                    var identNode = (ExprIdentNode) exprNode;
                    try
                    {
                        result = ResolveStaticMethodOrField(identNode, e, validationContext);
                    }
                    catch (ExprValidationException ex)
                    {
                        e = ex;
                        result = ResolveAsStreamName(identNode, e, validationContext);
                    }
                }
                else
                {
                    throw;
                }
            }

            // For top-level expressions check if we perform audit
            if (isTopLevel)
            {
                if (validationContext.IsExpressionAudit)
                {
                    return
                        (ExprNode)
                            ExprNodeProxy.NewInstance(
                                validationContext.StreamTypeService.EngineURIQualifier, validationContext.StatementName,
                                result);
                }
            }
            else
            {
                if (validationContext.IsExpressionNestedAudit && !(result is ExprIdentNode) && !(IsConstantValueExpr(result)))
                {
                    return
                        (ExprNode)
                            ExprNodeProxy.NewInstance(
                                validationContext.StreamTypeService.EngineURIQualifier, validationContext.StatementName,
                                result);
                }
            }

            return result;
        }

        private static ExprNode ResolveAsStreamName(
            ExprIdentNode identNode,
            ExprValidationException existingException,
            ExprValidationContext validationContext)
        {
            var exprStream = new ExprStreamUnderlyingNodeImpl(identNode.UnresolvedPropertyName, false);

            try
            {
                exprStream.Validate(validationContext);
            }
            catch (ExprValidationException)
            {
                throw existingException;
            }

            return exprStream;
        }

        // Since static method calls such as "Type.Method('a')" and mapped properties "Stream.Property('key')"
        // look the same, however as the validation could not resolve "Stream.Property('key')" before calling this method,
        // this method tries to resolve the mapped property as a static method.
        // Assumes that this is an ExprIdentNode.
        private static ExprNode ResolveStaticMethodOrField(
            ExprIdentNode identNode,
            ExprValidationException propertyException,
            ExprValidationContext validationContext)
        {
            // Reconstruct the original string
            var mappedProperty = new StringBuilder(identNode.UnresolvedPropertyName);
            if (identNode.StreamOrPropertyName != null)
            {
                mappedProperty.Insert(0, identNode.StreamOrPropertyName + '.');
            }

            // Parse the mapped property format into a class name, method and single string parameter
            var parse = ParseMappedProperty(mappedProperty.ToString());
            if (parse == null)
            {
                var constNode = ResolveIdentAsEnumConst(
                    mappedProperty.ToString(), validationContext.EngineImportService);
                if (constNode == null)
                {
                    throw propertyException;
                }
                else
                {
                    return constNode;
                }
            }

            // If there is a class name, assume a static method is possible.
            if (parse.ClassName != null)
            {
                var parameters = Collections.SingletonList((ExprNode) new ExprConstantNodeImpl(parse.ArgString));
                var chain = new List<ExprChainedSpec>();
                chain.Add(new ExprChainedSpec(parse.ClassName, Collections.GetEmptyList<ExprNode>(), false));
                chain.Add(new ExprChainedSpec(parse.MethodName, parameters, false));
                var result = new ExprDotNodeImpl(
                    chain, validationContext.EngineImportService.IsDuckType,
                    validationContext.EngineImportService.IsUdfCache);

                // Validate
                try
                {
                    result.Validate(validationContext);
                }
                catch (ExprValidationException e)
                {
                    throw new ExprValidationException(
                        "Failed to resolve enumeration method, date-time method or mapped property '" + mappedProperty +
                        "': " + e.Message);
                }

                return result;
            }

            // There is no class name, try a single-row function
            var functionName = parse.MethodName;
            try
            {
                var classMethodPair = validationContext.EngineImportService.ResolveSingleRow(functionName);
                IList<ExprNode> parameters =
                    Collections.SingletonList((ExprNode) new ExprConstantNodeImpl(parse.ArgString));
                IList<ExprChainedSpec> chain =
                    Collections.SingletonList(new ExprChainedSpec(classMethodPair.Second.MethodName, parameters, false));
                var result = new ExprPlugInSingleRowNode(
                    functionName, classMethodPair.First, chain, classMethodPair.Second);

                // Validate
                try
                {
                    result.Validate(validationContext);
                }
                catch (Exception e)
                {
                    throw new ExprValidationException(
                        "Plug-in aggregation function '" + parse.MethodName + "' failed validation: " + e.Message);
                }

                return result;
            }
            catch (EngineImportUndefinedException)
            {
                // Not an single-row function
            }
            catch (EngineImportException e)
            {
                throw new IllegalStateException("Error resolving single-row function: " + e.Message, e);
            }

            // Try an aggregation function factory
            try
            {
                var aggregationFactory =
                    validationContext.EngineImportService.ResolveAggregationFactory(parse.MethodName);
                var result = new ExprPlugInAggNode(false, aggregationFactory, parse.MethodName);
                result.AddChildNode(new ExprConstantNodeImpl(parse.ArgString));

                // Validate
                try
                {
                    result.Validate(validationContext);
                }
                catch (Exception e)
                {
                    throw new ExprValidationException(
                        "Plug-in aggregation function '" + parse.MethodName + "' failed validation: " + e.Message);
                }

                return result;
            }
            catch (EngineImportUndefinedException)
            {
                // Not an aggregation function
            }
            catch (EngineImportException e)
            {
                throw new IllegalStateException("Error resolving aggregation: " + e.Message, e);
            }

            // absolutely cannot be resolved
            throw propertyException;
        }

        private static ExprConstantNode ResolveIdentAsEnumConst(string constant, EngineImportService engineImportService)
        {
            var enumValue = TypeHelper.ResolveIdentAsEnumConst(constant, engineImportService, false);
            if (enumValue != null) {
                return new ExprConstantNodeImpl(enumValue);
            }
            return null;
        }
    
        /// <summary>
        /// Parse the mapped property into classname, method and string argument.
        /// Mind this has been parsed already and is a valid mapped property.
        /// </summary>
        /// <param name="property">is the string property to be passed as a static method invocation</param>
        /// <returns>descriptor object</returns>
        public static MappedPropertyParseResult ParseMappedProperty(string property) {
            // get argument
            var indexFirstDoubleQuote = property.IndexOf('"');
            var indexFirstSingleQuote = property.IndexOf('\'');
            int startArg;
            if ((indexFirstSingleQuote == -1) && (indexFirstDoubleQuote == -1)) {
                return null;
            }
            if ((indexFirstSingleQuote != -1) && (indexFirstDoubleQuote != -1)) {
                if (indexFirstSingleQuote < indexFirstDoubleQuote) {
                    startArg = indexFirstSingleQuote;
                } else {
                    startArg = indexFirstDoubleQuote;
                }
            } else if (indexFirstSingleQuote != -1) {
                startArg = indexFirstSingleQuote;
            } else {
                startArg = indexFirstDoubleQuote;
            }
    
            var indexLastDoubleQuote = property.LastIndexOf('"');
            var indexLastSingleQuote = property.LastIndexOf('\'');
            int endArg;
            if ((indexLastSingleQuote == -1) && (indexLastDoubleQuote == -1)) {
                return null;
            }
            if ((indexLastSingleQuote != -1) && (indexLastDoubleQuote != -1)) {
                if (indexLastSingleQuote > indexLastDoubleQuote) {
                    endArg = indexLastSingleQuote;
                } else {
                    endArg = indexLastDoubleQuote;
                }
            } else if (indexLastSingleQuote != -1) {
                if (indexLastSingleQuote == indexFirstSingleQuote) {
                    return null;
                }
                endArg = indexLastSingleQuote;
            } else {
                if (indexLastDoubleQuote == indexFirstDoubleQuote) {
                    return null;
                }
                endArg = indexLastDoubleQuote;
            }
            var argument = property.Between(startArg + 1, endArg);
    
            // get method
            var splitDots = property.RegexSplit("[\\.]");
            if (splitDots.Length == 0) {
                return null;
            }
    
            // find which element represents the method, its the element with the parenthesis
            var indexMethod = -1;
            for (var i = 0; i < splitDots.Length; i++) {
                if (splitDots[i].Contains('(')) {
                    indexMethod = i;
                    break;
                }
            }
            if (indexMethod == -1) {
                return null;
            }
    
            var method = splitDots[indexMethod];
            var indexParan = method.IndexOf('(');
            method = method.Substring(0, indexParan);
            if (method.Length == 0) {
                return null;
            }
    
            if (splitDots.Length == 1) {
                // no class name
                return new MappedPropertyParseResult(null, method, argument);
            }
    
    
            // get class
            var clazz = new StringBuilder();
            for (var i = 0; i < indexMethod; i++) {
                if (i > 0) {
                    clazz.Append('.');
                }
                clazz.Append(splitDots[i]);
            }
    
            return new MappedPropertyParseResult(clazz.ToString(), method, argument);
        }

        public static bool IsAllConstants(IEnumerable<ExprNode> parameters)
        {
            return parameters.All(node => node.IsConstantResult);
        }

        public static ExprIdentNode GetExprIdentNode(EventType[] typesPerStream, int streamId, string property)
        {
            return new ExprIdentNodeImpl(typesPerStream[streamId], property, streamId);
        }

        public static Type[] GetExprResultTypes(ExprEvaluator[] evaluators) {
            var returnTypes = new Type[evaluators.Length];
            for (var i = 0; i < evaluators.Length; i++) {
                returnTypes[i] = evaluators[i].ReturnType;
            }
            return returnTypes;
        }
    
        public static Type[] GetExprResultTypes(IList<ExprNode> expressions) {
            var returnTypes = new Type[expressions.Count];
            for (var i = 0; i < expressions.Count; i++) {
                returnTypes[i] = expressions[i].ExprEvaluator.ReturnType;
            }
            return returnTypes;
        }

        public static ExprNodeUtilMethodDesc ResolveMethodAllowWildcardAndStream(
            string className,
            Type optionalClass,
            string methodName,
            IList<ExprNode> parameters,
            EngineImportService engineImportService,
            EventAdapterService eventAdapterService,
            int statementId,
            bool allowWildcard,
            EventType wildcardType,
            ExprNodeUtilResolveExceptionHandler exceptionHandler,
            string functionName,
            TableService tableService,
            string engineURI)
        {
            var paramTypes = new Type[parameters.Count];
            var childEvals = new ExprEvaluator[parameters.Count];
            var count = 0;
            var allowEventBeanType = new bool[parameters.Count];
            var allowEventBeanCollType = new bool[parameters.Count];
            var childEvalsEventBeanReturnTypes = new ExprEvaluator[parameters.Count];
            var allConstants = true;
            foreach (var childNode in parameters) {
                if (!methodName.IsEnumerationMethod() && childNode is ExprLambdaGoesNode) {
                    throw new ExprValidationException("Unexpected lambda-expression encountered as parameter to UDF or static method '" + methodName + "'");
                }
                if (childNode is ExprWildcard) {
                    if (wildcardType == null || !allowWildcard) {
                        throw new ExprValidationException("Failed to resolve wildcard parameter to a given event type");
                    }
                    childEvals[count] = new ExprNodeUtilExprEvalStreamNumUnd(0, wildcardType.UnderlyingType);
                    childEvalsEventBeanReturnTypes[count] = new ExprNodeUtilExprEvalStreamNumEvent(0);
                    paramTypes[count] = wildcardType.UnderlyingType;
                    allowEventBeanType[count] = true;
                    allConstants = false;
                    count++;
                    continue;
                }
                if (childNode is ExprStreamUnderlyingNode) {
                    var und = (ExprStreamUnderlyingNode) childNode;
                    var tableMetadata = tableService.GetTableMetadataFromEventType(und.EventType);
                    if (tableMetadata == null) {
                        childEvals[count] = childNode.ExprEvaluator;
                        childEvalsEventBeanReturnTypes[count] = new ExprNodeUtilExprEvalStreamNumEvent(und.StreamId);
                    } else {
                        childEvals[count] = new BindProcessorEvaluatorStreamTable(und.StreamId, und.EventType.UnderlyingType, tableMetadata);
                        childEvalsEventBeanReturnTypes[count] = new ExprNodeUtilExprEvalStreamNumEventTable(und.StreamId, tableMetadata);
                    }
                    paramTypes[count] = childEvals[count].ReturnType;
                    allowEventBeanType[count] = true;
                    allConstants = false;
                    count++;
                    continue;
                }
                if (childNode is ExprEvaluatorEnumeration) {
                    var enumeration = (ExprEvaluatorEnumeration) childNode;
                    var eventType = enumeration.GetEventTypeSingle(eventAdapterService, statementId);
                    childEvals[count] = childNode.ExprEvaluator;
                    paramTypes[count] = childEvals[count].ReturnType;
                    allConstants = false;
                    if (eventType != null) {
                        childEvalsEventBeanReturnTypes[count] = new ExprNodeUtilExprEvalStreamNumEnumSingle(enumeration);
                        allowEventBeanType[count] = true;
                        count++;
                        continue;
                    }
                    var eventTypeColl = enumeration.GetEventTypeCollection(eventAdapterService, statementId);
                    if (eventTypeColl != null) {
                        childEvalsEventBeanReturnTypes[count] = new ExprNodeUtilExprEvalStreamNumEnumColl(enumeration);
                        allowEventBeanCollType[count] = true;
                        count++;
                        continue;
                    }
                }
                var eval = childNode.ExprEvaluator;
                childEvals[count] = eval;
                paramTypes[count] = eval.ReturnType;
                count++;
                if (!(childNode.IsConstantResult)) {
                    allConstants = false;
                }
            }
    
            // Try to resolve the method
            FastMethod staticMethod;
            MethodInfo method;
            try {
                if (optionalClass != null) {
                    method = engineImportService.ResolveMethod(optionalClass, methodName, paramTypes, allowEventBeanType, allowEventBeanCollType);
                } else {
                    method = engineImportService.ResolveMethodOverloadChecked(className, methodName, paramTypes, allowEventBeanType, allowEventBeanCollType);
                }
                var declaringClass = FastClass.Create(method.DeclaringType);
                staticMethod = declaringClass.GetMethod(method);
            } catch (Exception e) {
                throw exceptionHandler.Handle(e);
            }
    
            // rewrite those evaluator that should return the event itself
            var parameterTypes = method.IsExtensionMethod()
                ? method.GetParameterTypes().Skip(1).ToArray()
                : method.GetParameterTypes();

            if (CollectionUtil.IsAnySet(allowEventBeanType)) {
                for (var i = 0; i < parameters.Count; i++) {
                    if (allowEventBeanType[i] && parameterTypes[i] == typeof(EventBean)) {
                        childEvals[i] = childEvalsEventBeanReturnTypes[i];
                    }
                }
            }
    
            // rewrite those evaluators that should return the event collection
            if (CollectionUtil.IsAnySet(allowEventBeanCollType)) {
                for (var i = 0; i < parameters.Count; i++) {
                    if (allowEventBeanCollType[i] && parameterTypes[i].IsGenericCollection()) {
                        childEvals[i] = childEvalsEventBeanReturnTypes[i];
                    }
                }
            }
    
            // add an evaluator if the method expects a context object
            if (!method.IsVarArgs() && parameterTypes.Length > 0 &&
                    parameterTypes[parameterTypes.Length - 1] == typeof(EPLMethodInvocationContext))
            {
                childEvals = (ExprEvaluator[]) CollectionUtil.ArrayExpandAddSingle(
                    childEvals,
                    new ExprNodeUtilExprEvalMethodContext(engineURI, functionName, eventAdapterService));
            }
    
            // handle varargs
            if (method.IsVarArgs()) {
                // handle context parameter
                int numMethodParams = parameterTypes.Length;
                if (numMethodParams > 1 && parameterTypes[numMethodParams - 2] == typeof(EPLMethodInvocationContext))
                {
                    var rewritten = new ExprEvaluator[childEvals.Length + 1];
                    Array.Copy(childEvals, 0, rewritten, 0, numMethodParams - 2);
                    rewritten[numMethodParams - 2] = new ExprNodeUtilExprEvalMethodContext(engineURI, functionName, eventAdapterService);
                    Array.Copy(childEvals, numMethodParams - 2, rewritten, numMethodParams - 1, childEvals.Length - (numMethodParams - 2));
                    childEvals = rewritten;
                }
    
                childEvals = MakeVarargArrayEval(method, childEvals);
            }
    
            return new ExprNodeUtilMethodDesc(allConstants, paramTypes, childEvals, method, staticMethod, null);
        }

        public static void ValidatePlainExpression(ExprNodeOrigin origin, ExprNode expression)
        {
            ExprNodeSummaryVisitor summaryVisitor = new ExprNodeSummaryVisitor();
            ValidatePlainExpression(origin, expression, summaryVisitor);
        }

        public static void ValidatePlainExpression(ExprNodeOrigin origin, IEnumerable<ExprNode> expressions)
        {
            ExprNodeSummaryVisitor summaryVisitor = new ExprNodeSummaryVisitor();
            foreach (ExprNode expression in expressions) {
                ValidatePlainExpression(origin, expression, summaryVisitor);
            }
        }

        private static void ValidatePlainExpression(ExprNodeOrigin origin, ExprNode expression, ExprNodeSummaryVisitor summaryVisitor)
        {
            expression.Accept(summaryVisitor);
            if (summaryVisitor.HasAggregation 
                || summaryVisitor.HasSubselect 
                || summaryVisitor.HasStreamSelect 
                || summaryVisitor.HasPreviousPrior) {
                String expressionTextualName = ToExpressionStringMinPrecedenceSafe(expression);
                throw new ExprValidationException(
                    string.Format(
                        "Invalid {0} expression '{1}': Aggregation, sub-select, previous or prior functions are not supported in this context",
                        origin.GetClauseName(), expressionTextualName));
            }
        }

        public static ExprNode ValidateSimpleGetSubtree(
            ExprNodeOrigin origin,
            ExprNode expression,
            StatementContext statementContext,
            EventType optionalEventType,
            bool allowBindingConsumption)
        {
            ExprNodeUtility.ValidatePlainExpression(origin, expression);

            StreamTypeServiceImpl streamTypes;
            if (optionalEventType != null)
            {
                streamTypes = new StreamTypeServiceImpl(optionalEventType, null, true, statementContext.EngineURI);
            }
            else
            {
                streamTypes = new StreamTypeServiceImpl(statementContext.EngineURI, false);
            }

            var validationContext = new ExprValidationContext(
                statementContext.Container,
                streamTypes, statementContext.EngineImportService, statementContext.StatementExtensionServicesContext,
                null, statementContext.SchedulingService, statementContext.VariableService,
                statementContext.TableService, new ExprEvaluatorContextStatement(statementContext, false),
                statementContext.EventAdapterService, statementContext.StatementName, statementContext.StatementId,
                statementContext.Annotations, statementContext.ContextDescriptor, statementContext.ScriptingService, 
                false, false, allowBindingConsumption,
                false, null, false);
            return GetValidatedSubtree(origin, expression, validationContext);
        }

        public static ExprValidationContext GetExprValidationContextStatementOnly(StatementContext statementContext)
        {
            return new ExprValidationContext(
                statementContext.Container, 
                new StreamTypeServiceImpl(statementContext.EngineURI, false), statementContext.EngineImportService,
                statementContext.StatementExtensionServicesContext, null, statementContext.SchedulingService,
                statementContext.VariableService, statementContext.TableService,
                new ExprEvaluatorContextStatement(statementContext, false), statementContext.EventAdapterService,
                statementContext.StatementName, statementContext.StatementId, statementContext.Annotations,
                statementContext.ContextDescriptor, statementContext.ScriptingService,
                false, false, false, false, null, false);
        }

        public static ISet<string> GetPropertyNamesIfAllProps(ExprNode[] expressions)
        {
            foreach (var expression in expressions)
            {
                if (!(expression is ExprIdentNode))
                {
                    return null;
                }
            }
            var uniquePropertyNames = new HashSet<string>();
            foreach (var expression in expressions)
            {
                var identNode = (ExprIdentNode) expression;
                uniquePropertyNames.Add(identNode.UnresolvedPropertyName);
            }
            return uniquePropertyNames;
        }

        public static string[] ToExpressionStringsMinPrecedence(IList<ExprNode> expressions)
        {
            var texts = new string[expressions.Count];
            for (var i = 0; i < expressions.Count; i++)
            {
                texts[i] = ToExpressionStringMinPrecedenceSafe(expressions[i]);
            }
            return texts;
        }

        public static IList<Pair<ExprNode, ExprNode>> FindExpression(ExprNode selectExpression, ExprNode searchExpression) {
            var pairs = new List<Pair<ExprNode, ExprNode>>();
            if (DeepEquals(selectExpression, searchExpression, false)) {
                pairs.Add(new Pair<ExprNode, ExprNode>(null, selectExpression));
                return pairs;
            }
            FindExpressionChildRecursive(selectExpression, searchExpression, pairs);
            return pairs;
        }
    
        private static void FindExpressionChildRecursive(ExprNode parent, ExprNode searchExpression, ICollection<Pair<ExprNode, ExprNode>> pairs) {
            foreach (var child in parent.ChildNodes) {
                if (DeepEquals(child, searchExpression, false)) {
                    pairs.Add(new Pair<ExprNode, ExprNode>(parent, child));
                    continue;
                }
                FindExpressionChildRecursive(child, searchExpression, pairs);
            }
        }

        public static void ToExpressionStringParameterList(ExprNode[] childNodes, TextWriter buffer)
        {
            var delimiter = "";
            foreach (var childNode in childNodes) {
                buffer.Write(delimiter);
                buffer.Write(ToExpressionStringMinPrecedenceSafe(childNode));
                delimiter = ",";
            }
        }
    
        public static void ToExpressionStringWFunctionName(string functionName, IList<ExprNode> childNodes, TextWriter writer) {
            writer.Write(functionName);
            writer.Write("(");
            ToExpressionStringParameterList(childNodes, writer);
            writer.Write(')');
        }
    
        public static string[] GetIdentResolvedPropertyNames(IList<ExprNode> nodes) {
            var propertyNames = new string[nodes.Count];
            for (var i = 0; i < propertyNames.Length; i++)
            {
                if (nodes[i] is ExprIdentNode node)
                    propertyNames[i] = node.ResolvedPropertyName;
                else
                    throw new ArgumentException("Expressions are not ident nodes");
            }
            return propertyNames;
        }
    
        public static Type[] GetExprResultTypes(ExprNode[] groupByNodes) {
            var types = new Type[groupByNodes.Length];
            for (var i = 0; i < types.Length; i++) {
                types[i] = groupByNodes[i].ExprEvaluator.ReturnType;
            }
            return types;
        }
    
        public static ExprEvaluator MakeUnderlyingEvaluator(int streamNum, Type resultType, TableMetadata tableMetadata) {
            if (tableMetadata != null) {
                return new ExprNodeUtilUnderlyingEvaluatorTable(streamNum, resultType, tableMetadata);
            }
            return new ExprNodeUtilUnderlyingEvaluator(streamNum, resultType);
        }

        public static bool HasStreamSelect(IList<ExprNode> exprNodes)
        {
            var visitor = new ExprNodeStreamSelectVisitor(false);
            foreach (var node in exprNodes) {
                node.Accept(visitor);
                if (visitor.HasStreamSelect()) {
                    return true;
                }
            }
            return false;
        }
    
        public static void ValidateNoSpecialsGroupByExpressions(IList<ExprNode> groupByNodes) {
            var visitorSubselects = new ExprNodeSubselectDeclaredDotVisitor();
            var visitorGrouping = new ExprNodeGroupingVisitorWParent();
            var aggNodesInGroupBy = new List<ExprAggregateNode>(1);
    
            foreach (var groupByNode in groupByNodes) {
    
                // no subselects
                groupByNode.Accept(visitorSubselects);
                if (visitorSubselects.Subselects.Count > 0) {
                    throw new ExprValidationException("Subselects not allowed within group-by");
                }
    
                // no special grouping-clauses
                groupByNode.Accept(visitorGrouping);
                if (!visitorGrouping.GroupingIdNodes.IsEmpty()) {
                    throw ExprGroupingIdNode.MakeException("grouping_id");
                }
                if (!visitorGrouping.GroupingNodes.IsEmpty()) {
                    throw ExprGroupingIdNode.MakeException("grouping");
                }
    
                // no aggregations allowed
                ExprAggregateNodeUtil.GetAggregatesBottomUp(groupByNode, aggNodesInGroupBy);
                if (!aggNodesInGroupBy.IsEmpty()) {
                    throw new ExprValidationException("Group-by expressions cannot contain aggregate functions");
                }
            }
        }
    
        public static IDictionary<string, ExprNamedParameterNode> GetNamedExpressionsHandleDups(IList<ExprNode> parameters) {
            IDictionary<string, ExprNamedParameterNode> nameds = null;
    
            foreach (var node in parameters) {
                if (node is ExprNamedParameterNode) {
                    var named = (ExprNamedParameterNode) node;
                    if (nameds == null) {
                        nameds = new Dictionary<string, ExprNamedParameterNode>();
                    }
                    var lowerCaseName = named.ParameterName.ToLowerInvariant();
                    if (nameds.ContainsKey(lowerCaseName)) {
                        throw new ExprValidationException("Duplicate parameter '" + lowerCaseName + "'");
                    }
                    nameds.Put(lowerCaseName, named);
                }
            }
            if (nameds == null) {
                return Collections.GetEmptyMap<string, ExprNamedParameterNode>();
            }
            return nameds;
        }
    
        public static void ValidateNamed(IDictionary<string, ExprNamedParameterNode> namedExpressions, string[] namedParameters) {
            foreach (var entry in namedExpressions) {
                var found = false;
                foreach (var named in namedParameters) {
                    if (named.Equals(entry.Key)) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    throw new ExprValidationException("Unexpected named parameter '" + entry.Key + "', expecting any of the following: " + CollectionUtil.ToStringArray(namedParameters));
                }
            }
        }
    
        public static bool ValidateNamedExpectType(ExprNamedParameterNode namedParameterNode, Type[] expectedTypes) {
            if (namedParameterNode.ChildNodes.Count != 1) {
                throw GetNamedValidationException(namedParameterNode.ParameterName, expectedTypes);
            }
    
            var childNode = namedParameterNode.ChildNodes[0];
            Type returnType = childNode.ExprEvaluator.ReturnType.GetBoxedType();
    
            var found = false;
            foreach (var expectedType in expectedTypes) {
                if (expectedType == typeof(TimePeriod) && childNode is ExprTimePeriod) {
                    found = true;
                    break;
                }
                if (returnType == expectedType.GetBoxedType()) {
                    found = true;
                    break;
                }
            }
    
            if (found) {
                return namedParameterNode.ChildNodes[0].IsConstantResult;
            }
            throw GetNamedValidationException(namedParameterNode.ParameterName, expectedTypes);
        }
    
        private static ExprValidationException GetNamedValidationException(string parameterName, Type[] expected) {
            string expectedType;
            if (expected.Length == 1) {
                expectedType = "a " + TypeHelper.GetSimpleNameForType(expected[0]) + "-typed value";
            } else {
                var buf = new StringWriter();
                buf.Write("any of the following types: ");
                var delimiter = "";
                foreach (var clazz in expected) {
                    buf.Write(delimiter);
                    buf.Write(TypeHelper.GetSimpleNameForType(clazz));
                    delimiter = ",";
                }
                expectedType = buf.ToString();
            }
            var message = "Failed to validate named parameter '" + parameterName + "', expected a single expression returning " + expectedType;
            return new ExprValidationException(message);
        }

        public static ExprNode[] AddExpression(IList<ExprNode> expressions, ExprNode expression)
        {
            return expressions.Append(expression).ToArray();
        }

        public static QueryGraph ValidateFilterGetQueryGraphSafe(ExprNode filterExpression, StatementContext statementContext, StreamTypeServiceImpl typeService)
        {
            ExcludePlanHint excludePlanHint = null;
            try
            {
                excludePlanHint = ExcludePlanHint.GetHint(typeService.StreamNames, statementContext);
            }
            catch (ExprValidationException ex)
            {
                Log.Warn("Failed to consider exclude-plan hint: " + ex.Message, ex);
            }

            var queryGraph = new QueryGraph(1, excludePlanHint, false);
            ValidateFilterWQueryGraphSafe(queryGraph, filterExpression, statementContext, typeService);
            return queryGraph;
        }

        public static void ValidateFilterWQueryGraphSafe(QueryGraph queryGraph, ExprNode filterExpression, StatementContext statementContext, StreamTypeServiceImpl typeService)
        {
            try
            {
                ExprEvaluatorContextStatement evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
                ExprValidationContext validationContext = new ExprValidationContext(
                    statementContext.Container,
                    typeService, 
                    statementContext.EngineImportService, 
                    statementContext.StatementExtensionServicesContext, null, 
                    statementContext.TimeProvider, 
                    statementContext.VariableService, 
                    statementContext.TableService, evaluatorContextStmt, 
                    statementContext.EventAdapterService, 
                    statementContext.StatementName,
                    statementContext.StatementId, 
                    statementContext.Annotations, 
                    statementContext.ContextDescriptor,
                    statementContext.ScriptingService,
                    false, false, true, false, null, true);
                ExprNode validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.FILTER, filterExpression, validationContext);
                FilterExprAnalyzer.Analyze(validated, queryGraph, false);
            }
            catch (Exception ex)
            {
                Log.Warn("Unexpected exception analyzing filterable expression '" + ToExpressionStringMinPrecedenceSafe(filterExpression) + "': " + ex.Message, ex);
            }
        }

        public static void AcceptChain(ExprNodeVisitor visitor, IList<ExprChainedSpec> chainSpec)
        {
            foreach (var chain in chainSpec) {
                foreach (var param in chain.Parameters) {
                    param.Accept(visitor);
                }
            }
        }

        public static void AcceptChain(ExprNodeVisitorWithParent visitor, IList<ExprChainedSpec> chainSpec)
        {
            foreach (var chain in chainSpec) {
                foreach (var param in chain.Parameters) {
                    param.Accept(visitor);
                }
            }
        }

        public static void AcceptChain(ExprNodeVisitorWithParent visitor, IList<ExprChainedSpec> chainSpec, ExprNode parent)
        {
            foreach (var chain in chainSpec) {
                foreach (var param in chain.Parameters) {
                    param.AcceptChildnodes(visitor, parent);
                }
            }
        }
    
        public static void ReplaceChildNode(ExprNode parentNode, ExprNode nodeToReplace, ExprNode newNode) {
            var index = FindChildNode(parentNode, nodeToReplace);
            if (index == -1) {
                parentNode.ReplaceUnlistedChildNode(nodeToReplace, newNode);
            } else {
                parentNode.SetChildNode(index, newNode);
            }
        }
    
        private static int FindChildNode(ExprNode parentNode, ExprNode childNode) {
            for (var i = 0; i < parentNode.ChildNodes.Count; i++) {
                if (parentNode.ChildNodes[i] == childNode) {
                    return i;
                }
            }
            return -1;
        }

        public static void ReplaceChainChildNode(ExprNode nodeToReplace, ExprNode newNode, IList<ExprChainedSpec> chainSpec)
        {
            foreach (var chained in chainSpec) {
                var index = chained.Parameters.IndexOf(nodeToReplace);
                if (index != -1)
                {
                    chained.Parameters[index] = newNode;
                }
            }
        }

        public static ExprNodePropOrStreamSet GetNonAggregatedProps(EventType[] types, IList<ExprNode> exprNodes, ContextPropertyRegistry contextPropertyRegistry)
        {
            // Determine all event properties in the clause
            var nonAggProps = new ExprNodePropOrStreamSet();
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
            foreach (var node in exprNodes) {
                visitor.Reset();
                node.Accept(visitor);
                AddNonAggregatedProps(nonAggProps, visitor.GetRefs(), types, contextPropertyRegistry);
            }
    
            return nonAggProps;
        }

        private static void AddNonAggregatedProps(
            ExprNodePropOrStreamSet nonAggProps,
            IEnumerable<ExprNodePropOrStreamDesc> refs,
            EventType[] types,
            ContextPropertyRegistry contextPropertyRegistry)
        {
            foreach (var pair in refs)
            {
                if (pair is ExprNodePropOrStreamPropDesc)
                {
                    var propDesc = (ExprNodePropOrStreamPropDesc) pair;
                    var originType = types.Length > pair.StreamNum ? types[pair.StreamNum] : null;
                    if (originType == null || contextPropertyRegistry == null ||
                        !contextPropertyRegistry.IsPartitionProperty(originType, propDesc.PropertyName))
                    {
                        nonAggProps.Add(pair);
                    }
                }
                else
                {
                    nonAggProps.Add(pair);
                }
            }
        }

        public static void AddNonAggregatedProps(
            ExprNode exprNode,
            ExprNodePropOrStreamSet set,
            EventType[] types,
            ContextPropertyRegistry contextPropertyRegistry)
        {
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
            exprNode.Accept(visitor);
            AddNonAggregatedProps(set, visitor.GetRefs(), types, contextPropertyRegistry);
        }

        public static ExprNodePropOrStreamSet GetAggregatedProperties(IList<ExprAggregateNode> aggregateNodes)
        {
            // Get a list of properties being aggregated in the clause.
            var propertiesAggregated = new ExprNodePropOrStreamSet();
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(true);
            foreach (ExprNode selectAggExprNode in aggregateNodes)
            {
                visitor.Reset();
                selectAggExprNode.Accept(visitor);
                IList<ExprNodePropOrStreamDesc> properties = visitor.GetRefs();
                propertiesAggregated.AddAll(properties);
            }

            return propertiesAggregated;
        }

        public static ExprEvaluator[] GetEvaluators(ExprNode[] exprNodes)
        {
            if (exprNodes == null)
            {
                return null;
            }
            var eval = new ExprEvaluator[exprNodes.Length];
            for (var i = 0; i < exprNodes.Length; i++)
            {
                var node = exprNodes[i];
                if (node != null)
                {
                    eval[i] = node.ExprEvaluator;
                }
            }
            return eval;
        }

        public static ExprEvaluator[] GetEvaluators(IList<ExprNode> childNodes)
        {
            var eval = new ExprEvaluator[childNodes.Count];
            for (var i = 0; i < childNodes.Count; i++)
            {
                eval[i] = childNodes[i].ExprEvaluator;
            }
            return eval;
        }

        public static ISet<int> GetIdentStreamNumbers(ExprNode child)
        {
            var streams = new HashSet<int>();
            var visitor = new ExprNodeIdentifierCollectVisitor();
            child.Accept(visitor);
            foreach (var node in visitor.ExprProperties) {
                streams.Add(node.StreamId);
            }
            return streams;
        }
    
        /// <summary>
        /// Returns true if all properties within the expression are witin data window'd streams.
        /// </summary>
        /// <param name="child">expression to interrogate</param>
        /// <param name="streamTypeService">streams</param>
        /// <param name="unidirectionalJoin">indicator unidirection join</param>
        /// <returns>indicator</returns>
        public static bool HasRemoveStreamForAggregations(ExprNode child, StreamTypeService streamTypeService, bool unidirectionalJoin)
        {
            // Determine whether all streams are istream-only or irstream
            bool[] isIStreamOnly = streamTypeService.IsIStreamOnly;
            var isAllIStream = true;    // all true?
            var isAllIRStream = true;   // all false?
            foreach (var anIsIStreamOnly in isIStreamOnly) {
                if (!anIsIStreamOnly) {
                    isAllIStream = false;
                } else {
                    isAllIRStream = false;
                }
            }
    
            // determine if a data-window applies to this max function
            var hasDataWindows = true;
            if (isAllIStream) {
                hasDataWindows = false;
            } else if (!isAllIRStream) {
                if (streamTypeService.EventTypes.Length > 1) {
                    if (unidirectionalJoin) {
                        return false;
                    }
                    // In a join we assume that a data window is present or implicit via unidirectional
                } else {
                    hasDataWindows = false;
                    // get all aggregated properties to determine if any is from a windowed stream
                    var visitor = new ExprNodeIdentifierCollectVisitor();
                    child.Accept(visitor);
                    foreach (var node in visitor.ExprProperties) {
                        if (!isIStreamOnly[node.StreamId]) {
                            hasDataWindows = true;
                            break;
                        }
                    }
                }
            }
    
            return hasDataWindows;
        }

        /// <summary>
        /// Apply a filter expression.
        /// </summary>
        /// <param name="filter">expression</param>
        /// <param name="streamZeroEvent">the event that represents stream zero</param>
        /// <param name="streamOneEvents">all events thate are stream one events</param>
        /// <param name="exprEvaluatorContext">context for expression evaluation</param>
        /// <returns>filtered stream one events</returns>
        public static EventBean[] ApplyFilterExpression(
            ExprEvaluator filter,
            EventBean streamZeroEvent,
            EventBean[] streamOneEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var eventsPerStream = new EventBean[2];
            eventsPerStream[0] = streamZeroEvent;
    
            var filtered = new EventBean[streamOneEvents.Length];
            var countPass = 0;

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            foreach (var eventBean in streamOneEvents)
            {
                eventsPerStream[1] = eventBean;

                var result = filter.Evaluate(evaluateParams);
                if ((result != null) && true.Equals(result)) {
                    filtered[countPass] = eventBean;
                    countPass++;
                }
            }
    
            if (countPass == streamOneEvents.Length) {
                return streamOneEvents;
            }
            return EventBeanUtility.ResizeArray(filtered, countPass);
        }
    
        /// <summary>
        /// Apply a filter expression returning a pass indicator.
        /// </summary>
        /// <param name="filter">to apply</param>
        /// <param name="eventsPerStream">events per stream</param>
        /// <param name="exprEvaluatorContext">context for expression evaluation</param>
        /// <returns>pass indicator</returns>
        public static bool ApplyFilterExpression(ExprEvaluator filter, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = filter.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            return true.Equals(result);
        }

        /// <summary>
        /// Compare two expression nodes and their children in exact child-node sequence,
        /// returning true if the 2 expression nodes trees are equals, or false if they are not equals.
        /// <para>
        /// Recursive call since it uses this method to compare child nodes in the same exact sequence.
        /// Nodes are compared using the equalsNode method.
        /// </para>
        /// </summary>
        /// <param name="nodeOne">- first expression top node of the tree to compare</param>
        /// <param name="nodeTwo">- second expression top node of the tree to compare</param>
        /// <param name="ignoreStreamPrefix">if set to <c>true</c> [ignore stream prefix].</param>
        /// <returns>
        /// false if this or all child nodes are not equal, true if equal
        /// </returns>
        public static bool DeepEquals(ExprNode nodeOne, ExprNode nodeTwo, bool ignoreStreamPrefix)
        {
            if (nodeOne.ChildNodes.Count != nodeTwo.ChildNodes.Count)
            {
                return false;
            }
            if (!nodeOne.EqualsNode(nodeTwo, ignoreStreamPrefix)) {
                return false;
            }
            for (var i = 0; i < nodeOne.ChildNodes.Count; i++)
            {
                var childNodeOne = nodeOne.ChildNodes[i];
                var childNodeTwo = nodeTwo.ChildNodes[i];
    
                if (!DeepEquals(childNodeOne, childNodeTwo, ignoreStreamPrefix)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compares two expression nodes via deep comparison, considering all
        /// child nodes of either side.
        /// </summary>
        /// <param name="one">array of expressions</param>
        /// <param name="two">array of expressions</param>
        /// <param name="ignoreStreamPrefix">if set to <c>true</c> [ignore stream prefix].</param>
        /// <returns>
        /// true if the expressions are equal, false if not
        /// </returns>
        public static bool DeepEquals(ExprNode[] one, ExprNode[] two, bool ignoreStreamPrefix) {
            if (one.Length != two.Length) {
                return false;
            }
            for (var i = 0; i < one.Length; i++) {
                if (!DeepEquals(one[i], two[i], ignoreStreamPrefix)) {
                    return false;
                }
            }
            return true;
        }

        public static bool DeepEquals(IList<ExprNode> one, IList<ExprNode> two, bool ignoreStreamPrefix)
        {
            if (one.Count != two.Count) {
                return false;
            }
            for (var i = 0; i < one.Count; i++) {
                if (!DeepEquals(one[i], two[i], ignoreStreamPrefix)) {
                    return false;
                }
            }
            return true;
        }
    
        /// <summary>
        /// Check if the expression is minimal: does not have a subselect, aggregation and does not need view resources
        /// </summary>
        /// <param name="expression">to inspect</param>
        /// <returns>null if minimal, otherwise name of offending sub-expression</returns>
        public static string IsMinimalExpression(ExprNode expression) {
            var subselectVisitor = new ExprNodeSubselectDeclaredDotVisitor();
            expression.Accept(subselectVisitor);
            if (subselectVisitor.Subselects.Count > 0) {
                return "a subselect";
            }
    
            var viewResourceVisitor = new ExprNodeViewResourceVisitor();
            expression.Accept(viewResourceVisitor);
            if (viewResourceVisitor.ExprNodes.Count > 0) {
                return "a function that requires view resources (prior, prev)";
            }
    
            var aggregateNodes = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(expression, aggregateNodes);
            if (!aggregateNodes.IsEmpty()) {
                return "an aggregation function";
            }
            return null;
        }
    
        public static void ToExpressionString(IList<ExprChainedSpec> chainSpec, TextWriter buffer, bool prefixDot, string functionName) {
            var delimiterOuter = "";
            if (prefixDot) {
                delimiterOuter = ".";
            }
            var isFirst = true;
            foreach (var element in chainSpec) {
                buffer.Write(delimiterOuter);
                if (functionName != null) {
                    buffer.Write(functionName);
                } else {
                    buffer.Write(element.Name);
                }
    
                // the first item without dot-prefix and empty parameters should not be appended with parenthesis
                if (!isFirst || prefixDot || !element.Parameters.IsEmpty()) {
                    ToExpressionStringIncludeParen(element.Parameters, buffer);
                }
    
                delimiterOuter = ".";
                isFirst = false;
            }
        }

        public static void ToExpressionStringParameterList(IList<ExprNode> parameters, TextWriter buffer)
        {
            var delimiter = "";
            foreach (var param in parameters) {
                buffer.Write(delimiter);
                delimiter = ",";
                buffer.Write(ToExpressionStringMinPrecedenceSafe(param));
            }
        }

        public static void ToExpressionString(ExprNode node, TextWriter buffer)
        {
            node.ToEPL(buffer, ExprPrecedenceEnum.MINIMUM);
        }

        public static void ToExpressionStringIncludeParen(IList<ExprNode> parameters, TextWriter buffer) {
            buffer.Write("(");
            ToExpressionStringParameterList(parameters, buffer);
            buffer.Write(")");
        }

        public static void Validate(ExprNodeOrigin origin, IList<ExprChainedSpec> chainSpec, ExprValidationContext validationContext)
        {
            // validate all parameters
            foreach (var chainElement in chainSpec) {
                var validated = new List<ExprNode>();
                foreach (var expr in chainElement.Parameters) {
                    validated.Add(GetValidatedSubtree(origin, expr, validationContext));
                    if (expr is ExprNamedParameterNode) {
                        throw new ExprValidationException("Named parameters are not allowed");
                    }
                }
                chainElement.Parameters = validated;
            }
        }

        public static IList<ExprNode> CollectChainParameters(IList<ExprChainedSpec> chainSpec)
        {
            var result = new List<ExprNode>();
            foreach (var chainElement in chainSpec) {
                result.AddAll(chainElement.Parameters);
            }
            return result;
        }
    
        public static void ToExpressionStringParams(TextWriter writer, IList<ExprNode> @params) {
            writer.Write('(');
            var delimiter = "";
            foreach (var childNode in @params) {
                writer.Write(delimiter);
                delimiter = ",";
                writer.Write(ToExpressionStringMinPrecedenceSafe(childNode));
            }
            writer.Write(')');
        }
    
        public static string PrintEvaluators(ExprEvaluator[] evaluators) {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var evaluator in evaluators) {
                writer.Write(delimiter);
                writer.Write(evaluator.GetType().Name);
                delimiter = ", ";
            }
            return writer.ToString();
        }

        public static ExprEvaluator[] CrontabScheduleValidate(
            ExprNodeOrigin origin,
            IList<ExprNode> scheduleSpecExpressionList,
            StatementContext context,
            bool allowBindingConsumption)
        {
    
            // Validate the expressions
            var expressions = new ExprEvaluator[scheduleSpecExpressionList.Count];
            var count = 0;
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(context, false);
            foreach (var parameters in scheduleSpecExpressionList)
            {
                var validationContext = new ExprValidationContext(
                    context.Container,
                    new StreamTypeServiceImpl(context.EngineURI, false), context.EngineImportService,
                    context.StatementExtensionServicesContext, null, 
                    context.SchedulingService,
                    context.VariableService,
                    context.TableService,
                    evaluatorContextStmt,
                    context.EventAdapterService,
                    context.StatementName,
                    context.StatementId, 
                    context.Annotations,
                    context.ContextDescriptor, 
                    context.ScriptingService,
                    false, false, allowBindingConsumption, false, null, false);
                var node = GetValidatedSubtree(origin, parameters, validationContext);
                expressions[count++] = node.ExprEvaluator;
            }
    
            if (expressions.Length <= 4 || expressions.Length >= 8) {
                throw new ExprValidationException("Invalid schedule specification: " + ScheduleSpecUtil.GetExpressionCountException(expressions.Length));
            }
    
            return expressions;
        }
    
        public static ScheduleSpec CrontabScheduleBuild(ExprEvaluator[] scheduleSpecEvaluators, ExprEvaluatorContext context) {
    
            // Build a schedule
            try {
                var scheduleSpecParameterList = EvaluateExpressions(scheduleSpecEvaluators, context);
                return ScheduleSpecUtil.ComputeValues(scheduleSpecParameterList);
            } catch (ScheduleParameterException e) {
                throw new EPException("Invalid schedule specification: " + e.Message, e);
            }
        }
    
        public static Object[] EvaluateExpressions(ExprEvaluator[] parameters, ExprEvaluatorContext exprEvaluatorContext) {
            var evaluateParams = new EvaluateParams(null, true, exprEvaluatorContext);
            var results = new Object[parameters.Length];
            var count = 0;
            foreach (var expr in parameters) {
                try {
                    results[count] = expr.Evaluate(evaluateParams);
                    count++;
                } catch (Exception ex) {
                    var message = "Failed expression evaluation in crontab timer-at for parameter " + count + ": " + ex.Message;
                    Log.Error(message, ex);
                    throw new ArgumentException(message);
                }
            }
            return results;
        }
    
        public static ExprNode[] ToArray(ICollection<ExprNode> expressions) {
            if (expressions.IsEmpty()) {
                return EMPTY_EXPR_ARRAY;
            }
            return expressions.ToArray();
        }

        public static ExprDeclaredNode[] ToArray(IList<ExprDeclaredNode> declaredNodes)
        {
            if (declaredNodes.IsEmpty()) {
                return EMPTY_DECLARED_ARR;
            }
            return declaredNodes.ToArray();
        }
    
        public static ExprNodePropOrStreamSet GetGroupByPropertiesValidateHasOne(ExprNode[] groupByNodes)
                {
            // Get the set of properties refered to by all group-by expression nodes.
            var propertiesGroupBy = new ExprNodePropOrStreamSet();
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(true);
    
            foreach (var groupByNode in groupByNodes) {
                visitor.Reset();
                groupByNode.Accept(visitor);
                var propertiesNode = visitor.GetRefs();
                propertiesGroupBy.AddAll(propertiesNode);
    
                // For each group-by expression node, require at least one property.
                if (propertiesNode.IsEmpty()) {
                    throw new ExprValidationException("Group-by expressions must refer to property names");
                }
            }
    
            return propertiesGroupBy;
        }
    
        private static ExprEvaluator[] MakeVarargArrayEval(MethodInfo method, ExprEvaluator[] childEvals) {
            var parameterTypes = method.GetParameterTypes();
            var evals = new ExprEvaluator[parameterTypes.Length];
            var varargClass = parameterTypes[parameterTypes.Length - 1].GetElementType();
            var varargClassBoxed = varargClass.GetBoxedType();
            if (parameterTypes.Length > 1)
            {
                Array.Copy(childEvals, 0, evals, 0, evals.Length - 1);
            }
            var varargArrayLength = childEvals.Length - parameterTypes.Length + 1;
    
            // handle passing array along
            if (varargArrayLength == 1) {
                var last = childEvals[parameterTypes.Length - 1];
                Type lastReturns = last.ReturnType;
                if (lastReturns != null && lastReturns.IsArray) {
                    evals[parameterTypes.Length - 1] = last;
                    return evals;
                }
            }
    
            // handle parameter conversion to vararg parameter
            var varargEvals = new ExprEvaluator[varargArrayLength];
            var coercers = new Coercer[varargEvals.Length];
            var needCoercion = false;
            for (var i = 0; i < varargArrayLength; i++) {
                var childEvalIndex = i + parameterTypes.Length - 1;
                Type resultType = childEvals[childEvalIndex].ReturnType;
                varargEvals[i] = childEvals[childEvalIndex];
    
                if (TypeHelper.IsSubclassOrImplementsInterface(resultType, varargClass)) {
                    // no need to coerce
                    continue;
                }
    
                if (resultType.GetBoxedType() != varargClassBoxed) {
                    needCoercion = true;
                    coercers[i] = CoercerFactory.GetCoercer(resultType, varargClassBoxed);
                }
            }
    
            ExprEvaluator varargEval;
            if (!needCoercion) {
                varargEval = new VarargOnlyArrayEvalNoCoerce(varargEvals, varargClass);
            } else {
                varargEval = new VarargOnlyArrayEvalWithCoerce(varargEvals, varargClass, coercers);
            }
            evals[parameterTypes.Length - 1] = varargEval;
            return evals;
        }
    
        /// <summary>
        /// Encapsulates the parse result parsing a mapped property as a class and method name with args.
        /// </summary>
        public class MappedPropertyParseResult
        {
            /// <summary>
            /// Returns the parse result of the mapped property.
            /// </summary>
            /// <param name="className">is the class name, or null if there isn't one</param>
            /// <param name="methodName">is the method name</param>
            /// <param name="argString">is the argument</param>
            public MappedPropertyParseResult(string className, string methodName, string argString) {
                ClassName = className;
                MethodName = methodName;
                ArgString = argString;
            }

            /// <summary>
            /// Returns class name.
            /// </summary>
            /// <value>name of class</value>
            public string ClassName { get; private set; }

            /// <summary>
            /// Returns the method name.
            /// </summary>
            /// <value>method name</value>
            public string MethodName { get; private set; }

            /// <summary>
            /// Returns the method argument.
            /// </summary>
            /// <value>arg</value>
            public string ArgString { get; private set; }
        }

        private class VarargOnlyArrayEvalNoCoerce : ExprEvaluator
        {
            private readonly ExprEvaluator[] _evals;
            private readonly Type _varargClass;

            public VarargOnlyArrayEvalNoCoerce(ExprEvaluator[] evals, Type varargClass)
            {
                _evals = evals;
                _varargClass = varargClass;
            }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                var array = Array.CreateInstance(_varargClass, _evals.Length);
                for (var i = 0; i < _evals.Length; i++)
                {
                    var value = _evals[i].Evaluate(evaluateParams);
                    array.SetValue(value, i);
                }
                return array;
            }

            public Type ReturnType
            {
                get { return TypeHelper.GetArrayType(_varargClass); }
            }
        }

        private class VarargOnlyArrayEvalWithCoerce : ExprEvaluator
        {
            private readonly ExprEvaluator[] _evals;
            private readonly Type _varargClass;
            private readonly Coercer[] _coercers;

            public VarargOnlyArrayEvalWithCoerce(ExprEvaluator[] evals, Type varargClass, Coercer[] coercers)
            {
                _evals = evals;
                _varargClass = varargClass;
                _coercers = coercers;
            }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                var array = Array.CreateInstance(_varargClass, _evals.Length);
                for (var i = 0; i < _evals.Length; i++)
                {
                    var value = _evals[i].Evaluate(evaluateParams);
                    if (_coercers[i] != null)
                    {
                        value = _coercers[i].Invoke(value);
                    }
                    array.SetValue(value, i);
                }
                return array;
            }

            public Type ReturnType
            {
                get { return TypeHelper.GetArrayType(_varargClass); }
            }
        }
    }
} // end of namespace
