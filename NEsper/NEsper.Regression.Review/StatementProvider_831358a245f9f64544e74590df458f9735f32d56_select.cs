using System;
using System.Linq;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace generated {
  public class StatementProvider_831358a245f9f64544e74590df458f9735f32d56_select : StatementProvider {
    internal StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields;
    internal StatementInformationalsRuntime statementInformationals;
    internal StatementAIFactoryProvider factoryProvider;

    public StatementProvider_831358a245f9f64544e74590df458f9735f32d56_select(){
      statementInformationals=M0();
    }

    // StmtClassForgableStmtProvider --- StmtClassForgableStmtProvider.Forge():0
    public StatementInformationalsRuntime Informationals {
      get {
        return statementInformationals;
      }
    }

    // StmtClassForgableStmtProvider --- StmtClassForgableStmtProvider.MakeGetStatementAIFactoryProvider():0
    public StatementAIFactoryProvider StatementAIFactoryProvider {
      get {
        return factoryProvider;
      }
    }
    // StmtClassForgableStmtProvider --- StmtClassForgableStmtProvider.MakeInitialize():116
    public void Initialize(EPStatementInitServices stmtInitSvc){
      statementFields=new StatementFields_831358a245f9f64544e74590df458f9735f32d56_select();
      factoryProvider=new StatementAIFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select(stmtInitSvc,statementFields);
    }

    // StatementInformationalsCompileTime --- StatementInformationalsCompileTime.Make():145
    StatementInformationalsRuntime M0(){
      StatementInformationalsRuntime info=new StatementInformationalsRuntime();
      info.StatementNameCompileTime="select";
      info.IsAlwaysSynthesizeOutputEvents=false;
      info.OptionalContextName="HashByUserCtx";
      info.OptionalContextModuleName=null;
      info.OptionalContextVisibility=NameAccessModifier.PUBLIC;
      info.IsCanSelfJoin=false;
      info.HasSubquery=false;
      info.IsNeedDedup=false;
      info.IsStateless=true;
      info.Annotations=M1();
      info.UserObjectCompileTime=null;
      info.NumFilterCallbacks=1;
      info.NumScheduleCallbacks=0;
      info.NumNamedWindowCallbacks=0;
      info.StatementType=StatementType.SELECT;
      info.Priority=0;
      info.IsPreemptive=false;
      info.HasVariables=false;
      info.IsWritesToTables=false;
      info.HasTableAccess=false;
      info.SelectClauseTypes=null;
      info.SelectClauseColumnNames=null;
      info.IsForClauseDelivery=false;
      info.GroupDeliveryEval=null;
      info.Properties=Collections.SingletonMap(StatementProperty.EPL,((object)"@Name('select') context HashByUserCtx select P01 from SupportBean_S0"));
      info.HasMatchRecognize=false;
      info.AuditProvider=AuditProviderDefault.INSTANCE;
      info.IsInstrumented=false;
      info.InstrumentationProvider=null;
      info.SubstitutionParamTypes=null;
      info.SubstitutionParamNames=null;
      info.InsertIntoLatchName=null;
      info.IsAllowSubscriber=false;
      return info;
    }

    // AnnotationUtil --- AnnotationUtil.MakeAnnotations():121
    Attribute[] M1(){
      Attribute[] annotations=new Attribute[1];
      annotations[0]=new AnnotationName("select");
      return annotations;
    }
  }
}
