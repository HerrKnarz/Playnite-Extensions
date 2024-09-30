namespace LinkUtilities.Settings
{
    /// <summary>
    /// Class to store some global settings in a singleton
    /// </summary>
    public class GlobalSettings
    {
        private static GlobalSettings _instance;

        private GlobalSettings(bool onlyATest = false) => OnlyATest = onlyATest;

        /// <summary>
        /// Used to omit certain SDK interactions to make unit tests easier.
        /// </summary>
        public bool OnlyATest { get; }

        public static GlobalSettings Instance(bool onlyATest = false) => _instance ?? (_instance = new GlobalSettings(onlyATest));
    }
}