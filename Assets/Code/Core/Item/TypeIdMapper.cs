using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Core;

namespace Item
{
    public class TypeIdMapper
    {
        private readonly Dictionary<string, int> _typeIdMap;
        private int _nextTypeId;

        public TypeIdMapper()
        {
            _typeIdMap = new Dictionary<string, int>();
            _nextTypeId = 1;
            LoadTypeIdMap();
        }

        public int GetOrAssignId(string typeName)
        {
            int outId;

            if (_typeIdMap.TryGetValue(typeName, out int existingId))
            {
                outId = existingId;
            }
            else
            {
                int newId = GetNextTypeId();
                _typeIdMap[typeName] = newId;
                outId = newId;
            }

            return outId;
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(_typeIdMap, Formatting.Indented);
            File.WriteAllText(CoreConfig.MappingPath, json);
        }

        private void LoadTypeIdMap()
        {
            string path = CoreConfig.MappingPath;

            if (!File.Exists(path))
            {
                return;
            }

            string json = File.ReadAllText(path);
            Dictionary<string, int> loaded = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

            if (loaded == null)
            {
                return;
            }

            _typeIdMap.Clear();
            int maxId = 0;

            foreach (KeyValuePair<string, int> pair in loaded)
            {
                _typeIdMap[pair.Key] = pair.Value;
                if (pair.Value > maxId)
                {
                    maxId = pair.Value;
                }
            }

            _nextTypeId = maxId + 1;
        }

        private int GetNextTypeId()
        {
            return _nextTypeId++;
        }
    }
}
