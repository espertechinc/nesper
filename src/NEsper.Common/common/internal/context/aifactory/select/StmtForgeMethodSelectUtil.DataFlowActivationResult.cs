using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StmtForgeMethodSelectUtil
    {
        private class DataFlowActivationResult
        {
            private readonly IList<StmtClassForgeableFactory> additionalForgeables;
            private readonly string eventTypeName;
            private readonly IList<ScheduleHandleTracked> schedules;
            private readonly EventType streamEventType;
            private readonly ViewableActivatorForge viewableActivatorForge;
            private readonly IList<ViewFactoryForge> viewForges;

            public DataFlowActivationResult(
                EventType streamEventType,
                string eventTypeName,
                ViewableActivatorForge viewableActivatorForge,
                IList<ViewFactoryForge> viewForges,
                IList<StmtClassForgeableFactory> additionalForgeables,
                IList<ScheduleHandleTracked> schedules)
            {
                this.streamEventType = streamEventType;
                this.eventTypeName = eventTypeName;
                this.viewableActivatorForge = viewableActivatorForge;
                this.viewForges = viewForges;
                this.additionalForgeables = additionalForgeables;
                this.schedules = schedules;
            }

            public EventType StreamEventType => streamEventType;

            public string EventTypeName => eventTypeName;

            public ViewableActivatorForge ViewableActivatorForge => viewableActivatorForge;

            public IList<ViewFactoryForge> ViewForges => viewForges;

            public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

            public IList<ScheduleHandleTracked> Schedules => schedules;
        }
    }
}