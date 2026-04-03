using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8d2b41f4867f1426UL)]
    public enum TournamentType : ushort
    {
        singleElimination,
        doubleElimination,
        roundRobin,
        swiss,
        gauntlet,
        battleRoyale,
        ladder,
        custom
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfc461fb1a0a8b419UL)]
    public enum TournamentStatus : ushort
    {
        scheduled,
        registration,
        checkIn,
        inProgress,
        completed,
        cancelled,
        paused
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa2cd0defdd7c757eUL)]
    public class Tournament : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa2cd0defdd7c757eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TournamentId = reader.TournamentId;
            Name = reader.Name;
            Description = reader.Description;
            Type = reader.Type;
            GameId = reader.GameId;
            ModeId = reader.ModeId;
            RuleSetId = reader.RuleSetId;
            RegistrationStart = reader.RegistrationStart;
            RegistrationEnd = reader.RegistrationEnd;
            CheckInStart = reader.CheckInStart;
            CheckInEnd = reader.CheckInEnd;
            TournamentStart = reader.TournamentStart;
            EstimatedEnd = reader.EstimatedEnd;
            MaxParticipants = reader.MaxParticipants;
            MinParticipants = reader.MinParticipants;
            TeamSize = reader.TeamSize;
            IsTeamTournament = reader.IsTeamTournament;
            EntryFee = CapnpSerializable.Create<CapnpGen.PaymentInfo>(reader.EntryFee);
            MinRating = reader.MinRating;
            MaxRating = reader.MaxRating;
            RegionRestrictions = reader.RegionRestrictions;
            ItemRequirements = reader.ItemRequirements;
            PrizePool = CapnpSerializable.Create<CapnpGen.PrizePool>(reader.PrizePool);
            PrizeDistribution = reader.PrizeDistribution?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PrizeTier>(_));
            Status = reader.Status;
            CurrentRound = reader.CurrentRound;
            TotalRounds = reader.TotalRounds;
            Participants = reader.Participants;
            CheckedIn = reader.CheckedIn;
            OrganizerId = reader.OrganizerId;
            LogoUrl = reader.LogoUrl;
            BannerUrl = reader.BannerUrl;
            RulesUrl = reader.RulesUrl;
            StreamUrl = reader.StreamUrl;
            TournamentContract = reader.TournamentContract;
            ChainId = reader.ChainId;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TournamentId.Init(TournamentId);
            writer.Name = Name;
            writer.Description = Description;
            writer.Type = Type;
            writer.GameId.Init(GameId);
            writer.ModeId.Init(ModeId);
            writer.RuleSetId.Init(RuleSetId);
            writer.RegistrationStart = RegistrationStart;
            writer.RegistrationEnd = RegistrationEnd;
            writer.CheckInStart = CheckInStart;
            writer.CheckInEnd = CheckInEnd;
            writer.TournamentStart = TournamentStart;
            writer.EstimatedEnd = EstimatedEnd;
            writer.MaxParticipants = MaxParticipants;
            writer.MinParticipants = MinParticipants;
            writer.TeamSize = TeamSize;
            writer.IsTeamTournament = IsTeamTournament;
            EntryFee?.serialize(writer.EntryFee);
            writer.MinRating = MinRating;
            writer.MaxRating = MaxRating;
            writer.RegionRestrictions.Init(RegionRestrictions);
            writer.ItemRequirements.Init(ItemRequirements);
            PrizePool?.serialize(writer.PrizePool);
            writer.PrizeDistribution.Init(PrizeDistribution, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Status = Status;
            writer.CurrentRound = CurrentRound;
            writer.TotalRounds = TotalRounds;
            writer.Participants = Participants;
            writer.CheckedIn = CheckedIn;
            writer.OrganizerId.Init(OrganizerId);
            writer.LogoUrl = LogoUrl;
            writer.BannerUrl = BannerUrl;
            writer.RulesUrl = RulesUrl;
            writer.StreamUrl = StreamUrl;
            writer.TournamentContract.Init(TournamentContract);
            writer.ChainId = ChainId;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> TournamentId
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

        public CapnpGen.TournamentType Type
        {
            get;
            set;
        }

        public IReadOnlyList<byte> GameId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ModeId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> RuleSetId
        {
            get;
            set;
        }

        public ulong RegistrationStart
        {
            get;
            set;
        }

        public ulong RegistrationEnd
        {
            get;
            set;
        }

        public ulong CheckInStart
        {
            get;
            set;
        }

        public ulong CheckInEnd
        {
            get;
            set;
        }

        public ulong TournamentStart
        {
            get;
            set;
        }

        public ulong EstimatedEnd
        {
            get;
            set;
        }

        public uint MaxParticipants
        {
            get;
            set;
        }

        public uint MinParticipants
        {
            get;
            set;
        }

        public byte TeamSize
        {
            get;
            set;
        }

        public bool IsTeamTournament
        {
            get;
            set;
        }

        public CapnpGen.PaymentInfo EntryFee
        {
            get;
            set;
        }

        public float MinRating
        {
            get;
            set;
        }

        public float MaxRating
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Region> RegionRestrictions
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ItemRequirements
        {
            get;
            set;
        }

        public CapnpGen.PrizePool PrizePool
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.PrizeTier> PrizeDistribution
        {
            get;
            set;
        }

        public CapnpGen.TournamentStatus Status
        {
            get;
            set;
        }

        public uint CurrentRound
        {
            get;
            set;
        }

        public uint TotalRounds
        {
            get;
            set;
        }

        public uint Participants
        {
            get;
            set;
        }

        public uint CheckedIn
        {
            get;
            set;
        }

        public IReadOnlyList<byte> OrganizerId
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

        public string RulesUrl
        {
            get;
            set;
        }

        public string StreamUrl
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TournamentContract
        {
            get;
            set;
        }

        public ulong ChainId
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
            public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public string Description => ctx.ReadText(2, null);
            public CapnpGen.TournamentType Type => (CapnpGen.TournamentType)ctx.ReadDataUShort(0UL, (ushort)0);
            public IReadOnlyList<byte> GameId => ctx.ReadList(3).CastByte();
            public IReadOnlyList<byte> ModeId => ctx.ReadList(4).CastByte();
            public IReadOnlyList<byte> RuleSetId => ctx.ReadList(5).CastByte();
            public ulong RegistrationStart => ctx.ReadDataULong(64UL, 0UL);
            public ulong RegistrationEnd => ctx.ReadDataULong(128UL, 0UL);
            public ulong CheckInStart => ctx.ReadDataULong(192UL, 0UL);
            public ulong CheckInEnd => ctx.ReadDataULong(256UL, 0UL);
            public ulong TournamentStart => ctx.ReadDataULong(320UL, 0UL);
            public ulong EstimatedEnd => ctx.ReadDataULong(384UL, 0UL);
            public uint MaxParticipants => ctx.ReadDataUInt(32UL, 0U);
            public uint MinParticipants => ctx.ReadDataUInt(448UL, 0U);
            public byte TeamSize => ctx.ReadDataByte(16UL, (byte)0);
            public bool IsTeamTournament => ctx.ReadDataBool(24UL, false);
            public CapnpGen.PaymentInfo.READER EntryFee => ctx.ReadStruct(6, CapnpGen.PaymentInfo.READER.create);
            public float MinRating => ctx.ReadDataFloat(480UL, 0F);
            public float MaxRating => ctx.ReadDataFloat(512UL, 0F);
            public IReadOnlyList<CapnpGen.Region> RegionRestrictions => ctx.ReadList(7).CastEnums(_0 => (CapnpGen.Region)_0);
            public IReadOnlyList<byte> ItemRequirements => ctx.ReadList(8).CastByte();
            public CapnpGen.PrizePool.READER PrizePool => ctx.ReadStruct(9, CapnpGen.PrizePool.READER.create);
            public IReadOnlyList<CapnpGen.PrizeTier.READER> PrizeDistribution => ctx.ReadList(10).Cast(CapnpGen.PrizeTier.READER.create);
            public CapnpGen.TournamentStatus Status => (CapnpGen.TournamentStatus)ctx.ReadDataUShort(544UL, (ushort)0);
            public uint CurrentRound => ctx.ReadDataUInt(576UL, 0U);
            public uint TotalRounds => ctx.ReadDataUInt(608UL, 0U);
            public uint Participants => ctx.ReadDataUInt(640UL, 0U);
            public uint CheckedIn => ctx.ReadDataUInt(672UL, 0U);
            public IReadOnlyList<byte> OrganizerId => ctx.ReadList(11).CastByte();
            public string LogoUrl => ctx.ReadText(12, null);
            public string BannerUrl => ctx.ReadText(13, null);
            public string RulesUrl => ctx.ReadText(14, null);
            public string StreamUrl => ctx.ReadText(15, null);
            public IReadOnlyList<byte> TournamentContract => ctx.ReadList(16).CastByte();
            public ulong ChainId => ctx.ReadDataULong(704UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(12, 17);
            }

            public ListOfPrimitivesSerializer<byte> TournamentId
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

            public CapnpGen.TournamentType Type
            {
                get => (CapnpGen.TournamentType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public ListOfPrimitivesSerializer<byte> ModeId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public ListOfPrimitivesSerializer<byte> RuleSetId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(5);
                set => Link(5, value);
            }

            public ulong RegistrationStart
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong RegistrationEnd
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ulong CheckInStart
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public ulong CheckInEnd
            {
                get => this.ReadDataULong(256UL, 0UL);
                set => this.WriteData(256UL, value, 0UL);
            }

            public ulong TournamentStart
            {
                get => this.ReadDataULong(320UL, 0UL);
                set => this.WriteData(320UL, value, 0UL);
            }

            public ulong EstimatedEnd
            {
                get => this.ReadDataULong(384UL, 0UL);
                set => this.WriteData(384UL, value, 0UL);
            }

            public uint MaxParticipants
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public uint MinParticipants
            {
                get => this.ReadDataUInt(448UL, 0U);
                set => this.WriteData(448UL, value, 0U);
            }

            public byte TeamSize
            {
                get => this.ReadDataByte(16UL, (byte)0);
                set => this.WriteData(16UL, value, (byte)0);
            }

            public bool IsTeamTournament
            {
                get => this.ReadDataBool(24UL, false);
                set => this.WriteData(24UL, value, false);
            }

            public CapnpGen.PaymentInfo.WRITER EntryFee
            {
                get => BuildPointer<CapnpGen.PaymentInfo.WRITER>(6);
                set => Link(6, value);
            }

            public float MinRating
            {
                get => this.ReadDataFloat(480UL, 0F);
                set => this.WriteData(480UL, value, 0F);
            }

            public float MaxRating
            {
                get => this.ReadDataFloat(512UL, 0F);
                set => this.WriteData(512UL, value, 0F);
            }

            public ListOfPrimitivesSerializer<CapnpGen.Region> RegionRestrictions
            {
                get => BuildPointer<ListOfPrimitivesSerializer<CapnpGen.Region>>(7);
                set => Link(7, value);
            }

            public ListOfPrimitivesSerializer<byte> ItemRequirements
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(8);
                set => Link(8, value);
            }

            public CapnpGen.PrizePool.WRITER PrizePool
            {
                get => BuildPointer<CapnpGen.PrizePool.WRITER>(9);
                set => Link(9, value);
            }

            public ListOfStructsSerializer<CapnpGen.PrizeTier.WRITER> PrizeDistribution
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.PrizeTier.WRITER>>(10);
                set => Link(10, value);
            }

            public CapnpGen.TournamentStatus Status
            {
                get => (CapnpGen.TournamentStatus)this.ReadDataUShort(544UL, (ushort)0);
                set => this.WriteData(544UL, (ushort)value, (ushort)0);
            }

            public uint CurrentRound
            {
                get => this.ReadDataUInt(576UL, 0U);
                set => this.WriteData(576UL, value, 0U);
            }

            public uint TotalRounds
            {
                get => this.ReadDataUInt(608UL, 0U);
                set => this.WriteData(608UL, value, 0U);
            }

            public uint Participants
            {
                get => this.ReadDataUInt(640UL, 0U);
                set => this.WriteData(640UL, value, 0U);
            }

            public uint CheckedIn
            {
                get => this.ReadDataUInt(672UL, 0U);
                set => this.WriteData(672UL, value, 0U);
            }

            public ListOfPrimitivesSerializer<byte> OrganizerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(11);
                set => Link(11, value);
            }

            public string LogoUrl
            {
                get => this.ReadText(12, null);
                set => this.WriteText(12, value, null);
            }

            public string BannerUrl
            {
                get => this.ReadText(13, null);
                set => this.WriteText(13, value, null);
            }

            public string RulesUrl
            {
                get => this.ReadText(14, null);
                set => this.WriteText(14, value, null);
            }

            public string StreamUrl
            {
                get => this.ReadText(15, null);
                set => this.WriteText(15, value, null);
            }

            public ListOfPrimitivesSerializer<byte> TournamentContract
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(16);
                set => Link(16, value);
            }

            public ulong ChainId
            {
                get => this.ReadDataULong(704UL, 0UL);
                set => this.WriteData(704UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd918c0bd482db503UL)]
    public class PrizePool : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd918c0bd482db503UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TotalValue = reader.TotalValue;
            CurrencyToken = reader.CurrencyToken;
            IsGuaranteed = reader.IsGuaranteed;
            ContributionBreakdown = reader.ContributionBreakdown?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PrizeContribution>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TotalValue = TotalValue;
            writer.CurrencyToken.Init(CurrencyToken);
            writer.IsGuaranteed = IsGuaranteed;
            writer.ContributionBreakdown.Init(ContributionBreakdown, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong TotalValue
        {
            get;
            set;
        }

        public IReadOnlyList<byte> CurrencyToken
        {
            get;
            set;
        }

        public bool IsGuaranteed
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.PrizeContribution> ContributionBreakdown
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
            public ulong TotalValue => ctx.ReadDataULong(0UL, 0UL);
            public IReadOnlyList<byte> CurrencyToken => ctx.ReadList(0).CastByte();
            public bool IsGuaranteed => ctx.ReadDataBool(64UL, false);
            public IReadOnlyList<CapnpGen.PrizeContribution.READER> ContributionBreakdown => ctx.ReadList(1).Cast(CapnpGen.PrizeContribution.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 2);
            }

            public ulong TotalValue
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> CurrencyToken
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public bool IsGuaranteed
            {
                get => this.ReadDataBool(64UL, false);
                set => this.WriteData(64UL, value, false);
            }

            public ListOfStructsSerializer<CapnpGen.PrizeContribution.WRITER> ContributionBreakdown
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.PrizeContribution.WRITER>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc3f696719516d64eUL)]
    public class PrizeContribution : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc3f696719516d64eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Contributor = reader.Contributor;
            Amount = reader.Amount;
            Purpose = reader.Purpose;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Contributor.Init(Contributor);
            writer.Amount = Amount;
            writer.Purpose = Purpose;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> Contributor
        {
            get;
            set;
        }

        public ulong Amount
        {
            get;
            set;
        }

        public string Purpose
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
            public IReadOnlyList<byte> Contributor => ctx.ReadList(0).CastByte();
            public ulong Amount => ctx.ReadDataULong(0UL, 0UL);
            public string Purpose => ctx.ReadText(1, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public ListOfPrimitivesSerializer<byte> Contributor
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ulong Amount
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public string Purpose
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfa73f9e4564282d2UL)]
    public class PrizeTier : ICapnpSerializable
    {
        public const UInt64 typeId = 0xfa73f9e4564282d2UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlaceStart = reader.PlaceStart;
            PlaceEnd = reader.PlaceEnd;
            Percentage = reader.Percentage;
            FixedAmount = reader.FixedAmount;
            PrizeItems = reader.PrizeItems?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PrizeItem>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlaceStart = PlaceStart;
            writer.PlaceEnd = PlaceEnd;
            writer.Percentage = Percentage;
            writer.FixedAmount = FixedAmount;
            writer.PrizeItems.Init(PrizeItems, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint PlaceStart
        {
            get;
            set;
        }

        public uint PlaceEnd
        {
            get;
            set;
        }

        public float Percentage
        {
            get;
            set;
        }

        public ulong FixedAmount
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.PrizeItem> PrizeItems
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
            public uint PlaceStart => ctx.ReadDataUInt(0UL, 0U);
            public uint PlaceEnd => ctx.ReadDataUInt(32UL, 0U);
            public float Percentage => ctx.ReadDataFloat(64UL, 0F);
            public ulong FixedAmount => ctx.ReadDataULong(128UL, 0UL);
            public IReadOnlyList<CapnpGen.PrizeItem.READER> PrizeItems => ctx.ReadList(0).Cast(CapnpGen.PrizeItem.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 1);
            }

            public uint PlaceStart
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public uint PlaceEnd
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public float Percentage
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public ulong FixedAmount
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ListOfStructsSerializer<CapnpGen.PrizeItem.WRITER> PrizeItems
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.PrizeItem.WRITER>>(0);
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x992ccb4008546e3bUL)]
    public class PrizeItem : ICapnpSerializable
    {
        public const UInt64 typeId = 0x992ccb4008546e3bUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ItemId = reader.ItemId;
            Quantity = reader.Quantity;
            IsBound = reader.IsBound;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ItemId.Init(ItemId);
            writer.Quantity = Quantity;
            writer.IsBound = IsBound;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> ItemId
        {
            get;
            set;
        }

        public uint Quantity
        {
            get;
            set;
        }

        public bool IsBound
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
            public IReadOnlyList<byte> ItemId => ctx.ReadList(0).CastByte();
            public uint Quantity => ctx.ReadDataUInt(0UL, 0U);
            public bool IsBound => ctx.ReadDataBool(32UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public ListOfPrimitivesSerializer<byte> ItemId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public uint Quantity
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public bool IsBound
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc0cf5f51d40753b2UL)]
    public class TournamentRegistration : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc0cf5f51d40753b2UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RegistrationId = reader.RegistrationId;
            TournamentId = reader.TournamentId;
            TeamId = reader.TeamId;
            PlayerIds = reader.PlayerIds;
            CaptainId = reader.CaptainId;
            RegisteredAt = reader.RegisteredAt;
            CheckedIn = reader.CheckedIn;
            CheckedInAt = reader.CheckedInAt;
            PaidEntryFee = reader.PaidEntryFee;
            PaymentTxHash = reader.PaymentTxHash;
            Seed = reader.Seed;
            InitialRating = reader.InitialRating;
            IsActive = reader.IsActive;
            Disqualification = CapnpSerializable.Create<CapnpGen.Disqualification>(reader.Disqualification);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RegistrationId.Init(RegistrationId);
            writer.TournamentId.Init(TournamentId);
            writer.TeamId.Init(TeamId);
            writer.PlayerIds.Init(PlayerIds, (_s1, _v1) => _s1.Init(_v1));
            writer.CaptainId.Init(CaptainId);
            writer.RegisteredAt = RegisteredAt;
            writer.CheckedIn = CheckedIn;
            writer.CheckedInAt = CheckedInAt;
            writer.PaidEntryFee = PaidEntryFee;
            writer.PaymentTxHash.Init(PaymentTxHash);
            writer.Seed = Seed;
            writer.InitialRating = InitialRating;
            writer.IsActive = IsActive;
            Disqualification?.serialize(writer.Disqualification);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> RegistrationId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TournamentId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TeamId
        {
            get;
            set;
        }

        public IReadOnlyList<IReadOnlyList<byte>> PlayerIds
        {
            get;
            set;
        }

        public IReadOnlyList<byte> CaptainId
        {
            get;
            set;
        }

        public ulong RegisteredAt
        {
            get;
            set;
        }

        public bool CheckedIn
        {
            get;
            set;
        }

        public ulong CheckedInAt
        {
            get;
            set;
        }

        public bool PaidEntryFee
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PaymentTxHash
        {
            get;
            set;
        }

        public uint Seed
        {
            get;
            set;
        }

        public float InitialRating
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public CapnpGen.Disqualification Disqualification
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
            public IReadOnlyList<byte> RegistrationId => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> TournamentId => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> TeamId => ctx.ReadList(2).CastByte();
            public IReadOnlyList<IReadOnlyList<byte>> PlayerIds => ctx.ReadList(3).CastData();
            public IReadOnlyList<byte> CaptainId => ctx.ReadList(4).CastByte();
            public ulong RegisteredAt => ctx.ReadDataULong(0UL, 0UL);
            public bool CheckedIn => ctx.ReadDataBool(64UL, false);
            public ulong CheckedInAt => ctx.ReadDataULong(128UL, 0UL);
            public bool PaidEntryFee => ctx.ReadDataBool(65UL, false);
            public IReadOnlyList<byte> PaymentTxHash => ctx.ReadList(5).CastByte();
            public uint Seed => ctx.ReadDataUInt(96UL, 0U);
            public float InitialRating => ctx.ReadDataFloat(192UL, 0F);
            public bool IsActive => ctx.ReadDataBool(66UL, false);
            public CapnpGen.Disqualification.READER Disqualification => ctx.ReadStruct(6, CapnpGen.Disqualification.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 7);
            }

            public ListOfPrimitivesSerializer<byte> RegistrationId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> TournamentId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> TeamId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> PlayerIds
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(3);
                set => Link(3, value);
            }

            public ListOfPrimitivesSerializer<byte> CaptainId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public ulong RegisteredAt
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public bool CheckedIn
            {
                get => this.ReadDataBool(64UL, false);
                set => this.WriteData(64UL, value, false);
            }

            public ulong CheckedInAt
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public bool PaidEntryFee
            {
                get => this.ReadDataBool(65UL, false);
                set => this.WriteData(65UL, value, false);
            }

            public ListOfPrimitivesSerializer<byte> PaymentTxHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(5);
                set => Link(5, value);
            }

            public uint Seed
            {
                get => this.ReadDataUInt(96UL, 0U);
                set => this.WriteData(96UL, value, 0U);
            }

            public float InitialRating
            {
                get => this.ReadDataFloat(192UL, 0F);
                set => this.WriteData(192UL, value, 0F);
            }

            public bool IsActive
            {
                get => this.ReadDataBool(66UL, false);
                set => this.WriteData(66UL, value, false);
            }

            public CapnpGen.Disqualification.WRITER Disqualification
            {
                get => BuildPointer<CapnpGen.Disqualification.WRITER>(6);
                set => Link(6, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc793716ea1268f54UL)]
    public class Disqualification : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc793716ea1268f54UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            IsDisqualified = reader.IsDisqualified;
            Reason = reader.Reason;
            DisqualifiedAt = reader.DisqualifiedAt;
            DisqualifiedBy = reader.DisqualifiedBy;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.IsDisqualified = IsDisqualified;
            writer.Reason = Reason;
            writer.DisqualifiedAt = DisqualifiedAt;
            writer.DisqualifiedBy.Init(DisqualifiedBy);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool IsDisqualified
        {
            get;
            set;
        }

        public string Reason
        {
            get;
            set;
        }

        public ulong DisqualifiedAt
        {
            get;
            set;
        }

        public IReadOnlyList<byte> DisqualifiedBy
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
            public bool IsDisqualified => ctx.ReadDataBool(0UL, false);
            public string Reason => ctx.ReadText(0, null);
            public ulong DisqualifiedAt => ctx.ReadDataULong(64UL, 0UL);
            public IReadOnlyList<byte> DisqualifiedBy => ctx.ReadList(1).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 2);
            }

            public bool IsDisqualified
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public string Reason
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ulong DisqualifiedAt
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> DisqualifiedBy
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x889e62cebd4e1189UL)]
    public class Bracket : ICapnpSerializable
    {
        public const UInt64 typeId = 0x889e62cebd4e1189UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            BracketId = reader.BracketId;
            TournamentId = reader.TournamentId;
            Type = reader.Type;
            Rounds = reader.Rounds?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Round>(_));
            Matches = reader.Matches?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.TournamentMatch>(_));
            CurrentRound = reader.CurrentRound;
            IsFinalized = reader.IsFinalized;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.BracketId.Init(BracketId);
            writer.TournamentId.Init(TournamentId);
            writer.Type = Type;
            writer.Rounds.Init(Rounds, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Matches.Init(Matches, (_s1, _v1) => _v1?.serialize(_s1));
            writer.CurrentRound = CurrentRound;
            writer.IsFinalized = IsFinalized;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> BracketId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TournamentId
        {
            get;
            set;
        }

        public CapnpGen.TournamentType Type
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Round> Rounds
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.TournamentMatch> Matches
        {
            get;
            set;
        }

        public uint CurrentRound
        {
            get;
            set;
        }

        public bool IsFinalized
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
            public IReadOnlyList<byte> BracketId => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> TournamentId => ctx.ReadList(1).CastByte();
            public CapnpGen.TournamentType Type => (CapnpGen.TournamentType)ctx.ReadDataUShort(0UL, (ushort)0);
            public IReadOnlyList<CapnpGen.Round.READER> Rounds => ctx.ReadList(2).Cast(CapnpGen.Round.READER.create);
            public IReadOnlyList<CapnpGen.TournamentMatch.READER> Matches => ctx.ReadList(3).Cast(CapnpGen.TournamentMatch.READER.create);
            public uint CurrentRound => ctx.ReadDataUInt(32UL, 0U);
            public bool IsFinalized => ctx.ReadDataBool(16UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 4);
            }

            public ListOfPrimitivesSerializer<byte> BracketId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> TournamentId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public CapnpGen.TournamentType Type
            {
                get => (CapnpGen.TournamentType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ListOfStructsSerializer<CapnpGen.Round.WRITER> Rounds
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.Round.WRITER>>(2);
                set => Link(2, value);
            }

            public ListOfStructsSerializer<CapnpGen.TournamentMatch.WRITER> Matches
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.TournamentMatch.WRITER>>(3);
                set => Link(3, value);
            }

            public uint CurrentRound
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public bool IsFinalized
            {
                get => this.ReadDataBool(16UL, false);
                set => this.WriteData(16UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd390fb7653960bc6UL)]
    public class Round : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd390fb7653960bc6UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RoundNumber = reader.RoundNumber;
            Name = reader.Name;
            Matches = reader.Matches;
            StartTime = reader.StartTime;
            EndTime = reader.EndTime;
            IsCompleted = reader.IsCompleted;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RoundNumber = RoundNumber;
            writer.Name = Name;
            writer.Matches.Init(Matches, (_s1, _v1) => _s1.Init(_v1));
            writer.StartTime = StartTime;
            writer.EndTime = EndTime;
            writer.IsCompleted = IsCompleted;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint RoundNumber
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IReadOnlyList<IReadOnlyList<byte>> Matches
        {
            get;
            set;
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

        public bool IsCompleted
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
            public uint RoundNumber => ctx.ReadDataUInt(0UL, 0U);
            public string Name => ctx.ReadText(0, null);
            public IReadOnlyList<IReadOnlyList<byte>> Matches => ctx.ReadList(1).CastData();
            public ulong StartTime => ctx.ReadDataULong(64UL, 0UL);
            public ulong EndTime => ctx.ReadDataULong(128UL, 0UL);
            public bool IsCompleted => ctx.ReadDataBool(32UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 2);
            }

            public uint RoundNumber
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public string Name
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> Matches
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(1);
                set => Link(1, value);
            }

            public ulong StartTime
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong EndTime
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public bool IsCompleted
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xef7aff7dd63d066eUL)]
    public class TournamentMatch : ICapnpSerializable
    {
        public const UInt64 typeId = 0xef7aff7dd63d066eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchId = reader.MatchId;
            TournamentId = reader.TournamentId;
            RoundNumber = reader.RoundNumber;
            Team1Id = reader.Team1Id;
            Team2Id = reader.Team2Id;
            Team1Name = reader.Team1Name;
            Team2Name = reader.Team2Name;
            ScheduledTime = reader.ScheduledTime;
            EstimatedDuration = reader.EstimatedDuration;
            GameServer = CapnpSerializable.Create<CapnpGen.GameServer>(reader.GameServer);
            StreamUrl = reader.StreamUrl;
            WinnerId = reader.WinnerId;
            LoserId = reader.LoserId;
            IsDraw = reader.IsDraw;
            Scores = reader.Scores;
            CompletedAt = reader.CompletedAt;
            Status = reader.Status;
            Forfeit = CapnpSerializable.Create<CapnpGen.Forfeit>(reader.Forfeit);
            Dispute = CapnpSerializable.Create<CapnpGen.Dispute>(reader.Dispute);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchId.Init(MatchId);
            writer.TournamentId.Init(TournamentId);
            writer.RoundNumber = RoundNumber;
            writer.Team1Id.Init(Team1Id);
            writer.Team2Id.Init(Team2Id);
            writer.Team1Name = Team1Name;
            writer.Team2Name = Team2Name;
            writer.ScheduledTime = ScheduledTime;
            writer.EstimatedDuration = EstimatedDuration;
            GameServer?.serialize(writer.GameServer);
            writer.StreamUrl = StreamUrl;
            writer.WinnerId.Init(WinnerId);
            writer.LoserId.Init(LoserId);
            writer.IsDraw = IsDraw;
            writer.Scores.Init(Scores);
            writer.CompletedAt = CompletedAt;
            writer.Status = Status;
            Forfeit?.serialize(writer.Forfeit);
            Dispute?.serialize(writer.Dispute);
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

        public IReadOnlyList<byte> TournamentId
        {
            get;
            set;
        }

        public uint RoundNumber
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Team1Id
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Team2Id
        {
            get;
            set;
        }

        public string Team1Name
        {
            get;
            set;
        }

        public string Team2Name
        {
            get;
            set;
        }

        public ulong ScheduledTime
        {
            get;
            set;
        }

        public ulong EstimatedDuration
        {
            get;
            set;
        }

        public CapnpGen.GameServer GameServer
        {
            get;
            set;
        }

        public string StreamUrl
        {
            get;
            set;
        }

        public IReadOnlyList<byte> WinnerId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> LoserId
        {
            get;
            set;
        }

        public bool IsDraw
        {
            get;
            set;
        }

        public IReadOnlyList<ulong> Scores
        {
            get;
            set;
        }

        public ulong CompletedAt
        {
            get;
            set;
        }

        public CapnpGen.MatchStatus Status
        {
            get;
            set;
        }

        public CapnpGen.Forfeit Forfeit
        {
            get;
            set;
        }

        public CapnpGen.Dispute Dispute
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
            public IReadOnlyList<byte> TournamentId => ctx.ReadList(1).CastByte();
            public uint RoundNumber => ctx.ReadDataUInt(0UL, 0U);
            public IReadOnlyList<byte> Team1Id => ctx.ReadList(2).CastByte();
            public IReadOnlyList<byte> Team2Id => ctx.ReadList(3).CastByte();
            public string Team1Name => ctx.ReadText(4, null);
            public string Team2Name => ctx.ReadText(5, null);
            public ulong ScheduledTime => ctx.ReadDataULong(64UL, 0UL);
            public ulong EstimatedDuration => ctx.ReadDataULong(128UL, 0UL);
            public CapnpGen.GameServer.READER GameServer => ctx.ReadStruct(6, CapnpGen.GameServer.READER.create);
            public string StreamUrl => ctx.ReadText(7, null);
            public IReadOnlyList<byte> WinnerId => ctx.ReadList(8).CastByte();
            public IReadOnlyList<byte> LoserId => ctx.ReadList(9).CastByte();
            public bool IsDraw => ctx.ReadDataBool(32UL, false);
            public IReadOnlyList<ulong> Scores => ctx.ReadList(10).CastULong();
            public ulong CompletedAt => ctx.ReadDataULong(192UL, 0UL);
            public CapnpGen.MatchStatus Status => (CapnpGen.MatchStatus)ctx.ReadDataUShort(48UL, (ushort)0);
            public CapnpGen.Forfeit.READER Forfeit => ctx.ReadStruct(11, CapnpGen.Forfeit.READER.create);
            public CapnpGen.Dispute.READER Dispute => ctx.ReadStruct(12, CapnpGen.Dispute.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 13);
            }

            public ListOfPrimitivesSerializer<byte> MatchId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> TournamentId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public uint RoundNumber
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public ListOfPrimitivesSerializer<byte> Team1Id
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> Team2Id
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public string Team1Name
            {
                get => this.ReadText(4, null);
                set => this.WriteText(4, value, null);
            }

            public string Team2Name
            {
                get => this.ReadText(5, null);
                set => this.WriteText(5, value, null);
            }

            public ulong ScheduledTime
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong EstimatedDuration
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public CapnpGen.GameServer.WRITER GameServer
            {
                get => BuildPointer<CapnpGen.GameServer.WRITER>(6);
                set => Link(6, value);
            }

            public string StreamUrl
            {
                get => this.ReadText(7, null);
                set => this.WriteText(7, value, null);
            }

            public ListOfPrimitivesSerializer<byte> WinnerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(8);
                set => Link(8, value);
            }

            public ListOfPrimitivesSerializer<byte> LoserId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(9);
                set => Link(9, value);
            }

            public bool IsDraw
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }

            public ListOfPrimitivesSerializer<ulong> Scores
            {
                get => BuildPointer<ListOfPrimitivesSerializer<ulong>>(10);
                set => Link(10, value);
            }

            public ulong CompletedAt
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public CapnpGen.MatchStatus Status
            {
                get => (CapnpGen.MatchStatus)this.ReadDataUShort(48UL, (ushort)0);
                set => this.WriteData(48UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.Forfeit.WRITER Forfeit
            {
                get => BuildPointer<CapnpGen.Forfeit.WRITER>(11);
                set => Link(11, value);
            }

            public CapnpGen.Dispute.WRITER Dispute
            {
                get => BuildPointer<CapnpGen.Dispute.WRITER>(12);
                set => Link(12, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x875737a6f029afaaUL)]
    public enum MatchStatus : ushort
    {
        scheduled,
        inProgress,
        completed,
        cancelled,
        postponed
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdcb7ea863c4cf8e1UL)]
    public class Forfeit : ICapnpSerializable
    {
        public const UInt64 typeId = 0xdcb7ea863c4cf8e1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ForfeitedBy = reader.ForfeitedBy;
            Reason = reader.Reason;
            Approved = reader.Approved;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ForfeitedBy.Init(ForfeitedBy);
            writer.Reason = Reason;
            writer.Approved = Approved;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> ForfeitedBy
        {
            get;
            set;
        }

        public string Reason
        {
            get;
            set;
        }

        public bool Approved
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
            public IReadOnlyList<byte> ForfeitedBy => ctx.ReadList(0).CastByte();
            public string Reason => ctx.ReadText(1, null);
            public bool Approved => ctx.ReadDataBool(0UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public ListOfPrimitivesSerializer<byte> ForfeitedBy
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Reason
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public bool Approved
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf124ae6b35a523e3UL)]
    public class Dispute : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf124ae6b35a523e3UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            DisputedBy = reader.DisputedBy;
            Reason = reader.Reason;
            Evidence = reader.Evidence;
            Status = reader.Status;
            Resolution = reader.Resolution;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.DisputedBy.Init(DisputedBy);
            writer.Reason = Reason;
            writer.Evidence.Init(Evidence);
            writer.Status = Status;
            writer.Resolution = Resolution;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> DisputedBy
        {
            get;
            set;
        }

        public string Reason
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Evidence
        {
            get;
            set;
        }

        public CapnpGen.DisputeStatus Status
        {
            get;
            set;
        }

        public string Resolution
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
            public IReadOnlyList<byte> DisputedBy => ctx.ReadList(0).CastByte();
            public string Reason => ctx.ReadText(1, null);
            public IReadOnlyList<byte> Evidence => ctx.ReadList(2).CastByte();
            public CapnpGen.DisputeStatus Status => (CapnpGen.DisputeStatus)ctx.ReadDataUShort(0UL, (ushort)0);
            public string Resolution => ctx.ReadText(3, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 4);
            }

            public ListOfPrimitivesSerializer<byte> DisputedBy
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Reason
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public ListOfPrimitivesSerializer<byte> Evidence
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public CapnpGen.DisputeStatus Status
            {
                get => (CapnpGen.DisputeStatus)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public string Resolution
            {
                get => this.ReadText(3, null);
                set => this.WriteText(3, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe21a843fe1f15726UL)]
    public enum DisputeStatus : ushort
    {
        open,
        underReview,
        resolved,
        dismissed
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd1b79b60191027b4UL)]
    public class GameServer : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd1b79b60191027b4UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Address = reader.Address;
            Port = reader.Port;
            Region = reader.Region;
            Password = reader.Password;
            SpectatorSlot = reader.SpectatorSlot;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Address = Address;
            writer.Port = Port;
            writer.Region = Region;
            writer.Password = Password;
            writer.SpectatorSlot = SpectatorSlot;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string Address
        {
            get;
            set;
        }

        public ushort Port
        {
            get;
            set;
        }

        public CapnpGen.Region Region
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public bool SpectatorSlot
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
            public string Address => ctx.ReadText(0, null);
            public ushort Port => ctx.ReadDataUShort(0UL, (ushort)0);
            public CapnpGen.Region Region => (CapnpGen.Region)ctx.ReadDataUShort(16UL, (ushort)0);
            public string Password => ctx.ReadText(1, null);
            public bool SpectatorSlot => ctx.ReadDataBool(32UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public string Address
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ushort Port
            {
                get => this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, value, (ushort)0);
            }

            public CapnpGen.Region Region
            {
                get => (CapnpGen.Region)this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, (ushort)value, (ushort)0);
            }

            public string Password
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public bool SpectatorSlot
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdfec2a1339c8cbf5UL)]
    public class TournamentStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0xdfec2a1339c8cbf5UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TournamentId = reader.TournamentId;
            TotalMatches = reader.TotalMatches;
            CompletedMatches = reader.CompletedMatches;
            AverageMatchDuration = reader.AverageMatchDuration;
            TotalViewers = reader.TotalViewers;
            PeakViewers = reader.PeakViewers;
            PrizeDistribution = reader.PrizeDistribution?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PrizeDistribution>(_));
            RegionBreakdown = reader.RegionBreakdown?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.TournamentRegionStats>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TournamentId.Init(TournamentId);
            writer.TotalMatches = TotalMatches;
            writer.CompletedMatches = CompletedMatches;
            writer.AverageMatchDuration = AverageMatchDuration;
            writer.TotalViewers = TotalViewers;
            writer.PeakViewers = PeakViewers;
            writer.PrizeDistribution.Init(PrizeDistribution, (_s1, _v1) => _v1?.serialize(_s1));
            writer.RegionBreakdown.Init(RegionBreakdown, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> TournamentId
        {
            get;
            set;
        }

        public uint TotalMatches
        {
            get;
            set;
        }

        public uint CompletedMatches
        {
            get;
            set;
        }

        public ulong AverageMatchDuration
        {
            get;
            set;
        }

        public ulong TotalViewers
        {
            get;
            set;
        }

        public uint PeakViewers
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.PrizeDistribution> PrizeDistribution
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.TournamentRegionStats> RegionBreakdown
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
            public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
            public uint TotalMatches => ctx.ReadDataUInt(0UL, 0U);
            public uint CompletedMatches => ctx.ReadDataUInt(32UL, 0U);
            public ulong AverageMatchDuration => ctx.ReadDataULong(64UL, 0UL);
            public ulong TotalViewers => ctx.ReadDataULong(128UL, 0UL);
            public uint PeakViewers => ctx.ReadDataUInt(192UL, 0U);
            public IReadOnlyList<CapnpGen.PrizeDistribution.READER> PrizeDistribution => ctx.ReadList(1).Cast(CapnpGen.PrizeDistribution.READER.create);
            public IReadOnlyList<CapnpGen.TournamentRegionStats.READER> RegionBreakdown => ctx.ReadList(2).Cast(CapnpGen.TournamentRegionStats.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 3);
            }

            public ListOfPrimitivesSerializer<byte> TournamentId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public uint TotalMatches
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public uint CompletedMatches
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public ulong AverageMatchDuration
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong TotalViewers
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public uint PeakViewers
            {
                get => this.ReadDataUInt(192UL, 0U);
                set => this.WriteData(192UL, value, 0U);
            }

            public ListOfStructsSerializer<CapnpGen.PrizeDistribution.WRITER> PrizeDistribution
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.PrizeDistribution.WRITER>>(1);
                set => Link(1, value);
            }

            public ListOfStructsSerializer<CapnpGen.TournamentRegionStats.WRITER> RegionBreakdown
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.TournamentRegionStats.WRITER>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaef2a8aeb038b49aUL)]
    public class PrizeDistribution : ICapnpSerializable
    {
        public const UInt64 typeId = 0xaef2a8aeb038b49aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Place = reader.Place;
            ParticipantId = reader.ParticipantId;
            Amount = reader.Amount;
            DistributedAt = reader.DistributedAt;
            TxHash = reader.TxHash;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Place = Place;
            writer.ParticipantId.Init(ParticipantId);
            writer.Amount = Amount;
            writer.DistributedAt = DistributedAt;
            writer.TxHash.Init(TxHash);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint Place
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ParticipantId
        {
            get;
            set;
        }

        public ulong Amount
        {
            get;
            set;
        }

        public ulong DistributedAt
        {
            get;
            set;
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
            public uint Place => ctx.ReadDataUInt(0UL, 0U);
            public IReadOnlyList<byte> ParticipantId => ctx.ReadList(0).CastByte();
            public ulong Amount => ctx.ReadDataULong(64UL, 0UL);
            public ulong DistributedAt => ctx.ReadDataULong(128UL, 0UL);
            public IReadOnlyList<byte> TxHash => ctx.ReadList(1).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 2);
            }

            public uint Place
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public ListOfPrimitivesSerializer<byte> ParticipantId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ulong Amount
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong DistributedAt
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> TxHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9bd7d2516aa7a47aUL)]
    public class TournamentRegionStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9bd7d2516aa7a47aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Region = reader.Region;
            Participants = reader.Participants;
            AverageRating = reader.AverageRating;
            BestPlace = reader.BestPlace;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Region = Region;
            writer.Participants = Participants;
            writer.AverageRating = AverageRating;
            writer.BestPlace = BestPlace;
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

        public uint Participants
        {
            get;
            set;
        }

        public float AverageRating
        {
            get;
            set;
        }

        public uint BestPlace
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
            public uint Participants => ctx.ReadDataUInt(32UL, 0U);
            public float AverageRating => ctx.ReadDataFloat(64UL, 0F);
            public uint BestPlace => ctx.ReadDataUInt(96UL, 0U);
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

            public uint Participants
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public float AverageRating
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public uint BestPlace
            {
                get => this.ReadDataUInt(96UL, 0U);
                set => this.WriteData(96UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xda796c8ae6b4c139UL), Proxy(typeof(TournamentService_Proxy)), Skeleton(typeof(TournamentService_Skeleton))]
    public interface ITournamentService : IDisposable
    {
        Task<IReadOnlyList<byte>> CreateTournament(CapnpGen.Tournament tournament, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task UpdateTournament(IReadOnlyList<byte> tournamentId, CapnpGen.Tournament tournament, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task CancelTournament(IReadOnlyList<byte> tournamentId, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> Register(IReadOnlyList<byte> tournamentId, CapnpGen.TournamentRegistration registration, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task CheckIn(IReadOnlyList<byte> registrationId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task Withdraw(IReadOnlyList<byte> registrationId, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> GenerateBracket(IReadOnlyList<byte> tournamentId, CapnpGen.TournamentType bracketType, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task AdvanceRound(IReadOnlyList<byte> tournamentId, uint roundNumber, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> ScheduleMatch(CapnpGen.TournamentMatch match, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task ReportMatchResult(IReadOnlyList<byte> matchId, CapnpGen.MatchResult result, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task DisputeMatch(IReadOnlyList<byte> matchId, CapnpGen.Dispute dispute, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.Tournament> GetTournament(IReadOnlyList<byte> tournamentId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.Tournament>> ListTournaments(CapnpGen.TournamentFilter filter, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.Bracket> GetBracket(IReadOnlyList<byte> tournamentId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.Standing>> GetStandings(IReadOnlyList<byte> tournamentId, CancellationToken cancellationToken_ = default);
        Task DisqualifyParticipant(IReadOnlyList<byte> registrationId, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> DistributePrizes(IReadOnlyList<byte> tournamentId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xda796c8ae6b4c139UL)]
    public class TournamentService_Proxy : Proxy, ITournamentService
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xda796c8ae6b4c139UL)]
    public class TournamentService_Skeleton : Skeleton<ITournamentService>
    {
        public TournamentService_Skeleton()
        {
            SetMethodTable(CreateTournament, UpdateTournament, CancelTournament, Register, CheckIn, Withdraw, GenerateBracket, AdvanceRound, ScheduleMatch, ReportMatchResult, DisputeMatch, GetTournament, ListTournaments, GetBracket, GetStandings, DisqualifyParticipant, DistributePrizes);
        }

        public override ulong InterfaceId => 15742733316282171705UL;
        Task<AnswerOrCounterquestion> CreateTournament(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_CreateTournament>(d_);
                return Impatient.MaybeTailCall(Impl.CreateTournament(in_.Tournament, in_.Signature, cancellationToken_), tournamentId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_CreateTournament.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_CreateTournament{TournamentId = tournamentId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> UpdateTournament(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_UpdateTournament>(d_);
                await Impl.UpdateTournament(in_.TournamentId, in_.Tournament, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_UpdateTournament.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> CancelTournament(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_CancelTournament>(d_);
                await Impl.CancelTournament(in_.TournamentId, in_.Reason, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_CancelTournament.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> Register(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_Register>(d_);
                return Impatient.MaybeTailCall(Impl.Register(in_.TournamentId, in_.Registration, in_.Signature, cancellationToken_), registrationId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_Register.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_Register{RegistrationId = registrationId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> CheckIn(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_CheckIn>(d_);
                await Impl.CheckIn(in_.RegistrationId, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_CheckIn.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> Withdraw(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_Withdraw>(d_);
                await Impl.Withdraw(in_.RegistrationId, in_.Reason, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_Withdraw.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GenerateBracket(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_GenerateBracket>(d_);
                return Impatient.MaybeTailCall(Impl.GenerateBracket(in_.TournamentId, in_.BracketType, in_.Signature, cancellationToken_), bracketId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_GenerateBracket.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_GenerateBracket{BracketId = bracketId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> AdvanceRound(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_AdvanceRound>(d_);
                await Impl.AdvanceRound(in_.TournamentId, in_.RoundNumber, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_AdvanceRound.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> ScheduleMatch(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_ScheduleMatch>(d_);
                return Impatient.MaybeTailCall(Impl.ScheduleMatch(in_.Match, in_.Signature, cancellationToken_), matchId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_ScheduleMatch.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_ScheduleMatch{MatchId = matchId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> ReportMatchResult(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_ReportMatchResult>(d_);
                await Impl.ReportMatchResult(in_.MatchId, in_.Result, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_ReportMatchResult.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> DisputeMatch(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_DisputeMatch>(d_);
                await Impl.DisputeMatch(in_.MatchId, in_.Dispute, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_DisputeMatch.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GetTournament(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_GetTournament>(d_);
                return Impatient.MaybeTailCall(Impl.GetTournament(in_.TournamentId, cancellationToken_), tournament =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_GetTournament.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_GetTournament{Tournament = tournament};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> ListTournaments(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_ListTournaments>(d_);
                return Impatient.MaybeTailCall(Impl.ListTournaments(in_.Filter, cancellationToken_), tournaments =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_ListTournaments.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_ListTournaments{Tournaments = tournaments};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetBracket(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_GetBracket>(d_);
                return Impatient.MaybeTailCall(Impl.GetBracket(in_.TournamentId, cancellationToken_), bracket =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_GetBracket.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_GetBracket{Bracket = bracket};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetStandings(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_GetStandings>(d_);
                return Impatient.MaybeTailCall(Impl.GetStandings(in_.TournamentId, cancellationToken_), standings =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_GetStandings.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_GetStandings{Standings = standings};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> DisqualifyParticipant(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_DisqualifyParticipant>(d_);
                await Impl.DisqualifyParticipant(in_.RegistrationId, in_.Reason, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_DisqualifyParticipant.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> DistributePrizes(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TournamentService.Params_DistributePrizes>(d_);
                return Impatient.MaybeTailCall(Impl.DistributePrizes(in_.TournamentId, in_.Signature, cancellationToken_), distributionTxHash =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.TournamentService.Result_DistributePrizes.WRITER>();
                    var r_ = new CapnpGen.TournamentService.Result_DistributePrizes{DistributionTxHash = distributionTxHash};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class TournamentService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xce0bd153941cef77UL)]
        public class Params_CreateTournament : ICapnpSerializable
        {
            public const UInt64 typeId = 0xce0bd153941cef77UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Tournament = CapnpSerializable.Create<CapnpGen.Tournament>(reader.Tournament);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Tournament?.serialize(writer.Tournament);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.Tournament Tournament
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
                public CapnpGen.Tournament.READER Tournament => ctx.ReadStruct(0, CapnpGen.Tournament.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.Tournament.WRITER Tournament
                {
                    get => BuildPointer<CapnpGen.Tournament.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe472a326160106c7UL)]
        public class Result_CreateTournament : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe472a326160106c7UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa6f298ad06d6426bUL)]
        public class Params_UpdateTournament : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa6f298ad06d6426bUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                Tournament = CapnpSerializable.Create<CapnpGen.Tournament>(reader.Tournament);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
                Tournament?.serialize(writer.Tournament);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
            {
                get;
                set;
            }

            public CapnpGen.Tournament Tournament
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
                public CapnpGen.Tournament.READER Tournament => ctx.ReadStruct(1, CapnpGen.Tournament.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.Tournament.WRITER Tournament
                {
                    get => BuildPointer<CapnpGen.Tournament.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb86d32852a6d6e32UL)]
        public class Result_UpdateTournament : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb86d32852a6d6e32UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc1ea6412866546a5UL)]
        public class Params_CancelTournament : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc1ea6412866546a5UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                Reason = reader.Reason;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
                writer.Reason = Reason;
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
            {
                get;
                set;
            }

            public string Reason
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
                public string Reason => ctx.ReadText(1, null);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public string Reason
                {
                    get => this.ReadText(1, null);
                    set => this.WriteText(1, value, null);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8e4045fb36c153a8UL)]
        public class Result_CancelTournament : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8e4045fb36c153a8UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9fdfb8453c1298daUL)]
        public class Params_Register : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9fdfb8453c1298daUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                Registration = CapnpSerializable.Create<CapnpGen.TournamentRegistration>(reader.Registration);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
                Registration?.serialize(writer.Registration);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
            {
                get;
                set;
            }

            public CapnpGen.TournamentRegistration Registration
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
                public CapnpGen.TournamentRegistration.READER Registration => ctx.ReadStruct(1, CapnpGen.TournamentRegistration.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.TournamentRegistration.WRITER Registration
                {
                    get => BuildPointer<CapnpGen.TournamentRegistration.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9b3801dad4e6d747UL)]
        public class Result_Register : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9b3801dad4e6d747UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RegistrationId = reader.RegistrationId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RegistrationId.Init(RegistrationId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RegistrationId
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
                public IReadOnlyList<byte> RegistrationId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> RegistrationId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa55464a597a381beUL)]
        public class Params_CheckIn : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa55464a597a381beUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RegistrationId = reader.RegistrationId;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RegistrationId.Init(RegistrationId);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RegistrationId
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
                public IReadOnlyList<byte> RegistrationId => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> RegistrationId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9831fe4918f739f2UL)]
        public class Result_CheckIn : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9831fe4918f739f2UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xba1b406d6cb52326UL)]
        public class Params_Withdraw : ICapnpSerializable
        {
            public const UInt64 typeId = 0xba1b406d6cb52326UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RegistrationId = reader.RegistrationId;
                Reason = reader.Reason;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RegistrationId.Init(RegistrationId);
                writer.Reason = Reason;
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RegistrationId
            {
                get;
                set;
            }

            public string Reason
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
                public IReadOnlyList<byte> RegistrationId => ctx.ReadList(0).CastByte();
                public string Reason => ctx.ReadText(1, null);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> RegistrationId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public string Reason
                {
                    get => this.ReadText(1, null);
                    set => this.WriteText(1, value, null);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa263a3c0a30b1e4dUL)]
        public class Result_Withdraw : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa263a3c0a30b1e4dUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc6364397ebfd829cUL)]
        public class Params_GenerateBracket : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc6364397ebfd829cUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                BracketType = reader.BracketType;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
                writer.BracketType = BracketType;
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
            {
                get;
                set;
            }

            public CapnpGen.TournamentType BracketType
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
                public CapnpGen.TournamentType BracketType => (CapnpGen.TournamentType)ctx.ReadDataUShort(0UL, (ushort)0);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 2);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.TournamentType BracketType
                {
                    get => (CapnpGen.TournamentType)this.ReadDataUShort(0UL, (ushort)0);
                    set => this.WriteData(0UL, (ushort)value, (ushort)0);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xda655c5f9b34b796UL)]
        public class Result_GenerateBracket : ICapnpSerializable
        {
            public const UInt64 typeId = 0xda655c5f9b34b796UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                BracketId = reader.BracketId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.BracketId.Init(BracketId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> BracketId
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
                public IReadOnlyList<byte> BracketId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> BracketId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x98a9e5dbeb6e536fUL)]
        public class Params_AdvanceRound : ICapnpSerializable
        {
            public const UInt64 typeId = 0x98a9e5dbeb6e536fUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                RoundNumber = reader.RoundNumber;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
                writer.RoundNumber = RoundNumber;
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
            {
                get;
                set;
            }

            public uint RoundNumber
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
                public uint RoundNumber => ctx.ReadDataUInt(0UL, 0U);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 2);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public uint RoundNumber
                {
                    get => this.ReadDataUInt(0UL, 0U);
                    set => this.WriteData(0UL, value, 0U);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbf300dd2c5901a9cUL)]
        public class Result_AdvanceRound : ICapnpSerializable
        {
            public const UInt64 typeId = 0xbf300dd2c5901a9cUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe759b83b07ccc193UL)]
        public class Params_ScheduleMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe759b83b07ccc193UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Match = CapnpSerializable.Create<CapnpGen.TournamentMatch>(reader.Match);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Match?.serialize(writer.Match);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.TournamentMatch Match
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
                public CapnpGen.TournamentMatch.READER Match => ctx.ReadStruct(0, CapnpGen.TournamentMatch.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.TournamentMatch.WRITER Match
                {
                    get => BuildPointer<CapnpGen.TournamentMatch.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xace265d5c1f810e2UL)]
        public class Result_ScheduleMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xace265d5c1f810e2UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc094318046255e8dUL)]
        public class Params_ReportMatchResult : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc094318046255e8dUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                MatchId = reader.MatchId;
                Result = CapnpSerializable.Create<CapnpGen.MatchResult>(reader.Result);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.MatchId.Init(MatchId);
                Result?.serialize(writer.Result);
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

            public CapnpGen.MatchResult Result
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
                public CapnpGen.MatchResult.READER Result => ctx.ReadStruct(1, CapnpGen.MatchResult.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
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

                public CapnpGen.MatchResult.WRITER Result
                {
                    get => BuildPointer<CapnpGen.MatchResult.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfef58ccb43e19299UL)]
        public class Result_ReportMatchResult : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfef58ccb43e19299UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9864da6a46c47555UL)]
        public class Params_DisputeMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9864da6a46c47555UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                MatchId = reader.MatchId;
                Dispute = CapnpSerializable.Create<CapnpGen.Dispute>(reader.Dispute);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.MatchId.Init(MatchId);
                Dispute?.serialize(writer.Dispute);
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

            public CapnpGen.Dispute Dispute
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
                public CapnpGen.Dispute.READER Dispute => ctx.ReadStruct(1, CapnpGen.Dispute.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
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

                public CapnpGen.Dispute.WRITER Dispute
                {
                    get => BuildPointer<CapnpGen.Dispute.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb61d05a28dc61276UL)]
        public class Result_DisputeMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb61d05a28dc61276UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaaaad0edd70a4956UL)]
        public class Params_GetTournament : ICapnpSerializable
        {
            public const UInt64 typeId = 0xaaaad0edd70a4956UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf669c504bc99dd87UL)]
        public class Result_GetTournament : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf669c504bc99dd87UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Tournament = CapnpSerializable.Create<CapnpGen.Tournament>(reader.Tournament);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Tournament?.serialize(writer.Tournament);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.Tournament Tournament
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
                public CapnpGen.Tournament.READER Tournament => ctx.ReadStruct(0, CapnpGen.Tournament.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.Tournament.WRITER Tournament
                {
                    get => BuildPointer<CapnpGen.Tournament.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf7dc1a2ff290e2d8UL)]
        public class Params_ListTournaments : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf7dc1a2ff290e2d8UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Filter = CapnpSerializable.Create<CapnpGen.TournamentFilter>(reader.Filter);
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

            public CapnpGen.TournamentFilter Filter
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
                public CapnpGen.TournamentFilter.READER Filter => ctx.ReadStruct(0, CapnpGen.TournamentFilter.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.TournamentFilter.WRITER Filter
                {
                    get => BuildPointer<CapnpGen.TournamentFilter.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xabf15127690fd5a0UL)]
        public class Result_ListTournaments : ICapnpSerializable
        {
            public const UInt64 typeId = 0xabf15127690fd5a0UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Tournaments = reader.Tournaments?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Tournament>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Tournaments.Init(Tournaments, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.Tournament> Tournaments
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
                public IReadOnlyList<CapnpGen.Tournament.READER> Tournaments => ctx.ReadList(0).Cast(CapnpGen.Tournament.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.Tournament.WRITER> Tournaments
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.Tournament.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa2bed22d5802b5b7UL)]
        public class Params_GetBracket : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa2bed22d5802b5b7UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb306651dcb6999cdUL)]
        public class Result_GetBracket : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb306651dcb6999cdUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Bracket = CapnpSerializable.Create<CapnpGen.Bracket>(reader.Bracket);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Bracket?.serialize(writer.Bracket);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.Bracket Bracket
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
                public CapnpGen.Bracket.READER Bracket => ctx.ReadStruct(0, CapnpGen.Bracket.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.Bracket.WRITER Bracket
                {
                    get => BuildPointer<CapnpGen.Bracket.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x950ae197a9f03a23UL)]
        public class Params_GetStandings : ICapnpSerializable
        {
            public const UInt64 typeId = 0x950ae197a9f03a23UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaa751de08fc5f902UL)]
        public class Result_GetStandings : ICapnpSerializable
        {
            public const UInt64 typeId = 0xaa751de08fc5f902UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Standings = reader.Standings?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Standing>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Standings.Init(Standings, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.Standing> Standings
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
                public IReadOnlyList<CapnpGen.Standing.READER> Standings => ctx.ReadList(0).Cast(CapnpGen.Standing.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.Standing.WRITER> Standings
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.Standing.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd59dd9fbad7b3a29UL)]
        public class Params_DisqualifyParticipant : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd59dd9fbad7b3a29UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RegistrationId = reader.RegistrationId;
                Reason = reader.Reason;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RegistrationId.Init(RegistrationId);
                writer.Reason = Reason;
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RegistrationId
            {
                get;
                set;
            }

            public string Reason
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
                public IReadOnlyList<byte> RegistrationId => ctx.ReadList(0).CastByte();
                public string Reason => ctx.ReadText(1, null);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> RegistrationId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public string Reason
                {
                    get => this.ReadText(1, null);
                    set => this.WriteText(1, value, null);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe45b8ed2c45bfb7cUL)]
        public class Result_DisqualifyParticipant : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe45b8ed2c45bfb7cUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfa37823d412029feUL)]
        public class Params_DistributePrizes : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfa37823d412029feUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TournamentId = reader.TournamentId;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.TournamentId.Init(TournamentId);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> TournamentId
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
                public IReadOnlyList<byte> TournamentId => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> TournamentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe2716836c2654128UL)]
        public class Result_DistributePrizes : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe2716836c2654128UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                DistributionTxHash = reader.DistributionTxHash;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.DistributionTxHash.Init(DistributionTxHash);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> DistributionTxHash
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
                public IReadOnlyList<byte> DistributionTxHash => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> DistributionTxHash
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x82b1358241e32cf4UL)]
    public class MatchResult : ICapnpSerializable
    {
        public const UInt64 typeId = 0x82b1358241e32cf4UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            WinnerId = reader.WinnerId;
            LoserId = reader.LoserId;
            IsDraw = reader.IsDraw;
            Scores = reader.Scores;
            ReplayHash = reader.ReplayHash;
            Evidence = reader.Evidence;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.WinnerId.Init(WinnerId);
            writer.LoserId.Init(LoserId);
            writer.IsDraw = IsDraw;
            writer.Scores.Init(Scores);
            writer.ReplayHash.Init(ReplayHash);
            writer.Evidence.Init(Evidence);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> WinnerId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> LoserId
        {
            get;
            set;
        }

        public bool IsDraw
        {
            get;
            set;
        }

        public IReadOnlyList<ulong> Scores
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ReplayHash
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Evidence
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
            public IReadOnlyList<byte> WinnerId => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> LoserId => ctx.ReadList(1).CastByte();
            public bool IsDraw => ctx.ReadDataBool(0UL, false);
            public IReadOnlyList<ulong> Scores => ctx.ReadList(2).CastULong();
            public IReadOnlyList<byte> ReplayHash => ctx.ReadList(3).CastByte();
            public IReadOnlyList<byte> Evidence => ctx.ReadList(4).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 5);
            }

            public ListOfPrimitivesSerializer<byte> WinnerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> LoserId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public bool IsDraw
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public ListOfPrimitivesSerializer<ulong> Scores
            {
                get => BuildPointer<ListOfPrimitivesSerializer<ulong>>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> ReplayHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public ListOfPrimitivesSerializer<byte> Evidence
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd7ac00efccc4151eUL)]
    public class TournamentFilter : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd7ac00efccc4151eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            Status = reader.Status;
            Region = reader.Region;
            TournamentType = reader.TournamentType;
            HasEntryFee = reader.HasEntryFee;
            MinPrizePool = reader.MinPrizePool;
            RegistrationOpen = reader.RegistrationOpen;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.Status = Status;
            writer.Region = Region;
            writer.TournamentType = TournamentType;
            writer.HasEntryFee = HasEntryFee;
            writer.MinPrizePool = MinPrizePool;
            writer.RegistrationOpen = RegistrationOpen;
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

        public CapnpGen.TournamentStatus Status
        {
            get;
            set;
        }

        public CapnpGen.Region Region
        {
            get;
            set;
        }

        public CapnpGen.TournamentType TournamentType
        {
            get;
            set;
        }

        public bool HasEntryFee
        {
            get;
            set;
        }

        public ulong MinPrizePool
        {
            get;
            set;
        }

        public bool RegistrationOpen
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
            public CapnpGen.TournamentStatus Status => (CapnpGen.TournamentStatus)ctx.ReadDataUShort(0UL, (ushort)0);
            public CapnpGen.Region Region => (CapnpGen.Region)ctx.ReadDataUShort(16UL, (ushort)0);
            public CapnpGen.TournamentType TournamentType => (CapnpGen.TournamentType)ctx.ReadDataUShort(32UL, (ushort)0);
            public bool HasEntryFee => ctx.ReadDataBool(48UL, false);
            public ulong MinPrizePool => ctx.ReadDataULong(64UL, 0UL);
            public bool RegistrationOpen => ctx.ReadDataBool(49UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 1);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public CapnpGen.TournamentStatus Status
            {
                get => (CapnpGen.TournamentStatus)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.Region Region
            {
                get => (CapnpGen.Region)this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.TournamentType TournamentType
            {
                get => (CapnpGen.TournamentType)this.ReadDataUShort(32UL, (ushort)0);
                set => this.WriteData(32UL, (ushort)value, (ushort)0);
            }

            public bool HasEntryFee
            {
                get => this.ReadDataBool(48UL, false);
                set => this.WriteData(48UL, value, false);
            }

            public ulong MinPrizePool
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public bool RegistrationOpen
            {
                get => this.ReadDataBool(49UL, false);
                set => this.WriteData(49UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb980215463fcc409UL)]
    public class Standing : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb980215463fcc409UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Rank = reader.Rank;
            ParticipantId = reader.ParticipantId;
            Name = reader.Name;
            Wins = reader.Wins;
            Losses = reader.Losses;
            Draws = reader.Draws;
            Points = reader.Points;
            RatingChange = reader.RatingChange;
            PrizeEarned = reader.PrizeEarned;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Rank = Rank;
            writer.ParticipantId.Init(ParticipantId);
            writer.Name = Name;
            writer.Wins = Wins;
            writer.Losses = Losses;
            writer.Draws = Draws;
            writer.Points = Points;
            writer.RatingChange = RatingChange;
            writer.PrizeEarned = PrizeEarned;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint Rank
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ParticipantId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public uint Wins
        {
            get;
            set;
        }

        public uint Losses
        {
            get;
            set;
        }

        public uint Draws
        {
            get;
            set;
        }

        public uint Points
        {
            get;
            set;
        }

        public float RatingChange
        {
            get;
            set;
        }

        public ulong PrizeEarned
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
            public uint Rank => ctx.ReadDataUInt(0UL, 0U);
            public IReadOnlyList<byte> ParticipantId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public uint Wins => ctx.ReadDataUInt(32UL, 0U);
            public uint Losses => ctx.ReadDataUInt(64UL, 0U);
            public uint Draws => ctx.ReadDataUInt(96UL, 0U);
            public uint Points => ctx.ReadDataUInt(128UL, 0U);
            public float RatingChange => ctx.ReadDataFloat(160UL, 0F);
            public ulong PrizeEarned => ctx.ReadDataULong(192UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 2);
            }

            public uint Rank
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public ListOfPrimitivesSerializer<byte> ParticipantId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public uint Wins
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public uint Losses
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }

            public uint Draws
            {
                get => this.ReadDataUInt(96UL, 0U);
                set => this.WriteData(96UL, value, 0U);
            }

            public uint Points
            {
                get => this.ReadDataUInt(128UL, 0U);
                set => this.WriteData(128UL, value, 0U);
            }

            public float RatingChange
            {
                get => this.ReadDataFloat(160UL, 0F);
                set => this.WriteData(160UL, value, 0F);
            }

            public ulong PrizeEarned
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }
        }
    }
}