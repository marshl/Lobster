      "There need not be, there would not be, any real change in our designs, only in our means."
        - Saruman
    
      [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]


##Lobster

Lobster is a file synchronisation program for Oracle databases, designed to automatically push files into the database when local changes are made.

###Installation Guide
1) Download and extract the latest Lobster release, then run Lobster.exe

2) Lobster will automatically open the Connection List window. It is here that all the different databases Lobster can connect to are displayed. To connect Lobster to a database, a new database configuration needs to be created. 

3) For both PharmCIS and NOCI, the necessary configuration files have already been created. In this case, make sure that your repo is up to date, and use the "Add Existing Connection" button to select the CodeSource folder, and you are done configuring Lobster, and can proceed to the Using Lobster section

4) If you wish to initialise a new directory for a connection, click the "Create Connection" button and select a directory. This will add the LobsterConnection.xml file and LobsterTypes/ directory to that location. If Lobster warns you that those files/folders are already in that directory, you may be attempting to initialise a directory that has already been set up (see step 3).

5) Now fill in the following details of the database server on the right hand side.

**Name**: The name of the database (this is used for display purposes only).

**Host**: The host of the database server. Entries in the hostnames file can also be used.

**Port**: The port that the server listens on.

**SID**: The Oracle System ID of the database to connect to.

**Username**: The name of the user to connect as. It is important to connect as a user with the privileges to access every table that could be modified by Lobster, such as xviewmgr.

**UsePooling**: When Lobster connects to the Oracle database with pooling enabled, Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string. This can improve Lobster connection performance slightly, but will decrease server performance if the maximum number of connections is low.

**ClobTypes**: We'll deal with this one later.

6) Ensure that you can connect to the database by clicking "Test Connection". If you encounter an error, check your configuration and try again.

7) Now that Lobster can connect to the database, you need to tell it how it should map the directories in the CodeSource folder to tables in the database. Each one of these mapping is known as a "ClobType". Example ClobTypes can be found in the GitHub repo under /ExampleClobTypes, or you can create your own. See "Creating Your Own ClobTypes" 

6) Click "Save" to save the database configuration out to file.

7) All of this setup is saved, so you won't have to do anything the next time Lobster starts except choosing the config to use and connecting with it. To do that, just click "Connect" and enter the password.

###Using Lobster

Once you have chosen a connection, you will see the main Lobster screen, divided into three columns. The first column shows the ClobTypes for the current connection. Selecting one of these will populate the middle column with the files found in the directory for that type. These files can be displayed in two ways, controlled by the radio buttons below the list: local mode and database mode. In local mode, the file pane shows the files that are found locally, with those that aren't found on the database highlighted green. In database mode, the list shows all the files on the database, with those that are database only highlighted in blue. Read-only files can be hidden so only the files you are currently working on are shown.

When you select a file in the centre column, buttons to push, pull, diff, explore and insert the file can be enabled:
 - **Push** updates the database with the contents of the local file. This is done automatically when the file is changed by another program, but it can be done manually here.
 - **PUll** downloads the file as it is currently stored in the database, saves it to a local file and opens it. 
 - **Diff** downloads the file like Pull does, but instead opens the local and database versions of the file in a merging program of your choosing (in user options). 
 - **Explore** opens the location of the file in Windows Explorer.
 - **Insert** creates a new row in the database table for the file if it isn't in the database yet.

The right hand column shows the backup log for the currently selected file. When a file is pushed to the database, either automatically or manually, a backup copy of the database data is saved locally before it is overridden with the local data. The backup log can be used to push a previous version in place of the current version.

###Creating Your Own ClobType
Clob Types are Lobsters specific Xml files for describing the different tables located on the database and the rules that govern how files will stored within them. Every Database Connection file specifies where the ClobTypes for that connection is stored (which unlike Database Connections can be stored in version control).
Here is an example for FoxModules:
```xml
<?xml version="1.0" encoding="utf-8"?>
<ClobType xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Name>Fox5Modules</Name>
  <Enabled>true</Enabled>
  <Directory>Fox5Modules</Directory>
  <IncludeSubDirectories>true</IncludeSubDirectories>
  <Tables>
    <Table>
      <Schema>envmgr</Schema>
      <Name>fox_components_fox5</Name>
      <Columns>
        <Column>
          <Name>name</Name>
          <Purpose>MNEMONIC</Purpose>
        </Column>
        <Column>
          <Name>data</Name>
          <Purpose>CLOB_DATA</Purpose>
          <DataType>CLOB</DataType>
          <MimeTypes>
            <string>module</string>
            <string>text/css</string>
            <string>text/javascript</string>
            <string>text/mustache</string>
          </MimeTypes>
        </Column>
        <Column>
          <Name>bindata</Name>
          <Purpose>CLOB_DATA</Purpose>
          <DataType>BLOB</DataType>
          <MimeTypes>
            <string>image/png</string>
          </MimeTypes>
        </Column>
        <Column>
          <Name>type</Name>
          <Purpose>MIME_TYPE</Purpose>
        </Column>
      </Columns>
    </Table>
  </Tables>
</ClobType>
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

**Sequence**: if this element is attached to an ID column, then the sequence with this name will be automatically incremented when a new row is inserted into the table.