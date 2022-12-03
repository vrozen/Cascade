using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachInstDelete : Effect, IOperation {
    [Past] private IMetaObject /*MetaObject<MachInst>*/ mi;
    private IMetaObject /*MetaObject<Mach>*/ def;
    private IMetaObject /*MetaObject<Mach>*/ oldDef;
  
    public MachInstDelete([Past] IMetaObject /*MetaObject<MachInst>*/ mi, IMetaObject /*MetaObject<Mach>*/ def){
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
  
    public IMetaObject /*MetaObject<Mach>*/ getOldDef(){
      return oldDef;
    }
  
    public void setMi(IMetaObject /*MetaObject<MachInst>*/ mi){
      this.mi = mi;
    }
  
    public void setDef(IMetaObject /*MetaObject<Mach>*/ def){
      this.def = def;
    }
  
    public void setOldDef(IMetaObject /*MetaObject<Mach>*/ oldDef){
      this.oldDef = oldDef;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}