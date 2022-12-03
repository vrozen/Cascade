using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class MachCreate : Effect, IOperation {
    [Future] private IMetaObject /*MetaObject<Mach>*/ m;
    private StringValue name;
  
    public MachCreate([Future] IMetaObject /*MetaObject<Mach>*/ m, StringValue name){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.m = m;
      this.name = name;
    }
  
    public override IMetaObject getSubject(){
      return m;
    }
  
    public IMetaObject /*MetaObject<Mach>*/ getM(){
      return m;
    }
  
    public StringValue getName(){
      return name;
    }
  
    public void setM(IMetaObject /*MetaObject<Mach>*/ m){
      this.m = m;
    }
  
    public void setName(StringValue name){
      this.name = name;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}