using Unity.Build.Delegates;
using Unity.Builder;

namespace Unity.Strategies.Build
{
    public class ActivatedBuildStrategy
    {
        public static FactoryDelegate<IBuilderContext, ResolverDelegate> FactoryDelegate = c =>
        {

            return null;
        };
    }
}
