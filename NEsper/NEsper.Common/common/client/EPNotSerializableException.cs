///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.common.client
{
    public class EPNotSerializableException : EPRuntimeException
    {
        private Type classType;

        public Type ClassType => classType;

        public EPNotSerializableException(Type classType)
            : base("class is not serializable")
        {
            this.classType = classType;
        }

        public EPNotSerializableException(
            Exception cause,
            Type classType)
            : base("class is not serializable", cause)
        {
            this.classType = classType;
        }
    }
}