using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Data;
using System.IO;

namespace Lobster
{
    public class ClobFile
    {
        public ClobType parentClobType;

        public string filename;
        
        public DateTime lastClobbed;
        public DateTime lastModified;

        public STATUS status;
        public enum STATUS
        {
            SYNCHRONISED,
            LOCAL_ONLY,
            DELETED,
        }

        public string GetFullPath()
        {
            return this.parentClobType.parentModel.dbConfig.codeSource + "/" + this.parentClobType.directory + "/" + this.filename;
        }

        public void UpdateDatabase()
        {
            string mnemonic = this.filename.Replace( ".xml", "" );
            OracleConnection con = this.parentClobType.parentModel.oracleCon;
            OracleCommand command = con.CreateCommand();
            command.CommandText = "UPDATE " + this.parentClobType.schema + "." + this.parentClobType.table
                + " SET " + this.parentClobType.clobColumn + " = :data" 
                + " WHERE " + this.parentClobType.mnemonicColumn + " = '" + mnemonic + "'";
            string fullPath = this.GetFullPath();
            string contents = File.ReadAllText( this.GetFullPath() );

            command.Parameters.Add( "data", OracleDbType.XmlType, contents, ParameterDirection.Input );
            command.ExecuteNonQuery();
            command.Dispose();
        }

        public void InsertIntoDatabase()
        {
            string mnemonic = this.filename.Replace( ".xml", "" );
            OracleConnection con = this.parentClobType.parentModel.oracleCon;
            OracleCommand command = con.CreateCommand();
            command.CommandText = "INSERT INTO " + this.parentClobType.schema + "." + this.parentClobType.table
                + " ( " + this.parentClobType.mnemonicColumn + ", " + this.parentClobType.clobColumn + " )"
                + " VALUES ( '" + mnemonic + "', :data )";
            string fullPath = this.GetFullPath();
            string contents = File.ReadAllText( this.GetFullPath() );

            command.Parameters.Add( "data", OracleDbType.XmlType, contents, ParameterDirection.Input );
            command.ExecuteNonQuery();
            command.Dispose();
        }


    }
}
