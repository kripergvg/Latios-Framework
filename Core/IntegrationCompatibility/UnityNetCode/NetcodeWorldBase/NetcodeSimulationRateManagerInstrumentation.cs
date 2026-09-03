using System;
using Unity.Entities;
using Unity.NetCode;

namespace Latios.NetCodeCompatibility
{
    /// <summary>
    /// Receives the two decision edges of a NetCode simulation rate manager without replacing
    /// the concrete manager type that NetCode itself expects to find on the simulation group.
    /// </summary>
    public interface INetcodeSimulationRateManagerObserver
    {
        void BeforeShouldGroupUpdate(ComponentSystemGroup group);
        void AfterShouldGroupUpdate(ComponentSystemGroup group, bool shouldUpdate);
    }

    /// <summary>
    /// Creates an observed version of the stock NetCode simulation rate manager for the world's
    /// role. NetCode 6.6 queries the concrete server or host manager during initialization, so an
    /// ordinary <see cref="IRateManager"/> decorator is not compatible with a NetCode world.
    /// </summary>
    public static class NetcodeSimulationRateManagerInstrumentation
    {
        public static IRateManager Observe(
            ComponentSystemGroup group,
            INetcodeSimulationRateManagerObserver observer)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            if (group.RateManager is IObservedRateManager observed)
            {
                if (ReferenceEquals(observed.Observer, observer))
                    return group.RateManager;
                throw new InvalidOperationException("The NetCode simulation rate manager is already observed by another owner.");
            }

            if (group.World.IsHost())
            {
                if (group.RateManager is not NetcodeHostRateManager)
                    throw UnexpectedRateManager(group);
                return new ObservedHostRateManager(group, observer);
            }

            if (group.World.IsServer())
            {
                if (group.RateManager is not NetcodeServerRateManager)
                    throw UnexpectedRateManager(group);
                return new ObservedServerRateManager(group, observer);
            }

            if (group.World.IsClient() || group.World.IsThinClient())
            {
                if (group.RateManager is not NetcodeClientRateManager)
                    throw UnexpectedRateManager(group);
                return new ObservedClientRateManager(group, observer);
            }

            throw new InvalidOperationException($"World '{group.World.Name}' is not a NetCode simulation world.");
        }

        private static InvalidOperationException UnexpectedRateManager(ComponentSystemGroup group) =>
            new($"World '{group.World.Name}' has an unexpected NetCode simulation rate manager '{group.RateManager?.GetType().FullName ?? "<null>"}'.");

        private interface IObservedRateManager
        {
            INetcodeSimulationRateManagerObserver Observer { get; }
        }

        private sealed class ObservedServerRateManager : NetcodeServerRateManager, IRateManager, IObservedRateManager
        {
            public ObservedServerRateManager(
                ComponentSystemGroup group,
                INetcodeSimulationRateManagerObserver observer) : base(group)
            {
                Observer = observer;
            }

            public INetcodeSimulationRateManagerObserver Observer { get; }

            bool IRateManager.ShouldGroupUpdate(ComponentSystemGroup group)
            {
                Observer.BeforeShouldGroupUpdate(group);
                var shouldUpdate = base.ShouldGroupUpdate(group);
                Observer.AfterShouldGroupUpdate(group, shouldUpdate);
                return shouldUpdate;
            }
        }

        private sealed class ObservedHostRateManager : NetcodeHostRateManager, IRateManager, IObservedRateManager
        {
            public ObservedHostRateManager(
                ComponentSystemGroup group,
                INetcodeSimulationRateManagerObserver observer) : base(group)
            {
                Observer = observer;
            }

            public INetcodeSimulationRateManagerObserver Observer { get; }

            bool IRateManager.ShouldGroupUpdate(ComponentSystemGroup group)
            {
                Observer.BeforeShouldGroupUpdate(group);
                var shouldUpdate = base.ShouldGroupUpdate(group);
                Observer.AfterShouldGroupUpdate(group, shouldUpdate);
                return shouldUpdate;
            }
        }

        private sealed class ObservedClientRateManager : NetcodeClientRateManager, IRateManager, IObservedRateManager
        {
            public ObservedClientRateManager(
                ComponentSystemGroup group,
                INetcodeSimulationRateManagerObserver observer) : base(group)
            {
                Observer = observer;
            }

            public INetcodeSimulationRateManagerObserver Observer { get; }

            bool IRateManager.ShouldGroupUpdate(ComponentSystemGroup group)
            {
                Observer.BeforeShouldGroupUpdate(group);
                var shouldUpdate = base.ShouldGroupUpdate(group);
                Observer.AfterShouldGroupUpdate(group, shouldUpdate);
                return shouldUpdate;
            }
        }
    }
}
