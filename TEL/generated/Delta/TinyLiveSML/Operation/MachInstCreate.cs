using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachInstCreate : Effect, IOperation {
    [Future] private IMetaObject /*MetaObject<MachInst>*/ mi;
    private IMetaObject /*MetaObject<Mach>*/ def;
  
    public MachInstCreate([Future] IMetaObject /*MetaObject<MachInst>*/ mi, IMetaObject /*MetaObject<Mach>*/ def){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.mi = mi;
      this.def = def;
    }
  
    public override IMetaObject getSubject(){
      return mi;
    }
  
    public IMetaObject /*MetaObject<MachInst>*/ getMi(){
      return mi;
    }
  
    public IMetaObject /*MetaObject<Mach>*/ getDef(){
      return def;
    }
  
    public void setMi(IMetaObject /*MetaObject<MachInst>*/ mi){
      this.mi = mi;
    }
  
    public void setDef(IMetaObject /*MetaObject<Mach>*/ def){
      this.def = def;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}