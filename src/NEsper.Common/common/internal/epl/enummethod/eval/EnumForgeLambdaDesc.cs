///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumForgeLambdaDesc
    {
        public EnumForgeLambdaDesc(
            EventType[] types,
            string[] streamNames)
        {
            Types = types;
            StreamNames = streamNames;
        }

        public EventType[] Types { get; }

        public string[] StreamNames { get; }
    }
} // end of namespace