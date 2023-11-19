///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;
namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendAggregationMultiFunctionInlinedClass
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithneModule(execs);
            WiththerModule(execs);
            return execs;
        }

        public static IList<RegressionExecution> WiththerModule(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFInlinedOtherModule());
            return execs;
        }

        public static IList<RegressionExecution> WithneModule(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationMFInlinedOneModule());
            return execs;
        }

        private const string INLINEDCLASS_PREFIXMAP =
            "inlined_class \"\"\"\n" +
            "using System;\n" +
            "using System.Collections.Generic;\n" +
            "using com.espertech.esper.common.client;\n" +
            "using com.espertech.esper.common.client.hook.aggmultifunc;\n" +
            "using com.espertech.esper.common.client.hook.forgeinject;\n" +
            "using com.espertech.esper.common.@internal.epl.expression.core;\n" +
            "using com.espertech.esper.common.@internal.rettype;\n" +
            "using com.espertech.esper.common.@internal.epl.agg.core;\n" +
            //
            // For use with Apache Commons Collection 4:
            //
            //"import org.apache.commons.collections4.Trie;\n" +
            //"import org.apache.commons.collections4.trie.PatriciaTrie;\n" +
            "using com.espertech.esper.regressionlib.support.util;\n" +
            "namespace ${NAMESPACE} {\n" +
            "    [ExtensionAggregationMultiFunction(Names=\"trieState,trieEnter,triePrefixMap\")]\n" +
            "    /// <summary>\n" +
            "    /// The trie aggregation forge is the entry point for providing the multi-function aggregation.\n" +
            "    /// This example is compatible for use with tables.\n" +
            "    /// </summary>\n" +
            "    public class TrieAggForge : AggregationMultiFunctionForge {\n" +
            "        public void AddAggregationFunction(AggregationMultiFunctionDeclarationContext declarationContext) {}\n" +
            "        public AggregationMultiFunctionHandler ValidateGetHandler(AggregationMultiFunctionValidationContext validationContext) {\n" +
            "            string name = validationContext.FunctionName;\n" +
            "            if (name.Equals(\"trieState\")) {\n" +
            "                return new TrieAggHandlerTrieState();\n" +
            "            } else if (name.Equals(\"trieEnter\")) {\n" +
            "                return new TrieAggHandlerTrieEnter(validationContext.ParameterExpressions);\n" +
            "            } else if (name.Equals(\"triePrefixMap\")) {\n" +
            "                return new TrieAggHandlerTriePrefixMap();\n" +
            "            }\n" +
            "            throw new ArgumentException(\"Unrecognized name '\" + name + \"' for use with trie\");\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// This handler handles the \"trieState\"-type table column\n" +
            "        /// <summary />\n" +
            "        public class TrieAggHandlerTrieState : AggregationMultiFunctionHandler {\n" +
            "            public Type ReturnType => EPTypeHelper.SingleValue(typeof(SupportTrie<string, object>));\n" +
            "\n" +
            "            public AggregationMultiFunctionStateKey AggregationStateUniqueKey {\n" +
            "                get => new InertAggregationMultiFunctionStateKey();\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionStateMode StateMode {\n" +
            "                get {\n" +
            "                    InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(typeof(TrieAggStateFactory));\n" +
            "                return new AggregationMultiFunctionStateModeManaged(injection);\n" +
            "                }\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAccessorMode AccessorMode {\n" +
            "                get {\n" +
            "                // accessor that returns the trie itself\n" +
            "                    InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(typeof(TrieAggAccessorFactory));\n" +
            "                return new AggregationMultiFunctionAccessorModeManaged(injection);\n" +
            "                }\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAgentMode AgentMode {\n" +
            "                get {\n" +
            "                    throw new NotSupportedException(\"Trie aggregation access is only by the 'triePrefixMap' method\");\n" +
            "                }\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx) {\n" +
            "                throw new NotSupportedException(\"Trie aggregation access is only by the 'triePrefixMap' method\");\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// This handler handles the \"trieEnter\"-operation that updates trie state\n" +
            "        /// </summary>\n" +
            "        public class TrieAggHandlerTrieEnter : AggregationMultiFunctionHandler {\n" +
            "            private readonly ExprNode[] parameters;\n" +
            "\n" +
            "            public TrieAggHandlerTrieEnter(ExprNode[] parameters) {\n" +
            "                this.parameters = parameters;\n" +
            "            }\n" +
            "\n" +
            "            public EPChainableType ReturnType {\n" +
            "                // We return null unless using \"prefixMap\"\n" +
            "                get => EPChainableTypeNull.INSTANCE;\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionStateKey AggregationStateUniqueKey {\n" +
            "                get => throw new NotSupportedException(\"Not a trie state\");\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionStateMode StateMode {\n" +
            "                get => throw new NotSupportedException(\"Not a trie state\");\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAccessorMode AccessorMode {\n" +
            "                get {\n" +
            "                // accessor that returns the trie itself\n" +
            "                    InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(typeof(TrieAggAccessorFactory));\n" +
            "                return new AggregationMultiFunctionAccessorModeManaged(injection);\n" +
            "                }\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAgentMode AgentMode {\n" +
            "                get {\n" +
            "                    if (parameters.Length != 1 || parameters[0].Forge.EvaluationType != typeof(string)) {\n" +
            "                    throw new ArgumentException(\"Requires a single parameter returing a string value\");\n" +
            "                }\n" +
            "                    InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(typeof(TrieAggAgentFactory));\n" +
            "                    injection.AddExpression(\"keyExpression\", parameters[0]);\n" +
            "                return new AggregationMultiFunctionAgentModeManaged(injection);\n" +
            "            }\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx) {\n" +
            "                throw new NotSupportedException(\"Trie aggregation access is only by the 'triePrefixMap' method\");\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// This handler handles the \"prefixmap\" accessor for use with tables\n" +
            "        /// </summary>\n" +
            "        public class TrieAggHandlerTriePrefixMap : AggregationMultiFunctionHandler {\n" +
            "            public EPChainableType ReturnType {\n" +
            "                get => EPChainableTypeHelper.SingleValue(typeof(IDictionary<string, object>));\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionStateKey AggregationStateUniqueKey {\n" +
            "                get => throw new NotSupportedException(\"Not implemented for 'triePrefixMap' trie method\");\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionStateMode StateMode {\n" +
            "                get => throw new NotSupportedException(\"Not implemented for 'triePrefixMap' trie method\");\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAccessorMode AccessorMode {\n" +
            "                get => throw new NotSupportedException(\"Not implemented for 'triePrefixMap' trie method\");\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAgentMode AgentMode {\n" +
            "                get => throw new NotSupportedException(\"Not implemented for 'triePrefixMap' trie method\");\n" +
            "            }\n" +
            "\n" +
            "            public AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx) {\n" +
            "                if (ctx.Parameters.Length != 1 || ctx.Parameters[0].Forge.EvaluationType != typeof(string)) {\n" +
            "                    throw new ArgumentException(\"Requires a single parameter returning a string value\");\n" +
            "                }\n" +
            "                InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(typeof(TrieAggMethodFactoryPrefixMap));\n" +
            "                injection.AddExpression(\"keyExpression\", ctx.Parameters[0]);\n" +
            "                return new AggregationMultiFunctionAggregationMethodModeManaged(injection);\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The agent state factory is responsible for producing a state holder that holds the trie state\n" +
            "        /// </summary>\n" +
            "        public class TrieAggStateFactory : AggregationMultiFunctionStateFactory {\n" +
            "            public AggregationMultiFunctionState NewState(AggregationMultiFunctionStateFactoryContext ctx) {\n" +
            "                return new TrieAggState();\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The agent state is the state holder that holds the trie state\n" +
            "        /// </summary>\n" +
            "        public class TrieAggState : AggregationMultiFunctionState {\n" +
            "            internal readonly SupportTrie<string, IList<object>> trie = new SupportTrieSimpleStringKeyed<IList<object>>();\n" +
            "\n" +
            "            public SupportTrie<string, IList<object>> Trie => trie;\n" +
            "\n" +
            "            public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {\n" +
            "                throw new NotSupportedException(\"Not used since the agent updates the table\");\n" +
            "            }\n" +
            "\n" +
            "            public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {\n" +
            "                throw new NotSupportedException(\"Not used since the agent updates the table\");\n" +
            "            }\n" +
            "\n" +
            "            public void Clear() {\n" +
            "                trie.Clear();\n" +
            "            }\n" +
            "\n" +
            "            public void Add(string key, object underlying) {\n" +
            "                List<object> existing = (List<object>) trie.Get(key);\n" +
            "                if (existing != null) {\n" +
            "                    existing.Add(underlying);\n" +
            "                    return;\n" +
            "                }\n" +
            "                List<object> events = new List<object>(2);\n" +
            "                events.Add(underlying);\n" +
            "                trie.Put(key, events);\n" +
            "            }\n" +
            "\n" +
            "            public void Remove(string key, object underlying) {\n" +
            "                List<object> existing = (List<object>) trie.Get(key);\n" +
            "                if (existing != null) {\n" +
            "                    existing.Remove(underlying);\n" +
            "                    if (existing.Count == 0) {\n" +
            "                        trie.Remove(key);\n" +
            "                    }\n" +
            "                }\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The accessor factory is responsible for producing an accessor that returns the result of the trie table column when accessed without an aggregation method\n" +
            "        /// </summary>\n" +
            "        public class TrieAggAccessorFactory : AggregationMultiFunctionAccessorFactory {\n" +
            "            public AggregationMultiFunctionAccessor NewAccessor(AggregationMultiFunctionAccessorFactoryContext ctx) {\n" +
            "                return new TrieAggAccessor();\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The accessor returns the result of the trie table column when accessed without an aggregation method\n" +
            "        /// </summary>\n" +
            "        public class TrieAggAccessor : AggregationMultiFunctionAccessorBase {\n" +
            "            // This is the value return when just referring to the trie table column by itself without a method name such as \"prefixMap\".\n" +
            "            public override object GetValue(AggregationMultiFunctionState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {\n" +
            "                TrieAggState trie = (TrieAggState) state;\n" +
            "                return trie.Trie;\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The agent factory is responsible for producing an agent that handles all changes to the trie table column.\n" +
            "        /// </summary>\n" +
            "        public class TrieAggAgentFactory : AggregationMultiFunctionAgentFactory {\n" +
            "            public ExprEvaluator KeyExpression { get; set; }\n" +
            "            public AggregationMultiFunctionAgent NewAgent(AggregationMultiFunctionAgentFactoryContext ctx) {\n" +
            "                return new TrieAggAgent(this);\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The agent is responsible for all changes to the trie table column.\n" +
            "        /// </summary>\n" +
            "        public class TrieAggAgent : AggregationMultiFunctionAgent {\n" +
            "            private readonly TrieAggAgentFactory factory;\n" +
            "\n" +
            "            public TrieAggAgent(TrieAggAgentFactory factory) {\n" +
            "                this.factory = factory;\n" +
            "            }\n" +
            "\n" +
            "            public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationRow row, int column) {\n" +
            "                string key = (string) factory.KeyExpression.Evaluate(eventsPerStream, true, exprEvaluatorContext);\n" +
            "                TrieAggState trie = (TrieAggState) row.GetAccessState(column);\n" +
            "                trie.Add(key, eventsPerStream[0].Underlying);\n" +
            "            }\n" +
            "\n" +
            "            public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationRow row, int column) {\n" +
            "                string key = (string) factory.KeyExpression.Evaluate(eventsPerStream, false, exprEvaluatorContext);\n" +
            "                TrieAggState trie = (TrieAggState) row.GetAccessState(column);\n" +
            "                trie.Remove(key, eventsPerStream[0].Underlying);\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        public class MyAggregationMultiFunctionAggregationMethod : AggregationMultiFunctionAggregationMethodBase {\n" +
            "            public override object GetValue(int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {\n" +
            "                TrieAggState trie = (TrieAggState) row.GetAccessState(aggColNum);\n" +
            "                return trie.Trie;\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The aggregation method factory is responsible for producing an aggregation method for the \"trie\" view of the trie table column.\n" +
            "        /// </summary>\n" +
            "        public class TrieAggMethodFactoryTrieColumn : AggregationMultiFunctionAggregationMethodFactory {\n" +
            "            public AggregationMultiFunctionAggregationMethod NewMethod(AggregationMultiFunctionAggregationMethodFactoryContext context) {\n" +
            "                return new MyAggregationMultiFunctionAggregationMethod();\n" +
            "            }\n" +
            "            }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The aggregation method factory is responsible for producing an aggregation method for the \"prefixMap\" view of the trie table column.\n" +
            "        /// </summary>\n" +
            "        public class TrieAggMethodFactoryPrefixMap : AggregationMultiFunctionAggregationMethodFactory {\n" +
            "            public ExprEvaluator KeyExpression { get; set; }\n" +
            "            public AggregationMultiFunctionAggregationMethod NewMethod(AggregationMultiFunctionAggregationMethodFactoryContext context) {\n" +
            "                return new TrieAggMethodPrefixMap(this);\n" +
            "            }\n" +
            "        }\n" +
            "\n" +
            "        /// <summary>\n" +
            "        /// The aggregation method is responsible for the \"prefixMap\" view of the trie table column.\n" +
            "        /// </summary>\n" +
            "        public class TrieAggMethodPrefixMap : AggregationMultiFunctionAggregationMethodBase {\n" +
            "            private readonly TrieAggMethodFactoryPrefixMap factory;\n" +
            "\n" +
            "            public TrieAggMethodPrefixMap(TrieAggMethodFactoryPrefixMap factory) {\n" +
            "                this.factory = factory;\n" +
            "            }\n" +
            "\n" +
            "            public override object GetValue(int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {\n" +
            "                string key = (string) factory.KeyExpression.Evaluate(eventsPerStream, false, exprEvaluatorContext);\n" +
            "                TrieAggState trie = (TrieAggState) row.GetAccessState(aggColNum);\n" +
            "                return trie.Trie.PrefixMap(key);\n" +
            "            }\n" +
            "            }\n" +
            "        }\n" +
            "    }\n" +
            "\"\"\"\n";

        private class ClientExtendAggregationMFInlinedOneModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var ns = NamespaceGenerator.Create();
                var prefix = INLINEDCLASS_PREFIXMAP.Replace("${NAMESPACE}", ns);
                var epl = "@public @buseventtype create schema PersonEvent(name string, Id string);" +
                          $"create {prefix};\n" +
                          "@name('table') @public create table TableWithTrie(nameTrie trieState(string));\n" +
                          "@Priority(1) into table TableWithTrie select trieEnter(name) as nameTrie from PersonEvent;\n" +
                          "@Priority(0) @name('s0') select TableWithTrie.nameTrie.triePrefixMap(name) as c0 from PersonEvent;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                var p1 = MakeSendPerson(env, "Andreas", "P1");
                AssertReceived(env, CollectionUtil.BuildMap("Andreas", Collections.SingletonList(p1)));

                var p2 = MakeSendPerson(env, "Andras", "P2");
                AssertReceived(env, CollectionUtil.BuildMap("Andras", Collections.SingletonList(p2)));

                var p3 = MakeSendPerson(env, "Andras", "P3");
                AssertReceived(env, CollectionUtil.BuildMap("Andras", Arrays.AsList(p2, p3)));

                var p4 = MakeSendPerson(env, "And", "P4");
                AssertReceived(
                    env,
                    CollectionUtil.BuildMap(
                        "Andreas",
                        Collections.SingletonList(p1),
                        "Andras",
                        Arrays.AsList(p2, p3),
                        "And",
                        Collections.SingletonList(p4)));

                env.AssertThat(
                    () => {
                        var eplFAF = "select nameTrie as c0 from TableWithTrie";
                        var result = env.CompileExecuteFAF(eplFAF, path);
                        var trie = (SupportTrie<string, object>)result.Array[0].Get("c0");
                        Assert.AreEqual(3, trie.PrefixMap("And").Count);
                    });

                env.AssertIterator(
                    "table",
                    iterator => {
                        var trie = (SupportTrie<string, object>)env.GetEnumerator("table").Advance().Get("nameTrie");
                        Assert.AreEqual(3, trie.PrefixMap("And").Count);
                    });

                env.UndeployAll();
            }

            private void AssertReceived(
                RegressionEnvironment env,
                IDictionary<string, object> expected)
            {
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var received = (IDictionary<string, IList<IDictionary<string, object>>>)@event.Get("c0");
                        Assert.AreEqual(expected.Count, received.Count);
                        foreach (var expectedEntry in expected) {
                            var eventsExpected = (IList<IDictionary<string, object>>)expectedEntry.Value;
                            var eventsReceived = received.Get(expectedEntry.Key);
                            EPAssertionUtil.AssertEqualsAllowArray(
                                "failed to compare",
                                eventsExpected.ToArray(),
                                eventsReceived.ToArray());
                        }
                    });
            }
        }

        private class ClientExtendAggregationMFInlinedOtherModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplCreateInlined = "@name('clazz') @public create " + INLINEDCLASS_PREFIXMAP + ";\n";
                var path = new RegressionPath();
                env.Compile(eplCreateInlined, path);

                var epl = "@public @buseventtype create schema PersonEvent(name string, Id string);" +
                          "@name('table') create table TableWithTrie(nameTrie trieState(string));\n" +
                          "into table TableWithTrie select trieEnter(name) as nameTrie from PersonEvent;\n";
                var compiledTable = env.Compile(epl, path);

                env.CompileDeploy(eplCreateInlined);
                env.Deploy(compiledTable);

                MakeSendPerson(env, "Andreas", "P1");
                MakeSendPerson(env, "Andras", "P2");
                MakeSendPerson(env, "Andras", "P3");
                MakeSendPerson(env, "And", "P4");

                env.AssertIterator(
                    "table",
                    iterator => {
                        var trie = (SupportTrie<string, IList<object>>)iterator.Advance().Get("nameTrie");
                        Assert.AreEqual(3, trie.PrefixMap("And").Count);
                    });

                // assert dependencies
                SupportDeploymentDependencies.AssertSingle(
                    env,
                    "table",
                    "clazz",
                    EPObjectType.CLASSPROVIDED,
                    "TrieAggForge");

                env.UndeployAll();
            }
        }

        private static IDictionary<string, object> MakeSendPerson(
            RegressionEnvironment env,
            string name,
            string id)
        {
            var map = CollectionUtil.BuildMap("name", name, "Id", id);
            env.SendEventMap(map, "PersonEvent");
            return map;
        }
    }
} // end of namespace