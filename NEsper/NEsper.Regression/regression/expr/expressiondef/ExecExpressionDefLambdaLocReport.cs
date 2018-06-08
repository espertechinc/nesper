///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean.lrreport;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expressiondef
{
    using Map = IDictionary<string, object>;

    public class ExecExpressionDefLambdaLocReport : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("LocationReport", typeof(LocationReport));
            configuration.AddImport(typeof(LRUtil));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            /// <summary>Regular algorithm to find separated luggage and new owner.</summary>
            var theEvent = LocationReportFactory.MakeLarge();
            var separatedLuggage = LocationReportFactory.FindSeparatedLuggage(theEvent);
    
            foreach (var item in separatedLuggage) {
                //Log.Info("Luggage that are separated (dist>20): " + item);
                var newOwner = LocationReportFactory.FindPotentialNewOwner(theEvent, item);
                //Log.Info("Found new owner " + newOwner);
            }
    
            var eplFragment = "" +
                    "expression lostLuggage {" +
                    "  lr => lr.items.where(l => l.type='L' and " +
                    "    lr.items.anyof(p => p.type='P' and p.assetId=l.assetIdPassenger and LRUtil.Distance(l.location.x, l.location.y, p.location.x, p.location.y) > 20))" +
                    "}" +
                    "expression passengers {" +
                    "  lr => lr.items.where(l => l.type='P')" +
                    "}" +
                    "" +
                    "expression nearestOwner {" +
                    "  lr => lostLuggage(lr).toMap(key => key.assetId, " +
                    "     value => passengers(lr).minBy(p => LRUtil.Distance(value.location.x, value.location.y, p.location.x, p.location.y)))" +
                    "}" +
                    "" +
                    "select lostLuggage(lr) as val1, nearestOwner(lr) as val2 from LocationReport lr";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
    
            var bean = LocationReportFactory.MakeLarge();
            epService.EPRuntime.SendEvent(bean);
    
            var val1 = listener.AssertOneGetNew().Get("val1").UnwrapIntoArray<Item>();
            Assert.AreEqual(3, val1.Length);
            Assert.AreEqual("L00000", val1[0].AssetId);
            Assert.AreEqual("L00007", val1[1].AssetId);
            Assert.AreEqual("L00008", val1[2].AssetId);
    
            var val2 = listener.AssertOneGetNewAndReset().Get("val2").UnwrapDictionary();
            Assert.AreEqual(3, val2.Count);
            Assert.AreEqual("P00008", ((Item) val2.Get("L00000")).AssetId);
            Assert.AreEqual("P00001", ((Item) val2.Get("L00007")).AssetId);
            Assert.AreEqual("P00001", ((Item) val2.Get("L00008")).AssetId);
        }
    }
} // end of namespace
