# Cascade
Live programming brings code to life with immediate and continuous feedback. To enjoy its benefits, programmers need powerful languages and live programming environments for understanding the effects of coding actions and developing running programs. We aim to deliver the enabling technology that powers these languages. We introduce Cascade, a meta-language and framework for expressing languages with interface- and feedback-mechanisms that drive live programming.

## Cascade Framework
The Cascade framework consists of a compiler and a runtime.
Its sources are avaiable under the 3-clause BSD license.

* **TEL.** Transformation Effect Language (TEL) is the working title of the Cascade compiler.
This compiler is based on Rascal, a language work bench and meta-programming language developed at Centrum Wiskunde & Informatica.
https://www.rascal-mpl.org

* **Delta.** Detla is a virtual machine based on C# for running Cascade programs.
When executing user actions or coding actions, Delta generates cause-and-effect chains as transactions of edit operations in response.
As a result, it can account for run-time eventualities such as run-time state migrations.

## Instructions

### Running the TEL IDE
Cascade includes an Integrated Development Environment for creating programming languages.
Running TEL requires installing Rascal and importing the project.
Integrating TEL into the Eclipse IDE requires running the following commands.
```
import lang::tel::IDE;
tel_register();
```

### Running the TEL Compiler
Cascade offers a compiler for transforming TEL sources into C# code.
Running the compiler for TinyLiveSML requires the following commands
```
import lang::tel::Compiler;
compile(|project://TEL/models/TinyLiveSML|);
```
The C# sources will be generated into TEL/generated/Delta/TinyLiveSML.

### Running the Delta Runtime
Code generated by the TEL compiler integrates with Delta's sources.
For compiling the sources we have used JetBrains Rider.
Delta's serves its web-based user interfac on http://127.0.0.1:8000



