using Delta.TinyLiveSML.Model;
using System;
using Delta.Model.BaseType;

namespace Delta.TinyLiveSML.Model {
  public class Mach : IPatchable, INode {
    [Reference] private IMetaObject metaObject;
    public string name;
    public Set<State> states;
    public Set<MachInst> instances;
  
    public IMetaObject getMetaObject(){
      return metaObject;
    }
  
    public string getName(){
      return name;
    }
  
    public Set<State> getStates(){
      return states;
    }
  
    public Set<MachInst> getInstances(){
      return instances;
    }
  
    public override string ToString(){
      string r = "machine " + name + " \n";
      foreach(State s in states) {
        r = r + "  " + s + "\n";
        foreach(Trans t in s.o) {
          r = r + "    " + t + "\n";
        };
      };
      return r;
    }
  
    public void setMetaObject(IMetaObject metaObject){
      this.metaObject = metaObject;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}