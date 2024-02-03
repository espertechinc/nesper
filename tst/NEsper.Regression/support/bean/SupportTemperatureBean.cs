///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportTemperatureBean
    {
        private SupportTemperatureBean()
        {
            // need a private ctor for testing
        }

        public SupportTemperatureBean(string geom)
        {
            Geom = geom;
        }

        public string Geom { get; set; }
    }
} // end of namespace