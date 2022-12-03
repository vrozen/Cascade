using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachInstQuiescence : Signal, IOperation {
    private IMetaObject /*MetaObject<MachInst>*/ mi;
  
    public MachInstQuiescence(IMetaObject /*MetaObject<MachInst>*/ mi){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.mi = mi;
    }
  
    public override IMetaObject getSubject(){
      return mi;
    }
  
    public IMetaObject /*MetaObject<MachInst>*/ getMi(){
      return mi;
    }
  
    public void setMi(IMetaObject /*MetaObject<MachInst>*/ mi){
      this.mi = mi;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}