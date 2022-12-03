using Delta.TinyLiveSML.Model;
using System;
using Delta.Model.BaseType;

namespace Delta.TinyLiveSML.Model {
  public class Trans : IPatchable, INode {
    [Reference] private IMetaObject metaObject;
    [Reference] public State src;
    public string evt;
    [Reference] public State tgt;
  
    public IMetaObject getMetaObject(){
      return metaObject;
    }
  
    public State getSrc(){
      return src;
    }
  
    public string getEvt(){
      return evt;
    }
  
    public State getTgt(){
      return tgt;
    }
  
    public override string ToString(){
      return src + "-[" + evt + "]->" + tgt;
    }
  
    public void setMetaObject(IMetaObject metaObject){
      this.metaObject = metaObject;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}