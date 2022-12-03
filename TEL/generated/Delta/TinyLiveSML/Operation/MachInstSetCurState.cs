using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachInstSetCurState : Effect, IOperation {
    private IMetaObject /*MetaObject<MachInst>*/ mi;
    private IMetaObject /*MetaObject<StateInst>*/ cur;
    private IMetaObject /*MetaObject<StateInst>*/ oldCur;
  
    public MachInstSetCurState(IMetaObject /*MetaObject<MachInst>*/ mi, IMetaObject /*MetaObject<StateInst>*/ cur){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.mi = mi;
      this.cur = cur;
    }
  
    public override IMetaObject getSubject(){
      return mi;
    }
  
    public IMetaObject /*MetaObject<MachInst>*/ getMi(){
      return mi;
    }
  
    public IMetaObject /*MetaObject<StateInst>*/ getCur(){
      return cur;
    }
  
    public IMetaObject /*MetaObject<StateInst>*/ getOldCur(){
      return oldCur;
    }
  
    public void setMi(IMetaObject /*MetaObject<MachInst>*/ mi){
      this.mi = mi;
    }
  
    public void setCur(IMetaObject /*MetaObject<StateInst>*/ cur){
      this.cur = cur;
    }
  
    public void setOldCur(IMetaObject /*MetaObject<StateInst>*/ oldCur){
      this.oldCur = oldCur;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}