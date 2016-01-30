using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VTC.Core;

namespace VTC
{
    public struct FlowState
    {
        public static FlowState Valid = new FlowState(true, new Reachability());
        public static FlowState Unreachable = new FlowState(true,Reachability.CreateUnreachable());

        internal Reachability _reach;
        bool _success;
        public Reachability Reachable { get { return _reach; } set { _reach = value; } }
        public bool Success { get { return _success; } set { _success = value; } }

        public FlowState(bool succ, Reachability rc)
        {
            _reach = rc;
            _success = succ;
        }

        public static FlowState operator &(FlowState a, FlowState b)
        {
            return new FlowState(a._success & b._success, a._reach | b._reach);
        }
        public static FlowState operator |(FlowState a, FlowState b)
        {
            return new FlowState(a._success | b._success, a._reach | b._reach);
        }
    }
    public struct Reachability
    {
        readonly bool unreachable;
        public Location Loc;
        Reachability(bool unreachable)
        {
            Loc = Location.Null;
            this.unreachable = unreachable;
        }

        public bool IsUnreachable
        {
            get
            {
                return unreachable;
            }
        }

        public static Reachability CreateUnreachable()
        {
            return new Reachability(true);
        }

        public static Reachability operator &(Reachability a, Reachability b)
        {
            return new Reachability(a.unreachable && b.unreachable);
        }

        public static Reachability operator |(Reachability a, Reachability b)
        {
            return new Reachability(a.unreachable | b.unreachable);
        }
    }
    [Flags]
    public enum MemberState : byte
    {
        NONE = 0,
        USED = 1,
        ASSIGNED = 1 << 1
    }
   public class SignatureEntry
    {
       public MemberSpec Spec;
       public MemberState State;
       public SignatureEntry(MemberSpec ms, MemberState s)
       {
           Spec = ms;
           State = s;
       }
    }
    public class SignatureBitSet
    {
        Dictionary<string, SignatureEntry> _vals;

        public SignatureBitSet()
        {
            _vals = new Dictionary<string, SignatureEntry>();
        }

        public void MarkUsed(MemberSpec ms)
        {
            lock(_vals)
            {
                  if (_vals.ContainsKey(ms.Signature.Signature))
                    _vals[ms.Signature.Signature].State |= MemberState.USED;
                else if (ms is MethodSpec)
                    _vals.Add(ms.Signature.Signature,new SignatureEntry(ms, MemberState.USED));
            }
        }
        public bool IsAssigned(MemberSpec ms)
        {
            lock (_vals)
            {
                if (!_vals.ContainsKey(ms.Signature.Signature))
                    return true;
                else return (_vals[ms.Signature.Signature].State & MemberState.ASSIGNED )== MemberState.ASSIGNED;
            }
        }
      
        public void MarkUnused(MemberSpec ms)
        {
            lock (_vals)
            {
                if (!_vals.ContainsKey(ms.Signature.Signature))
                        _vals.Add(ms.Signature.Signature,new SignatureEntry(ms, MemberState.NONE));
            }
        }
        public void MarkAssigned(MemberSpec ms)
        {
            lock (_vals)
            {
                if (_vals.ContainsKey(ms.Signature.Signature))
                    _vals[ms.Signature.Signature].State |= MemberState.ASSIGNED;
            }
        }
        public List<MemberSpec> GetUnUsed()
        {
            List<MemberSpec> ms = new List<MemberSpec>();
            foreach (KeyValuePair<string, SignatureEntry> p in _vals)
                      if (p.Value.State == MemberState.NONE)
                          ms.Add(p.Value.Spec);


                  return ms;
        }
        public SignatureEntry this[MemberSpec ms]
        {
            get
            {
              return  _vals[ms.Signature.Signature];
            }
            set
            {
                _vals[ms.Signature.Signature] = value;
            }
        }
    }
   
    public class CodePath
    {
        public Location PathLocation { get; set; }
        public bool Returns { get; set; }
        public List<CodePath> CodePaths { get; set; }

        public CodePath(Location loc)
        {
            Returns = false;
            CodePaths = new List<CodePath>();
            PathLocation = loc;
        }

        public void AddPath(CodePath cp)
        {
            CodePaths.Add(cp);
        }
       
        public bool CheckReturn()
        {
        
            if (CodePaths.Count == 0 || Returns) // end of path or returns
                return Returns;

            bool ret = true;
            foreach (CodePath cp in CodePaths)
                ret &= cp.CheckReturn();
           
            return ret;
        }
    }

 

    public class FlowAnalysisContext
    {
        public bool NoReturnCheck=false;
        public CodePath CodePathReturn { get; set; }

        public static SignatureBitSet  ProgramFlowState { get; set; }

        public void MarkAsUsed(MemberSpec msig)
        {
            ProgramFlowState.MarkUsed(msig);
        }
        public void MarkAsAssigned(MemberSpec msig)
        {
            ProgramFlowState.MarkAssigned(msig);
        }
        public void AddNew(MemberSpec msig)
        {
            ProgramFlowState.MarkUnused(msig);
        }
        public bool IsAssigned(MemberSpec msig)
        {
            return ProgramFlowState.IsAssigned(msig);
        }

        public bool UnreachableReported { get; set; }
        public Declaration Decl { get; set; }
    
        public FlowAnalysisContext(Declaration decl)
        {
            CodePathReturn = new CodePath(decl.loc);
            Decl = decl;
         
        }
     
        public bool FindNoReturn()
        {
         return   CodePathReturn.CheckReturn();
        }

        public FlowState DoFlowAnalysis(ResolveContext rc, bool noretcheck = false)
        {
     
            
          FlowState fl =  Decl.DoFlowAnalysis(this);
          if (!NoReturnCheck && !noretcheck && !FindNoReturn())
              ResolveContext.Report.Warning(CodePathReturn.PathLocation, "Not all code paths returns a value ");
          Location loc = CodePathReturn.PathLocation;
          //if (FindUnreachableCode(ref loc))
          //    ResolveContext.Report.Warning(loc, "Unreachable code detected");


      
                
          return fl;
        }

        public void ReportUnreachable(Location loc)
        {
            ResolveContext.Report.Warning( loc, "Unreachable code detected");
        }
        public void ReportUseOfUnassigned(Location loc,string name)
        {
            ResolveContext.Report.Warning(loc, "Use of an unassigned variable " + name);
        }
        public void ReportUnusedVariableDeclaration(Location loc,string name)
        {
            ResolveContext.Report.Warning(loc, "Unused variable "+name);
        }
        public void ReportNotAllCodePathsReturns(Location loc)
        {
            ResolveContext.Report.Warning(loc,"Not all code paths returns a value");
        }
        public void ReportUnusedTypeDeclaration(Location loc, string name)
        {
            ResolveContext.Report.Warning(loc, "Unused Type " + name);
        }
    }
}
