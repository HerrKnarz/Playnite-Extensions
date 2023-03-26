namespace LinkUtilities.Settings
{
    public class GlobalSettings
    {
        private static GlobalSettings _instance = null;
        private static readonly object _mutex = new object();

        private GlobalSettings(bool onlyATest = false) => OnlyATest = onlyATest;

        public static GlobalSettings Instance(bool onlyATest = false)
        {
            if (_instance == null)
            {
                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = new GlobalSettings(onlyATest);
                    }
                }
            }

            return _instance;
        }

        public bool OnlyATest { get; }
    }
}
