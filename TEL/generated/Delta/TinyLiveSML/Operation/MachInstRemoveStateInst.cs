using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachInstRemoveStateInst : Effect, IOperation {
    private IMetaObject /*MetaObject<MachInst>*/ mi;
    private IMetaObject /*MetaObject<StateInst>*/ si;
    private IMetaObject /*MetaObject<State>*/ s;
  
    public MachInstRemoveStateInst(IMetaObject /*MetaObject<MachInst>*/ mi, IMetaObject /*MetaObject<StateInst>*/ si, IMetaObject /*MetaObject<State>*/ s){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.mi = mi;
      this.si = si;
      this.s = s;
    }
  
    public override IMetaObject getSubject(){
      return mi;
    }
  
    public IMetaObject /*MetaObject<MachInst>*/ getMi(){
      return mi;
    }
  
    public IMetaObject /*MetaObject<StateInst>*/ getSi(){
      return si;
    }
  
    public IMetaObject /*MetaObject<State>*/ getS(){
      return s;
    }
  
    public void setMi(IMetaObject /*MetaObject<MachInst>*/ mi){
      this.mi = mi;
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