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
            SYNCED,
            LOCAL_ONLY,
            DELETED,
        }

        public string GetFullPath()
        {
            return this.parentClobType.parentModel.dbConfig.codeSource + "/" + this.parentClobType.directory + "/" + this.filename;
        }

        public void ClobToDatabase()
        {
            OracleConnection con = this.parentClobType.parentModel.oracleCon;
            OracleCommand command = con.CreateCommand();
            string schema = this.parentClobType.schema;
            string tableName = this.parentClobType.table;
            string clobColumn = this.parentClobType.clobColumn;
            string mnemonicColumn = this.parentClobType.mnemonicColumn;
            string mnemonic = this.filename.Replace( ".xml", "" );

            //command.CommandText = "UPDATE envmgr.nav_bar_action_groups SET xml_data = :data";
            command.CommandText = "UPDATE " + schema + "." + tableName + " SET " + clobColumn + " = :data WHERE " + mnemonicColumn + " = '" + mnemonic + "'";
            // OracleParameter dataParam = new OracleParameter( "input", OracleDbType.XmlType );
            string fullPath = this.GetFullPath();
            string contents = File.ReadAllText( this.GetFullPath() );
            //OracleClob clob = OracleClob.
            //OracleXmlType xmlType = new OracleXmlType();
            //command.Parameters.Add( new OracleParameter( "data", OracleDbType.XmlType, contents )  );
            command.Parameters.Add( "data", OracleDbType.XmlType, contents, ParameterDirection.Input );

            //dataParam.Value = contents;
            command.ExecuteNonQuery();
            /*OracleDataReader reader = command.ExecuteReader();

            while ( reader.Read() )
            {
                Console.WriteLine( reader.GetString( 0 ) );
            }

            reader.Dispose();*/
            command.Dispose();
        }
    }
}
