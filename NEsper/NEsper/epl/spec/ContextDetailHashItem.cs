///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailHashItem
    {
        private readonly FilterSpecRaw _filterSpecRaw;
        private readonly ExprChainedSpec _function;

        [NonSerialized] private FilterSpecCompiled _filterSpecCompiled;
        private FilterSpecLookupable _lookupable;
        [NonSerialized] private FilterValueSetParam[][] _parametersCompiled;

        public ContextDetailHashItem(ExprChainedSpec function, FilterSpecRaw filterSpecRaw)
        {
            _function = function;
            _filterSpecRaw = filterSpecRaw;
        }

        public ExprChainedSpec Function
        {
            get { return _function; }
        }

        public FilterSpecRaw FilterSpecRaw
        {
            get { return _filterSpecRaw; }
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

        public FilterSpecLookupable Lookupable
        {
            get { return _lookupable; }
            set { _lookupable = value; }
        }
    }
}