using Unity.Container.Context;

namespace Unity.Aspect
{



    public delegate TResult AspectFactory<out TResult>(ref RegistrationContext context);
}
