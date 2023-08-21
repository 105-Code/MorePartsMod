using ModLoader;

namespace MorePartsMod.Utils
{
    public class MockMod : Mod
    {
        public string _modNameId;
        public override string ModNameID => _modNameId;

        public string _displayName;
        public override string DisplayName => _displayName;

        public string _author;
        public override string Author => _author;

        public override string MinimumGameVersionNecessary => "";
        public override string ModVersion => "";
        public override string Description => "";

        public MockMod(string id, string name, string author)
        {
            _modNameId = id;
            _displayName = name;
            _author = author;
        }
    }
}