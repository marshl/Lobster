
# Lobster

Lobster is a file synchronisation program for Oracle databases, designed to automatically push files into the database when local changes are made.

## Quick Start Guide
1) Download and extract the latest Lobster release, then run Lobster.exe

2) Lobster will automatically open the Connection List window. 
Lobster uses the "CodeSource directories" which are directories which contain a configuration file that includes the databases that directory can be connected to and a list of "DirectoryDescriptors" which describe the different subdirectories in the CodeSource directory and what SQL should be used to synchronise those files to the database.

3) Initially there won't be any records in the CodeSource Directories list. Click "Add CodeSource" to add one. If you are using code that has already been prepared for use with Lobster, click "Add Prepared CodeSource" and find that CodeSource directory to add. Otherwise, click the "Prepare New CodeSource" button, find the directory you want to add and give it a good name.

4) If you've added a pre-prepared CodeSource it should already have one or more database connections to pick from. If not you will need to create a new database connection by selecting the CodeSource directory in the top list and then clicking the Add button beneath the bottom list. Fill in the information for the new connection on the right side of the screen and save it. You can test the connection to make sure the details are correct, or just click the connect button after it is saved.

5) All of this setup is saved, so you won't have to do anything the next time Lobster starts except choosing the config to use and connecting with it. To do that, just select the CodeSource then select the connection for it and click the Connect button.

## Using Lobster

Once you have chosen a connection, you will see the main Lobster screen, divided into three columns. The first column shows the directories under the CodeSource directory. Select one of these and the middle column will show the files in that directory.

### Local files

When you select a file in the centre column, buttons next to the file list can be used:
 - **Push** updates the database with the contents of the local file. This is done automatically when the file is changed by another program, but it can be done manually here.
 - **Pull** downloads the file as it is currently stored in the database, saves it to a local file and opens it. 
 - **Diff** downloads the file like Pull does, but instead opens the local and database versions of the file in a merging program of your choosing (in user options). 
 - **Explore** opens the location of the file in Windows Explorer.
 - **Insert** creates a new row in the database table for the file if it isn't in the database yet.
 - **Delete** deletes the file from the database

The right hand column shows the backup log for the currently selected file. When a file is pushed to the database, either automatically or manually, a backup copy of the database data is saved locally before it is overridden with the local data.

### Connections

Connections are opened in tabs, and you can be have multiple connections open at the same time in multiple tabs. Each connection has some controls across the top you can use:
- **Autopush** toggles whether files should be pushed to the database automatically when changed. This can be changed in the  level or the DirectoryDescriptor level as well.
- **Reload** loads the DirectoryDescriptor configuration files and resynchronises with the list of local files. If you created a new file while Lobster was open and you want to insert it, you'll have to press Reload so that Lobster can find that new local file.
- **Disconnect** closes teh connection to the database. This will mean any further changes to local file won't be automatically pushed.


## LobsterLite

If you want to run a more lightweight version of Lobster, or run it on a platform that doesn't support WPF (such as Mono) then you can instead use LobsterLite.

You should execute LobsterLite in the following manner

`./LobsterLite.exe CodeSourceDir [--command file]`

If you omit additional options, then a connection will be made to one of the databases specified in the given CodeSource directory (you will be prompted to pick one) and it will then wait for the user to modify local files and then automatically update them.

If you specify a command (one of either push, insert or delete) then that command will be executed for the given file.