///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class ISupportDImpl : ISupportD
    {
        public ISupportDImpl(
            string valueD,
            string valueBaseD,
            string valueBaseDBase)
        {
            D = valueD;
            BaseD = valueBaseD;
            BaseDBase = valueBaseDBase;
        }

        public string D { get; }

        public string BaseD { get; }

        public string BaseDBase { get; }
    }
} // end of namespace
