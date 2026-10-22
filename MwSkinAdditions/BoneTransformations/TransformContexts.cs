using System;

namespace MwSkinAdditions {
    [Flags]
    public enum TransformContexts : uint {
        CSS = 1u,
        Run = 2u,
        All = uint.MaxValue,
    }
}
