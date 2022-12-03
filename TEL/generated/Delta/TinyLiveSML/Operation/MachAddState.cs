using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachAddState : Effect, IOperation {
    private IMetaObject /*MetaObject<Mach>*/ m;
    private IMetaObject /*MetaObject<State>*/ s;
  
    public MachAddState(IMetaObject /*MetaObject<Mach>*/ m, IMetaObject /*MetaObject<State>*/ s){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.m = m;
      this.s = s;
    }
  
    public override IMetaObject getSubject(){
      return m;
    }
  
    public IMetaObject /*MetaObject<Mach>*/ getM(){
      return m;
    }
  
    public IMetaObject /*MetaObject<State>*/ getS(){
      return s;
    }
  
    public void setM(IMetaObject /*MetaObject<Mach>*/ m){
      this.m = m;
    }
  
    public void setS(IMetaObject /*MetaObject<State>*/ s){
      this.s = s;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}