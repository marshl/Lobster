﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Lobster
{
    [DisplayName("Table")]
    [XmlType(TypeName = "table")]
    public class Table : ICloneable
    {
        [DisplayName("Schema/Owner Name")]
        [Description("The schema/owner of this table")]
        public string schema { get; set; }

        [DisplayName("Name")]
        [Description("The name of this table")]
        public string name { get; set; }

        [DisplayName("Column List")]
        [Description("The columns in this table")]
        public List<Column> columns { get; set; }

        [DisplayName("Parent Table")]
        [Description("The parent table if this table is in a parent/child relationship.")]
        public Table parentTable { get; set; }

        public string FullName { get { return this.schema + "." + this.name; } }

        public override string ToString()
        {
            return this.FullName;
        }

        public void LinkColumns()
        {
            if (this.columns != null)
            {
                foreach (Column c in this.columns)
                {
                    c.parent = this;
                }
            }

            if (this.parentTable != null)
            {
                this.parentTable.LinkColumns();
            }
        }

        public string BuildUpdateStatement(ClobFile _clobFile)
        {
            Column clobCol = _clobFile.DatabaseFile.GetColumn();
            Debug.Assert(clobCol != null);

            string command =
                   "UPDATE " + this.FullName
                 + " SET " + clobCol.FullName + " = :data";

            Column dateCol = this.columns.Find(x => x.purpose == Column.Purpose.DATETIME);
            if (dateCol != null)
            {
                command += ", " + dateCol.FullName + " = SYSDATE";
            }

            if (this.parentTable != null)
            {
                Table pt = this.parentTable;
                Column fkCol = this.columns.Find(x => x.purpose == Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.columns.Find(x => x.purpose == Column.Purpose.ID);
                Column parentMnemCol = pt.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);

                command += " WHERE " + fkCol.FullName + " = ("
                        + " SELECT " + parentIDCol.FullName
                        + " FROM " + pt.FullName
                        + " WHERE " + parentMnemCol.FullName + " = '" + _clobFile.DatabaseFile.mnemonic + "')";
            }
            else
            {
                Column mnemCol = this.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);
                command += " WHERE " + mnemCol.FullName + " = '" + _clobFile.DatabaseFile.mnemonic + "'";
            }
            return command;
        }

        public string BuildInsertParentStatement(string _mnemonic)
        {
            Debug.Assert(this.parentTable != null);
            Table pt = this.parentTable;
            Column idCol = pt.columns.Find(x => x.purpose == Column.Purpose.ID);
            Column mnemCol = pt.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);
            Debug.Assert(idCol != null);
            Debug.Assert(mnemCol != null);

            string command = "INSERT INTO " + pt.FullName
                + " (" + idCol.FullName + ", " + mnemCol.FullName + " )"
                + " VALUES( " + idCol.NextID + "), :mnemonic )";

            return command;
        }

        public string BuildInsertChildStatement(string _mnemonic, string _mimeType)
        {
            Column mnemCol = this.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);
            Debug.Assert(mnemCol != null);
            Column dateCol = this.columns.Find(x => x.purpose == Column.Purpose.DATETIME);
            Column idCol = this.columns.Find(x => x.purpose == Column.Purpose.ID);
            Column dataCol = this.columns.Find(x => x.purpose == Column.Purpose.CLOB_DATA
               && (_mimeType == null || x.mimeTypes.Contains(_mimeType)));
            Column fullNameCol = this.columns.Find(x => x.purpose == Column.Purpose.FULL_NAME);

            // Make a string for the column names...
            string insertCommand = "INSERT INTO " + this.FullName + " ( "
                + mnemCol.FullName;

            // ..and another for the values, which will concatenated together
            string valueCommand = " VALUES ( '" + _mnemonic + "' ";

            if (this.parentTable != null)
            {
                Table pt = this.parentTable;
                Column fkCol = this.columns.Find(x => x.purpose == Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.columns.Find(x => x.purpose == Column.Purpose.ID);
                Column parentMnemCol = pt.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);

                insertCommand += ", " + fkCol.FullName;

                valueCommand += parentIDCol.NextID + ", "
                        + "( SELECT " + parentIDCol.FullName + " FROM " + pt.FullName
                        + " WHERE " + parentMnemCol.FullName + " = '" + _mnemonic + "' )";
            }
            else // No parent table
            {
                if (idCol != null)
                {
                    insertCommand += ", " + idCol.FullName;
                    valueCommand += ", " + idCol.NextID;
                }

                if (_mimeType != null)
                {
                    Column mimeCol = this.columns.Find(x => x.purpose == Column.Purpose.MIME_TYPE);
                    Debug.Assert(mimeCol != null);
                    insertCommand += ", " + mimeCol.FullName;
                    valueCommand += ", " + _mimeType;
                }
            }

            if (dateCol != null)
            {
                insertCommand += ", " + dateCol.FullName;
                valueCommand += ", SYSDATE ";
            }

            if (fullNameCol != null)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string fullName = textInfo.ToTitleCase(_mnemonic.ToLower());
                insertCommand += ", " + fullNameCol.FullName;
                valueCommand += ", '" + fullName + "'";
            }

            insertCommand += ", " + dataCol.FullName + " )";
            valueCommand += ", :data ) ";
            return insertCommand + valueCommand;
        }

        public string BuildGetDataCommand(ClobFile _clobFile)
        {
            Column clobCol = _clobFile.DatabaseFile.GetColumn();
            if (this.parentTable != null)
            {
                Table pt = this.parentTable;
                Column fkCol = this.columns.Find(x => x.purpose == Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.columns.Find(x => x.purpose == Column.Purpose.ID);
                Column parentMnemCol = pt.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);
                return
                    "SELECT " + clobCol.FullName
                    + " FROM " + pt.FullName
                    + " JOIN " + this.FullName
                    + " ON " + fkCol.FullName + " = " + parentIDCol.FullName
                    + " WHERE " + parentMnemCol.FullName + " = :mnemonic";
            }
            else
            {
                Column mnemCol = this.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);
                return
                    "SELECT " + clobCol.FullName
                    + " FROM " + this.FullName
                    + " WHERE " + mnemCol.FullName + " = :mnemonic";
            }
        }

        public string GetFileListCommand()
        {
            Column mimeCol = this.columns.Find(x => x.purpose == Column.Purpose.MIME_TYPE);

            if (this.parentTable != null)
            {
                Table pt = this.parentTable;
                Column fkCol = this.columns.Find(x => x.purpose == Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.columns.Find(x => x.purpose == Column.Purpose.ID);
                Column parentMnemCol = pt.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);

                return "SELECT " + parentMnemCol.FullName
                    + (mimeCol != null ? ", " + mimeCol.FullName : null)
                    + " FROM " + pt.FullName
                    + " JOIN " + this.FullName
                    + " ON " + fkCol.FullName + " = " + parentIDCol.FullName;
            }
            else
            {
                Column mnemCol = this.columns.Find(x => x.purpose == Column.Purpose.MNEMONIC);
                return "SELECT " + mnemCol.FullName
                    + (mimeCol != null ? ", " + mimeCol.FullName : null)
                    + " FROM " + this.FullName;
            }
        }

        public object Clone()
        {
            Table copy = new Table();
            copy.schema = this.schema;
            copy.name = this.name;

            if (this.parentTable != null)
            {
                copy.parentTable = (Table)this.parentTable.Clone();
            }

            if (this.columns != null)
            {
                copy.columns = new List<Column>();
                foreach (Column column in this.columns)
                {
                    copy.columns.Add((Column)column.Clone());
                }
            }
            return copy;
        }
    }
}
