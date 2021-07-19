using System;
using System.Diagnostics;
using System.Text;

namespace NetMQ.Core.Mechanisms
{
    internal class PlainClientMechanism: PlainMechanismBase
    {
        private const string WelcomeCommand = "WELCOME";
        private const string ReadyCommand = "READY";
        private const string ErrorCommand = "ERROR";

        private enum State
        {
            SendHello,      //sending_hello
            ExpectWelcome,  //waiting_for_welcome
            SendInitiate,   //sending_initiate
            ExpectReady,    //waiting_for_ready
            ErrorReceived,  //error_command_received
            Connected       //ready
        };

        private State m_state;

        public PlainClientMechanism(SessionBase session, Options options) : base(session, options) => m_state = State.SendHello;

        public override void Dispose() => base.Dispose();

        public override MechanismStatus Status
        {
            get
            {
                if(m_state == State.Connected)
                    return MechanismStatus.Ready;
                if(m_state == State.ErrorReceived)
                    return MechanismStatus.Error;

                return MechanismStatus.Handshaking;
            }
        }

        // mechanism implementation
        public override PullMsgResult NextHandshakeCommand(ref Msg msg)
        {
            PullMsgResult result = PullMsgResult.Ok;
            switch(m_state)
            {
                case State.SendHello:
                    result = ProduceHello(ref msg);
                    if(result == PullMsgResult.Ok)
                        m_state = State.ExpectWelcome;
                    break;
                case State.SendInitiate:
                    result = ProduceInitiate(ref msg);
                    if(result == PullMsgResult.Ok)
                        m_state = State.ExpectReady;
                    break;
                default:
                    result = PullMsgResult.Empty;
                    break;
            }
            return result;
        }

        public override PushMsgResult ProcessHandshakeCommand(ref Msg msg)
        {
            PushMsgResult result = PushMsgResult.Ok;

            if(IsCommand(WelcomeCommand, ref msg))
                result = ProcessWelcome(ref msg);
            else if(IsCommand(ReadyCommand, ref msg))
                result = ProcessReady(ref msg);
            else if(IsCommand(ErrorCommand, ref msg))
                result = ProcessError(ref msg);
            else
                result = PushMsgResult.Error;
            if(result == PushMsgResult.Ok)
            {
                msg.Close();
                msg.InitEmpty();
            }
            return result;
        }

        PullMsgResult ProduceHello(ref Msg msg)
        {
           // Prepare the username
            byte[] username = Encoding.ASCII.GetBytes(Options.PlainUsername ?? string.Empty);
            Debug.Assert(username.Length <= byte.MaxValue, $"PlainUsername to long. Max username size is {byte.MaxValue} chars.");

            // Prepare the password
            byte[] password = Encoding.ASCII.GetBytes(Options.PlainPassword ?? string.Empty);
            Debug.Assert(password.Length <= byte.MaxValue, $"PlainPassword to long. Max password size is {byte.MaxValue} chars.");

            msg.InitPool(HelloLiteral.Length + 1 // username size
                + username.Length + 1// password size 
                + password.Length);

            Span<byte> hello = msg;
            HelloLiteral.CopyTo(hello);
            int idx = HelloLiteral.Length;
            hello[idx++] = (byte)username.Length;
            username.CopyTo(hello.Slice(idx));
            idx += username.Length;
            hello[idx++] = (byte)password.Length;
            password.CopyTo(hello.Slice(idx));

            return PullMsgResult.Ok;
        }

        PushMsgResult ProcessWelcome(ref Msg msg)
        {
            //LIBZMQ_UNUSED(cmd_data_);

            //if(_state != waiting_for_welcome)
            //{
            //    session->get_socket()->event_handshake_failed_protocol(
            //      session->get_endpoint(), ZMQ_PROTOCOL_ERROR_ZMTP_UNEXPECTED_COMMAND);
            //    errno = EPROTO;
            //    return -1;
            //}
            //if(data_size_ != welcome_prefix_len)
            //{
            //    session->get_socket()->event_handshake_failed_protocol(
            //      session->get_endpoint(),
            //      ZMQ_PROTOCOL_ERROR_ZMTP_MALFORMED_COMMAND_WELCOME);
            //    errno = EPROTO;
            //    return -1;
            //}
            //_state = sending_initiate;
            //return 0;
            return PushMsgResult.Ok;
        }

        //void produce_initiate(msg_t* msg_) const;
        //void zmq::plain_client_t::produce_initiate(msg_t* msg_) const
        PullMsgResult ProduceInitiate(ref Msg msg)
        {
            //make_command_with_basic_properties (msg_, initiate_prefix,
            //                           initiate_prefix_len);
            return PullMsgResult.Ok;
        }

        //int process_ready(const unsigned char* cmd_data_, size_t data_size_);
        //int zmq::plain_client_t::process_ready(const unsigned char* cmd_data_, size_t data_size_)
        PushMsgResult ProcessReady(ref Msg msg)
        {
            //if(_state != waiting_for_ready)
            //{
            //    session->get_socket()->event_handshake_failed_protocol(
            //      session->get_endpoint(), ZMQ_PROTOCOL_ERROR_ZMTP_UNEXPECTED_COMMAND);
            //    errno = EPROTO;
            //    return -1;
            //}
            //const int rc = parse_metadata(cmd_data_ + ready_prefix_len,
            //                               data_size_ - ready_prefix_len);
            //if(rc == 0)
            //    _state = ready;
            //else
            //    session->get_socket()->event_handshake_failed_protocol(
            //      session->get_endpoint(), ZMQ_PROTOCOL_ERROR_ZMTP_INVALID_METADATA);

            //return rc;
            return PushMsgResult.Ok;
        }

        //int process_error(const unsigned char* cmd_data_, size_t data_size_);
        //int zmq::plain_client_t::process_error(const unsigned char* cmd_data_, size_t data_size_)
        PushMsgResult ProcessError(ref Msg msg)
        {
            //if(_state != waiting_for_welcome && _state != waiting_for_ready)
            //{
            //    session->get_socket()->event_handshake_failed_protocol(
            //      session->get_endpoint(), ZMQ_PROTOCOL_ERROR_ZMTP_UNEXPECTED_COMMAND);
            //    errno = EPROTO;
            //    return -1;
            //}
            //const size_t start_of_error_reason = error_prefix_len + brief_len_size;
            //if(data_size_ < start_of_error_reason)
            //{
            //    session->get_socket()->event_handshake_failed_protocol(
            //      session->get_endpoint(),
            //      ZMQ_PROTOCOL_ERROR_ZMTP_MALFORMED_COMMAND_ERROR);
            //    errno = EPROTO;
            //    return -1;
            //}
            //const size_t error_reason_len =
            //  static_cast<size_t>(cmd_data_[error_prefix_len]);
            //if(error_reason_len > data_size_ - start_of_error_reason)
            //{
            //    session->get_socket()->event_handshake_failed_protocol(
            //      session->get_endpoint(),
            //      ZMQ_PROTOCOL_ERROR_ZMTP_MALFORMED_COMMAND_ERROR);
            //    errno = EPROTO;
            //    return -1;
            //}
            //const char* error_reason =
            //  reinterpret_cast <const char*> (cmd_data_) + start_of_error_reason;
            //handle_error_reason(error_reason, error_reason_len);
            //_state = error_command_received;
            //return 0;
            return PushMsgResult.Ok;
        }
    }
}