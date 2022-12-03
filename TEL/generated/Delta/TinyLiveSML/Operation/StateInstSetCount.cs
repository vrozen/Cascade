using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class StateInstSetCount : Effect, IOperation {
    private IMetaObject /*MetaObject<StateInst>*/ si;
    private IntValue count;
    private IntValue oldCount;
  
    public StateInstSetCount(IMetaObject /*MetaObject<StateInst>*/ si, IntValue count){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.si = si;
      this.count = count;
    }
  
    public override IMetaObject getSubject(){
      return si;
    }
  
    public IMetaObject /*MetaObject<StateInst>*/ getSi(){
      return si;
    }
  
    public IntValue getCount(){
      return count;
    }
  
    public IntValue getOldCount(){
      return oldCount;
    }
  
    public void setSi(IMetaObject /*MetaObject<StateInst>*/ si){
      this.si = si;
    }
  
    public void setCount(IntValue count){
      this.count = count;
    }
  
    public void setOldCount(IntValue oldCount){
      this.oldCount = oldCount;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}