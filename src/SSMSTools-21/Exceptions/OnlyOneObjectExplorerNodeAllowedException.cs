using System;

namespace SSMSTools_21.Exceptions
{
    internal class OnlyOneObjectExplorerNodeAllowedException : Exception
    {
        public OnlyOneObjectExplorerNodeAllowedException()
        {
        }

        public OnlyOneObjectExplorerNodeAllowedException(string message) : base(message)
        {
        }

        public OnlyOneObjectExplorerNodeAllowedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}