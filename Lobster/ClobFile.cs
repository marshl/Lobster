﻿using Oracle.DataAccess.Client;
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

        public STATUS? status;
        public enum STATUS
        {
            SYNCHRONISED,
            LOCAL_ONLY,
        }

        public bool UpdateDatabase()
        {
            string mnemonic = this.fileInfo.Name.Replace( ".xml", "" );
            OracleConnection con = this.parentClobDirectory.parentModel.oracleCon;
            OracleCommand command = con.CreateCommand();

            ClobType ct = this.parentClobDirectory.clobType;

            if ( ct.hasParentTable )
            {
                command.CommandText =
                    "UPDATE " + ct.schema + "." + ct.table + " child"
                    + " SET " + ct.clobColumn + " = :data"
                    + " WHERE " + ct.mnemonicColumn + " = ("
                        + "SELECT parent." + ct.parentIDColumn
                        + " FROM " + ct.schema + " parent"
                        + " WHERE parent." + ct.parentMnemonicColumn + " = " + mnemonic;
            }
            else
            {
                command.CommandText = "UPDATE " + ct.schema + "." + ct.table
                    + " SET " + ct.clobColumn + " = :data"
                    + " WHERE " + ct.mnemonicColumn + " = '" + mnemonic + "'";
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
            OracleTransaction trans = con.BeginTransaction();
            ClobType ct = this.parentClobDirectory.clobType;

            if ( ct.hasParentTable )
            {
                command.CommandText = "INSERT INTO " + ct.schema + "." + ct.parentTable
                    + " (" + ct.parentIDColumn + ", " + ct.parentMnemonicColumn + " )"
                    + " VALUES( ( SELECT MAX( " + ct.parentIDColumn + " ) + 1 "
                    + " FROM " + ct.schema + "." + ct.parentTable + " ), '" + mnemonic + "' )";

                try
                {
                    command.ExecuteNonQuery();
                }
                catch ( Exception _e )
                {
                    trans.Rollback();
                    Console.WriteLine( "Error creating new clob: " + _e.Message );
                    return false;
                }
                command.Dispose();

                command = con.CreateCommand();
                command.CommandText = "INSERT INTO " + ct.schema + "." + ct.table
                    + "( " + ct.mnemonicColumn + ", " + ct.parentIDColumn + ", start_datetime, " + ct.clobColumn + " )"
                    + "VALUES( ( SELECT MAX( " + ct.parentIDColumn + " ) + 1 FROM " + ct.schema + "." + ct.table + " ), "
                    + "( SELECT " + ct.parentIDColumn + " FROM " + ct.schema + "." + ct.parentTable
                        + " WHERE " + ct.parentMnemonicColumn + " = '" + mnemonic + "'),"
                    + " SYSDATE, :data )";
            }
            else
            {
                command.CommandText = "INSERT INTO " + ct.schema + "." + ct.table
                    + " ( " + ct.mnemonicColumn + ", " + ct.clobColumn + " )"
                    + " VALUES ( '" + mnemonic + "', :data )";
                
            }

            string contents = File.ReadAllText( this.fileInfo.FullName );
            command.Parameters.Add( "data", OracleDbType.XmlType, contents, ParameterDirection.Input );

            try
            {
                command.ExecuteNonQuery();
            }
            catch ( Exception _e )
            {
                trans.Rollback();
                Console.WriteLine( "Error creating new clob: " + _e.Message );
                return false; 
            }
            command.Dispose();
            trans.Commit();
            this.status = STATUS.SYNCHRONISED;
            return true;
        }
    }
}
