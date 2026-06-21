using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x90b291895070d79eUL), Proxy(typeof(GameSessionService_Proxy)), Skeleton(typeof(GameSessionService_Skeleton))]
    public interface IGameSessionService : IDisposable
    {
        Task<CapnpGen.IUserSession> Login(ulong gameId, IReadOnlyList<byte> signature, IReadOnlyList<byte> challengePayload, CancellationToken cancellationToken_ = default);
        Task<(IReadOnlyList<byte>, ulong)> RequestChallenge(ulong gameId, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x90b291895070d79eUL)]
    public class GameSessionService_Proxy : Proxy, IGameSessionService
    {
        public Task<CapnpGen.IUserSession> Login(ulong gameId, IReadOnlyList<byte> signature, IReadOnlyList<byte> challengePayload, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameSessionService.Params_Login.WRITER>();
            var arg_ = new CapnpGen.GameSessionService.Params_Login()
            {GameId = gameId, Signature = signature, ChallengePayload = challengePayload};
            arg_?.serialize(in_);
            return Impatient.MakePipelineAware(Call(10426556106262239134UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_), d_ =>
            {
                using (d_)
                {
                    var r_ = CapnpSerializable.Create<CapnpGen.GameSessionService.Result_Login>(d_);
                    return (r_.Session);
                }
            }

            );
        }

        public async Task<(IReadOnlyList<byte>, ulong)> RequestChallenge(ulong gameId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameSessionService.Params_RequestChallenge.WRITER>();
            var arg_ = new CapnpGen.GameSessionService.Params_RequestChallenge()
            {GameId = gameId};
            arg_?.serialize(in_);
            using (var d_ = await Call(10426556106262239134UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameSessionService.Result_RequestChallenge>(d_);
                return (r_.Challenge, r_.ExpiresAt);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x90b291895070d79eUL)]
    public class GameSessionService_Skeleton : Skeleton<IGameSessionService>
    {
        public GameSessionService_Skeleton()
        {
            SetMethodTable(Login, RequestChallenge);
        }

        public override ulong InterfaceId => 10426556106262239134UL;
        Task<AnswerOrCounterquestion> Login(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameSessionService.Params_Login>(d_);
                return Impatient.MaybeTailCall(Impl.Login(in_.GameId, in_.Signature, in_.ChallengePayload, cancellationToken_), session =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameSessionService.Result_Login.WRITER>();
                    var r_ = new CapnpGen.GameSessionService.Result_Login{Session = session};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> RequestChallenge(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameSessionService.Params_RequestChallenge>(d_);
                return Impatient.MaybeTailCall(Impl.RequestChallenge(in_.GameId, cancellationToken_), (challenge, expiresAt) =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameSessionService.Result_RequestChallenge.WRITER>();
                    var r_ = new CapnpGen.GameSessionService.Result_RequestChallenge{Challenge = challenge, ExpiresAt = expiresAt};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class GameSessionService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xea8442a159ee3b31UL)]
        public class Params_Login : ICapnpSerializable
        {
            public const UInt64 typeId = 0xea8442a159ee3b31UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameId = reader.GameId;
                Signature = reader.Signature;
                ChallengePayload = reader.ChallengePayload;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.GameId = GameId;
                writer.Signature.Init(Signature);
                writer.ChallengePayload.Init(ChallengePayload);
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

            public IReadOnlyList<byte> Signature
            {
                get;
                set;
            }

            public IReadOnlyList<byte> ChallengePayload
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
                public IReadOnlyList<byte> Signature => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> ChallengePayload => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 2);
                }

                public ulong GameId
                {
                    get => this.ReadDataULong(0UL, 0UL);
                    set => this.WriteData(0UL, value, 0UL);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> ChallengePayload
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd7124a5eab13ea76UL)]
        public class Result_Login : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd7124a5eab13ea76UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc40005efa636ad50UL)]
        public class Params_RequestChallenge : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc40005efa636ad50UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameId = reader.GameId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.GameId = GameId;
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
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 0);
                }

                public ulong GameId
                {
                    get => this.ReadDataULong(0UL, 0UL);
                    set => this.WriteData(0UL, value, 0UL);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd8be5c7ee8a3eb27UL)]
        public class Result_RequestChallenge : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd8be5c7ee8a3eb27UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Challenge = reader.Challenge;
                ExpiresAt = reader.ExpiresAt;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Challenge.Init(Challenge);
                writer.ExpiresAt = ExpiresAt;
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> Challenge
            {
                get;
                set;
            }

            public ulong ExpiresAt
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
                public IReadOnlyList<byte> Challenge => ctx.ReadList(0).CastByte();
                public ulong ExpiresAt => ctx.ReadDataULong(0UL, 0UL);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 1);
                }

                public ListOfPrimitivesSerializer<byte> Challenge
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ulong ExpiresAt
                {
                    get => this.ReadDataULong(0UL, 0UL);
                    set => this.WriteData(0UL, value, 0UL);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x897b3f34b12e7f49UL), Proxy(typeof(UserSession_Proxy)), Skeleton(typeof(UserSession_Skeleton))]
    public interface IUserSession : IDisposable
    {
        Task<(CapnpGen.MatchAssignment, CapnpGen.IMatchSession)> RequestMatch(CapnpGen.GameMatchRequest req, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.IMatchSession> Reconnect(IReadOnlyList<byte> matchId, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x897b3f34b12e7f49UL)]
    public class UserSession_Proxy : Proxy, IUserSession
    {
        public Task<(CapnpGen.MatchAssignment, CapnpGen.IMatchSession)> RequestMatch(CapnpGen.GameMatchRequest req, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.UserSession.Params_RequestMatch.WRITER>();
            var arg_ = new CapnpGen.UserSession.Params_RequestMatch()
            {Req = req};
            arg_?.serialize(in_);
            return Impatient.MakePipelineAware(Call(9906581300875001673UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_), d_ =>
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
            return Impatient.MakePipelineAware(Call(9906581300875001673UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_), d_ =>
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x897b3f34b12e7f49UL)]
    public class UserSession_Skeleton : Skeleton<IUserSession>
    {
        public UserSession_Skeleton()
        {
            SetMethodTable(RequestMatch, Reconnect);
        }

        public override ulong InterfaceId => 9906581300875001673UL;
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
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcf96bba70f1b170fUL)]
        public class Params_RequestMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xcf96bba70f1b170fUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe4dd6aece9b3cb4cUL)]
        public class Result_RequestMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe4dd6aece9b3cb4cUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd25f9fcf575378adUL)]
        public class Params_Reconnect : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd25f9fcf575378adUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb4956d48a8b9c71fUL)]
        public class Result_Reconnect : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb4956d48a8b9c71fUL;
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x874be70686adbb2bUL), Proxy(typeof(MatchSession_Proxy)), Skeleton(typeof(MatchSession_Skeleton))]
    public interface IMatchSession : IDisposable
    {
        Task<IReadOnlyList<byte>> SubmitOutcome(CapnpGen.OutcomeSubmission submission, CancellationToken cancellationToken_ = default);
        Task SubscribeToEvents(CapnpGen.IMatchListener subscriber, CancellationToken cancellationToken_ = default);
        Task EmitGameEvent(CapnpGen.GameEvent @event, CancellationToken cancellationToken_ = default);
        Task EmitTelemetry(CapnpGen.AmpTelemetryEvent @event, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x874be70686adbb2bUL)]
    public class MatchSession_Proxy : Proxy, IMatchSession
    {
        public async Task<IReadOnlyList<byte>> SubmitOutcome(CapnpGen.OutcomeSubmission submission, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchSession.Params_SubmitOutcome.WRITER>();
            var arg_ = new CapnpGen.MatchSession.Params_SubmitOutcome()
            {Submission = submission};
            arg_?.serialize(in_);
            using (var d_ = await Call(9749139833588923179UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
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
            using (var d_ = await Call(9749139833588923179UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
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
            using (var d_ = await Call(9749139833588923179UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
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
            using (var d_ = await Call(9749139833588923179UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchSession.Result_EmitTelemetry>(d_);
                return;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x874be70686adbb2bUL)]
    public class MatchSession_Skeleton : Skeleton<IMatchSession>
    {
        public MatchSession_Skeleton()
        {
            SetMethodTable(SubmitOutcome, SubscribeToEvents, EmitGameEvent, EmitTelemetry);
        }

        public override ulong InterfaceId => 9749139833588923179UL;
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
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb2dc3b6168926c48UL)]
        public class Params_SubmitOutcome : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb2dc3b6168926c48UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xefe2ce78f62b1a0aUL)]
        public class Result_SubmitOutcome : ICapnpSerializable
        {
            public const UInt64 typeId = 0xefe2ce78f62b1a0aUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc41b18fff24e7b05UL)]
        public class Params_SubscribeToEvents : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc41b18fff24e7b05UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb3b96bc8636e07f9UL)]
        public class Result_SubscribeToEvents : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb3b96bc8636e07f9UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x81e8a58e0d3b0d52UL)]
        public class Params_EmitGameEvent : ICapnpSerializable
        {
            public const UInt64 typeId = 0x81e8a58e0d3b0d52UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x90d3a52b16b3e38bUL)]
        public class Result_EmitGameEvent : ICapnpSerializable
        {
            public const UInt64 typeId = 0x90d3a52b16b3e38bUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x923da56d91727937UL)]
        public class Params_EmitTelemetry : ICapnpSerializable
        {
            public const UInt64 typeId = 0x923da56d91727937UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc3b741290578892eUL)]
        public class Result_EmitTelemetry : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc3b741290578892eUL;
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaa4956db47eea5edUL), Proxy(typeof(MatchListener_Proxy)), Skeleton(typeof(MatchListener_Skeleton))]
    public interface IMatchListener : IDisposable
    {
        Task OnMatchSettled(CapnpGen.Outcome outcome, CancellationToken cancellationToken_ = default);
        Task OnOpponentDisconnected(CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaa4956db47eea5edUL)]
    public class MatchListener_Proxy : Proxy, IMatchListener
    {
        public async Task OnMatchSettled(CapnpGen.Outcome outcome, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchListener.Params_OnMatchSettled.WRITER>();
            var arg_ = new CapnpGen.MatchListener.Params_OnMatchSettled()
            {Outcome = outcome};
            arg_?.serialize(in_);
            using (var d_ = await Call(12270434159552275949UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
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
            using (var d_ = await Call(12270434159552275949UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchListener.Result_OnOpponentDisconnected>(d_);
                return;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaa4956db47eea5edUL)]
    public class MatchListener_Skeleton : Skeleton<IMatchListener>
    {
        public MatchListener_Skeleton()
        {
            SetMethodTable(OnMatchSettled, OnOpponentDisconnected);
        }

        public override ulong InterfaceId => 12270434159552275949UL;
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
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf21c678e24dc1ffaUL)]
        public class Params_OnMatchSettled : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf21c678e24dc1ffaUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x918db29687dac42aUL)]
        public class Result_OnMatchSettled : ICapnpSerializable
        {
            public const UInt64 typeId = 0x918db29687dac42aUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbf27ceeda9625a02UL)]
        public class Params_OnOpponentDisconnected : ICapnpSerializable
        {
            public const UInt64 typeId = 0xbf27ceeda9625a02UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x99f94a8c3af71da4UL)]
        public class Result_OnOpponentDisconnected : ICapnpSerializable
        {
            public const UInt64 typeId = 0x99f94a8c3af71da4UL;
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcf7550435cb188f2UL), Proxy(typeof(ExtendedPlayerService_Proxy)), Skeleton(typeof(ExtendedPlayerService_Skeleton))]
    public interface IExtendedPlayerService : CapnpGen.IPlayerProfileService
    {
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcf7550435cb188f2UL)]
    public class ExtendedPlayerService_Proxy : Proxy, IExtendedPlayerService
    {
        public async Task<IReadOnlyList<byte>> CreateOrUpdateProfile(IReadOnlyList<byte> playerId, string displayName, IReadOnlyList<byte> walletAddress, string preferredRole, string language, string platform, string region, uint maxPingMs, float initialMmr, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Params_CreateOrUpdateProfile.WRITER>();
            var arg_ = new CapnpGen.PlayerProfileService.Params_CreateOrUpdateProfile()
            {PlayerId = playerId, DisplayName = displayName, WalletAddress = walletAddress, PreferredRole = preferredRole, Language = language, Platform = platform, Region = region, MaxPingMs = maxPingMs, InitialMmr = initialMmr};
            arg_?.serialize(in_);
            using (var d_ = await Call(13905551579536347693UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Result_CreateOrUpdateProfile>(d_);
                return (r_.PlayerId);
            }
        }

        public async Task<CapnpGen.PlayerProfile> GetProfile(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Params_GetProfile.WRITER>();
            var arg_ = new CapnpGen.PlayerProfileService.Params_GetProfile()
            {PlayerId = playerId};
            arg_?.serialize(in_);
            using (var d_ = await Call(13905551579536347693UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Result_GetProfile>(d_);
                return (r_.Profile);
            }
        }

        public async Task RecordMatchResult(IReadOnlyList<byte> playerId, IReadOnlyList<byte> opponentId, IReadOnlyList<byte> gameId, float score, ulong playTimeMs, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Params_RecordMatchResult.WRITER>();
            var arg_ = new CapnpGen.PlayerProfileService.Params_RecordMatchResult()
            {PlayerId = playerId, OpponentId = opponentId, GameId = gameId, Score = score, PlayTimeMs = playTimeMs};
            arg_?.serialize(in_);
            using (var d_ = await Call(13905551579536347693UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Result_RecordMatchResult>(d_);
                return;
            }
        }

        public async Task SetOffline(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Params_SetOffline.WRITER>();
            var arg_ = new CapnpGen.PlayerProfileService.Params_SetOffline()
            {PlayerId = playerId};
            arg_?.serialize(in_);
            using (var d_ = await Call(13905551579536347693UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Result_SetOffline>(d_);
                return;
            }
        }

        public async Task<IReadOnlyList<CapnpGen.PlayerProfile>> ListPlayers(CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Params_ListPlayers.WRITER>();
            var arg_ = new CapnpGen.PlayerProfileService.Params_ListPlayers()
            {};
            arg_?.serialize(in_);
            using (var d_ = await Call(13905551579536347693UL, 4, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Result_ListPlayers>(d_);
                return (r_.Players);
            }
        }

        public async Task ApplyRestriction(IReadOnlyList<byte> playerId, bool ban, ulong cooldownMs, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Params_ApplyRestriction.WRITER>();
            var arg_ = new CapnpGen.PlayerProfileService.Params_ApplyRestriction()
            {PlayerId = playerId, Ban = ban, CooldownMs = cooldownMs};
            arg_?.serialize(in_);
            using (var d_ = await Call(13905551579536347693UL, 5, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Result_ApplyRestriction>(d_);
                return;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcf7550435cb188f2UL)]
    public class ExtendedPlayerService_Skeleton : Skeleton<IExtendedPlayerService>
    {
        public ExtendedPlayerService_Skeleton()
        {
            SetMethodTable();
        }

        public override ulong InterfaceId => 14948942788374399218UL;
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcdec5f8e9381ff4fUL), Proxy(typeof(ExtendedMatchmakingService_Proxy)), Skeleton(typeof(ExtendedMatchmakingService_Skeleton))]
    public interface IExtendedMatchmakingService : CapnpGen.IMatchmakingRuleService
    {
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcdec5f8e9381ff4fUL)]
    public class ExtendedMatchmakingService_Proxy : Proxy, IExtendedMatchmakingService
    {
        public async Task<IReadOnlyList<byte>> CreateRuleSet(CapnpGen.RuleSet ruleSet, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Params_CreateRuleSet.WRITER>();
            var arg_ = new CapnpGen.MatchmakingRuleService.Params_CreateRuleSet()
            {RuleSet = ruleSet, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15308525362632493867UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Result_CreateRuleSet>(d_);
                return (r_.RuleSetId);
            }
        }

        public async Task UpdateRuleSet(IReadOnlyList<byte> ruleSetId, CapnpGen.RuleSet ruleSet, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Params_UpdateRuleSet.WRITER>();
            var arg_ = new CapnpGen.MatchmakingRuleService.Params_UpdateRuleSet()
            {RuleSetId = ruleSetId, RuleSet = ruleSet, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15308525362632493867UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Result_UpdateRuleSet>(d_);
                return;
            }
        }

        public async Task DeleteRuleSet(IReadOnlyList<byte> ruleSetId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Params_DeleteRuleSet.WRITER>();
            var arg_ = new CapnpGen.MatchmakingRuleService.Params_DeleteRuleSet()
            {RuleSetId = ruleSetId, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15308525362632493867UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Result_DeleteRuleSet>(d_);
                return;
            }
        }

        public async Task<CapnpGen.RuleSet> GetRuleSet(IReadOnlyList<byte> ruleSetId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Params_GetRuleSet.WRITER>();
            var arg_ = new CapnpGen.MatchmakingRuleService.Params_GetRuleSet()
            {RuleSetId = ruleSetId};
            arg_?.serialize(in_);
            using (var d_ = await Call(15308525362632493867UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Result_GetRuleSet>(d_);
                return (r_.RuleSet);
            }
        }

        public async Task<IReadOnlyList<CapnpGen.RuleSet>> ListRuleSets(IReadOnlyList<byte> gameId, IReadOnlyList<byte> modeId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Params_ListRuleSets.WRITER>();
            var arg_ = new CapnpGen.MatchmakingRuleService.Params_ListRuleSets()
            {GameId = gameId, ModeId = modeId};
            arg_?.serialize(in_);
            using (var d_ = await Call(15308525362632493867UL, 4, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Result_ListRuleSets>(d_);
                return (r_.RuleSets);
            }
        }

        public async Task<(CapnpGen.MatchQuality, bool)> EvaluateMatch(CapnpGen.RuleEvaluationContext context, IReadOnlyList<byte> ruleSetId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Params_EvaluateMatch.WRITER>();
            var arg_ = new CapnpGen.MatchmakingRuleService.Params_EvaluateMatch()
            {Context = context, RuleSetId = ruleSetId};
            arg_?.serialize(in_);
            using (var d_ = await Call(15308525362632493867UL, 5, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Result_EvaluateMatch>(d_);
                return (r_.Quality, r_.Passes);
            }
        }

        public async Task<(IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>>, CapnpGen.MatchQuality)> FindBestMatch(IReadOnlyList<CapnpGen.PlayerContext> players, IReadOnlyList<byte> ruleSetId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Params_FindBestMatch.WRITER>();
            var arg_ = new CapnpGen.MatchmakingRuleService.Params_FindBestMatch()
            {Players = players, RuleSetId = ruleSetId};
            arg_?.serialize(in_);
            using (var d_ = await Call(15308525362632493867UL, 6, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Result_FindBestMatch>(d_);
                return (r_.Teams, r_.Quality);
            }
        }

        public async Task<CapnpGen.RuleStats> GetRuleStatistics(IReadOnlyList<byte> ruleSetId, CapnpGen.TimeRange timeRange, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Params_GetRuleStatistics.WRITER>();
            var arg_ = new CapnpGen.MatchmakingRuleService.Params_GetRuleStatistics()
            {RuleSetId = ruleSetId, TimeRange = timeRange};
            arg_?.serialize(in_);
            using (var d_ = await Call(15308525362632493867UL, 7, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Result_GetRuleStatistics>(d_);
                return (r_.Stats);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcdec5f8e9381ff4fUL)]
    public class ExtendedMatchmakingService_Skeleton : Skeleton<IExtendedMatchmakingService>
    {
        public ExtendedMatchmakingService_Skeleton()
        {
            SetMethodTable();
        }

        public override ulong InterfaceId => 14838339938243706703UL;
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa7dc66602fd7e20bUL), Proxy(typeof(ExtendedInventoryService_Proxy)), Skeleton(typeof(ExtendedInventoryService_Skeleton))]
    public interface IExtendedInventoryService : CapnpGen.IInventoryService
    {
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa7dc66602fd7e20bUL)]
    public class ExtendedInventoryService_Proxy : Proxy, IExtendedInventoryService
    {
        public async Task<CapnpGen.ItemMetadata> GetItemMetadata(IReadOnlyList<byte> itemId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_GetItemMetadata.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_GetItemMetadata()
            {ItemId = itemId};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_GetItemMetadata>(d_);
                return (r_.Metadata);
            }
        }

        public async Task<IReadOnlyList<byte>> CreateItem(CapnpGen.ItemMetadata metadata, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_CreateItem.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_CreateItem()
            {Metadata = metadata, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_CreateItem>(d_);
                return (r_.ItemId);
            }
        }

        public async Task UpdateItem(IReadOnlyList<byte> itemId, CapnpGen.ItemMetadata metadata, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_UpdateItem.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_UpdateItem()
            {ItemId = itemId, Metadata = metadata, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_UpdateItem>(d_);
                return;
            }
        }

        public async Task<IReadOnlyList<CapnpGen.InventoryItem>> GetInventory(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_GetInventory.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_GetInventory()
            {PlayerId = playerId};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_GetInventory>(d_);
                return (r_.Items);
            }
        }

        public async Task<IReadOnlyList<byte>> AddItem(IReadOnlyList<byte> playerId, IReadOnlyList<byte> templateId, uint quantity, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_AddItem.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_AddItem()
            {PlayerId = playerId, TemplateId = templateId, Quantity = quantity, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 4, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_AddItem>(d_);
                return (r_.ItemId);
            }
        }

        public async Task RemoveItem(IReadOnlyList<byte> playerId, IReadOnlyList<byte> itemId, uint quantity, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_RemoveItem.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_RemoveItem()
            {PlayerId = playerId, ItemId = itemId, Quantity = quantity, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 5, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_RemoveItem>(d_);
                return;
            }
        }

        public async Task TransferItem(IReadOnlyList<byte> fromPlayer, IReadOnlyList<byte> toPlayer, IReadOnlyList<byte> itemId, uint quantity, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_TransferItem.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_TransferItem()
            {FromPlayer = fromPlayer, ToPlayer = toPlayer, ItemId = itemId, Quantity = quantity, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 6, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_TransferItem>(d_);
                return;
            }
        }

        public async Task EquipItem(IReadOnlyList<byte> playerId, IReadOnlyList<byte> itemId, string slot, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_EquipItem.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_EquipItem()
            {PlayerId = playerId, ItemId = itemId, Slot = slot};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 7, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_EquipItem>(d_);
                return;
            }
        }

        public async Task UnequipItem(IReadOnlyList<byte> playerId, string slot, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_UnequipItem.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_UnequipItem()
            {PlayerId = playerId, Slot = slot};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 8, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_UnequipItem>(d_);
                return;
            }
        }

        public async Task<IReadOnlyList<byte>> CreateLoadout(CapnpGen.Loadout loadout, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_CreateLoadout.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_CreateLoadout()
            {Loadout = loadout, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 9, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_CreateLoadout>(d_);
                return (r_.LoadoutId);
            }
        }

        public async Task ActivateLoadout(IReadOnlyList<byte> playerId, IReadOnlyList<byte> loadoutId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_ActivateLoadout.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_ActivateLoadout()
            {PlayerId = playerId, LoadoutId = loadoutId};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 10, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_ActivateLoadout>(d_);
                return;
            }
        }

        public async Task<(bool, IReadOnlyList<byte>)> CraftItem(IReadOnlyList<byte> playerId, IReadOnlyList<byte> recipeId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_CraftItem.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_CraftItem()
            {PlayerId = playerId, RecipeId = recipeId, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 11, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_CraftItem>(d_);
                return (r_.Success, r_.ItemId);
            }
        }

        public async Task<IReadOnlyList<CapnpGen.Recipe>> GetRecipes(IReadOnlyList<byte> playerId, CapnpGen.RecipeFilter filter, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_GetRecipes.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_GetRecipes()
            {PlayerId = playerId, Filter = filter};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 12, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_GetRecipes>(d_);
                return (r_.Recipes);
            }
        }

        public async Task<IReadOnlyList<byte>> CreateTradeOffer(CapnpGen.TradeOffer offer, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_CreateTradeOffer.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_CreateTradeOffer()
            {Offer = offer, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 13, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_CreateTradeOffer>(d_);
                return (r_.OfferId);
            }
        }

        public async Task AcceptTradeOffer(IReadOnlyList<byte> offerId, IReadOnlyList<byte> buyerId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_AcceptTradeOffer.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_AcceptTradeOffer()
            {OfferId = offerId, BuyerId = buyerId, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 14, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_AcceptTradeOffer>(d_);
                return;
            }
        }

        public async Task CancelTradeOffer(IReadOnlyList<byte> offerId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_CancelTradeOffer.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_CancelTradeOffer()
            {OfferId = offerId, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 15, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_CancelTradeOffer>(d_);
                return;
            }
        }

        public async Task<(ulong, IReadOnlyList<CapnpGen.AssetValue>)> GetInventoryValue(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Params_GetInventoryValue.WRITER>();
            var arg_ = new CapnpGen.InventoryService.Params_GetInventoryValue()
            {PlayerId = playerId};
            arg_?.serialize(in_);
            using (var d_ = await Call(16495165711826257929UL, 16, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.InventoryService.Result_GetInventoryValue>(d_);
                return (r_.Value, r_.Breakdown);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa7dc66602fd7e20bUL)]
    public class ExtendedInventoryService_Skeleton : Skeleton<IExtendedInventoryService>
    {
        public ExtendedInventoryService_Skeleton()
        {
            SetMethodTable();
        }

        public override ulong InterfaceId => 12095655262515880459UL;
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x99ace0b83a2d3f2eUL), Proxy(typeof(ExtendedTournamentService_Proxy)), Skeleton(typeof(ExtendedTournamentService_Skeleton))]
    public interface IExtendedTournamentService : CapnpGen.ITournamentService
    {
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x99ace0b83a2d3f2eUL)]
    public class ExtendedTournamentService_Proxy : Proxy, IExtendedTournamentService
    {
        public async Task<IReadOnlyList<byte>> CreateTournament(CapnpGen.Tournament tournament, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_CreateTournament.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_CreateTournament()
            {Tournament = tournament, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_CreateTournament>(d_);
                return (r_.TournamentId);
            }
        }

        public async Task UpdateTournament(IReadOnlyList<byte> tournamentId, CapnpGen.Tournament tournament, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_UpdateTournament.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_UpdateTournament()
            {TournamentId = tournamentId, Tournament = tournament, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_UpdateTournament>(d_);
                return;
            }
        }

        public async Task CancelTournament(IReadOnlyList<byte> tournamentId, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_CancelTournament.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_CancelTournament()
            {TournamentId = tournamentId, Reason = reason, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_CancelTournament>(d_);
                return;
            }
        }

        public async Task<IReadOnlyList<byte>> Register(IReadOnlyList<byte> tournamentId, CapnpGen.TournamentRegistration registration, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_Register.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_Register()
            {TournamentId = tournamentId, Registration = registration, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_Register>(d_);
                return (r_.RegistrationId);
            }
        }

        public async Task CheckIn(IReadOnlyList<byte> registrationId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_CheckIn.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_CheckIn()
            {RegistrationId = registrationId, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 4, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_CheckIn>(d_);
                return;
            }
        }

        public async Task Withdraw(IReadOnlyList<byte> registrationId, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_Withdraw.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_Withdraw()
            {RegistrationId = registrationId, Reason = reason, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 5, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_Withdraw>(d_);
                return;
            }
        }

        public async Task<IReadOnlyList<byte>> GenerateBracket(IReadOnlyList<byte> tournamentId, CapnpGen.TournamentType bracketType, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_GenerateBracket.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_GenerateBracket()
            {TournamentId = tournamentId, BracketType = bracketType, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 6, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_GenerateBracket>(d_);
                return (r_.BracketId);
            }
        }

        public async Task AdvanceRound(IReadOnlyList<byte> tournamentId, uint roundNumber, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_AdvanceRound.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_AdvanceRound()
            {TournamentId = tournamentId, RoundNumber = roundNumber, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 7, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_AdvanceRound>(d_);
                return;
            }
        }

        public async Task<IReadOnlyList<byte>> ScheduleMatch(CapnpGen.TournamentMatch match, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_ScheduleMatch.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_ScheduleMatch()
            {Match = match, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 8, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_ScheduleMatch>(d_);
                return (r_.MatchId);
            }
        }

        public async Task ReportMatchResult(IReadOnlyList<byte> matchId, CapnpGen.MatchResult result, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_ReportMatchResult.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_ReportMatchResult()
            {MatchId = matchId, Result = result, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 9, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_ReportMatchResult>(d_);
                return;
            }
        }

        public async Task DisputeMatch(IReadOnlyList<byte> matchId, CapnpGen.Dispute dispute, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_DisputeMatch.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_DisputeMatch()
            {MatchId = matchId, Dispute = dispute, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 10, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_DisputeMatch>(d_);
                return;
            }
        }

        public async Task<CapnpGen.Tournament> GetTournament(IReadOnlyList<byte> tournamentId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_GetTournament.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_GetTournament()
            {TournamentId = tournamentId};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 11, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_GetTournament>(d_);
                return (r_.Tournament);
            }
        }

        public async Task<IReadOnlyList<CapnpGen.Tournament>> ListTournaments(CapnpGen.TournamentFilter filter, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_ListTournaments.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_ListTournaments()
            {Filter = filter};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 12, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_ListTournaments>(d_);
                return (r_.Tournaments);
            }
        }

        public async Task<CapnpGen.Bracket> GetBracket(IReadOnlyList<byte> tournamentId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_GetBracket.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_GetBracket()
            {TournamentId = tournamentId};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 13, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_GetBracket>(d_);
                return (r_.Bracket);
            }
        }

        public async Task<IReadOnlyList<CapnpGen.Standing>> GetStandings(IReadOnlyList<byte> tournamentId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_GetStandings.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_GetStandings()
            {TournamentId = tournamentId};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 14, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_GetStandings>(d_);
                return (r_.Standings);
            }
        }

        public async Task DisqualifyParticipant(IReadOnlyList<byte> registrationId, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_DisqualifyParticipant.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_DisqualifyParticipant()
            {RegistrationId = registrationId, Reason = reason, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 15, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_DisqualifyParticipant>(d_);
                return;
            }
        }

        public async Task<IReadOnlyList<byte>> DistributePrizes(IReadOnlyList<byte> tournamentId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Params_DistributePrizes.WRITER>();
            var arg_ = new CapnpGen.TournamentService.Params_DistributePrizes()
            {TournamentId = tournamentId, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(15742733316282171705UL, 16, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TournamentService.Result_DistributePrizes>(d_);
                return (r_.DistributionTxHash);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x99ace0b83a2d3f2eUL)]
    public class ExtendedTournamentService_Skeleton : Skeleton<IExtendedTournamentService>
    {
        public ExtendedTournamentService_Skeleton()
        {
            SetMethodTable();
        }

        public override ulong InterfaceId => 11073472665651855150UL;
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf90806cd2318e283UL), Proxy(typeof(ExtendedSecurityService_Proxy)), Skeleton(typeof(ExtendedSecurityService_Skeleton))]
    public interface IExtendedSecurityService : CapnpGen.ISecurityService
    {
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf90806cd2318e283UL)]
    public class ExtendedSecurityService_Proxy : Proxy, IExtendedSecurityService
    {
        public async Task<IReadOnlyList<byte>> ReportIncident(CapnpGen.SecurityIncident incident, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_ReportIncident.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_ReportIncident()
            {Incident = incident, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_ReportIncident>(d_);
                return (r_.IncidentId);
            }
        }

        public async Task UpdateIncident(IReadOnlyList<byte> incidentId, CapnpGen.SecurityIncident incident, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_UpdateIncident.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_UpdateIncident()
            {IncidentId = incidentId, Incident = incident, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_UpdateIncident>(d_);
                return;
            }
        }

        public async Task ResolveIncident(IReadOnlyList<byte> incidentId, CapnpGen.Resolution resolution, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_ResolveIncident.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_ResolveIncident()
            {IncidentId = incidentId, Resolution = resolution, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_ResolveIncident>(d_);
                return;
            }
        }

        public async Task<CapnpGen.BehaviorProfile> GetBehaviorProfile(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_GetBehaviorProfile.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_GetBehaviorProfile()
            {PlayerId = playerId};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_GetBehaviorProfile>(d_);
                return (r_.Profile);
            }
        }

        public async Task ApplySecurityAction(IReadOnlyList<byte> playerId, CapnpGen.SecurityAction action, ulong duration, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_ApplySecurityAction.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_ApplySecurityAction()
            {PlayerId = playerId, Action = action, Duration = duration, Reason = reason, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 4, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_ApplySecurityAction>(d_);
                return;
            }
        }

        public async Task<bool> AppealAction(IReadOnlyList<byte> actionId, CapnpGen.Appeal appeal, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_AppealAction.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_AppealAction()
            {ActionId = actionId, Appeal = appeal, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 5, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_AppealAction>(d_);
                return (r_.Accepted);
            }
        }

        public async Task<(float, IReadOnlyList<CapnpGen.FraudIndicator>)> DetectFraud(IReadOnlyList<byte> matchId, CapnpGen.FraudDetectionData data, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_DetectFraud.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_DetectFraud()
            {MatchId = matchId, Data = data};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 6, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_DetectFraud>(d_);
                return (r_.RiskScore, r_.Indicators);
            }
        }

        public async Task<IReadOnlyList<CapnpGen.FraudPattern>> GetFraudPatterns(IReadOnlyList<byte> gameId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_GetFraudPatterns.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_GetFraudPatterns()
            {GameId = gameId};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 7, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_GetFraudPatterns>(d_);
                return (r_.Patterns);
            }
        }

        public async Task<IReadOnlyList<byte>> SubmitAntiCheatReport(CapnpGen.AntiCheatReport report, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_SubmitAntiCheatReport.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_SubmitAntiCheatReport()
            {Report = report, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 8, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_SubmitAntiCheatReport>(d_);
                return (r_.ReportId);
            }
        }

        public async Task ReviewAntiCheatReport(IReadOnlyList<byte> reportId, CapnpGen.Review review, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_ReviewAntiCheatReport.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_ReviewAntiCheatReport()
            {ReportId = reportId, Review = review, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 9, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_ReviewAntiCheatReport>(d_);
                return;
            }
        }

        public async Task<CapnpGen.SecurityStatistics> GetSecurityStats(CapnpGen.TimeRange timeRange, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_GetSecurityStats.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_GetSecurityStats()
            {TimeRange = timeRange};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 10, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_GetSecurityStats>(d_);
                return (r_.Stats);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf90806cd2318e283UL)]
    public class ExtendedSecurityService_Skeleton : Skeleton<IExtendedSecurityService>
    {
        public ExtendedSecurityService_Skeleton()
        {
            SetMethodTable();
        }

        public override ulong InterfaceId => 17944600193384637059UL;
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8c81c101f96ba2e6UL), Proxy(typeof(ProtocolRegistryService_Proxy)), Skeleton(typeof(ProtocolRegistryService_Skeleton))]
    public interface IProtocolRegistryService : IDisposable
    {
        Task<IReadOnlyList<byte>> RegisterGame(CapnpGen.GameRegistrationInfo gameInfo, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.ServiceEndpoint>> GetServiceEndpoints(CapnpGen.ServiceType serviceType, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.ProtocolVersion> GetProtocolVersion(CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8c81c101f96ba2e6UL)]
    public class ProtocolRegistryService_Proxy : Proxy, IProtocolRegistryService
    {
        public async Task<IReadOnlyList<byte>> RegisterGame(CapnpGen.GameRegistrationInfo gameInfo, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.ProtocolRegistryService.Params_RegisterGame.WRITER>();
            var arg_ = new CapnpGen.ProtocolRegistryService.Params_RegisterGame()
            {GameInfo = gameInfo, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(10124585651529294566UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.ProtocolRegistryService.Result_RegisterGame>(d_);
                return (r_.GameId);
            }
        }

        public async Task<IReadOnlyList<CapnpGen.ServiceEndpoint>> GetServiceEndpoints(CapnpGen.ServiceType serviceType, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.ProtocolRegistryService.Params_GetServiceEndpoints.WRITER>();
            var arg_ = new CapnpGen.ProtocolRegistryService.Params_GetServiceEndpoints()
            {ServiceType = serviceType};
            arg_?.serialize(in_);
            using (var d_ = await Call(10124585651529294566UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.ProtocolRegistryService.Result_GetServiceEndpoints>(d_);
                return (r_.Endpoints);
            }
        }

        public async Task<CapnpGen.ProtocolVersion> GetProtocolVersion(CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.ProtocolRegistryService.Params_GetProtocolVersion.WRITER>();
            var arg_ = new CapnpGen.ProtocolRegistryService.Params_GetProtocolVersion()
            {};
            arg_?.serialize(in_);
            using (var d_ = await Call(10124585651529294566UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.ProtocolRegistryService.Result_GetProtocolVersion>(d_);
                return (r_.Version);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8c81c101f96ba2e6UL)]
    public class ProtocolRegistryService_Skeleton : Skeleton<IProtocolRegistryService>
    {
        public ProtocolRegistryService_Skeleton()
        {
            SetMethodTable(RegisterGame, GetServiceEndpoints, GetProtocolVersion);
        }

        public override ulong InterfaceId => 10124585651529294566UL;
        Task<AnswerOrCounterquestion> RegisterGame(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.ProtocolRegistryService.Params_RegisterGame>(d_);
                return Impatient.MaybeTailCall(Impl.RegisterGame(in_.GameInfo, in_.Signature, cancellationToken_), gameId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.ProtocolRegistryService.Result_RegisterGame.WRITER>();
                    var r_ = new CapnpGen.ProtocolRegistryService.Result_RegisterGame{GameId = gameId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetServiceEndpoints(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.ProtocolRegistryService.Params_GetServiceEndpoints>(d_);
                return Impatient.MaybeTailCall(Impl.GetServiceEndpoints(in_.ServiceType, cancellationToken_), endpoints =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.ProtocolRegistryService.Result_GetServiceEndpoints.WRITER>();
                    var r_ = new CapnpGen.ProtocolRegistryService.Result_GetServiceEndpoints{Endpoints = endpoints};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetProtocolVersion(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                return Impatient.MaybeTailCall(Impl.GetProtocolVersion(cancellationToken_), version =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.ProtocolRegistryService.Result_GetProtocolVersion.WRITER>();
                    var r_ = new CapnpGen.ProtocolRegistryService.Result_GetProtocolVersion{Version = version};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class ProtocolRegistryService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8f0ff1a8dc61116aUL)]
        public class Params_RegisterGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8f0ff1a8dc61116aUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameInfo = CapnpSerializable.Create<CapnpGen.GameRegistrationInfo>(reader.GameInfo);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                GameInfo?.serialize(writer.GameInfo);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.GameRegistrationInfo GameInfo
            {
                get;
                set;
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
                public CapnpGen.GameRegistrationInfo.READER GameInfo => ctx.ReadStruct(0, CapnpGen.GameRegistrationInfo.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.GameRegistrationInfo.WRITER GameInfo
                {
                    get => BuildPointer<CapnpGen.GameRegistrationInfo.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfedb279f49001e1bUL)]
        public class Result_RegisterGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfedb279f49001e1bUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameId = reader.GameId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.GameId.Init(GameId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> GameId
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
                public IReadOnlyList<byte> GameId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> GameId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x951a26171124d2b1UL)]
        public class Params_GetServiceEndpoints : ICapnpSerializable
        {
            public const UInt64 typeId = 0x951a26171124d2b1UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                ServiceType = reader.ServiceType;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.ServiceType = ServiceType;
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.ServiceType ServiceType
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
                public CapnpGen.ServiceType ServiceType => (CapnpGen.ServiceType)ctx.ReadDataUShort(0UL, (ushort)0);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 0);
                }

                public CapnpGen.ServiceType ServiceType
                {
                    get => (CapnpGen.ServiceType)this.ReadDataUShort(0UL, (ushort)0);
                    set => this.WriteData(0UL, (ushort)value, (ushort)0);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcd195e23df8faea6UL)]
        public class Result_GetServiceEndpoints : ICapnpSerializable
        {
            public const UInt64 typeId = 0xcd195e23df8faea6UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Endpoints = reader.Endpoints?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.ServiceEndpoint>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Endpoints.Init(Endpoints, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.ServiceEndpoint> Endpoints
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
                public IReadOnlyList<CapnpGen.ServiceEndpoint.READER> Endpoints => ctx.ReadList(0).Cast(CapnpGen.ServiceEndpoint.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.ServiceEndpoint.WRITER> Endpoints
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.ServiceEndpoint.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x965471e2d5f3c65cUL)]
        public class Params_GetProtocolVersion : ICapnpSerializable
        {
            public const UInt64 typeId = 0x965471e2d5f3c65cUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x80b438b4bd1c5e99UL)]
        public class Result_GetProtocolVersion : ICapnpSerializable
        {
            public const UInt64 typeId = 0x80b438b4bd1c5e99UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Version = CapnpSerializable.Create<CapnpGen.ProtocolVersion>(reader.Version);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Version?.serialize(writer.Version);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.ProtocolVersion Version
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
                public CapnpGen.ProtocolVersion.READER Version => ctx.ReadStruct(0, CapnpGen.ProtocolVersion.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.ProtocolVersion.WRITER Version
                {
                    get => BuildPointer<CapnpGen.ProtocolVersion.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd99cb8ac597595b1UL)]
    public class GameRegistrationInfo : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd99cb8ac597595b1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Name = reader.Name;
            Description = reader.Description;
            Developer = reader.Developer;
            Website = reader.Website;
            SupportedRegions = reader.SupportedRegions;
            GameModes = reader.GameModes?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.GameModeInfo>(_));
            AdminAddress = reader.AdminAddress;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Name = Name;
            writer.Description = Description;
            writer.Developer = Developer;
            writer.Website = Website;
            writer.SupportedRegions.Init(SupportedRegions);
            writer.GameModes.Init(GameModes, (_s1, _v1) => _v1?.serialize(_s1));
            writer.AdminAddress.Init(AdminAddress);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string Developer
        {
            get;
            set;
        }

        public string Website
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Region> SupportedRegions
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.GameModeInfo> GameModes
        {
            get;
            set;
        }

        public IReadOnlyList<byte> AdminAddress
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
            public string Name => ctx.ReadText(0, null);
            public string Description => ctx.ReadText(1, null);
            public string Developer => ctx.ReadText(2, null);
            public string Website => ctx.ReadText(3, null);
            public IReadOnlyList<CapnpGen.Region> SupportedRegions => ctx.ReadList(4).CastEnums(_0 => (CapnpGen.Region)_0);
            public IReadOnlyList<CapnpGen.GameModeInfo.READER> GameModes => ctx.ReadList(5).Cast(CapnpGen.GameModeInfo.READER.create);
            public IReadOnlyList<byte> AdminAddress => ctx.ReadList(6).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 7);
            }

            public string Name
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public string Description
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public string Developer
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }

            public string Website
            {
                get => this.ReadText(3, null);
                set => this.WriteText(3, value, null);
            }

            public ListOfPrimitivesSerializer<CapnpGen.Region> SupportedRegions
            {
                get => BuildPointer<ListOfPrimitivesSerializer<CapnpGen.Region>>(4);
                set => Link(4, value);
            }

            public ListOfStructsSerializer<CapnpGen.GameModeInfo.WRITER> GameModes
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.GameModeInfo.WRITER>>(5);
                set => Link(5, value);
            }

            public ListOfPrimitivesSerializer<byte> AdminAddress
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(6);
                set => Link(6, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfe5a3ebea05cf7a8UL)]
    public class GameModeInfo : ICapnpSerializable
    {
        public const UInt64 typeId = 0xfe5a3ebea05cf7a8UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ModeId = reader.ModeId;
            Name = reader.Name;
            Description = reader.Description;
            MinPlayers = reader.MinPlayers;
            MaxPlayers = reader.MaxPlayers;
            DefaultRuleSet = reader.DefaultRuleSet;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ModeId.Init(ModeId);
            writer.Name = Name;
            writer.Description = Description;
            writer.MinPlayers = MinPlayers;
            writer.MaxPlayers = MaxPlayers;
            writer.DefaultRuleSet.Init(DefaultRuleSet);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> ModeId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public byte MinPlayers
        {
            get;
            set;
        }

        public byte MaxPlayers
        {
            get;
            set;
        }

        public IReadOnlyList<byte> DefaultRuleSet
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
            public IReadOnlyList<byte> ModeId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public string Description => ctx.ReadText(2, null);
            public byte MinPlayers => ctx.ReadDataByte(0UL, (byte)0);
            public byte MaxPlayers => ctx.ReadDataByte(8UL, (byte)0);
            public IReadOnlyList<byte> DefaultRuleSet => ctx.ReadList(3).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 4);
            }

            public ListOfPrimitivesSerializer<byte> ModeId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public string Description
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }

            public byte MinPlayers
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public byte MaxPlayers
            {
                get => this.ReadDataByte(8UL, (byte)0);
                set => this.WriteData(8UL, value, (byte)0);
            }

            public ListOfPrimitivesSerializer<byte> DefaultRuleSet
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x992278f11366082cUL)]
    public class ServiceEndpoint : ICapnpSerializable
    {
        public const UInt64 typeId = 0x992278f11366082cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ServiceType = reader.ServiceType;
            Url = reader.Url;
            ChainId = reader.ChainId;
            Version = reader.Version;
            LoadBalancer = reader.LoadBalancer;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ServiceType = ServiceType;
            writer.Url = Url;
            writer.ChainId = ChainId;
            writer.Version = Version;
            writer.LoadBalancer = LoadBalancer;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.ServiceType ServiceType
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public ulong ChainId
        {
            get;
            set;
        }

        public string Version
        {
            get;
            set;
        }

        public string LoadBalancer
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
            public CapnpGen.ServiceType ServiceType => (CapnpGen.ServiceType)ctx.ReadDataUShort(0UL, (ushort)0);
            public string Url => ctx.ReadText(0, null);
            public ulong ChainId => ctx.ReadDataULong(64UL, 0UL);
            public string Version => ctx.ReadText(1, null);
            public string LoadBalancer => ctx.ReadText(2, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 3);
            }

            public CapnpGen.ServiceType ServiceType
            {
                get => (CapnpGen.ServiceType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public string Url
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ulong ChainId
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public string Version
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public string LoadBalancer
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf5625b065e011fb6UL)]
    public enum ServiceType : ushort
    {
        matchmaking,
        playerProfiles,
        inventory,
        tournaments,
        security,
        telemetry,
        gameSession
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc10090961cbf358dUL)]
    public class ProtocolVersion : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc10090961cbf358dUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Major = reader.Major;
            Minor = reader.Minor;
            Patch = reader.Patch;
            Supported = reader.Supported;
            MinRequired = reader.MinRequired;
            Changelog = reader.Changelog;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Major = Major;
            writer.Minor = Minor;
            writer.Patch = Patch;
            writer.Supported = Supported;
            writer.MinRequired = MinRequired;
            writer.Changelog = Changelog;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ushort Major
        {
            get;
            set;
        }

        public ushort Minor
        {
            get;
            set;
        }

        public ushort Patch
        {
            get;
            set;
        }

        public bool Supported
        {
            get;
            set;
        }

        public bool MinRequired
        {
            get;
            set;
        }

        public string Changelog
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
            public ushort Major => ctx.ReadDataUShort(0UL, (ushort)0);
            public ushort Minor => ctx.ReadDataUShort(16UL, (ushort)0);
            public ushort Patch => ctx.ReadDataUShort(32UL, (ushort)0);
            public bool Supported => ctx.ReadDataBool(48UL, false);
            public bool MinRequired => ctx.ReadDataBool(49UL, false);
            public string Changelog => ctx.ReadText(0, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public ushort Major
            {
                get => this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, value, (ushort)0);
            }

            public ushort Minor
            {
                get => this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, value, (ushort)0);
            }

            public ushort Patch
            {
                get => this.ReadDataUShort(32UL, (ushort)0);
                set => this.WriteData(32UL, value, (ushort)0);
            }

            public bool Supported
            {
                get => this.ReadDataBool(48UL, false);
                set => this.WriteData(48UL, value, false);
            }

            public bool MinRequired
            {
                get => this.ReadDataBool(49UL, false);
                set => this.WriteData(49UL, value, false);
            }

            public string Changelog
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
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