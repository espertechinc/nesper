///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class ISupportAImplSuperGImpl : ISupportAImplSuperG
    {
        public ISupportAImplSuperGImpl(
            string valueG,
            string valueA,
            string valueBaseAB)
        {
            G = valueG;
            A = valueA;
            BaseAB = valueBaseAB;
        }

        public override string G { get; }

        public override string A { get; }

        public override string BaseAB { get; }
    }
} // end of namespace