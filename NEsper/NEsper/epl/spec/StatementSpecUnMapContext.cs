///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Un-mapping context for mapping from an internal specifications to an SODA object model.
    /// </summary>
    public class StatementSpecUnMapContext
    {
        private readonly List<SubstitutionParameterExpressionBase> _substitutionParams;

        public StatementSpecUnMapContext()
        {
            _substitutionParams = new List<SubstitutionParameterExpressionBase>();
        }

        public void Add(SubstitutionParameterExpressionBase subsParam)
        {
            _substitutionParams.Add(subsParam);
        }

        public IList<SubstitutionParameterExpressionBase> GetSubstitutionParams()
        {
            return _substitutionParams;
        }

        public void AddAll(IList<SubstitutionParameterExpressionBase> inner)
        {
            _substitutionParams.AddRange(inner);
        }
    }
} // End of namespace
