using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class TransCreate : Effect, IOperation {
    [Future] private IMetaObject /*MetaObject<Trans>*/ t;
    private IMetaObject /*MetaObject<State>*/ src;
    private StringValue evt;
    private IMetaObject /*MetaObject<State>*/ tgt;
  
    public TransCreate([Future] IMetaObject /*MetaObject<Trans>*/ t, IMetaObject /*MetaObject<State>*/ src, StringValue evt, IMetaObject /*MetaObject<State>*/ tgt){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.t = t;
      this.src = src;
      this.evt = evt;
      this.tgt = tgt;
    }
  
    public override IMetaObject getSubject(){
      return t;
    }
  
    public IMetaObject /*MetaObject<Trans>*/ getT(){
      return t;
    }
  
    public IMetaObject /*MetaObject<State>*/ getSrc(){
      return src;
    }
  
    public StringValue getEvt(){
      return evt;
    }
  
    public IMetaObject /*MetaObject<State>*/ getTgt(){
      return tgt;
    }
  
    public void setT(IMetaObject /*MetaObject<Trans>*/ t){
      this.t = t;
    }
  
    public void setSrc(IMetaObject /*MetaObject<State>*/ src){
      this.src = src;
    }
  
    public void setEvt(StringValue evt){
      this.evt = evt;
    }
  
    public void setTgt(IMetaObject /*MetaObject<State>*/ tgt){
      this.tgt = tgt;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}