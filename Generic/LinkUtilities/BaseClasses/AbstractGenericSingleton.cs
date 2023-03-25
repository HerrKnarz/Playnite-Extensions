using System;

namespace LinkUtilities.BaseClasses
{
    internal abstract class AbstractGenericSingleton<T> where T : AbstractGenericSingleton<T>
    {
        private static T _instance;

        protected static bool Initialised => _instance != null;

        protected static T UniqueInstance => Initialised ? SingletonCreator._instance : null;

        protected AbstractGenericSingleton() { }

        protected static void Init(T newInstance) => _instance = newInstance ?? throw new ArgumentNullException();

        private class SingletonCreator
        {
            static SingletonCreator() { }

            internal static readonly T _instance = AbstractGenericSingleton<T>._instance;
        }
    }
}
