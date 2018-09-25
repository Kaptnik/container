using System.Linq;
using System.Reflection;

namespace Unity.Expressions
{
    public class MethodExpressions
    {
        public static readonly MethodInfo StringFormat =
            typeof(string).GetTypeInfo()
                .DeclaredMethods
                .First(m =>
                {
                    var parameters = m.GetParameters();
                    return m.Name == nameof(string.Format) &&
                           m.GetParameters().Length == 2 &&
                           typeof(object) == parameters[1].ParameterType;
                });
    }
}
