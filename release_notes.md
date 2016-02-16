 
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