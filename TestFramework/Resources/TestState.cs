using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Common.Logic;

namespace TestFramework.Resources
{
    public class TestState
    {
        public List<object> _stateObjects;

        public string SessionId { get; set; }

        public TestState()
        {
            _stateObjects = new List<object>();
            SessionId = Guid.NewGuid().ToString();
        }

        public void Clear()
        {
            _stateObjects = new List<object>();
        }

        public void SaveInstance<TInstance>(TInstance t, bool overwriteExisting = true)
        {
            if (overwriteExisting)
            {
                var existing = GetInstance<TInstance>();
                if (existing != null)
                {
                    _stateObjects.Remove(existing);
                }
            }

            _stateObjects.Add(t);
        }

        public void SaveInstanceWithKey<TInstance>(string key, TInstance t, bool overwriteExisting = false)
        {
            if (overwriteExisting)
            {
                var existing = GetTupleWithKey(key);
                if (existing != null)
                {
                    _stateObjects.Remove(existing.Item2);
                    _stateObjects.Remove(existing);
                }
            }
            _stateObjects.Add(t);
            _stateObjects.Add(new Tuple<string, object>(key, t));
        }

        public async Task SaveInstanceWithKeyAsync<TInstance>(string key, TInstance t, bool overwriteExisting = false)
        {
            await Task.Run(() =>
            {
                if (overwriteExisting)
                {
                    var existing = GetTupleWithKey(key);
                    if (existing != null)
                    {
                        _stateObjects.Remove(existing.Item2);
                        _stateObjects.Remove(existing);
                    }
                }
                _stateObjects.Add(t);
                _stateObjects.Add(new Tuple<string, object>(key, t));
            });
        }

        public object[] GetArrayOfType(string typeName)
        {
            return GetInstanceOfType(typeName) as object[];
        }

        public object GetInstanceOfType(string typeName)
        {
            foreach (var obj in _stateObjects)
            {
                try
                {
                    var thisObjectType = obj.GetType().Name;

                    if (thisObjectType.ToLower() == typeName.ToLower())
                        return obj;
                }
                catch (Exception) { }

            }

            return null;
        }
        public object GetInstanceWithKey(string keyName)
        {
            foreach (var obj in _stateObjects)
            {
                try
                {
                    var tuple = obj as Tuple<string, object>;
                    if (tuple == null) continue;

                    if (tuple.Item1 == keyName)
                        return tuple.Item2;
                }
                catch (Exception) { }

            }
            return null;
        }

        public async Task<object> GetInstanceWithKeyAsync(string keyName)
        {
            return await Task.Run(() => (
            from tuple
            in _stateObjects.OfType<Tuple<string, object>>()
            where tuple.Item1 == keyName
            select tuple.Item2).FirstOrDefault());
        }


        public List<object> GetInstances(string typeName)
        {
            var list = new List<object>();
            foreach (var obj in _stateObjects)
            {
                try
                {
                    var thisObjectType = obj.GetType().Name;

                    if (thisObjectType.ToLower() == typeName.ToLower())
                        list.Add(obj);
                }
                catch (Exception) { }

            }

            return list;
        }

        public List<TInstance> GetInstances<TInstance>()
        {
            return _stateObjects.OfType<TInstance>().Select(o => (TInstance)Convert.ChangeType(o, typeof(TInstance))).ToList();
        }

        public TInstance GetInstanceWithKey<TInstance>(string keyName)
        {
            foreach (var obj in _stateObjects)
            {
                try
                {
                    var tuple = obj as Tuple<string, object>;
                    if (tuple == null) continue;

                    if (tuple.Item1 == keyName)
                        return (TInstance)Convert.ChangeType(tuple.Item2, typeof(TInstance));
                }
                catch (Exception err)
                {
                }

            }

            return default(TInstance);
        }

        public async Task<TInstance> GetInstanceWithKeyAsync<TInstance>(string keyName)
        {
            return await Task.Run(() =>
            {
                foreach (var obj in _stateObjects)
                {
                    var tuple = obj as Tuple<string, object>;
                    if (tuple == null) continue;

                    if (tuple.Item1 == keyName)
                        return (TInstance)Convert.ChangeType(tuple.Item2, typeof(TInstance));
                }
                return default(TInstance);
            });
        }

        private Tuple<string, object> GetTupleWithKey(string keyName)
        {
            foreach (var obj in _stateObjects)
            {
                try
                {
                    var tuple = obj as Tuple<string, object>;
                    if (tuple == null) continue;

                    if (tuple.Item1 == keyName)
                        return tuple;
                }
                catch (Exception) { }

            }

            return null;
        }

        public TInstance GetInstance<TInstance>()
        {
            foreach (var obj in _stateObjects.OfType<TInstance>())
            {
                return (TInstance)Convert.ChangeType(obj, typeof(TInstance));
            }

            return default(TInstance);
        }

        public bool HasInstance<TInstance>()
        {
            return _stateObjects.OfType<TInstance>().Any();
        }

        public bool HasKey(string keyName)
        {
            foreach (var obj in _stateObjects)
            {
                try
                {
                    var tuple = obj as Tuple<string, object>;
                    if (tuple == null) continue;

                    if (tuple.Item1 == keyName)
                        return true;
                }
                catch (Exception err)
                {
                }
            }
            return false;
        }

        public string GetStateObjectsAsJson(bool prettyfyString = true)
        {
            var copy = new List<object>();
            foreach (var obj in _stateObjects)
            {
                try
                {
                    JsonConvert.SerializeObject(obj);
                    copy.Add(obj);
                }
                catch (Exception) { }
            }

            var json = JsonConvert.SerializeObject(copy);

            return prettyfyString ? json.TryMakeitPretty() : json;
        }


        public bool Contains(string key)
        {
            return null != GetInstanceWithKey(key);
        }
    }
}
