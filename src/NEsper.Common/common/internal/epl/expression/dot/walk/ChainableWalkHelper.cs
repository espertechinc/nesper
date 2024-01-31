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
using System.Text;

using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityPrint;

namespace com.espertech.esper.common.@internal.epl.expression.dot.walk
{
    public class ChainableWalkHelper
    {
        public static ExprNode ProcessDot(
            bool useChainAsIs,
            bool resolveObjects,
            IList<Chainable> chain,
            StatementSpecMapContext mapContext)
        {
            if (chain.IsEmpty()) {
                throw new ArgumentException("Empty chain");
            }

            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction = chainSpec => {
                    var dotNode = new ExprDotNodeImpl(
                        chainSpec,
                        mapContext.Configuration.Compiler.Expression.IsDuckTyping,
                        mapContext.Configuration.Compiler.Expression.IsUdfCache);
                    // add any variables that are referenced
                    var variable = dotNode.IsVariableOpGetName(mapContext.VariableCompileTimeResolver);
                    if (variable != null) {
                        mapContext.VariableNames.Add(variable.VariableName);
                    }

                    return dotNode;
                }
                ;

            // Resolve objects if required
            if (resolveObjects) {
                var resolved = ResolveObject(chain, mapContext, dotNodeFunction);
                if (resolved != null) {
                    return resolved;
                }
            }

            // Check if we are dealing with a plain event property expression, i.e. one without any eventstream-dependent expression
            var plain = DeterminePlainProperty(chain);
            if (plain) {
                return HandlePlain(chain, dotNodeFunction, useChainAsIs);
            }

            return HandleNonPlain(chain, dotNodeFunction);
        }

        private static ExprNode ResolveObject(
            IList<Chainable> chain,
            StatementSpecMapContext mapContext,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            var chainFirst = chain[0];
            var chainFirstName = chainFirst.RootNameOrEmptyString;
            var chainFirstParams = chainFirst.ParametersOrEmpty;

            // Handle script
            var scriptNode = ExprDeclaredHelper.GetExistsScript(
                mapContext.Configuration.Compiler.Scripts.DefaultDialect,
                chainFirstName,
                chainFirstParams,
                mapContext.Scripts,
                mapContext.MapEnv);
            if (scriptNode != null) {
                return HandleScript(scriptNode, chain, dotNodeFunction);
            }

            // Handle Table-related exceptions
            // A table will be "table.more" or "table[x, ...].more"
            var table = mapContext.TableCompileTimeResolver.Resolve(chainFirstName);
            if (table != null) {
                var nodes = HandleTable(chain, table, dotNodeFunction);
                if (nodes != null) {
                    mapContext.TableExpressions.Add(nodes.Second);
                    return nodes.First;
                }
            }

            // Handle Variable-related exceptions
            // A variable will be "variable.more" or "variable[x, ...].more"
            var variable = mapContext.VariableCompileTimeResolver.Resolve(chainFirstName);
            if (variable != null) {
                mapContext.VariableNames.Add(variable.VariableName);
                return HandleVariable(chain, variable, mapContext, dotNodeFunction);
            }

            // Handle plug-in single-row functions
            var singleRow = TrySingleRow(mapContext, chainFirstName);
            if (singleRow != null) {
                return HandleSingleRow(singleRow, chain);
            }

            // try additional built-in single-row function
            var singleRowExtNode = mapContext.ImportService.ResolveSingleRowExtendedBuiltin(chainFirstName);
            if (singleRowExtNode != null) {
                return HandleSingleRowExt(singleRowExtNode, chain, dotNodeFunction);
            }

            // Handle declared-expression
            var declaredExpr = ExprDeclaredHelper.GetExistsDeclaredExpr(
                mapContext.Container,
                chainFirstName,
                chainFirstParams,
                mapContext.ExpressionDeclarations.Values,
                mapContext.ContextCompileTimeDescriptor,
                mapContext.MapEnv,
                mapContext.PlugInAggregations,
                mapContext.Scripts);
            if (declaredExpr != null) {
                mapContext.Add(declaredExpr.Second);
                return HandleDeclaredExpr(declaredExpr.First, chain, dotNodeFunction);
            }

            // Handle aggregation function
            var aggregationNode = chainFirst is ChainableName
                ? null
                : ASTAggregationHelper.TryResolveAsAggregation(
                    mapContext.ImportService,
                    chainFirst.IsDistinct,
                    chainFirstName,
                    mapContext.PlugInAggregations,
                    mapContext.ClassProvidedExtension);
            if (aggregationNode != null) {
                return HandleAggregation(aggregationNode, chain, dotNodeFunction);
            }

            // Handle context property
            if (mapContext.ContextCompileTimeDescriptor != null &&
                mapContext.ContextCompileTimeDescriptor.ContextPropertyRegistry
                    .IsContextPropertyPrefix(chainFirstName)) {
                return HandleContextProp(chain, dotNodeFunction);
            }

            // Handle min-max case
            var chainFirstLowerCase = chainFirstName.ToLowerInvariant();
            if (!(chainFirst is ChainableName) &&
                (chainFirstLowerCase.Equals("max") ||
                 chainFirstLowerCase.Equals("min") ||
                 chainFirstLowerCase.Equals("fmax") ||
                 chainFirstLowerCase.Equals("fmin"))) {
                return HandleMinMax(chainFirstLowerCase, chain, dotNodeFunction);
            }

            // Handle class name
            var classChain = HandleClassPrefixedNonProp(mapContext, chain);
            if (classChain != null) {
                return dotNodeFunction.Invoke(classChain);
            }

            return null;
        }

        private static ExprNode HandleContextProp(
            IList<Chainable> chain,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            try {
                var subproperty = ToPlainPropertyString(chain, 1);
                return new ExprContextPropertyNodeImpl(subproperty);
            }
            catch (ChainableWalkNotAPropertyException) {
                // may not be a property, handle as a dot-expression
            }

            for (var i = chain.Count - 1; i > 1; i--) {
                var subchain = chain.SubList(1, i);
                try {
                    var subproperty = ToPlainPropertyString(subchain, 0);
                    var contextProperty = new ExprContextPropertyNodeImpl(subproperty);
                    var calls = chain.SubList(i, chain.Count);
                    ExprNode dot = dotNodeFunction.Invoke(calls);
                    dot.AddChildNode(contextProperty);
                    return dot;
                }
                catch (ChainableWalkNotAPropertyException) {
                    // may not be a property, handle as a dot-expression
                }
            }

            return null;
        }

        private static ExprNode HandleSingleRowExt(
            ExprNode singleRowExtNode,
            IList<Chainable> chain,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            singleRowExtNode.AddChildNodes(chain[0].ParametersOrEmpty);
            if (chain.Count == 1) {
                return singleRowExtNode;
            }

            IList<Chainable> spec = new List<Chainable>(chain.SubList(1, chain.Count));
            ExprNode dot = dotNodeFunction.Invoke(spec);
            dot.AddChildNode(singleRowExtNode);
            return dot;
        }

        private static ExprNode HandleDeclaredExpr(
            ExprDeclaredNodeImpl node,
            IList<Chainable> chain,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            if (chain.Count == 1) {
                return node;
            }

            IList<Chainable> spec = new List<Chainable>(chain.SubList(1, chain.Count));
            ExprNode dot = dotNodeFunction.Invoke(spec);
            dot.AddChildNode(node);
            return dot;
        }

        private static ExprNode HandleMinMax(
            string chainFirstLowerCase,
            IList<Chainable> chain,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            var node = HandleMinMaxNode(chainFirstLowerCase, chain[0]);
            if (chain.Count == 1) {
                return node;
            }

            IList<Chainable> spec = new List<Chainable>(chain.SubList(1, chain.Count));
            ExprNode dot = dotNodeFunction.Invoke(spec);
            dot.AddChildNode(node);
            return dot;
        }

        private static ExprNode HandleMinMaxNode(
            string chainFirstLowerCase,
            Chainable spec)
        {
            MinMaxTypeEnum minMaxTypeEnum;
            var filtered = chainFirstLowerCase.StartsWith("f");
            if (chainFirstLowerCase.Equals("min") || chainFirstLowerCase.Equals("fmin")) {
                minMaxTypeEnum = MinMaxTypeEnum.MIN;
            }
            else if (chainFirstLowerCase.Equals("max") || chainFirstLowerCase.Equals("fmax")) {
                minMaxTypeEnum = MinMaxTypeEnum.MAX;
            }
            else {
                throw new ValidationException(
                    "Uncountered unrecognized min or max node '" + spec.RootNameOrEmptyString + "'");
            }

            var args = spec.ParametersOrEmpty;
            var distinct = spec.IsDistinct;
            var numArgsPositional = ExprAggregateNodeUtil.CountPositionalArgs(args);
            if (numArgsPositional > 1 && spec.IsDistinct && !filtered) {
                throw new ValidationException(
                    "The distinct keyword is not valid in per-row min and max " +
                    "functions with multiple sub-expressions");
            }

            ExprNode minMaxNode;
            if (!distinct && numArgsPositional > 1 && !filtered) {
                // use the row function
                minMaxNode = new ExprMinMaxRowNode(minMaxTypeEnum);
            }
            else {
                // use the aggregation function
                minMaxNode = new ExprMinMaxAggrNode(distinct, minMaxTypeEnum, filtered, false);
            }

            minMaxNode.AddChildNodes(args);
            return minMaxNode;
        }

        private static ExprNode HandleSingleRow(
            Pair<Type, ImportSingleRowDesc> singleRow,
            IList<Chainable> chain)
        {
            IList<Chainable> spec = new List<Chainable>();
            var methodName = singleRow.Second.MethodName;
            var nameUsed = chain[0].RootNameOrEmptyString;
            var call = new ChainableCall(methodName, chain[0].ParametersOrEmpty);
            spec.Add(call);
            spec.AddAll(chain.SubList(1, chain.Count));
            return new ExprPlugInSingleRowNode(nameUsed, singleRow.First, spec, singleRow.Second);
        }

        private static Pair<Type, ImportSingleRowDesc> TrySingleRow(
            StatementSpecMapContext mapContext,
            string chainFirstName)
        {
            try {
                return mapContext.ImportService.ResolveSingleRow(chainFirstName, mapContext.ClassProvidedExtension);
            }
            catch (ImportException) {
                return null;
            }
            catch (ImportUndefinedException) {
                return null;
            }
        }

        private static ExprNode HandleScript(
            ExprNodeScript scriptNode,
            IList<Chainable> chain,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            if (chain.Count == 1) {
                return scriptNode;
            }

            var subchain = chain.SubList(1, chain.Count);
            ExprDotNode dot = dotNodeFunction.Invoke(subchain);
            dot.AddChildNode(scriptNode);
            return dot;
        }

        private static ExprNode HandleVariable(
            IList<Chainable> chain,
            VariableMetaData variable,
            StatementSpecMapContext mapContext,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            var message = VariableUtil.CheckVariableContextName(mapContext.ContextName, variable);
            if (message != null) {
                throw new ValidationException(message);
            }

            ExprNode rootNode = new ExprVariableNodeImpl(variable, null);
            if (chain.Count == 1) {
                return rootNode;
            }

            // Handle simple-subproperty by means of variable node
            if (chain.Count == 2 && chain[1] is ChainableName) {
                return new ExprVariableNodeImpl(variable, chain[1].RootNameOrEmptyString);
            }

            var subchain = chain.SubList(1, chain.Count);
            ExprDotNode dot = dotNodeFunction.Invoke(subchain);
            dot.AddChildNode(rootNode);
            return dot;
        }

        private static IList<Chainable> HandleClassPrefixedNonProp(
            StatementSpecMapContext mapContext,
            IList<Chainable> chain)
        {
            var indexOfLastProp = GetClassIndexOfLastProp(chain);
            if (indexOfLastProp == -1 || indexOfLastProp == chain.Count - 1) {
                return null;
            }

            var depth = indexOfLastProp;
            var depthFound = -1;
            while (depth > 0) {
                var classNameCandidate = BuildClassName(chain, depth);
                try {
                    mapContext.ImportService.ResolveType(classNameCandidate, false, mapContext.ClassProvidedExtension);
                    depthFound = depth;
                    break;
                }
                catch (Exception) {
                    // expected, handled later when expression validation takes place
                }

                depth--;
            }

            if (depthFound == -1) {
                return null;
            }

            if (depth == indexOfLastProp) {
                var classNameCandidate = BuildClassName(chain, depth);
                return BuildSubchainWClassname(classNameCandidate, depth + 1, chain);
            }

            // include the next identifier, i.e. ENUM or CONSTANT etc.
            var classNameCandidateX = BuildClassName(chain, depth + 1);
            return BuildSubchainWClassname(classNameCandidateX, depth + 2, chain);
        }

        private static IList<Chainable> BuildSubchainWClassname(
            string classNameCandidate,
            int depth,
            IList<Chainable> chain)
        {
            IList<Chainable> newChain = new List<Chainable>(2);
            newChain.Add(new ChainableName(classNameCandidate));
            newChain.AddAll(chain.SubList(depth, chain.Count));
            return newChain;
        }

        private static int GetClassIndexOfLastProp(IList<Chainable> chain)
        {
            var indexOfLastProp = -1;
            for (var i = 0; i < chain.Count; i++) {
                var spec = chain[i];
                if (!(spec is ChainableName) || spec.IsOptional) {
                    return indexOfLastProp;
                }

                if (chain.Count > i + 1 && chain[i + 1] is ChainableArray) {
                    return indexOfLastProp;
                }

                indexOfLastProp = i;
            }

            return indexOfLastProp;
        }

        private static string BuildClassName(
            IList<Chainable> chain,
            int depthInclusive)
        {
            var builder = new StringBuilder();
            var delimiter = "";
            for (var i = 0; i < depthInclusive + 1; i++) {
                builder.Append(delimiter);
                builder.Append(chain[i].RootNameOrEmptyString);
                delimiter = ".";
            }

            return builder.ToString();
        }

        // Event properties can be plain properties or complex chains including function chain.
        //
        // Plain properties:
        // - have just constants, i.e. just array[0] and map('x')
        // - don't have inner expressions, i.e. don't have array[index_expr] or map(key_expr)
        // - they are handled by ExprIdentNode and completely by each event type
        // - this allows chains such as a.array[0].map('x') to evaluate directly within the underlying itself
        //   and without EventBean instance allocation and with eliminating casting the underlying
        //
        // Complex chain:
        // - always have an expression such as "array[index_indexexpr]"
        // - are handled by ExprDotNode
        // - evaluated as chain, using fragment event type i.e. EventBean instance allocation when required
        //
        private static ExprNode HandlePlain(
            IList<Chainable> chain,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction,
            bool useChainAsIs)
        {
            // Handle properties that are not prefixed by a stream name
            var first = chain[0];
            string propertyName = null;
            
            if (chain.Count == 1 ||
                IsArrayProperty(first, chain[1]) ||
                first.IsOptional ||
                IsMappedProperty(first)) {
                if (useChainAsIs) {
                    return dotNodeFunction.Invoke(chain);
                }

                propertyName = null;
                try {
                    propertyName = ToPlainPropertyString(chain, 0);
                }
                catch (ChainableWalkNotAPropertyException e) {
                    throw new ArgumentException("invalid argument", e);
                }

                return new ExprIdentNodeImpl(propertyName);
            }

            // Handle properties that can be prefixed by a stream name
            var leadingIdentifier = chain[0].RootNameOrEmptyString;
            var streamOrNestedPropertyName = DotEscaper.EscapeDot(leadingIdentifier);
            try {
                propertyName = ToPlainPropertyString(chain, 1);
            }
            catch (ChainableWalkNotAPropertyException e) {
                throw new ArgumentException("invalid argument", e);
            }

            return new ExprIdentNodeImpl(propertyName, streamOrNestedPropertyName);
        }

        private static bool IsArrayProperty(
            Chainable chainable,
            Chainable next)
        {
            if (!(next is ChainableArray array)) {
                return false;
            }

            return chainable is ChainableName && IsSingleParameterConstantOfType(array.Indexes, typeof(int?));
        }

        private static bool IsMappedProperty(Chainable chainable)
        {
            if (!(chainable is ChainableCall call)) {
                return false;
            }

            return IsSingleParameterConstantOfType(call.Parameters, typeof(string));
        }

        private static ExprNode HandleAggregation(
            ExprNode aggregationNode,
            IList<Chainable> chain,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            var firstSpec = chain.DeleteAt(0);
            aggregationNode.AddChildNodes(firstSpec.ParametersOrEmpty);
            ExprNode exprNode;
            if (chain.IsEmpty()) {
                exprNode = aggregationNode;
            }
            else {
                exprNode = dotNodeFunction.Invoke(chain);
                exprNode.AddChildNode(aggregationNode);
            }

            return exprNode;
        }

        private static ExprNode HandleNonPlain(
            IList<Chainable> chain,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            if (chain.Count == 1) {
                return dotNodeFunction.Invoke(chain);
            }

            // We know that this is not a plain event property.
            // Build a class name from the prefix.
            var indexOfLastProp = GetClassIndexOfLastProp(chain);
            if (indexOfLastProp != -1 && indexOfLastProp < chain.Count - 1) {
                var classNameCandidate = BuildClassName(chain, indexOfLastProp);
                chain = BuildSubchainWClassname(classNameCandidate, indexOfLastProp + 1, chain);
                return dotNodeFunction.Invoke(chain);
            }

            return dotNodeFunction.Invoke(chain);
        }

        private static bool DeterminePlainProperty(IList<Chainable> chain)
        {
            Chainable previous = null;
            foreach (var spec in chain) {
                if (spec is ChainableArray array) {
                    // must be "[index]" with index being an integer constant
                    if (!IsSingleParameterConstantOfType(array.Indexes, typeof(int?))) {
                        return false;
                    }

                    if (previous is ChainableArray) {
                        // plain property expressions don't allow two-dimensional array
                        return false;
                    }
                }

                if (spec is ChainableCall call) {
                    // must be "x(key)" with key being a string constant
                    if (!IsSingleParameterConstantOfType(call.Parameters, typeof(string))) {
                        return false;
                    }
                }

                previous = spec;
            }

            return true;
        }

        private static Pair<ExprNode, ExprTableAccessNode> HandleTable(
            IList<Chainable> chain,
            TableMetaData table,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            if (chain.Count == 1) {
                var node = new ExprTableAccessNodeTopLevel(table.TableName);
                return new Pair<ExprNode, ExprTableAccessNode>(node, node);
            }

            if (chain[1] is ChainableArray) {
                var tableKeys = ((ChainableArray)chain[1]).Indexes;
                return HandleTableSubchain(tableKeys, chain.SubList(2, chain.Count), table, dotNodeFunction);
            }
            else {
                return HandleTableSubchain(
                    EmptyList<ExprNode>.Instance, 
                    chain.SubList(1, chain.Count),
                    table,
                    dotNodeFunction);
            }
        }

        private static Pair<ExprNode, ExprTableAccessNode> HandleTableSubchain(
            IList<ExprNode> tableKeys,
            IList<Chainable> chain,
            TableMetaData table,
            Func<IList<Chainable>, ExprDotNodeImpl> dotNodeFunction)
        {
            if (chain.IsEmpty()) {
                var node = new ExprTableAccessNodeTopLevel(table.TableName);
                node.AddChildNodes(tableKeys);
                return new Pair<ExprNode, ExprTableAccessNode>(node, node);
            }

            // We make an exception when the table is keyed and the column is found and there are no table keys provided.
            // This accommodates the case "select MyTable.a from MyTable".
            var columnOrOtherName = chain[0].RootNameOrEmptyString;
            var tableColumn = table.Columns.Get(columnOrOtherName);
            if (tableColumn != null && table.IsKeyed && tableKeys.IsEmpty()) {
                return null; // let this be resolved as an identifier
            }

            if (chain.Count == 1) {
                if (chain[0] is ChainableName) {
                    var node = new ExprTableAccessNodeSubprop(
                        table.TableName,
                        columnOrOtherName);
                    node.AddChildNodes(tableKeys);
                    return new Pair<ExprNode, ExprTableAccessNode>(node, node);
                }

                if (columnOrOtherName.ToLowerInvariant().Equals("keys")) {
                    var node = new ExprTableAccessNodeKeys(table.TableName);
                    node.AddChildNodes(tableKeys);
                    return new Pair<ExprNode, ExprTableAccessNode>(node, node);
                }
                else {
                    throw new ValidationException(
                        "Invalid use of table '" +
                        table.TableName +
                        "', unrecognized use of function '" +
                        columnOrOtherName +
                        "', expected 'keys()'");
                }
            }

            var nodeX = new ExprTableAccessNodeSubprop(table.TableName, columnOrOtherName);
            nodeX.AddChildNodes(tableKeys);
            var subchain = chain.SubList(1, chain.Count);
            ExprNode exprNode = dotNodeFunction.Invoke(subchain);
            exprNode.AddChildNode(nodeX);
            return new Pair<ExprNode, ExprTableAccessNode>(exprNode, nodeX);
        }

        private static bool IsSingleParameterConstantOfType(
            IList<ExprNode> expressions,
            Type expected)
        {
            if (expressions.Count != 1) {
                return false;
            }

            var first = expressions[0];
            return IsConstantExprOfType(first, expected);
        }

        private static bool IsConstantExprOfType(
            ExprNode node,
            Type expected)
        {
            if (!(node is ExprConstantNode constantNode)) {
                return false;
            }

            if (!constantNode.ConstantAvailable) {
                return false;
            }

            var type = constantNode.ConstantType;
            if (type == null) {
                return expected == null;
            }

            var typeClass = type;
            return typeClass.GetBoxedType() == expected;
        }

        private static string ToPlainPropertyString(
            IList<Chainable> chain,
            int startIndex)
        {
            var buffer = new StringWriter();
            var delimiter = "";
            foreach (var element in chain.SubList(startIndex, chain.Count)) {
                if (element is ChainableName name) {
                    buffer.Write(delimiter);
                    buffer.Write(name.NameUnescaped);
                }
                else if (element is ChainableArray array) {
                    if (array.Indexes.Count != 1) {
                        throw new ChainableWalkNotAPropertyException(
                            "Expected plain array property but found multiple index expressions");
                    }

                    buffer.Write("[");
                    buffer.Write(ToExpressionStringMinPrecedenceSafe(array.Indexes[0]));
                    buffer.Write("]");
                }
                else if (element is ChainableCall call) {
                    if (call.Parameters.Count != 1) {
                        throw new ChainableWalkNotAPropertyException(
                            "Expected plain mapped property but found multiple key expressions");
                    }

                    buffer.Write(delimiter);
                    buffer.Write(call.NameUnescaped);
                    buffer.Write("(");
                    var param = call.Parameters[0];
                    if (!(param is ExprConstantNode constantNode)) {
                        throw new ChainableWalkNotAPropertyException(
                            "Expected plain mapped property single constant parameter");
                    }

                    if (constantNode.StringConstantWhenProvided != null) {
                        buffer.Write(constantNode.StringConstantWhenProvided);
                    }
                    else {
                        buffer.Write("'");
                        buffer.Write((string)constantNode.ConstantValue);
                        buffer.Write("'");
                    }

                    buffer.Write(")");
                }

                if (element.IsOptional) {
                    buffer.Write("?");
                }

                delimiter = ".";
            }

            return buffer.ToString();
        }
    }
} // end of namespace