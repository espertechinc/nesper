///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.patternassert
{
    public class EventExpressionCase
    {
        public EventExpressionCase(string expressionText)
        {
            ExpressionText = expressionText;
            ExpectedResults = new Dictionary<string, IList<EventDescriptor>>();
        }

        public EventExpressionCase(EPStatementObjectModel objectModel)
        {
            ObjectModel = objectModel;
            ExpectedResults = new Dictionary<string, IList<EventDescriptor>>();
        }

        public string ExpressionText { get; }

        public EPStatementObjectModel ObjectModel { get; }

        public IDictionary<string, IList<EventDescriptor>> ExpectedResults { get; }

        public void Add(string expectedOnEventId)
        {
            AddDesc(expectedOnEventId);
        }

        public void Add(
            string expectedOnEventId,
            string tag,
            object bean)
        {
            var desc = AddDesc(expectedOnEventId);
            desc.Put(tag, bean);
        }

        public void Add(
            string expectedOnEventId,
            string tagOne,
            object beanOne,
            string tagTwo,
            object beanTwo)
        {
            var desc = AddDesc(expectedOnEventId);
            desc.Put(tagOne, beanOne);
            desc.Put(tagTwo, beanTwo);
        }

        public void Add(
            string expectedOnEventId,
            string tagOne,
            object beanOne,
            string tagTwo,
            object beanTwo,
            string tagThree,
            object beanThree)
        {
            var desc = AddDesc(expectedOnEventId);
            desc.Put(tagOne, beanOne);
            desc.Put(tagTwo, beanTwo);
            desc.Put(tagThree, beanThree);
        }

        public void Add(
            string expectedOnEventId,
            string tagOne,
            object beanOne,
            string tagTwo,
            object beanTwo,
            string tagThree,
            object beanThree,
            string tagFour,
            object beanFour)
        {
            var desc = AddDesc(expectedOnEventId);
            desc.Put(tagOne, beanOne);
            desc.Put(tagTwo, beanTwo);
            desc.Put(tagThree, beanThree);
            desc.Put(tagFour, beanFour);
        }

        public void Add(
            string expectedOnEventId,
            object[][] tagsAndBeans)
        {
            var desc = AddDesc(expectedOnEventId);
            for (var i = 0; i < tagsAndBeans.Length; i++) {
                desc.Put((string) tagsAndBeans[i][0], tagsAndBeans[i][1]);
            }
        }

        private EventDescriptor AddDesc(string expectedOnEventId)
        {
            var resultList = ExpectedResults.Get(expectedOnEventId);
            if (resultList == null) {
                resultList = new List<EventDescriptor>();
                ExpectedResults.Put(expectedOnEventId, resultList);
            }

            var eventDesc = new EventDescriptor();
            resultList.Add(eventDesc);
            return eventDesc;
        }
    }
} // end of namespace