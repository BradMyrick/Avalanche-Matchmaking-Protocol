using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa4ebb1e316d0add5UL)]
    public class MMR : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa4ebb1e316d0add5UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Rating = reader.Rating;
            Uncertainty = reader.Uncertainty;
            Volatility = reader.Volatility;
            GamesPlayed = reader.GamesPlayed;
            LastUpdated = reader.LastUpdated;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Rating = Rating;
            writer.Uncertainty = Uncertainty;
            writer.Volatility = Volatility;
            writer.GamesPlayed = GamesPlayed;
            writer.LastUpdated = LastUpdated;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float Rating
        {
            get;
            set;
        }

        public float Uncertainty
        {
            get;
            set;
        }

        public float Volatility
        {
            get;
            set;
        }

        public uint GamesPlayed
        {
            get;
            set;
        }

        public ulong LastUpdated
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
            public float Rating => ctx.ReadDataFloat(0UL, 0F);
            public float Uncertainty => ctx.ReadDataFloat(32UL, 0F);
            public float Volatility => ctx.ReadDataFloat(64UL, 0F);
            public uint GamesPlayed => ctx.ReadDataUInt(96UL, 0U);
            public ulong LastUpdated => ctx.ReadDataULong(128UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 0);
            }

            public float Rating
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Uncertainty
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float Volatility
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public uint GamesPlayed
            {
                get => this.ReadDataUInt(96UL, 0U);
                set => this.WriteData(96UL, value, 0U);
            }

            public ulong LastUpdated
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9a3a5e65541a8247UL)]
    public class PlayerAttributes : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9a3a5e65541a8247UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PreferredRole = reader.PreferredRole;
            PlayStyle = reader.PlayStyle;
            Language = reader.Language;
            Platform = reader.Platform;
            InputDevice = reader.InputDevice;
            TimeZone = reader.TimeZone;
            MicEnabled = reader.MicEnabled;
            MaxPingMs = reader.MaxPingMs;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PreferredRole = PreferredRole;
            writer.PlayStyle = PlayStyle;
            writer.Language = Language;
            writer.Platform = Platform;
            writer.InputDevice = InputDevice;
            writer.TimeZone = TimeZone;
            writer.MicEnabled = MicEnabled;
            writer.MaxPingMs = MaxPingMs;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string PreferredRole
        {
            get;
            set;
        }

        public string PlayStyle
        {
            get;
            set;
        }

        public string Language
        {
            get;
            set;
        }

        public string Platform
        {
            get;
            set;
        }

        public string InputDevice
        {
            get;
            set;
        }

        public string TimeZone
        {
            get;
            set;
        }

        public bool MicEnabled
        {
            get;
            set;
        }

        public uint MaxPingMs
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
            public string PreferredRole => ctx.ReadText(0, null);
            public string PlayStyle => ctx.ReadText(1, null);
            public string Language => ctx.ReadText(2, null);
            public string Platform => ctx.ReadText(3, null);
            public string InputDevice => ctx.ReadText(4, null);
            public string TimeZone => ctx.ReadText(5, null);
            public bool MicEnabled => ctx.ReadDataBool(0UL, false);
            public uint MaxPingMs => ctx.ReadDataUInt(32UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 6);
            }

            public string PreferredRole
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public string PlayStyle
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public string Language
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }

            public string Platform
            {
                get => this.ReadText(3, null);
                set => this.WriteText(3, value, null);
            }

            public string InputDevice
            {
                get => this.ReadText(4, null);
                set => this.WriteText(4, value, null);
            }

            public string TimeZone
            {
                get => this.ReadText(5, null);
                set => this.WriteText(5, value, null);
            }

            public bool MicEnabled
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public uint MaxPingMs
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x808f26329b0a9435UL)]
    public class GameStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0x808f26329b0a9435UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            MatchesPlayed = reader.MatchesPlayed;
            Wins = reader.Wins;
            Losses = reader.Losses;
            Draws = reader.Draws;
            WinRate = reader.WinRate;
            TotalPlayTimeMs = reader.TotalPlayTimeMs;
            BestStreak = reader.BestStreak;
            CurrentStreak = reader.CurrentStreak;
            Achievements = reader.Achievements?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Achievement>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.MatchesPlayed = MatchesPlayed;
            writer.Wins = Wins;
            writer.Losses = Losses;
            writer.Draws = Draws;
            writer.WinRate = WinRate;
            writer.TotalPlayTimeMs = TotalPlayTimeMs;
            writer.BestStreak = BestStreak;
            writer.CurrentStreak = CurrentStreak;
            writer.Achievements.Init(Achievements, (_s1, _v1) => _v1?.serialize(_s1));
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

        public uint MatchesPlayed
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

        public float WinRate
        {
            get;
            set;
        }

        public ulong TotalPlayTimeMs
        {
            get;
            set;
        }

        public ushort BestStreak
        {
            get;
            set;
        }

        public short CurrentStreak
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Achievement> Achievements
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
            public uint MatchesPlayed => ctx.ReadDataUInt(0UL, 0U);
            public uint Wins => ctx.ReadDataUInt(32UL, 0U);
            public uint Losses => ctx.ReadDataUInt(64UL, 0U);
            public uint Draws => ctx.ReadDataUInt(96UL, 0U);
            public float WinRate => ctx.ReadDataFloat(128UL, 0F);
            public ulong TotalPlayTimeMs => ctx.ReadDataULong(192UL, 0UL);
            public ushort BestStreak => ctx.ReadDataUShort(160UL, (ushort)0);
            public short CurrentStreak => ctx.ReadDataShort(176UL, (short)0);
            public IReadOnlyList<CapnpGen.Achievement.READER> Achievements => ctx.ReadList(1).Cast(CapnpGen.Achievement.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 2);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public uint MatchesPlayed
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
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

            public float WinRate
            {
                get => this.ReadDataFloat(128UL, 0F);
                set => this.WriteData(128UL, value, 0F);
            }

            public ulong TotalPlayTimeMs
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public ushort BestStreak
            {
                get => this.ReadDataUShort(160UL, (ushort)0);
                set => this.WriteData(160UL, value, (ushort)0);
            }

            public short CurrentStreak
            {
                get => this.ReadDataShort(176UL, (short)0);
                set => this.WriteData(176UL, value, (short)0);
            }

            public ListOfStructsSerializer<CapnpGen.Achievement.WRITER> Achievements
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.Achievement.WRITER>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd2daf2ddc5d33c4bUL)]
    public class Achievement : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd2daf2ddc5d33c4bUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            AchievementId = reader.AchievementId;
            Name = reader.Name;
            Description = reader.Description;
            UnlockedAt = reader.UnlockedAt;
            Rarity = reader.Rarity;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.AchievementId.Init(AchievementId);
            writer.Name = Name;
            writer.Description = Description;
            writer.UnlockedAt = UnlockedAt;
            writer.Rarity = Rarity;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> AchievementId
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

        public ulong UnlockedAt
        {
            get;
            set;
        }

        public float Rarity
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
            public IReadOnlyList<byte> AchievementId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public string Description => ctx.ReadText(2, null);
            public ulong UnlockedAt => ctx.ReadDataULong(0UL, 0UL);
            public float Rarity => ctx.ReadDataFloat(64UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 3);
            }

            public ListOfPrimitivesSerializer<byte> AchievementId
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

            public ulong UnlockedAt
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public float Rarity
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa063b005fa7b41c1UL)]
    public class PlayerPreferences : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa063b005fa7b41c1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            AutoAccept = reader.AutoAccept;
            MaxWaitTimeMs = reader.MaxWaitTimeMs;
            MinSkillDiff = reader.MinSkillDiff;
            RegionPriority = reader.RegionPriority;
            LanguageFilter = reader.LanguageFilter;
            ContentFilter = CapnpSerializable.Create<CapnpGen.ContentFilter>(reader.ContentFilter);
            NotificationPrefs = CapnpSerializable.Create<CapnpGen.NotificationPrefs>(reader.NotificationPrefs);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.AutoAccept = AutoAccept;
            writer.MaxWaitTimeMs = MaxWaitTimeMs;
            writer.MinSkillDiff = MinSkillDiff;
            writer.RegionPriority.Init(RegionPriority);
            writer.LanguageFilter.Init(LanguageFilter);
            ContentFilter?.serialize(writer.ContentFilter);
            NotificationPrefs?.serialize(writer.NotificationPrefs);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool AutoAccept
        {
            get;
            set;
        }

        public ulong MaxWaitTimeMs
        {
            get;
            set;
        }

        public float MinSkillDiff
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Region> RegionPriority
        {
            get;
            set;
        }

        public IReadOnlyList<string> LanguageFilter
        {
            get;
            set;
        }

        public CapnpGen.ContentFilter ContentFilter
        {
            get;
            set;
        }

        public CapnpGen.NotificationPrefs NotificationPrefs
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
            public bool AutoAccept => ctx.ReadDataBool(0UL, false);
            public ulong MaxWaitTimeMs => ctx.ReadDataULong(64UL, 0UL);
            public float MinSkillDiff => ctx.ReadDataFloat(32UL, 0F);
            public IReadOnlyList<CapnpGen.Region> RegionPriority => ctx.ReadList(0).CastEnums(_0 => (CapnpGen.Region)_0);
            public IReadOnlyList<string> LanguageFilter => ctx.ReadList(1).CastText2();
            public CapnpGen.ContentFilter.READER ContentFilter => ctx.ReadStruct(2, CapnpGen.ContentFilter.READER.create);
            public CapnpGen.NotificationPrefs.READER NotificationPrefs => ctx.ReadStruct(3, CapnpGen.NotificationPrefs.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 4);
            }

            public bool AutoAccept
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public ulong MaxWaitTimeMs
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public float MinSkillDiff
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public ListOfPrimitivesSerializer<CapnpGen.Region> RegionPriority
            {
                get => BuildPointer<ListOfPrimitivesSerializer<CapnpGen.Region>>(0);
                set => Link(0, value);
            }

            public ListOfTextSerializer LanguageFilter
            {
                get => BuildPointer<ListOfTextSerializer>(1);
                set => Link(1, value);
            }

            public CapnpGen.ContentFilter.WRITER ContentFilter
            {
                get => BuildPointer<CapnpGen.ContentFilter.WRITER>(2);
                set => Link(2, value);
            }

            public CapnpGen.NotificationPrefs.WRITER NotificationPrefs
            {
                get => BuildPointer<CapnpGen.NotificationPrefs.WRITER>(3);
                set => Link(3, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe701fa2468c4c75aUL)]
    public class ContentFilter : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe701fa2468c4c75aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            AllowViolence = reader.AllowViolence;
            AllowLanguage = reader.AllowLanguage;
            AllowGambling = reader.AllowGambling;
            AgeRating = reader.AgeRating;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.AllowViolence = AllowViolence;
            writer.AllowLanguage = AllowLanguage;
            writer.AllowGambling = AllowGambling;
            writer.AgeRating = AgeRating;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool AllowViolence
        {
            get;
            set;
        }

        public bool AllowLanguage
        {
            get;
            set;
        }

        public bool AllowGambling
        {
            get;
            set;
        }

        public string AgeRating
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
            public bool AllowViolence => ctx.ReadDataBool(0UL, false);
            public bool AllowLanguage => ctx.ReadDataBool(1UL, false);
            public bool AllowGambling => ctx.ReadDataBool(2UL, false);
            public string AgeRating => ctx.ReadText(0, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public bool AllowViolence
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public bool AllowLanguage
            {
                get => this.ReadDataBool(1UL, false);
                set => this.WriteData(1UL, value, false);
            }

            public bool AllowGambling
            {
                get => this.ReadDataBool(2UL, false);
                set => this.WriteData(2UL, value, false);
            }

            public string AgeRating
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa07ce161722a6049UL)]
    public class NotificationPrefs : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa07ce161722a6049UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchReady = reader.MatchReady;
            FriendOnline = reader.FriendOnline;
            TournamentStart = reader.TournamentStart;
            RewardReceived = reader.RewardReceived;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchReady = MatchReady;
            writer.FriendOnline = FriendOnline;
            writer.TournamentStart = TournamentStart;
            writer.RewardReceived = RewardReceived;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool MatchReady
        {
            get;
            set;
        }

        public bool FriendOnline
        {
            get;
            set;
        }

        public bool TournamentStart
        {
            get;
            set;
        }

        public bool RewardReceived
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
            public bool MatchReady => ctx.ReadDataBool(0UL, false);
            public bool FriendOnline => ctx.ReadDataBool(1UL, false);
            public bool TournamentStart => ctx.ReadDataBool(2UL, false);
            public bool RewardReceived => ctx.ReadDataBool(3UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public bool MatchReady
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public bool FriendOnline
            {
                get => this.ReadDataBool(1UL, false);
                set => this.WriteData(1UL, value, false);
            }

            public bool TournamentStart
            {
                get => this.ReadDataBool(2UL, false);
                set => this.WriteData(2UL, value, false);
            }

            public bool RewardReceived
            {
                get => this.ReadDataBool(3UL, false);
                set => this.WriteData(3UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd49db6ea57f6b177UL)]
    public class PlayerRestrictions : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd49db6ea57f6b177UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            IsBanned = reader.IsBanned;
            BanExpiry = reader.BanExpiry;
            BanReason = reader.BanReason;
            MatchmakingCoolDown = reader.MatchmakingCoolDown;
            DailyMatchLimit = reader.DailyMatchLimit;
            ChatRestricted = reader.ChatRestricted;
            VoiceRestricted = reader.VoiceRestricted;
            ReportCount = reader.ReportCount;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.IsBanned = IsBanned;
            writer.BanExpiry = BanExpiry;
            writer.BanReason = BanReason;
            writer.MatchmakingCoolDown = MatchmakingCoolDown;
            writer.DailyMatchLimit = DailyMatchLimit;
            writer.ChatRestricted = ChatRestricted;
            writer.VoiceRestricted = VoiceRestricted;
            writer.ReportCount = ReportCount;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool IsBanned
        {
            get;
            set;
        }

        public ulong BanExpiry
        {
            get;
            set;
        }

        public string BanReason
        {
            get;
            set;
        }

        public ulong MatchmakingCoolDown
        {
            get;
            set;
        }

        public uint DailyMatchLimit
        {
            get;
            set;
        }

        public bool ChatRestricted
        {
            get;
            set;
        }

        public bool VoiceRestricted
        {
            get;
            set;
        }

        public uint ReportCount
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
            public bool IsBanned => ctx.ReadDataBool(0UL, false);
            public ulong BanExpiry => ctx.ReadDataULong(64UL, 0UL);
            public string BanReason => ctx.ReadText(0, null);
            public ulong MatchmakingCoolDown => ctx.ReadDataULong(128UL, 0UL);
            public uint DailyMatchLimit => ctx.ReadDataUInt(32UL, 0U);
            public bool ChatRestricted => ctx.ReadDataBool(1UL, false);
            public bool VoiceRestricted => ctx.ReadDataBool(2UL, false);
            public uint ReportCount => ctx.ReadDataUInt(192UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 1);
            }

            public bool IsBanned
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public ulong BanExpiry
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public string BanReason
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ulong MatchmakingCoolDown
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public uint DailyMatchLimit
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public bool ChatRestricted
            {
                get => this.ReadDataBool(1UL, false);
                set => this.WriteData(1UL, value, false);
            }

            public bool VoiceRestricted
            {
                get => this.ReadDataBool(2UL, false);
                set => this.WriteData(2UL, value, false);
            }

            public uint ReportCount
            {
                get => this.ReadDataUInt(192UL, 0U);
                set => this.WriteData(192UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd977142f95139984UL)]
    public class FriendInfo : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd977142f95139984UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            DisplayName = reader.DisplayName;
            Status = reader.Status;
            FriendshipDate = reader.FriendshipDate;
            LastPlayedTogether = reader.LastPlayedTogether;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId.Init(PlayerId);
            writer.DisplayName = DisplayName;
            writer.Status = Status;
            writer.FriendshipDate = FriendshipDate;
            writer.LastPlayedTogether = LastPlayedTogether;
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

        public CapnpGen.FriendStatus Status
        {
            get;
            set;
        }

        public ulong FriendshipDate
        {
            get;
            set;
        }

        public ulong LastPlayedTogether
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
            public CapnpGen.FriendStatus Status => (CapnpGen.FriendStatus)ctx.ReadDataUShort(0UL, (ushort)0);
            public ulong FriendshipDate => ctx.ReadDataULong(64UL, 0UL);
            public ulong LastPlayedTogether => ctx.ReadDataULong(128UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 2);
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

            public CapnpGen.FriendStatus Status
            {
                get => (CapnpGen.FriendStatus)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ulong FriendshipDate
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong LastPlayedTogether
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd4e351f3a41061b7UL)]
    public enum FriendStatus : ushort
    {
        offline,
        online,
        inGame,
        away,
        doNotDisturb
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x85d4fee15ea17643UL)]
    public class BlockedPlayer : ICapnpSerializable
    {
        public const UInt64 typeId = 0x85d4fee15ea17643UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            DisplayName = reader.DisplayName;
            BlockedAt = reader.BlockedAt;
            Reason = reader.Reason;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId.Init(PlayerId);
            writer.DisplayName = DisplayName;
            writer.BlockedAt = BlockedAt;
            writer.Reason = Reason;
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

        public ulong BlockedAt
        {
            get;
            set;
        }

        public string Reason
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
            public ulong BlockedAt => ctx.ReadDataULong(0UL, 0UL);
            public string Reason => ctx.ReadText(2, null);
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

            public ulong BlockedAt
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public string Reason
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd35ae6477184f6a1UL)]
    public class PlayerProfile : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd35ae6477184f6a1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            DisplayName = reader.DisplayName;
            WalletAddress = reader.WalletAddress;
            GlobalMMR = CapnpSerializable.Create<CapnpGen.MMR>(reader.GlobalMMR);
            GameStats = reader.GameStats?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.GameStats>(_));
            TotalPlayTimeMs = reader.TotalPlayTimeMs;
            Attributes = CapnpSerializable.Create<CapnpGen.PlayerAttributes>(reader.Attributes);
            Preferences = CapnpSerializable.Create<CapnpGen.PlayerPreferences>(reader.Preferences);
            Friends = reader.Friends?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.FriendInfo>(_));
            BlockedPlayers = reader.BlockedPlayers?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.BlockedPlayer>(_));
            Restrictions = CapnpSerializable.Create<CapnpGen.PlayerRestrictions>(reader.Restrictions);
            CreatedAt = reader.CreatedAt;
            LastLogin = reader.LastLogin;
            IsOnline = reader.IsOnline;
            InventoryId = reader.InventoryId;
            LoadoutIds = reader.LoadoutIds;
            Achievements = reader.Achievements?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Achievement>(_));
            Title = reader.Title;
            AvatarUrl = reader.AvatarUrl;
            BannerUrl = reader.BannerUrl;
            Biography = reader.Biography;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId.Init(PlayerId);
            writer.DisplayName = DisplayName;
            writer.WalletAddress.Init(WalletAddress);
            GlobalMMR?.serialize(writer.GlobalMMR);
            writer.GameStats.Init(GameStats, (_s1, _v1) => _v1?.serialize(_s1));
            writer.TotalPlayTimeMs = TotalPlayTimeMs;
            Attributes?.serialize(writer.Attributes);
            Preferences?.serialize(writer.Preferences);
            writer.Friends.Init(Friends, (_s1, _v1) => _v1?.serialize(_s1));
            writer.BlockedPlayers.Init(BlockedPlayers, (_s1, _v1) => _v1?.serialize(_s1));
            Restrictions?.serialize(writer.Restrictions);
            writer.CreatedAt = CreatedAt;
            writer.LastLogin = LastLogin;
            writer.IsOnline = IsOnline;
            writer.InventoryId.Init(InventoryId);
            writer.LoadoutIds.Init(LoadoutIds, (_s1, _v1) => _s1.Init(_v1));
            writer.Achievements.Init(Achievements, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Title = Title;
            writer.AvatarUrl = AvatarUrl;
            writer.BannerUrl = BannerUrl;
            writer.Biography = Biography;
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

        public IReadOnlyList<byte> WalletAddress
        {
            get;
            set;
        }

        public CapnpGen.MMR GlobalMMR
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.GameStats> GameStats
        {
            get;
            set;
        }

        public ulong TotalPlayTimeMs
        {
            get;
            set;
        }

        public CapnpGen.PlayerAttributes Attributes
        {
            get;
            set;
        }

        public CapnpGen.PlayerPreferences Preferences
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.FriendInfo> Friends
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.BlockedPlayer> BlockedPlayers
        {
            get;
            set;
        }

        public CapnpGen.PlayerRestrictions Restrictions
        {
            get;
            set;
        }

        public ulong CreatedAt
        {
            get;
            set;
        }

        public ulong LastLogin
        {
            get;
            set;
        }

        public bool IsOnline
        {
            get;
            set;
        }

        public IReadOnlyList<byte> InventoryId
        {
            get;
            set;
        }

        public IReadOnlyList<IReadOnlyList<byte>> LoadoutIds
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Achievement> Achievements
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string AvatarUrl
        {
            get;
            set;
        }

        public string BannerUrl
        {
            get;
            set;
        }

        public string Biography
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
            public IReadOnlyList<byte> WalletAddress => ctx.ReadList(2).CastByte();
            public CapnpGen.MMR.READER GlobalMMR => ctx.ReadStruct(3, CapnpGen.MMR.READER.create);
            public IReadOnlyList<CapnpGen.GameStats.READER> GameStats => ctx.ReadList(4).Cast(CapnpGen.GameStats.READER.create);
            public ulong TotalPlayTimeMs => ctx.ReadDataULong(0UL, 0UL);
            public CapnpGen.PlayerAttributes.READER Attributes => ctx.ReadStruct(5, CapnpGen.PlayerAttributes.READER.create);
            public CapnpGen.PlayerPreferences.READER Preferences => ctx.ReadStruct(6, CapnpGen.PlayerPreferences.READER.create);
            public IReadOnlyList<CapnpGen.FriendInfo.READER> Friends => ctx.ReadList(7).Cast(CapnpGen.FriendInfo.READER.create);
            public IReadOnlyList<CapnpGen.BlockedPlayer.READER> BlockedPlayers => ctx.ReadList(8).Cast(CapnpGen.BlockedPlayer.READER.create);
            public CapnpGen.PlayerRestrictions.READER Restrictions => ctx.ReadStruct(9, CapnpGen.PlayerRestrictions.READER.create);
            public ulong CreatedAt => ctx.ReadDataULong(64UL, 0UL);
            public ulong LastLogin => ctx.ReadDataULong(128UL, 0UL);
            public bool IsOnline => ctx.ReadDataBool(192UL, false);
            public IReadOnlyList<byte> InventoryId => ctx.ReadList(10).CastByte();
            public IReadOnlyList<IReadOnlyList<byte>> LoadoutIds => ctx.ReadList(11).CastData();
            public IReadOnlyList<CapnpGen.Achievement.READER> Achievements => ctx.ReadList(12).Cast(CapnpGen.Achievement.READER.create);
            public string Title => ctx.ReadText(13, null);
            public string AvatarUrl => ctx.ReadText(14, null);
            public string BannerUrl => ctx.ReadText(15, null);
            public string Biography => ctx.ReadText(16, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 17);
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

            public ListOfPrimitivesSerializer<byte> WalletAddress
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public CapnpGen.MMR.WRITER GlobalMMR
            {
                get => BuildPointer<CapnpGen.MMR.WRITER>(3);
                set => Link(3, value);
            }

            public ListOfStructsSerializer<CapnpGen.GameStats.WRITER> GameStats
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.GameStats.WRITER>>(4);
                set => Link(4, value);
            }

            public ulong TotalPlayTimeMs
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public CapnpGen.PlayerAttributes.WRITER Attributes
            {
                get => BuildPointer<CapnpGen.PlayerAttributes.WRITER>(5);
                set => Link(5, value);
            }

            public CapnpGen.PlayerPreferences.WRITER Preferences
            {
                get => BuildPointer<CapnpGen.PlayerPreferences.WRITER>(6);
                set => Link(6, value);
            }

            public ListOfStructsSerializer<CapnpGen.FriendInfo.WRITER> Friends
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.FriendInfo.WRITER>>(7);
                set => Link(7, value);
            }

            public ListOfStructsSerializer<CapnpGen.BlockedPlayer.WRITER> BlockedPlayers
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.BlockedPlayer.WRITER>>(8);
                set => Link(8, value);
            }

            public CapnpGen.PlayerRestrictions.WRITER Restrictions
            {
                get => BuildPointer<CapnpGen.PlayerRestrictions.WRITER>(9);
                set => Link(9, value);
            }

            public ulong CreatedAt
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong LastLogin
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public bool IsOnline
            {
                get => this.ReadDataBool(192UL, false);
                set => this.WriteData(192UL, value, false);
            }

            public ListOfPrimitivesSerializer<byte> InventoryId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(10);
                set => Link(10, value);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> LoadoutIds
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(11);
                set => Link(11, value);
            }

            public ListOfStructsSerializer<CapnpGen.Achievement.WRITER> Achievements
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.Achievement.WRITER>>(12);
                set => Link(12, value);
            }

            public string Title
            {
                get => this.ReadText(13, null);
                set => this.WriteText(13, value, null);
            }

            public string AvatarUrl
            {
                get => this.ReadText(14, null);
                set => this.WriteText(14, value, null);
            }

            public string BannerUrl
            {
                get => this.ReadText(15, null);
                set => this.WriteText(15, value, null);
            }

            public string Biography
            {
                get => this.ReadText(16, null);
                set => this.WriteText(16, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc0fa717ca022d22dUL), Proxy(typeof(PlayerProfileService_Proxy)), Skeleton(typeof(PlayerProfileService_Skeleton))]
    public interface IPlayerProfileService : IDisposable
    {
        Task<IReadOnlyList<byte>> CreateOrUpdateProfile(IReadOnlyList<byte> playerId, string displayName, IReadOnlyList<byte> walletAddress, string preferredRole, string language, string platform, string region, uint maxPingMs, float initialMmr, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.PlayerProfile> GetProfile(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default);
        Task RecordMatchResult(IReadOnlyList<byte> playerId, IReadOnlyList<byte> opponentId, IReadOnlyList<byte> gameId, float score, ulong playTimeMs, CancellationToken cancellationToken_ = default);
        Task SetOffline(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.PlayerProfile>> ListPlayers(CancellationToken cancellationToken_ = default);
        Task ApplyRestriction(IReadOnlyList<byte> playerId, bool ban, ulong cooldownMs, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc0fa717ca022d22dUL)]
    public class PlayerProfileService_Proxy : Proxy, IPlayerProfileService
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc0fa717ca022d22dUL)]
    public class PlayerProfileService_Skeleton : Skeleton<IPlayerProfileService>
    {
        public PlayerProfileService_Skeleton()
        {
            SetMethodTable(CreateOrUpdateProfile, GetProfile, RecordMatchResult, SetOffline, ListPlayers, ApplyRestriction);
        }

        public override ulong InterfaceId => 13905551579536347693UL;
        Task<AnswerOrCounterquestion> CreateOrUpdateProfile(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Params_CreateOrUpdateProfile>(d_);
                return Impatient.MaybeTailCall(Impl.CreateOrUpdateProfile(in_.PlayerId, in_.DisplayName, in_.WalletAddress, in_.PreferredRole, in_.Language, in_.Platform, in_.Region, in_.MaxPingMs, in_.InitialMmr, cancellationToken_), playerId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Result_CreateOrUpdateProfile.WRITER>();
                    var r_ = new CapnpGen.PlayerProfileService.Result_CreateOrUpdateProfile{PlayerId = playerId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetProfile(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Params_GetProfile>(d_);
                return Impatient.MaybeTailCall(Impl.GetProfile(in_.PlayerId, cancellationToken_), profile =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Result_GetProfile.WRITER>();
                    var r_ = new CapnpGen.PlayerProfileService.Result_GetProfile{Profile = profile};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> RecordMatchResult(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Params_RecordMatchResult>(d_);
                await Impl.RecordMatchResult(in_.PlayerId, in_.OpponentId, in_.GameId, in_.Score, in_.PlayTimeMs, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Result_RecordMatchResult.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> SetOffline(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Params_SetOffline>(d_);
                await Impl.SetOffline(in_.PlayerId, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Result_SetOffline.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> ListPlayers(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                return Impatient.MaybeTailCall(Impl.ListPlayers(cancellationToken_), players =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Result_ListPlayers.WRITER>();
                    var r_ = new CapnpGen.PlayerProfileService.Result_ListPlayers{Players = players};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> ApplyRestriction(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.PlayerProfileService.Params_ApplyRestriction>(d_);
                await Impl.ApplyRestriction(in_.PlayerId, in_.Ban, in_.CooldownMs, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.PlayerProfileService.Result_ApplyRestriction.WRITER>();
                return s_;
            }
        }
    }

    public static class PlayerProfileService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9323e67992a0fc46UL)]
        public class Params_CreateOrUpdateProfile : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9323e67992a0fc46UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                DisplayName = reader.DisplayName;
                WalletAddress = reader.WalletAddress;
                PreferredRole = reader.PreferredRole;
                Language = reader.Language;
                Platform = reader.Platform;
                Region = reader.Region;
                MaxPingMs = reader.MaxPingMs;
                InitialMmr = reader.InitialMmr;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.DisplayName = DisplayName;
                writer.WalletAddress.Init(WalletAddress);
                writer.PreferredRole = PreferredRole;
                writer.Language = Language;
                writer.Platform = Platform;
                writer.Region = Region;
                writer.MaxPingMs = MaxPingMs;
                writer.InitialMmr = InitialMmr;
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

            public IReadOnlyList<byte> WalletAddress
            {
                get;
                set;
            }

            public string PreferredRole
            {
                get;
                set;
            }

            public string Language
            {
                get;
                set;
            }

            public string Platform
            {
                get;
                set;
            }

            public string Region
            {
                get;
                set;
            }

            public uint MaxPingMs
            {
                get;
                set;
            }

            public float InitialMmr
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
                public IReadOnlyList<byte> WalletAddress => ctx.ReadList(2).CastByte();
                public string PreferredRole => ctx.ReadText(3, null);
                public string Language => ctx.ReadText(4, null);
                public string Platform => ctx.ReadText(5, null);
                public string Region => ctx.ReadText(6, null);
                public uint MaxPingMs => ctx.ReadDataUInt(0UL, 0U);
                public float InitialMmr => ctx.ReadDataFloat(32UL, 0F);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 7);
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

                public ListOfPrimitivesSerializer<byte> WalletAddress
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }

                public string PreferredRole
                {
                    get => this.ReadText(3, null);
                    set => this.WriteText(3, value, null);
                }

                public string Language
                {
                    get => this.ReadText(4, null);
                    set => this.WriteText(4, value, null);
                }

                public string Platform
                {
                    get => this.ReadText(5, null);
                    set => this.WriteText(5, value, null);
                }

                public string Region
                {
                    get => this.ReadText(6, null);
                    set => this.WriteText(6, value, null);
                }

                public uint MaxPingMs
                {
                    get => this.ReadDataUInt(0UL, 0U);
                    set => this.WriteData(0UL, value, 0U);
                }

                public float InitialMmr
                {
                    get => this.ReadDataFloat(32UL, 0F);
                    set => this.WriteData(32UL, value, 0F);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd19e00ef210788b0UL)]
        public class Result_CreateOrUpdateProfile : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd19e00ef210788b0UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
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
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa7068d812dfd3713UL)]
        public class Params_GetProfile : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa7068d812dfd3713UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
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
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfb7a6132172e34f8UL)]
        public class Result_GetProfile : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfb7a6132172e34f8UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Profile = CapnpSerializable.Create<CapnpGen.PlayerProfile>(reader.Profile);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Profile?.serialize(writer.Profile);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.PlayerProfile Profile
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
                public CapnpGen.PlayerProfile.READER Profile => ctx.ReadStruct(0, CapnpGen.PlayerProfile.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.PlayerProfile.WRITER Profile
                {
                    get => BuildPointer<CapnpGen.PlayerProfile.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe36ed6db4288dbb7UL)]
        public class Params_RecordMatchResult : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe36ed6db4288dbb7UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                OpponentId = reader.OpponentId;
                GameId = reader.GameId;
                Score = reader.Score;
                PlayTimeMs = reader.PlayTimeMs;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.OpponentId.Init(OpponentId);
                writer.GameId.Init(GameId);
                writer.Score = Score;
                writer.PlayTimeMs = PlayTimeMs;
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

            public IReadOnlyList<byte> OpponentId
            {
                get;
                set;
            }

            public IReadOnlyList<byte> GameId
            {
                get;
                set;
            }

            public float Score
            {
                get;
                set;
            }

            public ulong PlayTimeMs
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
                public IReadOnlyList<byte> OpponentId => ctx.ReadList(1).CastByte();
                public IReadOnlyList<byte> GameId => ctx.ReadList(2).CastByte();
                public float Score => ctx.ReadDataFloat(0UL, 0F);
                public ulong PlayTimeMs => ctx.ReadDataULong(64UL, 0UL);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(2, 3);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> OpponentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> GameId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }

                public float Score
                {
                    get => this.ReadDataFloat(0UL, 0F);
                    set => this.WriteData(0UL, value, 0F);
                }

                public ulong PlayTimeMs
                {
                    get => this.ReadDataULong(64UL, 0UL);
                    set => this.WriteData(64UL, value, 0UL);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfecb99b9229a2575UL)]
        public class Result_RecordMatchResult : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfecb99b9229a2575UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8d1c67db5e5181a1UL)]
        public class Params_SetOffline : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8d1c67db5e5181a1UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
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
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8158e2f541e1f1e6UL)]
        public class Result_SetOffline : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8158e2f541e1f1e6UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x952da071d8bf8d88UL)]
        public class Params_ListPlayers : ICapnpSerializable
        {
            public const UInt64 typeId = 0x952da071d8bf8d88UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9f1c400d21b68f00UL)]
        public class Result_ListPlayers : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9f1c400d21b68f00UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Players = reader.Players?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PlayerProfile>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Players.Init(Players, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.PlayerProfile> Players
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
                public IReadOnlyList<CapnpGen.PlayerProfile.READER> Players => ctx.ReadList(0).Cast(CapnpGen.PlayerProfile.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.PlayerProfile.WRITER> Players
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.PlayerProfile.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb8ddc8b8394a86b7UL)]
        public class Params_ApplyRestriction : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb8ddc8b8394a86b7UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                Ban = reader.Ban;
                CooldownMs = reader.CooldownMs;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.Ban = Ban;
                writer.CooldownMs = CooldownMs;
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

            public bool Ban
            {
                get;
                set;
            }

            public ulong CooldownMs
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
                public bool Ban => ctx.ReadDataBool(0UL, false);
                public ulong CooldownMs => ctx.ReadDataULong(64UL, 0UL);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(2, 1);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public bool Ban
                {
                    get => this.ReadDataBool(0UL, false);
                    set => this.WriteData(0UL, value, false);
                }

                public ulong CooldownMs
                {
                    get => this.ReadDataULong(64UL, 0UL);
                    set => this.WriteData(64UL, value, 0UL);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc56fb4d493ff8207UL)]
        public class Result_ApplyRestriction : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc56fb4d493ff8207UL;
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
}