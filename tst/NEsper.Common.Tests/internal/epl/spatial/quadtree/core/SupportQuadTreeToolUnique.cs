///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.core.SupportQuadTreeUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportQuadTreeToolUnique<L>
    {
        public AdderUnique<L> adderUnique;
        public Factory<L> factory;
        public Generator generator;
        public bool pointInsideChecking;
        public Querier<L> querier;
        public Remover<L> remover;

        public SupportQuadTreeToolUnique(
            Factory<L> factory,
            Generator generator,
            AdderUnique<L> adderUnique,
            Remover<L> remover,
            Querier<L> querier,
            bool pointInsideChecking)
        {
            this.factory = factory;
            this.generator = generator;
            this.adderUnique = adderUnique;
            this.remover = remover;
            this.querier = querier;
            this.pointInsideChecking = pointInsideChecking;
        }
    }
} // end of namespace
