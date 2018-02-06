///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestIndexFactory 
    {
        private EventType _eventType;
        private FilterServiceGranularLockFactory _lockFactory;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            _lockFactory = new FilterServiceGranularLockFactoryReentrant(_container.RWLockManager());
        }

        [Test]
        public void TestCreateIndex()
        {
            // Create a "greater" index
            FilterParamIndexBase index = IndexFactory.CreateIndex(MakeLookupable("IntPrimitive"), _lockFactory, FilterOperator.GREATER);
    
            Assert.IsTrue(index != null);
            Assert.IsTrue(index is FilterParamIndexCompare);
            Assert.IsTrue(GetPropName(index).Equals("IntPrimitive"));
            Assert.IsTrue(index.FilterOperator == FilterOperator.GREATER);
    
            // Create an "equals" index
            index = IndexFactory.CreateIndex(MakeLookupable("TheString"), _lockFactory, FilterOperator.EQUAL);
    
            Assert.IsTrue(index != null);
            Assert.IsTrue(index is FilterParamIndexEquals);
            Assert.AreEqual(GetPropName(index), "TheString");
            Assert.IsTrue(index.FilterOperator == FilterOperator.EQUAL);
    
            // Create an "not equals" index
            index = IndexFactory.CreateIndex(MakeLookupable("TheString"), _lockFactory, FilterOperator.NOT_EQUAL);
    
            Assert.IsTrue(index != null);
            Assert.IsTrue(index is FilterParamIndexNotEquals);
            Assert.AreEqual(GetPropName(index), "TheString");
            Assert.IsTrue(index.FilterOperator == FilterOperator.NOT_EQUAL);
    
            // Create a range index
            index = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), _lockFactory, FilterOperator.RANGE_CLOSED);
            Assert.IsTrue(index is FilterParamIndexDoubleRange);
            index = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), _lockFactory, FilterOperator.NOT_RANGE_CLOSED);
            Assert.IsTrue(index is FilterParamIndexDoubleRangeInverted);
    
            // Create a in-index
            index = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), _lockFactory, FilterOperator.IN_LIST_OF_VALUES);
            Assert.IsTrue(index is FilterParamIndexIn);
            index = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), _lockFactory, FilterOperator.NOT_IN_LIST_OF_VALUES);
            Assert.IsTrue(index is FilterParamIndexNotIn);
    
            // Create a boolean-expression-index
            index = IndexFactory.CreateIndex(MakeLookupable("boolean"), _lockFactory, FilterOperator.BOOLEAN_EXPRESSION);
            Assert.IsTrue(index is FilterParamIndexBooleanExpr);
            index = IndexFactory.CreateIndex(MakeLookupable("boolean"), _lockFactory, FilterOperator.BOOLEAN_EXPRESSION);
            Assert.IsTrue(index is FilterParamIndexBooleanExpr);
        }
    
        private String GetPropName(FilterParamIndexBase index) {
            FilterParamIndexLookupableBase propIndex = (FilterParamIndexLookupableBase) index;
            return propIndex.Lookupable.Expression;
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _eventType.GetGetter(fieldName), _eventType.GetPropertyType(fieldName), false);
        }
    }
    
}
