namespace NetMQ.Core.Mechanisms
{
    internal class PlainServerMechanism: PlainMechanismBase
    {
        protected enum State
        {
            WaitingForHello,
            SendingWelcome,
            SendingReady,
            WaitingForInitiate,
            Ready
        }
        private State m_state;

        public PlainServerMechanism(SessionBase session, Options options) : base(session, options)
        {
            //    //  Note that there is no point to PLAIN if ZAP is not set up to handle the
            //    //  username and password, so if ZAP is not configured it is considered a
            //    //  failure.
            //    //  Given this is a backward-incompatible change, it's behind a socket
            //    //  option disabled by default.
            //    if (options.zap_enforce_domain)
            //        zmq_assert (zap_required ());
            m_state = State.WaitingForHello;
        }

        public override void Dispose() => base.Dispose();

        public override MechanismStatus Status
        {
            get
            {
                if(m_state == State.Ready)
                    return MechanismStatus.Ready;
                else
                    return MechanismStatus.Handshaking;
            }
        }

        public override PullMsgResult NextHandshakeCommand(ref Msg msg) 
        {
            PullMsgResult result;
            switch(m_state)
            {
                case State.SendingWelcome:
                    result = ProduceWelcome(ref msg);
                    if(result == PullMsgResult.Ok)
                        m_state = State.WaitingForInitiate;
                    break;
                case State.SendingReady:
                    result = ProduceReady(ref msg);
                    if(result == PullMsgResult.Ok)
                        m_state = State.Ready;
                    break;
                //case sending_error:
                //    produce_error (msg_);
                //    state = error_sent;
                //    break;
                default:
                    result = PullMsgResult.Empty;
                    break;
            }
            return result;
        }

        //int process_handshake_command (msg_t *msg_);
        //int zmq::plain_server_t::process_handshake_command (msg_t *msg_)
        //{
        //    int rc = 0;

        //    switch (state) {
        //        case waiting_for_hello:
        //            rc = process_hello (msg_);
        //            break;
        //        case waiting_for_initiate:
        //            rc = process_initiate (msg_);
        //            break;
        //        default:
        //            //  TODO see comment in curve_server_t::process_handshake_command
        //            session->get_socket ()->event_handshake_failed_protocol (
        //              session->get_endpoint (), ZMQ_PROTOCOL_ERROR_ZMTP_UNSPECIFIED);
        //            errno = EPROTO;
        //            rc = -1;
        //            break;
        //    }
        //    if (rc == 0) {
        //        rc = msg_->close ();
        //        errno_assert (rc == 0);
        //        rc = msg_->init ();
        //        errno_assert (rc == 0);
        //    }
        //    return rc;
        //}
        public override PushMsgResult ProcessHandshakeCommand(ref Msg msg) => PushMsgResult.Error;
        //{
        //    PushMsgResult result;

        //    switch(m_state)
        //    {
        //        case State.WaitingForHello:
        //            result = ProcessHello(ref msg);
        //            break;
        //        case State.WaitingForInitiate:
        //            result = ProcessInitiate(ref msg);
        //            break;
        //        default:
        //            return PushMsgResult.Error;
        //    }

        //    if(result == PushMsgResult.Ok)
        //    {
        //        msg.Close();
        //        msg.InitEmpty();
        //    }

        //    return result;
        //}

        //int process_hello (msg_t *msg_);
        //int zmq::plain_server_t::process_hello (msg_t *msg_)
        //{
        //    int rc = check_basic_command_structure (msg_);
        //    if (rc == -1)
        //        return -1;

        //    const char *ptr = static_cast<char *> (msg_->data ());
        //    size_t bytes_left = msg_->size ();

        //    if (bytes_left < hello_prefix_len
        //        || memcmp (ptr, hello_prefix, hello_prefix_len) != 0) {
        //        session->get_socket ()->event_handshake_failed_protocol (
        //          session->get_endpoint (), ZMQ_PROTOCOL_ERROR_ZMTP_UNEXPECTED_COMMAND);
        //        errno = EPROTO;
        //        return -1;
        //    }
        //    ptr += hello_prefix_len;
        //    bytes_left -= hello_prefix_len;

        //    if (bytes_left < 1) {
        //        //  PLAIN I: invalid PLAIN client, did not send username
        //        session->get_socket ()->event_handshake_failed_protocol (
        //          session->get_endpoint (),
        //          ZMQ_PROTOCOL_ERROR_ZMTP_MALFORMED_COMMAND_HELLO);
        //        errno = EPROTO;
        //        return -1;
        //    }
        //    const uint8_t username_length = *ptr++;
        //    bytes_left -= sizeof (username_length);

        //    if (bytes_left < username_length) {
        //        //  PLAIN I: invalid PLAIN client, sent malformed username
        //        session->get_socket ()->event_handshake_failed_protocol (
        //          session->get_endpoint (),
        //          ZMQ_PROTOCOL_ERROR_ZMTP_MALFORMED_COMMAND_HELLO);
        //        errno = EPROTO;
        //        return -1;
        //    }
        //    const std::string username = std::string (ptr, username_length);
        //    ptr += username_length;
        //    bytes_left -= username_length;
        //    if (bytes_left < 1) {
        //        //  PLAIN I: invalid PLAIN client, did not send password
        //        session->get_socket ()->event_handshake_failed_protocol (
        //          session->get_endpoint (),
        //          ZMQ_PROTOCOL_ERROR_ZMTP_MALFORMED_COMMAND_HELLO);
        //        errno = EPROTO;
        //        return -1;
        //    }

        //    const uint8_t password_length = *ptr++;
        //    bytes_left -= sizeof (password_length);
        //    if (bytes_left != password_length) {
        //        //  PLAIN I: invalid PLAIN client, sent malformed password or
        //        //  extraneous data
        //        session->get_socket ()->event_handshake_failed_protocol (
        //          session->get_endpoint (),
        //          ZMQ_PROTOCOL_ERROR_ZMTP_MALFORMED_COMMAND_HELLO);
        //        errno = EPROTO;
        //        return -1;
        //    }

        //    const std::string password = std::string (ptr, password_length);

        //    //  Use ZAP protocol (RFC 27) to authenticate the user.
        //    rc = session->zap_connect ();
        //    if (rc != 0) {
        //        session->get_socket ()->event_handshake_failed_no_detail (
        //          session->get_endpoint (), EFAULT);
        //        return -1;
        //    }

        //    send_zap_request (username, password);
        //    state = waiting_for_zap_reply;

        //    //  TODO actually, it is quite unlikely that we can read the ZAP
        //    //  reply already, but removing this has some strange side-effect
        //    //  (probably because the pipe's in_active flag is true until a read
        //    //  is attempted)
        //    return receive_and_process_zap_reply () == -1 ? -1 : 0;
        //}
        PushMsgResult ProcessHello(ref Msg msg) => PushMsgResult.Error;
        //{
        //    if(!CheckBasicCommandStructure(ref msg))
        //        return PushMsgResult.Error;

        //    Span<byte> hello = msg;

        //    if(!IsCommand("HELLO", ref msg))
        //        return PushMsgResult.Error;

        //    if(hello.Length != 200)
        //        return PushMsgResult.Error;

        //    byte major = hello[6];
        //    byte minor = hello[7];

        //    if(major != 1 || minor != 0)
        //    {
        //        // client HELLO has unknown version number
        //        return PushMsgResult.Error;
        //    }

        //    // Save client's short-term public key (C')
        //    hello.Slice(80, 32).CopyTo(m_cnClientKey);

        //    Span<byte> helloNonce = stackalloc byte[Curve25519XSalsa20Poly1305.NonceLength];
        //    HelloNoncePrefix.CopyTo(helloNonce);
        //    hello.Slice(112, 8).CopyTo(helloNonce.Slice(16));
        //    m_peerNonce = NetworkOrderBitsConverter.ToUInt64(hello, 112);

        //    using var box = new Curve25519XSalsa20Poly1305(m_secretKey, m_cnClientKey);

        //    Span<byte> helloPlaintext = stackalloc byte[80];
        //    bool isDecrypted = box.TryDecrypt(helloPlaintext, hello.Slice(120, 80), helloNonce);
        //    if(!isDecrypted)
        //        return PushMsgResult.Error;

        //    helloPlaintext.Clear();

        //    m_state = State.SendingWelcome;
        //    return PushMsgResult.Ok;
        //}

        //    static void produce_welcome (msg_t *msg_);
        //void zmq::plain_server_t::produce_welcome (msg_t *msg_)
        //{
        //    const int rc = msg_->init_size (welcome_prefix_len);
        //    errno_assert (rc == 0);
        //    memcpy (msg_->data (), welcome_prefix, welcome_prefix_len);
        //}
        PullMsgResult ProduceWelcome(ref Msg msg) => PullMsgResult.Error;
        //        {
        //            Span<byte> cookieNonce = stackalloc byte[Curve25519XSalsa20Poly1305.NonceLength];
        //            Span<byte> cookiePlaintext = stackalloc byte[64];
        //            Span<byte> cookieCiphertext = stackalloc byte[64 + XSalsa20Poly1305.TagLength];

        //            //  Create full nonce for encryption
        //            //  8-byte prefix plus 16-byte random nonce
        //            CookieNoncePrefix.CopyTo(cookieNonce);
        //            using var rng = RandomNumberGenerator.Create();
        //#if NETSTANDARD2_1
        //            rng.GetBytes(cookieNonce.Slice(8));
        //#else
        //            byte[] temp = new byte[16];
        //            rng.GetBytes(temp);
        //            temp.CopyTo(cookieNonce.Slice(8));
        //#endif

        //            // Generate cookie = Box [C' + s'](t)
        //            m_cnClientKey.CopyTo(cookiePlaintext);
        //            m_cnSecretKey.CopyTo(cookiePlaintext.Slice(32));

        //            // Generate fresh cookie key
        //            rng.GetBytes(m_cookieKey);

        //            // Encrypt using symmetric cookie key
        //            using var secretBox = new XSalsa20Poly1305(m_cookieKey);
        //            secretBox.Encrypt(cookieCiphertext, cookiePlaintext, cookieNonce);
        //            cookiePlaintext.Clear();

        //            Span<byte> welcomeNonce = stackalloc byte[Curve25519XSalsa20Poly1305.NonceLength];
        //            Span<byte> welcomePlaintext = stackalloc byte[128];
        //            Span<byte> welcomeCiphertext = stackalloc byte[128 + Curve25519XSalsa20Poly1305.TagLength];

        //            //  Create full nonce for encryption
        //            //  8-byte prefix plus 16-byte random nonce
        //            WelcomeNoncePrefix.CopyTo(welcomeNonce);
        //#if NETSTANDARD2_1
        //            rng.GetBytes(welcomeNonce.Slice(8));
        //#else
        //            rng.GetBytes(temp);
        //            temp.CopyTo(welcomeNonce.Slice(8));
        //#endif

        //            // Create 144-byte Box [S' + cookie](S->C')
        //            m_cnPublicKey.CopyTo(welcomePlaintext);
        //            cookieNonce.Slice(8).CopyTo(welcomePlaintext.Slice(32));
        //            cookieCiphertext.CopyTo(welcomePlaintext.Slice(48));
        //            using var box = new Curve25519XSalsa20Poly1305(m_secretKey, m_cnClientKey);
        //            box.Encrypt(welcomeCiphertext, welcomePlaintext, welcomeNonce);
        //            welcomePlaintext.Clear();

        //            msg.InitPool(168); // TODO: we can save some allocation here by allocating this earlier
        //            Span<byte> welcome = msg;
        //            WelcomeLiteral.CopyTo(welcome);
        //            welcomeNonce.Slice(8, 16).CopyTo(welcome.Slice(8));
        //            welcomeCiphertext.CopyTo(welcome.Slice(24));

        //            return PullMsgResult.Ok;
        //        }

        //    int process_initiate (msg_t *msg_);
        //int zmq::plain_server_t::process_initiate (msg_t *msg_)
        //{
        //    const unsigned char *ptr = static_cast<unsigned char *> (msg_->data ());
        //    const size_t bytes_left = msg_->size ();

        //    if (bytes_left < initiate_prefix_len
        //        || memcmp (ptr, initiate_prefix, initiate_prefix_len) != 0) {
        //        session->get_socket ()->event_handshake_failed_protocol (
        //          session->get_endpoint (), ZMQ_PROTOCOL_ERROR_ZMTP_UNEXPECTED_COMMAND);
        //        errno = EPROTO;
        //        return -1;
        //    }
        //    const int rc = parse_metadata (ptr + initiate_prefix_len,
        //                                   bytes_left - initiate_prefix_len);
        //    if (rc == 0)
        //        state = sending_ready;
        //    return rc;
        //}
        PushMsgResult ProcessInitiate(ref Msg msg) => PushMsgResult.Error;
        //{
        //    if(!CheckBasicCommandStructure(ref msg))
        //        return PushMsgResult.Error;

        //    Span<byte> initiate = msg;

        //    if(!IsCommand("INITIATE", ref msg))
        //        return PushMsgResult.Error;

        //    if(initiate.Length < 257)
        //        return PushMsgResult.Error;

        //    Span<byte> cookieNonce = stackalloc byte[Curve25519XSalsa20Poly1305.NonceLength];
        //    Span<byte> cookiePlaintext = stackalloc byte[64];
        //    Span<byte> cookieBox = initiate.Slice(25, 80);

        //    CookieNoncePrefix.CopyTo(cookieNonce);
        //    initiate.Slice(9, 16).CopyTo(cookieNonce.Slice(8));

        //    using var secretBox = new XSalsa20Poly1305(m_cookieKey);
        //    bool decrypted = secretBox.TryDecrypt(cookiePlaintext, cookieBox, cookieNonce);

        //    if(!decrypted)
        //        return PushMsgResult.Error;

        //    //  Check cookie plain text is as expected [C' + s']
        //    if(!SpanUtility.Equals(m_cnClientKey, cookiePlaintext.Slice(0, 32)) ||
        //        !SpanUtility.Equals(m_cnSecretKey, cookiePlaintext.Slice(32, 32)))
        //        return PushMsgResult.Error;

        //    Span<byte> initiateNonce = stackalloc byte[Curve25519XSalsa20Poly1305.NonceLength];
        //    byte[] initiatePlaintext = new byte[msg.Size - 113];
        //    var initiateBox = initiate.Slice(113);

        //    InitiatieNoncePrefix.CopyTo(initiateNonce);
        //    initiate.Slice(105, 8).CopyTo(initiateNonce.Slice(16));
        //    m_peerNonce = NetworkOrderBitsConverter.ToUInt64(initiate, 105);

        //    using var box = new Curve25519XSalsa20Poly1305(m_cnSecretKey, m_cnClientKey);
        //    bool decrypt = box.TryDecrypt(initiatePlaintext, initiateBox, initiateNonce);
        //    if(!decrypt)
        //        return PushMsgResult.Error;

        //    Span<byte> vouchNonce = stackalloc byte[Curve25519XSalsa20Poly1305.NonceLength];
        //    Span<byte> vouchPlaintext = stackalloc byte[64];
        //    Span<byte> vouchBox = new Span<byte>(initiatePlaintext, 48, 80);
        //    var clientKey = new Span<byte>(initiatePlaintext, 0, 32);

        //    VouchNoncePrefix.CopyTo(vouchNonce);
        //    new Span<byte>(initiatePlaintext, 32, 16).CopyTo(vouchNonce.Slice(8));

        //    using var box2 = new Curve25519XSalsa20Poly1305(m_cnSecretKey, clientKey);
        //    decrypt = box2.TryDecrypt(vouchPlaintext, vouchBox, vouchNonce);
        //    if(!decrypt)
        //        return PushMsgResult.Error;

        //    //  What we decrypted must be the client's short-term public key
        //    if(!SpanUtility.Equals(vouchPlaintext.Slice(0, 32), m_cnClientKey))
        //        return PushMsgResult.Error;

        //    //  Create the session box
        //    m_box = new Curve25519XSalsa20Poly1305(m_cnSecretKey, m_cnClientKey);

        //    //  This supports the Stonehouse pattern (encryption without authentication).
        //    m_state = State.SendingReady;

        //    if(!ParseMetadata(new Span<byte>(initiatePlaintext, 128, initiatePlaintext.Length - 128 - 16)))
        //        return PushMsgResult.Error;

        //    vouchPlaintext.Clear();
        //    Array.Clear(initiatePlaintext, 0, initiatePlaintext.Length);

        //    return PushMsgResult.Ok;
        //}

        //    void produce_ready (msg_t *msg_) const;
        //void zmq::plain_server_t::produce_ready (msg_t *msg_) const
        //{
        //    make_command_with_basic_properties (msg_, ready_prefix, ready_prefix_len);
        //}
        PullMsgResult ProduceReady(ref Msg msg) => PullMsgResult.Error;
        //{
        //    int metadataLength = BasicPropertiesLength;
        //    Span<byte> readyNonce = stackalloc byte[Curve25519XSalsa20Poly1305.NonceLength];
        //    byte[] readyPlaintext = new byte[metadataLength];

        //    //  Create Box [metadata](S'->C')
        //    AddBasicProperties(readyPlaintext);

        //    ReadyNoncePrefix.CopyTo(readyNonce);
        //    NetworkOrderBitsConverter.PutUInt64(m_nonce, readyNonce.Slice(16));

        //    msg.InitPool(14 + Curve25519XSalsa20Poly1305.TagLength + metadataLength);
        //    var readyBox = msg.Slice(14);

        //    Assumes.NotNull(m_box);

        //    m_box.Encrypt(readyBox, readyPlaintext, readyNonce);
        //    Array.Clear(readyPlaintext, 0, readyPlaintext.Length);

        //    Span<byte> ready = msg;
        //    ReadyLiteral.CopyTo(ready);

        //    //  Short nonce, prefixed by "CurveZMQREADY---"
        //    readyNonce.Slice(16).CopyTo(ready.Slice(6));

        //    m_nonce++;

        //    return 0;
        //}












        //    void produce_error (msg_t *msg_) const;
        //void zmq::plain_server_t::produce_error (msg_t *msg_) const
        //{
        //    const char expected_status_code_len = 3;
        //    zmq_assert (status_code.length ()
        //                == static_cast<size_t> (expected_status_code_len));
        //    const size_t status_code_len_size = sizeof (expected_status_code_len);
        //    const int rc = msg_->init_size (error_prefix_len + status_code_len_size
        //                                    + expected_status_code_len);
        //    zmq_assert (rc == 0);
        //    char *msg_data = static_cast<char *> (msg_->data ());
        //    memcpy (msg_data, error_prefix, error_prefix_len);
        //    msg_data[error_prefix_len] = expected_status_code_len;
        //    memcpy (msg_data + error_prefix_len + status_code_len_size,
        //            status_code.c_str (), status_code.length ());
        //}

        //    void send_zap_request (const std::string &username_,
        //                           const std::string &password_);
        //void zmq::plain_server_t::send_zap_request (const std::string &username_,
        //                                            const std::string &password_)
        //{
        //    const uint8_t *credentials[] = {
        //      reinterpret_cast<const uint8_t *> (username_.c_str ()),
        //      reinterpret_cast<const uint8_t *> (password_.c_str ())};
        //    size_t credentials_sizes[] = {username_.size (), password_.size ()};
        //    const char plain_mechanism_name[] = "PLAIN";
        //    zap_client_t::send_zap_request (
        //      plain_mechanism_name, sizeof (plain_mechanism_name) - 1, credentials,
        //      credentials_sizes, sizeof (credentials) / sizeof (credentials[0]));
    }
}
