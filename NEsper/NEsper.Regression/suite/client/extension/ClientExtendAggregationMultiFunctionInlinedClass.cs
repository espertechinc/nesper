///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;

using static com.espertech.esper.compat.collections.Collections;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
	public class ClientExtendAggregationMultiFunctionInlinedClass
	{
		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientExtendAggregationMFInlinedOneModule());
			execs.Add(new ClientExtendAggregationMFInlinedOtherModule());
			return execs;
		}

		const string INLINEDCLASS_PREFIXMAP = "inlined_class \"\"\"\n" +
		                                      "import com.espertech.esper.common.client.*;\n" +
		                                      "import com.espertech.esper.common.client.hook.aggmultifunc.*;\n" +
		                                      "import com.espertech.esper.common.client.hook.forgeinject.*;\n" +
		                                      "import com.espertech.esper.common.internal.epl.expression.core.*;\n" +
		                                      "import com.espertech.esper.common.internal.rettype.*;\n" +
		                                      "import com.espertech.esper.common.internal.epl.agg.core.*;\n" +
		                                      //
		                                      // For use with Apache Commons Collection 4:
		                                      //
		                                      //"import org.apache.commons.collections4.Trie;\n" +
		                                      //"import org.apache.commons.collections4.trie.PatriciaTrie;\n" +
		                                      "import com.espertech.esper.regressionlib.support.util.*;\n" +
		                                      "import java.util.*;\n" +
		                                      "import java.util.function.*;\n" +
		                                      "@ExtensionAggregationMultiFunction(names=\"trieState,trieEnter,triePrefixMap\")\n" +
		                                      "    /**\n" +
		                                      "     * The trie aggregation forge is the entry point for providing the multi-function aggregation.\n" +
		                                      "     * This example is compatible for use with tables.\n" +
		                                      "     */\n" +
		                                      "    public class TrieAggForge implements AggregationMultiFunctionForge {\n" +
		                                      "        public AggregationMultiFunctionHandler validateGetHandler(AggregationMultiFunctionValidationContext validationContext) {\n" +
		                                      "            String name = validationContext.getFunctionName();\n" +
		                                      "            if (name.equals(\"trieState\")) {\n" +
		                                      "                return new TrieAggHandlerTrieState();\n" +
		                                      "            } else if (name.equals(\"trieEnter\")) {\n" +
		                                      "                return new TrieAggHandlerTrieEnter(validationContext.getParameterExpressions());\n" +
		                                      "            } else if (name.equals(\"triePrefixMap\")) {\n" +
		                                      "                return new TrieAggHandlerTriePrefixMap();\n" +
		                                      "            }\n" +
		                                      "            throw new IllegalStateException(\"Unrecognized name '\" + name + \"' for use with trie\");\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * This handler handles the \"trieState\"-type table column\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggHandlerTrieState implements AggregationMultiFunctionHandler {\n" +
		                                      "            public EPType getReturnType() {\n" +
		                                      "                return EPTypeHelper.singleValue(SupportTrie.class);\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionStateKey getAggregationStateUniqueKey() {\n" +
		                                      "                return new AggregationMultiFunctionStateKey() {\n" +
		                                      "                };\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionStateMode getStateMode() {\n" +
		                                      "                InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(TrieAggStateFactory.class);\n" +
		                                      "                return new AggregationMultiFunctionStateModeManaged(injection);\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAccessorMode getAccessorMode() {\n" +
		                                      "                // accessor that returns the trie itself\n" +
		                                      "                InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(TrieAggAccessorFactory.class);\n" +
		                                      "                return new AggregationMultiFunctionAccessorModeManaged(injection);\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAgentMode getAgentMode() {\n" +
		                                      "                throw new UnsupportedOperationException(\"Trie aggregation access is only by the 'triePrefixMap' method\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAggregationMethodMode getAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx) {\n" +
		                                      "                throw new UnsupportedOperationException(\"Trie aggregation access is only by the 'triePrefixMap' method\");\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * This handler handles the \"trieEnter\"-operation that updates trie state\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggHandlerTrieEnter implements AggregationMultiFunctionHandler {\n" +
		                                      "            private final ExprNode[] parameters;\n" +
		                                      "\n" +
		                                      "            public TrieAggHandlerTrieEnter(ExprNode[] parameters) {\n" +
		                                      "                this.parameters = parameters;\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public EPType getReturnType() {\n" +
		                                      "                // We return null unless using \"prefixMap\"\n" +
		                                      "                return EPTypeHelper.singleValue(null);\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionStateKey getAggregationStateUniqueKey() {\n" +
		                                      "                throw new UnsupportedOperationException(\"Not a trie state\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionStateMode getStateMode() {\n" +
		                                      "                throw new UnsupportedOperationException(\"Not a trie state\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAccessorMode getAccessorMode() {\n" +
		                                      "                // accessor that returns the trie itself\n" +
		                                      "                InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(TrieAggAccessorFactory.class);\n" +
		                                      "                return new AggregationMultiFunctionAccessorModeManaged(injection);\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAgentMode getAgentMode() {\n" +
		                                      "                if (parameters.length != 1 || parameters[0].getForge().getEvaluationType() != String.class) {\n" +
		                                      "                    throw new IllegalArgumentException(\"Requires a single parameter returing a string value\");\n" +
		                                      "                }\n" +
		                                      "                InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(TrieAggAgentFactory.class);\n" +
		                                      "                injection.addExpression(\"keyExpression\", parameters[0]);\n" +
		                                      "                return new AggregationMultiFunctionAgentModeManaged(injection);\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAggregationMethodMode getAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx) {\n" +
		                                      "                throw new UnsupportedOperationException(\"Trie aggregation access is only by the 'triePrefixMap' method\");\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * This handler handles the \"prefixmap\" accessor for use with tables\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggHandlerTriePrefixMap implements AggregationMultiFunctionHandler {\n" +
		                                      "            public EPType getReturnType() {\n" +
		                                      "                return EPTypeHelper.singleValue(Map.class);\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionStateKey getAggregationStateUniqueKey() {\n" +
		                                      "                throw new UnsupportedOperationException(\"Not implemented for 'triePrefixMap' trie method\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionStateMode getStateMode() {\n" +
		                                      "                throw new UnsupportedOperationException(\"Not implemented for 'triePrefixMap' trie method\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAccessorMode getAccessorMode() {\n" +
		                                      "                throw new UnsupportedOperationException(\"Not implemented for 'triePrefixMap' trie method\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAgentMode getAgentMode() {\n" +
		                                      "                throw new UnsupportedOperationException(\"Not implemented for 'triePrefixMap' trie method\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAggregationMethodMode getAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx) {\n" +
		                                      "                if (ctx.getParameters().length != 1 || ctx.getParameters()[0].getForge().getEvaluationType() != String.class) {\n" +
		                                      "                    throw new IllegalArgumentException(\"Requires a single parameter returning a string value\");\n" +
		                                      "                }\n" +
		                                      "                InjectionStrategyClassNewInstance injection = new InjectionStrategyClassNewInstance(TrieAggMethodFactoryPrefixMap.class);\n" +
		                                      "                injection.addExpression(\"keyExpression\", ctx.getParameters()[0]);\n" +
		                                      "                return new AggregationMultiFunctionAggregationMethodModeManaged(injection);\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The agent state factory is responsible for producing a state holder that holds the trie state\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggStateFactory implements AggregationMultiFunctionStateFactory {\n" +
		                                      "            public AggregationMultiFunctionState newState(AggregationMultiFunctionStateFactoryContext ctx) {\n" +
		                                      "                return new TrieAggState();\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The agent state is the state holder that holds the trie state\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggState implements AggregationMultiFunctionState {\n" +
		                                      "            private final SupportTrie<String, List<Object>> trie = new SupportTrieSimpleStringKeyed<>();\n" +
		                                      "\n" +
		                                      "            public void applyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {\n" +
		                                      "                throw new UnsupportedOperationException(\"Not used since the agent updates the table\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public void applyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {\n" +
		                                      "                throw new UnsupportedOperationException(\"Not used since the agent updates the table\");\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public void clear() {\n" +
		                                      "                trie.clear();\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public void add(String key, Object underlying) {\n" +
		                                      "                List<Object> existing = (List<Object>) trie.get(key);\n" +
		                                      "                if (existing != null) {\n" +
		                                      "                    existing.add(underlying);\n" +
		                                      "                    return;\n" +
		                                      "                }\n" +
		                                      "                List<Object> events = new ArrayList<>(2);\n" +
		                                      "                events.add(underlying);\n" +
		                                      "                trie.put(key, events);\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public void remove(String key, Object underlying) {\n" +
		                                      "                List<Object> existing = (List<Object>) trie.get(key);\n" +
		                                      "                if (existing != null) {\n" +
		                                      "                    existing.remove(underlying);\n" +
		                                      "                    if (existing.isEmpty()) {\n" +
		                                      "                        trie.remove(key);\n" +
		                                      "                    }\n" +
		                                      "                }\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The accessor factory is responsible for producing an accessor that returns the result of the trie table column when accessed without an aggregation method\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggAccessorFactory implements AggregationMultiFunctionAccessorFactory {\n" +
		                                      "            public AggregationMultiFunctionAccessor newAccessor(AggregationMultiFunctionAccessorFactoryContext ctx) {\n" +
		                                      "                return new TrieAggAccessor();\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The accessor returns the result of the trie table column when accessed without an aggregation method\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggAccessor implements AggregationMultiFunctionAccessor {\n" +
		                                      "            // This is the value return when just referring to the trie table column by itself without a method name such as \"prefixMap\".\n" +
		                                      "            public Object getValue(AggregationMultiFunctionState state, EventBean[] eventsPerStream, boolean isNewData, ExprEvaluatorContext exprEvaluatorContext) {\n" +
		                                      "                TrieAggState trie = (TrieAggState) state;\n" +
		                                      "                return trie.trie;\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The agent factory is responsible for producing an agent that handles all changes to the trie table column.\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggAgentFactory implements AggregationMultiFunctionAgentFactory {\n" +
		                                      "            private ExprEvaluator keyExpression;\n" +
		                                      "\n" +
		                                      "            public void setKeyExpression(ExprEvaluator keyExpression) {\n" +
		                                      "                this.keyExpression = keyExpression;\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAgent newAgent(AggregationMultiFunctionAgentFactoryContext ctx) {\n" +
		                                      "                return new TrieAggAgent(this);\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The agent is responsible for all changes to the trie table column.\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggAgent implements AggregationMultiFunctionAgent {\n" +
		                                      "            private final TrieAggAgentFactory factory;\n" +
		                                      "\n" +
		                                      "            public TrieAggAgent(TrieAggAgentFactory factory) {\n" +
		                                      "                this.factory = factory;\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public void applyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationRow row, int column) {\n" +
		                                      "                String key = (String) factory.keyExpression.evaluate(eventsPerStream, true, exprEvaluatorContext);\n" +
		                                      "                TrieAggState trie = (TrieAggState) row.getAccessState(column);\n" +
		                                      "                trie.add(key, eventsPerStream[0].getUnderlying());\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public void applyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationRow row, int column) {\n" +
		                                      "                String key = (String) factory.keyExpression.evaluate(eventsPerStream, false, exprEvaluatorContext);\n" +
		                                      "                TrieAggState trie = (TrieAggState) row.getAccessState(column);\n" +
		                                      "                trie.remove(key, eventsPerStream[0].getUnderlying());\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The aggregation method factory is responsible for producing an aggregation method for the \"trie\" view of the trie table column.\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggMethodFactoryTrieColumn implements AggregationMultiFunctionAggregationMethodFactory {\n" +
		                                      "            public AggregationMultiFunctionAggregationMethod newMethod(AggregationMultiFunctionAggregationMethodFactoryContext context) {\n" +
		                                      "                return new AggregationMultiFunctionAggregationMethod() {\n" +
		                                      "                    public Object getValue(int aggColNum, AggregationRow row, EventBean[] eventsPerStream, boolean isNewData, ExprEvaluatorContext exprEvaluatorContext) {\n" +
		                                      "                        TrieAggState trie = (TrieAggState) row.getAccessState(aggColNum);\n" +
		                                      "                        return trie.trie;\n" +
		                                      "                    }\n" +
		                                      "                };\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The aggregation method factory is responsible for producing an aggregation method for the \"prefixMap\" view of the trie table column.\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggMethodFactoryPrefixMap implements AggregationMultiFunctionAggregationMethodFactory {\n" +
		                                      "            private ExprEvaluator keyExpression;\n" +
		                                      "\n" +
		                                      "            public void setKeyExpression(ExprEvaluator keyExpression) {\n" +
		                                      "                this.keyExpression = keyExpression;\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public AggregationMultiFunctionAggregationMethod newMethod(AggregationMultiFunctionAggregationMethodFactoryContext context) {\n" +
		                                      "                return new TrieAggMethodPrefixMap(this);\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "\n" +
		                                      "        /**\n" +
		                                      "         * The aggregation method is responsible for the \"prefixMap\" view of the trie table column.\n" +
		                                      "         */\n" +
		                                      "        public class TrieAggMethodPrefixMap implements AggregationMultiFunctionAggregationMethod {\n" +
		                                      "            private final TrieAggMethodFactoryPrefixMap factory;\n" +
		                                      "\n" +
		                                      "            public TrieAggMethodPrefixMap(TrieAggMethodFactoryPrefixMap factory) {\n" +
		                                      "                this.factory = factory;\n" +
		                                      "            }\n" +
		                                      "\n" +
		                                      "            public Object getValue(int aggColNum, AggregationRow row, EventBean[] eventsPerStream, boolean isNewData, ExprEvaluatorContext exprEvaluatorContext) {\n" +
		                                      "                String key = (String) factory.keyExpression.evaluate(eventsPerStream, false, exprEvaluatorContext);\n" +
		                                      "                TrieAggState trie = (TrieAggState) row.getAccessState(aggColNum);\n" +
		                                      "                return trie.trie.prefixMap(key);\n" +
		                                      "            }\n" +
		                                      "        }\n" +
		                                      "    }\n" +
		                                      "\"\"\"\n";

		private class ClientExtendAggregationMFInlinedOneModule : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@public @buseventtype create schema PersonEvent(name string, id string);" +
				          "create " +
				          INLINEDCLASS_PREFIXMAP +
				          ";\n" +
				          "@name('table') create table TableWithTrie(nameTrie trieState(string));\n" +
				          "@Priority(1) into table TableWithTrie select trieEnter(name) as nameTrie from PersonEvent;\n" +
				          "@Priority(0) @name('s0') select TableWithTrie.nameTrie.triePrefixMap(name) as c0 from PersonEvent;\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				var p1 = MakeSendPerson(env, "Andreas", "P1");
				AssertReceived(env, CollectionUtil.BuildMap("Andreas", SingletonList(p1)));

				var p2 = MakeSendPerson(env, "Andras", "P2");
				AssertReceived(env, CollectionUtil.BuildMap("Andras", SingletonList(p2)));

				var p3 = MakeSendPerson(env, "Andras", "P3");
				AssertReceived(env, CollectionUtil.BuildMap("Andras", Arrays.AsList(p2, p3)));

				var p4 = MakeSendPerson(env, "And", "P4");
				AssertReceived(env, CollectionUtil.BuildMap("Andreas", SingletonList(p1), "Andras", Arrays.AsList(p2, p3), "And", SingletonList(p4)));

				var eplFAF = "select nameTrie as c0 from TableWithTrie";
				var result = env.CompileExecuteFAF(eplFAF, path);
				var trie = (SupportTrie<string, object>) result.Array[0].Get("c0");
				Assert.AreEqual(3, trie.PrefixMap("And").Count);

				trie = (SupportTrie<string, object>) env.GetEnumerator("table").Advance().Get("nameTrie");
				Assert.AreEqual(3, trie.PrefixMap("And").Count);

				env.UndeployAll();
			}

			private void AssertReceived(
				RegressionEnvironment env,
				IDictionary<string, object> expected)
			{
				var received =
					(IDictionary<string, IList<IDictionary<string, object>>>) env.Listener("s0").AssertOneGetNewAndReset().Get("c0");
				Assert.AreEqual(expected.Count, received.Count);
				foreach (var expectedEntry in expected) {
					var eventsExpected = (IList<IDictionary<string, object>>) expectedEntry.Value;
					var eventsReceived = received.Get(expectedEntry.Key);
					EPAssertionUtil.AssertEqualsAllowArray("failed to compare", eventsExpected.ToArray(), eventsReceived.ToArray());
				}
			}
		}

		private class ClientExtendAggregationMFInlinedOtherModule : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string eplCreateInlined = "@name('clazz') @public create " + INLINEDCLASS_PREFIXMAP + ";\n";
				RegressionPath path = new RegressionPath();
				env.Compile(eplCreateInlined, path);

				string epl = "@public @buseventtype create schema PersonEvent(name string, id string);" +
				             "@name('table') create table TableWithTrie(nameTrie trieState(string));\n" +
				             "into table TableWithTrie select trieEnter(name) as nameTrie from PersonEvent;\n";
				EPCompiled compiledTable = env.Compile(epl, path);

				env.CompileDeploy(eplCreateInlined);
				env.Deploy(compiledTable);

				MakeSendPerson(env, "Andreas", "P1");
				MakeSendPerson(env, "Andras", "P2");
				MakeSendPerson(env, "Andras", "P3");
				MakeSendPerson(env, "And", "P4");

				SupportTrie<string, IList<object>> trie = (SupportTrie<string, IList<object>>) env.GetEnumerator("table").Advance().Get("nameTrie");
				Assert.AreEqual(3, trie.PrefixMap("And").Count);

				// assert dependencies
				SupportDeploymentDependencies.AssertSingle(env, "table", "clazz", EPObjectType.CLASSPROVIDED, "TrieAggForge");

				env.UndeployAll();
			}
		}

		private static IDictionary<string, object> MakeSendPerson(
			RegressionEnvironment env,
			string name,
			string id)
		{
			IDictionary<string, object> map = CollectionUtil.BuildMap("name", name, "id", id);
			env.SendEventMap(map, "PersonEvent");
			return map;
		}
	}
} // end of namespace
