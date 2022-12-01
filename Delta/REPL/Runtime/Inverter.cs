/******************************************************************************
 * Copyright (c) 2022, Centrum Wiskunde & Informatica (CWI)
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holder nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * Contributors:
 *   * Riemer van Rozen - rozen@cwi.nl - CWI
 ******************************************************************************/
using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.Model.QualifiedName;
using Delta.REPL.Model;
using Delta.REPL.Operation;
using Delta.Runtime;

namespace Delta.REPL.Runtime
{
  public class Inverter: IInverter, Delta.REPL.Operation.IVisitor
  {
    private ICommand iOp;
    
    public Event invert(Event evt)
    {
      if (evt is REPL.Operation.ICommand command)
      {
        return (Event) invert(command);
      }
      
      throw new PatchException("Cannot invert unknown event");
    }

    private ICommand invert(ICommand command)
    {
      command.accept(this);
      return iOp;
    }

    public void visit(Initialization op)
    {
      iOp = new Initialization();
    }

    public void visit(SymbolTableCreate op)
    {
      MetaObject<SymbolTable> symbolTableMeta = op.getSymbolTable();
      iOp = new SymbolTableDelete(symbolTableMeta);
    }

    public void visit(SymbolTableDelete op)
    {
      MetaObject<SymbolTable> symbolTableMeta = op.getSymbolTable();
      iOp = new SymbolTableCreate(symbolTableMeta);
    }

    public void visit(SymbolTableStoreVariable op)
    {
      MetaObject<SymbolTable> symbolTableMeta = op.getSymbolTable();
      MetaObject<Var> varMeta = op.getVar();
      iOp = new SymbolTableRemoveVariable(symbolTableMeta, varMeta);
    }

    public void visit(SymbolTableRemoveVariable op)
    {
      MetaObject<SymbolTable> symbolTableMeta = op.getSymbolTable();
      MetaObject<Var> varMeta = op.getVar();
      iOp = new SymbolTableStoreVariable(symbolTableMeta, varMeta);
    }

    public void visit(VarCreate op)
    {
      MetaObject<Var> varMeta = op.getVar();
      string name = op.getName();
      iOp = new VarDelete(varMeta, name);
    }

    public void visit(VarDelete op)
    {
      MetaObject<Var> varMeta = op.getVar();
      string name = op.getName();
      iOp = new VarCreate(varMeta, name);
    }

    public void visit(VarSetValue op)
    {
      MetaObject<Var> varMeta = op.getVar();
      Value newValue = op.getNewValue();
      Value oldValue = op.getOldValue();
      iOp = new VarSetValue(varMeta, oldValue, newValue);
    }

    public void visit(Declaration op)
    {
      Field varName = op.getVarName();
      iOp = new Deletion(varName);
    }

    public void visit(Deletion op)
    {
      Field varName = op.getVarName();
      iOp = new Declaration(varName);
    }

    public void visit(Assignment op)
    {
      Field varName = op.getVarName();
      Value value = op.getValue();
      iOp = new Assignment(varName, value);
    }

    public void visit(Import op)
    {
      Name languageName = op.getLanguageName();
      iOp = new Import(languageName);
    }

    public void visit(Call op)
    {
      Name eventName =  op.getEventName();
      List<Value> operands = op.getOperands();
      iOp = new Call(eventName, operands);
    }

    public void visit(Help op)
    {
      iOp = new Help();
    }

    public void visit(Print op)
    {
      Field name = op.getVarName();
      iOp = new Print(name);
    }

    public void visit(Undo op)
    {
      iOp = new Undo();
    }
    
    public void visit(Reconstruct op)
    {
      iOp = new Reconstruct();
    }
  }
}