using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbf96a8ce004be5baUL), Proxy(typeof(RelayerService_Proxy)), Skeleton(typeof(RelayerService_Skeleton))]
    public interface IRelayerService : IDisposable
    {
        Task<IReadOnlyList<byte>> GetGameAdmin(ulong gameId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> GetCustodialAddress(ulong gameId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> SubmitOutcome(IReadOnlyList<byte> matchId, byte outcome, IReadOnlyList<byte> transcriptHash, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbf96a8ce004be5baUL)]
    public class RelayerService_Proxy : Proxy, IRelayerService
    {
        public async Task<IReadOnlyList<byte>> GetGameAdmin(ulong gameId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.RelayerService.Params_GetGameAdmin.WRITER>();
            var arg_ = new CapnpGen.RelayerService.Params_GetGameAdmin()
            {GameId = gameId};
            arg_?.serialize(in_);
            using (var d_ = await Call(13805407310472537530UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.RelayerService.Result_GetGameAdmin>(d_);
                return (r_.Admin);
            }
        }

        public async Task<IReadOnlyList<byte>> GetCustodialAddress(ulong gameId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.RelayerService.Params_GetCustodialAddress.WRITER>();
            var arg_ = new CapnpGen.RelayerService.Params_GetCustodialAddress()
            {GameId = gameId};
            arg_?.serialize(in_);
            using (var d_ = await Call(13805407310472537530UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.RelayerService.Result_GetCustodialAddress>(d_);
                return (r_.Address);
            }
        }

        public async Task<IReadOnlyList<byte>> SubmitOutcome(IReadOnlyList<byte> matchId, byte outcome, IReadOnlyList<byte> transcriptHash, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.RelayerService.Params_SubmitOutcome.WRITER>();
            var arg_ = new CapnpGen.RelayerService.Params_SubmitOutcome()
            {MatchId = matchId, Outcome = outcome, TranscriptHash = transcriptHash, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(13805407310472537530UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.RelayerService.Result_SubmitOutcome>(d_);
                return (r_.TxHash);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbf96a8ce004be5baUL)]
    public class RelayerService_Skeleton : Skeleton<IRelayerService>
    {
        public RelayerService_Skeleton()
        {
            SetMethodTable(GetGameAdmin, GetCustodialAddress, SubmitOutcome);
        }

        public override ulong InterfaceId => 13805407310472537530UL;
        Task<AnswerOrCounterquestion> GetGameAdmin(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.RelayerService.Params_GetGameAdmin>(d_);
                return Impatient.MaybeTailCall(Impl.GetGameAdmin(in_.GameId, cancellationToken_), admin =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.RelayerService.Result_GetGameAdmin.WRITER>();
                    var r_ = new CapnpGen.RelayerService.Result_GetGameAdmin{Admin = admin};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetCustodialAddress(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.RelayerService.Params_GetCustodialAddress>(d_);
                return Impatient.MaybeTailCall(Impl.GetCustodialAddress(in_.GameId, cancellationToken_), address =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.RelayerService.Result_GetCustodialAddress.WRITER>();
                    var r_ = new CapnpGen.RelayerService.Result_GetCustodialAddress{Address = address};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> SubmitOutcome(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.RelayerService.Params_SubmitOutcome>(d_);
                return Impatient.MaybeTailCall(Impl.SubmitOutcome(in_.MatchId, in_.Outcome, in_.TranscriptHash, in_.Signature, cancellationToken_), txHash =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.RelayerService.Result_SubmitOutcome.WRITER>();
                    var r_ = new CapnpGen.RelayerService.Result_SubmitOutcome{TxHash = txHash};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class RelayerService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9cd876bec863912dUL)]
        public class Params_GetGameAdmin : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9cd876bec863912dUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x95ff7bc19beed23aUL)]
        public class Result_GetGameAdmin : ICapnpSerializable
        {
            public const UInt64 typeId = 0x95ff7bc19beed23aUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Admin = reader.Admin;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Admin.Init(Admin);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> Admin
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
                public IReadOnlyList<byte> Admin => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> Admin
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdc996087776f548bUL)]
        public class Params_GetCustodialAddress : ICapnpSerializable
        {
            public const UInt64 typeId = 0xdc996087776f548bUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdadee121559729fdUL)]
        public class Result_GetCustodialAddress : ICapnpSerializable
        {
            public const UInt64 typeId = 0xdadee121559729fdUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Address = reader.Address;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Address.Init(Address);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> Address
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
                public IReadOnlyList<byte> Address => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> Address
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc344a2b6a3339ab6UL)]
        public class Params_SubmitOutcome : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc344a2b6a3339ab6UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                MatchId = reader.MatchId;
                Outcome = reader.Outcome;
                TranscriptHash = reader.TranscriptHash;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.MatchId.Init(MatchId);
                writer.Outcome = Outcome;
                writer.TranscriptHash.Init(TranscriptHash);
                writer.Signature.Init(Signature);
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

            public byte Outcome
            {
                get;
                set;
            }

            public IReadOnlyList<byte> TranscriptHash
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
                public IReadOnlyList<byte> MatchId => ctx.ReadList(0).CastByte();
                public byte Outcome => ctx.ReadDataByte(0UL, (byte)0);
                public IReadOnlyList<byte> TranscriptHash => ctx.ReadList(1).CastByte();
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 3);
                }

                public ListOfPrimitivesSerializer<byte> MatchId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public byte Outcome
                {
                    get => this.ReadDataByte(0UL, (byte)0);
                    set => this.WriteData(0UL, value, (byte)0);
                }

                public ListOfPrimitivesSerializer<byte> TranscriptHash
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd34678a07ecb91c0UL)]
        public class Result_SubmitOutcome : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd34678a07ecb91c0UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TxHash = reader.TxHash;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TxHash.Init(TxHash);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TxHash
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
                public IReadOnlyList<byte> TxHash => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> TxHash
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }
    }
}