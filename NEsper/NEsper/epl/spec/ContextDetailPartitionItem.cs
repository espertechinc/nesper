///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailPartitionItem
    {
        private readonly FilterSpecRaw _filterSpecRaw;
        private readonly IList<String> _propertyNames;
    
        [NonSerialized] private FilterSpecCompiled _filterSpecCompiled;
        [NonSerialized] private FilterValueSetParam[][] _parametersCompiled;

        public ContextDetailPartitionItem(FilterSpecRaw filterSpecRaw, IList<String> propertyNames)
        {
            _filterSpecRaw = filterSpecRaw;
            _propertyNames = propertyNames;
        }

        public FilterSpecRaw FilterSpecRaw
        {
            get { return _filterSpecRaw; }
        }

        public IList<string> PropertyNames
        {
            get { return _propertyNames; }
        }

        public FilterSpecCompiled FilterSpecCompiled
        {
            get { return _filterSpecCompiled; }
            set
            {
                _filterSpecCompiled = value;
                _parametersCompiled = value.GetValueSet(null, null, null).Parameters;
            }
        }

        public FilterValueSetParam[][] ParametersCompiled
        {
            get { return _parametersCompiled; }
        }
    }
}
