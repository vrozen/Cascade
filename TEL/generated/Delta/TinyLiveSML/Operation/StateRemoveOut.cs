using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class StateRemoveOut : Effect, IOperation {
    private IMetaObject /*MetaObject<State>*/ s;
    private IMetaObject /*MetaObject<Trans>*/ t;
  
    public StateRemoveOut(IMetaObject /*MetaObject<State>*/ s, IMetaObject /*MetaObject<Trans>*/ t){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.s = s;
      this.t = t;
    }
  
    public override IMetaObject getSubject(){
      return s;
    }
  
    public IMetaObject /*MetaObject<State>*/ getS(){
      return s;
    }
  
    public IMetaObject /*MetaObject<Trans>*/ getT(){
      return t;
    }
  
    public void setS(IMetaObject /*MetaObject<State>*/ s){
      this.s = s;
    }
  
    public void setT(IMetaObject /*MetaObject<Trans>*/ t){
      this.t = t;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}