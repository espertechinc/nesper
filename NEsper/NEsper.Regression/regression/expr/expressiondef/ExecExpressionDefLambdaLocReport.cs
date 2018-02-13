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

// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expressiondef
{
    public class ExecExpressionDefLambdaLocReport : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("LocationReport", typeof(LocationReport));
            configuration.AddImport(typeof(LRUtil));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            /// <summary>Regular algorithm to find separated luggage and new owner.</summary>
            LocationReport theEvent = LocationReportFactory.MakeLarge();
            List<Item> separatedLuggage = LocationReportFactory.FindSeparatedLuggage(theEvent);
    
            foreach (Item item in separatedLuggage) {
                //Log.Info("Luggage that are separated (dist>20): " + item);
                Item newOwner = LocationReportFactory.FindPotentialNewOwner(theEvent, item);
                //Log.Info("Found new owner " + newOwner);
            }
    
            string eplFragment = "" +
                    "expression lostLuggage {" +
                    "  lr => lr.items.Where(l => l.type='L' and " +
                    "    lr.items.Anyof(p => p.type='P' and p.assetId=l.assetIdPassenger and LRUtil.Distance(l.location.x, l.location.y, p.location.x, p.location.y) > 20))" +
                    "}" +
                    "expression passengers {" +
                    "  lr => lr.items.Where(l => l.type='P')" +
                    "}" +
                    "" +
                    "expression nearestOwner {" +
                    "  lr => LostLuggage(lr).ToMap(key => key.assetId, " +
                    "     value => Passengers(lr).MinBy(p => LRUtil.Distance(value.location.x, value.location.y, p.location.x, p.location.y)))" +
                    "}" +
                    "" +
                    "select LostLuggage(lr) as val1, NearestOwner(lr) as val2 from LocationReport lr";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
    
            LocationReport bean = LocationReportFactory.MakeLarge();
            epService.EPRuntime.SendEvent(bean);
    
            Item[] val1 = ItemArray((ICollection<Item>) listener.AssertOneGetNew().Get("val1"));
            Assert.AreEqual(3, val1.Length);
            Assert.AreEqual("L00000", val1[0].AssetId);
            Assert.AreEqual("L00007", val1[1].AssetId);
            Assert.AreEqual("L00008", val1[2].AssetId);
    
            Map val2 = (Map) listener.AssertOneGetNewAndReset().Get("val2");
            Assert.AreEqual(3, val2.Count);
            Assert.AreEqual("P00008", ((Item) val2.Get("L00000")).AssetId);
            Assert.AreEqual("P00001", ((Item) val2.Get("L00007")).AssetId);
            Assert.AreEqual("P00001", ((Item) val2.Get("L00008")).AssetId);
        }
    
        private Item[] ItemArray(ICollection<Item> it) {
            return It.ToArray(new Item[it.Count]);
        }
    }
} // end of namespace
