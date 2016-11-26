using System;
using System.Collections.Generic;
using LobsterModel;

namespace LobsterWpf.ViewModels
{
    public class WatchedDirectoryView : WatchedNodeView
    {
        public WatchedDirectoryView(WatchedDirectory baseDirectory) : base(baseDirectory)
        {
            this.BaseDirectory = baseDirectory;

            this.ChildNodes = new List<WatchedNodeView>();

            foreach (WatchedNode node in this.BaseDirectory.ChildNodes)
            {
                if (node is WatchedDirectory)
                {
                    this.ChildNodes.Add(new WatchedDirectoryView((WatchedDirectory)node));
                }
                else
                {
                    this.ChildNodes.Add(new WatchedFileView((WatchedFile)node));
                }
            }
        }

        WatchedDirectory BaseDirectory { get; }

        public List<WatchedNodeView> ChildNodes { get; }


        public override void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView)
        {
            foreach (WatchedNodeView child in this.ChildNodes)
            {
                child.CheckFileSynchronisation(connectionView, watcherView);
            }
        }
    }
}
