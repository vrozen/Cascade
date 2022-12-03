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
module lang::tel::Compiler

import IO;
import String;

import lang::tel::AST;
import lang::tel::Printer;
import lang::tel::Checker;
import lang::tel::Desugar;
import lang::tel::Specializer;
import lang::tel::Hierarchy;
import lang::tel::gen::Util;
import lang::tel::gen::Model;
import lang::tel::gen::Operation;
import lang::tel::gen::Generator;
import lang::tel::gen::Language;
import lang::tel::gen::Inverter;
import lang::tel::gen::PreMigrator;
import lang::tel::gen::Marshaller;

public loc TinyLiveSML = |project://TEL/models/TinyLiveSML|;
public loc LiveSML = |project://TEL/models/LiveSML|;
public loc LiveQL = |project://TEL/models/LiveQL|;
public loc LiveMM = |project://TEL/models/LiveMM|;
public loc LiveBT = |project://TEL/models/LiveBT|;

public void doTinyLiveSML() = compile(TinyLiveSML);

data Unit
  = unit(Package specializedInput, Package model)
  | unit(Package resolvedInput, Package model, Package operation);

public void compile(loc dir){
  //phase 1: generate models en extract enums
  //println("Compiler Phase 1");
  set[Unit] units = {};
  units = compileRec(dir, units);
  
  println("Phase 2: Extract enums");  
  set[Package] eachModelPre = {u.model | u <- units};
  map[ID,list[ID]] enums = getEnums(eachModelPre);
  
  //phase 2: resolve enums and generate operations
  //println("Compiler Phase 2");
  set[Unit] units2 = {};
  for(Unit u <- units){
    println("Phase 2: Process package <toString(u.specializedInput.name)>");
  
    println("Phase 2: Resolve enums");
    Package resolvedInput = resolveEnums(u.specializedInput, enums);
    Package model = resolveEnums(u.model, enums);
    
    println("Phase 2: Generate events");    
    Package operation = genOperationPackage(resolvedInput);
    units2 += {unit(resolvedInput, model, operation)};
  }
  
  //phase 3: compile packages
  set[Package] eachModel = {u.model | u <- units2};
  set[Package] eachOperation = {u.operation | u <- units2};
  
  for(Unit u <- units2){ 
    compilePackage(u.resolvedInput, u.model, u.operation, eachModel, eachOperation);
  }
}

public set[Unit] compileRec(loc dir, set[Unit] runits){
  bool package = false;  
  for(loc file <- dir.ls){
    if(file.extension == "tel"){
      package = true;
    }
    else if(file.extension == ""){
      runits = compileRec(file, runits);
    }
  }
  if(package == true){
    println("Phase 1: Read directory <dir>");
    runits = runits + {compilePackage(dir)};
  }
  return runits;
}

public Unit compilePackage(loc dir){
  println("Phase 1: Parse files");
  Package input = loadPackage(dir);

  println("Phase 1: Desugar package");
  Package desugaredInput = desugar(input);

  println("Phase 1: Type check package");
  map[loc,Typ] types = check(desugaredInput);
  
  println("Phase 1: Specialize edit operations");
  Package specializedInput = specialize(desugaredInput, types);
  
  println("Phase 1: Generate model");  
  Package model = genModelPackage(specializedInput);
  //Package operation = genOperationPackage(input);
  
  return unit(specializedInput, model);
}

public Package filterMethods(Package p) =
  visit(p)
  {
    case list[Event] evts =>
      [e | e <- evts, h_method (Vis vis, Imp imp, Poly poly, Typ rtyp, ID name, list[Param] params, Body body) !:= e]
  };

public void compilePackage(Package resolvedInput, Package model, Package operation, 
                           set[Package] eachModel, set[Package] eachOperation){
  Package filteredInput = filterMethods(resolvedInput);
                        
  println("Phase 3: Process package <toString(resolvedInput.name)>");              
  println("Phase 3: Generate code generator");
  Package generator = genGenerator(filteredInput, model, operation);

  println("Phase 3: Generate language interface");
  Package language = genLanguage(filteredInput, model, operation);
  
  println("Phase 3: Check inverse relation");
  map[ID,ID] inverseRelation = checkInverseRelation(filteredInput);  

  println("Phase 3: Generate event inverter");
  Package inverter = genInverter(filteredInput, model, operation, inverseRelation);
    
  println("Phase 3: Generate event migrators");
  Package migrator = genMigrator(filteredInput, model, operation, eachModel, eachOperation);
  
  println("Phase 3: Generate marshaller");
  Package marshaller = genMarshaller(filteredInput, model);
  
  //writeFile(|project://TEL/models/LiveQL/test.txt|, migrator);

  println("Phase 3: Generate C# code");  
  writePackage(model);
  writePackage(operation);
  writePackage(generator);
  writePackage(language);
  writePackage(inverter);
  writePackage(migrator);
  writePackage(marshaller);
}


public Package loadPackage(loc dir)
{
  Package PackageNone = package(using([]), name(id("")),[]);
  Package g = PackageNone;
  
  for(loc file <- dir.ls, file.extension == "tel")
  {
    //println("Building <file>");
    Package p = tel_build(file);
    
    if(g == PackageNone)
    {
      g = p;     
    }
    else if(g.name == p.name)
    {
      //Using using = using([name(n.head,n.path+[p_field(id("Model"))])| Name n <- p.using.units]);
      
      g.using = combine([g.using, p.using]);
 
      g.units += p.units;
    }
    else
    {
      throw "Load package failed. Expected package name <toString(g.name)> found <toString(p.name)>";
    }
  }
  
  return g;
}

private void writePackage(Package p)
{
  str packageName = lang::tel::Printer::print(p.name);  
  loc packageDir = |project://TEL/generated/<replaceAll(packageName,".","/")>|;

  for(Unit u <- p.units)
  {
    str unitFile = toString(u.name) + ".cs";
    loc unitDir = packageDir + "/" + unitFile;

    str unitContent = toString(p, u);
    
    writeFile(unitDir, unitContent);
  }
}