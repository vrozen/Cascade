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
using Delta.Model.BaseOperation;
using Delta.Model.DesignSpace;
using Delta.Model.QualifiedName;
using Delta.REPL.Operation;
using Delta.REPL.Model;
using Delta.Runtime;

namespace Delta.REPL.Runtime
{
  public class Generator: IGenerator, Delta.REPL.Operation.IVisitor
  {
    private readonly Patcher patcher;

    private static readonly StringValue VarType = new StringValue(typeof(Var).ToString());
    private static readonly Field VarNameField = new Field("name");
    private static readonly Field VarValueField = new Field("value");
    
    private static readonly StringValue SymbolTableType = new StringValue(typeof(SymbolTable).ToString());
    private static readonly Field SymbolTableVariablesField = new Field("variables");
    private static readonly StringValue SymbolTableVariablesType = new StringValue(typeof(Map<string,Var>).ToString());

    public Generator(Patcher patcher)
    {
      this.patcher = patcher;
    }
    
    public void generate(Event evt)
    {
      if (evt is ICommand command)
      {
        command.accept(this);
      }
      else
      {
        
        //pass it to the correct generator
      }
    }

    public void visit(Initialization command)
    {
      //nothing
    }

    public void visit(SymbolTableCreate command)
    {
      IMetaObject symbolTableMeta = command.getSymbolTable();
      MetaObject<Map<string, Var>> variablesMeta = patcher.createMetaObject<Map<string, Var>>();
      ID symbolTableId = symbolTableMeta.getKey();
      ID variablesId = variablesMeta.getKey();
      Path symbolTablePath = new Path(symbolTableId);
      Path variablesPath = new Path(variablesId);

      command.addOperation(new ObjectCreate(symbolTableId, SymbolTableType));
      command.addOperation(new ObjectCreate(variablesId, SymbolTableVariablesType));
      command.addOperation(new ObjectSet(symbolTablePath, SymbolTableVariablesField, variablesPath));
    }

    public void visit(SymbolTableDelete command)
    {
      MetaObject<SymbolTable> symbolTableMeta = command.getSymbolTable();
      SymbolTable symbolTable = symbolTableMeta.getObject();
      Map<string,Var> variables = symbolTable.getVariables();
      IMetaObject variablesMeta = variables.getMetaObject();
      ID symbolTableId = symbolTableMeta.getKey();
      ID variablesId = variablesMeta.getKey();
      Path symbolTablePath = new Path(symbolTableId);

      command.addOperation(new ObjectSet(symbolTablePath, SymbolTableVariablesField, NullValue.Null));
      command.addOperation(new ObjectDelete(variablesId, SymbolTableVariablesType));
      command.addOperation(new ObjectDelete(symbolTableId, SymbolTableType));
    }

    public void visit(SymbolTableStoreVariable command)
    {
      MetaObject<SymbolTable> symbolTableMeta = command.getSymbolTable();
      SymbolTable symbolTable = symbolTableMeta.getObject();
      Map<string,Var> variables = symbolTable.getVariables();
      IMetaObject variablesMeta = variables.getMetaObject();
      ID variablesId = variablesMeta.getKey();
      Path variablesPath = new Path(variablesId);
      
      MetaObject<Var> varMeta = command.getVar();
      ID varId = varMeta.getKey();
      Var var = varMeta.getObject();
      string varName = var.getName();
      Value keyValue = new StringValue(varName);
      Path varPath = new Path(varId);

      command.addOperation(new MapAdd(variablesPath, keyValue));
      command.addOperation(new MapSet(variablesPath, keyValue, varPath));
    }

    public void visit(SymbolTableRemoveVariable command)
    {
      MetaObject<SymbolTable> symbolTableMeta = command.getSymbolTable();
      SymbolTable symbolTable = symbolTableMeta.getObject();
      Map<string,Var> variables = symbolTable.getVariables();
      IMetaObject variablesMeta = variables.getMetaObject();
      ID variablesId = variablesMeta.getKey();
      Path variablesPath = new Path(variablesId);
      
      MetaObject<Var> varMeta = command.getVar();
      Var varValue = varMeta.getObject();
      string varName = varValue.getName();
      Value keyValue = new StringValue(varName);

      command.addOperation(new MapSet(variablesPath, keyValue, NullValue.Null));
      command.addOperation(new MapRemove(variablesPath, keyValue));
    }

    public void visit(VarCreate command)
    {
      MetaObject<Var> varMeta = command.getVar();
      ID varId = varMeta.getKey();
      Path varPath = new Path(varId);
      string name = command.getName();
      Value nameValue = new StringValue(name);

      command.addOperation(new ObjectCreate(varId, VarType));
      command.addOperation(new ObjectSet(varPath, VarNameField, nameValue));
      command.addOperation(new ObjectSet(varPath, VarValueField, NullValue.Null));
    }

    public void visit(VarDelete command)
    {
      MetaObject<Var> varMeta = command.getVar();
      ID varId = varMeta.getKey();
      Path varPath = new Path(varId);
      
      command.addOperation(new ObjectSet(varPath, VarValueField, NullValue.Null));
      command.addOperation(new ObjectSet(varPath, VarNameField, NullValue.Null));
      command.addOperation(new ObjectDelete(varId, VarType));
    }

    public void visit(VarSetValue command)
    {
      MetaObject<Var> var = command.getVar();
      ID varId = var.getKey();
      Path varPath = new Path(varId);
      Value value = command.getNewValue();
      
      command.addOperation(new ObjectSet(varPath, VarValueField, value));
    }

    public void visit(Declaration command)
    {
      //only has post-migration effects
    }

    public void visit(Deletion command)
    {
      //only has post-migration effects
    }

    public void visit(Assignment command)
    {
      //only has post-migration effects
    }

    public void visit(Import command)
    {
      //only has post-migration effects      
    }

    public void visit(Call command)
    {
      //only has post-migration effects
    }

    public void visit(Help command)
    {
      //only has post-migration code that prints the help
    }

    public void visit(Print command)
    {
      //only has post-migration code that prints a variable
    }

    public void visit(Undo command)
    {
      //only has post-migration effects
    }
    
    public void visit(Reconstruct command)
    {
      //only has post-migration effects
    }
  }
}