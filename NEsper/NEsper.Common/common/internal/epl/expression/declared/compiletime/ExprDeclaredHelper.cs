///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredHelper
    {
        public static Pair<ExprDeclaredNodeImpl, StatementSpecMapContext> GetExistsDeclaredExpr(
            string name,
            IList<ExprNode> parameters,
            ICollection<ExpressionDeclItem> stmtLocalExpressions,
            ContextCompileTimeDescriptor contextCompileTimeDescriptor,
            StatementSpecMapEnv mapEnv,
            LazyAllocatedMap<HashableMultiKey, AggregationMultiFunctionForge> plugInAggregations,
            IList<ExpressionScriptProvided> scripts)
        {
            // Find among local expressions
            if (!stmtLocalExpressions.IsEmpty()) {
                foreach (var declNode in stmtLocalExpressions) {
                    if (declNode.Name.Equals(name)) {
                        var pair = GetExprDeclaredNode(
                            declNode.OptionalSoda,
                            stmtLocalExpressions,
                            contextCompileTimeDescriptor,
                            mapEnv,
                            plugInAggregations,
                            scripts);
                        var declared = new ExprDeclaredNodeImpl(
                            declNode,
                            parameters,
                            contextCompileTimeDescriptor,
                            pair.First);
                        return new Pair<ExprDeclaredNodeImpl, StatementSpecMapContext>(declared, pair.Second);
                    }
                }
            }

            // find among global expressions
            var found = mapEnv.ExprDeclaredCompileTimeResolver.Resolve(name);
            if (found != null) {
                var expression = found.OptionalSoda;
                if (expression == null) {
                    var bytes = found.OptionalSodaBytes.Invoke();
                    expression = (Expression) SerializerUtil.ByteArrToObject(bytes);
                }

                var pair = GetExprDeclaredNode(expression, stmtLocalExpressions, contextCompileTimeDescriptor, mapEnv, plugInAggregations, scripts);
                var declared = new ExprDeclaredNodeImpl(found, parameters, contextCompileTimeDescriptor, pair.First);
                return new Pair<ExprDeclaredNodeImpl, StatementSpecMapContext>(declared, pair.Second);
            }

            return null;
        }

        private static Pair<ExprNode, StatementSpecMapContext> GetExprDeclaredNode(
            Expression expression,
            ICollection<ExpressionDeclItem> stmtLocalExpressions,
            ContextCompileTimeDescriptor contextCompileTimeDescriptor,
            StatementSpecMapEnv mapEnv,
            LazyAllocatedMap<HashableMultiKey, AggregationMultiFunctionForge> plugInAggregations,
            IList<ExpressionScriptProvided> scripts)
        {
            var mapContext = new StatementSpecMapContext(contextCompileTimeDescriptor, mapEnv, plugInAggregations, scripts);
            foreach (var item in stmtLocalExpressions) {
                mapContext.AddExpressionDeclaration(item);
            }

            var body = StatementSpecMapper.MapExpression(expression, mapContext);
            return new Pair<ExprNode, StatementSpecMapContext>(body, mapContext);
        }

        public static ExprNodeScript GetExistsScript(
            string defaultDialect,
            string expressionName,
            IList<ExprNode> parameters,
            ICollection<ExpressionScriptProvided> scriptExpressions,
            StatementSpecMapEnv mapEnv)
        {
            ExpressionScriptProvided script;

            if (!scriptExpressions.IsEmpty()) {
                script = FindScript(expressionName, parameters.Count, scriptExpressions);
                if (script != null) {
                    return new ExprNodeScript(defaultDialect, script, parameters);
                }
            }

            script = mapEnv.ScriptCompileTimeResolver.Resolve(expressionName, parameters.Count);
            if (script != null) {
                return new ExprNodeScript(defaultDialect, script, parameters);
            }

            return null;
        }

        private static ExpressionScriptProvided FindScript(
            string name,
            int parameterCount,
            ICollection<ExpressionScriptProvided> scriptsByName)
        {
            if (scriptsByName == null || scriptsByName.IsEmpty()) {
                return null;
            }

            ExpressionScriptProvided nameMatchedScript = null;
            foreach (var script in scriptsByName) {
                if (script.Name.Equals(name) && script.ParameterNames.Length == parameterCount) {
                    return script;
                }

                if (script.Name.Equals(name)) {
                    nameMatchedScript = script;
                }
            }

            return nameMatchedScript;
        }
    }
} // end of namespace