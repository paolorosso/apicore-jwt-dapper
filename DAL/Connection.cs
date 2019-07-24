using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DAL
{
    public interface IConnection
    {
        System.Data.IDbConnection Db { get; set; }
    }


    public class Connection : IConnection, IDisposable
    {
        public System.Data.IDbConnection Db { get; set; }


        public Connection()
        {
            // Apro la connessione
            Db = new SqlConnection(LOAD.ConnDb);
        }



        public void Dispose()
        {
            if (Db == null)
                return;

            Db.Close();
            Db.Dispose();
        }
    }
}
