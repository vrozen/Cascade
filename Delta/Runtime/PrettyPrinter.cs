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
using System.Reflection;
using System.Text;
using Delta.Model.BaseType;
using Delta.Model.BaseOperation;
using Delta.Model.DesignSpace;
using Delta.Model.QualifiedName;

namespace Delta.Runtime
{
  public class PrettyPrinter : Model.DesignSpace.IVisitor, Model.BaseOperation.IVisitor,
    Model.QualifiedName.IVisitor, Model.BaseType.IVisitor
  {
    private StringBuilder buf;
    private int indent;

    /*
    public void setIndent(int indent)
    {
      this.indent = indent;
    }*/

    private void appendSpaces()
    {
      for (int i = 0; i < indent; i++)
      {
        buf.Append(' ');
      }
    }

    /*
     * Pretty-prints an event.
     * @param evt event to serialize
     * @note public API
     */
    public string print(Event evt)
    {
      buf = new StringBuilder();
      evt.accept(this);
      return buf.ToString();
    }

    public string printSignature(Event evt)
    {
      buf = new StringBuilder();
      appendSignature(evt);
      return buf.ToString();
    }
    
    /*
     * Pretty-prints a history of events.
     * @param h history to serialize
     * @note public API
     */
    public string print(History h)
    {
      buf = new StringBuilder();
      h.accept(this);
      return buf.ToString();
    }

    public string print(Operation op)
    {
      buf = new StringBuilder();
      op.accept(this);
      return buf.ToString();
    }

    public string print(Name name)
    {
      buf = new StringBuilder();
      name.accept(this);
      return buf.ToString();
    } 
    
    public string print(Value val)
    {
      buf = new StringBuilder();
      val.accept(this);
      return buf.ToString();
    }

    public string printAt(Value val)
    {
      buf = new StringBuilder();
      val.accept(this);

      Location loc = val.getLocation();
      if(loc != null)
      {
        string at = printAt(loc);
        buf.Append(at);
      }
      
      return buf.ToString();
    }
    
    public string printAt(Name name)
    {
      buf = new StringBuilder();
      name.accept(this);

      Location loc = name.getLocation();
      if(loc != null)
      {
        string at = printAt(loc);
        buf.Append(at);
      }
      
      return buf.ToString();
    }

    public string printAt(Location loc)
    {
      string r = "";
      if (loc != null)
      {
        r = " at line " + loc.getBeginLine() + " column " + loc.getBeginColumn();
      }
      return r;
    }
    
    public void visit(History history)
    {
      Juncture head = history.getHead();
      Juncture j = head;
      List<Event> events = new List<Event>();
      while (j.getPrevious() != null)
      {
        j = j.getPrevious();
        events.add((j.getEvent()));
      }
      events.Reverse();
      foreach (Event e in events)
      {
        e.accept(this);
      }
    }
    
    private void appendSignature(Event evt)
    {
      string eventName = evt.getTypeName();
      buf.Append(eventName);
      buf.Append('(');

      BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      Type type = evt.GetType();
      FieldInfo[] info = type.GetFields(flags);

      for (int pos = 0; pos < info.Length; pos++)
      {
        FieldInfo field = info[pos];
        object value = field.GetValue(evt);

        appendArgument(value);
        if (pos < info.Length - 1)
        {
          buf.Append(',');
        }
      }

      buf.Append(')');
    }

    private void appendArgument(object obj)
    {
      if (obj is IMetaObject metaObject)
      {
        UUID id = (UUID) metaObject.getKey();
        buf.Append("uuid");
        buf.Append(id.getKey());
      }
      else if (obj is Value value)
      {
        value.accept(this);
      }
      else if (obj is Field field)
      {
        string strVal = field.getFieldName();
        buf.Append('"');
        buf.Append(strVal);
        buf.Append('"');
      }
      else if (obj is string)
      {
        buf.Append('"');
        buf.Append(obj);
        buf.Append('"');
      }
      else
      {
        buf.Append(obj);
      }
    }

    public void visit(Effect effect)
    {
      List<Operation> operations = effect.getOperations();
      
      appendSpaces();
      appendSignature(effect);
      buf.Append(" {\n");
      indent += 2;
      List<Event> preEffects = effect.getPreEffects();
      if (preEffects.Count > 0)
      {
        appendSpaces();
        buf.Append("//pre effects\n");
        foreach (Event pre in preEffects)
        {
          pre.accept(this);
        }
      }
      appendSpaces();
      buf.Append("//operations\n");
      foreach(Operation op in operations)
      {
        op.accept(this);
        buf.Append("\n");
      }
      List<Event> postEffects = effect.getPostEffects();
      if (postEffects.Count > 0)
      {
        appendSpaces();
        buf.Append("//post effects\n");
        foreach (Event post in postEffects)
        {
          post.accept(this);
        }
      }
      indent -= 2;
      appendSpaces();      
      buf.Append("}\n");
    }
    
    public void visit(Trigger cause)
    {
      appendSpaces();
      //buf.Append("cause");
      //buf.Append(' ');
      appendSignature(cause);
      buf.Append(" {\n");
      indent += 2;
      List<Event> postEffects = cause.getPostEffects();
      if (postEffects.Count > 0)
      {
        appendSpaces();
        buf.Append("//post effects\n");
        foreach (Event post in postEffects)
        {
          post.accept(this);
        }
      }
      indent -= 2;
      appendSpaces();      
      buf.Append("}\n");
    }
    
    public void visit(Signal signal)
    {
      appendSpaces();
      //buf.Append("signal");
      //buf.Append(' ');
      appendSignature(signal);
      buf.Append("\n");
    }

    public void visit(ObjectCreate op)
    {
      appendSpaces();
      ID id = op.getId();
      StringValue className = op.getClassName();
      buf.Append('[');
      id.accept(this);
      buf.Append(']');
      buf.Append(' ');
      buf.Append('=');
      buf.Append(' ');
      buf.Append("new");
      buf.Append(' ');
      className.accept(this);
      buf.Append('(');
      buf.Append(')');
    }

    public void visit(ObjectDelete op)
    {
      appendSpaces();
      ID id = op.getId();
      buf.Append("delete");
      buf.Append(' ');
      buf.Append('[');
      id.accept(this);
      buf.Append(']');
    }

    public void visit(ObjectKey op)
    {
      appendSpaces();
      ID oldId = op.getId();
      ID newId = op.getNewId();
      buf.Append("key");
      buf.Append('(');
      oldId.accept(this);
      buf.Append(',');
      newId.accept(this);
      buf.Append(')');
    }

    public void visit(ObjectSet op)
    {
      appendSpaces();
      Path path = op.getPath();
      Field field = op.getField();
      Value value = op.getNewValue();
      path.accept(this);
      if(field!=null)
        field.accept(this);
      else
        buf.Append("null");
      buf.Append(' ');
      buf.Append('=');
      buf.Append(' ');
      if (value != null)
        value.accept(this);
      else
        buf.Append("null");
    }

    public void visit(ListInsert op)
    {
      appendSpaces();
      Path path = op.getPath();
      IntValue index = op.getIndex();
      Value value = op.getValue();
      path.accept(this);
      buf.Append('.');
      buf.Append("insert");
      buf.Append('(');
      index.accept(this);
      buf.Append(',');
      if (value != null)
        value.accept(this);
      else
        buf.Append("null");
      buf.Append(')');
    }

    public void visit(ListRemove op)
    {
      appendSpaces();
      Path path = op.getPath();
      IntValue index = op.getIndex();
      Value value = op.getValue();
      path.accept(this);
      buf.Append('.');
      buf.Append("remove");
      buf.Append('(');
      index.accept(this);
      buf.Append(',');
      if (value != null)
        value.accept(this);
      else
        buf.Append("null");
      buf.Append(')');
    }

    public void visit(ListPush op)
    {
      appendSpaces();
      Path path = op.getPath();
      Value value = op.getValue();
      path.accept(this);
      buf.Append('.');
      buf.Append("push");
      buf.Append('(');
      if (value != null)
        value.accept(this);
      else
        buf.Append("null");
      buf.Append(')');
    }

    public void visit(ListPop op)
    {
      appendSpaces();
      Path path = op.getPath();
      path.accept(this);
      buf.Append('.');
      buf.Append("pop");
      buf.Append('(');
      buf.Append(')');
    }
    
    public void visit(ListSet op)
    {
      appendSpaces();
      Path path = op.getPath();
      Value index = op.getIndex();
      Value value = op.getNewValue();
      path.accept(this);
      buf.Append('[');
      index.accept(this);
      buf.Append(']');
      buf.Append(' ');
      buf.Append('=');
      buf.Append(' ');
      if (value != null)
        value.accept(this);
      else
        buf.Append("null");
    }

    public void visit(SetAdd op)
    {
      appendSpaces();
      Path path = op.getPath();
      Value value = op.getValue();
      path.accept(this);
      buf.Append('.');
      buf.Append("add");
      buf.Append('(');
      if (value != null)
        value.accept(this);
      else
        buf.Append("null");
      buf.Append(')');
    }

    public void visit(SetRemove op)
    {
      appendSpaces();
      Path path = op.getPath();
      Value value = op.getValue();
      
      path.accept(this);
      buf.Append('.');
      buf.Append("remove");
      buf.Append('(');
      if (value != null)
        value.accept(this);
      else
        buf.Append("null");
      buf.Append(')');
    }

    public void visit(MapAdd op)
    {
      appendSpaces();
      Path path = op.getPath();
      Value key = op.getKey();
      path.accept(this);
      buf.Append('.');
      buf.Append("add");
      buf.Append('(');
      if (key != null)
        key.accept(this);
      else
        buf.Append("null");
      buf.Append(')');
    }

    public void visit(MapRemove op)
    {
      appendSpaces();
      Path path = op.getPath();
      Value key = op.getKey();
      path.accept(this);
      buf.Append('.');
      buf.Append("remove");
      buf.Append('(');
      if (key != null)
        key.accept(this);
      else
        buf.Append("null");
      buf.Append(')');
    }
    
    public void visit(MapSet op)
    {
      appendSpaces();
      Path path = op.getPath();
      Value key = op.getKey();
      Value value = op.getNewValue();
      path.accept(this);
      buf.Append('[');
      if (key != null)
        key.accept(this);
      else
        buf.Append("null");
      buf.Append(']');
      buf.Append(' ');
      buf.Append('=');
      buf.Append(' ');
      if (value != null)
        value.accept(this);
      else
        buf.Append("null");
    }

    public void visit(NullValue value)
    {
      buf.Append("null");
    }

    public void visit(Path path)
    {
      Name name = path.getName();
      name.accept(this);
    }

    public void visit(Field name)
    {
      string fieldName = name.getFieldName();
      Name child = name.getName();
      buf.Append(fieldName);
      if (child != null)
      {
        buf.Append('.');
        child.accept(this);
      }
    }
    
    public void visit(Lookup name)
    {
      Value key = name.getKey();
      Name child = name.getName();
      buf.Append('[');
      key.accept(this);
      buf.Append(']');
      if (child != null)
      {
        child.accept(this);
      }
    }

    public void visit(UUID value)
    {
      int key = value.getKey();
      buf.Append("|uuid://");
      buf.Append(key);
      buf.Append("|");
    }

    public void visit(Location value)
    {
      buf.Append( "|loc://");
      buf.Append(value.getPath());
      buf.Append('(');
      buf.Append(value.getOffset());
      buf.Append(',');
      buf.Append(value.getLength());
      buf.Append('<');
      buf.Append(value.getBeginLine());
      buf.Append(',');
      buf.Append(value.getBeginColumn());
      buf.Append('>');
      buf.Append(',');
      buf.Append('<');
      buf.Append(value.getEndLine());
      buf.Append(',');
      buf.Append(value.getEndColumn());
      buf.Append('>');
      buf.Append(')');
      buf.Append('|');
    }

    public void visit(IntValue value)
    {
      int iVal = value.getValue();
      buf.Append(iVal);
    }

    public void visit(BoolValue value)
    {
      bool bVal = value.getValue();
      if (bVal)
      {
        buf.Append("true");
      }
      else
      {
        buf.Append("false");
      }
    }

    public void visit(StringValue value)
    {
      string sVal = value.getValue();
      buf.Append('"');
      buf.Append(sVal);
      buf.Append('"');
    }

    public void visit(EnumValue value)
    {
      object eVal = value.getValue();
      buf.Append(eVal);
    }
  }
}