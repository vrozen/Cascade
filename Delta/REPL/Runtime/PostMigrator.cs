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
using System.Linq;
using System.Reflection;
using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.Model.QualifiedName;
using Delta.REPL.Operation;
using Delta.REPL.Model;
using Delta.Runtime;

namespace Delta.REPL.Runtime
{
  public class PostMigrator : IPostMigrator, Delta.REPL.Operation.IVisitor
  {
    private readonly Engine engine;
    private readonly Patcher patcher;
    private readonly Language language;
    private readonly PrettyPrinter printer;
    private readonly Resolver resolver;
    private readonly Aligner aligner;
    private readonly Dispatcher dispatcher;
    private readonly MetaObject<SymbolTable> symbolTableMeta;

    public PostMigrator(Engine engine, Language language, Aligner aligner, MetaObject<SymbolTable> symbolTableMeta)
    {
      this.engine = engine;
      this.language = language;
      this.aligner = aligner;
      this.symbolTableMeta = symbolTableMeta;

      dispatcher = engine.getDispatcher();
      patcher = engine.getPatcher();
      printer = patcher.getPrettyPrinter();
      resolver = patcher.getResolver();
    }

    public void postMigrate(Event evt)
    {
      if (evt is ICommand command)
      {
        command.accept(this);
      }
    }

    public void visit(Initialization command)
    {
      Effect create = new SymbolTableCreate(symbolTableMeta);
      command.addPostEffect(create);
    }

    public void visit(SymbolTableCreate command)
    {
      //nothing
    }

    public void visit(SymbolTableDelete command)
    {
      //nothing
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
      //Todo: decide if cleanup goes here
      //MetaObject<Var> varMeta = command.getVar();
      //Var var = varMeta.getObject();
      //var.setMetaObject(null);
    }

    public void visit(VarSetValue command)
    {
      //nothing
    }

    public void visit(Declaration command)
    {
      Field field = command.getVarName();
      string varName = field.getFieldName();

      SymbolTable symbolTable = symbolTableMeta.getObject();

      Map<string, Var> vars = symbolTable.getVariables();

      if (vars.containsKey(varName))
      {
        throw new PatchException("Duplicate declaration " + varName);
      }

      MetaObject<Var> varMeta = patcher.createMetaObject<Var>();

      command.addPostEffect(new VarCreate(varMeta, varName));
      command.addPostEffect(new SymbolTableStoreVariable(symbolTableMeta, varMeta));
    }

    public void visit(Deletion command)
    {
      Field field = command.getVarName();
      string varName = field.getFieldName();
      SymbolTable symbolTable = symbolTableMeta.getObject();

      if (symbolTable.contains(varName) == false)
      {
        throw new PatchException("Missing variable " + varName);
      }

      Var var = symbolTable.getVar(varName);
      MetaObject<Var> varMeta = (MetaObject<Var>) var.getMetaObject();

      command.addPostEffect(new SymbolTableRemoveVariable(symbolTableMeta, varMeta));
      command.addPostEffect(new VarDelete(varMeta, varName));
    }

    public void visit(Assignment command)
    {
      Field field = command.getVarName();
      string varName = field.getFieldName();
      SymbolTable symbolTable = symbolTableMeta.getObject();

      if (symbolTable.contains(varName) == false)
      {
        //todo: add locations to commands and use those instead
        Location loc = command.getValue().getLocation();
        throw new PatchException("Missing variable '" + varName + "' in assignment " + printer.printAt(loc));
      }

      Var var = symbolTable.getVar(varName);
      MetaObject<Var> varMeta = (MetaObject<Var>) var.getMetaObject();

      Value value = command.getValue();
      aligner.align(value); //replaces variables by id's

      Object obj = resolver.resolve(value);
      
      Value resolvedValue = Patcher.encode(obj);

      command.addPostEffect(new VarSetValue(varMeta, resolvedValue));
    }

    public void visit(Import command)
    {
      Name languageName = command.getLanguageName();
      string className = printer.print(languageName);
      className = className.Substring(1, className.Length - 1); //HACK ouch FIXME todo
      Type type = Type.GetType(className);

      if (type != null)
      {
        object[] args = {patcher, this.language};
        ILanguage lang = (Language) Activator.CreateInstance(type, args);
        dispatcher.addLanguage(lang);
        string name = language.getName();
        Console.Out.WriteLine("Registered language "+name);
      }
      else
      {
        Console.Out.WriteLine("Unable to find language "+className);
      }
    }
    
    public void visit(Call command)
    {
      Name qualifiedName = command.getEventName();
      Type eventType = dispatcher.resolveEventName(qualifiedName);
      
      List<Value> operands = command.getOperands();
      BindingFlags[] flags = {BindingFlags.Public};
      ConstructorInfo ci = eventType.GetConstructors()[0];
      ParameterInfo[] paramInfos = ci.GetParameters();

      if (paramInfos.Length != operands.Count)
      {
        throw new PatchException("Expected number of parameters " + paramInfos.Length + " found " + operands.Count);
      }

      Assignment assignment = null;
      object[] args = new object[paramInfos.Length];

      for(int pos = 0; pos < paramInfos.Length; pos++)
      {
        ParameterInfo paramInfo = paramInfos[pos];
        Value operand = (Value) operands.elementAt(pos);
        Type paramType = paramInfo.ParameterType;
        bool isFutureParameter = Attribute.IsDefined(paramInfo, typeof(Future));
        
        if (isFutureParameter)
        {
          Field field;
          if (operand is Path path && path.getName() is Field aField)
          {
            field = aField;
          }
          else
          {
            throw new PatchException("Expected variable "+printer.printAt(operand));
          }
          paramType = getMetaObjectType(qualifiedName);
          
          IMetaObject metaObject = patcher.createMetaObject(paramType);
          args[pos] = metaObject;
          //MetaObject<Language> languageMeta = command.getLanguageMeta();

          ID id = metaObject.getKey();
          Value value = new Path(new Lookup(id));

          assignment = new Assignment(field, value);
        }
        else //normal parameter
        {
          aligner.align(operand); //replaces variables by id's
          Object obj = resolver.resolve(operand);
          
          if (obj is IPatchable patchable)
          {
            obj = patchable.getMetaObject();
          }
          else
          {
            obj = Patcher.encode(obj);
          }

          args[pos] = obj;
        }

        Type expectedType = paramType;
        Type foundType = args[pos].GetType();

        if (args[pos] == null)
        {
          throw new PatchException("Missing operand of type " + expectedType + printer.printAt(operand));
        }
        if (foundType != expectedType)
        {
          if (expectedType != typeof(IMetaObject)) //todo check it is MetaObject<T>
          {
            throw new PatchException("Expected parameter type " + paramType +
                                     ", found " + args[pos].GetType() + printer.printAt(operand));
          }
        }
      }

      Event evt = (Event) Activator.CreateInstance(eventType, args);

      command.addPostEffect(evt);

      if (assignment != null)
      {
        command.addPostEffect(assignment);
      }
    }

    private Type getMetaObjectType(Name qualifiedName)
    {
      String name = printer.print(qualifiedName);
      int lastDot = name.LastIndexOf('.');
      String nameMinusEvent = name.Substring(0, lastDot); 
      lastDot = nameMinusEvent.LastIndexOf('.');
      String className = nameMinusEvent.Substring(lastDot+1);
      String packageName = nameMinusEvent.Substring(0, lastDot);
      String fullName = packageName + ".Model." + className;
      Type basePramType = Type.GetType(fullName);      //get the type T of the parameter (without meta-wrap)
      Type metaType = typeof(MetaObject<>);
      Type[] typeArgs = { basePramType };
      Type paramType = metaType.MakeGenericType(typeArgs);  //obtain the meta-object type MetaObject<T>
      return paramType;
    }

    public void visit(Help command)
    {
    }

    public void visit(Print command)
    {
      Field field = command.getVarName();
      string varName = field.getFieldName();
      SymbolTable st = symbolTableMeta.getObject();
      Var var = st.getVar(varName);
      object value = var.getValue();
      string sValue = "";
      if (value == null)
      {
        sValue = "null";
      }
      else if (value is Value v)
      {
        sValue = printer.print(v);
      }
      else
      {
        sValue = value.ToString();
      }
      language.write(sValue);
    }

    public void visit(Undo command)
    {
      /*History history = patcher.getHistory();
      Moment moment = history.getHead();
      if (moment is Juncture juncture)
      {
        Event lastEvent = juncture.getEvent();
        Moment previousMoment = juncture.getPrevious();
        Event iLastEvent = Delta.Runtime.Inverter.reverse(lastEvent);
        engine.schedule(iLastEvent);
        history.setHead(previousMoment);
      }*/
    }

    public void visit(Reconstruct command)
    {
      History history = patcher.getHistory();
      Juncture head = history.getHead();
      Event reverseEvent = head.getEvent();
      Event causeEvent = reverseEvent.getCause();
      List<Event> postEffects = causeEvent.getPostEffects();
      int siblingIndex = -1;
      for(int i = 0; i<postEffects.Count; i++)
      {
        if (postEffects[i] == reverseEvent)
        {
          siblingIndex = i - 1;
          break;
        }
      }
      if (siblingIndex != -1)
      {
        //reverse the effects of the immediate predecessor (sibling in the causing event) 
        Event siblingEvent = (Event) postEffects.elementAt(siblingIndex);
        Event iEffect = Delta.Runtime.Inverter.reverse(siblingEvent);
        command.addPostEffect(iEffect);
      }
    }
  }
}