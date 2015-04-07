///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;


namespace com.espertech.esper.core.service
{
    public class ConfiguratorContext {
        private readonly String engineURI;
        private readonly IDictionary<String, EPServiceProviderSPI> runtimes;
    
        public ConfiguratorContext(String engineURI, IDictionary<String, EPServiceProviderSPI> runtimes) {
            this.engineURI = engineURI;
            this.runtimes = runtimes;
        }
    
        public String GetEngineURI() {
            return engineURI;
        }
    
        public IDictionary<String, EPServiceProviderSPI> GetRuntimes() {
            return runtimes;
        }
    }
}
