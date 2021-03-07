using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RuneOptim;
using RuneOptim.swar;

namespace RuneService
{
    public class SQLTable<T>
    {
        SQLiteConnection conn = null;
        string name = null;
        public SQLTable(SQLiteConnection c, string n)
        {
            conn = c;
            name = n;
        }

        Dictionary<PropertyInfo, string> propMap = new Dictionary<PropertyInfo, string>();

        public void Create()
        {
            StringBuilder sb = new StringBuilder("CREATE TABLE @0 (");
            var qq = typeof(T);
            var props = qq.GetProperties();
            if (props.Length == 0)
                throw new ArgumentException();
            foreach (var p in props)
            {
                
            }
            sb.Append(")");
            SQLiteCommand com = new SQLiteCommand(sb.ToString());
            com.Parameters.AddWithValue("@0", name);
            com.ExecuteNonQuery();
        }

        public void Drop()
        {

        }

        public void Insert(T obj)
        {

        }

        public void Update(T obj, object selector)
        {

        }
    }

    public static class StatServer
    {
        static SQLiteConnection connection = null;

        static SQLTable<Rune> runeTable = null;

        public static void Init()
        {
            if (!File.Exists("stats.sqlite"))
                SQLiteConnection.CreateFile("stats.sqlite");

            connection = new SQLiteConnection("stats.sqlite");
            connection.Open();

            runeTable = new SQLTable<Rune>(connection, "runes");
            runeTable.Create();
        }

        public static void LogRune(Rune rune)
        {
            if (!hasTable("runeUpgrades"))
            {
                SQLiteCommand com = new SQLiteCommand("CREATE TABLE runes (rune_id BIGINT, wizard_id BIGINT, slot_no SMALLINT, );");

            }
            if (hasIdInTable("runes", "rune_id", rune.Id))
            {

            }
            else
            {
                SQLiteCommand com = new SQLiteCommand("INSERT INTO runes (rune_id, wizard_id, slot_no) values () ;");
                //com.Parameters.AddWithValue("@0", tname);

            }
        }

        private static bool hasTable(string tname)
        {
            SQLiteCommand com = new SQLiteCommand("SELECT count(*) FROM sqlite_master WHERE type='table' AND name=@0;");
            com.Parameters.AddWithValue("@0", tname);
            return ((int?)com.ExecuteScalar() ?? 0) != 0;
        }

        private static bool hasIdInTable(string tname, string col, ulong id)
        {
            SQLiteCommand com = new SQLiteCommand("SELECT count(*) FROM @0 WHERE @1=@2;");
            com.Parameters.AddWithValue("@0", tname);
            return ((int?)com.ExecuteScalar() ?? 0) != 0;
        }

        public static void Shutdown()
        {
            connection.Close();
        }
    }
}
