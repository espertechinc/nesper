///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.regression.support
{
    public class EventExpressionCase
    {
        private String expressionText;
        private EPStatementObjectModel objectModel;
        private LinkedHashMap<String, LinkedList<EventDescriptor>> expectedResults;
    
        public EventExpressionCase(String expressionText)
        {
            this.expressionText = expressionText;
            this.expectedResults = new LinkedHashMap<String, LinkedList<EventDescriptor>>();
        }
    
        public EventExpressionCase(EPStatementObjectModel objectModel)
        {
            this.objectModel = objectModel;
            this.expectedResults = new LinkedHashMap<String, LinkedList<EventDescriptor>>();
        }

        public string ExpressionText
        {
            get { return expressionText; }
        }

        public EPStatementObjectModel ObjectModel
        {
            get { return objectModel; }
        }

        public LinkedHashMap<string, LinkedList<EventDescriptor>> ExpectedResults
        {
            get { return expectedResults; }
        }

        public void Add(String expectedOnEventId)
        {
            AddDesc(expectedOnEventId);
        }
    
        public void Add(String expectedOnEventId, String tag, Object bean)
        {
            EventDescriptor desc = AddDesc(expectedOnEventId);
            desc.Put(tag, bean);
        }
    
        public void Add(String expectedOnEventId, String tagOne, Object beanOne,
                        String tagTwo, Object beanTwo)
        {
            EventDescriptor desc = AddDesc(expectedOnEventId);
            desc.Put(tagOne, beanOne);
            desc.Put(tagTwo, beanTwo);
        }
    
        public void Add(String expectedOnEventId, String tagOne, Object beanOne,
                        String tagTwo, Object beanTwo,
                        String tagThree, Object beanThree)
        {
            EventDescriptor desc = AddDesc(expectedOnEventId);
            desc.Put(tagOne, beanOne);
            desc.Put(tagTwo, beanTwo);
            desc.Put(tagThree, beanThree);
        }
    
        public void Add(String expectedOnEventId, String tagOne, Object beanOne,
                        String tagTwo, Object beanTwo,
                        String tagThree, Object beanThree,
                        String tagFour, Object beanFour)
        {
            EventDescriptor desc = AddDesc(expectedOnEventId);
            desc.Put(tagOne, beanOne);
            desc.Put(tagTwo, beanTwo);
            desc.Put(tagThree, beanThree);
            desc.Put(tagFour, beanFour);
        }
    
        public void Add(String expectedOnEventId, object[][] tagsAndBeans)
        {
            EventDescriptor desc = AddDesc(expectedOnEventId);
            for (int i = 0; i < tagsAndBeans.Length; i++)
            {
                desc.Put((String)tagsAndBeans[i][0], tagsAndBeans[i][1]);
            }
        }
    
        private EventDescriptor AddDesc(String expectedOnEventId)
        {
            LinkedList<EventDescriptor> resultList = expectedResults.Get(expectedOnEventId);
    
            if (resultList == null)
            {
                resultList = new LinkedList<EventDescriptor>();
                expectedResults.Put(expectedOnEventId, resultList);
            }
    
            EventDescriptor eventDesc = new EventDescriptor();
            resultList.AddLast(eventDesc);
            return eventDesc;
        }
    }
}
