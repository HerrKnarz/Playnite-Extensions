using LinkUtilities.BaseClasses;
using LinkUtilities.Linker;
using System.Collections;
using System.Collections.Generic;

namespace LinkUtilities.Test
{
    internal class LinkTestCases : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[]
            {
                new LinkAdventureGamers(),
                "Day of the Tentacle",
                "https://adventuregamers.com/games/view/15427",
                "https://adventuregamers.com/games/view/15427"
            },
            // Todo: add ArcadeDatabase -> needs roms though!
            new object[]
            {
                 new LinkCoOptimus(),
                 "Minecraft",
                 "https://www.co-optimus.com/game/3006/Android/minecraft.html",
                 "https://www.co-optimus.com/game/3006/Android/minecraft.html"
            }
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
