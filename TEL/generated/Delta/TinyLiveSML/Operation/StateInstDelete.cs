using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class StateInstDelete : Effect, IOperation {
    [Past] private IMetaObject /*MetaObject<StateInst>*/ si;
    private IMetaObject /*MetaObject<State>*/ def;
    private IMetaObject /*MetaObject<State>*/ oldDef;
  
    public StateInstDelete([Past] IMetaObject /*MetaObject<StateInst>*/ si, IMetaObject /*MetaObject<State>*/ def){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.si = si;
      this.def = def;
    }
  
    public override IMetaObject getSubject(){
      return si;
    }
  
    public IMetaObject /*MetaObject<StateInst>*/ getSi(){
      return si;
    }
  
    public IMetaObject /*MetaObject<State>*/ getDef(){
      return def;
    }
  
    public IMetaObject /*MetaObject<State>*/ getOldDef(){
      return oldDef;
    }
  
    public void setSi(IMetaObject /*MetaObject<StateInst>*/ si){
      this.si = si;
    }
  
    public void setDef(IMetaObject /*MetaObject<State>*/ def){
      this.def = def;
    }
  
    public void setOldDef(IMetaObject /*MetaObject<State>*/ oldDef){
      this.oldDef = oldDef;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}