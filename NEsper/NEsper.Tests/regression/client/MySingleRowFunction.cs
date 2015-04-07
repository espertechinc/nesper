///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;

namespace com.espertech.esper.regression.client
{
    public class MySingleRowFunction
    {
        private static readonly IList<EPLMethodInvocationContext> _methodInvokeContexts =
            new List<EPLMethodInvocationContext>();

        public static IList<EPLMethodInvocationContext> MethodInvokeContexts
        {
            get { return _methodInvokeContexts; }
        }

        public static int ComputePower3(int i)
        {
            return i*i*i;
        }

        public static int ComputePower3WithContext(int i, EPLMethodInvocationContext context)
        {
            _methodInvokeContexts.Add(context);
            return i*i*i;
        }

        public static String Surroundx(String target)
        {
            return "X" + target + "X";
        }

        public static InnerSingleRow GetChainTop()
        {
            return new InnerSingleRow();
        }

        public static void ThrowException()
        {
            throw new Exception("This is a 'throwexception' generated exception");
        }

        public static bool IsNullValue(EventBean @event, String propertyName)
        {
            return @event.Get(propertyName) == null;
        }

        public static String GetValueAsString(EventBean @event, String propertyName)
        {
            Object result = @event.Get(propertyName);
            return result != null ? result.ToString() : null;
        }

        public class InnerSingleRow
        {
            public int ChainValue(int i, int j)
            {
                return i*j;
            }
        }

        public static bool EventsCheckStrings(ICollection<EventBean> events, String property, String value)
        {
            return events.Any(@event => @event.Get(property).Equals(value));
        }
    }
}
