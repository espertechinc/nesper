///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.methodbase;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeMethodDesc
    {
        private readonly DateTimeMethodEnum _datetimeMethod;
        private readonly DatetimeMethodProviderForgeFactory _forgeFactory;
        private readonly DotMethodFP[] _parameters;

        public DateTimeMethodEnum DatetimeMethod => _datetimeMethod;

        public DatetimeMethodProviderForgeFactory ForgeFactory => _forgeFactory;

        public DotMethodFP[] Footprints => _parameters;

        public DatetimeMethodDesc(
            DateTimeMethodEnum datetimeMethod,
            DatetimeMethodProviderForgeFactory forgeFactory,
            DotMethodFP[] parameters)
        {
            _datetimeMethod = datetimeMethod;
            _forgeFactory = forgeFactory;
            _parameters = parameters;
        }
    }
}