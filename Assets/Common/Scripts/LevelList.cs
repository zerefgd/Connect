using System.Collections.Generic;
using UnityEngine;

namespace Connect.Common
{
    [CreateAssetMenu(fileName = "Levels",menuName = "SO/AllLevels")]
    public class LevelList : ScriptableObject
    {
        public List<LevelData> Levels;
    }
}
