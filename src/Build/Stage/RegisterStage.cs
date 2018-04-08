
namespace Unity.Build.Stage
{
    /// <summary>
    /// The stages used in registration pipeline.
    /// </summary>
    public enum RegisterStage
    {
        /// <summary>
        /// By default, nothing happens here.
        /// </summary>
        Setup,

        /// <summary>
        /// Stage where Array or IEnumerable is resolved
        /// </summary>
        Collections,

        /// <summary>
        /// Lifetime managers are checked here,
        /// and if they're available the rest of the pipeline is skipped.
        /// </summary>
        Lifetime,

        /// <summary>
        /// Instance creation with injected factory happens here.
        /// </summary>
        Injection,

        /// <summary>
        /// Type mapping occurs here.
        /// </summary>
        TypeMapping,

        /// <summary>
        /// Instance creation and dependency resolution happens here.
        /// </summary>
        Creation
    }
}
