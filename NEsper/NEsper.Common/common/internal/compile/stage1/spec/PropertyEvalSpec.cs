///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for property evaluation.
    /// </summary>
    [Serializable]
    public class PropertyEvalSpec
    {
        /// <summary>
        /// Return a list of atoms.
        /// </summary>
        /// <value>atoms</value>
        public IList<PropertyEvalAtom> Atoms { get; private set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        public PropertyEvalSpec()
        {
            Atoms = new List<PropertyEvalAtom>();
        }

        /// <summary>Add an atom. </summary>
        /// <param name="atom">to add</param>
        public void Add(PropertyEvalAtom atom)
        {
            Atoms.Add(atom);
        }
    }
}