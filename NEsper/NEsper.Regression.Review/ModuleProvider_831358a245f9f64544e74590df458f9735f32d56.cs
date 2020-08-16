using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace generated {
  public class ModuleProvider_831358a245f9f64544e74590df458f9735f32d56 : ModuleProvider {

    public ModuleProvider_831358a245f9f64544e74590df458f9735f32d56(){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():0
    public string ModuleName {
      get {
        return null;
      }
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():0
    public IDictionary<ModuleProperty,object> ModuleProperties {
      get {
        return Collections.GetEmptyMap<ModuleProperty,object>();
      }
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():0
    public ModuleDependenciesRuntime ModuleDependencies {
      get {
        ModuleDependenciesRuntime md=new ModuleDependenciesRuntime();
        md.PathEventTypes=NameAndModule.EMPTY_ARRAY;
        md.PathNamedWindows=NameAndModule.EMPTY_ARRAY;
        md.PathTables=NameAndModule.EMPTY_ARRAY;
        md.PathVariables=NameAndModule.EMPTY_ARRAY;
        md.PathContexts=new NameAndModule[]{new NameAndModule("HashByUserCtx",null)};
        md.PathExpressions=NameAndModule.EMPTY_ARRAY;
        md.PathIndexes=ModuleIndexMeta.EMPTY_ARRAY;
        md.PathScripts=NameParamNumAndModule.EMPTY_ARRAY;
        md.PublicEventTypes=new string[] {"SupportBean_S0"};
        md.PublicVariables=new string[]{};
        return md;
      }
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():0
    public IList<StatementProvider> Statements {
      get {
        IList<StatementProvider> statements=new List<StatementProvider>(1);
        statements.Add(new generated.StatementProvider_831358a245f9f64544e74590df458f9735f32d56_select());
        return statements;
      }
    }
    // EPCompilerImpl --- CompilerHelperModuleProvider.MakeInitEventTypes():488
    public void InitializeEventTypes(EPModuleEventTypeInitServices moduleETInitSvc){
      M0(moduleETInitSvc);
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():203
    public void InitializeNamedWindows(EPModuleNamedWindowInitServices moduleNWInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():219
    public void InitializeTables(EPModuleTableInitServices moduleTableInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():229
    public void InitializeIndexes(EPModuleIndexInitServices moduleIdxInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():240
    public void InitializeContexts(EPModuleContextInitServices moduleCtxInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():256
    public void InitializeVariables(EPModuleVariableInitServices moduleVarInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():272
    public void InitializeExprDeclareds(EPModuleExprDeclaredInitServices moduleDeclInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():288
    public void InitializeScripts(EPModuleScriptInitServices moduleScriptInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.RegisterEventTypeCodegen():598
    void M0(EPModuleEventTypeInitServices moduleETInitSvc){
      EventTypeMetadata metadata=new EventTypeMetadata("stmt0_out0",null,EventTypeTypeClass.STATEMENTOUT,EventTypeApplicationType.MAP,NameAccessModifier.TRANSIENT,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(-1L,-1L));
      LinkedHashMap<string,object> props=M1(moduleETInitSvc);
      moduleETInitSvc.EventTypeCollector.RegisterMap(metadata,props,null,null,null);
    }

    // CompilerHelperModuleProvider --- CompilerHelperModuleProvider.MakePropsCodegen():768
    LinkedHashMap<string,object> M1(EPModuleEventTypeInitServices moduleETInitSvc){
      LinkedHashMap<string,object> props=new LinkedHashMap<string,object>();
      props.Put("P01",typeof(string));
      return props;
    }
  }
}
