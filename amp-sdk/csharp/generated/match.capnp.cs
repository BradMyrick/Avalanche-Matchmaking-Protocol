using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x94a871b56b1041c1UL)]
    public enum MatchType : ushort
    {
        turnBased,
        realTime
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xea4e1b245a95ea57UL)]
    public enum Region : ushort
    {
        na,
        eu,
        sa,
        @as
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9b439bdc23480c53UL)]
    public enum Elo : ushort
    {
        unranked,
        bronze,
        silver,
        gold,
        platinum,
        diamond,
        master,
        grandmaster
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb6df84125444afe7UL)]
    public class PaymentInfo : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb6df84125444afe7UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PayerWallet = reader.PayerWallet;
            FeeToken = reader.FeeToken;
            AuthSpend = reader.AuthSpend;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PayerWallet.Init(PayerWallet);
            writer.FeeToken.Init(FeeToken);
            writer.AuthSpend = AuthSpend;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> PayerWallet
        {
            get;
            set;
        }

        public IReadOnlyList<byte> FeeToken
        {
            get;
            set;
        }

        public ulong AuthSpend
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
            public IReadOnlyList<byte> PayerWallet => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> FeeToken => ctx.ReadList(1).CastByte();
            public ulong AuthSpend => ctx.ReadDataULong(0UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public ListOfPrimitivesSerializer<byte> PayerWallet
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> FeeToken
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ulong AuthSpend
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x950900d3a2976912UL)]
    public class PlayerInfo : ICapnpSerializable
    {
        public const UInt64 typeId = 0x950900d3a2976912UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            DisplayName = reader.DisplayName;
            PlayerWallet = reader.PlayerWallet;
            Elo = reader.Elo;
            Region = reader.Region;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId.Init(PlayerId);
            writer.DisplayName = DisplayName;
            writer.PlayerWallet.Init(PlayerWallet);
            writer.Elo = Elo;
            writer.Region = Region;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> PlayerId
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PlayerWallet
        {
            get;
            set;
        }

        public CapnpGen.Elo Elo
        {
            get;
            set;
        }

        public CapnpGen.Region Region
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
            public IReadOnlyList<byte> PlayerId => ctx.ReadList(0).CastByte();
            public string DisplayName => ctx.ReadText(1, null);
            public IReadOnlyList<byte> PlayerWallet => ctx.ReadList(2).CastByte();
            public CapnpGen.Elo Elo => (CapnpGen.Elo)ctx.ReadDataUShort(0UL, (ushort)0);
            public CapnpGen.Region Region => (CapnpGen.Region)ctx.ReadDataUShort(16UL, (ushort)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public ListOfPrimitivesSerializer<byte> PlayerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string DisplayName
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public ListOfPrimitivesSerializer<byte> PlayerWallet
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public CapnpGen.Elo Elo
            {
                get => (CapnpGen.Elo)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.Region Region
            {
                get => (CapnpGen.Region)this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, (ushort)value, (ushort)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbfec7f142f8f0d86UL)]
    public class GameMatchRequest : ICapnpSerializable
    {
        public const UInt64 typeId = 0xbfec7f142f8f0d86UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            RulesType = reader.RulesType;
            Stake = CapnpSerializable.Create<CapnpGen.PaymentInfo>(reader.Stake);
            PlayerInfo = CapnpSerializable.Create<CapnpGen.PlayerInfo>(reader.PlayerInfo);
            OptionalConfig = reader.OptionalConfig;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.RulesType = RulesType;
            Stake?.serialize(writer.Stake);
            PlayerInfo?.serialize(writer.PlayerInfo);
            writer.OptionalConfig.Init(OptionalConfig);
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

        public string RulesType
        {
            get;
            set;
        }

        public CapnpGen.PaymentInfo Stake
        {
            get;
            set;
        }

        public CapnpGen.PlayerInfo PlayerInfo
        {
            get;
            set;
        }

        public IReadOnlyList<byte> OptionalConfig
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
            public string RulesType => ctx.ReadText(1, null);
            public CapnpGen.PaymentInfo.READER Stake => ctx.ReadStruct(2, CapnpGen.PaymentInfo.READER.create);
            public CapnpGen.PlayerInfo.READER PlayerInfo => ctx.ReadStruct(3, CapnpGen.PlayerInfo.READER.create);
            public IReadOnlyList<byte> OptionalConfig => ctx.ReadList(4).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 5);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string RulesType
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public CapnpGen.PaymentInfo.WRITER Stake
            {
                get => BuildPointer<CapnpGen.PaymentInfo.WRITER>(2);
                set => Link(2, value);
            }

            public CapnpGen.PlayerInfo.WRITER PlayerInfo
            {
                get => BuildPointer<CapnpGen.PlayerInfo.WRITER>(3);
                set => Link(3, value);
            }

            public ListOfPrimitivesSerializer<byte> OptionalConfig
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa1dc4bc08af66044UL)]
    public class MatchAssignment : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa1dc4bc08af66044UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchId = reader.MatchId;
            Opponents = reader.Opponents?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PlayerInfo>(_));
            GameConfig = CapnpSerializable.Create<CapnpGen.MatchConfig>(reader.GameConfig);
            AssignedVerifier = reader.AssignedVerifier;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchId.Init(MatchId);
            writer.Opponents.Init(Opponents, (_s1, _v1) => _v1?.serialize(_s1));
            GameConfig?.serialize(writer.GameConfig);
            writer.AssignedVerifier.Init(AssignedVerifier);
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

        public IReadOnlyList<CapnpGen.PlayerInfo> Opponents
        {
            get;
            set;
        }

        public CapnpGen.MatchConfig GameConfig
        {
            get;
            set;
        }

        public IReadOnlyList<byte> AssignedVerifier
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
            public IReadOnlyList<CapnpGen.PlayerInfo.READER> Opponents => ctx.ReadList(1).Cast(CapnpGen.PlayerInfo.READER.create);
            public CapnpGen.MatchConfig.READER GameConfig => ctx.ReadStruct(2, CapnpGen.MatchConfig.READER.create);
            public IReadOnlyList<byte> AssignedVerifier => ctx.ReadList(3).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 4);
            }

            public ListOfPrimitivesSerializer<byte> MatchId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfStructsSerializer<CapnpGen.PlayerInfo.WRITER> Opponents
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.PlayerInfo.WRITER>>(1);
                set => Link(1, value);
            }

            public CapnpGen.MatchConfig.WRITER GameConfig
            {
                get => BuildPointer<CapnpGen.MatchConfig.WRITER>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> AssignedVerifier
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc07916f6f50a8052UL)]
    public enum OutcomeType : ushort
    {
        unknown,
        win,
        draw,
        @void
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa423c8403e5872c3UL)]
    public class Outcome : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa423c8403e5872c3UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Type = reader.Type;
            Scores = reader.Scores;
            Victor = reader.Victor;
            Metadata = reader.Metadata;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Type = Type;
            writer.Scores.Init(Scores);
            writer.Victor = Victor;
            writer.Metadata.Init(Metadata);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.OutcomeType Type
        {
            get;
            set;
        }

        public IReadOnlyList<ulong> Scores
        {
            get;
            set;
        }

        public byte Victor
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Metadata
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
            public CapnpGen.OutcomeType Type => (CapnpGen.OutcomeType)ctx.ReadDataUShort(0UL, (ushort)0);
            public IReadOnlyList<ulong> Scores => ctx.ReadList(0).CastULong();
            public byte Victor => ctx.ReadDataByte(16UL, (byte)0);
            public IReadOnlyList<byte> Metadata => ctx.ReadList(1).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public CapnpGen.OutcomeType Type
            {
                get => (CapnpGen.OutcomeType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<ulong> Scores
            {
                get => BuildPointer<ListOfPrimitivesSerializer<ulong>>(0);
                set => Link(0, value);
            }

            public byte Victor
            {
                get => this.ReadDataByte(16UL, (byte)0);
                set => this.WriteData(16UL, value, (byte)0);
            }

            public ListOfPrimitivesSerializer<byte> Metadata
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbc2691552f9e8d28UL)]
    public class OutcomeSubmission : ICapnpSerializable
    {
        public const UInt64 typeId = 0xbc2691552f9e8d28UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchId = reader.MatchId;
            Outcome = CapnpSerializable.Create<CapnpGen.Outcome>(reader.Outcome);
            ReplayHash = reader.ReplayHash;
            Signature = reader.Signature;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchId.Init(MatchId);
            Outcome?.serialize(writer.Outcome);
            writer.ReplayHash.Init(ReplayHash);
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

        public CapnpGen.Outcome Outcome
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ReplayHash
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
            public CapnpGen.Outcome.READER Outcome => ctx.ReadStruct(1, CapnpGen.Outcome.READER.create);
            public IReadOnlyList<byte> ReplayHash => ctx.ReadList(2).CastByte();
            public IReadOnlyList<byte> Signature => ctx.ReadList(3).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 4);
            }

            public ListOfPrimitivesSerializer<byte> MatchId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public CapnpGen.Outcome.WRITER Outcome
            {
                get => BuildPointer<CapnpGen.Outcome.WRITER>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> ReplayHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> Signature
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc889ba49438bfe75UL)]
    public class MatchTranscript : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc889ba49438bfe75UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchId = reader.MatchId;
            Events = reader.Events?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.GameEvent>(_));
            FinalStateHash = reader.FinalStateHash;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchId.Init(MatchId);
            writer.Events.Init(Events, (_s1, _v1) => _v1?.serialize(_s1));
            writer.FinalStateHash.Init(FinalStateHash);
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

        public IReadOnlyList<CapnpGen.GameEvent> Events
        {
            get;
            set;
        }

        public IReadOnlyList<byte> FinalStateHash
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
            public IReadOnlyList<CapnpGen.GameEvent.READER> Events => ctx.ReadList(1).Cast(CapnpGen.GameEvent.READER.create);
            public IReadOnlyList<byte> FinalStateHash => ctx.ReadList(2).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 3);
            }

            public ListOfPrimitivesSerializer<byte> MatchId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfStructsSerializer<CapnpGen.GameEvent.WRITER> Events
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.GameEvent.WRITER>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> FinalStateHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }
        }
    }
}