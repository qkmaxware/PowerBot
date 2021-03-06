﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace FlatFileDatabase {

    /*
        WS := [ \t\n\r] -> skip;
        NAME := [a-zA-Z][a-zA-Z0-9]*;
        INTEGER := [0-9]+;
        FLOAT := [0-9]+ '.' [0-9]+;
        BOOLEAN := 'TRUE' | 'FALSE';
        STRING := '"' .*? '"';
        PARAMETER := '$' [0-9]+;
 
        program := query*;
        query := (create | drop | select | insert | update | delete) ';'?;
        create := 'CREATE TABLE' NAME '(' (NAME NAME (',' NAME NAME)*)? ')';
        drop := 'DROP TABLE' NAME;
        select := 'SELECT' ('*' | (NAME (',' NAME)*)) 'FROM' table (WHERE expression)?;
               table := (table 'JOIN' table 'ON' expression) | NAME;
        insert := 'INSERT INTO' NAME ('(' NAME (',' NAME)* ')') 'VALUES' expression (',' expression)*;
        update := 'UPDATE' NAME 'SET' (NAME '=' expression) (',' NAME '=' expression)* ('WHERE' expression)?
        delete := 'DELETE FROM' NAME ('WHERE' expression)?
 
        expression := (expression ( 'AND' | 'OR' ) expression) | ('NOT' expression) | (term);
        term := value | (value ('>' | <' | '=') value);
        value := literal | ( '(' exp ')' );
        literal := INTEGER | FLOAT | BOOLEAN | STRING | NAME;
    */

    public class QueryCompiler {

        public class RQ {
            private Match[] matches;
            private int index = 0;
            public RQ(ICollection<Match> matches) {
                this.matches = matches.ToArray<Match>();
            }

            public int Step() {
                return index;
            }
            public void Restore(int i) {
                index = i;
            }
            public int Count() {
                return matches.Length - index;
            }
            public bool IsNext(Token tok) {
                return (index < matches.Length && matches[index].token == tok);
            }
            public Match Pop() {
                index++;
                return matches[index - 1];
            }
            public bool IsEmpty() {
                return index == matches.Length;
            }
            public Match Peek() {
                return matches[index];
            }
        }

        public class Token {
            private Regex regex;
            public Token(string exp, RegexOptions opts = RegexOptions.None) {
                regex = new Regex("^\\s*(" + exp + ")", opts);
            }

            public bool Matches(string input) {
                return regex.IsMatch(input);
            }

            public Match Extract(string input, out string output) {
                Match m = new Match();
                m.token = this;
                m.value = regex.Match(input).Value.Trim();
                output = regex.Replace(input, string.Empty).Trim();
                return m;
            }

            public override string ToString() {
                return regex.ToString();
            }
        }

        public class Match {
            public Token token;
            public string value;

            public override string ToString() {
                return "[" + token.ToString() + "] - " + value;
            }
        }

        public class ParseException : Exception {
            public ParseException(string msg) : base(msg) { }
        }

        private Dictionary<string, Token> tokens = new Dictionary<string, Token>(){
            {"create", new Token("CREATE(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"table", new Token("TABLE(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"drop", new Token("DROP(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"select", new Token("SELECT(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"from", new Token("FROM(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"join", new Token("JOIN(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"where", new Token("WHERE(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"insert", new Token("INSERT(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"into", new Token("INTO(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"values", new Token("VALUES(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"update", new Token("UPDATE(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"set", new Token("SET(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"delete", new Token("DELETE(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"and", new Token("AND(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"or", new Token("OR(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"not", new Token("NOT(?:\\s|$)", RegexOptions.IgnoreCase)},
            {"open", new Token("\\(")},
            {"close", new Token("\\)")},
            {"comma", new Token("\\,")},
            {"compare", new Token("(?:[><=])")},             //Comparison
            {"name", new Token("[a-zA-Z][a-zA-Z0-9]*")},  //Name
            {"int", new Token("[0-9]+")},                //Integer
            {"float", new Token("[0-9]+\\.[0-9]+")},       //Float
            {"bool", new Token("(?:TRUE)|(?:FALSE)")},    //Boolean
            {"string", new Token("'.*?'")},           //String
            {"param", new Token("\\$[0-9]+")},             //Parameter,
            {"*", new Token("\\*")}                    //Anything
        };

        public List<Match> Tokenize(string input) {
            List<Match> matches = new List<Match>();
            input = input.Trim();

            while (input != string.Empty) {
                bool foundMatch = false;
                foreach (Token token in tokens.Values) {
                    if (token.Matches(input)) {
                        foundMatch = true;
                        matches.Add(token.Extract(input, out input));
                        break;
                    }
                }

                if (!foundMatch) {
                    throw new ParseException("Invalid input starting at: " + input);
                }
            }

            return matches;
        }

        public Query Parse(List<Match> tokens) {
            Query q = null;
            RQ queue = new RQ(tokens);
            int ind = queue.Step();

            q = ParseCreate(queue);
            if (q != null)
                return q;
            queue.Restore(ind);

            q = ParseDrop(queue);
            if (q != null)
                return q;
            queue.Restore(ind);

            q = ParseSelect(queue);
            if (q != null)
                return q;
            queue.Restore(ind);

            q = ParseInsert(queue);
            if (q != null)
                return q;
            queue.Restore(ind);

            q = ParseUpdate(queue);
            if (q != null)
                return q;
            queue.Restore(ind);

            q = ParseDelete(queue);
            if (q != null)
                return q;
            queue.Restore(ind);
            
            throw new ParseException("Input string is not one of: create | drop | select | insert | update | delete");
        }

        //'CREATE TABLE' NAME '(' (NAME NAME (',' NAME NAME)*)? ')';
        private Query ParseCreate(RQ q) {
            if (!q.IsNext(tokens["create"]))
                return null;
            q.Pop();
            
            if (!q.IsNext(tokens["table"]))
                return null;
            q.Pop();

            if (!q.IsNext(tokens["name"]))
                return null;
            string table = q.Pop().value;

            if (!q.IsNext(tokens["open"]))
                return null;
            q.Pop();
            
            List<string> elems = new List<string>();

            //Column list
            while (q.IsNext(tokens["name"])) {
                string name = q.Pop().value;

                elems.Add(name);

                if (!q.IsNext(tokens["comma"]))
                    break;
                q.Pop();
            }

            if (!q.IsNext(tokens["close"]))
                return null;
            q.Pop();

            CreateTableQuery ct = new CreateTableQuery();
            ct.name = table;
            ct.columns = elems.ToArray();
            return ct;
        }

        //'DROP TABLE' NAME;
        private Query ParseDrop(RQ q) {
            if (!q.IsNext(tokens["drop"]))
                return null;
            q.Pop();

            if (!q.IsNext(tokens["table"]))
                return null;
            q.Pop();

            if (!q.IsNext(tokens["name"]))
                return null;
            string table = q.Pop().value;

            DropTableQuery dt = new DropTableQuery();
            dt.name = table;
            return dt;
        }

        //NAME | NAME JOIN SOURCE
        private TableSource ParseSource(RQ q) {
            if (!q.IsNext(tokens["name"])) {
                return null;
            }
            string left = q.Pop().value;

            if (!q.IsNext(tokens["join"])) {
                RawSource r = new RawSource();
                r.table = left;
                return r;
            }
            q.Pop();

            TableSource right = ParseSource(q);

            JoinSource j = new JoinSource();
            j.left = left;
            j.right = right;
            return j;
        }

        //'SELECT' ('*' | (NAME (',' NAME)*)) 'FROM' table (WHERE expression)?;
        private Query ParseSelect(RQ q) {
            if (!q.IsNext(tokens["select"]))
                return null;
            q.Pop();

            SelectFromQuery sf = new SelectFromQuery();

            if (q.IsNext(tokens["*"])) {
                sf.all = true;
                q.Pop();
            }
            else {
                List<string> names = new List<string>();
                while (true) {
                    if (!q.IsNext(tokens["name"])) {
                        break;
                    }
                    string name = q.Pop().value;

                    names.Add(name);

                    if (!q.IsNext(tokens["comma"])) {
                        break;
                    }
                    q.Pop();
                }
                sf.fields = names.ToArray();
            }

            if (!q.IsNext(tokens["from"]))
                return null;
            q.Pop();

            TableSource source = ParseSource(q);

            if (!q.IsNext(tokens["where"]))
                return null;
            q.Pop();

            Expression e = ParseExpression(q);
            if (e == null)
                e = new TrueExpression();

            sf.source = source;
            sf.expr = e;
            return sf;
        }

        //'INSERT INTO' NAME ('(' NAME (',' NAME)* ')') 'VALUES' expression (',' expression)*;
        private Query ParseInsert(RQ q) {
            if (!q.IsNext(tokens["insert"]))
                return null;
            q.Pop();

            if (!q.IsNext(tokens["into"]))
                return null;
            q.Pop();

            TableSource source = ParseSource(q);

            if (!q.IsNext(tokens["open"]))
                return null;
            q.Pop();

            List<string> names = new List<string>();
            while (q.IsNext(tokens["name"])) {
                string name = q.Pop().value;

                names.Add(name);

                if (!q.IsNext(tokens["comma"]))
                    break;
                q.Pop();
            }

            if (!q.IsNext(tokens["close"]))
                return null;
            q.Pop();

            if (!q.IsNext(tokens["values"]))
                return null;
            q.Pop();

            List<Expression> ex = new List<Expression>();
            while (true) {
                Expression e = ParseExpression(q);
                if (e == null)
                    break;

                ex.Add(e);

                if (!q.IsNext(tokens["comma"]))
                    break;
                q.Pop();
            }

            InsertIntoQuery ii = new InsertIntoQuery();
            ii.exp = ex.ToArray();
            ii.fields = names.ToArray();
            ii.source = source;
            return ii;
        }

        //'UPDATE' NAME 'SET' (NAME '=' expression) (',' NAME '=' expression)* ('WHERE' expression)?
        private Query ParseUpdate(RQ q) {
            if (!q.IsNext(tokens["update"]))
                return null;
            q.Pop();

            TableSource s = ParseSource(q);

            if (!q.IsNext(tokens["set"]))
                return null;
            q.Pop();

            List<string> fields = new List<string>();
            List<Expression> values = new List<Expression>();
            while (true) {
                if (!q.IsNext(tokens["name"]))
                    break;
                string field = q.Pop().value;
                if (!q.IsNext(tokens["compare"]))
                    break;
                string op = q.Peek().value;
                if (op != "=")
                    break;
                q.Pop();

                Expression e2 = ParseExpression(q);

                fields.Add(field);
                values.Add(e2);

                if (!q.IsNext(tokens["comma"]))
                    break;
            }

            if (!q.IsNext(tokens["where"]))
                return null;
            q.Pop();

            Expression e = ParseExpression(q);
            if (e == null)
                e = new TrueExpression();

            UpdateQuery u = new UpdateQuery();
            u.table = s;
            u.fields = fields.ToArray();
            u.values = values.ToArray();
            u.exp = e;
            return u;
        }

        //'DELETE FROM' NAME ('WHERE' expression)?
        private Query ParseDelete(RQ q) {
            if (!q.IsNext(tokens["delete"]))
                return null;
            q.Pop();

            if (!q.IsNext(tokens["from"]))
                return null;
            q.Pop();

            TableSource source = ParseSource(q);

            DeleteFromQuery df = new DeleteFromQuery();
            df.table = source;
            df.eval = new TrueExpression();

            if (q.IsNext(tokens["where"])) {
                q.Pop();
                Expression e = ParseExpression(q);
                if (e == null) {
                    e = new TrueExpression();
                }
                else
                    df.eval = e;
            }

            return df;
        }

        //(expression ( 'AND' | 'OR' ) expression) | ('NOT' expression) | (term);
        private Expression ParseExpression(RQ q) {
            Expression left = null;
            if (q.IsNext(tokens["not"])) {
                q.Pop();
                NotExpression n = new NotExpression();
                n.exp = ParseExpression(q);
                if (n.exp == null)
                    return null;
                left = n;
            }
            else {
                left = ParseTerm(q);
            }

            BooleanExpression b = new BooleanExpression();
            if (q.IsNext(tokens["and"]) || q.IsNext(tokens["or"])) {
                b.or = q.Pop().value.ToLower() == "or";

                Expression right = null;
                if (q.IsNext(tokens["not"])) {
                    q.Pop();
                    NotExpression n = new NotExpression();
                    n.exp = ParseExpression(q);
                    if (n.exp == null)
                        return null;
                    right = n;
                }
                else {
                    right = ParseTerm(q);
                }

                if (right == null || left == null)
                    return null;
                b.left = left;
                b.right = right;
                return b;
            }
            else {
                return left;
            }

            return null;
        }

        //value | (value ('>' | <' | '=') value);
        private Expression ParseTerm(RQ q) {
            Expression left = ParseValue(q);

            if (!q.IsNext(tokens["compare"])) {
                return left;
            }
            string comp = q.Pop().value;

            Expression right = ParseValue(q);

            if (left == null || right == null)
                return null;

            CompareExpression c = new CompareExpression();
            c.left = left;
            c.right = right;
            c.op = comp;
            return c;
        }

        //literal | ( '(' exp ')' );
        private Expression ParseValue(RQ q) {
            if (q.IsNext(tokens["open"])) {
                q.Pop();
                Expression exp = ParseExpression(q);
                if (exp == null)
                    throw new ParseException("No expression in brackets");
                if (q.IsNext(tokens["close"])) {
                    q.Pop();
                    return exp;
                }
                return null;
            }

            return ParseLiteral(q);
        }

        //INTEGER | FLOAT | BOOLEAN | STRING | NAME | PARAM;
        private Expression ParseLiteral(RQ q) {
            if (q.IsNext(tokens["name"])) {
                RowValueExpression rv = new RowValueExpression();
                rv.row = q.Pop().value;
                return rv;
            }

            if (q.IsNext(tokens["int"]) || q.IsNext(tokens["float"]) || q.IsNext(tokens["bool"])) {
                ValueExpression v = new ValueExpression();
                v.value = q.Pop().value;
                return v;
            }

            if (q.IsNext(tokens["param"])) {
                ValueExpression v = new ValueExpression();
                v.value = q.Peek().value;
                v.parameterId = int.Parse(v.value.Remove(0, 1));
                return v;
            }

            if (q.IsNext(tokens["string"])) {
                ValueExpression v = new ValueExpression();
                v.value = q.Peek().value.Remove(0,1);
                v.value = v.value.Remove(v.value.Length - 1);
                q.Pop();
                return v;
            }

            return null;
        }
    }
}
