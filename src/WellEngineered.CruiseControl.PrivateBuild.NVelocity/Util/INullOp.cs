using System;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Util
{
    interface INullOp<T>
    {
        bool HasValue(T value);
        bool AddIfNotNull(ref T accumulator, T value);
    }
}
