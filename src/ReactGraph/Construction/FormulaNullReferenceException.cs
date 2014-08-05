using System;
using System.Runtime.Serialization;
using ReactGraph.Properties;

namespace ReactGraph.Construction
{
    [Serializable]
    class FormulaNullReferenceException : NullReferenceException
    {
        [UsedImplicitly]
        public FormulaNullReferenceException(string expression) : 
            base(string.Format("Cannot evaluate expression '{0}', would cause a null reference exception", expression))
        {
        }

        protected FormulaNullReferenceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}