///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.orderby
{
    public class ExecOrderBySimpleSortCollator : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Language.IsSortUsingCollator = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));

            string frenchForSin = "p\u00E9ch\u00E9";
            string frenchForFruit = "p\u00EAche";
            string[] sortedFrench = new string[] {
                frenchForFruit,
                frenchForSin
            };

            // CurrentCulture cannot be assigned in .NET 4.5 and below

#if NET46 || NET47 || NETSTANDARD2_0
            var englishUS = CultureInfo.GetCultureInfo("en-US");
            var englishUSComparer = StringComparer.Create(englishUS, false);

            CultureInfo.CurrentCulture = englishUS;

            var french = CultureInfo.GetCultureInfo("fr-FR");
            var frenchComparer = StringComparer.Create(french, false);

            Assert.AreEqual(1, frenchForFruit.CompareTo(frenchForSin));
            Assert.AreEqual(-1, frenchForSin.CompareTo(frenchForFruit));

            CultureInfo.CurrentCulture = french;

#if FALSE
            Assert.AreEqual(1, frenchForFruit.CompareTo(frenchForSin));
            Assert.AreEqual(-1, frenchComparer.Compare(frenchForFruit, frenchForSin));
            Assert.AreEqual(-1, frenchForSin.CompareTo(frenchForFruit));
            Assert.AreEqual(1, frenchComparer.Compare(frenchForSin, frenchForFruit));
            Assert.IsFalse(frenchForSin.Equals(frenchForFruit));
    
            Collections.Sort(items);
            Log.Info("Sorted default" + items);
    
            Collections.Sort(items, new ProxyComparator<string>() {
                Collator collator = Collator.GetInstance(Locale.FRANCE);
                public int Compare(string o1, string o2)
                {
                    return Collator.Compare(o1, o2);
                }
            });
            Log.Info("Sorted FR" + items);
#endif

            // test order by
            string stmtText = "select TheString from SupportBean#keepall order by TheString asc";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtText);
            epService.EPRuntime.SendEvent(new SupportBean(frenchForSin, 1));
            epService.EPRuntime.SendEvent(new SupportBean(frenchForFruit, 1));
            EPAssertionUtil.AssertPropsPerRow(stmtOne.GetEnumerator(), "TheString".Split(','), new object[][] {
                new object[] {sortedFrench[0]},
                new object[] {sortedFrench[1]}
            });
    
            // test sort view
            var listener = new SupportUpdateListener();
            stmtText = "select irstream TheString from SupportBean#sort(2, TheString asc)";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtText);
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(frenchForSin, 1));
            epService.EPRuntime.SendEvent(new SupportBean(frenchForFruit, 1));
            epService.EPRuntime.SendEvent(new SupportBean("abc", 1));
    
            Assert.AreEqual(frenchForSin, listener.LastOldData[0].Get("TheString"));
#endif
        }
    }
} // end of namespace
