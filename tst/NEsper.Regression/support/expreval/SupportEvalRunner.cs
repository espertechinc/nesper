///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.expreval
{
	public class SupportEvalRunner {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(SupportEvalRunner));

	    public static SupportEvalBuilderResult Run(RegressionEnvironment env, bool soda, SupportEvalBuilder builder) {
	        VerifyAssertions(builder);
	        SupportEvalBuilderResult result = null;
	        if (!builder.IsExcludeEPLAssertion) {
	            result = RunEPL(env, builder, soda);
	        }
	        env.AssertThat(() => RunNonCompile(env, builder));
	        return result;
	    }

	    private static void VerifyAssertions(SupportEvalBuilder builder) {
	        foreach (var assertion in builder.Assertions) {
	            var expected = assertion.Builder.Results;
	            foreach (var expression in builder.Expressions) {
	                if (!expected.ContainsKey(expression.Key)) {
	                    throw new IllegalStateException("No expected value for expression '" + expression.Key + "'");
	                }
	            }
	        }
	    }

	    private static void RunNonCompile(RegressionEnvironment env, SupportEvalBuilder builder) {
	        var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(builder.EventType);
	        if (eventType == null) {
	            throw new ArgumentException("Cannot find preconfigured event type '" + builder.EventType + "'");
	        }
	        var typesPerStream = new EventType[]{eventType};
	        var typeAliases = new string[] {
		        builder.StreamAlias ?? "somealias"
	        };

	        IDictionary<string, ExprEvaluator> nodes = new Dictionary<string, ExprEvaluator>();
	        foreach (var entry in builder.Expressions) {
	            if (builder.ExcludeNamesExcept != null && !builder.ExcludeNamesExcept.Equals(entry.Key)) {
	                continue;
	            }
	            var node = ((EPRuntimeSPI) env.Runtime).ReflectiveCompileSvc.ReflectiveCompileExpression(entry.Value, typesPerStream, typeAliases);
	            var eval = node.Forge.ExprEvaluator;
	            nodes.Put(entry.Key, eval);
	        }

	        var count = -1;
	        foreach (var assertion in builder.Assertions) {
	            count++;
	            if (builder.ExcludeAssertionsExcept != null && count != builder.ExcludeAssertionsExcept) {
	                continue;
	            }
	            RunNonCompileAssertion(count, eventType, nodes, assertion, env, builder);
	        }
	    }

	    private static SupportEvalBuilderResult RunEPL(RegressionEnvironment env, SupportEvalBuilder builder, bool soda) {
	        var epl = new StringBuilder();
	        epl.Append("@name('s0') select ");

	        var delimiter = "";
	        foreach (var entry in builder.Expressions) {
	            if (builder.ExcludeNamesExcept != null && !builder.ExcludeNamesExcept.Equals(entry.Key)) {
	                continue;
	            }
	            epl.Append(delimiter);
	            epl.Append(entry.Value);
	            if (!entry.Value.Equals(entry.Key)) {
	                epl.Append(" as ").Append(entry.Key);
	            }
	            delimiter = ", ";
	        }

	        epl.Append(" from ").Append(builder.EventType);
	        if (builder.StreamAlias != null) {
	            epl.Append(" as ").Append(builder.StreamAlias);
	        }
	        var eplText = epl.ToString();
	        if (builder.IsLogging) {
	            Log.Info("EPL: " + eplText);
	        }

	        if (builder.Path != null) {
	            env.CompileDeploy(soda, eplText, builder.Path).AddListener("s0");
	        } else {
	            env.CompileDeploy(soda, eplText).AddListener("s0");
	        }

	        if (builder.StatementConsumer != null && builder.ExcludeAssertionsExcept == null && builder.ExcludeNamesExcept == null) {
	            env.AssertStatement("s0", statement => builder.StatementConsumer.Invoke(statement));
	        }

	        var count = -1;
	        foreach (var assertion in builder.Assertions) {
	            count++;
	            if (builder.ExcludeAssertionsExcept != null && count != builder.ExcludeAssertionsExcept) {
	                continue;
	            }
	            RunEPLAssertion(count, builder.EventType, env, assertion, builder);
	        }

	        env.UndeployModuleContaining("s0");
	        return new SupportEvalBuilderResult(eplText);
	    }

	    private static void RunEPLAssertion(int assertionNumber, string eventType, RegressionEnvironment env, SupportEvalAssertionPair assertion, SupportEvalBuilder builder) {
	        if (assertion.Underlying is IDictionary<string, object> underlying) {
	            env.SendEventMap(underlying, eventType);
	            if (builder.IsLogging) {
	                Log.Info("Sending event: " + underlying);
	            }
	        } else {
	            env.SendEventBean(assertion.Underlying);
	            if (builder.IsLogging) {
	                Log.Info("Sending event: " + assertion.Underlying);
	            }
	        }

	        env.AssertEventNew("s0", @event => {
	            if (builder.IsLogging) {
	                Log.Info("Received event: " + EventBeanUtility.PrintEvent(@event));
	            }
	            foreach (var expected in assertion.Builder.Results) {
	                var name = expected.Key;
	                if (builder.ExcludeNamesExcept != null && !builder.ExcludeNamesExcept.Equals(name)) {
	                    continue;
	                }
	                var actual = @event.Get(name);
	                DoAssert(true, assertionNumber, expected.Key, expected.Value, actual);
	            }
	        });
	    }

	    private static void RunNonCompileAssertion(int assertionNumber, EventType eventType, IDictionary<string, ExprEvaluator> nodes, SupportEvalAssertionPair assertion, RegressionEnvironment env, SupportEvalBuilder builder) {
	        EventBean @event;
	        if (assertion.Underlying is IDictionary<string, object> underlyingDictionary) {
	            @event = new MapEventBean(underlyingDictionary, eventType);
	        } else {
	            if (eventType.UnderlyingType != assertion.Underlying) {
	                eventType = GetSubtype(assertion.Underlying, env);
	            }
	            @event = new BeanEventBean(assertion.Underlying, eventType);
	        }
	        var eventsPerStream = new EventBean[]{@event};

	        foreach (var expected in assertion.Builder.Results) {
	            if (builder.ExcludeNamesExcept != null && !builder.ExcludeNamesExcept.Equals(expected.Key)) {
	                continue;
	            }

	            var eval = nodes.Get(expected.Key);
	            object result = null;
	            try {
	                result = eval.Evaluate(eventsPerStream, true, null);
	            } catch (Exception ex) {
	                Log.Error("Failed at expression " + expected.Key + " at event #" + assertionNumber, ex);
	                Assert.Fail();
	            }
	            DoAssert(false, assertionNumber, expected.Key, expected.Value, result);
	        }
	    }

	    private static EventType GetSubtype(object underlying, RegressionEnvironment env) {
	        var type = env.Runtime.EventTypeService.GetEventTypePreconfigured(underlying.GetType().Name);
	        if (type == null) {
	            Assert.Fail("Cannot find type '" + underlying.GetType().Name + "'");
	        }
	        return type;
	    }

	    private static void DoAssert(bool epl, int assertionNumber, string columnName, SupportEvalExpected expected, object actual) {
	        var message = epl ? "For EPL assertion" : "For Eval assertion";
	        message += ", failed to assert property '" + columnName + "' for event #" + assertionNumber;
	        expected.AssertValue(message, actual);
	    }
	}
} // end of namespace
