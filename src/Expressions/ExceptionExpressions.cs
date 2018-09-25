using System;
using System.Linq;
using System.Reflection;

namespace Unity.Expressions
{
    public class ExceptionExpressions
    {
        public static readonly ConstructorInfo InvalidOperationExceptionCtor =
            typeof(InvalidOperationException)
                .GetTypeInfo()
                .DeclaredConstructors
                .First(c =>
                {
                    var parameters = c.GetParameters();
                    return 2 == parameters.Length &&
                           typeof(string) == parameters[0].ParameterType &&
                           typeof(Exception) == parameters[1].ParameterType;
                });
    }
}
