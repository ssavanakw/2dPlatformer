using System;

namespace Game2D.Core
{
    [Serializable]
    public sealed class StatModifierData
    {
        public StatId stat;
        public StatModifierType type;
        public float value;
    }
}
