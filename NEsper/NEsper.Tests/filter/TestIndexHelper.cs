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
using com.espertech.esper.collection;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestIndexHelper 
    {
        private EventType _eventType;
        private LinkedList<FilterValueSetParam> _parameters;
        private FilterValueSetParam _parameterOne;
        private FilterValueSetParam _parameterTwo;
        private FilterValueSetParam _parameterThree;
        private FilterServiceGranularLockFactory _lockFactory;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _lockFactory = new FilterServiceGranularLockFactoryReentrant(_container.RWLockManager());
            _eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            _parameters = new LinkedList<FilterValueSetParam>();
    
            // Create parameter test list
            _parameterOne = new FilterValueSetParamImpl(MakeLookupable("IntPrimitive"), FilterOperator.GREATER, 10);
            _parameters.AddLast(_parameterOne);
            _parameterTwo = new FilterValueSetParamImpl(MakeLookupable("DoubleBoxed"), FilterOperator.GREATER, 20d);
            _parameters.AddLast(_parameterTwo);
            _parameterThree = new FilterValueSetParamImpl(MakeLookupable("TheString"), FilterOperator.EQUAL, "sometext");
            _parameters.AddLast(_parameterThree);
        }
    
        [Test]
        public void TestFindIndex()
        {
            List<FilterParamIndexBase> indexes = new List<FilterParamIndexBase>();
    
            // Create index list wity index that doesn't match
            FilterParamIndexBase indexOne = IndexFactory.CreateIndex(MakeLookupable("BoolPrimitive"), _lockFactory, FilterOperator.EQUAL);
            indexes.Add(indexOne);
            Assert.IsTrue(IndexHelper.FindIndex(_parameters, indexes) == null);
    
            // Create index list wity index that doesn't match
            indexOne = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), _lockFactory, FilterOperator.GREATER_OR_EQUAL);
            indexes.Clear();
            indexes.Add(indexOne);
            Assert.IsTrue(IndexHelper.FindIndex(_parameters, indexes) == null);
    
            // Add an index that does match a parameter
            FilterParamIndexBase indexTwo = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), _lockFactory, FilterOperator.GREATER);
            indexes.Add(indexTwo);
            Pair<FilterValueSetParam, FilterParamIndexBase> pair = IndexHelper.FindIndex(_parameters, indexes);
            Assert.IsTrue(pair != null);
            Assert.AreEqual(_parameterTwo, pair.First);
            Assert.AreEqual(indexTwo, pair.Second);
    
            // Add another index that does match a parameter, should return first match however which is doubleBoxed
            FilterParamIndexBase indexThree = IndexFactory.CreateIndex(MakeLookupable("IntPrimitive"), _lockFactory, FilterOperator.GREATER);
            indexes.Add(indexThree);
            pair = IndexHelper.FindIndex(_parameters, indexes);
            Assert.AreEqual(_parameterOne, pair.First);
            Assert.AreEqual(indexThree, pair.Second);
    
            // Try again removing one index
            indexes.Remove(indexTwo);
            pair = IndexHelper.FindIndex(_parameters, indexes);
            Assert.AreEqual(_parameterOne, pair.First);
            Assert.AreEqual(indexThree, pair.Second);
        }
    
        [Test]
        public void TestFindParameter()
        {
            FilterParamIndexBase indexOne = IndexFactory.CreateIndex(MakeLookupable("BoolPrimitive"), _lockFactory, FilterOperator.EQUAL);
            Assert.IsNull(IndexHelper.FindParameter(_parameters, indexOne));

            FilterParamIndexBase indexTwo = IndexFactory.CreateIndex(MakeLookupable("TheString"), _lockFactory, FilterOperator.EQUAL);
            Assert.AreEqual(_parameterThree, IndexHelper.FindParameter(_parameters, indexTwo));

            FilterParamIndexBase indexThree = IndexFactory.CreateIndex(MakeLookupable("IntPrimitive"), _lockFactory, FilterOperator.GREATER);
            Assert.AreEqual(_parameterOne, IndexHelper.FindParameter(_parameters, indexThree));
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName)
        {
            return new FilterSpecLookupable(fieldName, _eventType.GetGetter(fieldName), _eventType.GetPropertyType(fieldName), false);
        }
    }
}
