///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Factory service for resolving view names and for creating view instances based on a view specification including
    ///     view name and namespace.
    /// </summary>
    public interface ViewResolutionService
    {
        /// <summary>
        ///     Instantiates a <see cref="ViewFactoryForge" /> based on the view namespace and name stored in the view spec.
        ///     <para>
        ///         Does not actually use the view factory object created.
        ///     </para>
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="name">is the view name</param>
        /// <param name="optionalCreateNamedWindowName">named window name</param>
        /// <throws>ViewProcessingException if the view namespace or name cannot resolve</throws>
        ViewFactoryForge Create(
            string @namespace,
            string name,
            string optionalCreateNamedWindowName);
    }
}