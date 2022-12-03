using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachAddMachInst : Effect, IOperation {
    private IMetaObject /*MetaObject<Mach>*/ m;
    private IMetaObject /*MetaObject<MachInst>*/ mi;
  
    public MachAddMachInst(IMetaObject /*MetaObject<Mach>*/ m, IMetaObject /*MetaObject<MachInst>*/ mi){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.m = m;
      this.mi = mi;
    }
  
    public override IMetaObject getSubject(){
      return m;
    }
  
    public IMetaObject /*MetaObject<Mach>*/ getM(){
      return m;
    }
  
    public IMetaObject /*MetaObject<MachInst>*/ getMi(){
      return mi;
    }
  
    public void setM(IMetaObject /*MetaObject<Mach>*/ m){
      this.m = m;
    }
  
    public void setMi(IMetaObject /*MetaObject<MachInst>*/ mi){
      this.mi = mi;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}