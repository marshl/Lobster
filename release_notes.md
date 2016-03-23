##Release 2.4.0.0
#####Released: 24-03-2016

####Features
 - Added a new element to ClobTypes to prevent automatic file updates for files in that directory;
 - Changed the auto-updater to display the download progress in a popup window that also lets the user cancel the update;
 - Updated the readme
 
####Bug Fixes
 - Fixed the "IncludeSubDirectories" flag not preventing file auto-updates or changing the file tree display;
 - Fixed a bug where Lobster could crash when attempting to dispose of the message log;
 - Fixed a possible crash if a ClobType file has no tables;
 - Fixed a bug that prevented success/failure sounds from playing;
 - Fixed a bug where newly inserted files would still appear green;
 - Fixed some issues where the connection window or password prompt window could appear behind other windows;
 
####Appearance
 - Added tooltips for most actions in the connection list and main windows;
 - Improved the layout of the backup list;
 - Improved the layout and title of the password prompt window;
 - Changed the "Cancel Unsaved Changes" popup to have a fallback value if the database config does not have a name;

##Release 2.3.1.0
#####Released: 23-02-2016

####Bug Fixes
 - Fixed a crash caused by inserting files with a mime types that has no prefix (such as a module):
 - Fixed a crash that would occur if the user attempts to save database configuration to a readonly file;
####Appearance
 - Changed the unhandled exception message;
 
##Release 2.3.0.0
#####Released: 22-02-2016

####Breaking Changes
 - Changed the configuration storage to place the DatabaseConfig and ClobTypes in the CodeSource directory (breaking change);
 - Fixed casing in the Xml files (breaking change);
 
####Features
 - Added the ability to "Pull" files from the database and execute them immediately;
 - Changed the message log view to more efficiently update the UI when a new message is logged;
 - Changed the DatabaseConnection to only log file event messages when the user setting FileLogEvents is set;
 
####Bug Fixes
 - Fixed a bug in the auto-updater where the wrong path would be used to restart the executable;
 - Fixed a bug where closing the program after an auto-update would instead crash;
 
####Appearance
 - Improved the layout of the Message window;
 - Changed the appearance of the backup log;

##Release 2.2.0.0
#####Released: 15-02-2016

####Features
 - Improved the file tree to remember which nodes were expanded when the tree is refreshed
 - Added a message list window to show the user what events have taken place
 - Changed the backup system to store files in a mirror of the CodeSource directory tree, with a directory for each file, and files with dates for names for each backup. The backups can now be used between sessions
 - Changed the connection list window to only prompt to cancel unsaved changes if changes were actually made
 - The log file is no longer locked on program startup
 - Changed the auto-updater to run on a separate thread
 - Changed the auto-updater to wait on process ID, instead of process name
 - Changed the logger to flush after every write
 
####Bug Fixes
 - Fixed a bug whereby old connections were not properly closed after opening a new connection
 - Fixed the automatic updates setting not working
 
####Appearance
 - Changed all windows to use the Lobster icon