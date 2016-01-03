##Lobster - Database File Synchroniser###

Lobster a file synchronisation program for Oracle databases, designed to automatically push files into the database when local changes are made.

###Installation Guide
1) Download and extract the latest Lobster release

2) Run Lobster.exe

3) Lobster will automatically open the Connection List window. It is here that all the different databases Lobster can connect to are displayed. To connect Lobster to a database, a new database configuration needs to be created.

4) First a place must be chosen to store the configuration files. All connection files are stored in a single directory, and cannot be distributed as they contain sensitive information, such as database passwords. Click "Change Directory" and a folder selector dialog will be opened. Create a new directory in a place of your choosing and select that as your Database Connection folder.

5) Now create a new Database Connection configuration file by clicking the "New Connection" and fill in the following details of the database on the right hand side. To test the currant settings, click "Test Connection".

**Name**: The name of the database (this is used for display purposes only).

**Host**: The host of the database server. Entries in the hostnames file can also be used.

**Port**: The port that the server listens on.

**SID**: The Oracle System ID of the database to connect to.

**Username**: The name of the user to connect as. It is important to connect as a user with the privileges to access every table that could be modified by Lobster, such as xviewmgr.

**Password**: The password of the user to connect with.

**CodeSource**: This is the location of the CodeSource directory that is used for this database.

**UsePooling**: If pooling is enabled, when Lobster connects to the Oracle database Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string. This can improve Lobster connection performance slightly, but will decrease server performance if the maximum number of connections is low.

**ClobTypeDir**: Ignore this for now.

6) Lastly the ClobType Directory for the connection needs to be set. A ClobType is a Lobster specific Xml file that basically describes how fies from a single local directory in the CodeSource folder are mapped to the rows in a single table on the database. It can get more complex then that, as a ClobType could operate on more than one table, or multiple directories, but that is the basic idea. These files can be stored in a single directory and can be references by more than one Database Connection. For example, PharmCIS Dev and PharmCIS SIT can use the same ClobTypes.

I have already created the ClobTypes for ICON and for PharmCIS, and are located in the svn directories /ICONWS/trunk/LobsterClobTypes and /PharmCIS/LobsterClobTypes respectively. Checkout the necessary ClobType directory to your machine and set the ClobType Directory in Lobster to that location.

7) Click "Save" to save the database configuration out to file. make sure you save it in the Database Connection directory chosen earlier so Lobster will load it next time it is run.

8) All of this setup is saved, so you won't have to do anything the next time Lobster starts except choosing the config to use and connecting with it. To do that, just click "Connect".

###Using Lobster

Once you have chosen a connection, you will see the main Lobster screen, divided into three columns. The first column shows the ClobTypes for the current connection, selecting one of these will populate the central column with the files found in the directory for that type. These files can be displayed in two ways, controlled by the radio buttons below the list: local mode and database mode. In local mode, the file pane shows the files that are found locally, with those that aren't found on the database highlighted green. In database mode, the list shows all the files on the database, with those that are database only highlighted in blue. Readonly files can be hidden so only the files you are currently working on are shown.
When you select a file in the centre column, buttons to push, diff, explore and insert the file can be enabled. Push updates the database with the contents of the local file. This is done automatically when the file is changed by another program, but it can be done manually here. Diff opens the local and database versions of the file in a merging program of your choosing. Explpre opens the location of the file in Windows Explorer. Insert creates a new row in the database table for the file if it isn't in the database yet.
The right hand column shows the backup log for the currently selected file. When a file is pushed to the database, either automatically or manually, a backup copy of the database data is saved locally before it is overriden with the local data. The backup log can be used to push a previous version in place of the current version.

###Creating Your Own ClobType
Clob Types are Lobsters specific Xml files for describing the different tables located on the database and the rules that govern how files will stored within them. Every Database Connection file specifies where the ClobTypes for that connection is stored (which unlike Database Connections can be stored in version control).
Here is an example for FoxModules:
```xml
    <?xml version="1.0" encoding="utf-8"?>
    <clobtype xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
	 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <name>FoxModules</name>
      <directory>FoxModules</directory>
      <includeSubDirectories>true</includeSubDirectories>
      <tables>
        <table>
          <schema>envmgr</schema>
          <name>fox_components</name>
          <columns>
            <column>
              <name>name</name>
              <purpose>MNEMONIC</purpose>
            </column>
            <column>
              <name>data</name>
              <purpose>CLOB_DATA</purpose>
              <dataType>CLOB</dataType>
              <mimeTypes>
                <string>module</string>
                <string>text/html</string>
                <string>text/css</string>
                <string>text/javascript</string>
              </mimeTypes>
            </column>
            <column>
              <name>bindata</name>
              <purpose>CLOB_DATA</purpose>
              <dataType>BLOB</dataType>
              <mimeTypes>
                <string>image/gif</string>
                <string>image/jpg</string>
                <string>image/png</string>
                <string>image/x-icon</string>
              </mimeTypes>
            </column>
            <column>
              <name>type</name>
              <purpose>MIME_TYPE</purpose>
            </column>
          </columns>
        </table>
      </tables>
    </clobtype>
```
**Name**: The name of the ClobType. Like the name of a Database Connection, this has no functional bearing, but is used for display purposes.

**Directory**: The directory name in the CodeSource that this Clob Type will use. Note that this can contain path separators, so the directory does not have to be a top level CodeSource directory ( e.g. ApplicationMetadata/WorkRequestTypes )

**IncludeSubDirectories** [true/false]: If set to true, then Lobster will recursively dig through all directories in this directory for files to use.

**Tables**: This element contains one or more table definitions to use. Most Clob Types will only use a single table, but you can define more. A Clob Type with multiple tables functions the same a Clob Type with one, however you will be prompted which table you want to insert new files into.

### Table
**Schema**: The schema (or user) that owns the table.
Name: The name of the table
**Columns**: Every ClobType needs multiple columns to describe the table. Each one has a specific purpose for Lobster, and they are stored as a list in this element.

### Column:

**Name**: The name of the column.

**ParentTable**: The parent table is an optional Table element used for ClobTypes that have a parent-child relationship between two tables. For example, the BusinessProcessDefinitions folder has its files stored in the business_process_definitions table, with an ID column foreign keyed to the business_processes table. The business_processes table contains only an ID column and a mnemonic column, which the business_process_definitions table uses to link a file name to a specific row.

*Note: Lobster does not support ParentTables with their own ParentTables.*

**Purpose**: Every column must have a purpose, which must be one of the following:

 - **ID**: The ID field of the table. This ID column can have an optional sequence element defined, which will be incremented and used to insert new IDs in the column. Otherwise the MAX+1 of the column will be used.
 - **CLOB_DATA**: The column where the data for the file will be stored. CLOB_DATA columns must also have the dataType element, which determines thee Oracle data type that will be used to insert the file into the database (CLOB, BLOB, or XMLTYPE). A table can have multiple CLOB_DATA fields, but they must each have a different dataType.
 - **MNEMONIC**: The mnemonic column is what is used by Lobster to tie the database file to the local file, and is usually the name of the local file with no file extension.
 - **DATETIME**: Date time columns are automatically assigned to SYSDATE when the file is created or updated.
 - **FOREIGN_KEY**: The foreign key column is used by Lobster when using a table that has a parent able defined. The foreign key column will map to the ID column of the parent table.
 - **MIME_TYPE**: The mime type column stores the type of the file that is clobbed. When you insert a new file through Lobster, it will prompt you for the mime type to use if the table has a mime type field. The mime types in the database are converted to file extensions before being mapped to local files using the MimeTypes.xml file (discussed further below)
 - **FULL_NAME**: The full name column is a special case that will be rarely used. It is a column in the WorkRequestTypes table that is stored as an init-capped copy of the mnemonic.

**MimeTypes**: The mime type list is used exclusively by columns with the CLOB_DATA purpose. It defines which mime types can be used by this column. So for the FoxModules table, which stores text CLOB data in one column, and binary BLOB data in another, each column has a distinct list of mime types accepted by that column. A mime type should not appear in multiple columns, but no warning will be raised if it does. However an error will be raised if you specify a mime type that does not appear in any CLOB_DATA columns.

**Sequence**: if this element is attached to an ID column, then the sequence with this name will be automatically incremented and used when inserting data into this table.