///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;
using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestIndexHelper : AbstractRuntimeTest
    {
        [SetUp]
        public void SetUp()
        {
            lockFactory = new FilterServiceGranularLockFactoryReentrant(
                container.RWLockManager());

            eventType = SupportEventTypeFactory
                .GetInstance(container)
                .CreateBeanType(typeof(SupportBean));
            parameters = new ArrayDeque<FilterValueSetParam>();

            // Create parameter test list
            parameterOne = new FilterValueSetParamImpl(MakeLookupable("IntPrimitive"), FilterOperator.GREATER, 10);
            parameters.Add(parameterOne);
            parameterTwo = new FilterValueSetParamImpl(MakeLookupable("DoubleBoxed"), FilterOperator.GREATER, 20d);
            parameters.Add(parameterTwo);
            parameterThree = new FilterValueSetParamImpl(MakeLookupable("string"), FilterOperator.EQUAL, "sometext");
            parameters.Add(parameterThree);
        }

        private EventType eventType;
        private ArrayDeque<FilterValueSetParam> parameters;
        private FilterValueSetParam parameterOne;
        private FilterValueSetParam parameterTwo;
        private FilterValueSetParam parameterThree;
        private FilterServiceGranularLockFactory lockFactory;

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            SupportExprEventEvaluator eval = new SupportExprEventEvaluator(eventType.GetGetter(fieldName));
            return new ExprFilterSpecLookupable(fieldName, eval, null, eventType.GetPropertyType(fieldName), false, null);
        }

        [Test, RunInApplicationDomain]
        public void TestFindIndex()
        {
            IList<FilterParamIndexBase> indexes = new List<FilterParamIndexBase>();

            // Create index list wity index that doesn't match
            var indexOne = IndexFactory.CreateIndex(MakeLookupable("BoolPrimitive"), lockFactory, FilterOperator.EQUAL);
            indexes.Add(indexOne);
            Assert.IsTrue(IndexHelper.FindIndex(parameters, indexes) == null);

            // Create index list wity index that doesn't match
            indexOne = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), lockFactory, FilterOperator.GREATER_OR_EQUAL);
            indexes.Clear();
            indexes.Add(indexOne);
            Assert.IsTrue(IndexHelper.FindIndex(parameters, indexes) == null);

            // Add an index that does match a parameter
            var indexTwo = IndexFactory.CreateIndex(MakeLookupable("DoubleBoxed"), lockFactory, FilterOperator.GREATER);
            indexes.Add(indexTwo);
            var pair = IndexHelper.FindIndex(parameters, indexes);
            Assert.IsTrue(pair != null);
            Assert.AreEqual(parameterTwo, pair.First);
            Assert.AreEqual(indexTwo, pair.Second);

            // Add another index that does match a parameter, should return first match however which is doubleBoxed
            var indexThree = IndexFactory.CreateIndex(MakeLookupable("IntPrimitive"), lockFactory, FilterOperator.GREATER);
            indexes.Add(indexThree);
            pair = IndexHelper.FindIndex(parameters, indexes);
            Assert.AreEqual(parameterOne, pair.First);
            Assert.AreEqual(indexThree, pair.Second);

            // Try again removing one index
            indexes.Remove(indexTwo);
            pair = IndexHelper.FindIndex(parameters, indexes);
            Assert.AreEqual(parameterOne, pair.First);
            Assert.AreEqual(indexThree, pair.Second);
        }

        [Test, RunInApplicationDomain]
        public void TestFindParameter()
        {
            var indexOne = IndexFactory.CreateIndex(MakeLookupable("BoolPrimitive"), lockFactory, FilterOperator.EQUAL);
            Assert.IsNull(IndexHelper.FindParameter(parameters, indexOne));

            var indexTwo = IndexFactory.CreateIndex(MakeLookupable("string"), lockFactory, FilterOperator.EQUAL);
            Assert.AreEqual(parameterThree, IndexHelper.FindParameter(parameters, indexTwo));

            var indexThree = IndexFactory.CreateIndex(MakeLookupable("IntPrimitive"), lockFactory, FilterOperator.GREATER);
            Assert.AreEqual(parameterOne, IndexHelper.FindParameter(parameters, indexThree));
        }
    }
} // end of namespace
