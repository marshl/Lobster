using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Data;
using System.IO;

namespace Lobster
{
    public class ClobFile
    {
        public ClobFile( FileInfo _fileInfo, ClobNode _parentClobNode, ClobDirectory _parentClobDirectory )
        {
            this.fileInfo = _fileInfo;
            this.parentClobNode = _parentClobNode;
            this.parentClobDirectory = _parentClobDirectory;    
        }
        public FileInfo fileInfo;
        public ClobNode parentClobNode;

        public ClobDirectory parentClobDirectory;
        
        public DateTime lastClobbed;

        public STATUS status;
        public enum STATUS
        {
            SYNCHRONISED,
            LOCAL_ONLY,
            DELETED,
        }

        public bool UpdateDatabase()
        {
            string mnemonic = this.fileInfo.Name.Replace( ".xml", "" );
            OracleConnection con = this.parentClobDirectory.parentModel.oracleCon;
            OracleCommand command = con.CreateCommand();

            if ( this.parentClobDirectory.clobType.hasParentTable )
            {
                command.CommandText =
                    "UPDATE " + this.parentClobDirectory.clobType.schema + "." + this.parentClobDirectory.clobType.table + " child"
                    + " SET " + this.parentClobDirectory.clobType.clobColumn + " = :data"
                    + " WHERE " + this.parentClobDirectory.clobType.mnemonicColumn + " = ("
                        + "SELECT parent." + this.parentClobDirectory.clobType.parentIDColumn
                        + " FROM " + this.parentClobDirectory.clobType.schema + " parent"
                        + " WHERE parent." + this.parentClobDirectory.clobType.parentMnemonicColumn + " = " + mnemonic;
            }
            else
            {
                command.CommandText = "UPDATE " + this.parentClobDirectory.clobType.schema + "." + this.parentClobDirectory.clobType.table
                    + " SET " + this.parentClobDirectory.clobType.clobColumn + " = :data"
                    + " WHERE " + this.parentClobDirectory.clobType.mnemonicColumn + " = '" + mnemonic + "'";
            }
            string fullPath = this.fileInfo.FullName;
            string contents = File.ReadAllText( fullPath );

            command.Parameters.Add( "data", OracleDbType.XmlType, contents, ParameterDirection.Input );
            try
            {
                command.ExecuteNonQuery();
            }
            catch ( Exception _e )
            {
                Console.WriteLine( "Error updating database: " + _e.Message );
                return false;
            }
            command.Dispose();
            return true;
        }

        public bool InsertIntoDatabase()
        {
            string mnemonic = this.fileInfo.Name.Replace( ".xml", "" );
            OracleConnection con = this.parentClobDirectory.parentModel.oracleCon;
            OracleCommand command = con.CreateCommand();
            command.CommandText = "INSERT INTO " + this.parentClobDirectory.clobType.schema + "." + this.parentClobDirectory.clobType.table
                + " ( " + this.parentClobDirectory.clobType.mnemonicColumn + ", " + this.parentClobDirectory.clobType.clobColumn + " )"
                + " VALUES ( '" + mnemonic + "', :data )";
            string contents = File.ReadAllText( this.fileInfo.FullName );

            command.Parameters.Add( "data", OracleDbType.XmlType, contents, ParameterDirection.Input );

            try
            {
                command.ExecuteNonQuery();
            }
            catch ( Exception _e )
            {
                Console.WriteLine( "Error creating new clob: " + _e.Message );
                return false; 
            }
            command.Dispose();
            return true;
        }


    }
}
