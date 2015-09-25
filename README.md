> *"Sh! sh!" they hissed, when they heard his voice ...*
> *But in the end, when Bilbo actually began to stamp in the floor,* 
> *and screamed out light!' at the top of his thrill voice, Thorin gave*
> *way*
> 
> [ _The Hobbit_, XIII: "Not At Home"]

##Lobster##

### The Database File Synchroniser###

Lobster a file synchronisation program for Oracle databases, designed to automatically push files into the database when local changes are made.

###Configuring Lobster

After downloading and unzipping the latest release of Lobster, you should first open up the LobsterSettings directory. Within  you will see the following files

    media/
    ClobType.xsd
    DatabaseConfig.xsd
    MimeTypes.xml

Along with these directories create by Lobster, there are two other important ones that Lobster needs to operator: the Database Connections directory, and one or more Clob Type directories.  The media directory is where the success and fail sounds are located, which you can modify if you want to. The ClobType.xsd, DatabaseConfig.xsd and MimeTypes files are related to the ClobTypes and DatabaseConnections directories, and will explored in further detail below.

Database Connections
--------------------------
When Lobster is first started, you will prompted to choose a Database Connection directory. Inside that directory you should put a connection file for each database that you want Lobster to be able to connect to. Each file contains a database host, SID, username and password required to connect to the database, and therefore should never be placed into version control.  Database Connection files are modifiable within Lobster itself, or they can be edited manually. Here an example connection file:

```xml
    <?xml version="1.0" encoding="utf-8"?>
    <DatabaseConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
     xmlns:xsd="http://www.w3.org/2001/XMLSchema">
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
```

**Name**: This is the name of the database, and has no impact on  how Lobster connects to it, but this string is used for display purposes.

**Host**: The host location of the database server, in this case it is on the local machine. Entries in the hostnames file can also be used.

**Port**: The port that the server listens on. This will usually be the Oracle default of 1521.

**SID**: The Oracle System ID of the database to connect to.

**Username**: The name of the user to connect as. It is important to connect as a user with the privileges to access every table that could be modified by Lobster, such as xviewmgr.

**Password**: The password of the user to connect as. This is stored as free text, and is why DatabaseConnection files should be stored securely.

**CodeSource**: This is the location of the CodeSource directory that is used for this database. If it is invalid, Lobster will prompt you as it starts up.

**UsePooling**: If pooling is enabled, when Lobster connects to the Oracle database Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string. This can improve Lobster connection performance slightly, but will decrease server performance if the maximum number of connections is low.

**ClobTypeDir**: ClobTypes are Lobster specific Xml files for describing the different tables located on the database and the rules that govern them (further description below). These files should be stored in a single directory alongside the CodeSource directory and can be version controlled as they contain no security information.

###DatabaseConnection.xsd

Database Connection files are automatically validated using the DatabaseConnection.xsd schema. If your file fails to load, check the schema for detailed information about how a Database Connection file should be formatted or check the logs for the exact validation error..

Clob Types
-------------
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

###ClobType.xsd

The ClobType.xsd file is used to validate all Clob Type files. If Lobster warns you that there are errors in your Clob Type file, review the log for the exact problems and check the schema for a technical description of how to write a Clob Type file.

###Using Lobster
Please take everything beyond this point with a pinch of salt, as I haven't nailed down the UI of Lobster yet, and it may change without updates to this readme.

When you start Lobster you will first see a list that shows all the connection files that Lobster found in the specified DatabaseConnections folder. Lobster can only be connected to one database at at time, and it is here where you select which database to connect to.

Select your database connection, and then click Connect

If the connection is successful, you will be moved to the second tab. Here you will see two panes. The left pane is the a tree view representation of the folders in the CodeSource directory that have a ClobType associated with them.

If you select one of the folders, the right hand pane will then display all files associated with that folder. A file can be in one of three states:

 - **Synchronised**: A synchronised file is one that is both on the local computer and on the database, and has successfully been connected by Lobster. Saving any changes to a synchronised file will automatically push those changes to the database.
 - **Local Only**: Local only files are files that do not have a counterpart on the database. These files can be inserted using the Insert button. Once a local-only file has been inserted, it will become synchronised.
 - **Database Only**: A database only file is one that does not have a local counterpart. These files may have been deleted your folder, from the repository itself, or they may have been automatically created by FOX.

The third tab is the Working File List. Any files that are not marked as read-only will appear here. This includes files that are synchronised, as well as files that are local-only.

Both the Tree View tab and the Working Files tab have the same set of actions in the action ribbon. Those  actions are:

 - **Refresh**: This button re-queries the database and re-parses the local file system for files. The local directories are automatically re-parsed when any modifications to them are made, but this action can be used if there are new files on the database.
 - **Insert**: This button is only enabled when a local-only file is selected. It creates a new row in the table for that file's Clob Type and inserts the file data. If the Clob Type has more than one table, Lobster will ask which table you want to insert into. If the Clob Type uses mime-types, then Lobster will ask which mime-type this file should be inserted as.
 - **Reclob**: This button is only enabled when a synchronised file is selected. It updates the file data in the row that corresponds to that file with the data stored locally. This action is called automatically whenever the file is modified, but this action can force and update if the file is locked, although you will be prompted.
 - **Explore**: This button is only enabled for files that are located locally (local-only or synchronised). It opens a new instance of Windows Explorer in the directory of the folder, with the file selected.
 - **Diff**: This button is only enabled when a synchronised file is selected. It opens a new instance your specified merging program (TortoiseMerge by default) with the local version and database versions of the file displayed. If the file has been last updated by Clobber or Lobster, there will be a comment at the end of the file specifying when and by who.
 - **Pull**: This button is only enabled for files that are database-only. It pulls down a copy of the file into a temporary location and executes it with the default application.
