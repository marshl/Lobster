
Lobster - The FOX File Pusher
=============

Lobster is designed as a more lightweight, easier to use, more feature complete replacement for Clobber, without the absurdity of being forced to use a specific beta version of the Java SDK just to get the thing to run. Also its written in C#, which is an order of magnitude better than Java.

###Using Lobster

After download and unzipping the latest release of Lobster, you should open up the LobsterSettings directory. Within  you will see the following files

    ClobTypes/
    Database Connections/
    media/
    ClobType.xsd
    DatabaseConfig.xsd
    MimeTypes.xml

The important two are the ClobTypes directory, and the DatabaseConnections directory. The media directory is where the success and fail sounds are located, which you can modify if you want to. The ClobType.xsd, DatabaseConfig.xsd and MimeTypes files are related to the ClobTypes and DatabaseConnections directories, and will explored in furhter detail below.

DatabaseConnections
-------------------------
Inside the DatabaseConnections directory you should put all of the connection files that you want lobster to be able to use. Each one specifies a database Host, SID, usernames and passwords need to access the database, and for that reason should never be placed into version control. However, from a fresh Lobster install, there will be a few example connections files that you can copy and modify. In the near future DatabaseConnection files will be modifiable within Lobster itself, but for now you will need to modify them manually:

    <?xml version="1.0" encoding="utf-8"?>
    <DatabaseConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <name>Home Database</name>
      <host>127.0.0.1</host>
      <port>11521</port>
      <sid>liamdb</sid>
      <username>promotemgr</username>
      <password>password</password>
      <codeSource>C:\IconWS\trunk\CodeSource</codeSource>
      <usePooling>true</usePooling>
      <clobTypeDir>C:\IconWS\trunk\LobsterClobTypes</clobTypeDir>
    </DatabaseConfig>

**Name**: This is the name of the database, and has no impact on the interals of Lobster, but this string is displayed in the list of DatabaseConnections in Lobster.
**Host**: The host location of the database server, in this case it is on the local machine. Entries in the hostnames file can also be used.
**Port**: The port that the server listens on. This will usually be the Oracle default of 1521.
**SID**: The Oracle System ID of the database to connect to
Username: The name of the user to connect as. It is important to connect as a user with the privileges to access every table that could be modifed by Lobster.
**Password**: The password of the user to connect as. This is stored as free text, and is why DatabaseConnection files should be stored securely.
**CodeSource**: This is the location of the CodeSource directory that is used for this database. If it is invalid, Lobster will prompt you as it starts up.
**UsePooling**: If pooling is enabled, when Lobster connects to the Oracle database Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string. This can improve Lobster connection performance slightly, but will decrease server performance if the maximum number of connections is low.
**ClobTypeDir**: ClobTypes are Lobster specific Xml files for describing the different tables located on the database and the rules that govern them (further description below). These files should be stored in a single directory alongside the CodeSource directory and can be version controlled as they contain no security information.

DatabaseConnection files are automatically validated using the DatabaseConnection.xsd schema. If your file fails to load, check the schema for detailed information about how a DatabaseConnection file should be formatted, check the logs for the exact validation error, or look at one of the example files provided.

ClobTypes
-------------
As mentioned above, ClobTypes are Lobsters specific Xml files for describing the different tables located on the database and the rules that gover nhow files will stored within them.
This directory is only an example, but it does contain some good examples for how to write your own ClobType. Lets look at one more in-depth:

    <?xml version="1.0" encoding="utf-8"?>
    <clobtype xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <name>FoxModules</name>
      <enabled>true</enabled>
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

**Name**: The name of the ClobType. Like the name of the DatabaseConnection, this has no functional bearing, but is used for display purposes.
**Enabled** [true/false]: If this field is set to false, then this ClobType file will be skipped.
**Directory**: The directory name in the CodeSource that this ClobType will govern. Note that this can contain path separators, so the directory does not have to be a top level CodeSource directory ( e.g. ApplicationMetadata/WorkRequestTypes )
**IncludeSubDirectories** [true/false]: If set to true, then Lobster will recursively dig through all directories in this directory for files to use.
**Tables**: This element contains one or more table definitions to use. Most ClobTypes will only use a single table, but you can define more. If you do, then you will be prompted which table you want to insert new files into.

### Table
**Schema**: The schema (or user) that the table is owned by. Usually ***mgr for most FOX tables.
Name: The name of the table
**Columns**: Every ClobType needs multiple columns to describe the table. Each one has a specific purpose for Lobster, and they are stored as a list in this element.

### Column:

**Name**: The name of the column.
**Purpose**: Every column must have a purpose, which include:

 - **ID**: The ID field of the table. This ID column can have an optional sequence element defined, which will be incremented and used to insert new IDs in the column. Otherwise the MAX+1 of the column will be used.
 - **CLOB_DATA**: The column where the data for the file will be stored. CLOB_DATA columns must also have the dataType element, which determines thee Oracle data type that will be used to insert the file into the database (CLOB, BLOB, or XMLTYPE). A table can have multiple CLOB_DATA fields, but each must have a different DataType.
 - **MNEMONIC**: The mnemonic column is what is used by Lobster to tie the database file to the local file, and is usually the name of the local file with no file extension.
 - **DATETIME:** Date time columns are automatically set to SYSDATE when the file is created or updated.
 - **FOREIGN_KEY:** The foreign key column is used by Lobster when using a table that has a parent able defined. The foreign key column will map to the ID column of the parent table. Parent-child table relationships will be discussed further below.
 - **MIME_TYPE**: The mime type column stores the type of the file that is clobbed. When you insert a new file through Lobster, it will prompt you for the mime type to use if the table has a mime type field. THe mime types in the database are converted to file extensions before being mapped to local files using the MimeTypes.xml file (discussed further below)
 - **FULL_NAME**: The full name column is a special case that will be rarely used. It is a column in the WorkRequestTypes table that is stored as an init-capped copy of the mnemonic.

**MimeTypes**: The mime type list is used exclusively by columns with the CLOB_DATA purpose. It defines which mime types can be used by this column. So for the FoxModules table, which stores text CLOB data in one column, and binary BLOB data in another, each column has a distinct list of mime types accepted by that column. A mime type should not appear in multiple columns, but no warning will be raised if it does. However an error will be raised if the user specifies a mime type that does not appear in any CLOB_DATA columns.

**Sequence**: if this element is attached to an ID column, then the sequence with this name will be automatically incremented and used when inserting data into this table.

The ClobType.xsd file is used to validate all ClobType files. If Lobster warns you that there are errors in your ClobType file, review the log for the exact problems, check the schema for a technical description of how to write a ClobType file, or look at some of the example ClobType files that come with Lobster.

