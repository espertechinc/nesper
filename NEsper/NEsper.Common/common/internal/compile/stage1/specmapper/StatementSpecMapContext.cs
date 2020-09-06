///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.specmapper
{
    using PlugInAggregationsMap =
        LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction, AggregationMultiFunctionForge>;

    /// <summary>
    ///     Context for mapping a SODA statement to a statement specification, or multiple for subqueries,
    ///     and obtaining certain optimization information from a statement.
    /// </summary>
    public class StatementSpecMapContext
    {
        private IDictionary<string, ExpressionDeclItem> expressionDeclarations;

        public StatementSpecMapContext(
            ContextCompileTimeDescriptor contextCompileTimeDescriptor,
            StatementSpecMapEnv mapEnv,
            LazyAllocatedMap<HashableMultiKey, AggregationMultiFunctionForge> plugInAggregations,
            IList<ExpressionScriptProvided> scriptExpressions)
        {
            VariableNames = new HashSet<string>();
            MapEnv = mapEnv;
            ContextCompileTimeDescriptor = contextCompileTimeDescriptor;
            this.PlugInAggregations = plugInAggregations;
            Scripts = scriptExpressions;
        }

        public StatementSpecMapContext(
            ContextCompileTimeDescriptor contextCompileTimeDescriptor,
            StatementSpecMapEnv mapEnv) : this(
            contextCompileTimeDescriptor,
            mapEnv,
            new LazyAllocatedMap<HashableMultiKey, AggregationMultiFunctionForge>(),
            new List<ExpressionScriptProvided>(1))
        {
        }

        public VariableCompileTimeResolver VariableCompileTimeResolver => MapEnv.VariableCompileTimeResolver;

        /// <summary>
        ///     Returns the runtime import service.
        /// </summary>
        /// <returns>service</returns>
        public ImportServiceCompileTime ImportService => MapEnv.ImportService;

        /// <summary>
        ///     Returns the configuration.
        /// </summary>
        /// <returns>config</returns>
        public Configuration Configuration => MapEnv.Configuration;

        /// <summary>
        ///     Returns variables.
        /// </summary>
        /// <returns>variables</returns>
        public ISet<string> VariableNames { get; }


        public IDictionary<string, ExpressionDeclItem> ExpressionDeclarations {
            get {
                if (expressionDeclarations == null) {
                    return Collections.GetEmptyMap<string, ExpressionDeclItem>();
                }

                return expressionDeclarations;
            }
        }

        public IList<ExpressionScriptProvided> Scripts { get; }

        public string ContextName => ContextCompileTimeDescriptor?.ContextName;
        public ExprDeclaredCompileTimeResolver ExprDeclaredCompileTimeResolver => MapEnv.ExprDeclaredCompileTimeResolver;

        public TableCompileTimeResolver TableCompileTimeResolver => MapEnv.TableCompileTimeResolver;

        public LazyAllocatedMap<HashableMultiKey, AggregationMultiFunctionForge> PlugInAggregations { get; }

        public ContextCompileTimeDescriptor ContextCompileTimeDescriptor { get; }

        public ISet<ExprTableAccessNode> TableExpressions { get; } = new HashSet<ExprTableAccessNode>();

        public bool HasPriorExpression { get; set; }

        public StatementSpecMapEnv MapEnv { get; }

        public IList<ExprSubstitutionNode> SubstitutionNodes { get; } = new List<ExprSubstitutionNode>();
        
        public bool IsAttachPatternText => MapEnv.Configuration.Compiler.ByteCode.IsAttachPatternEPL;

        public ClassProvidedExtension ClassProvidedExtension => MapEnv.ClassProvidedExtension;

        public void Add(StatementSpecMapContext other)
        {
            TableExpressions.AddAll(other.TableExpressions);
            VariableNames.AddAll(other.VariableNames);
        }

        public void AddExpressionDeclarations(ExpressionDeclDesc expressionDeclarations)
        {
            foreach (ExpressionDeclItem item in expressionDeclarations.Expressions) {
                AddExpressionDeclaration(item);
            }
        }

        public void AddTo(StatementSpecRaw statementSpec)
        {
            statementSpec.TableExpressions.AddAll(TableExpressions);
            statementSpec.ReferencedVariables.AddAll(VariableNames);
        }

        public void AddExpressionDeclaration(ExpressionDeclItem item)
        {
            if (expressionDeclarations == null) {
                expressionDeclarations = new Dictionary<string, ExpressionDeclItem>();
            }

            expressionDeclarations.Put(item.Name, item);
        }

        public void AddScript(ExpressionScriptProvided item)
        {
            Scripts.Add(item);
        }
    }
} // end of namespace