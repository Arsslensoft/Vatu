using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    public class MemberSpecEqualityComparer : IEqualityComparer<MemberSpec>
    {


       public bool Equals(MemberSpec x, MemberSpec y)
            {
                return x.Signature == y.Signature;
            }

       public int GetHashCode(MemberSpec x)
            {
                return x.Signature.Signature.GetHashCode();
            }

        
    }
}
