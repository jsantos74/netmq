using System.Text;

namespace NetMQ.Core.Mechanisms
{
    internal abstract class PlainMechanismBase: Mechanism
    {
        protected static readonly byte[] WelcomeLiteral = Encoding.ASCII.GetBytes("\x07WELCOME");
        protected static readonly byte[] ReadyLiteral = Encoding.ASCII.GetBytes("\x05READY");
        protected static readonly byte[] HelloLiteral = Encoding.ASCII.GetBytes("\x05HELLO");
        protected static readonly byte[] InitiateLiteral = Encoding.ASCII.GetBytes("\x08INITIATE");
        protected static readonly byte[] ErrorLiteral = Encoding.ASCII.GetBytes("\x05ERROR");
        private static readonly byte[] MessageLiteral = Encoding.ASCII.GetBytes("\x07MESSAGE");

        protected PlainMechanismBase(SessionBase session, Options options) : base(session, options) { }

        public override void Dispose() { }
    }
}

