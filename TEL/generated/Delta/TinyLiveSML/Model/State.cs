using Delta.TinyLiveSML.Model;
using System;
using Delta.Model.BaseType;

namespace Delta.TinyLiveSML.Model {
  public class State : IPatchable, INode {
    [Reference] private IMetaObject metaObject;
    public string name;
    public Set<Trans> i;
    public Set<Trans> o;
  
    public IMetaObject getMetaObject(){
      return metaObject;
    }
  
    public string getName(){
      return name;
    }
  
    public Set<Trans> getI(){
      return i;
    }
  
    public Set<Trans> getO(){
      return o;
    }
  
    public override string ToString(){
      return name;
    }
  
    public void setMetaObject(IMetaObject metaObject){
      this.metaObject = metaObject;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}