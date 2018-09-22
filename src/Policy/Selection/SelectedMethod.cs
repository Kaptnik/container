using System.Reflection;
using Unity.Build.Context;

namespace Unity.Policy.Selection
{
    /// <summary>
    /// Objects of this type are the return value from <see cref="SelectedMethod"/>.
    /// It encapsulates the desired <see cref="MethodInfo"/> with the string keys
    /// needed to look up the <see cref="IResolverPolicy"/> for each
    /// parameter.
    /// </summary>
    public class SelectedMethod<TContext> : SelectedMemberWithParameters<TContext, MethodInfo>
        where TContext : IBuildContext
    {
        /// <summary>
        /// Create a new <see cref="SelectedMethod&lt;TContext&gt;"/> instance which
        /// contains the given method.
        /// </summary>
        /// <param name="method">The method</param>
        public SelectedMethod(MethodInfo method)
            : base(method)
        {
        }

        /// <summary>
        /// The constructor this object wraps.
        /// </summary>
        public MethodInfo Method => MemberInfo;
    }
}
