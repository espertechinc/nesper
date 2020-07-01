///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestIndexFactory : AbstractRuntimeTest
    {
        private EventType eventType;
        private FilterServiceGranularLockFactory lockFactory;

        [SetUp]
        public void SetUp()
        {
            lockFactory = new FilterServiceGranularLockFactoryReentrant(
                container.RWLockManager());

            eventType = SupportEventTypeFactory
                .GetInstance(container)
                .CreateBeanType(typeof(SupportBean));
        }

        [Test, RunInApplicationDomain]
        public void TestCreateIndex()
        {
            // Create a "greater" index
            FilterParamIndexBase index = IndexFactory.CreateIndex(MakeLookupable("IntPrimitive"), lockFactory, FilterOperator.GREATER);

            Assert.IsTrue(index != null);
            Assert.IsTrue(index is FilterParamIndexCompare);
            Assert.IsTrue(GetPropName(index).Equals("IntPrimitive"));
            Assert.IsTrue(index.FilterOperator == FilterOperator.GREATER);

            // Create an "Equals" index
            index = IndexFactory.CreateIndex(MakeLookupable("string"), lockFactory, FilterOperator.EQUAL);

            Assert.IsTrue(index != null);
            Assert.IsTrue(index is FilterParamIndexEquals);
            Assert.IsTrue(GetPropName(index).Equals("string"));
            Assert.IsTrue(index.FilterOperator == FilterOperator.EQUAL);

            // Create an "not equals" index
            index = IndexFactory.CreateIndex(MakeLookupable("string"), lockFactory, FilterOperator.NOT_EQUAL);

            Assert.IsTrue(index != null);
            Assert.IsTrue(index is FilterParamIndexNotEquals);
            Assert.IsTrue(GetPropName(index).Equals("string"));
            Assert.IsTrue(index.FilterOperator == FilterOperator.NOT_EQUAL);

            // Create a range index
            index = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), lockFactory, FilterOperator.RANGE_CLOSED);
            Assert.IsTrue(index is FilterParamIndexDoubleRange);
            index = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), lockFactory, FilterOperator.NOT_RANGE_CLOSED);
            Assert.IsTrue(index is FilterParamIndexDoubleRangeInverted);

            // Create a in-index
            index = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), lockFactory, FilterOperator.IN_LIST_OF_VALUES);
            Assert.IsTrue(index is FilterParamIndexIn);
            index = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), lockFactory, FilterOperator.NOT_IN_LIST_OF_VALUES);
            Assert.IsTrue(index is FilterParamIndexNotIn);

            // Create a boolean-expression-index
            index = IndexFactory.CreateIndex(MakeLookupable("boolean"), lockFactory, FilterOperator.BOOLEAN_EXPRESSION);
            Assert.IsTrue(index is FilterParamIndexBooleanExpr);
            index = IndexFactory.CreateIndex(MakeLookupable("boolean"), lockFactory, FilterOperator.BOOLEAN_EXPRESSION);
            Assert.IsTrue(index is FilterParamIndexBooleanExpr);
        }

        private string GetPropName(FilterParamIndexBase index)
        {
            FilterParamIndexLookupableBase propIndex = (FilterParamIndexLookupableBase) index;
            return propIndex.Lookupable.Expression;
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            SupportExprEventEvaluator eval = new SupportExprEventEvaluator(eventType.GetGetter(fieldName));
            return new ExprFilterSpecLookupable(fieldName, eval, null, eventType.GetPropertyType(fieldName), false, null);
        }
    }
} // end of namespace
