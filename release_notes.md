##Release 2.3.1.0
 - Fixed a crash caused by inserting files with a mime types that has no prefix (such as a module):
 - Fixed a crash that would occur if the user attempts to save database configuration to a readonly file;
 - Changed the unhandled exception message;
##Release 2.3.0.0
 - Changed the configuration storage to place the DatabaseConfig and ClobTypes in the CodeSource directory (breaking change);
 - Fixed casing in the Xml files (breaking change);
 - Improved the layout of the Message window;
 - Changed the message log view to more efficiently update the UI when a new message is logged;
 - Changed the DatabaseConnection to only log file event messages when the user setting FileLogEvents is set;
 - Changed the appearance of the backup log;
 - Added the ability to "Pull" files from the database and execute them immediately;
 - Fixed a bug in the auto-updater where the wrong path would be used to restart the executable;
 - Fixed a bug where closing the program after an auto-update would instead crash;

##Release 2.2.0.0
 - Improved the file tree to remember which nodes were expanded when the tree is refreshed
 - Fixed a bug whereby old connections were not properly closed after opening a new connection
 - Added a message list window to show the user what events have taken place
 - Changed all windows to use the Lobster icon
 - The log file is no longer locked on program startup
 - Changed the backup system to store files in a mirror of the CodeSource directory tree, with a directory for each file, and files with dates for names for each backup. The backups can now be used between sessions
 - Changed the auto-updater to run on a separate thread
 - Changed the auto-updater to wait on process ID, instead of process name
 - Changed the logger to flush after every write
 - Fixed the automatic updates setting not working
 - Changed the connection list window to only prompt to cancel unsaved changes if changes were actually made