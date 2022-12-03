using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class TransDelete : Effect, IOperation {
    [Past] private IMetaObject /*MetaObject<Trans>*/ t;
    private IMetaObject /*MetaObject<State>*/ src;
    private IMetaObject /*MetaObject<State>*/ oldSrc;
    private StringValue evt;
    private StringValue oldEvt;
    private IMetaObject /*MetaObject<State>*/ tgt;
    private IMetaObject /*MetaObject<State>*/ oldTgt;
  
    public TransDelete([Past] IMetaObject /*MetaObject<Trans>*/ t, IMetaObject /*MetaObject<State>*/ src, StringValue evt, IMetaObject /*MetaObject<State>*/ tgt){
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
  
    public IMetaObject /*MetaObject<State>*/ getOldSrc(){
      return oldSrc;
    }
  
    public StringValue getEvt(){
      return evt;
    }
  
    public StringValue getOldEvt(){
      return oldEvt;
    }
  
    public IMetaObject /*MetaObject<State>*/ getTgt(){
      return tgt;
    }
  
    public IMetaObject /*MetaObject<State>*/ getOldTgt(){
      return oldTgt;
    }
  
    public void setT(IMetaObject /*MetaObject<Trans>*/ t){
      this.t = t;
    }
  
    public void setSrc(IMetaObject /*MetaObject<State>*/ src){
      this.src = src;
    }
  
    public void setOldSrc(IMetaObject /*MetaObject<State>*/ oldSrc){
      this.oldSrc = oldSrc;
    }
  
    public void setEvt(StringValue evt){
      this.evt = evt;
    }
  
    public void setOldEvt(StringValue oldEvt){
      this.oldEvt = oldEvt;
    }
  
    public void setTgt(IMetaObject /*MetaObject<State>*/ tgt){
      this.tgt = tgt;
    }
  
    public void setOldTgt(IMetaObject /*MetaObject<State>*/ oldTgt){
      this.oldTgt = oldTgt;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}