///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.support.bean
{
    [Serializable]	
	public sealed class SupportBeanWithEnum
	{
        public String TheString { get; private set; }

        public SupportEnum SupportEnum { get; private set; }

        public SupportBeanWithEnum(String stringValue, SupportEnum supportEnum)
		{
			TheString = stringValue;
			SupportEnum = supportEnum;
		}
	}
}
