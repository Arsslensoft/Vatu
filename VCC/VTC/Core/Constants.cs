using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vasm;
using Vasm.x86;

namespace VTC.Core
{
    public class ByteConstant : ConstantExpression
    {
        internal byte _value;
        public ByteConstant(byte value, Location loc)
            : base(BuiltinTypeSpec.Byte, loc)
        {
            _value = value;
        }
       

        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (ushort)_value, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec,RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)_value, Size = 16 });
            return true;
        }
        public override string CommentString()
        {
            return _value.ToString();
        }
    }
    public class StringConstant : ConstantExpression
    {
        // ==================================================================================
  /// <summary>
  /// This static class involves helper methods that use strings.
  /// </summary>
  // ==================================================================================
        public static class StringHelper
  {
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Converts a C# literal string into a normal string.
    /// </summary>
    /// <param name="source">Source C# literal string.</param>
    /// <returns>
    /// Normal string representation.
    /// </returns>
    // --------------------------------------------------------------------------------
    public static string StringFromCSharpLiteral(string source)
    {
      StringBuilder sb = new StringBuilder(source.Length);
      int pos = 0;
      while (pos < source.Length)
      {
        char c = source[pos];
        if (c == '\\')
        {
          // --- Handle escape sequences
          pos++;
          if (pos >= source.Length) throw new ArgumentException("Missing escape sequence");
          switch (source[pos])
          {
            // --- Simple character escapes
            case '\'': c = '\''; break;
            case '\"': c = '\"'; break;
            case '\\': c = '\\'; break;
            case '0': c = '\0'; break;
            case 'a': c = '\a'; break;
            case 'b': c = '\b'; break;
            case 'f': c = '\f'; break;
            case 'n': c = '\n'; break;
            case 'r': c = '\r'; break;
            case 't': c = '\t'; break;
            case 'v': c = '\v'; break;
            case 'x':
              // --- Hexa escape (1-4 digits)
              StringBuilder hexa = new StringBuilder(10);
              pos++;
              if (pos >= source.Length)
                throw new ArgumentException("Missing escape sequence");
              c = source[pos];
              if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
              {
                hexa.Append(c);
                pos++;
                if (pos < source.Length)
                {
                  c = source[pos];
                  if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                  {
                    hexa.Append(c);
                    pos++;
                    if (pos < source.Length)
                    {
                      c = source[pos];
                      if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') ||
                        (c >= 'A' && c <= 'F'))
                      {
                        hexa.Append(c);
                        pos++;
                        if (pos < source.Length)
                        {
                          c = source[pos];
                          if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') ||
                            (c >= 'A' && c <= 'F'))
                          {
                            hexa.Append(c);
                            pos++;
                          }
                        }
                      }
                    }
                  }
                }
              }
              c = (char)Int32.Parse(hexa.ToString(), NumberStyles.HexNumber);
              pos--;
              break;
            case 'u':
              // Unicode hexa escape (exactly 4 digits)
              pos++;
              if (pos + 3 >= source.Length)
                throw new ArgumentException("Unrecognized escape sequence");
              try
              {
                uint charValue = UInt32.Parse(source.Substring(pos, 4),
                  NumberStyles.HexNumber);
                c = (char)charValue;
                pos += 3;
              }
              catch (SystemException)
              {
                throw new ArgumentException("Unrecognized escape sequence");
              }
              break;
            case 'U':
              // Unicode hexa escape (exactly 8 digits, first four must be 0000)
              pos++;
              if (pos + 7 >= source.Length)
                throw new ArgumentException("Unrecognized escape sequence");
              try
              {
                uint charValue = UInt32.Parse(source.Substring(pos, 8),
                  NumberStyles.HexNumber);
                if (charValue > 0xffff)
                  throw new ArgumentException("Unrecognized escape sequence");
                c = (char)charValue;
                pos += 7;
              }
              catch (SystemException)
              {
                throw new ArgumentException("Unrecognized escape sequence");
              }
              break;
            default:
              throw new ArgumentException("Unrecognized escape sequence");
          }
        }
        pos++;
        sb.Append(c);
      }
      return sb.ToString();
    }
 
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Converts a C# verbatim literal string into a normal string.
    /// </summary>
    /// <param name="source">Source C# literal string.</param>
    /// <returns>
    /// Normal string representation.
    /// </returns>
    // --------------------------------------------------------------------------------
    public static string StringFromVerbatimLiteral(string source)
    {
      StringBuilder sb = new StringBuilder(source.Length);
      int pos = 0;
      while (pos < source.Length)
      {
        char c = source[pos];
        if (c == '\"')
        {
          // --- Handle escape sequences
          pos++;
          if (pos >= source.Length) throw new ArgumentException("Missing escape sequence");
          if (source[pos] == '\"') c = '\"';
          else throw new ArgumentException("Unrecognized escape sequence");
        }
        pos++;
        sb.Append(c);
      }
      return sb.ToString();
    }
 
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Converts a C# literal string into a normal character..
    /// </summary>
    /// <param name="source">Source C# literal string.</param>
    /// <returns>
    /// Normal char representation.
    /// </returns>
    // --------------------------------------------------------------------------------
    public static char CharFromCSharpLiteral(string source)
    {
      string result = StringFromCSharpLiteral(source);
      if (result.Length != 1)
        throw new ArgumentException("Invalid char literal");
      return result[0];
    }
  }


        public static int id = 0;
        public FieldSpec ConstVar { get; set; }
        string _value;
        public StringConstant(string value, Location loc)
            : base(BuiltinTypeSpec.String, loc)
        {
            _value = value;
            Normalize();
        }
         void Normalize()
        {

            _value = StringHelper.StringFromCSharpLiteral(_value); 
        }
        bool decl = false;
        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {
            if (ConstVar != null)
            {
                ec.EmitData(new DataMember(ConstVar.Signature.ToString(), _value,true), ConstVar, true);
                ec.EmitInstruction(new Push() { DestinationRef = ElementReference.New(ConstVar.Signature.ToString()), Size = 16 });

            }
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
    
                ConstVar = new FieldSpec(rc.CurrentNamespace, "STRC_" + id,  Modifiers.Const, Type, loc);
          
            id++;
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            if (!decl)
                Emit(ec);
      
     
            return true;
        }
     
        public override string CommentString()
        {
            return _value.ToString();
        }
    }
    public class SByteConstant : ConstantExpression
    {
        internal sbyte _value;
        public SByteConstant(sbyte value, Location loc)
            : base(BuiltinTypeSpec.SByte, loc)
        {
            _value = value;
        }

        public override string CommentString()
        {
            return _value.ToString();
        }
        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
      
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (ushort)_value, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)_value, Size = 16 });
            return true;
        }
    }
    public class IntConstant : ConstantExpression
    {
        internal short _value;
        public IntConstant(short value, Location loc)
            : base(BuiltinTypeSpec.Int, loc)
        {
            _value = value;
        }

        public override string CommentString()
        {
            return _value.ToString();
        }
        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }

        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (ushort)_value, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)_value, Size = 16 });
            return true;
        }
    }
    public class ArrayConstant : ConstantExpression
    {
        internal List<byte> _value;
        public ArrayConstant(byte[] b, Location loc)
            : base(BuiltinTypeSpec.Byte.MakeArray(), loc)
        {
            _value = new List<byte>();
            _value .AddRange(b);
        }

        public override string CommentString()
        {
            string vals = "";
            foreach (byte b in _value)
                vals += " " + b.ToString();
            return vals;
        }
        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + CommentString();
        }
        public override object GetValue()
        {
            return _value.ToArray();
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
       
            return true;
        }

      
    }
    public class UIntConstant : ConstantExpression
    {
        internal ushort _value;
        public UIntConstant(ushort value, Location loc)
            : base(BuiltinTypeSpec.UInt, loc)
        {
            _value = value;
        }

        public override string CommentString()
        {
            return _value.ToString();
        }
        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = (ushort)_value, Size = 16 });
            return true;
        }

        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue = (ushort)_value, Size = 16 });
            return true;
        }
    }
    public class BoolConstant : ConstantExpression
    {
        internal bool _value;
        public BoolConstant(bool value, Location loc)
            : base(BuiltinTypeSpec.Bool, loc)
        {
            _value = value;
        }
        public override string CommentString()
        {
            return _value.ToString();
        }

        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
       
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }

        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = _value ? (ushort)EmitContext.TRUE : (ushort)0, Size = 16 });
            return true;
        }

        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = ec.GetLow(rg), SourceValue = _value ? (ushort)EmitContext.TRUE : (ushort)0, Size = 16 });
            return true;
        }
    }
    public class NullConstant : ConstantExpression
    {
        int _value;
        public NullConstant(Location loc)
            : base(BuiltinTypeSpec.UInt, loc)
        {
            _value = 0;
        }

        public override string CommentString()
        {
            return _value.ToString();
        }
        public override string ToString()
        {
            return "[" + Type.GetTypeName(type) + "] " + GetValue();
        }
        public override object GetValue()
        {
            return _value;
        }
        public override ConstantExpression ConvertImplicitly(ResolveContext rc, TypeSpec type, ref bool cv)
        {
            cv = false;
            if (type.IsPointer)
            {
                cv = true;
                this.type = type;
          

            }
             return this;
        }
        public override bool Emit(EmitContext ec)
        {
            EmitToStack(ec);
            return true;
        }
        public override bool Resolve(ResolveContext rc)
        {
            return true;
        }
        public override SimpleToken DoResolve(ResolveContext rc)
        {
            return this;
        }
        public override bool EmitToStack(EmitContext ec)
        {
            ec.EmitInstruction(new Push() { DestinationValue = 0, Size = 16 });
            return true;
        }
        public override bool EmitToRegister(EmitContext ec, RegistersEnum rg)
        {
            ec.EmitInstruction(new Mov() { DestinationReg = rg, SourceValue =0, Size = 16 });
            return true;
        }
    }




}
