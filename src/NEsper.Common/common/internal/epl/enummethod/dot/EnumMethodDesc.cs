///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.methodbase;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class EnumMethodDesc
    {
        private readonly string _enumMethodName;
        private readonly EnumMethodEnum _enumMethod;
        private readonly ExprDotForgeEnumMethodFactory _factory;
        private readonly DotMethodFP[] _parameters;

        public EnumMethodDesc(
            string methodName,
            EnumMethodEnum enumMethod,
            ExprDotForgeEnumMethodFactory factory,
            DotMethodFP[] parameters)
        {
            _enumMethodName = methodName;
            _enumMethod = enumMethod;
            _factory = factory;
            _parameters = parameters;
        }

        public string EnumMethodName => _enumMethodName;

        public EnumMethodEnum EnumMethod => _enumMethod;

        public ExprDotForgeEnumMethodFactory Factory => _factory;

        public DotMethodFP[] Footprints => _parameters;
    }
} // end of namespace