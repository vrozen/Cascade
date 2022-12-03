using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachInstTrigger : Trigger, IOperation {
    private IMetaObject /*MetaObject<MachInst>*/ mi;
    private StringValue evt;
  
    public MachInstTrigger(IMetaObject /*MetaObject<MachInst>*/ mi, StringValue evt){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.mi = mi;
      this.evt = evt;
    }
  
    public override IMetaObject getSubject(){
      return mi;
    }
  
    public IMetaObject /*MetaObject<MachInst>*/ getMi(){
      return mi;
    }
  
    public StringValue getEvt(){
      return evt;
    }
  
    public void setMi(IMetaObject /*MetaObject<MachInst>*/ mi){
      this.mi = mi;
    }
  
    public void setEvt(StringValue evt){
      this.evt = evt;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}