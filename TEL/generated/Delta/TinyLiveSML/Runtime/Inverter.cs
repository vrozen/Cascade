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
  public class Inverter : IInverter, Operation.IVisitor {
    private Delta.Runtime.Inverter inverter;
    private Operation.IOperation iOp;
  
    public Inverter(){
      this.inverter = new Delta.Runtime.Inverter();
    }
  
    public Event invert(Event evt){
      if(evt is Operation.IOperation op){
        op.accept(this);
      };
      return (Event) iOp;
    }
  
    public void visit(MachCreate op){
      iOp = new MachDelete(op.getM(), op.getName());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachDelete op){
      iOp = new MachCreate(op.getM(), op.getName());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachAddState op){
      iOp = new MachRemoveState(op.getM(), op.getS());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachRemoveState op){
      iOp = new MachAddState(op.getM(), op.getS());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachAddMachInst op){
      iOp = new MachRemoveMachInst(op.getM(), op.getMi());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachRemoveMachInst op){
      iOp = new MachAddMachInst(op.getM(), op.getMi());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachInstCreate op){
      iOp = new MachInstDelete(op.getMi(), op.getDef());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachInstDelete op){
      iOp = new MachInstCreate(op.getMi(), op.getDef());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachInstAddStateInst op){
      iOp = new MachInstRemoveStateInst(op.getMi(), op.getSi(), op.getS());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachInstRemoveStateInst op){
      iOp = new MachInstAddStateInst(op.getMi(), op.getSi(), op.getS());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(MachInstInitialize op){
      iOp = new MachInstInitialize(op.getMi());
    }
  
    public void visit(MachInstSetCurState op){
      iOp = null;
    }
  
    public void visit(MachInstTrigger op){
      iOp = new MachInstTrigger(op.getMi(), op.getEvt());
    }
  
    public void visit(MachInstMissingCurrentState op){
      iOp = new MachInstMissingCurrentState(op.getMi());
    }
  
    public void visit(MachInstQuiescence op){
      iOp = new MachInstQuiescence(op.getMi());
    }
  
    public void visit(StateCreate op){
      iOp = new StateDelete(op.getS(), op.getName(), op.getM());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(StateDelete op){
      iOp = new StateCreate(op.getS(), op.getName(), op.getM());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(StateAddIn op){
      iOp = new StateRemoveIn(op.getS(), op.getT());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(StateRemoveIn op){
      iOp = new StateAddIn(op.getS(), op.getT());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(StateAddOut op){
      iOp = new StateRemoveOut(op.getS(), op.getT());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(StateRemoveOut op){
      iOp = new StateAddOut(op.getS(), op.getT());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(StateInstCreate op){
      iOp = new StateInstDelete(op.getSi(), op.getS());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(StateInstDelete op){
      iOp = new StateInstCreate(op.getSi(), op.getDef());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(StateInstSetCount op){
      iOp = null;
    }
  
    public void visit(TransCreate op){
      iOp = new TransDelete(op.getT(), op.getSrc(), op.getEvt(), op.getTgt());
      inverter.invertOperations(op, (Effect) iOp);
    }
  
    public void visit(TransDelete op){
      iOp = new TransCreate(op.getT(), op.getSrc(), op.getEvt(), op.getTgt());
      inverter.invertOperations(op, (Effect) iOp);
    }
  }
}