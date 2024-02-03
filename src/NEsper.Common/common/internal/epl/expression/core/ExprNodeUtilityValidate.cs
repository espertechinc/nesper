///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.assign;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.@event.propertyparser.PropertyParserNoDep;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityValidate
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public static Type ValidateLHSTypeAnyAllSomeIn(Type type)
        {
            // collections, array or map not supported
            var message =
                "Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords";
            if (type == null) {
                throw new ExprValidationException(message);
            }

            var typeClass = type;
            if (typeClass.IsArray ||
                typeClass.IsGenericCollection() ||
                typeClass.IsGenericStringDictionary()) {
                throw new ExprValidationException(message);
            }

            return typeClass;
        }

        public static Type ValidateReturnsNumeric(ExprForge forge)
        {
            var type = forge.EvaluationType;
            ValidateReturnsNumeric(
                forge,
                () => "Implicit conversion from datatype '" +
                      (type == null ? "null" : type.CleanName()) +
                      "' to numeric is not allowed");
            return type;
        }

        public static void ValidateReturnsNumeric(
            ExprForge forge,
            Supplier<string> msg)
        {
            var type = forge.EvaluationType;
            if (!type.IsTypeNumeric()) {
                throw new ExprValidationException(msg.Invoke());
            }
        }

        public static void ValidatePlainExpression(
            ExprNodeOrigin origin,
            ExprNode[] expressions)
        {
            var summaryVisitor = new ExprNodeSummaryVisitor();
            foreach (var expression in expressions) {
                ValidatePlainExpression(origin, expression, summaryVisitor);
            }
        }

        public static void ValidatePlainExpression(
            ExprNodeOrigin origin,
            ExprNode expression)
        {
            var summaryVisitor = new ExprNodeSummaryVisitor();
            ValidatePlainExpression(origin, expression, summaryVisitor);
        }

        public static void ValidateAssignment(
            bool allowLHSVariables,
            ExprNodeOrigin origin,
            OnTriggerSetAssignment spec,
            ExprValidationContext validationContext)
        {
            // equals-assignments are "a=1" and "a[1]=2" and such
            // they are not "a.reset()"
            var assignment = CheckGetStraightAssignment(spec.Expression, allowLHSVariables);
            if (assignment == null) {
                assignment = new ExprAssignmentCurly(spec.Expression);
            }

            assignment.Validate(origin, validationContext);
            spec.Validated = assignment;
        }

        /// <summary>
        /// Check if the expression is minimal: does not have a subselect, aggregation and does not need view resources
        /// </summary>
        /// <param name="expression">to inspect</param>
        /// <returns>null if minimal, otherwise name of offending sub-expression</returns>
        public static string IsMinimalExpression(ExprNode expression)
        {
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

            IList<ExprAggregateNode> aggregateNodes = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(expression, aggregateNodes);
            if (!aggregateNodes.IsEmpty()) {
                return "an aggregation function";
            }

            return null;
        }

        /// <summary>
        /// Validates the expression node subtree that has this
        /// node as root. Some of the nodes of the tree, including the
        /// root, might be replaced in the process.
        /// </summary>
        /// <param name="origin">validate origin</param>
        /// <param name="exprNode">node</param>
        /// <param name="validationContext">context</param>
        /// <returns>the root node of the validated subtree, possibly different than the root node of the unvalidated subtree
        /// </returns>
        /// <throws>ExprValidationException when the validation fails</throws>
        public static ExprNode GetValidatedSubtree(
            ExprNodeOrigin origin,
            ExprNode exprNode,
            ExprValidationContext validationContext)
        {
            if (exprNode is ExprLambdaGoesNode) {
                return exprNode;
            }

            try {
                return GetValidatedSubtreeInternal(exprNode, validationContext, true);
            }
            catch (ExprValidationException ex) {
                try {
                    string text;
                    if (exprNode is ExprSubselectNode subselect) {
                        text = ExprNodeUtilityMake.GetSubqueryInfoText(subselect);
                    }
                    else {
                        text = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode);
                        if (text.Length > 40) {
                            var shortened = text.Substring(0, 35);
                            text = shortened + "...(" + text.Length + " chars)";
                        }

                        text = "'" + text + "'";
                    }

                    throw MakeValidationExWExpression(origin, text, ex);
                }
                catch (ExprValidationException) {
                    throw;
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception rtex) {
                    Log.Debug("Failed to render nice validation message text: " + rtex.Message, rtex);
                    // fall through
                }
            
                throw;
            }
        }

        public static ExprValidationException MakeValidationExWExpression(
            ExprNodeOrigin origin,
            string text,
            ExprValidationException ex)
        {
            return new ExprValidationException(
                $"Failed to validate {origin.GetClauseName()} expression {text}: {ex.Message}",
                ex);
        }

        public static bool ValidateNamedExpectType(
            ExprNamedParameterNode namedParameterNode,
            Type[] expectedTypes)
        {
            if (namedParameterNode.ChildNodes.Length != 1) {
                throw GetNamedValidationException(namedParameterNode.ParameterName, expectedTypes);
            }

            var childNode = namedParameterNode.ChildNodes[0];
            var returnType = childNode.Forge.EvaluationType.GetBoxedType();

            var found = false;
            foreach (var expectedType in expectedTypes) {
                if (expectedType == typeof(TimePeriod) && childNode is ExprTimePeriod) {
                    found = true;
                    break;
                }

                if ((returnType == expectedType.GetBoxedType()) ||
                    (expectedType.IsAssignableFrom(returnType))) {
                    found = true;
                    break;
                }
            }

            if (found) {
                return namedParameterNode.ChildNodes[0].Forge.ForgeConstantType.IsCompileTimeConstant;
            }

            throw GetNamedValidationException(namedParameterNode.ParameterName, expectedTypes);
        }

        private static ExprValidationException GetNamedValidationException(
            string parameterName,
            Type[] expected)
        {
            string expectedType;
            if (expected.Length == 1) {
                expectedType = "a " + TypeHelper.GetSimpleNameForType(expected[0]) + "-typed value";
            }
            else {
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

            var message = "Failed to validate named parameter '" +
                          parameterName +
                          "', expected a single expression returning " +
                          expectedType;
            return new ExprValidationException(message);
        }

        public static IDictionary<string, ExprNamedParameterNode> GetNamedExpressionsHandleDups(
            IList<ExprNode> parameters)
        {
            IDictionary<string, ExprNamedParameterNode> nameds = null;

            foreach (var node in parameters) {
                if (node is ExprNamedParameterNode named) {
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
                return EmptyDictionary<string, ExprNamedParameterNode>.Instance;
            }

            return nameds;
        }

        public static void ValidateNamed(
            IDictionary<string, ExprNamedParameterNode> namedExpressions,
            string[] namedParameters)
        {
            foreach (var entry in namedExpressions) {
                var found = false;
                foreach (var named in namedParameters) {
                    if (named.Equals(entry.Key)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    throw new ExprValidationException(
                        "Unexpected named parameter '" +
                        entry.Key +
                        "', expecting any of the following: " +
                        namedParameters.RenderAny());
                }
            }
        }

        private static ExprNode GetValidatedSubtreeInternal(
            ExprNode exprNode,
            ExprValidationContext validationContext,
            bool isTopLevel)
        {
            var result = exprNode;
            if (exprNode is ExprLambdaGoesNode) {
                return exprNode;
            }

            for (var i = 0; i < exprNode.ChildNodes.Length; i++) {
                var childNode = exprNode.ChildNodes[i];
                if (childNode is ExprDeclaredOrLambdaNode node) {
                    if (node.IsValidated) {
                        continue;
                    }
                }

                var childNodeValidated = GetValidatedSubtreeInternal(childNode, validationContext, false);
                exprNode.SetChildNode(i, childNodeValidated);
            }

            try {
                var optionalReplacement = exprNode.Validate(validationContext);
                if (optionalReplacement != null) {
                    return GetValidatedSubtreeInternal(optionalReplacement, validationContext, isTopLevel);
                }
            }
            catch (ExprValidationException e) {
                if (exprNode is ExprIdentNode identNode) {
                    // indicator for whether we should rethrow 'e'
                    bool shouldRethrowE = false;
                    
                    try {
                        if (!ResolveStaticMethodOrField(identNode, e, validationContext, out result)) {
                            throw;
                        }
                    }
                    catch (ExprValidationException ex) {
                        if (!TryResolveAsStreamName(identNode, validationContext, out result)) {
                            throw;
                        }
                    }
                }
                else {
                    throw;
                }
            }

            // For top-level expressions check if we perform audit
            if (isTopLevel) {
                if (validationContext.IsExpressionAudit) {
                    return (ExprNode)ExprNodeProxy.NewInstance(result);
                }
            }
            else {
                if (validationContext.IsExpressionNestedAudit &&
                    !(result is ExprIdentNode) &&
                    !(ExprNodeUtilityQuery.IsConstant(result))) {
                    return (ExprNode)ExprNodeProxy.NewInstance(result);
                }
            }

            return result;
        }

        public static void GetValidatedSubtree(
            ExprNodeOrigin origin,
            ExprNode[] exprNode,
            ExprValidationContext validationContext)
        {
            if (exprNode == null) {
                return;
            }

            for (var i = 0; i < exprNode.Length; i++) {
                exprNode[i] = GetValidatedSubtree(origin, exprNode[i], validationContext);
            }
        }

        public static void GetValidatedSubtree(
            ExprNodeOrigin origin,
            ExprNode[][] exprNode,
            ExprValidationContext validationContext)
        {
            if (exprNode == null) {
                return;
            }

            foreach (var anExprNode in exprNode) {
                GetValidatedSubtree(origin, anExprNode, validationContext);
            }
        }

        public static void Validate(
            ExprNodeOrigin origin,
            IList<Chainable> chainSpec,
            ExprValidationContext validationContext)
        {
            // validate all parameters
            foreach (var chainElement in chainSpec) {
                chainElement.Validate(origin, validationContext);
            }
        }

        private static void ValidatePlainExpression(
            ExprNodeOrigin origin,
            ExprNode expression,
            ExprNodeSummaryVisitor summaryVisitor)
        {
            expression.Accept(summaryVisitor);
            if (summaryVisitor.HasAggregation ||
                summaryVisitor.HasSubselect ||
                summaryVisitor.HasStreamSelect ||
                summaryVisitor.HasPreviousPrior) {
                var text = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expression);
                throw new ExprValidationException(
                    "Invalid " +
                    origin.GetClauseName() +
                    " expression '" +
                    text +
                    "': Aggregation, sub-select, previous or prior functions are not supported in this context");
            }
        }

        // Since static method calls such as "Class.method('a')" and mapped properties "Stream.property('key')"
        // look the same, however as the validation could not resolve "Stream.property('key')" before calling this method,
        // this method tries to resolve the mapped property as a static method.
        // Assumes that this is an ExprIdentNode.
        private static bool ResolveStaticMethodOrField(
            ExprIdentNode identNode,
            ExprValidationException propertyException,
            ExprValidationContext validationContext,
            out ExprNode exprNode)
        {
            // Reconstruct the original string
            var mappedProperty = new StringBuilder(identNode.UnresolvedPropertyName);
            if (identNode.StreamOrPropertyName != null) {
                mappedProperty.Insert(0, identNode.StreamOrPropertyName + '.');
            }

            // Parse the mapped property format into a class name, method and single string parameter
            var parse = ParseMappedProperty(mappedProperty.ToString());
            if (parse == null) {
                var constNode = ResolveIdentAsEnumConst(
                    mappedProperty.ToString(),
                    validationContext.ImportService,
                    validationContext.ClassProvidedExtension);
                if (constNode == null) {
                    exprNode = null;
                    return false;
                }

                exprNode = constNode;
                return true;
            }

            // If there is a class name, assume a static method is possible.
            if (parse.ClassName != null) {
                var parameters = Collections.SingletonList<ExprNode>(new ExprConstantNodeImpl(parse.ArgString));
                var chain = new List<Chainable>();
                chain.Add(new ChainableName(parse.ClassName));
                chain.Add(new ChainableCall(parse.MethodName, parameters));
                
                var exprConfig =
                    validationContext.StatementCompileTimeService.Configuration.Compiler.Expression;
                ExprNode result = new ExprDotNodeImpl(chain, exprConfig.IsDuckTyping, exprConfig.IsUdfCache);

                // Validate
                try {
                    result.Validate(validationContext);
                }
                catch (ExprValidationException e) {
                    throw new ExprValidationException(
                        $"Failed to resolve enumeration method, date-time method or mapped property '{mappedProperty}': {e.Message}", e);
                }

                exprNode = result;
                return true;
            }

            // There is no class name, try a single-row function
            var functionName = parse.MethodName;
            try {
                var classMethodPair = validationContext.ImportService.ResolveSingleRow(
                    functionName,
                    validationContext.ClassProvidedExtension);
                var parameters = Collections.SingletonList<ExprNode>(new ExprConstantNodeImpl(parse.ArgString));
                var chain = Collections.SingletonList<Chainable>(
                    new ChainableCall(classMethodPair.Second.MethodName, parameters));
                
                ExprNode result = new ExprPlugInSingleRowNode(
                    functionName,
                    classMethodPair.First,
                    chain,
                    classMethodPair.Second);

                // Validate
                try {
                    result.Validate(validationContext);
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    throw new ExprValidationException(
                        $"Plug-in aggregation function '{parse.MethodName}' failed validation: {ex.Message}");
                }

                exprNode = result;
                return true;
            }
            catch (ImportUndefinedException) {
                // Not an single-row function
            }
            catch (ImportException e) {
                throw new IllegalStateException("Error resolving single-row function: " + e.Message, e);
            }

            // Try an aggregation function factory
            try {
                var aggregationForge = validationContext.ImportService.ResolveAggregationFunction(
                    parse.MethodName, validationContext.ClassProvidedExtension);
                ExprNode result = new ExprPlugInAggNode(false, aggregationForge, parse.MethodName);
                result.AddChildNode(new ExprConstantNodeImpl(parse.ArgString));

                // Validate
                try {
                    result.Validate(validationContext);
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception e) {
                    throw new ExprValidationException(
                        "Plug-in aggregation function '" + parse.MethodName + "' failed validation: " + e.Message);
                }

                exprNode = result;
                return true;
            }
            catch (ImportUndefinedException) {
                // Not an aggregation function
            }
            catch (ImportException e) {
                throw new IllegalStateException("Error resolving aggregation: " + e.Message, e);
            }

            // absolutely cannot be resolved
            throw propertyException;
        }

        private static bool TryResolveAsStreamName(
            ExprIdentNode identNode,
            ExprValidationContext validationContext, 
            out ExprNode exprNode)
        {
            ExprStreamUnderlyingNode exprStream = new ExprStreamUnderlyingNodeImpl(
                identNode.UnresolvedPropertyName,
                false);

            try {
                exprStream.Validate(validationContext);
            }
            catch (ExprValidationException)
            {
                exprNode = default;
                return false;
            }

            exprNode = exprStream;
            return true;
        }

        private static ExprConstantNode ResolveIdentAsEnumConst(
            string constant,
            ImportServiceCompileTime importService,
            ExtensionClass extension)
        {
            var enumValue = ImportCompileTimeUtil.ResolveIdentAsEnum(
                constant,
                importService,
                extension,
                false);
            return enumValue != null ? new ExprConstantNodeImpl(enumValue) : null;
        }

        private static ExprAssignment CheckGetStraightAssignment(
            ExprNode node,
            bool allowLHSVariables)
        {
            var prop = CheckGetAssignmentToProp(node);
            if (prop != null) {
                return new ExprAssignmentStraight(node, new ExprAssignmentLHSIdent(prop.First), prop.Second);
            }

            if (!(node is ExprEqualsNode equalsNode)) {
                return null;
            }

            var lhs = equalsNode.ChildNodes[0];
            var rhs = equalsNode.ChildNodes[1];

            if (lhs is ExprVariableNode) {
                var variableNode = (ExprVariableNode)equalsNode.ChildNodes[0];
                if (!allowLHSVariables) {
                    throw new ExprValidationException(
                        "Left-hand-side does not allow variables for variable '" +
                        variableNode.VariableMetadata.VariableName +
                        "'");
                }

                var variableNameWSubprop = variableNode.VariableNameWithSubProp;
                var variableName = variableNameWSubprop;
                string subPropertyName = null;
                var indexOfDot = variableNameWSubprop.IndexOf('.');
                if (indexOfDot != -1) {
                    subPropertyName = variableNameWSubprop.Substring(indexOfDot + 1);
                    variableName = variableNameWSubprop.Substring(0, indexOfDot);
                }

                ExprAssignmentLHS lhsAssign;
                if (subPropertyName != null) {
                    lhsAssign = new ExprAssignmentLHSIdentWSubprop(variableName, subPropertyName);
                }
                else {
                    lhsAssign = new ExprAssignmentLHSIdent(variableName);
                }

                return new ExprAssignmentStraight(equalsNode, lhsAssign, rhs);
            }

            if (lhs is ExprDotNode dot) {
                var chainables = dot.ChainSpec;
                if (chainables.Count == 2 &&
                    chainables[0] is ChainableName chainableName &&
                    chainables[1] is ChainableArray chainableArray) {
                    return new ExprAssignmentStraight(
                        equalsNode,
                        new ExprAssignmentLHSArrayElement(chainableName.Name, chainableArray.Indexes),
                        rhs);
                }

                if (allowLHSVariables &&
                    dot.ChildNodes[0] is ExprVariableNode &&
                    chainables.Count == 1 &&
                    chainables[0] is ChainableArray) {
                    var variable = (ExprVariableNode)dot.ChildNodes[0];
                    var array = (ChainableArray)chainables[0];
                    return new ExprAssignmentStraight(
                        equalsNode,
                        new ExprAssignmentLHSArrayElement(variable.VariableMetadata.VariableName, array.Indexes),
                        rhs);
                }

                throw new ExprValidationException(
                    "Unrecognized left-hand-side assignment '" +
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(dot) +
                    "'");
            }

            if (lhs is ExprTableAccessNode) {
                throw new ExprValidationException(
                    "Table access expression not allowed on the left hand side, please remove the table prefix");
            }

            return null;
        }

        private static Pair<String, ExprNode> CheckGetAssignmentToProp(ExprNode node)
        {
            if (node is ExprEqualsNode equals) {
                if (equals.ChildNodes[0] is ExprIdentNode identNode) {
                    return new Pair<string, ExprNode>(identNode.FullUnresolvedName, equals.ChildNodes[1]);
                }
            }

            return null;
        }
        
        public static ExprEqualsNode GetEqualsNodeIfAssignment(ExprNode node)
        {
            return node is ExprEqualsNode equalsNode ? equalsNode : null;
        }
        
        public static void ValidateNoSpecialsGroupByExpressions(ExprNode[] groupByNodes)
        {
            var visitorSubselects = new ExprNodeSubselectDeclaredDotVisitor();
            var visitorGrouping = new ExprNodeGroupingVisitorWParent();
            IList<ExprAggregateNode> aggNodesInGroupBy = new List<ExprAggregateNode>(1);

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
    }
} // end of namespace