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
using System;
using Delta.Model.BaseType;
using Delta.Model.QualifiedName;
using Delta.REPL.Model;

namespace Delta.REPL.Runtime
{
  public class Aligner: Delta.Model.BaseType.IVisitor, Delta.Model.QualifiedName.IVisitor
  {
    private readonly MetaObject<SymbolTable> symbolTableMeta;
    private SymbolTable symbolTable;

    public Aligner(MetaObject<SymbolTable> symbolTableMeta)
    {
      this.symbolTableMeta = symbolTableMeta;
    }

    /**
     * Aligns a variable path with the object space, such that the resolver can interpret it.
     */
    public void align(Value value)
    {
      symbolTable = symbolTableMeta.getObject();
      value.accept(this);
    }

    public void visit(IntValue value)
    {
    }

    public void visit(BoolValue value)
    {
    }

    public void visit(StringValue value)
    {
    }

    public void visit(NullValue value)
    {
    }

    public void visit(EnumValue value)
    {
    }

    public void visit(Path path)
    {
      Name name = path.getName();
      Name childName = name.getName();
      
      //if the head of the path is a variable name
      if (name is Field field)
      {
        string varName = field.getFieldName();

        //lookup the variable, which must refer to an object
        Var var = symbolTable.getVar(varName);
        object value = var.getValue();
        if (value is IPatchable patchable)
        {
          IMetaObject metaObject = patchable.getMetaObject();
          ID id = metaObject.getKey();
          Lookup lookup = new Lookup(id);
          path.setName(lookup);
          lookup.setName(childName);
        }
        else
        {
          IMetaObject metaObject = var.getMetaObject();
          ID id = metaObject.getKey();
          Lookup lookup = new Lookup(id);
          path.setName(lookup);
          Field childField = new Field("value");
          lookup.setName(childField);
        }
      }

      //visit child name
      if (childName != null)
      {
        childName.accept(this);
      }
    }

    public void visit(Location value)
    {
    }

    public void visit(UUID value)
    {
    }

    public void visit(Field name)
    {
      Name childName = name.getName();
      if (childName != null)
      {
        childName.accept(this);
      }
    }

    public void visit(Lookup name)
    {
      Value value = name.getKey();
      value.accept(this);
      Name childName = name.getName();
      if (childName != null)
      {
        childName.accept(this);
      }
    }
  }
}