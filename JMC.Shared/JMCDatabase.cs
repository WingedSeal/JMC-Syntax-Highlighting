using System.Data.SQLite;
using System.Reflection;

namespace JMC.Shared
{
    internal class JMCDatabase
    {
        public readonly SQLiteConnection DatabaseConnection;
        public string Version { get; set; } = "1_20_1";
        public JMCDatabase()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "jmc.db");
            using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("JMC.Shared.Resource.jmc.db") ??
                throw new NotImplementedException();
            using var fs = File.OpenWrite(path);
            CopyStream(rs, fs);
            DatabaseConnection = new("Data Source=" + path);
        }

        public string GetMinecraftFileString(string path)
        {
            var command = DatabaseConnection.CreateCommand();
            command.CommandText = @$"SELECT text FROM v{Version} WHERE path = $path";
            command.Parameters.AddWithValue("$path", path);
            using var reader = command.ExecuteReader();
            if (reader.Read())
                return reader.GetString(0);
            else
                return string.Empty;
        }

        public string GetBuiltinFunctionString()
        {
            var command = DatabaseConnection.CreateCommand();
            command.CommandText = @"SELECT text FROM jmc WHERE path = $path";
            command.Parameters.AddWithValue("$path", "BuiltInFunctions");
            using var reader = command.ExecuteReader();
            if (reader.Read())
                return reader.GetString(0);
            else
                return string.Empty;
        }

        private static void CopyStream(Stream inputStream, Stream outputStream)
        {
            CopyStream(inputStream, outputStream, 4096);
        }

        private static void CopyStream(Stream inputStream, Stream outputStream, int bufferLength)
        {
            var buffer = new byte[bufferLength];
            int bytesRead;
            while ((bytesRead = inputStream.Read(buffer, 0, bufferLength)) > 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}
