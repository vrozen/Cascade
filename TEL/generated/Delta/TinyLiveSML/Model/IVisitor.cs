using Delta.TinyLiveSML.Model;
using System;
using Delta.Model.BaseType;

namespace Delta.TinyLiveSML.Model {
  public interface IVisitor {
    void visit(Mach op);
    void visit(MachInst op);
    void visit(State op);
    void visit(StateInst op);
    void visit(Trans op);
  }
}