using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatFileDatabase {
    public class Query {
        private static QueryCompiler compiler = new QueryCompiler();
        protected Query() { }
        public static Query Compile(string query, params string[] parameters) {
            return compiler.Parse(compiler.Tokenize(query));
        }
        public virtual Table Operate(Database db) { return null; }
    }

    public class CreateTableQuery: Query{
        public string name;
        public string[] columns;
        public override Table Operate(Database db) {
            Table.Column[] col = new Table.Column[columns.Length];
            for (int i = 0; i < columns.Length; i++) {
                col[i] = new Table.Column();
                col[i].name = columns[i];
            } 
            Table t = new Table(col);

            db.AddTable(name, t);
            return t;
        }
    }

    public class DropTableQuery : Query {
        public string name;
        public override Table Operate(Database db) {
            db.RemoveTable(name);
            return null;
        }
    }

    public class DeleteFromQuery : Query {
        public TableSource table;
        public Expression eval;
        public override Table Operate(Database db) {
            Table source = table.Value(db);
            List<Table.Row> toRemove = new List<Table.Row>();
            foreach (Table.Row row in source.rows) {
                bool eval = this.eval.Evaluate(row) == "TRUE";
                if (eval)
                    toRemove.Add(row);
            }
            source.rows = source.rows.Except(toRemove).ToList();
            return source;
        }
    }

    public class SelectFromQuery : Query {
        public bool all;
        public string[] fields;
        public TableSource source;
        public Expression expr;
        public override Table Operate(Database db) {
            Table src = source.Value(db);

            Table.Column[] col;
            if (!all) {
                col = new Table.Column[fields.Length];
                for (int i = 0; i < fields.Length; i++) {
                    col[i] = new Table.Column();
                    col[i].name = fields[i];
                }
            }
            else {
                col = new Table.Column[src.columns.Length];
                for (int i = 0; i < src.columns.Length; i++) {
                    col[i] = new Table.Column();
                    col[i].name = src.columns[i].name;
                }
            }
            Table result = new Table(col);

            foreach (Table.Row row in src.rows) {
                bool eval = this.expr.Evaluate(row) == "TRUE";
                if(eval)
                    result.rows.Add(row);
            }

            return result;
        }
    }

    public class InsertIntoQuery : Query {
        public string[] fields;
        public TableSource source;
        public Expression[] exp;
        public override Table Operate(Database db) {
            Table src = source.Value(db);
            Table.Row row = new Table.Row();
            for (int i = 0; i < fields.Length; i++) {
                row.values[fields[i]] = (exp[i].Evaluate(row));
            }
            src.rows.Add(row);

            return src;
        }
    }

    public class UpdateQuery : Query {
        public string[] fields;
        public TableSource table;
        public Expression[] values;
        public Expression exp = null;
        public override Table Operate(Database db) {
            Table src = table.Value(db);

            foreach (Table.Row row in src.rows) {
                bool eval = this.exp.Evaluate(row) == "TRUE";
                if (!eval)
                    continue;

                for (int i = 0; i < fields.Length; i++) {
                    row.values[fields[i]] = values[i].Evaluate(row);
                }
            }

            return src;
        }
    }

    //----------------------------------------------------------------------------------
    // Source evaluation
    //----------------------------------------------------------------------------------

    public class TableSource {
        public virtual Table Value(Database db) {
            return null;
        }
    }

    public class RawSource : TableSource {
        public string table;
        public override Table Value(Database db) {
            return db.tables[table];
        }
    }

    public class JoinSource : TableSource {
        public string left;
        public TableSource right;
        public override Table Value(Database db) {
            Table l = db.tables[left];
            Table r = right.Value(db);

            List<Table.Column> cols = new List<Table.Column>();
            for (int i = 0; i < l.columns.Length; i++) {
                Table.Column c = new Table.Column();
                c.name = "left." + l.columns[i].name;
                cols.Add(c);
            }
            for (int i = 0; i < r.columns.Length; i++) {
                Table.Column c = new Table.Column();
                c.name = "right." + r.columns[i].name;
                cols.Add(c);
            }

            Table j = new Table(cols.ToArray());
            foreach(Table.Row row in l.rows) {
                foreach (Table.Row row2 in r.rows) {
                    Table.Row jr = new Table.Row();
                    foreach (KeyValuePair<string, string> pair in row.values) {
                        jr.values["left." + pair.Key] = pair.Value;
                    }
                    foreach (KeyValuePair<string, string> pair in row2.values) {
                        jr.values["right." + pair.Key] = pair.Value;
                    }
                    j.rows.Add(jr);
                }
            }
            return j;
        }
    }

    //----------------------------------------------------------------------------------
    // Expression evaluation
    //----------------------------------------------------------------------------------

    public class Expression {
        public virtual string Evaluate(Table.Row row) {
            return "FALSE";
        }
    }

    public class TrueExpression : Expression {
        public override string Evaluate(Table.Row row) {
            return "TRUE";
        }
    }

    public class BooleanExpression : Expression {
        public Expression left;
        public Expression right;
        public bool or = false;
        public override string Evaluate(Table.Row row) {
            bool l = bool.Parse(left.Evaluate(row));
            bool r = bool.Parse(right.Evaluate(row));

            bool q;
            if (or)
                q = l || r;
            else
                q = l && r;

            return q ? "TRUE" : "FALSE";
        }
    }

    public class ValueExpression : Expression {
        public string value;
        public int parameterId = -1;
        public override string Evaluate(Table.Row row) {
            return value;
        }
    }

    public class RowValueExpression : Expression {
        public string row;
        public override string Evaluate(Table.Row row) {
            return row.values[this.row];
        }
    }

    public class NotExpression : Expression {
        public Expression exp;
        public override string Evaluate(Table.Row row) {
            string value = exp.Evaluate(row);
            bool b = bool.Parse(value);
            return !b ? "TRUE" : "FALSE";
        }
    }

    public class CompareExpression : Expression {
        public string op;
        public Expression left;
        public Expression right;
        public override string Evaluate(Table.Row row) {
            switch (op) {
                case "=":
                    return (left.Evaluate(row) == right.Evaluate(row)) ? "TRUE" : "FALSE";
                case ">":
                    return double.Parse(left.Evaluate(row)) > double.Parse(right.Evaluate(row)) ? "TRUE" : "FALSE";
                case "<":
                    return double.Parse(left.Evaluate(row)) < double.Parse(right.Evaluate(row)) ? "TRUE" : "FALSE";
            }
            return base.Evaluate(row);
        }
    }
}
