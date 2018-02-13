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
using com.espertech.esper.core.service;
using com.espertech.esper.filter;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.fail;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.util
{
    public class SupportFilterHelper {
        public static void AssertFilterTwo(EPServiceProvider epService, string epl, string expressionOne, FilterOperator opOne, string expressionTwo, FilterOperator opTwo) {
            EPStatementSPI statementSPI = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            if (((FilterServiceSPI) statementSPI.StatementContext.FilterService).IsSupportsTakeApply) {
                FilterValueSetParam[] multi = GetFilterMulti(statementSPI);
                Assert.AreEqual(2, multi.Length);
                Assert.AreEqual(opOne, multi[0].FilterOperator);
                Assert.AreEqual(expressionOne, multi[0].Lookupable.Expression);
                Assert.AreEqual(opTwo, multi[1].FilterOperator);
                Assert.AreEqual(expressionTwo, multi[1].Lookupable.Expression);
            }
        }
    
        public static FilterValueSetParam GetFilterSingle(EPStatementSPI statementSPI) {
            FilterValueSetParam[] @params = GetFilterMulti(statementSPI);
            Assert.AreEqual(1, @params.Length);
            return @params[0];
        }
    
        public static FilterValueSetParam[] GetFilterMulti(EPStatementSPI statementSPI) {
            FilterServiceSPI filterServiceSPI = (FilterServiceSPI) statementSPI.StatementContext.FilterService;
            FilterSet set = filterServiceSPI.Take(Collections.Singleton(statementSPI.StatementId));
            Assert.AreEqual(1, set.Filters.Count);
            FilterValueSet valueSet = set.Filters[0].FilterValueSet;
            return ValueSet.Parameters[0];
        }
    
        public static EPStatement AssertFilterMulti(EPServiceProvider epService, string epl, string eventTypeName, SupportFilterItem[][] expected) {
            EPStatementSPI statementSPI = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            if (!((FilterServiceSPI) statementSPI.StatementContext.FilterService).IsSupportsTakeApply) {
                return statementSPI;
            }
            AssertFilterMulti(statementSPI, eventTypeName, expected);
            return statementSPI;
        }
    
        public static void AssertFilterMulti(EPStatementSPI statementSPI, string eventTypeName, SupportFilterItem[][] expected) {
            FilterServiceSPI filterServiceSPI = (FilterServiceSPI) statementSPI.StatementContext.FilterService;
            FilterSet set = filterServiceSPI.Take(Collections.Singleton(statementSPI.StatementId));
    
            FilterSetEntry filterSetEntry = null;
            foreach (FilterSetEntry entry in set.Filters) {
                if (entry.FilterValueSet.EventType.Name.Equals(eventTypeName)) {
                    if (filterSetEntry != null) {
                        Fail("Multiple filters for type " + eventTypeName);
                    }
                    filterSetEntry = entry;
                }
            }
    
            FilterValueSet valueSet = filterSetEntry.FilterValueSet;
            FilterValueSetParam[][] @params = valueSet.Parameters;
    
            var comparator = new Comparison<SupportFilterItem>(
                (o1, o2) => {
                    if (o1.Name.Equals(o2.Name)) {
                        if (o1.Op.Ordinal() > o1.Op.Ordinal()) {
                            return 1;
                        }
                        if (o1.Op.Ordinal() < o1.Op.Ordinal()) {
                            return -1;
                        }
                        return 0;
                    }
                    return o1.Name.CompareTo(o2.Name);
                });
    
            var found = new SupportFilterItem[@params.Length][];
            for (int i = 0; i < found.Length; i++) {
                found[i] = new SupportFilterItem[@params[i].Length];
                for (int j = 0; j < @params[i].Length; j++) {
                    found[i][j] = new SupportFilterItem(@params[i][j].Lookupable.Expression.ToString(),
                        @params[i][j].FilterOperator);
                }
                Arrays.Sort(found[i], comparator);
            }
    
            for (int i = 0; i < expected.Length; i++) {
                Arrays.Sort(expected[i], comparator);
            }
    
            EPAssertionUtil.AssertEqualsAnyOrder(expected, found);
            filterServiceSPI.Apply(set);
        }
    
    }
} // end of namespace
