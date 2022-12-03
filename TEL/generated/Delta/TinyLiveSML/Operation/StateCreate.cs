using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class StateCreate : Effect, IOperation {
    [Future] private IMetaObject /*MetaObject<State>*/ s;
    private StringValue name;
    private IMetaObject /*MetaObject<Mach>*/ m;
  
    public StateCreate([Future] IMetaObject /*MetaObject<State>*/ s, StringValue name, IMetaObject /*MetaObject<Mach>*/ m){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.s = s;
      this.name = name;
      this.m = m;
    }
  
    public override IMetaObject getSubject(){
      return s;
    }
  
    public IMetaObject /*MetaObject<State>*/ getS(){
      return s;
    }
  
    public StringValue getName(){
      return name;
    }
  
    public IMetaObject /*MetaObject<Mach>*/ getM(){
      return m;
    }
  
    public void setS(IMetaObject /*MetaObject<State>*/ s){
      this.s = s;
    }
  
    public void setName(StringValue name){
      this.name = name;
    }
  
    public void setM(IMetaObject /*MetaObject<Mach>*/ m){
      this.m = m;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}