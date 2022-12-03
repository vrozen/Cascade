using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.Runtime;
using Delta.TinyLiveSML.Model;
using System;
using Delta.TinyLiveSML.Operation;
using System.Linq;
using Delta.Model.QualifiedName;
using Delta.Model.BaseOperation;

namespace Delta.TinyLiveSML.Runtime {
  public class Language : ILanguage, IPatchable {
    private static readonly String LanguageName = "Delta.TinyLiveSML";
    private readonly PreMigrator preMigrator;
    private readonly PostMigrator postMigrator;
    private readonly Generator generator;
    private readonly Inverter inverter;
    [Reference] private IMetaObject metaObject;
  
    public Language(Patcher patcher){
      UUID id = new UUID(0);
      metaObject = new MetaObject<Language>(id);
      metaObject.bind(this);
      preMigrator = new PreMigrator(patcher);
      postMigrator = new PostMigrator(patcher);
      generator = new Generator(patcher);
      inverter = new Inverter();
    }
  
    public void init(){
    }
  
    public void deinit(){
    }
  
    public string getName(){
      return LanguageName;
    }
  
    public void generate(Event evt){
      generator.generate(evt);
    }
  
    public void preMigrate(Event evt){
      preMigrator.preMigrate(evt);
    }
  
    public void postMigrate(Event evt){
      postMigrator.postMigrate(evt);
    }
  
    public Event invert(Event evt){
      return inverter.invert(evt);
    }
  
    public void notifyScheduled(Event evt){
    }
  
    public void notifyGenerated(Event evt){
    }
  
    public void notifyCommitted(Event evt){
    }
  
    public void notifyCompleted(Event evt){
    }
  
    public void write(string text){
    }
  
    public void writeError(string text){
    }
  
    public Type[] getEventTypes(){
      Type[] types = {
        typeof(MachCreate),
        typeof(MachDelete),
        typeof(MachAddState),
        typeof(MachRemoveState),
        typeof(MachAddMachInst),
        typeof(MachRemoveMachInst),
        typeof(MachInstCreate),
        typeof(MachInstDelete),
        typeof(MachInstAddStateInst),
        typeof(MachInstRemoveStateInst),
        typeof(MachInstInitialize),
        typeof(MachInstSetCurState),
        typeof(MachInstTrigger),
        typeof(MachInstMissingCurrentState),
        typeof(MachInstQuiescence),
        typeof(StateCreate),
        typeof(StateDelete),
        typeof(StateAddIn),
        typeof(StateRemoveIn),
        typeof(StateAddOut),
        typeof(StateRemoveOut),
        typeof(StateInstCreate),
        typeof(StateInstDelete),
        typeof(StateInstSetCount),
        typeof(TransCreate),
        typeof(TransDelete)
      };
      return types;
    }
  
    public IMetaObject getMetaObject(){
      return metaObject;
    }
  
    public void setMetaObject(IMetaObject metaObject){
      this.metaObject = metaObject;
    }
  }
}