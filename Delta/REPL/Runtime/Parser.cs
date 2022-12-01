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
using Antlr4.Runtime.Tree.Xpath;
using Delta.Model.BaseType;
using Delta.Model.QualifiedName;
using Delta.REPL.Operation;
using Delta.Runtime;

namespace Delta.REPL.Runtime
{
  public class Parser
  {
    private readonly Scanner scanner;
    private readonly PrettyPrinter printer;

    //private readonly MetaObject<Language> language; //hack

    public Parser() //MetaObject<Language> language
    {
      this.scanner = new Scanner();
      this.printer = new PrettyPrinter();
      //this.language = language;
    }
    
    public ICommand parseCommand(string command, string path)
    {
      scanner.setInput(command, path);
      return parseCommand();
    }
    
    public List<ICommand> parseProgram(string program, string path)
    {
      List<ICommand> commands = new List<ICommand>();
      scanner.setInput(program, path);
      while (scanner.hasToken())
      { 
        ICommand command = parseCommand();
        commands.add(command);
      }
      return commands;
    }
    
    private ICommand parseCommand()
    {
      ICommand command = null;
      Token firstToken = scanner.peek();
      Token.Type firstTokenType = firstToken.getTokenType();
      Location firstTokenLoc = firstToken.getLocation();
      string str = firstToken.getString();

      if (firstTokenType != Token.Type.IDENTIFIER)
      {
        int line = firstTokenLoc.getBeginLine();
        int col = firstTokenLoc.getBeginColumn();
        throw new ParserException("Expected IDENTIFIER, found " + firstTokenType +
                                  " at line " + line + " column " + col);
      }
      
      if (str.Equals("help"))
      {
        scanner.accept();
        command = new Help();
      }
      else if (str.Equals("undo"))
      {
        scanner.accept();
        command = new Undo();
      }
      else if (str.Equals("reconstruct"))
      {
        scanner.accept();
        command = new Reconstruct();
      }
      else if (str.Equals("var"))
      {
        scanner.accept();
        Field name = parseName();
        command = new Declaration(name);
      }
      else if (str.Equals("delete"))
      {
        scanner.accept();
        Field name = parseName();
        command = new Deletion(name);          
      }
      else if (str.Equals("import"))
      {
        scanner.accept();
        Name name = parseName();
        command = new Import(name);
      }
      else //event call or assignment
      {
        Field name = parseName();
        Token secondToken = scanner.peek();
        Token.Type secondTokenType = secondToken.getTokenType();

        if (secondTokenType == Token.Type.SEMICOLON)
        {
          command = new Print(name);
        }
        else if (secondTokenType == Token.Type.LPAREN)
        {
          command = parseTrigger(name);
        }
        else if (secondTokenType == Token.Type.EQUALS)
        { 
          scanner.accept();
          Value value = parseValue();
          command = new Assignment(name, value);
        }
        else
        {
          Location secondTokenLoc = secondToken.getLocation();
          int line = secondTokenLoc.getBeginLine();
          int col = secondTokenLoc.getBeginColumn();
          throw new ParserException("Expected LPAREN or EQUALS, found " + secondTokenType +
                                    " at line " + line + " column " + col);
        }
      }
      
      scanner.accept(Token.Type.SEMICOLON);
      command.setLocation(firstTokenLoc);
      return command;
    }
    
    private ICommand parseTrigger(Name evenName)
    {
      List<Value> operands = new List<Value>();
      scanner.accept(Token.Type.LPAREN);

      Token lookAheadToken = scanner.peek();
      Token.Type lookAheadTokenType = lookAheadToken.getTokenType();

      if (lookAheadTokenType == Token.Type.RPAREN)
      {
        scanner.accept();
      }

      while(lookAheadTokenType != Token.Type.RPAREN)
      {
        Value value = parseValue(); 
        operands.add(value);
        
        lookAheadToken = scanner.peek(); 
        lookAheadTokenType = lookAheadToken.getTokenType();
        switch (lookAheadTokenType)
        {
          case Token.Type.RPAREN:
            scanner.accept();
            break;
          case Token.Type.COMMA:
            scanner.accept();
            break;
          default:
            Location loc = lookAheadToken.getLocation();
            int line = loc.getBeginLine();
            int col = loc.getBeginColumn();
            throw new ParserException("Expected LPAREN or COMMA at line " + line + " column " + col);
            break;
        }
      }

      return new Call(evenName, operands);
    }

    private Field parseName()
    {
      Token headToken = scanner.accept(Token.Type.IDENTIFIER);
      string headTokenString = headToken.getString();
      Field field = new Field(headTokenString);
      
      Name curName = field;
      bool inName = true;
      while (inName)
      {
        Token lookAheadToken = scanner.peek();
        Token.Type lookAheadTokenType = lookAheadToken.getTokenType();
        
        switch (lookAheadTokenType)
        {
          case Token.Type.DOT:
            scanner.accept();
            Token fieldToken = scanner.accept(Token.Type.IDENTIFIER);
            string fieldTokenString = fieldToken.getString();
            Location fieldTokenLoc = fieldToken.getLocation();
            Field childField = new Field(fieldTokenString);
            field.setLocation(fieldTokenLoc);
            curName.setName(childField);
            curName = childField;
            break;
          case Token.Type.LBRACKET:
            Token bracketToken = scanner.accept();
            Location bracketTokenLoc = bracketToken.getLocation();
            Value value = parseValue();
            scanner.accept(Token.Type.RBRACKET);
            Lookup lookup = new Lookup(value);
            lookup.setLocation(bracketTokenLoc);
            curName.setName(lookup);
            curName = lookup;
            break;
          default:
            inName = false;
            break;
        }
      }
      return field;
    }

    private Value parseValue()
    {
      Value value = null;
      Token lookAheadToken = scanner.peek();
      scanner.setAcceptWhiteSpace(false);
      Token.Type lookAheadTokenType = lookAheadToken.getTokenType();

      switch (lookAheadTokenType)
      {
        case Token.Type.INTEGER:
          Token intToken = scanner.accept();
          int iVal = parseInt(intToken);
          Location intLoc = intToken.getLocation();
          value = new IntValue(iVal);
          value.setLocation(intLoc);
          break;
        case Token.Type.TRUE:
          Token trueToken = scanner.accept();
          Location trueLocation = trueToken.getLocation();
          value = new BoolValue(true);
          value.setLocation(trueLocation);
          break;
        case Token.Type.FALSE:
          Token falseToken = scanner.accept();
          Location falseLocation = falseToken.getLocation();
          value = new BoolValue(false);
          value.setLocation(falseLocation);
          break;
        case Token.Type.NULL:
          scanner.accept();
          value = null; //todo: introduce a null class
          break;
        case Token.Type.STRING:
          Token stringToken = scanner.accept();
          Location stringLocation = stringToken.getLocation();
          string stringVal = stringToken.getString();
          value = new StringValue(stringVal);
          value.setLocation(stringLocation);
          break;
        case Token.Type.IDENTIFIER:
          value = parsePath();
          
          //check if the path is an enum and replace it
          value = checkEnum(value);
          break;
        case Token.Type.PIPE:
          value = parseId();
          break;
        default:
          Location loc = lookAheadToken.getLocation();
          int line = loc.getBeginLine();
          int col = loc.getBeginColumn();
          throw new ParserException("Expected value, found token " + lookAheadTokenType + " at line " + line + " column " + col);
          break;
      }
      
      scanner.setAcceptWhiteSpace(true);
      return value;
    }

    private int parseInt(Token token)
    {
      string sVal = token.getString();
      int iVal = 0;
      bool success = int.TryParse(sVal, out iVal);
      if(success == false)
      {
        Location loc = token.getLocation();
        int line = loc.getBeginLine();
        int col = loc.getBeginColumn();
        throw new ParserException("Expected integer at line " + line + " column " + col);
      }
      return iVal;
    }

    private Value parsePath()
    {
      Name name = parseName();
      return new Path(name);
    }
    
    private Value parseId()
    {
      Value value = null;
      Token pipeToken = scanner.accept(Token.Type.PIPE);
      Token headToken = scanner.accept(Token.Type.IDENTIFIER);

      Location pipeLocation = pipeToken.getLocation();
      
      string head = headToken.getString();

      if (head.Equals("loc"))
      {
        value = parseLocation();
        value.setLocation(pipeLocation);
      }
      else if (head.Equals("uuid"))
      {
        value = parseUuid();
        value.setLocation(pipeLocation);
      }
      else
      {
        Location loc = headToken.getLocation();
        int line = loc.getBeginLine();
        int col = loc.getBeginColumn();
        throw new ParserException("Expected 'loc' or 'uuid' at line " + line + " column " + col);
      }
      
      return value;
    }
    
    private Value parseLocation()
    {
      scanner.accept(Token.Type.COLON);
      scanner.accept(Token.Type.SLASH);
      scanner.accept(Token.Type.SLASH);
      Token pathToken = scanner.accept(Token.Type.IDENTIFIER);
      scanner.accept(Token.Type.LPAREN);
      scanner.accept(Token.Type.RPAREN);
      Token offsetToken = scanner.accept(Token.Type.INTEGER);
      scanner.accept(Token.Type.COMMA);
      Token lengthToken = scanner.accept(Token.Type.INTEGER);
      scanner.accept(Token.Type.LT);
      Token beginLineToken = scanner.accept(Token.Type.INTEGER);
      scanner.accept(Token.Type.COMMA);
      Token beginColumnToken = scanner.accept(Token.Type.INTEGER);
      scanner.accept(Token.Type.GT);
      scanner.accept(Token.Type.LT);
      Token endLineToken = scanner.accept(Token.Type.INTEGER);
      scanner.accept(Token.Type.COMMA);
      Token endColumnToken = scanner.accept(Token.Type.INTEGER);
      scanner.accept(Token.Type.GT);
      scanner.accept(Token.Type.PIPE);

      string path = pathToken.getString();
      int offset = parseInt(offsetToken);
      int length = parseInt(lengthToken);
      int beginLine = parseInt(beginLineToken);
      int beginColumn = parseInt(beginColumnToken);
      int endLine = parseInt(endLineToken);
      int endColumn = parseInt(endColumnToken);
      
      return new Location(path, offset, length, beginLine, beginColumn, endLine, endColumn);
    }

    private Value parseUuid()
    {
      scanner.accept(Token.Type.COLON);
      scanner.accept(Token.Type.SLASH);
      scanner.accept(Token.Type.SLASH);
      Token idToken = scanner.accept(Token.Type.INTEGER);
      scanner.accept(Token.Type.PIPE);
      
      int uuid = int.Parse(idToken.getString());
      return new UUID(uuid);
    }

    private Value checkEnum(Value value)
    {
      Value rval = value;
      string name = printer.print(value);
      int lastDot = name.LastIndexOf(".");

      if (lastDot != -1)
      {
        //Console.Out.WriteLine(name);
        string head = name.Substring(0, lastDot);
        string tail = name.Substring(lastDot + 1);
        //Console.Out.WriteLine(head);
        //Console.Out.WriteLine(tail);

        Type t = Type.GetType(head);
        if (t != null && t.IsEnum)
        {
          object ev = Enum.Parse(t, tail);
          rval = new EnumValue(ev);
        }
      }
      return rval;
    }
  }
}