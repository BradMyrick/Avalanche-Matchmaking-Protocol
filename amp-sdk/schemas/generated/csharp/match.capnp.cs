using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcee7defaf95a04ffUL)]
    public enum MatchType : ushort
    {
        turnBased,
        realTime
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x87d8dbd4d7f7794aUL)]
    public enum Region : ushort
    {
        na,
        eu,
        sa,
        @as
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdc65064ee9628bb3UL)]
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe52c13deeedc27aaUL)]
    public class PaymentInfo : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe52c13deeedc27aaUL;
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa97026b3610e39fbUL)]
    public class PlayerInfo : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa97026b3610e39fbUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            DisplayName = reader.DisplayName;
            PlayerWallet = reader.PlayerWallet;
            Elo = reader.Elo;
            Region = reader.Region;
            ProfileId = reader.ProfileId;
            TeamId = reader.TeamId;
            LoadoutId = reader.LoadoutId;
            MmrRating = reader.MmrRating;
            MmrUncertainty = reader.MmrUncertainty;
            IsReady = reader.IsReady;
            Preferences = reader.Preferences;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId.Init(PlayerId);
            writer.DisplayName = DisplayName;
            writer.PlayerWallet.Init(PlayerWallet);
            writer.Elo = Elo;
            writer.Region = Region;
            writer.ProfileId.Init(ProfileId);
            writer.TeamId.Init(TeamId);
            writer.LoadoutId.Init(LoadoutId);
            writer.MmrRating = MmrRating;
            writer.MmrUncertainty = MmrUncertainty;
            writer.IsReady = IsReady;
            writer.Preferences.Init(Preferences);
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

        public IReadOnlyList<byte> ProfileId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TeamId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> LoadoutId
        {
            get;
            set;
        }

        public float MmrRating
        {
            get;
            set;
        }

        public float MmrUncertainty
        {
            get;
            set;
        }

        public bool IsReady
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Preferences
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
            public IReadOnlyList<byte> ProfileId => ctx.ReadList(3).CastByte();
            public IReadOnlyList<byte> TeamId => ctx.ReadList(4).CastByte();
            public IReadOnlyList<byte> LoadoutId => ctx.ReadList(5).CastByte();
            public float MmrRating => ctx.ReadDataFloat(32UL, 0F);
            public float MmrUncertainty => ctx.ReadDataFloat(64UL, 0F);
            public bool IsReady => ctx.ReadDataBool(96UL, false);
            public IReadOnlyList<byte> Preferences => ctx.ReadList(6).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 7);
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

            public ListOfPrimitivesSerializer<byte> ProfileId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public ListOfPrimitivesSerializer<byte> TeamId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public ListOfPrimitivesSerializer<byte> LoadoutId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(5);
                set => Link(5, value);
            }

            public float MmrRating
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float MmrUncertainty
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public bool IsReady
            {
                get => this.ReadDataBool(96UL, false);
                set => this.WriteData(96UL, value, false);
            }

            public ListOfPrimitivesSerializer<byte> Preferences
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(6);
                set => Link(6, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa6bef0fc8056327cUL)]
    public class GameMatchRequest : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa6bef0fc8056327cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            RulesType = reader.RulesType;
            Stake = CapnpSerializable.Create<CapnpGen.PaymentInfo>(reader.Stake);
            PlayerInfo = CapnpSerializable.Create<CapnpGen.PlayerInfo>(reader.PlayerInfo);
            OptionalConfig = reader.OptionalConfig;
            RuleSetId = reader.RuleSetId;
            MatchType = reader.MatchType;
            QueuePriority = reader.QueuePriority;
            CreationTime = reader.CreationTime;
            TimeoutMs = reader.TimeoutMs;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.RulesType = RulesType;
            Stake?.serialize(writer.Stake);
            PlayerInfo?.serialize(writer.PlayerInfo);
            writer.OptionalConfig.Init(OptionalConfig);
            writer.RuleSetId.Init(RuleSetId);
            writer.MatchType = MatchType;
            writer.QueuePriority = QueuePriority;
            writer.CreationTime = CreationTime;
            writer.TimeoutMs = TimeoutMs;
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

        public IReadOnlyList<byte> RuleSetId
        {
            get;
            set;
        }

        public CapnpGen.MatchType MatchType
        {
            get;
            set;
        }

        public byte QueuePriority
        {
            get;
            set;
        }

        public ulong CreationTime
        {
            get;
            set;
        }

        public ulong TimeoutMs
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
            public IReadOnlyList<byte> RuleSetId => ctx.ReadList(5).CastByte();
            public CapnpGen.MatchType MatchType => (CapnpGen.MatchType)ctx.ReadDataUShort(0UL, (ushort)0);
            public byte QueuePriority => ctx.ReadDataByte(16UL, (byte)0);
            public ulong CreationTime => ctx.ReadDataULong(64UL, 0UL);
            public ulong TimeoutMs => ctx.ReadDataULong(128UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 6);
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

            public ListOfPrimitivesSerializer<byte> RuleSetId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(5);
                set => Link(5, value);
            }

            public CapnpGen.MatchType MatchType
            {
                get => (CapnpGen.MatchType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public byte QueuePriority
            {
                get => this.ReadDataByte(16UL, (byte)0);
                set => this.WriteData(16UL, value, (byte)0);
            }

            public ulong CreationTime
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong TimeoutMs
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd3d0c460945637c7UL)]
    public class MatchAssignment : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd3d0c460945637c7UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchId = reader.MatchId;
            Opponents = reader.Opponents?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PlayerInfo>(_));
            GameConfig = CapnpSerializable.Create<CapnpGen.MatchConfig>(reader.GameConfig);
            AssignedVerifier = reader.AssignedVerifier;
            ServerAddress = reader.ServerAddress;
            ServerPort = reader.ServerPort;
            ConnectionToken = reader.ConnectionToken;
            Region = reader.Region;
            RuleSetId = reader.RuleSetId;
            MatchQuality = reader.MatchQuality;
            AssignmentTime = reader.AssignmentTime;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchId.Init(MatchId);
            writer.Opponents.Init(Opponents, (_s1, _v1) => _v1?.serialize(_s1));
            GameConfig?.serialize(writer.GameConfig);
            writer.AssignedVerifier.Init(AssignedVerifier);
            writer.ServerAddress = ServerAddress;
            writer.ServerPort = ServerPort;
            writer.ConnectionToken.Init(ConnectionToken);
            writer.Region = Region;
            writer.RuleSetId.Init(RuleSetId);
            writer.MatchQuality = MatchQuality;
            writer.AssignmentTime = AssignmentTime;
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

        public string ServerAddress
        {
            get;
            set;
        }

        public ushort ServerPort
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ConnectionToken
        {
            get;
            set;
        }

        public CapnpGen.Region Region
        {
            get;
            set;
        }

        public IReadOnlyList<byte> RuleSetId
        {
            get;
            set;
        }

        public float MatchQuality
        {
            get;
            set;
        }

        public ulong AssignmentTime
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
            public string ServerAddress => ctx.ReadText(4, null);
            public ushort ServerPort => ctx.ReadDataUShort(0UL, (ushort)0);
            public IReadOnlyList<byte> ConnectionToken => ctx.ReadList(5).CastByte();
            public CapnpGen.Region Region => (CapnpGen.Region)ctx.ReadDataUShort(16UL, (ushort)0);
            public IReadOnlyList<byte> RuleSetId => ctx.ReadList(6).CastByte();
            public float MatchQuality => ctx.ReadDataFloat(32UL, 0F);
            public ulong AssignmentTime => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 7);
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

            public string ServerAddress
            {
                get => this.ReadText(4, null);
                set => this.WriteText(4, value, null);
            }

            public ushort ServerPort
            {
                get => this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> ConnectionToken
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(5);
                set => Link(5, value);
            }

            public CapnpGen.Region Region
            {
                get => (CapnpGen.Region)this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> RuleSetId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(6);
                set => Link(6, value);
            }

            public float MatchQuality
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public ulong AssignmentTime
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf12606e2bde6c6acUL)]
    public enum OutcomeType : ushort
    {
        unknown,
        win,
        draw,
        @void
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9939a276ffbf6c6bUL)]
    public class Outcome : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9939a276ffbf6c6bUL;
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8d4d8f338f0b15c5UL)]
    public class OutcomeSubmission : ICapnpSerializable
    {
        public const UInt64 typeId = 0x8d4d8f338f0b15c5UL;
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd505cb1586e57721UL)]
    public class MatchTranscript : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd505cb1586e57721UL;
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf5b386d86c997ccbUL)]
    public class QueueStatistics : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf5b386d86c997ccbUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            RuleSetId = reader.RuleSetId;
            PlayersInQueue = reader.PlayersInQueue;
            EstimatedWaitMs = reader.EstimatedWaitMs;
            AvgQueueTimeMs = reader.AvgQueueTimeMs;
            MatchesPerHour = reader.MatchesPerHour;
            SuccessRate = reader.SuccessRate;
            AvgMatchQuality = reader.AvgMatchQuality;
            RegionBreakdown = reader.RegionBreakdown?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RegionStats>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.RuleSetId.Init(RuleSetId);
            writer.PlayersInQueue = PlayersInQueue;
            writer.EstimatedWaitMs = EstimatedWaitMs;
            writer.AvgQueueTimeMs = AvgQueueTimeMs;
            writer.MatchesPerHour = MatchesPerHour;
            writer.SuccessRate = SuccessRate;
            writer.AvgMatchQuality = AvgMatchQuality;
            writer.RegionBreakdown.Init(RegionBreakdown, (_s1, _v1) => _v1?.serialize(_s1));
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

        public IReadOnlyList<byte> RuleSetId
        {
            get;
            set;
        }

        public uint PlayersInQueue
        {
            get;
            set;
        }

        public ulong EstimatedWaitMs
        {
            get;
            set;
        }

        public ulong AvgQueueTimeMs
        {
            get;
            set;
        }

        public uint MatchesPerHour
        {
            get;
            set;
        }

        public float SuccessRate
        {
            get;
            set;
        }

        public float AvgMatchQuality
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.RegionStats> RegionBreakdown
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
            public IReadOnlyList<byte> RuleSetId => ctx.ReadList(1).CastByte();
            public uint PlayersInQueue => ctx.ReadDataUInt(0UL, 0U);
            public ulong EstimatedWaitMs => ctx.ReadDataULong(64UL, 0UL);
            public ulong AvgQueueTimeMs => ctx.ReadDataULong(128UL, 0UL);
            public uint MatchesPerHour => ctx.ReadDataUInt(32UL, 0U);
            public float SuccessRate => ctx.ReadDataFloat(192UL, 0F);
            public float AvgMatchQuality => ctx.ReadDataFloat(224UL, 0F);
            public IReadOnlyList<CapnpGen.RegionStats.READER> RegionBreakdown => ctx.ReadList(2).Cast(CapnpGen.RegionStats.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 3);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> RuleSetId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public uint PlayersInQueue
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public ulong EstimatedWaitMs
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong AvgQueueTimeMs
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public uint MatchesPerHour
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public float SuccessRate
            {
                get => this.ReadDataFloat(192UL, 0F);
                set => this.WriteData(192UL, value, 0F);
            }

            public float AvgMatchQuality
            {
                get => this.ReadDataFloat(224UL, 0F);
                set => this.WriteData(224UL, value, 0F);
            }

            public ListOfStructsSerializer<CapnpGen.RegionStats.WRITER> RegionBreakdown
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.RegionStats.WRITER>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x85460a83eaaa5d35UL)]
    public class RegionStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0x85460a83eaaa5d35UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Region = reader.Region;
            PlayerCount = reader.PlayerCount;
            AvgLatencyMs = reader.AvgLatencyMs;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Region = Region;
            writer.PlayerCount = PlayerCount;
            writer.AvgLatencyMs = AvgLatencyMs;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.Region Region
        {
            get;
            set;
        }

        public uint PlayerCount
        {
            get;
            set;
        }

        public uint AvgLatencyMs
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
            public CapnpGen.Region Region => (CapnpGen.Region)ctx.ReadDataUShort(0UL, (ushort)0);
            public uint PlayerCount => ctx.ReadDataUInt(32UL, 0U);
            public uint AvgLatencyMs => ctx.ReadDataUInt(64UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public CapnpGen.Region Region
            {
                get => (CapnpGen.Region)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public uint PlayerCount
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public uint AvgLatencyMs
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb05e94387740426eUL)]
    public class TimeRange : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb05e94387740426eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            StartTime = reader.StartTime;
            EndTime = reader.EndTime;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.StartTime = StartTime;
            writer.EndTime = EndTime;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong StartTime
        {
            get;
            set;
        }

        public ulong EndTime
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
            public ulong StartTime => ctx.ReadDataULong(0UL, 0UL);
            public ulong EndTime => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public ulong StartTime
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ulong EndTime
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x907f75e3e2ec183dUL)]
    public class TeamInfo : ICapnpSerializable
    {
        public const UInt64 typeId = 0x907f75e3e2ec183dUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TeamId = reader.TeamId;
            Name = reader.Name;
            Members = reader.Members?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PlayerInfo>(_));
            CaptainId = reader.CaptainId;
            Rating = reader.Rating;
            FormationTime = reader.FormationTime;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TeamId.Init(TeamId);
            writer.Name = Name;
            writer.Members.Init(Members, (_s1, _v1) => _v1?.serialize(_s1));
            writer.CaptainId.Init(CaptainId);
            writer.Rating = Rating;
            writer.FormationTime = FormationTime;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> TeamId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.PlayerInfo> Members
        {
            get;
            set;
        }

        public IReadOnlyList<byte> CaptainId
        {
            get;
            set;
        }

        public float Rating
        {
            get;
            set;
        }

        public ulong FormationTime
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
            public IReadOnlyList<byte> TeamId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public IReadOnlyList<CapnpGen.PlayerInfo.READER> Members => ctx.ReadList(2).Cast(CapnpGen.PlayerInfo.READER.create);
            public IReadOnlyList<byte> CaptainId => ctx.ReadList(3).CastByte();
            public float Rating => ctx.ReadDataFloat(0UL, 0F);
            public ulong FormationTime => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 4);
            }

            public ListOfPrimitivesSerializer<byte> TeamId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public ListOfStructsSerializer<CapnpGen.PlayerInfo.WRITER> Members
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.PlayerInfo.WRITER>>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> CaptainId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public float Rating
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public ulong FormationTime
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }
}