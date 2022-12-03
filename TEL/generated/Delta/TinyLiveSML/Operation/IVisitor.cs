using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public interface IVisitor {
    void visit(MachCreate op);
    void visit(MachDelete op);
    void visit(MachAddState op);
    void visit(MachRemoveState op);
    void visit(MachAddMachInst op);
    void visit(MachRemoveMachInst op);
    void visit(MachInstCreate op);
    void visit(MachInstDelete op);
    void visit(MachInstAddStateInst op);
    void visit(MachInstRemoveStateInst op);
    void visit(MachInstInitialize op);
    void visit(MachInstSetCurState op);
    void visit(MachInstTrigger op);
    void visit(MachInstMissingCurrentState op);
    void visit(MachInstQuiescence op);
    void visit(StateCreate op);
    void visit(StateDelete op);
    void visit(StateAddIn op);
    void visit(StateRemoveIn op);
    void visit(StateAddOut op);
    void visit(StateRemoveOut op);
    void visit(StateInstCreate op);
    void visit(StateInstDelete op);
    void visit(StateInstSetCount op);
    void visit(TransCreate op);
    void visit(TransDelete op);
  }
}