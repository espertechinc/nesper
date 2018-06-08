///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    using Map = IDictionary<object, object>;
    using Collection = ICollection<object>;

    public class ExecEnumGroupBy : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }

        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "extractAfterUnderscore", GetType(), "ExtractAfterUnderscore");

            RunAssertionKeySelectorOnly(epService);
            RunAssertionKeyValueSelector(epService);
        }

        private void RunAssertionKeySelectorOnly(EPServiceProvider epService)
        {
            // - duplicate key allowed, creates a list of values
            // - null key & value allowed

            var eplFragment = "select Contained.GroupBy(c => id) as val from Bean";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val".Split(','), new[] {
                typeof(IDictionary<object, ICollection<object>>)
            });
            var extractorEvents = new EPAssertionUtil.ProxyAssertionCollectionValueString
            {
                ProcExtractValue = collectionItem =>
                {
                    var p00 = ((SupportBean_ST0) collectionItem).P00;
                    return Convert.ToString(p00);
                }
            };

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E1,2", "E2,5"));
            EPAssertionUtil.AssertMapOfCollection(
                (IDictionary<object, ICollection<object>>) listener.AssertOneGetNewAndReset().Get("val"),
                "E1,E2".Split(','), new[] {"1,2", "5"},
                extractorEvents);

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val"));

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            Assert.AreEqual(0, listener.AssertOneGetNewAndReset().Get("val").UnwrapEnumerable<object>().Count());
            stmtFragment.Dispose();

            // test scalar
            var eplScalar = "select Strvals.GroupBy(c => extractAfterUnderscore(c)) as val from SupportCollection";
            var stmtScalar = epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtScalar.EventType, "val".Split(','), new[] {
                typeof(IDictionary<object, ICollection<object>>)
            });

            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1_2,E2_1,E3_2"));
            EPAssertionUtil.AssertMapOfCollection(
                listener.AssertOneGetNewAndReset().Get("val").UnwrapStringDictionary(),
                "2,1".Split(','),
                new[] {"E1_2,E3_2", "E2_1"}, GetExtractorScalar());

            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val"));

            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            Assert.AreEqual(0, listener.AssertOneGetNewAndReset().Get("val").Unwrap<object>().Count);

            stmtScalar.Dispose();
        }

        private void RunAssertionKeyValueSelector(EPServiceProvider epService)
        {
            var eplFragment = "select Contained.GroupBy(k => id, v => p00) as val from Bean";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            var extractor = new EPAssertionUtil.ProxyAssertionCollectionValueString
            {
                ProcExtractValue = collectionItem =>
                {
                    var p00 = collectionItem.AsInt();
                    return Convert.ToString(p00);
                }
            };

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E1,2", "E2,5"));
            EPAssertionUtil.AssertMapOfCollection(
                listener.AssertOneGetNewAndReset().Get("val").UnwrapStringDictionary(),
                "E1,E2".Split(','),
                new[] {"1,2", "5"}, extractor);

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val"));

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            Assert.AreEqual(0, listener.AssertOneGetNewAndReset().Get("val").Unwrap<object>().Count);

            // test scalar
            var eplScalar =
                "select Strvals.GroupBy(k => extractAfterUnderscore(k), v => v) as val from SupportCollection";
            var stmtScalar = epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtScalar.EventType, "val".Split(','), new[] {
                typeof(IDictionary<object, ICollection<object>>)
            });

            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1_2,E2_1,E3_2"));
            EPAssertionUtil.AssertMapOfCollection(
                listener.AssertOneGetNewAndReset().Get("val").UnwrapStringDictionary(),
                "2,1".Split(','),
                new[] {"E1_2,E3_2", "E2_1"}, GetExtractorScalar());

            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val"));

            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            Assert.AreEqual(0, listener.AssertOneGetNewAndReset().Get("val").Unwrap<object>().Count);

            stmtScalar.Dispose();
        }

        public static string ExtractAfterUnderscore(string @string)
        {
            var indexUnderscore = @string.IndexOf("_");
            if (indexUnderscore == -1)
            {
                Assert.Fail();
            }

            return @string.Substring(indexUnderscore + 1);
        }

        private static EPAssertionUtil.AssertionCollectionValueString GetExtractorScalar()
        {
            return new EPAssertionUtil.ProxyAssertionCollectionValueString
            {
                ProcExtractValue = collectionItem => collectionItem.ToString()
            };
        }
    }
} // end of namespace