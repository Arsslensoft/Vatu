using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using VJay;

namespace VCC
{
    public class Known
    {
        public List<TypeSpec> KnownTypes { get; set; }
        public List<FieldSpec> KnownGlobals { get; set; }
        public List<MethodSpec> KnownMethods {get;set;}

        public Known()
        {
            KnownGlobals = new List<FieldSpec>();
            KnownMethods = new List<MethodSpec>();
            KnownTypes = new List<TypeSpec>();
        }
    }
    public enum LabelType
    {
        LOOP,
        IF,
        ELSE,
        WHILE,
        DO,
        DO_WHILE,
        CASE,
        LABEL

    }

    /// <summary>
    /// Basic Emit for CodeGen
    /// </summary>
    public interface IEmitExpr
    {

        /// <summary>
        /// Emit code
        /// </summary>
        /// <returns>Success or fail</returns>
        bool EmitFromStack(EmitContext ec);
        /// <summary>
        /// Emit code to stack [push]
        /// </summary>
        /// <returns>Success or fail</returns>
        bool EmitToStack(EmitContext ec);
    }


    /// <summary>
    /// Basic Emit for CodeGen
    /// </summary>
    public interface IEmit
    {
        /// <summary>
        /// Emit code
        /// </summary>
        /// <returns>Success or fail</returns>
        bool Emit(EmitContext ec);

    }

    public interface IResolve
    {
        bool Resolve(ResolveContext rc);
    }

    /// <summary>
    /// Emit Context
    /// </summary>
   public class EmitContext
    {
       Vasm.AsmContext ag;
       List<string> _names;
       Label enterLoop, exitLoop;
      
       public static Dictionary<LabelType, int> Labels = new Dictionary<LabelType, int>();
       EmitContext()
       {
           _names = new List<string>();
       }

       string GenerateLabelName(LabelType lb)
       {
           if (Labels.ContainsKey(lb))
           {
               Labels[lb]++;
               return lb.ToString() + "_" + Labels[lb].ToString();
           }
           else
           {
               Labels.Add(lb, 0);
               return lb.ToString() + "_" + Labels[lb].ToString();
           }
       }

       public EmitContext(Vasm.AssemblyWriter asmw)
       {
           ag = new Vasm.AsmContext(asmw);
       }

       public EmitContext(Vasm.AsmContext ac)
       {
           ag = ac;
       }

       public void EmitInstruction(Instruction ins)
       {
           ag.Emit(ins);
       }
       
       public void EmitData(DataMember dm)
       {
           ag.DefineData(dm);
       }
       public void EmitData(string name,byte[] data)
       {
           ag.DefineData(DefineData(name, data));
       }
       public DataMember DefineData(string name, byte[] data)
       {
           return ag.Declare(name, data);
       }

       public void MarkLabel(Label lb)
       {
           ag.MarkLabel(lb);
       }
       public Label DefineLabel(string name)
       {
           return ag.DefineLabel(name);
       }
       public Label DefineLabel()
       {
           return ag.DefineLabel(GenerateLabelName(LabelType.LABEL));
       }
       public Label DefineLabel(LabelType lbt)
       {
           return ag.DefineLabel(GenerateLabelName(lbt));
       }
       public void MarkLoopEnter()
       {
           enterLoop = DefineLabel(LabelType.LOOP);
           MarkLabel(enterLoop);
      
       }
       public void MarkLoopExit()
       {
           exitLoop = DefineLabel("E_" + enterLoop.Name);
           MarkLabel(exitLoop);
       }

    }

   public class ResolveContext : IDisposable
   {
       Block current_block;
       MemberSpec current_member;
       
       Known _known;
       void FillKnown()
       {
           _known.KnownTypes.Add(BuiltinTypeSpec.Bool);
           _known.KnownTypes.Add(BuiltinTypeSpec.Char);
           _known.KnownTypes.Add(BuiltinTypeSpec.Byte);
           _known.KnownTypes.Add(BuiltinTypeSpec.Short);
           _known.KnownTypes.Add(BuiltinTypeSpec.UShort);
           _known.KnownTypes.Add(BuiltinTypeSpec.Int);
           _known.KnownTypes.Add(BuiltinTypeSpec.UInt);
           _known.KnownTypes.Add(BuiltinTypeSpec.Long);
           _known.KnownTypes.Add(BuiltinTypeSpec.ULong);
           _known.KnownTypes.Add(BuiltinTypeSpec.Float);
           _known.KnownTypes.Add(BuiltinTypeSpec.Double);
          
       }
       public ResolveContext(Block b, MemberSpec cm, Known known)
       {
           _known = known;
           current_block = b;
           current_member = cm;

       }
       public ResolveContext(Block b, MemberSpec cm)
      
       {
           _known = new Known();
           current_block = b;
           current_member = cm;
           FillKnown();
       }
      


       public TypeSpec ResolveType(string name)
       {
           foreach (TypeSpec kt in _known.KnownTypes)
               if (kt.Name == name)
                   return kt;

           return null;
       }
       public FieldSpec ResolveField(string name)
       {
           foreach (FieldSpec kt in _known.KnownGlobals)
               if (kt.Name == name)
                   return kt;

           return null;
       }
       public MethodSpec ResolveMethod(string name)
       {
           foreach (MethodSpec kt in _known.KnownMethods)
               if (kt.Name == name)
                   return kt;

           return null;
       }

       #region IDisposable Members

       public void Dispose()
       {

       }

       #endregion

   }

   public class CompilerContext
   {
       public static Location TranslateLocation(bsn.GoldParser.Parser.LineInfo li)
       {
           return new Location(new SourceFile("this", "null", 0), li.Line, li.Column);
       }
   }
}
