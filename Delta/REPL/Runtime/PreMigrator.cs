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
using Delta.REPL.Operation;
using Delta.REPL.Model;
using Delta.Runtime;

namespace Delta.REPL.Runtime
{
  public class PreMigrator : IPreMigrator, Delta.REPL.Operation.IVisitor
  {
    private readonly Patcher patcher;

    public PreMigrator(Patcher patcher)
    {
      this.patcher = patcher;
    }

    public void preMigrate(Event evt)
    {
      if (evt is Effect && evt is ICommand command)
      {
        command.accept(this);
      }
    }

    public void visit(Initialization command)
    {
      //nothing
    }

    public void visit(SymbolTableCreate command)
    {
      //nothing
    }

    public void visit(SymbolTableDelete command)
    {
      MetaObject<SymbolTable> symbolTableMeta = command.getSymbolTable();
      SymbolTable symbolTable = symbolTableMeta.getObject();
      Map<string, Var> variables = symbolTable.getVariables();

      foreach (Var var in variables.Values)
      {
        IMetaObject varMeta = var.getMetaObject();
        Effect effect = new SymbolTableRemoveVariable(symbolTableMeta, (MetaObject<Var>) varMeta);
        command.addPreEffect(effect);
      }
    }

    public void visit(SymbolTableStoreVariable command)
    {
      //nothing
    }

    public void visit(SymbolTableRemoveVariable command)
    {
      //nothing
    }

    public void visit(VarCreate command)
    {
      //nothing
    }

    public void visit(VarDelete command)
    {
      //nothing
    }

    public void visit(VarSetValue command)
    {
      //nothing
    }

    public void visit(Declaration command)
    {
      //nothing
    }

    public void visit(Deletion command)
    {
      //nothing
    }

    public void visit(Assignment command)
    {
      //call resolve instead... this isn't really a pre-migration
      //Value value = command.getValue();
      //aligner.align(value); 
    }

    public void visit(Import command)
    {
      //nothing
    }

    public void visit(Call command)
    {
      //nothing
    }

    public void visit(Help command)
    {
      //nothing
    }

    public void visit(Print command)
    {
      //nothing
    }

    public void visit(Undo command)
    {
      //nothing
    }

    public void visit(Reconstruct command)
    {
      //nothing
    }
  }
}