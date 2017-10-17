using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatFileDatabase {
    public class Query {
        private static QueryCompiler compiler = new QueryCompiler();
        protected Query() { }
        public Query Compile(string query, params string[] parameters) {
            return compiler.Parse(compiler.Tokenize(query));
        }
        protected virtual void Operate(Database db) { }
    }

    public class CreateTableQuery: Query{
        public string name;
        public string[] columns; 
        protected override void Operate(Database db) {
            
        }
    }

    public class DropTableQuery : Query {
        public string name;
        protected override void Operate(Database db) {

        }
    }

    public class DeleteFromQuery : Query {
        public string table;
        public Expression eval;
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
