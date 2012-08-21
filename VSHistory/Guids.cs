// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.VSHistory
{
    static class GuidList
    {
        public const string guidVSHistoryPkgString = "c57236b3-77d8-497e-80f1-e298ea83bcc1";
        public const string guidVSHistoryCmdSetString = "85b0e391-5842-41be-b18f-c7580ccd8e66";

        public static readonly Guid guidVSHistoryCmdSet = new Guid(guidVSHistoryCmdSetString);
    };
}