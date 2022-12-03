using Delta.TinyLiveSML.Model;
using System;
using Delta.Model.BaseType;

namespace Delta.TinyLiveSML.Model {
  public class MachInst : IPatchable, INode {
    [Reference] private IMetaObject metaObject;
    [Reference] public Mach def;
    [Reference] public StateInst cur;
    public Map<State,StateInst> sis;
    public StateInst invalid;
  
    public IMetaObject getMetaObject(){
      return metaObject;
    }
  
    public Mach getDef(){
      return def;
    }
  
    public StateInst getCur(){
      return cur;
    }
  
    public Map<State,StateInst> getSis(){
      return sis;
    }
  
    public StateInst getInvalid(){
      return invalid;
    }
  
    public override string ToString(){
      string r = "machine instance " + def.getName() + " \n";
      foreach(StateInst si in sis.Values) {
        r = r + "  " + si;
        if(si == cur){
          r = r + " *";
        };
        r = r + "\n";
        foreach(Trans t in si.def.o) {
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