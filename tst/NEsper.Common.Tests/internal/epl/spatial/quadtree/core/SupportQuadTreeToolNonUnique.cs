///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportQuadTreeToolNonUnique<L>
    {
        public SupportQuadTreeUtil.AdderNonUnique<L> adderNonUnique;
        public SupportQuadTreeUtil.Factory<L> factory;
        public SupportQuadTreeUtil.Generator generator;
        public bool pointInsideChecking;
        public SupportQuadTreeUtil.Querier<L> querier;
        public SupportQuadTreeUtil.Remover<L> remover;

        public SupportQuadTreeToolNonUnique(
            SupportQuadTreeUtil.Factory<L> factory,
            SupportQuadTreeUtil.Generator generator,
            SupportQuadTreeUtil.AdderNonUnique<L> adderNonUnique,
            SupportQuadTreeUtil.Remover<L> remover,
            SupportQuadTreeUtil.Querier<L> querier,
            bool pointInsideChecking)
        {
            this.factory = factory;
            this.generator = generator;
            this.adderNonUnique = adderNonUnique;
            this.remover = remover;
            this.querier = querier;
            this.pointInsideChecking = pointInsideChecking;
        }
    }
} // end of namespace
