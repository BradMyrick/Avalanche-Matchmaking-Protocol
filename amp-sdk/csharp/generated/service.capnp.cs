using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc24b6cc6fc418df9UL), Proxy(typeof(GameSessionService_Proxy)), Skeleton(typeof(GameSessionService_Skeleton))]
    public interface IGameSessionService : IDisposable
    {
        Task<CapnpGen.IUserSession> Login(ulong gameId, IReadOnlyList<byte> signedChallenge, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc24b6cc6fc418df9UL)]
    public class GameSessionService_Proxy : Proxy, IGameSessionService
    {
        public Task<CapnpGen.IUserSession> Login(ulong gameId, IReadOnlyList<byte> signedChallenge, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameSessionService.Params_Login.WRITER>();
            var arg_ = new CapnpGen.GameSessionService.Params_Login()
            {GameId = gameId, SignedChallenge = signedChallenge};
            arg_?.serialize(in_);
            return Impatient.MakePipelineAware(Call(14000403468502797817UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_), d_ =>
            {
                using (d_)
                {
                    var r_ = CapnpSerializable.Create<CapnpGen.GameSessionService.Result_Login>(d_);
                    return (r_.Session);
                }
            }

            );
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc24b6cc6fc418df9UL)]
    public class GameSessionService_Skeleton : Skeleton<IGameSessionService>
    {
        public GameSessionService_Skeleton()
        {
            SetMethodTable(Login);
        }

        public override ulong InterfaceId => 14000403468502797817UL;
        Task<AnswerOrCounterquestion> Login(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameSessionService.Params_Login>(d_);
                return Impatient.MaybeTailCall(Impl.Login(in_.GameId, in_.SignedChallenge, cancellationToken_), session =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameSessionService.Result_Login.WRITER>();
                    var r_ = new CapnpGen.GameSessionService.Result_Login{Session = session};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class GameSessionService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd9fbd819c7b90c04UL)]
        public class Params_Login : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd9fbd819c7b90c04UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameId = reader.GameId;
                SignedChallenge = reader.SignedChallenge;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.GameId = GameId;
                writer.SignedChallenge.Init(SignedChallenge);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public ulong GameId
            {
                get;
                set;
            }

            public IReadOnlyList<byte> SignedChallenge
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public ulong GameId => ctx.ReadDataULong(0UL, 0UL);
                public IReadOnlyList<byte> SignedChallenge => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 1);
                }

                public ulong GameId
                {
                    get => this.ReadDataULong(0UL, 0UL);
                    set => this.WriteData(0UL, value, 0UL);
                }

                public ListOfPrimitivesSerializer<byte> SignedChallenge
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x93ed8eadf6d2f611UL)]
        public class Result_Login : ICapnpSerializable
        {
            public const UInt64 typeId = 0x93ed8eadf6d2f611UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Session = reader.Session;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Session = Session;
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.IUserSession Session
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.IUserSession Session => ctx.ReadCap<CapnpGen.IUserSession>(0);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.IUserSession Session
                {
                    get => ReadCap<CapnpGen.IUserSession>(0);
                    set => LinkObject(0, value);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xca30dd1a61881e14UL), Proxy(typeof(UserSession_Proxy)), Skeleton(typeof(UserSession_Skeleton))]
    public interface IUserSession : IDisposable
    {
        Task<(CapnpGen.MatchAssignment, CapnpGen.IMatchSession)> RequestMatch(CapnpGen.GameMatchRequest req, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.IMatchSession> Reconnect(IReadOnlyList<byte> matchId, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xca30dd1a61881e14UL)]
    public class UserSession_Proxy : Proxy, IUserSession
    {
        public Task<(CapnpGen.MatchAssignment, CapnpGen.IMatchSession)> RequestMatch(CapnpGen.GameMatchRequest req, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.UserSession.Params_RequestMatch.WRITER>();
            var arg_ = new CapnpGen.UserSession.Params_RequestMatch()
            {Req = req};
            arg_?.serialize(in_);
            return Impatient.MakePipelineAware(Call(14569387899918753300UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_), d_ =>
            {
                using (d_)
                {
                    var r_ = CapnpSerializable.Create<CapnpGen.UserSession.Result_RequestMatch>(d_);
                    return (r_.Assignment, r_.Session);
                }
            }

            );
        }

        public Task<CapnpGen.IMatchSession> Reconnect(IReadOnlyList<byte> matchId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.UserSession.Params_Reconnect.WRITER>();
            var arg_ = new CapnpGen.UserSession.Params_Reconnect()
            {MatchId = matchId};
            arg_?.serialize(in_);
            return Impatient.MakePipelineAware(Call(14569387899918753300UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_), d_ =>
            {
                using (d_)
                {
                    var r_ = CapnpSerializable.Create<CapnpGen.UserSession.Result_Reconnect>(d_);
                    return (r_.Session);
                }
            }

            );
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xca30dd1a61881e14UL)]
    public class UserSession_Skeleton : Skeleton<IUserSession>
    {
        public UserSession_Skeleton()
        {
            SetMethodTable(RequestMatch, Reconnect);
        }

        public override ulong InterfaceId => 14569387899918753300UL;
        Task<AnswerOrCounterquestion> RequestMatch(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.UserSession.Params_RequestMatch>(d_);
                return Impatient.MaybeTailCall(Impl.RequestMatch(in_.Req, cancellationToken_), (assignment, session) =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.UserSession.Result_RequestMatch.WRITER>();
                    var r_ = new CapnpGen.UserSession.Result_RequestMatch{Assignment = assignment, Session = session};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> Reconnect(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.UserSession.Params_Reconnect>(d_);
                return Impatient.MaybeTailCall(Impl.Reconnect(in_.MatchId, cancellationToken_), session =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.UserSession.Result_Reconnect.WRITER>();
                    var r_ = new CapnpGen.UserSession.Result_Reconnect{Session = session};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class UserSession
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd7d7354203ff3ee7UL)]
        public class Params_RequestMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd7d7354203ff3ee7UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Req = CapnpSerializable.Create<CapnpGen.GameMatchRequest>(reader.Req);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Req?.serialize(writer.Req);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.GameMatchRequest Req
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.GameMatchRequest.READER Req => ctx.ReadStruct(0, CapnpGen.GameMatchRequest.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.GameMatchRequest.WRITER Req
                {
                    get => BuildPointer<CapnpGen.GameMatchRequest.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa5c563f5e47f269bUL)]
        public class Result_RequestMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa5c563f5e47f269bUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Assignment = CapnpSerializable.Create<CapnpGen.MatchAssignment>(reader.Assignment);
                Session = reader.Session;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Assignment?.serialize(writer.Assignment);
                writer.Session = Session;
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.MatchAssignment Assignment
            {
                get;
                set;
            }

            public CapnpGen.IMatchSession Session
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.MatchAssignment.READER Assignment => ctx.ReadStruct(0, CapnpGen.MatchAssignment.READER.create);
                public CapnpGen.IMatchSession Session => ctx.ReadCap<CapnpGen.IMatchSession>(1);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.MatchAssignment.WRITER Assignment
                {
                    get => BuildPointer<CapnpGen.MatchAssignment.WRITER>(0);
                    set => Link(0, value);
                }

                public CapnpGen.IMatchSession Session
                {
                    get => ReadCap<CapnpGen.IMatchSession>(1);
                    set => LinkObject(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf31a3b0e2119f63eUL)]
        public class Params_Reconnect : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf31a3b0e2119f63eUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                MatchId = reader.MatchId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.MatchId.Init(MatchId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> MatchId
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public IReadOnlyList<byte> MatchId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> MatchId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbfd642fe1003f5b0UL)]
        public class Result_Reconnect : ICapnpSerializable
        {
            public const UInt64 typeId = 0xbfd642fe1003f5b0UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Session = reader.Session;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Session = Session;
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.IMatchSession Session
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.IMatchSession Session => ctx.ReadCap<CapnpGen.IMatchSession>(0);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.IMatchSession Session
                {
                    get => ReadCap<CapnpGen.IMatchSession>(0);
                    set => LinkObject(0, value);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8e8338034be7fdf9UL), Proxy(typeof(MatchSession_Proxy)), Skeleton(typeof(MatchSession_Skeleton))]
    public interface IMatchSession : IDisposable
    {
        Task<IReadOnlyList<byte>> SubmitOutcome(CapnpGen.OutcomeSubmission submission, CancellationToken cancellationToken_ = default);
        Task SubscribeToEvents(CapnpGen.IMatchListener subscriber, CancellationToken cancellationToken_ = default);
        Task EmitGameEvent(CapnpGen.GameEvent @event, CancellationToken cancellationToken_ = default);
        Task EmitTelemetry(CapnpGen.AmpTelemetryEvent @event, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8e8338034be7fdf9UL)]
    public class MatchSession_Proxy : Proxy, IMatchSession
    {
        public async Task<IReadOnlyList<byte>> SubmitOutcome(CapnpGen.OutcomeSubmission submission, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Params_SubmitOutcome.WRITER>();
            var arg_ = new CapnpGen.MatchSession.Params_SubmitOutcome()
            {Submission = submission};
            arg_?.serialize(in_);
            using (var d_ = await Call(10269113162144415225UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchSession.Result_SubmitOutcome>(d_);
                return (r_.Signature);
            }
        }

        public async Task SubscribeToEvents(CapnpGen.IMatchListener subscriber, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Params_SubscribeToEvents.WRITER>();
            var arg_ = new CapnpGen.MatchSession.Params_SubscribeToEvents()
            {Subscriber = subscriber};
            arg_?.serialize(in_);
            using (var d_ = await Call(10269113162144415225UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchSession.Result_SubscribeToEvents>(d_);
                return;
            }
        }

        public async Task EmitGameEvent(CapnpGen.GameEvent @event, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Params_EmitGameEvent.WRITER>();
            var arg_ = new CapnpGen.MatchSession.Params_EmitGameEvent()
            {Event = @event};
            arg_?.serialize(in_);
            using (var d_ = await Call(10269113162144415225UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchSession.Result_EmitGameEvent>(d_);
                return;
            }
        }

        public async Task EmitTelemetry(CapnpGen.AmpTelemetryEvent @event, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Params_EmitTelemetry.WRITER>();
            var arg_ = new CapnpGen.MatchSession.Params_EmitTelemetry()
            {Event = @event};
            arg_?.serialize(in_);
            using (var d_ = await Call(10269113162144415225UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchSession.Result_EmitTelemetry>(d_);
                return;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8e8338034be7fdf9UL)]
    public class MatchSession_Skeleton : Skeleton<IMatchSession>
    {
        public MatchSession_Skeleton()
        {
            SetMethodTable(SubmitOutcome, SubscribeToEvents, EmitGameEvent, EmitTelemetry);
        }

        public override ulong InterfaceId => 10269113162144415225UL;
        Task<AnswerOrCounterquestion> SubmitOutcome(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchSession.Params_SubmitOutcome>(d_);
                return Impatient.MaybeTailCall(Impl.SubmitOutcome(in_.Submission, cancellationToken_), signature =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Result_SubmitOutcome.WRITER>();
                    var r_ = new CapnpGen.MatchSession.Result_SubmitOutcome{Signature = signature};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> SubscribeToEvents(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchSession.Params_SubscribeToEvents>(d_);
                await Impl.SubscribeToEvents(in_.Subscriber, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Result_SubscribeToEvents.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> EmitGameEvent(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchSession.Params_EmitGameEvent>(d_);
                await Impl.EmitGameEvent(in_.Event, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Result_EmitGameEvent.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> EmitTelemetry(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchSession.Params_EmitTelemetry>(d_);
                await Impl.EmitTelemetry(in_.Event, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Result_EmitTelemetry.WRITER>();
                return s_;
            }
        }
    }

    public static class MatchSession
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe08c0dc1c2a0d0b0UL)]
        public class Params_SubmitOutcome : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe08c0dc1c2a0d0b0UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Submission = CapnpSerializable.Create<CapnpGen.OutcomeSubmission>(reader.Submission);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Submission?.serialize(writer.Submission);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.OutcomeSubmission Submission
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.OutcomeSubmission.READER Submission => ctx.ReadStruct(0, CapnpGen.OutcomeSubmission.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.OutcomeSubmission.WRITER Submission
                {
                    get => BuildPointer<CapnpGen.OutcomeSubmission.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb943b56283014590UL)]
        public class Result_SubmitOutcome : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb943b56283014590UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> Signature
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public IReadOnlyList<byte> Signature => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8a9f2489106723d3UL)]
        public class Params_SubscribeToEvents : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8a9f2489106723d3UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Subscriber = reader.Subscriber;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Subscriber = Subscriber;
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.IMatchListener Subscriber
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.IMatchListener Subscriber => ctx.ReadCap<CapnpGen.IMatchListener>(0);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.IMatchListener Subscriber
                {
                    get => ReadCap<CapnpGen.IMatchListener>(0);
                    set => LinkObject(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd451a3ec3e118996UL)]
        public class Result_SubscribeToEvents : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd451a3ec3e118996UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 0);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb43f482e9e151e23UL)]
        public class Params_EmitGameEvent : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb43f482e9e151e23UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Event = CapnpSerializable.Create<CapnpGen.GameEvent>(reader.Event);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Event?.serialize(writer.Event);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.GameEvent Event
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.GameEvent.READER Event => ctx.ReadStruct(0, CapnpGen.GameEvent.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.GameEvent.WRITER Event
                {
                    get => BuildPointer<CapnpGen.GameEvent.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe2edfbdc8b510e93UL)]
        public class Result_EmitGameEvent : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe2edfbdc8b510e93UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 0);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x90ef98fce5494410UL)]
        public class Params_EmitTelemetry : ICapnpSerializable
        {
            public const UInt64 typeId = 0x90ef98fce5494410UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Event = CapnpSerializable.Create<CapnpGen.AmpTelemetryEvent>(reader.Event);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Event?.serialize(writer.Event);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.AmpTelemetryEvent Event
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.AmpTelemetryEvent.READER Event => ctx.ReadStruct(0, CapnpGen.AmpTelemetryEvent.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.AmpTelemetryEvent.WRITER Event
                {
                    get => BuildPointer<CapnpGen.AmpTelemetryEvent.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x80e333dcd7ea06c7UL)]
        public class Result_EmitTelemetry : ICapnpSerializable
        {
            public const UInt64 typeId = 0x80e333dcd7ea06c7UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 0);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfc81f34e1dd8cdccUL), Proxy(typeof(MatchListener_Proxy)), Skeleton(typeof(MatchListener_Skeleton))]
    public interface IMatchListener : IDisposable
    {
        Task OnMatchSettled(CapnpGen.Outcome outcome, CancellationToken cancellationToken_ = default);
        Task OnOpponentDisconnected(CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfc81f34e1dd8cdccUL)]
    public class MatchListener_Proxy : Proxy, IMatchListener
    {
        public async Task OnMatchSettled(CapnpGen.Outcome outcome, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchListener.Params_OnMatchSettled.WRITER>();
            var arg_ = new CapnpGen.MatchListener.Params_OnMatchSettled()
            {Outcome = outcome};
            arg_?.serialize(in_);
            using (var d_ = await Call(18195091486387260876UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchListener.Result_OnMatchSettled>(d_);
                return;
            }
        }

        public async Task OnOpponentDisconnected(CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchListener.Params_OnOpponentDisconnected.WRITER>();
            var arg_ = new CapnpGen.MatchListener.Params_OnOpponentDisconnected()
            {};
            arg_?.serialize(in_);
            using (var d_ = await Call(18195091486387260876UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchListener.Result_OnOpponentDisconnected>(d_);
                return;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfc81f34e1dd8cdccUL)]
    public class MatchListener_Skeleton : Skeleton<IMatchListener>
    {
        public MatchListener_Skeleton()
        {
            SetMethodTable(OnMatchSettled, OnOpponentDisconnected);
        }

        public override ulong InterfaceId => 18195091486387260876UL;
        async Task<AnswerOrCounterquestion> OnMatchSettled(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchListener.Params_OnMatchSettled>(d_);
                await Impl.OnMatchSettled(in_.Outcome, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.MatchListener.Result_OnMatchSettled.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> OnOpponentDisconnected(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                await Impl.OnOpponentDisconnected(cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.MatchListener.Result_OnOpponentDisconnected.WRITER>();
                return s_;
            }
        }
    }

    public static class MatchListener
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa558f407810cd239UL)]
        public class Params_OnMatchSettled : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa558f407810cd239UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Outcome = CapnpSerializable.Create<CapnpGen.Outcome>(reader.Outcome);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Outcome?.serialize(writer.Outcome);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.Outcome Outcome
            {
                get;
                set;
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
                public CapnpGen.Outcome.READER Outcome => ctx.ReadStruct(0, CapnpGen.Outcome.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.Outcome.WRITER Outcome
                {
                    get => BuildPointer<CapnpGen.Outcome.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9403cf0d1a49bfb8UL)]
        public class Result_OnMatchSettled : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9403cf0d1a49bfb8UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 0);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfdfcc22bad8dd53dUL)]
        public class Params_OnOpponentDisconnected : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfdfcc22bad8dd53dUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 0);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8fb87e8e0ac5e59cUL)]
        public class Result_OnOpponentDisconnected : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8fb87e8e0ac5e59cUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public struct READER
            {
                readonly DeserializerState ctx;
                public READER(DeserializerState ctx)
                {
                    this.ctx = ctx;
                }

                public static READER create(DeserializerState ctx) => new READER(ctx);
                public static implicit operator DeserializerState(READER reader) => reader.ctx;
                public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 0);
                }
            }
        }
    }

    public static partial class PipeliningSupportExtensions_service
    {
        static readonly MemberAccessPath Path__UserSession_requestMatch_Session = new MemberAccessPath(1U);
        public static CapnpGen.IMatchSession Session(this Task<(CapnpGen.MatchAssignment, CapnpGen.IMatchSession)> task)
        {
            async Task<IDisposable> AwaitProxy() => (await task).Item2;
            return (CapnpGen.IMatchSession)CapabilityReflection.CreateProxy<CapnpGen.IMatchSession>(Impatient.Access(task, Path__UserSession_requestMatch_Session, AwaitProxy()));
        }
    }
}