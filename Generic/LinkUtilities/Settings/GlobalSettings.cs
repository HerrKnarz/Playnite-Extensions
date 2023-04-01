namespace LinkUtilities.Settings
{
    /// <summary>
    /// Class to store some global settings in a singleton
    /// </summary>
    public class GlobalSettings
    {
        private static GlobalSettings _instance;
        private static readonly object _mutex = new object();

        private GlobalSettings(bool onlyATest = false) => OnlyATest = onlyATest;

        public static GlobalSettings Instance(bool onlyATest = false)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new GlobalSettings(onlyATest);
                }
            }

            return _instance;
        }

        /// <summary>
        /// Used to omit certain SDK interactions to make unit tests easier.
        /// </summary>
        public bool OnlyATest { get; }
    }
}