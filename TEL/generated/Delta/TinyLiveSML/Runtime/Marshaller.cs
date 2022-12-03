using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.Runtime;
using Delta.TinyLiveSML.Model;
using System;
using Delta.TinyLiveSML.Operation;
using System.Linq;
using Delta.Model.QualifiedName;
using Delta.Model.BaseOperation;

namespace Delta.TinyLiveSML.Runtime {
  public class Marshaller : Model.IVisitor {
    private Patcher patcher;
  
    public Marshaller(Patcher patcher){
      this.patcher = patcher;
    }
  
    public bool marshal(Model.INode node){
      node.accept(this);
      return true;
    }
  
    public void visit(Mach node){
      IMetaObject /*MetaObject<Mach>*/ metaObject = patcher.createMetaObject<Mach>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
    }
  
    public void visit(MachInst node){
      IMetaObject /*MetaObject<MachInst>*/ metaObject = patcher.createMetaObject<MachInst>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
      Model.INode childNodeInvalid = node.getInvalid();
      childNodeInvalid.accept(this);
    }
  
    public void visit(State node){
      IMetaObject /*MetaObject<State>*/ metaObject = patcher.createMetaObject<State>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
    }
  
    public void visit(StateInst node){
      IMetaObject /*MetaObject<StateInst>*/ metaObject = patcher.createMetaObject<StateInst>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
    }
  
    public void visit(Trans node){
      IMetaObject /*MetaObject<Trans>*/ metaObject = patcher.createMetaObject<Trans>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
    }
  }
}