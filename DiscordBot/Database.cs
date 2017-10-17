using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Script.Serialization;
using System.IO;

namespace FlatFileDatabase {

    public class Table {
        public class Column {
            public string name;
        }

        public class Row {
            public Dictionary<string, string> values = new Dictionary<string, string>();
        }

        public Column[] columns;
        public List<Row> rows;

        public Table(params Column[] cols) {
            columns = cols;
        }
    }

    public class Database {
        public Dictionary<string, Table> tables = new Dictionary<string, Table>();

        private object lockAndKey = new object();
        private string file;

        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        private Database(string file) {
            this.file = file;
        }

        public static Database Create(string file) {
            Database db;
            if (File.Exists(file)) {
                string json = string.Join("\n", File.ReadAllLines(file));
                db = serializer.Deserialize<Database>(json);
            }
            else {
                db = new Database(file);
                db.Save();
            }
            return db;
        }

        public void Save() {
            string json = serializer.Serialize(this);
            File.WriteAllLines(file, new string[] { json });
        }

        public void Query(Query query) {
            lock (lockAndKey) {
                Table src = ObtainSrc(query);
            }
        }

        private Table ObtainSrc(Query query) {
            return null;
        }
    }
}
