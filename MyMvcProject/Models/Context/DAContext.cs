using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMvcProject.Models.Context;

namespace MyMvcProject.Models.Context
{
    public class DAContext : Dbase, IDisposable
    {
        public DAContext(string connName = "Conn") :
            base(connName)
        {
            //pass
        }

        void IDisposable.Dispose()
        {
            if (_trans != null && _trans.Connection != null && _trans.Connection.State != ConnectionState.Closed)
            {
                _trans.Rollback();
                _trans.Connection.Close();
            }

            if (_conn != null && _conn.State != ConnectionState.Closed)
            {
                _conn.Close();
            }
        }
    }
}
