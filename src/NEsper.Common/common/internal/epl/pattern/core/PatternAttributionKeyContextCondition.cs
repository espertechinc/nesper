///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public class PatternAttributionKeyContextCondition : PatternAttributionKey
    {
        public PatternAttributionKeyContextCondition(
            string contextName,
            NameAccessModifier contextVisibility,
            string moduleName,
            int nestingLevel,
            bool startCondition,
            bool keyed)
        {
            ContextName = contextName;
            ContextVisibility = contextVisibility;
            ModuleName = moduleName;
            NestingLevel = nestingLevel;
            IsStartCondition = startCondition;
            IsKeyed = keyed;
        }

        public int NestingLevel { get; }

        public bool IsStartCondition { get; }

        public string ContextName { get; }

        public NameAccessModifier ContextVisibility { get; }

        public string ModuleName { get; }

        public bool IsKeyed { get; }

        public T Accept<T>(
            PatternAttributionKeyVisitor<T> visitor,
            short factoryNodeId)
        {
            return visitor.Visit(this, factoryNodeId);
        }
    }
} // end of namespace