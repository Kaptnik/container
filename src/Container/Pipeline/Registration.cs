using Unity.Container.Context;

namespace Unity.Container.Pipeline
{
    public delegate TResult Registration<out TResult>(ref RegistrationContext context);
}
