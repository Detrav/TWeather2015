using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TWeather2015.Core
{
    class DIconPositionManager
    {
        public SerializableDictionary<string,DIconPosition> items;
        static private DIconPositionManager instance = new DIconPositionManager();
        static private string fileName = "positions.dat";
        private DIconPositionManager()
        { }

        bool loaded = false;
        bool needToSave = false;

        static public void loadPositions()
        {
            if (!instance.loaded)
                instance.load();
        }
        /// <summary>
        /// Не возвращает нулей!
        /// </summary>
        /// <param name="name"></param>
        /// <returns>(0,0) если нету нужного места</returns>
        static public DIconPosition getPosition(string name)
        {
            if (instance.loaded == false) loadPositions();
            if (instance.items == null) return new DIconPosition();
            if (instance.items.ContainsKey(name))
                return new DIconPosition(instance.items[name].x, instance.items[name].y);
            return new DIconPosition();
        }

        static public void savePositions()
        {
            instance.save();
        }

        static public DIconPosition setPosition(string name,int x,int y)
        {
            return instance.set(name, x, y);
        }

        private void load()
        {
            if(File.Exists(fileName))
            {
                try {
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, DIconPosition>));
                    using (TextReader r = new StreamReader(fileName))
                        items = serializer.Deserialize(r) as SerializableDictionary<string, DIconPosition>;
                }
                catch { items = new SerializableDictionary<string, DIconPosition>(); }
            }
            else
            {
                items = new SerializableDictionary<string, DIconPosition>();
            }
            loaded = true;
        }

        private void save()
        {
            if (!needToSave) return;
            if (items == null) return;

            XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, DIconPosition>));
            using (TextWriter w = new StreamWriter(fileName))
                serializer.Serialize(w, items);
            needToSave = false;
        }

        private DIconPosition set(string name, int x, int y)
        {
            if (items == null) items = new SerializableDictionary<string, DIconPosition>();
            if (!items.ContainsKey(name))
            {
                needToSave = true;
                items[name] = new DIconPosition(x, y);
            }
            else
            {
                if (items[name].x != x || items[name].y != y)
                {
                    needToSave = true;
                    items[name] = new DIconPosition(x, y);
                }
            }
            //Console.WriteLine("x:{0}-y:{1}", x, y);
            return items[name];
        }

        internal static void deletePosition(string fullPath)
        {
            if (instance.items == null) return;
            if (instance.items.ContainsKey(fullPath))
                instance.items.Remove(fullPath);
            instance.needToSave = true;
        }

        internal static void renamePosition(string oldFullPath, string fullPath)
        {
            if (instance.items == null) return;
            if (instance.items.ContainsKey(oldFullPath))
            {
                var p = instance.items[oldFullPath];
                instance.items.Remove(oldFullPath);
                instance.items[fullPath] = p;
                instance.needToSave = true;
            }
        }
    }

    public class DIconPosition
    {
        public int x { get; set; }
        public int y { get; set; }

        public DIconPosition() : this(0, 0) { }

        public DIconPosition(int x,int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
