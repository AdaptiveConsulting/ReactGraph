using System;
using System.Runtime.Serialization;

namespace ReactGraph.Graph
{
    [Serializable]
    public class CycleDetectedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public CycleDetectedException()
        {
        }

        public CycleDetectedException(string message) : base(message)
        {
        }

        public CycleDetectedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CycleDetectedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}