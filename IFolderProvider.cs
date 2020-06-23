using System;
using System.Collections.Generic;
using System.Text;

namespace YtoMp3
{
    public interface IFolderProvider
    {
        string MusicFolder { get; }
        string DownloadFolder { get; }

        IList<string> AvailableFolders { get; }
    }
}
