## Release 3.1.0
##### Released: 12-11-2018

#### Breaking Changes
 - Changed from using LobsterTypes to using LobsterDescriptors, which contain the SQL used to update/delete/insert files (no SQL is automatically generated any more).
   Example LobsterDescriptors can be found in the Sample folder
 - Files that are only on the database can no longer be viewed by Lobster. Instead every local file has its synchronisation checked via an SQL statement in the LobsterDescriptor.
 - Removed the ability to push file backups (they can still be viewed however)
 - Removed the list of extensions that restrict which files can be diffed 
 - Removed the ability to modify LobsterType (now LobsterDescriptor) files in the GUI. This rarely happened in the wild, and can be performed manually anyway
#### Improvements
 - Added a LobsterLite version that is run through via the command line and can be run on other operating systems via Mono
 - Synchronised files can now be deleted from the database using a new button in the file toolbar
 - Files can be excluded or included by using IncludeRules in the LobsterDescriptors (see the wiki)
 - Files are now more safely auto-updated, with a background running to update modified files using a short delay and a locking system to prevent file contention or multiple updates of the same file.
 - Modifying a LobsterDescriptor file should cause Lobster to automatically reload the descriptor and the files underneath it.
 - Connections are now tabbed, so you can have multiple connections open at once and you can close a connection without having to close the program.
 - Added the option to not write informational messages to the log file
 - Added the option to truncate the log file on program startup
 - Changed the "View log" menu button to open the log file in the users editor instead of opening a window for it.
 - User created file update success/failure sounds can now be any format playable by Windows Media Player
 - Added samples to the release folder
 
#### Bug FIxes
 - Fixed failure/success sounds not being customisable
 - Fixed a crash that would occur if the user supplied diff program arguments are incorrect
#### Appearance
- Changed auto-update notifications to appear from the bottom of the page instead of from the right.
- Removed rarely used windows such as the message log window and the Edit LobsterType window tree
- Connections are now tabbed as multiple connections can be opened at the same time. The buttons at the top of each tab are specific to that connection

## Release 2.6.1.0
##### Released: 24-10-2016

#### Bug Fixes
 - Fixed #105: Connections Remain Open after Changing Connection
 
## Release 2.6.0.0
##### Released: 12-10-2016

#### Breaking Changes
 - Changed the connection system to allow a single CodeSource folder to be connection to many different databases with a list in the new LobsterConfig.xml file located in the CodeSource directory.
#### Features
 - Improved the unhandled exception handler;
 - Improved the unhandled exception handlers exception handler;
 - Updated target .Net framework to 4.5;
 - Improved the adding of existing CodeSource directories, and the creating of new CodeSource directories;
 
## Release 2.5.0.0
##### Released: 03-07-2016

#### Features
 - Added a command line interface (auto-updates only);
 - Implemented Hot Reloading of Lobster Types (if a change is detected in the LobsterTypes directory);
  - Lobster Types can now also be reloaded through a menu item;
#### Bug Fixes
 - Fixed #97: Crash on ClobType Select;
 - Changed the auto updater to not wait on the new instance of Lobster to exit before exiting itself;
 
## Release 2.4.2.0
##### Released: 01-06-2016

#### Bug Fixes
 - Fixed #90: Selecting a database only file causes a crash;
 - Fixed #91: Incorrect date string in lobster.log;
 - Fixed #92: Crash on SVN Update
 - Fixed #92: Crash on file delete;

## Release 2.4.1.0
##### Released: 15-05-2016

#### Features
 - Added the option to delete backup files after a given number of days;
 - Changed the user settings window to let the user cancel changes;
 - Improved the file tree rebuild speed by ~30% (70ms => 50ms on a 2014 laptop);
 - Changed the file tree to only rebuild when necessary;
 - Improved event logging speed;
 - Reduced risk of exposing passwords via uninitialised memory attacks;
#### Bug Fixes
 - Fixed an issue where temporary files would not be deleted when a connection is closed;
 - Fixed the default argument for the Diff program not working for files with spaces in their names;
 - Fixed an issue where attempting to "Pull" a database only file would cause a crash;
 - Fixed an issue where the file backup list was not initialised when selecting a file;
 - Fixed a bug where all file events would have the same text;
#### Appearance
 - Changed the error message symbol;
 - Improved appearance of the message list;
 - Added a warning symbol to rows in the directory list if the XML file had parsing or validation errors;
 - Fixed issue #86: Incorrect date format for file backup list;

## Release 2.4.0.0
##### Released: 24-03-2016
#### Features
 - Added a new element to ClobTypes to prevent automatic file updates for files in that directory;
 - Changed the auto-updater to display the download progress in a popup window that also lets the user cancel the update;
 - Updated the readme
#### Bug Fixes
 - Fixed the "IncludeSubDirectories" flag not preventing file auto-updates or changing the file tree display;
 - Fixed a bug where Lobster could crash when attempting to dispose of the message log;
 - Fixed a possible crash if a ClobType file has no tables;
 - Fixed a bug that prevented success/failure sounds from playing;
 - Fixed a bug where newly inserted files would still appear green;
 - Fixed some issues where the connection window or password prompt window could appear behind other windows;
#### Appearance
 - Added tooltips for most actions in the connection list and main windows;
 - Improved the layout of the backup list;
 - Improved the layout and title of the password prompt window;
 - Changed the "Cancel Unsaved Changes" popup to have a fallback value if the database config does not have a name;

## Release 2.3.1.0
##### Released: 23-02-2016
#### Bug Fixes
 - Fixed a crash caused by inserting files with a mime types that has no prefix (such as a module):
 - Fixed a crash that would occur if the user attempts to save database configuration to a readonly file;
#### Appearance
 - Changed the unhandled exception message;

## Release 2.3.0.0
##### Released: 22-02-2016
#### Breaking Changes
 - Changed the configuration storage to place the DatabaseConfig and ClobTypes in the CodeSource directory;
 - Fixed casing in the Xml files;
#### Features
 - Added the ability to "Pull" files from the database and execute them immediately;
 - Changed the message log view to more efficiently update the UI when a new message is logged;
 - Changed the DatabaseConnection to only log file event messages when the user setting FileLogEvents is set;
#### Bug Fixes
 - Fixed a bug in the auto-updater where the wrong path would be used to restart the executable;
 - Fixed a bug where closing the program after an auto-update would instead crash;
#### Appearance
 - Improved the layout of the Message window;
 - Changed the appearance of the backup log;

## Release 2.2.0.0
##### Released: 15-02-2016
#### Features
 - Improved the file tree to remember which nodes were expanded when the tree is refreshed
 - Added a message list window to show the user what events have taken place
 - Changed the backup system to store files in a mirror of the CodeSource directory tree, with a directory for each file, and files with dates for names for each backup. The backups can now be used between sessions
 - Changed the connection list window to only prompt to cancel unsaved changes if changes were actually made
 - The log file is no longer locked on program startup
 - Changed the auto-updater to run on a separate thread
 - Changed the auto-updater to wait on process ID, instead of process name
 - Changed the logger to flush after every write
#### Bug Fixes
 - Fixed a bug whereby old connections were not properly closed after opening a new connection
 - Fixed the automatic updates setting not working
#### Appearance
 - Changed all windows to use the Lobster icon
