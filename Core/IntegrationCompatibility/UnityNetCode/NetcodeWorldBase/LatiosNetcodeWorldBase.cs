using Unity.Collections;
using Unity.Entities;

namespace Latios.NetCodeCompatibility
{
    /// <summary>
    /// Exposes NetCode 6.6's world initialization to LatiosWorld without copying or replacing
    /// NetCode runtime state. This source is compiled into Unity.NetCode through the adjacent
    /// assembly reference, so it can invoke NetcodeWorld's package-internal constructors.
    /// </summary>
    public class LatiosNetcodeWorldBase : Unity.NetCode.NetcodeWorld
    {
        protected LatiosNetcodeWorldBase(string name, WorldFlags flags = WorldFlags.Simulation) : base(name, flags)
        {
        }

        protected LatiosNetcodeWorldBase(
            string name,
            WorldFlags flags,
            AllocatorManager.AllocatorHandle backingAllocatorHandle) : base(name, flags, backingAllocatorHandle)
        {
        }
    }
}
