using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Script.Serialization;
using System.IO;
using Newtonsoft.Json;

namespace FlatFileDatabase {

    public class Table {
        public class Column {
            public string name;
        }

        public class Row {
            public Dictionary<string, string> values = new Dictionary<string, string>();
        }

        public Column[] columns;
        public List<Row> rows = new List<Row>();

        public Table(params Column[] cols) {
            columns = cols;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class Database {
        public Dictionary<string, Table> tables = new Dictionary<string, Table>();

        private object lockAndKey = new object();
        private string file;

        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        private Database() { }

        private Database(string file) {
            this.file = file;
        }

        public static Database Create(string file) {
            Database db = null;
            if (File.Exists(file)) {
                string json = string.Join("\n", File.ReadAllLines(file));
                db = JsonConvert.DeserializeObject<Database>(json);
                db.file = file;
            }
            else {
                db = new Database(file);
                db.Save();
            }
            return db;
        }

        public void Save() {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllLines(file, new string[] { json });
        }

        public Table Query(Query query) {
            Table t = null;
            lock (lockAndKey) {
                t = query.Operate(this);
                this.Save();
            }
            return t;
        }

        public void AddTable(string name, Table t) {
            tables[name] = t;
        }

        public void RemoveTable(string name) {
            tables.Remove(name);
        }
    }
}
