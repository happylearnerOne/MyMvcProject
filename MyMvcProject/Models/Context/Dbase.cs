using Npgsql;
using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace MyMvcProject.Models.Context
{
    public class Dbase
    {
        private const int CONNECTION_POOL_EXHAUSTED_CODE = -2147467259;

        protected NpgsqlConnection _conn = null;
        protected NpgsqlTransaction _trans = null;

        public int RETRY_TIME { get; set; }

        public Dbase(string connName = "Conn")
        {
            string connString = ConfigurationManager.ConnectionStrings[connName].ConnectionString;
            _conn = new NpgsqlConnection(connString);
        }

        public Dbase(NpgsqlConnection conn)
        {
            this._conn = conn;
        }

        /// <summary>
        /// 將 Parameter 的 null 換成 DBNull
        /// </summary>
        /// <param name="cmd"></param>
        protected void _ReplaceNullToDBNull(ref NpgsqlCommand cmd)
        {
            foreach (NpgsqlParameter i in cmd.Parameters)
            {
                if (null == i.Value)
                    i.Value = DBNull.Value;
            }
        }

        protected void _OpenConnection()
        {
            _OpenConnection(ref _conn);
        }

        protected void _OpenConnection(ref NpgsqlConnection conn)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
            }
            catch (NpgsqlException e)
            {
                if (e.ErrorCode.Equals(CONNECTION_POOL_EXHAUSTED_CODE))
                {
                    NpgsqlConnection.ClearAllPools();
                }
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected void _BindConnection(ref NpgsqlCommand cmd)
        {
            if (this._trans == null)
            {
                _OpenConnection();
                cmd.Connection = _conn;
            }
            else
            {
                cmd.Transaction = _trans;
                cmd.Connection = _trans.Connection;
            }
        }

        /// <summary>
        /// Transaction 開始
        /// </summary>
        /// <returns>交易物件</returns>
        public void TransactionBegin()
        {
            try
            {
                _OpenConnection();
                _trans = _conn.BeginTransaction();
            }
            catch (Exception ex)
            {
                if (null != _conn && _conn.State != ConnectionState.Closed)
                    _conn.Close();
                throw ex;
            }
        }

        /// <summary>
        /// Transaction 結束
        /// </summary>
        /// <param name="trans">交易物件</param>
        /// <param name="isCommit"></param>
        public void TransactionEnd(bool isCommit = true)
        {
            if (_trans == null)
                return;

            try
            {
                if (isCommit)
                {
                    _trans.Commit();
                }
                else
                {
                    _trans.Rollback();
                }
            }
            catch (Exception ex)
            {
                _trans.Rollback();
                throw ex;
            }
            finally
            {
                _trans.Dispose();
                _trans = null;
            }
        }

        /// <summary>
        /// 讀取 DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable QueryDataTable(string sql)
        {
            sql = sql.Replace('`', '"');
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql))
            {
                return QueryDataTable(cmd);
            }

        }

        /// <summary>
        /// 讀取 DataTable
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public DataTable QueryDataTable(NpgsqlCommand cmd)
        {
            try
            {
                _BindConnection(ref cmd);
                _ReplaceNullToDBNull(ref cmd);

                DataTable dt = new DataTable();
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);

                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_trans == null && _conn != null && _conn.State != ConnectionState.Closed)
                    _conn.Close();
            }
        }

        /// <summary>
        /// 讀取整數
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int QueryScale(string sql)
        {
            sql = sql.Replace('`', '"');
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql))
            {
                return QueryScale(cmd);
            }
        }

        /// <summary>
        /// 讀取整數
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int QueryScale(NpgsqlCommand cmd)
        {
            try
            {
                _BindConnection(ref cmd);
                _ReplaceNullToDBNull(ref cmd);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null == _trans && null != _conn && _conn.State != ConnectionState.Closed)
                    _conn.Close();
            }
        }

        public Int64 QueryBigScale(string sql)
        {
            sql = sql.Replace('`', '"');
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql))
            {
                return QueryBigScale(cmd);
            }
        }

        public Int64 QueryBigScale(NpgsqlCommand cmd)
        {
            try
            {
                _BindConnection(ref cmd);
                _ReplaceNullToDBNull(ref cmd);

                return Convert.ToInt64(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null == _trans && null != _conn && _conn.State != ConnectionState.Closed)
                    _conn.Close();
            }
        }

        /// <summary>
        /// 執行非查詢SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            sql = sql.Replace('`', '"');
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql))
            {
                return ExecuteNonQuery(cmd);
            }
        }

        /// <summary>
        /// 執行非查詢SQL
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(NpgsqlCommand cmd)
        {
            try
            {
                _BindConnection(ref cmd);
                _ReplaceNullToDBNull(ref cmd);

                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null == _trans && null != _conn && _conn.State != ConnectionState.Closed)
                    _conn.Close();
            }
        }
    }
}
