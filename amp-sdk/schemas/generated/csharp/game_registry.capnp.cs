using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9512183cb024b577UL)]
    public class GameDeveloper : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9512183cb024b577UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            DeveloperId = reader.DeveloperId;
            Name = reader.Name;
            Wallet = reader.Wallet;
            ContactEmail = reader.ContactEmail;
            Website = reader.Website;
            IsVerified = reader.IsVerified;
            VerificationDate = reader.VerificationDate;
            Reputation = reader.Reputation;
            TotalGames = reader.TotalGames;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.DeveloperId.Init(DeveloperId);
            writer.Name = Name;
            writer.Wallet.Init(Wallet);
            writer.ContactEmail = ContactEmail;
            writer.Website = Website;
            writer.IsVerified = IsVerified;
            writer.VerificationDate = VerificationDate;
            writer.Reputation = Reputation;
            writer.TotalGames = TotalGames;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> DeveloperId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Wallet
        {
            get;
            set;
        }

        public string ContactEmail
        {
            get;
            set;
        }

        public string Website
        {
            get;
            set;
        }

        public bool IsVerified
        {
            get;
            set;
        }

        public ulong VerificationDate
        {
            get;
            set;
        }

        public float Reputation
        {
            get;
            set;
        }

        public uint TotalGames
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
            public IReadOnlyList<byte> DeveloperId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public IReadOnlyList<byte> Wallet => ctx.ReadList(2).CastByte();
            public string ContactEmail => ctx.ReadText(3, null);
            public string Website => ctx.ReadText(4, null);
            public bool IsVerified => ctx.ReadDataBool(0UL, false);
            public ulong VerificationDate => ctx.ReadDataULong(64UL, 0UL);
            public float Reputation => ctx.ReadDataFloat(32UL, 0F);
            public uint TotalGames => ctx.ReadDataUInt(128UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 5);
            }

            public ListOfPrimitivesSerializer<byte> DeveloperId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public ListOfPrimitivesSerializer<byte> Wallet
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public string ContactEmail
            {
                get => this.ReadText(3, null);
                set => this.WriteText(3, value, null);
            }

            public string Website
            {
                get => this.ReadText(4, null);
                set => this.WriteText(4, value, null);
            }

            public bool IsVerified
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public ulong VerificationDate
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public float Reputation
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public uint TotalGames
            {
                get => this.ReadDataUInt(128UL, 0U);
                set => this.WriteData(128UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf4a94944bd4110a7UL)]
    public class GameMode : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf4a94944bd4110a7UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ModeId = reader.ModeId;
            Name = reader.Name;
            Description = reader.Description;
            Rules = reader.Rules;
            IsRanked = reader.IsRanked;
            IsTournament = reader.IsTournament;
            IsCooperative = reader.IsCooperative;
            MinPlayers = reader.MinPlayers;
            MaxPlayers = reader.MaxPlayers;
            TeamCount = reader.TeamCount;
            PlayersPerTeam = reader.PlayersPerTeam;
            EstimatedTimeMs = reader.EstimatedTimeMs;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ModeId.Init(ModeId);
            writer.Name = Name;
            writer.Description = Description;
            writer.Rules.Init(Rules);
            writer.IsRanked = IsRanked;
            writer.IsTournament = IsTournament;
            writer.IsCooperative = IsCooperative;
            writer.MinPlayers = MinPlayers;
            writer.MaxPlayers = MaxPlayers;
            writer.TeamCount = TeamCount;
            writer.PlayersPerTeam.Init(PlayersPerTeam);
            writer.EstimatedTimeMs = EstimatedTimeMs;
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

        public IReadOnlyList<byte> Rules
        {
            get;
            set;
        }

        public bool IsRanked
        {
            get;
            set;
        }

        public bool IsTournament
        {
            get;
            set;
        }

        public bool IsCooperative
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

        public byte TeamCount
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PlayersPerTeam
        {
            get;
            set;
        }

        public ulong EstimatedTimeMs
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
            public IReadOnlyList<byte> Rules => ctx.ReadList(3).CastByte();
            public bool IsRanked => ctx.ReadDataBool(0UL, false);
            public bool IsTournament => ctx.ReadDataBool(1UL, false);
            public bool IsCooperative => ctx.ReadDataBool(2UL, false);
            public byte MinPlayers => ctx.ReadDataByte(8UL, (byte)0);
            public byte MaxPlayers => ctx.ReadDataByte(16UL, (byte)0);
            public byte TeamCount => ctx.ReadDataByte(24UL, (byte)0);
            public IReadOnlyList<byte> PlayersPerTeam => ctx.ReadList(4).CastByte();
            public ulong EstimatedTimeMs => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 5);
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

            public ListOfPrimitivesSerializer<byte> Rules
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public bool IsRanked
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public bool IsTournament
            {
                get => this.ReadDataBool(1UL, false);
                set => this.WriteData(1UL, value, false);
            }

            public bool IsCooperative
            {
                get => this.ReadDataBool(2UL, false);
                set => this.WriteData(2UL, value, false);
            }

            public byte MinPlayers
            {
                get => this.ReadDataByte(8UL, (byte)0);
                set => this.WriteData(8UL, value, (byte)0);
            }

            public byte MaxPlayers
            {
                get => this.ReadDataByte(16UL, (byte)0);
                set => this.WriteData(16UL, value, (byte)0);
            }

            public byte TeamCount
            {
                get => this.ReadDataByte(24UL, (byte)0);
                set => this.WriteData(24UL, value, (byte)0);
            }

            public ListOfPrimitivesSerializer<byte> PlayersPerTeam
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public ulong EstimatedTimeMs
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9a6ca74e4040d2fbUL)]
    public class GameConfiguration : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9a6ca74e4040d2fbUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            Name = reader.Name;
            Description = reader.Description;
            Version = reader.Version;
            DeveloperId = reader.DeveloperId;
            SupportedModes = reader.SupportedModes?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.GameMode>(_));
            EntryFeeModel = CapnpSerializable.Create<CapnpGen.FeeModel>(reader.EntryFeeModel);
            RewardModel = CapnpSerializable.Create<CapnpGen.RewardModel>(reader.RewardModel);
            ClientHash = reader.ClientHash;
            SupportedRegions = reader.SupportedRegions;
            MinClientVersion = reader.MinClientVersion;
            ServerRequirements = CapnpSerializable.Create<CapnpGen.ServerRequirements>(reader.ServerRequirements);
            Tags = reader.Tags;
            AgeRating = reader.AgeRating;
            LogoUrl = reader.LogoUrl;
            BannerUrl = reader.BannerUrl;
            Website = reader.Website;
            Status = reader.Status;
            TotalMatches = reader.TotalMatches;
            ActivePlayers = reader.ActivePlayers;
            AverageQueueTimeMs = reader.AverageQueueTimeMs;
            SmartContracts = CapnpSerializable.Create<CapnpGen.SmartContractConfig>(reader.SmartContracts);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.Name = Name;
            writer.Description = Description;
            writer.Version = Version;
            writer.DeveloperId.Init(DeveloperId);
            writer.SupportedModes.Init(SupportedModes, (_s1, _v1) => _v1?.serialize(_s1));
            EntryFeeModel?.serialize(writer.EntryFeeModel);
            RewardModel?.serialize(writer.RewardModel);
            writer.ClientHash.Init(ClientHash);
            writer.SupportedRegions.Init(SupportedRegions);
            writer.MinClientVersion = MinClientVersion;
            ServerRequirements?.serialize(writer.ServerRequirements);
            writer.Tags.Init(Tags);
            writer.AgeRating = AgeRating;
            writer.LogoUrl = LogoUrl;
            writer.BannerUrl = BannerUrl;
            writer.Website = Website;
            writer.Status = Status;
            writer.TotalMatches = TotalMatches;
            writer.ActivePlayers = ActivePlayers;
            writer.AverageQueueTimeMs = AverageQueueTimeMs;
            SmartContracts?.serialize(writer.SmartContracts);
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

        public string Version
        {
            get;
            set;
        }

        public IReadOnlyList<byte> DeveloperId
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.GameMode> SupportedModes
        {
            get;
            set;
        }

        public CapnpGen.FeeModel EntryFeeModel
        {
            get;
            set;
        }

        public CapnpGen.RewardModel RewardModel
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ClientHash
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Region> SupportedRegions
        {
            get;
            set;
        }

        public string MinClientVersion
        {
            get;
            set;
        }

        public CapnpGen.ServerRequirements ServerRequirements
        {
            get;
            set;
        }

        public IReadOnlyList<string> Tags
        {
            get;
            set;
        }

        public string AgeRating
        {
            get;
            set;
        }

        public string LogoUrl
        {
            get;
            set;
        }

        public string BannerUrl
        {
            get;
            set;
        }

        public string Website
        {
            get;
            set;
        }

        public CapnpGen.GameStatus Status
        {
            get;
            set;
        }

        public ulong TotalMatches
        {
            get;
            set;
        }

        public uint ActivePlayers
        {
            get;
            set;
        }

        public ulong AverageQueueTimeMs
        {
            get;
            set;
        }

        public CapnpGen.SmartContractConfig SmartContracts
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
            public string Name => ctx.ReadText(1, null);
            public string Description => ctx.ReadText(2, null);
            public string Version => ctx.ReadText(3, null);
            public IReadOnlyList<byte> DeveloperId => ctx.ReadList(4).CastByte();
            public IReadOnlyList<CapnpGen.GameMode.READER> SupportedModes => ctx.ReadList(5).Cast(CapnpGen.GameMode.READER.create);
            public CapnpGen.FeeModel.READER EntryFeeModel => ctx.ReadStruct(6, CapnpGen.FeeModel.READER.create);
            public CapnpGen.RewardModel.READER RewardModel => ctx.ReadStruct(7, CapnpGen.RewardModel.READER.create);
            public IReadOnlyList<byte> ClientHash => ctx.ReadList(8).CastByte();
            public IReadOnlyList<CapnpGen.Region> SupportedRegions => ctx.ReadList(9).CastEnums(_0 => (CapnpGen.Region)_0);
            public string MinClientVersion => ctx.ReadText(10, null);
            public CapnpGen.ServerRequirements.READER ServerRequirements => ctx.ReadStruct(11, CapnpGen.ServerRequirements.READER.create);
            public IReadOnlyList<string> Tags => ctx.ReadList(12).CastText2();
            public string AgeRating => ctx.ReadText(13, null);
            public string LogoUrl => ctx.ReadText(14, null);
            public string BannerUrl => ctx.ReadText(15, null);
            public string Website => ctx.ReadText(16, null);
            public CapnpGen.GameStatus Status => (CapnpGen.GameStatus)ctx.ReadDataUShort(0UL, (ushort)0);
            public ulong TotalMatches => ctx.ReadDataULong(64UL, 0UL);
            public uint ActivePlayers => ctx.ReadDataUInt(32UL, 0U);
            public ulong AverageQueueTimeMs => ctx.ReadDataULong(128UL, 0UL);
            public CapnpGen.SmartContractConfig.READER SmartContracts => ctx.ReadStruct(17, CapnpGen.SmartContractConfig.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 18);
            }

            public ListOfPrimitivesSerializer<byte> GameId
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

            public string Version
            {
                get => this.ReadText(3, null);
                set => this.WriteText(3, value, null);
            }

            public ListOfPrimitivesSerializer<byte> DeveloperId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public ListOfStructsSerializer<CapnpGen.GameMode.WRITER> SupportedModes
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.GameMode.WRITER>>(5);
                set => Link(5, value);
            }

            public CapnpGen.FeeModel.WRITER EntryFeeModel
            {
                get => BuildPointer<CapnpGen.FeeModel.WRITER>(6);
                set => Link(6, value);
            }

            public CapnpGen.RewardModel.WRITER RewardModel
            {
                get => BuildPointer<CapnpGen.RewardModel.WRITER>(7);
                set => Link(7, value);
            }

            public ListOfPrimitivesSerializer<byte> ClientHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(8);
                set => Link(8, value);
            }

            public ListOfPrimitivesSerializer<CapnpGen.Region> SupportedRegions
            {
                get => BuildPointer<ListOfPrimitivesSerializer<CapnpGen.Region>>(9);
                set => Link(9, value);
            }

            public string MinClientVersion
            {
                get => this.ReadText(10, null);
                set => this.WriteText(10, value, null);
            }

            public CapnpGen.ServerRequirements.WRITER ServerRequirements
            {
                get => BuildPointer<CapnpGen.ServerRequirements.WRITER>(11);
                set => Link(11, value);
            }

            public ListOfTextSerializer Tags
            {
                get => BuildPointer<ListOfTextSerializer>(12);
                set => Link(12, value);
            }

            public string AgeRating
            {
                get => this.ReadText(13, null);
                set => this.WriteText(13, value, null);
            }

            public string LogoUrl
            {
                get => this.ReadText(14, null);
                set => this.WriteText(14, value, null);
            }

            public string BannerUrl
            {
                get => this.ReadText(15, null);
                set => this.WriteText(15, value, null);
            }

            public string Website
            {
                get => this.ReadText(16, null);
                set => this.WriteText(16, value, null);
            }

            public CapnpGen.GameStatus Status
            {
                get => (CapnpGen.GameStatus)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ulong TotalMatches
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public uint ActivePlayers
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public ulong AverageQueueTimeMs
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public CapnpGen.SmartContractConfig.WRITER SmartContracts
            {
                get => BuildPointer<CapnpGen.SmartContractConfig.WRITER>(17);
                set => Link(17, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa7c0095f2ac6dfafUL)]
    public class ServerRequirements : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa7c0095f2ac6dfafUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            CpuCores = reader.CpuCores;
            RamMB = reader.RamMB;
            BandwidthMbps = reader.BandwidthMbps;
            Os = reader.Os;
            StorageMB = reader.StorageMB;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.CpuCores = CpuCores;
            writer.RamMB = RamMB;
            writer.BandwidthMbps = BandwidthMbps;
            writer.Os = Os;
            writer.StorageMB = StorageMB;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public byte CpuCores
        {
            get;
            set;
        }

        public uint RamMB
        {
            get;
            set;
        }

        public uint BandwidthMbps
        {
            get;
            set;
        }

        public string Os
        {
            get;
            set;
        }

        public uint StorageMB
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
            public byte CpuCores => ctx.ReadDataByte(0UL, (byte)0);
            public uint RamMB => ctx.ReadDataUInt(32UL, 0U);
            public uint BandwidthMbps => ctx.ReadDataUInt(64UL, 0U);
            public string Os => ctx.ReadText(0, null);
            public uint StorageMB => ctx.ReadDataUInt(96UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 1);
            }

            public byte CpuCores
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public uint RamMB
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public uint BandwidthMbps
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }

            public string Os
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public uint StorageMB
            {
                get => this.ReadDataUInt(96UL, 0U);
                set => this.WriteData(96UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc4a70b09f1f365d8UL)]
    public class FeeModel : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc4a70b09f1f365d8UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            BaseFee = reader.BaseFee;
            FeeToken = reader.FeeToken;
            FeeDistribution = CapnpSerializable.Create<CapnpGen.FeeDistribution>(reader.FeeDistribution);
            DeveloperCut = reader.DeveloperCut;
            ProtocolCut = reader.ProtocolCut;
            VerifierCut = reader.VerifierCut;
            StakingReward = reader.StakingReward;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.BaseFee = BaseFee;
            writer.FeeToken.Init(FeeToken);
            FeeDistribution?.serialize(writer.FeeDistribution);
            writer.DeveloperCut = DeveloperCut;
            writer.ProtocolCut = ProtocolCut;
            writer.VerifierCut = VerifierCut;
            writer.StakingReward = StakingReward;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong BaseFee
        {
            get;
            set;
        }

        public IReadOnlyList<byte> FeeToken
        {
            get;
            set;
        }

        public CapnpGen.FeeDistribution FeeDistribution
        {
            get;
            set;
        }

        public float DeveloperCut
        {
            get;
            set;
        }

        public float ProtocolCut
        {
            get;
            set;
        }

        public float VerifierCut
        {
            get;
            set;
        }

        public float StakingReward
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
            public ulong BaseFee => ctx.ReadDataULong(0UL, 0UL);
            public IReadOnlyList<byte> FeeToken => ctx.ReadList(0).CastByte();
            public CapnpGen.FeeDistribution.READER FeeDistribution => ctx.ReadStruct(1, CapnpGen.FeeDistribution.READER.create);
            public float DeveloperCut => ctx.ReadDataFloat(64UL, 0F);
            public float ProtocolCut => ctx.ReadDataFloat(96UL, 0F);
            public float VerifierCut => ctx.ReadDataFloat(128UL, 0F);
            public float StakingReward => ctx.ReadDataFloat(160UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 2);
            }

            public ulong BaseFee
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> FeeToken
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public CapnpGen.FeeDistribution.WRITER FeeDistribution
            {
                get => BuildPointer<CapnpGen.FeeDistribution.WRITER>(1);
                set => Link(1, value);
            }

            public float DeveloperCut
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public float ProtocolCut
            {
                get => this.ReadDataFloat(96UL, 0F);
                set => this.WriteData(96UL, value, 0F);
            }

            public float VerifierCut
            {
                get => this.ReadDataFloat(128UL, 0F);
                set => this.WriteData(128UL, value, 0F);
            }

            public float StakingReward
            {
                get => this.ReadDataFloat(160UL, 0F);
                set => this.WriteData(160UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x96f79ce8e83f1f78UL)]
    public class FeeDistribution : ICapnpSerializable
    {
        public const UInt64 typeId = 0x96f79ce8e83f1f78UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            WinnerPayout = reader.WinnerPayout;
            ParticipationReward = reader.ParticipationReward;
            PerformanceBonus = reader.PerformanceBonus;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.WinnerPayout = WinnerPayout;
            writer.ParticipationReward = ParticipationReward;
            writer.PerformanceBonus = PerformanceBonus;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float WinnerPayout
        {
            get;
            set;
        }

        public float ParticipationReward
        {
            get;
            set;
        }

        public float PerformanceBonus
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
            public float WinnerPayout => ctx.ReadDataFloat(0UL, 0F);
            public float ParticipationReward => ctx.ReadDataFloat(32UL, 0F);
            public float PerformanceBonus => ctx.ReadDataFloat(64UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public float WinnerPayout
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float ParticipationReward
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float PerformanceBonus
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x810a49ae869e5a4dUL)]
    public class RewardModel : ICapnpSerializable
    {
        public const UInt64 typeId = 0x810a49ae869e5a4dUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Type = reader.Type;
            TokenAddress = reader.TokenAddress;
            AmountPerWin = reader.AmountPerWin;
            AmountPerLoss = reader.AmountPerLoss;
            AmountPerDraw = reader.AmountPerDraw;
            BonusMultiplier = reader.BonusMultiplier;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Type = Type;
            writer.TokenAddress.Init(TokenAddress);
            writer.AmountPerWin = AmountPerWin;
            writer.AmountPerLoss = AmountPerLoss;
            writer.AmountPerDraw = AmountPerDraw;
            writer.BonusMultiplier = BonusMultiplier;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.RewardType Type
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TokenAddress
        {
            get;
            set;
        }

        public ulong AmountPerWin
        {
            get;
            set;
        }

        public ulong AmountPerLoss
        {
            get;
            set;
        }

        public ulong AmountPerDraw
        {
            get;
            set;
        }

        public float BonusMultiplier
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
            public CapnpGen.RewardType Type => (CapnpGen.RewardType)ctx.ReadDataUShort(0UL, (ushort)0);
            public IReadOnlyList<byte> TokenAddress => ctx.ReadList(0).CastByte();
            public ulong AmountPerWin => ctx.ReadDataULong(64UL, 0UL);
            public ulong AmountPerLoss => ctx.ReadDataULong(128UL, 0UL);
            public ulong AmountPerDraw => ctx.ReadDataULong(192UL, 0UL);
            public float BonusMultiplier => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 1);
            }

            public CapnpGen.RewardType Type
            {
                get => (CapnpGen.RewardType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> TokenAddress
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ulong AmountPerWin
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong AmountPerLoss
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ulong AmountPerDraw
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public float BonusMultiplier
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcea20b63ee2a93d0UL)]
    public enum RewardType : ushort
    {
        none,
        @fixed,
        dynamic,
        tournament,
        seasonal
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfdb8470811448655UL)]
    public class SmartContractConfig : ICapnpSerializable
    {
        public const UInt64 typeId = 0xfdb8470811448655UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameContract = reader.GameContract;
            NftContract = reader.NftContract;
            TokenContract = reader.TokenContract;
            RegistryContract = reader.RegistryContract;
            ChainId = reader.ChainId;
            SubnetId = reader.SubnetId;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameContract.Init(GameContract);
            writer.NftContract.Init(NftContract);
            writer.TokenContract.Init(TokenContract);
            writer.RegistryContract.Init(RegistryContract);
            writer.ChainId = ChainId;
            writer.SubnetId = SubnetId;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> GameContract
        {
            get;
            set;
        }

        public IReadOnlyList<byte> NftContract
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TokenContract
        {
            get;
            set;
        }

        public IReadOnlyList<byte> RegistryContract
        {
            get;
            set;
        }

        public ulong ChainId
        {
            get;
            set;
        }

        public string SubnetId
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
            public IReadOnlyList<byte> GameContract => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> NftContract => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> TokenContract => ctx.ReadList(2).CastByte();
            public IReadOnlyList<byte> RegistryContract => ctx.ReadList(3).CastByte();
            public ulong ChainId => ctx.ReadDataULong(0UL, 0UL);
            public string SubnetId => ctx.ReadText(4, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 5);
            }

            public ListOfPrimitivesSerializer<byte> GameContract
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> NftContract
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> TokenContract
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> RegistryContract
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public ulong ChainId
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public string SubnetId
            {
                get => this.ReadText(4, null);
                set => this.WriteText(4, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdd77d179457746a5UL)]
    public enum GameStatus : ushort
    {
        active,
        suspended,
        deprecated,
        maintenance,
        beta,
        alpha
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x811113049753d3c7UL)]
    public class GameStatistics : ICapnpSerializable
    {
        public const UInt64 typeId = 0x811113049753d3c7UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            TotalMatches = reader.TotalMatches;
            TotalPlayers = reader.TotalPlayers;
            TotalVolume = reader.TotalVolume;
            AvgMatchQuality = reader.AvgMatchQuality;
            PeakConcurrent = reader.PeakConcurrent;
            AvgQueueTimeMs = reader.AvgQueueTimeMs;
            RegionStats = reader.RegionStats?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RegionGameStats>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.TotalMatches = TotalMatches;
            writer.TotalPlayers = TotalPlayers;
            writer.TotalVolume = TotalVolume;
            writer.AvgMatchQuality = AvgMatchQuality;
            writer.PeakConcurrent = PeakConcurrent;
            writer.AvgQueueTimeMs = AvgQueueTimeMs;
            writer.RegionStats.Init(RegionStats, (_s1, _v1) => _v1?.serialize(_s1));
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

        public ulong TotalMatches
        {
            get;
            set;
        }

        public ulong TotalPlayers
        {
            get;
            set;
        }

        public ulong TotalVolume
        {
            get;
            set;
        }

        public float AvgMatchQuality
        {
            get;
            set;
        }

        public uint PeakConcurrent
        {
            get;
            set;
        }

        public ulong AvgQueueTimeMs
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.RegionGameStats> RegionStats
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
            public ulong TotalMatches => ctx.ReadDataULong(0UL, 0UL);
            public ulong TotalPlayers => ctx.ReadDataULong(64UL, 0UL);
            public ulong TotalVolume => ctx.ReadDataULong(128UL, 0UL);
            public float AvgMatchQuality => ctx.ReadDataFloat(192UL, 0F);
            public uint PeakConcurrent => ctx.ReadDataUInt(224UL, 0U);
            public ulong AvgQueueTimeMs => ctx.ReadDataULong(256UL, 0UL);
            public IReadOnlyList<CapnpGen.RegionGameStats.READER> RegionStats => ctx.ReadList(1).Cast(CapnpGen.RegionGameStats.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(5, 2);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ulong TotalMatches
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ulong TotalPlayers
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong TotalVolume
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public float AvgMatchQuality
            {
                get => this.ReadDataFloat(192UL, 0F);
                set => this.WriteData(192UL, value, 0F);
            }

            public uint PeakConcurrent
            {
                get => this.ReadDataUInt(224UL, 0U);
                set => this.WriteData(224UL, value, 0U);
            }

            public ulong AvgQueueTimeMs
            {
                get => this.ReadDataULong(256UL, 0UL);
                set => this.WriteData(256UL, value, 0UL);
            }

            public ListOfStructsSerializer<CapnpGen.RegionGameStats.WRITER> RegionStats
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.RegionGameStats.WRITER>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb5ab761a9ef3b4bcUL)]
    public class RegionGameStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb5ab761a9ef3b4bcUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Region = reader.Region;
            PlayerCount = reader.PlayerCount;
            MatchCount = reader.MatchCount;
            AvgLatencyMs = reader.AvgLatencyMs;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Region = Region;
            writer.PlayerCount = PlayerCount;
            writer.MatchCount = MatchCount;
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

        public ulong MatchCount
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
            public ulong MatchCount => ctx.ReadDataULong(64UL, 0UL);
            public uint AvgLatencyMs => ctx.ReadDataUInt(128UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 0);
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

            public ulong MatchCount
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public uint AvgLatencyMs
            {
                get => this.ReadDataUInt(128UL, 0U);
                set => this.WriteData(128UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcbe8a21c8007a582UL), Proxy(typeof(GameRegistryService_Proxy)), Skeleton(typeof(GameRegistryService_Skeleton))]
    public interface IGameRegistryService : IDisposable
    {
        Task<IReadOnlyList<byte>> RegisterGame(CapnpGen.GameConfiguration config, IReadOnlyList<byte> developerSig, CancellationToken cancellationToken_ = default);
        Task UpdateGame(IReadOnlyList<byte> gameId, CapnpGen.GameConfiguration config, IReadOnlyList<byte> developerSig, CancellationToken cancellationToken_ = default);
        Task SuspendGame(IReadOnlyList<byte> gameId, string reason, IReadOnlyList<byte> adminSig, CancellationToken cancellationToken_ = default);
        Task DeprecateGame(IReadOnlyList<byte> gameId, IReadOnlyList<byte> migrationTarget, IReadOnlyList<byte> adminSig, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.GameConfiguration> GetGameConfig(IReadOnlyList<byte> gameId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.GameConfiguration>> ListGames(CapnpGen.GameFilter filter, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.GameStatistics> GetGameStats(IReadOnlyList<byte> gameId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> RegisterDeveloper(CapnpGen.GameDeveloper developer, IReadOnlyList<byte> proof, CancellationToken cancellationToken_ = default);
        Task VerifyDeveloper(IReadOnlyList<byte> developerId, bool verified, IReadOnlyList<byte> adminSig, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.GameStatsEntry>> GetPopularGames(uint limit, CapnpGen.TimeRange timeRange, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcbe8a21c8007a582UL)]
    public class GameRegistryService_Proxy : Proxy, IGameRegistryService
    {
        public async Task<IReadOnlyList<byte>> RegisterGame(CapnpGen.GameConfiguration config, IReadOnlyList<byte> developerSig, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_RegisterGame.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_RegisterGame()
            {Config = config, DeveloperSig = developerSig};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_RegisterGame>(d_);
                return (r_.GameId);
            }
        }

        public async Task UpdateGame(IReadOnlyList<byte> gameId, CapnpGen.GameConfiguration config, IReadOnlyList<byte> developerSig, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_UpdateGame.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_UpdateGame()
            {GameId = gameId, Config = config, DeveloperSig = developerSig};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_UpdateGame>(d_);
                return;
            }
        }

        public async Task SuspendGame(IReadOnlyList<byte> gameId, string reason, IReadOnlyList<byte> adminSig, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_SuspendGame.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_SuspendGame()
            {GameId = gameId, Reason = reason, AdminSig = adminSig};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_SuspendGame>(d_);
                return;
            }
        }

        public async Task DeprecateGame(IReadOnlyList<byte> gameId, IReadOnlyList<byte> migrationTarget, IReadOnlyList<byte> adminSig, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_DeprecateGame.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_DeprecateGame()
            {GameId = gameId, MigrationTarget = migrationTarget, AdminSig = adminSig};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_DeprecateGame>(d_);
                return;
            }
        }

        public async Task<CapnpGen.GameConfiguration> GetGameConfig(IReadOnlyList<byte> gameId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_GetGameConfig.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_GetGameConfig()
            {GameId = gameId};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 4, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_GetGameConfig>(d_);
                return (r_.Config);
            }
        }

        public async Task<IReadOnlyList<CapnpGen.GameConfiguration>> ListGames(CapnpGen.GameFilter filter, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_ListGames.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_ListGames()
            {Filter = filter};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 5, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_ListGames>(d_);
                return (r_.Games);
            }
        }

        public async Task<CapnpGen.GameStatistics> GetGameStats(IReadOnlyList<byte> gameId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_GetGameStats.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_GetGameStats()
            {GameId = gameId};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 6, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_GetGameStats>(d_);
                return (r_.Stats);
            }
        }

        public async Task<IReadOnlyList<byte>> RegisterDeveloper(CapnpGen.GameDeveloper developer, IReadOnlyList<byte> proof, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_RegisterDeveloper.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_RegisterDeveloper()
            {Developer = developer, Proof = proof};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 7, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_RegisterDeveloper>(d_);
                return (r_.DeveloperId);
            }
        }

        public async Task VerifyDeveloper(IReadOnlyList<byte> developerId, bool verified, IReadOnlyList<byte> adminSig, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_VerifyDeveloper.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_VerifyDeveloper()
            {DeveloperId = developerId, Verified = verified, AdminSig = adminSig};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 8, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_VerifyDeveloper>(d_);
                return;
            }
        }

        public async Task<IReadOnlyList<CapnpGen.GameStatsEntry>> GetPopularGames(uint limit, CapnpGen.TimeRange timeRange, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Params_GetPopularGames.WRITER>();
            var arg_ = new CapnpGen.GameRegistryService.Params_GetPopularGames()
            {Limit = limit, TimeRange = timeRange};
            arg_?.serialize(in_);
            using (var d_ = await Call(14693172027587011970UL, 9, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Result_GetPopularGames>(d_);
                return (r_.Games);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcbe8a21c8007a582UL)]
    public class GameRegistryService_Skeleton : Skeleton<IGameRegistryService>
    {
        public GameRegistryService_Skeleton()
        {
            SetMethodTable(RegisterGame, UpdateGame, SuspendGame, DeprecateGame, GetGameConfig, ListGames, GetGameStats, RegisterDeveloper, VerifyDeveloper, GetPopularGames);
        }

        public override ulong InterfaceId => 14693172027587011970UL;
        Task<AnswerOrCounterquestion> RegisterGame(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_RegisterGame>(d_);
                return Impatient.MaybeTailCall(Impl.RegisterGame(in_.Config, in_.DeveloperSig, cancellationToken_), gameId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_RegisterGame.WRITER>();
                    var r_ = new CapnpGen.GameRegistryService.Result_RegisterGame{GameId = gameId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> UpdateGame(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_UpdateGame>(d_);
                await Impl.UpdateGame(in_.GameId, in_.Config, in_.DeveloperSig, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_UpdateGame.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> SuspendGame(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_SuspendGame>(d_);
                await Impl.SuspendGame(in_.GameId, in_.Reason, in_.AdminSig, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_SuspendGame.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> DeprecateGame(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_DeprecateGame>(d_);
                await Impl.DeprecateGame(in_.GameId, in_.MigrationTarget, in_.AdminSig, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_DeprecateGame.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GetGameConfig(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_GetGameConfig>(d_);
                return Impatient.MaybeTailCall(Impl.GetGameConfig(in_.GameId, cancellationToken_), config =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_GetGameConfig.WRITER>();
                    var r_ = new CapnpGen.GameRegistryService.Result_GetGameConfig{Config = config};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> ListGames(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_ListGames>(d_);
                return Impatient.MaybeTailCall(Impl.ListGames(in_.Filter, cancellationToken_), games =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_ListGames.WRITER>();
                    var r_ = new CapnpGen.GameRegistryService.Result_ListGames{Games = games};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetGameStats(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_GetGameStats>(d_);
                return Impatient.MaybeTailCall(Impl.GetGameStats(in_.GameId, cancellationToken_), stats =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_GetGameStats.WRITER>();
                    var r_ = new CapnpGen.GameRegistryService.Result_GetGameStats{Stats = stats};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> RegisterDeveloper(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_RegisterDeveloper>(d_);
                return Impatient.MaybeTailCall(Impl.RegisterDeveloper(in_.Developer, in_.Proof, cancellationToken_), developerId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_RegisterDeveloper.WRITER>();
                    var r_ = new CapnpGen.GameRegistryService.Result_RegisterDeveloper{DeveloperId = developerId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> VerifyDeveloper(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_VerifyDeveloper>(d_);
                await Impl.VerifyDeveloper(in_.DeveloperId, in_.Verified, in_.AdminSig, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_VerifyDeveloper.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GetPopularGames(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.GameRegistryService.Params_GetPopularGames>(d_);
                return Impatient.MaybeTailCall(Impl.GetPopularGames(in_.Limit, in_.TimeRange, cancellationToken_), games =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.GameRegistryService.Result_GetPopularGames.WRITER>();
                    var r_ = new CapnpGen.GameRegistryService.Result_GetPopularGames{Games = games};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class GameRegistryService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa8951f147f827347UL)]
        public class Params_RegisterGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa8951f147f827347UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Config = CapnpSerializable.Create<CapnpGen.GameConfiguration>(reader.Config);
                DeveloperSig = reader.DeveloperSig;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Config?.serialize(writer.Config);
                writer.DeveloperSig.Init(DeveloperSig);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.GameConfiguration Config
            {
                get;
                set;
            }

            public IReadOnlyList<byte> DeveloperSig
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
                public CapnpGen.GameConfiguration.READER Config => ctx.ReadStruct(0, CapnpGen.GameConfiguration.READER.create);
                public IReadOnlyList<byte> DeveloperSig => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.GameConfiguration.WRITER Config
                {
                    get => BuildPointer<CapnpGen.GameConfiguration.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> DeveloperSig
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaead1dccd0b34189UL)]
        public class Result_RegisterGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0xaead1dccd0b34189UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd3ea673832f0f497UL)]
        public class Params_UpdateGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd3ea673832f0f497UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameId = reader.GameId;
                Config = CapnpSerializable.Create<CapnpGen.GameConfiguration>(reader.Config);
                DeveloperSig = reader.DeveloperSig;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.GameId.Init(GameId);
                Config?.serialize(writer.Config);
                writer.DeveloperSig.Init(DeveloperSig);
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

            public CapnpGen.GameConfiguration Config
            {
                get;
                set;
            }

            public IReadOnlyList<byte> DeveloperSig
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
                public CapnpGen.GameConfiguration.READER Config => ctx.ReadStruct(1, CapnpGen.GameConfiguration.READER.create);
                public IReadOnlyList<byte> DeveloperSig => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> GameId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.GameConfiguration.WRITER Config
                {
                    get => BuildPointer<CapnpGen.GameConfiguration.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> DeveloperSig
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8f513157dc2fba37UL)]
        public class Result_UpdateGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8f513157dc2fba37UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd48304d27e522ac4UL)]
        public class Params_SuspendGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd48304d27e522ac4UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameId = reader.GameId;
                Reason = reader.Reason;
                AdminSig = reader.AdminSig;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.GameId.Init(GameId);
                writer.Reason = Reason;
                writer.AdminSig.Init(AdminSig);
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

            public string Reason
            {
                get;
                set;
            }

            public IReadOnlyList<byte> AdminSig
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
                public string Reason => ctx.ReadText(1, null);
                public IReadOnlyList<byte> AdminSig => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> GameId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public string Reason
                {
                    get => this.ReadText(1, null);
                    set => this.WriteText(1, value, null);
                }

                public ListOfPrimitivesSerializer<byte> AdminSig
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9dce0aa0926d2d29UL)]
        public class Result_SuspendGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9dce0aa0926d2d29UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xadaf4e9ddc80727dUL)]
        public class Params_DeprecateGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0xadaf4e9ddc80727dUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameId = reader.GameId;
                MigrationTarget = reader.MigrationTarget;
                AdminSig = reader.AdminSig;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.GameId.Init(GameId);
                writer.MigrationTarget.Init(MigrationTarget);
                writer.AdminSig.Init(AdminSig);
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

            public IReadOnlyList<byte> MigrationTarget
            {
                get;
                set;
            }

            public IReadOnlyList<byte> AdminSig
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
                public IReadOnlyList<byte> MigrationTarget => ctx.ReadList(1).CastByte();
                public IReadOnlyList<byte> AdminSig => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> GameId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> MigrationTarget
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> AdminSig
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd399966c777dbd6aUL)]
        public class Result_DeprecateGame : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd399966c777dbd6aUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8596f3e849352eb0UL)]
        public class Params_GetGameConfig : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8596f3e849352eb0UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb7d6f33b97cfdad7UL)]
        public class Result_GetGameConfig : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb7d6f33b97cfdad7UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Config = CapnpSerializable.Create<CapnpGen.GameConfiguration>(reader.Config);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Config?.serialize(writer.Config);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.GameConfiguration Config
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
                public CapnpGen.GameConfiguration.READER Config => ctx.ReadStruct(0, CapnpGen.GameConfiguration.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.GameConfiguration.WRITER Config
                {
                    get => BuildPointer<CapnpGen.GameConfiguration.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8609bdaa785e5041UL)]
        public class Params_ListGames : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8609bdaa785e5041UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Filter = CapnpSerializable.Create<CapnpGen.GameFilter>(reader.Filter);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Filter?.serialize(writer.Filter);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.GameFilter Filter
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
                public CapnpGen.GameFilter.READER Filter => ctx.ReadStruct(0, CapnpGen.GameFilter.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.GameFilter.WRITER Filter
                {
                    get => BuildPointer<CapnpGen.GameFilter.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9f1c33f206ce923fUL)]
        public class Result_ListGames : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9f1c33f206ce923fUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Games = reader.Games?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.GameConfiguration>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Games.Init(Games, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.GameConfiguration> Games
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
                public IReadOnlyList<CapnpGen.GameConfiguration.READER> Games => ctx.ReadList(0).Cast(CapnpGen.GameConfiguration.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.GameConfiguration.WRITER> Games
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.GameConfiguration.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf0434ab12e2c39c1UL)]
        public class Params_GetGameStats : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf0434ab12e2c39c1UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcee65e0e3d413bffUL)]
        public class Result_GetGameStats : ICapnpSerializable
        {
            public const UInt64 typeId = 0xcee65e0e3d413bffUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Stats = CapnpSerializable.Create<CapnpGen.GameStatistics>(reader.Stats);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Stats?.serialize(writer.Stats);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.GameStatistics Stats
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
                public CapnpGen.GameStatistics.READER Stats => ctx.ReadStruct(0, CapnpGen.GameStatistics.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.GameStatistics.WRITER Stats
                {
                    get => BuildPointer<CapnpGen.GameStatistics.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xacc06a791e0ee79dUL)]
        public class Params_RegisterDeveloper : ICapnpSerializable
        {
            public const UInt64 typeId = 0xacc06a791e0ee79dUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Developer = CapnpSerializable.Create<CapnpGen.GameDeveloper>(reader.Developer);
                Proof = reader.Proof;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Developer?.serialize(writer.Developer);
                writer.Proof.Init(Proof);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.GameDeveloper Developer
            {
                get;
                set;
            }

            public IReadOnlyList<byte> Proof
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
                public CapnpGen.GameDeveloper.READER Developer => ctx.ReadStruct(0, CapnpGen.GameDeveloper.READER.create);
                public IReadOnlyList<byte> Proof => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.GameDeveloper.WRITER Developer
                {
                    get => BuildPointer<CapnpGen.GameDeveloper.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Proof
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe27283f946f5b19aUL)]
        public class Result_RegisterDeveloper : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe27283f946f5b19aUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                DeveloperId = reader.DeveloperId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.DeveloperId.Init(DeveloperId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> DeveloperId
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
                public IReadOnlyList<byte> DeveloperId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> DeveloperId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf8383c40c68da280UL)]
        public class Params_VerifyDeveloper : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf8383c40c68da280UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                DeveloperId = reader.DeveloperId;
                Verified = reader.Verified;
                AdminSig = reader.AdminSig;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.DeveloperId.Init(DeveloperId);
                writer.Verified = Verified;
                writer.AdminSig.Init(AdminSig);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> DeveloperId
            {
                get;
                set;
            }

            public bool Verified
            {
                get;
                set;
            }

            public IReadOnlyList<byte> AdminSig
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
                public IReadOnlyList<byte> DeveloperId => ctx.ReadList(0).CastByte();
                public bool Verified => ctx.ReadDataBool(0UL, false);
                public IReadOnlyList<byte> AdminSig => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 2);
                }

                public ListOfPrimitivesSerializer<byte> DeveloperId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public bool Verified
                {
                    get => this.ReadDataBool(0UL, false);
                    set => this.WriteData(0UL, value, false);
                }

                public ListOfPrimitivesSerializer<byte> AdminSig
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf49c27ded8d5c1c8UL)]
        public class Result_VerifyDeveloper : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf49c27ded8d5c1c8UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb146aa6e0163e3dfUL)]
        public class Params_GetPopularGames : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb146aa6e0163e3dfUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Limit = reader.Limit;
                TimeRange = CapnpSerializable.Create<CapnpGen.TimeRange>(reader.TimeRange);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Limit = Limit;
                TimeRange?.serialize(writer.TimeRange);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public uint Limit
            {
                get;
                set;
            }

            public CapnpGen.TimeRange TimeRange
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
                public uint Limit => ctx.ReadDataUInt(0UL, 0U);
                public CapnpGen.TimeRange.READER TimeRange => ctx.ReadStruct(0, CapnpGen.TimeRange.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 1);
                }

                public uint Limit
                {
                    get => this.ReadDataUInt(0UL, 0U);
                    set => this.WriteData(0UL, value, 0U);
                }

                public CapnpGen.TimeRange.WRITER TimeRange
                {
                    get => BuildPointer<CapnpGen.TimeRange.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb600f3a8e5093034UL)]
        public class Result_GetPopularGames : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb600f3a8e5093034UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Games = reader.Games?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.GameStatsEntry>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Games.Init(Games, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.GameStatsEntry> Games
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
                public IReadOnlyList<CapnpGen.GameStatsEntry.READER> Games => ctx.ReadList(0).Cast(CapnpGen.GameStatsEntry.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.GameStatsEntry.WRITER> Games
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.GameStatsEntry.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x97f89b21cea459aeUL)]
    public class GameFilter : ICapnpSerializable
    {
        public const UInt64 typeId = 0x97f89b21cea459aeUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Tags = reader.Tags;
            MinPlayers = reader.MinPlayers;
            MaxPlayers = reader.MaxPlayers;
            IsRanked = reader.IsRanked;
            Region = reader.Region;
            Status = reader.Status;
            DeveloperId = reader.DeveloperId;
            HasRewards = reader.HasRewards;
            FreeToPlay = reader.FreeToPlay;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Tags.Init(Tags);
            writer.MinPlayers = MinPlayers;
            writer.MaxPlayers = MaxPlayers;
            writer.IsRanked = IsRanked;
            writer.Region = Region;
            writer.Status = Status;
            writer.DeveloperId.Init(DeveloperId);
            writer.HasRewards = HasRewards;
            writer.FreeToPlay = FreeToPlay;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<string> Tags
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

        public bool IsRanked
        {
            get;
            set;
        }

        public CapnpGen.Region Region
        {
            get;
            set;
        }

        public CapnpGen.GameStatus Status
        {
            get;
            set;
        }

        public IReadOnlyList<byte> DeveloperId
        {
            get;
            set;
        }

        public bool HasRewards
        {
            get;
            set;
        }

        public bool FreeToPlay
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
            public IReadOnlyList<string> Tags => ctx.ReadList(0).CastText2();
            public byte MinPlayers => ctx.ReadDataByte(0UL, (byte)0);
            public byte MaxPlayers => ctx.ReadDataByte(8UL, (byte)0);
            public bool IsRanked => ctx.ReadDataBool(16UL, false);
            public CapnpGen.Region Region => (CapnpGen.Region)ctx.ReadDataUShort(32UL, (ushort)0);
            public CapnpGen.GameStatus Status => (CapnpGen.GameStatus)ctx.ReadDataUShort(48UL, (ushort)0);
            public IReadOnlyList<byte> DeveloperId => ctx.ReadList(1).CastByte();
            public bool HasRewards => ctx.ReadDataBool(17UL, false);
            public bool FreeToPlay => ctx.ReadDataBool(18UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public ListOfTextSerializer Tags
            {
                get => BuildPointer<ListOfTextSerializer>(0);
                set => Link(0, value);
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

            public bool IsRanked
            {
                get => this.ReadDataBool(16UL, false);
                set => this.WriteData(16UL, value, false);
            }

            public CapnpGen.Region Region
            {
                get => (CapnpGen.Region)this.ReadDataUShort(32UL, (ushort)0);
                set => this.WriteData(32UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.GameStatus Status
            {
                get => (CapnpGen.GameStatus)this.ReadDataUShort(48UL, (ushort)0);
                set => this.WriteData(48UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> DeveloperId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public bool HasRewards
            {
                get => this.ReadDataBool(17UL, false);
                set => this.WriteData(17UL, value, false);
            }

            public bool FreeToPlay
            {
                get => this.ReadDataBool(18UL, false);
                set => this.WriteData(18UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe848f21626804711UL)]
    public class GameStatsEntry : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe848f21626804711UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            Name = reader.Name;
            PlayerCount = reader.PlayerCount;
            MatchCount = reader.MatchCount;
            Trend = reader.Trend;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.Name = Name;
            writer.PlayerCount = PlayerCount;
            writer.MatchCount = MatchCount;
            writer.Trend = Trend;
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

        public string Name
        {
            get;
            set;
        }

        public uint PlayerCount
        {
            get;
            set;
        }

        public ulong MatchCount
        {
            get;
            set;
        }

        public float Trend
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
            public string Name => ctx.ReadText(1, null);
            public uint PlayerCount => ctx.ReadDataUInt(0UL, 0U);
            public ulong MatchCount => ctx.ReadDataULong(64UL, 0UL);
            public float Trend => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 2);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public uint PlayerCount
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public ulong MatchCount
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public float Trend
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }
}