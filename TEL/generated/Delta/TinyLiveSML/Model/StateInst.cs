using Delta.TinyLiveSML.Model;
using System;
using Delta.Model.BaseType;

namespace Delta.TinyLiveSML.Model {
  public class StateInst : IPatchable, INode {
    [Reference] private IMetaObject metaObject;
    [Reference] public State def;
    public int count;
  
    public IMetaObject getMetaObject(){
      return metaObject;
    }
  
    public State getDef(){
      return def;
    }
  
    public int getCount(){
      return count;
    }
  
    public override string ToString(){
      return "  " + def.getName() + " : " + count;
    }
  
    public void setMetaObject(IMetaObject metaObject){
      this.metaObject = metaObject;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}