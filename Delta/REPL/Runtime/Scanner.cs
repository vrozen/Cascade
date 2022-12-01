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

namespace Delta.REPL.Runtime
{
  public class Scanner
  {
    private string path;
    private string input;
    private Token curToken;
    private Token nextToken;
    
    private Location curLoc;
    private int curLocBeginLine;
    private int curLocBeginCol;
    
    private int curPos;
    private int startPos;
    private int endPos;
    private int curCol;
    private int curLine;
    
    private bool acceptWhiteSpace;

    public Scanner()
    {
    }

    public void setInput(string program, string programPath)
    {
      
      this.input = program;
      this.path = programPath;
      this.curToken = null;
      this.nextToken = null;
      this.curLoc = null;
      
      this.curLocBeginLine = 1;
      this.curLocBeginCol = 0;


      this.curPos = 0;
      this.startPos = 0;
      this.endPos = 0;
      this.curCol = 0;
      this.curLine = 1;

      acceptWhiteSpace = true;
    }

    public void setAcceptWhiteSpace(bool acceptWhiteSpace)
    {
      this.acceptWhiteSpace = acceptWhiteSpace;
    }

    public bool hasToken()
    {
      return curPos < input.Length;
    }
    
    private Token scanToken()
    {
      if (acceptWhiteSpace)
      {
        scanWhiteSpace();
      }

      char c = next();
      Token token = null;
      bool tryIdentifier = false;
      
      switch (c)
      {
        case '{':
          token = scanSingleCharTokenAs(Token.Type.LCURLY);
          break;
        case '}':
          token = scanSingleCharTokenAs(Token.Type.RCURLY);
          break;
        case '[':
          token = scanSingleCharTokenAs(Token.Type.LBRACKET);
          break;
        case ']':
          token = scanSingleCharTokenAs(Token.Type.RBRACKET);
          break;
        case '(':
          token = scanSingleCharTokenAs(Token.Type.LPAREN);
          break;
        case ')':
          token = scanSingleCharTokenAs(Token.Type.RPAREN);
          break;
        case ':':
          token = scanSingleCharTokenAs(Token.Type.COLON);
          break;
        case ';':
          token = scanSingleCharTokenAs(Token.Type.SEMICOLON);
          break;
        case ',':
          token = scanSingleCharTokenAs(Token.Type.COMMA);
          break;
        case '.':
          token = scanSingleCharTokenAs(Token.Type.DOT);
          break;
        case '=':
          token = scanSingleCharTokenAs((Token.Type.EQUALS));
          break;
        case '|':
          token = scanSingleCharTokenAs((Token.Type.PIPE));
          break;
        case '/':
          token = scanSingleCharTokenAs((Token.Type.SLASH));
          break;
        case '<':
          token = scanSingleCharTokenAs((Token.Type.LT));
          break;
        case '>':
          token = scanSingleCharTokenAs((Token.Type.GT));
          break;
        case '"':
          token = scanString();
          break;
        case '-':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
          token = scanInt();
          break;
        case '\0':
          startToken();
          endToken();
          token = new Token("", Token.Type.EOF, curLoc);
          break;
        default:
          tryIdentifier = true;
          break;
      }

      if (tryIdentifier)
      {
        if (c >= 'a' && c < 'z' || c >= 'A' && c <= 'Z')
        {
          token = scanIdentifier(); //or true, false, null
        }
        else
        {
          throw new ParserException("Unexpected character '" + c + "' on line "+ curLine + " column " + curCol);
        }
      }

      return token;
    }

    private void scanWhiteSpace()
    {
      bool inWhiteSpace = true;
      while (inWhiteSpace)
      {
        char c = next();

        switch (c)
        {
          case ' ':
            acceptChar();
            break;
          case '\n':
            acceptChar();
            curLine++;
            curCol = 0;
            break;
          case '\r':
            acceptChar();
            curCol = 0;
            break;
          case '\t':
            acceptChar();
            break;
          default:
            inWhiteSpace = false;
            break;
        }
      }
    }
    
    private Token scanSingleCharTokenAs(Token.Type type)
    {
      startToken();
      char c = acceptChar();
      endToken();
      return new Token("" + c, type, curLoc);
    }

    private Token scanString()
    {
      startToken();
      acceptChar('"');
      char c;
      do
      {
        c = acceptChar();
        //todo: set overflow protection
      } while (c != '"');

      endToken();

      int offset = startPos + 1; //+1 removes opening quote
      int length = endPos - startPos - 2; //-2 removes ending quote

      string s = input.Substring(offset, length);

      return new Token(s, Token.Type.STRING, curLoc);
    }

    private Token scanInt()
    {
      startToken();

      char c = next();

      if (c == '-')
      {
        acceptChar();
      }
      //todo: do not accept only '-' as a valid int

      bool inInt = true;
      do
      {
        c = next();
        switch (c)
        {
          case '0':
          case '1':
          case '2':
          case '3':
          case '4':
          case '5':
          case '6':
          case '7':
          case '8':
          case '9':
            acceptChar();
            break;
          default:
            inInt = false;
            break;
        }
      } while (inInt);

      endToken();

      int offset = startPos;
      int length = endPos - startPos;

      string s = input.Substring(offset, length);

      return new Token(s, Token.Type.INTEGER, curLoc);
    }

    private Token scanIdentifier()
    {
      startToken();

      char c = next();
      
      if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z')
      {
        acceptChar();
      }
      else
      {
        startToken();
        endToken();
        return new Token("", Token.Type.ERROR, curLoc);
      }

      while (hasToken())
      {
        c = next();
        if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9')
        {
          acceptChar();
        }
        else
        {
          break;
        }
      }
      
      endToken();

      int offset = startPos;
      int length = endPos - startPos;

      string str = input.Substring(offset, length);
      
      Token.Type type = Token.Type.IDENTIFIER;
      
      if (str.Equals("null"))
      {
        type = Token.Type.NULL;
      }
      else if (str.Equals("true"))
      {
        type = Token.Type.TRUE;
      }
      else if (str.Equals("false"))
      {
        type = Token.Type.FALSE;
      }
      
      return new Token(str, type, curLoc);
    }

    private void startToken()
    {
      startPos = curPos;
      curLocBeginLine = curLine;
      curLocBeginCol = curCol;
    }

    private void endToken()
    {
      endPos = curPos;
      int length = endPos - startPos;
      curLoc = new Location(path, startPos, length, curLocBeginLine, curLocBeginCol, curLine, curCol);
    }

    private void acceptChar(char e)
    {
      char c = acceptChar();

      if (e != c)
      {
        throw new ParserException("Expected char '" + e + "', found char '" + c +
                                  "' on line " + curLine + " column " + curCol);
      }
    }

    private char acceptChar()
    {
      char c = next();
      curPos++;
      curCol++;
      return c;
    }

    private char next()
    {
      char c = '\0';
      if (curPos < input.Length)
      {
        c = input[curPos];
      }
      return c;
    }
    
    //parser API follows below

    public Token accept()
    {
      if (nextToken == null)
      {
        peek();
      }

      curToken = nextToken;
      nextToken = null;
      return curToken;
    }

    public Token accept(Token.Type expectedType)
    {
      if (nextToken == null)
      {
        peek();
      }

      curToken = nextToken;
      nextToken = null;

      Token.Type foundType = curToken.getTokenType();

      if (foundType != expectedType)
      {
        throw new ParserException("Expected " + expectedType + " found " + foundType +
                                  " on line " + curLine + " column " + curCol);
      }

      return curToken;
    }

    public Token peek()
    {
      if (nextToken == null)
      {
        nextToken = scanToken();
      }

      return nextToken;
    }
  }
}