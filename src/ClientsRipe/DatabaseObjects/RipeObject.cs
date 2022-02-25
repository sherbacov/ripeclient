using System;
using System.Collections.Generic;
using System.Linq;

namespace RipeDatabaseObjects
{
    public interface IRipeObject
    {
        public string GetKey(bool allowSpaces);
        public string GetRipeObjectType();
        public string[] ObjectKeys { get; }
    }

    public class RipeObject : List<KeyValuePair<string, string>>, IRipeObject
    {
        public virtual string GetRipeObjectType()
        {
            throw new NotImplementedException();
        }

        public string[] ObjectKeys { get; set; }

        public virtual string GetKey(bool allowSpaces)
        {
            throw new NotImplementedException();
        }

        public virtual void Add(string key, string value)
        {
            Add(new KeyValuePair<string, string>(key, value));
        }

        public virtual void AddMnt(string mnt)
        {
            //check if exists
            var mntPair = this.Any(r => r.Key == "mnt-by" && r.Value == mnt);
            if (mntPair) return;
            
            Add("mnt-by", mnt);
        }

        public virtual void AddList(string key, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                Add(key, value);
            }
        }

        public DateTime GetDateValue(string valueName)
        {
            var parce = this[valueName];

            var date = DateTime.Parse(parce);

            return date.ToUniversalTime();
        }

        public DateTime Created()
        {
            return GetDateValue("created");
        }

        public DateTime Updated()
        {
            return GetDateValue("last-modified");
        }
        
        public bool ContainsKey(string key)
        {
            return this.Any(c => c.Key == key);
        }

        public int KeysCount(string key)
        {
            return this.Count(c => c.Key == key);
        }

        public string this[string index]
        {
            get
            {
                var obj = this.FirstOrDefault(f => f.Key == index);
                return obj.Value;
            }

            set
            {
                var obj = this.FirstOrDefault(f => f.Key == index);
                var indexOf = IndexOf(obj);
                Remove(obj);
                Insert(indexOf, new KeyValuePair<string, string>(index, value));
            }
        }
    }
}