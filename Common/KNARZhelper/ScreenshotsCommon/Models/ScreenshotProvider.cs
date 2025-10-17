using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace KNARZhelper.ScreenshotsCommon.Models
{
    /// <summary>
    /// Class representing a screenshot provider with properties for ID and name. This usually is the add-on providing the screenshots.
    /// </summary>
    public class ScreenshotProvider : ObservableObject
    {
        private Guid _id = Guid.NewGuid();
        private string _name;

        /// <summary>
        /// Creates a new instance of the ScreenshotProvider class.
        /// </summary>
        /// <param name="name">Name of the provider</param>
        /// <param name="id">Unique identifier of the provider. Should be the same as the id of the provider add-on itself.</param>
        public ScreenshotProvider(string name = "", Guid id = default)
        {
            _id = id == default ? _id : id;
            _name = name;
        }

        /// <summary>
        /// Name of the provider
        /// </summary>
        [SerializationPropertyName("id")]
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        /// <summary>
        /// Unique identifier of the provider. Should be the same as the id of the provider add-on itself.
        /// </summary>
        [SerializationPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }
    }
}
