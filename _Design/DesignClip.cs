using ClipBoard.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipBoard._Design
{
    public class DesignClip : Clip
    {
        public DesignClip() : base() {
            Id = null;
            ClipGroupId = 0;
            AppId = "";
            AppName = "";
            Name = "New Clip";
            Description = "fdghdfghdfghdfghdfgh";
            Value = "test value";
            JsonValue = "";
            ContentType = "";
            CopyHotKey = "";
            PasteHotKey = "";
            SortOrder = 0;
            Timestamp = DateTime.UtcNow;
        }
    }
}
