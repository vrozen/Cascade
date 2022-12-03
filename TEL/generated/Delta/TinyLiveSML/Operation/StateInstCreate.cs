using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class StateInstCreate : Effect, IOperation {
    [Future] private IMetaObject /*MetaObject<StateInst>*/ si;
    private IMetaObject /*MetaObject<State>*/ s;
  
    public StateInstCreate([Future] IMetaObject /*MetaObject<StateInst>*/ si, IMetaObject /*MetaObject<State>*/ s){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.si = si;
      this.s = s;
    }
  
    public override IMetaObject getSubject(){
      return si;
    }
  
    public IMetaObject /*MetaObject<StateInst>*/ getSi(){
      return si;
    }
  
    public IMetaObject /*MetaObject<State>*/ getS(){
      return s;
    }
  
    public void setSi(IMetaObject /*MetaObject<StateInst>*/ si){
      this.si = si;
    }
  
    public void setS(IMetaObject /*MetaObject<State>*/ s){
      this.s = s;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}