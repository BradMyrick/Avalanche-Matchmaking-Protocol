using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc8a21012e99b24cbUL)]
    public class MatchmakingRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc8a21012e99b24cbUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RuleId = reader.RuleId;
            Name = reader.Name;
            Description = reader.Description;
            Type = reader.Type;
            Parameters = reader.Parameters;
            Weight = reader.Weight;
            IsHardConstraint = reader.IsHardConstraint;
            Priority = reader.Priority;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RuleId = RuleId;
            writer.Name = Name;
            writer.Description = Description;
            writer.Type = Type;
            writer.Parameters.Init(Parameters);
            writer.Weight = Weight;
            writer.IsHardConstraint = IsHardConstraint;
            writer.Priority = Priority;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string RuleId
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

        public CapnpGen.RuleType Type
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Parameters
        {
            get;
            set;
        }

        public float Weight
        {
            get;
            set;
        }

        public bool IsHardConstraint
        {
            get;
            set;
        }

        public byte Priority
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
            public string RuleId => ctx.ReadText(0, null);
            public string Name => ctx.ReadText(1, null);
            public string Description => ctx.ReadText(2, null);
            public CapnpGen.RuleType Type => (CapnpGen.RuleType)ctx.ReadDataUShort(0UL, (ushort)0);
            public IReadOnlyList<byte> Parameters => ctx.ReadList(3).CastByte();
            public float Weight => ctx.ReadDataFloat(32UL, 0F);
            public bool IsHardConstraint => ctx.ReadDataBool(16UL, false);
            public byte Priority => ctx.ReadDataByte(24UL, (byte)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 4);
            }

            public string RuleId
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
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

            public CapnpGen.RuleType Type
            {
                get => (CapnpGen.RuleType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> Parameters
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public float Weight
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public bool IsHardConstraint
            {
                get => this.ReadDataBool(16UL, false);
                set => this.WriteData(16UL, value, false);
            }

            public byte Priority
            {
                get => this.ReadDataByte(24UL, (byte)0);
                set => this.WriteData(24UL, value, (byte)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc39eb4356c790c6bUL)]
    public enum RuleType : ushort
    {
        latency,
        skill,
        teamBalance,
        region,
        language,
        schedule,
        inventory,
        party,
        avoidance,
        preference,
        custom,
        pingBased,
        skillDecay,
        recentMatches,
        connectionQuality
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcb6fd42a231a38e5UL)]
    public class LatencyRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0xcb6fd42a231a38e5UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MaxPingMs = reader.MaxPingMs;
            MeasurementMethod = reader.MeasurementMethod;
            RegionOverride = reader.RegionOverride;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MaxPingMs = MaxPingMs;
            writer.MeasurementMethod = MeasurementMethod;
            writer.RegionOverride = RegionOverride;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint MaxPingMs
        {
            get;
            set;
        }

        public string MeasurementMethod
        {
            get;
            set;
        }

        public bool RegionOverride
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
            public uint MaxPingMs => ctx.ReadDataUInt(0UL, 0U);
            public string MeasurementMethod => ctx.ReadText(0, null);
            public bool RegionOverride => ctx.ReadDataBool(32UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public uint MaxPingMs
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public string MeasurementMethod
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public bool RegionOverride
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9782a7b58589b6c6UL)]
    public class SkillRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9782a7b58589b6c6UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MaxDifference = reader.MaxDifference;
            UseTrueSkill = reader.UseTrueSkill;
            TeamVariance = reader.TeamVariance;
            TimeDecay = reader.TimeDecay;
            DecayRate = reader.DecayRate;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MaxDifference = MaxDifference;
            writer.UseTrueSkill = UseTrueSkill;
            writer.TeamVariance = TeamVariance;
            writer.TimeDecay = TimeDecay;
            writer.DecayRate = DecayRate;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float MaxDifference
        {
            get;
            set;
        }

        public bool UseTrueSkill
        {
            get;
            set;
        }

        public float TeamVariance
        {
            get;
            set;
        }

        public bool TimeDecay
        {
            get;
            set;
        }

        public float DecayRate
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
            public float MaxDifference => ctx.ReadDataFloat(0UL, 0F);
            public bool UseTrueSkill => ctx.ReadDataBool(32UL, false);
            public float TeamVariance => ctx.ReadDataFloat(64UL, 0F);
            public bool TimeDecay => ctx.ReadDataBool(33UL, false);
            public float DecayRate => ctx.ReadDataFloat(96UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public float MaxDifference
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public bool UseTrueSkill
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }

            public float TeamVariance
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public bool TimeDecay
            {
                get => this.ReadDataBool(33UL, false);
                set => this.WriteData(33UL, value, false);
            }

            public float DecayRate
            {
                get => this.ReadDataFloat(96UL, 0F);
                set => this.WriteData(96UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc9cfbc61ebe2466aUL)]
    public class TeamBalanceRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc9cfbc61ebe2466aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RequiredRoles = reader.RequiredRoles;
            MaxDuplicates = reader.MaxDuplicates;
            Composition = CapnpSerializable.Create<CapnpGen.TeamComposition>(reader.Composition);
            RoleWeights = reader.RoleWeights;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RequiredRoles.Init(RequiredRoles);
            writer.MaxDuplicates = MaxDuplicates;
            Composition?.serialize(writer.Composition);
            writer.RoleWeights.Init(RoleWeights);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<string> RequiredRoles
        {
            get;
            set;
        }

        public byte MaxDuplicates
        {
            get;
            set;
        }

        public CapnpGen.TeamComposition Composition
        {
            get;
            set;
        }

        public IReadOnlyList<byte> RoleWeights
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
            public IReadOnlyList<string> RequiredRoles => ctx.ReadList(0).CastText2();
            public byte MaxDuplicates => ctx.ReadDataByte(0UL, (byte)0);
            public CapnpGen.TeamComposition.READER Composition => ctx.ReadStruct(1, CapnpGen.TeamComposition.READER.create);
            public IReadOnlyList<byte> RoleWeights => ctx.ReadList(2).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public ListOfTextSerializer RequiredRoles
            {
                get => BuildPointer<ListOfTextSerializer>(0);
                set => Link(0, value);
            }

            public byte MaxDuplicates
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public CapnpGen.TeamComposition.WRITER Composition
            {
                get => BuildPointer<CapnpGen.TeamComposition.WRITER>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> RoleWeights
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xda3f8dc6fa3553a9UL)]
    public class TeamComposition : ICapnpSerializable
    {
        public const UInt64 typeId = 0xda3f8dc6fa3553a9UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TeamCount = reader.TeamCount;
            RolesPerTeam = reader.RolesPerTeam?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RoleRequirement>(_));
            FlexSlots = reader.FlexSlots;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TeamCount = TeamCount;
            writer.RolesPerTeam.Init(RolesPerTeam, (_s1, _v1) => _v1?.serialize(_s1));
            writer.FlexSlots = FlexSlots;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public byte TeamCount
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.RoleRequirement> RolesPerTeam
        {
            get;
            set;
        }

        public byte FlexSlots
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
            public byte TeamCount => ctx.ReadDataByte(0UL, (byte)0);
            public IReadOnlyList<CapnpGen.RoleRequirement.READER> RolesPerTeam => ctx.ReadList(0).Cast(CapnpGen.RoleRequirement.READER.create);
            public byte FlexSlots => ctx.ReadDataByte(8UL, (byte)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public byte TeamCount
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public ListOfStructsSerializer<CapnpGen.RoleRequirement.WRITER> RolesPerTeam
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.RoleRequirement.WRITER>>(0);
                set => Link(0, value);
            }

            public byte FlexSlots
            {
                get => this.ReadDataByte(8UL, (byte)0);
                set => this.WriteData(8UL, value, (byte)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9118dd56d4a085acUL)]
    public class RoleRequirement : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9118dd56d4a085acUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Role = reader.Role;
            MinPlayers = reader.MinPlayers;
            MaxPlayers = reader.MaxPlayers;
            Priority = reader.Priority;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Role = Role;
            writer.MinPlayers = MinPlayers;
            writer.MaxPlayers = MaxPlayers;
            writer.Priority = Priority;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string Role
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

        public byte Priority
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
            public string Role => ctx.ReadText(0, null);
            public byte MinPlayers => ctx.ReadDataByte(0UL, (byte)0);
            public byte MaxPlayers => ctx.ReadDataByte(8UL, (byte)0);
            public byte Priority => ctx.ReadDataByte(16UL, (byte)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public string Role
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
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

            public byte Priority
            {
                get => this.ReadDataByte(16UL, (byte)0);
                set => this.WriteData(16UL, value, (byte)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x87ea729e14d01429UL)]
    public class InventoryRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0x87ea729e14d01429UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RequiredItems = reader.RequiredItems?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.ItemRequirement>(_));
            BannedItems = reader.BannedItems;
            MaxItemRarity = reader.MaxItemRarity;
            MinCollectionScore = reader.MinCollectionScore;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RequiredItems.Init(RequiredItems, (_s1, _v1) => _v1?.serialize(_s1));
            writer.BannedItems.Init(BannedItems, (_s1, _v1) => _s1.Init(_v1));
            writer.MaxItemRarity = MaxItemRarity;
            writer.MinCollectionScore = MinCollectionScore;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<CapnpGen.ItemRequirement> RequiredItems
        {
            get;
            set;
        }

        public IReadOnlyList<IReadOnlyList<byte>> BannedItems
        {
            get;
            set;
        }

        public string MaxItemRarity
        {
            get;
            set;
        }

        public uint MinCollectionScore
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
            public IReadOnlyList<CapnpGen.ItemRequirement.READER> RequiredItems => ctx.ReadList(0).Cast(CapnpGen.ItemRequirement.READER.create);
            public IReadOnlyList<IReadOnlyList<byte>> BannedItems => ctx.ReadList(1).CastData();
            public string MaxItemRarity => ctx.ReadText(2, null);
            public uint MinCollectionScore => ctx.ReadDataUInt(0UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public ListOfStructsSerializer<CapnpGen.ItemRequirement.WRITER> RequiredItems
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.ItemRequirement.WRITER>>(0);
                set => Link(0, value);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> BannedItems
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(1);
                set => Link(1, value);
            }

            public string MaxItemRarity
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }

            public uint MinCollectionScore
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdd8dfc9af82f81eeUL)]
    public class ItemRequirement : ICapnpSerializable
    {
        public const UInt64 typeId = 0xdd8dfc9af82f81eeUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ItemId = reader.ItemId;
            MinQuantity = reader.MinQuantity;
            Slot = reader.Slot;
            Alternatives = reader.Alternatives;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ItemId.Init(ItemId);
            writer.MinQuantity = MinQuantity;
            writer.Slot = Slot;
            writer.Alternatives.Init(Alternatives, (_s1, _v1) => _s1.Init(_v1));
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

        public uint MinQuantity
        {
            get;
            set;
        }

        public string Slot
        {
            get;
            set;
        }

        public IReadOnlyList<IReadOnlyList<byte>> Alternatives
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
            public uint MinQuantity => ctx.ReadDataUInt(0UL, 0U);
            public string Slot => ctx.ReadText(1, null);
            public IReadOnlyList<IReadOnlyList<byte>> Alternatives => ctx.ReadList(2).CastData();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public ListOfPrimitivesSerializer<byte> ItemId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public uint MinQuantity
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public string Slot
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> Alternatives
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xea399d6deafa9935UL)]
    public class PartyRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0xea399d6deafa9935UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            AllowMixedParties = reader.AllowMixedParties;
            MaxPartySize = reader.MaxPartySize;
            PartySkillCalculation = reader.PartySkillCalculation;
            PartyRatingAdjustment = reader.PartyRatingAdjustment;
            SoloQueueBonus = reader.SoloQueueBonus;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.AllowMixedParties = AllowMixedParties;
            writer.MaxPartySize = MaxPartySize;
            writer.PartySkillCalculation = PartySkillCalculation;
            writer.PartyRatingAdjustment = PartyRatingAdjustment;
            writer.SoloQueueBonus = SoloQueueBonus;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool AllowMixedParties
        {
            get;
            set;
        }

        public byte MaxPartySize
        {
            get;
            set;
        }

        public CapnpGen.PartySkillMethod PartySkillCalculation
        {
            get;
            set;
        }

        public float PartyRatingAdjustment
        {
            get;
            set;
        }

        public bool SoloQueueBonus
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
            public bool AllowMixedParties => ctx.ReadDataBool(0UL, false);
            public byte MaxPartySize => ctx.ReadDataByte(8UL, (byte)0);
            public CapnpGen.PartySkillMethod PartySkillCalculation => (CapnpGen.PartySkillMethod)ctx.ReadDataUShort(16UL, (ushort)0);
            public float PartyRatingAdjustment => ctx.ReadDataFloat(32UL, 0F);
            public bool SoloQueueBonus => ctx.ReadDataBool(1UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public bool AllowMixedParties
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public byte MaxPartySize
            {
                get => this.ReadDataByte(8UL, (byte)0);
                set => this.WriteData(8UL, value, (byte)0);
            }

            public CapnpGen.PartySkillMethod PartySkillCalculation
            {
                get => (CapnpGen.PartySkillMethod)this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, (ushort)value, (ushort)0);
            }

            public float PartyRatingAdjustment
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public bool SoloQueueBonus
            {
                get => this.ReadDataBool(1UL, false);
                set => this.WriteData(1UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xda991e110ef3601aUL)]
    public enum PartySkillMethod : ushort
    {
        highest,
        average,
        weighted,
        adjustedAverage
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfebea11d4aa61705UL)]
    public class AvoidanceRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0xfebea11d4aa61705UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MaxRecentOpponents = reader.MaxRecentOpponents;
            AvoidFriends = reader.AvoidFriends;
            AvoidBlocked = reader.AvoidBlocked;
            CooldownMinutes = reader.CooldownMinutes;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MaxRecentOpponents = MaxRecentOpponents;
            writer.AvoidFriends = AvoidFriends;
            writer.AvoidBlocked = AvoidBlocked;
            writer.CooldownMinutes = CooldownMinutes;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public byte MaxRecentOpponents
        {
            get;
            set;
        }

        public bool AvoidFriends
        {
            get;
            set;
        }

        public bool AvoidBlocked
        {
            get;
            set;
        }

        public uint CooldownMinutes
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
            public byte MaxRecentOpponents => ctx.ReadDataByte(0UL, (byte)0);
            public bool AvoidFriends => ctx.ReadDataBool(8UL, false);
            public bool AvoidBlocked => ctx.ReadDataBool(9UL, false);
            public uint CooldownMinutes => ctx.ReadDataUInt(32UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public byte MaxRecentOpponents
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public bool AvoidFriends
            {
                get => this.ReadDataBool(8UL, false);
                set => this.WriteData(8UL, value, false);
            }

            public bool AvoidBlocked
            {
                get => this.ReadDataBool(9UL, false);
                set => this.WriteData(9UL, value, false);
            }

            public uint CooldownMinutes
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd247105509285fd2UL)]
    public class ScheduleRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd247105509285fd2UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TimeWindows = reader.TimeWindows?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.TimeWindow>(_));
            DayOfWeek = reader.DayOfWeek;
            TimeZone = reader.TimeZone;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TimeWindows.Init(TimeWindows, (_s1, _v1) => _v1?.serialize(_s1));
            writer.DayOfWeek.Init(DayOfWeek);
            writer.TimeZone = TimeZone;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<CapnpGen.TimeWindow> TimeWindows
        {
            get;
            set;
        }

        public IReadOnlyList<byte> DayOfWeek
        {
            get;
            set;
        }

        public string TimeZone
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
            public IReadOnlyList<CapnpGen.TimeWindow.READER> TimeWindows => ctx.ReadList(0).Cast(CapnpGen.TimeWindow.READER.create);
            public IReadOnlyList<byte> DayOfWeek => ctx.ReadList(1).CastByte();
            public string TimeZone => ctx.ReadText(2, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 3);
            }

            public ListOfStructsSerializer<CapnpGen.TimeWindow.WRITER> TimeWindows
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.TimeWindow.WRITER>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> DayOfWeek
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public string TimeZone
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc3be3ccf8d14839eUL)]
    public class TimeWindow : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc3be3ccf8d14839eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            StartHour = reader.StartHour;
            EndHour = reader.EndHour;
            Priority = reader.Priority;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.StartHour = StartHour;
            writer.EndHour = EndHour;
            writer.Priority = Priority;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public byte StartHour
        {
            get;
            set;
        }

        public byte EndHour
        {
            get;
            set;
        }

        public byte Priority
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
            public byte StartHour => ctx.ReadDataByte(0UL, (byte)0);
            public byte EndHour => ctx.ReadDataByte(8UL, (byte)0);
            public byte Priority => ctx.ReadDataByte(16UL, (byte)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public byte StartHour
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public byte EndHour
            {
                get => this.ReadDataByte(8UL, (byte)0);
                set => this.WriteData(8UL, value, (byte)0);
            }

            public byte Priority
            {
                get => this.ReadDataByte(16UL, (byte)0);
                set => this.WriteData(16UL, value, (byte)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xed35c971c1d90d65UL)]
    public class ConnectionQualityRule : ICapnpSerializable
    {
        public const UInt64 typeId = 0xed35c971c1d90d65UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MinPacketLoss = reader.MinPacketLoss;
            MaxJitterMs = reader.MaxJitterMs;
            MinBandwidthKbps = reader.MinBandwidthKbps;
            RequireNATType = reader.RequireNATType;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MinPacketLoss = MinPacketLoss;
            writer.MaxJitterMs = MaxJitterMs;
            writer.MinBandwidthKbps = MinBandwidthKbps;
            writer.RequireNATType = RequireNATType;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float MinPacketLoss
        {
            get;
            set;
        }

        public uint MaxJitterMs
        {
            get;
            set;
        }

        public uint MinBandwidthKbps
        {
            get;
            set;
        }

        public CapnpGen.NATType RequireNATType
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
            public float MinPacketLoss => ctx.ReadDataFloat(0UL, 0F);
            public uint MaxJitterMs => ctx.ReadDataUInt(32UL, 0U);
            public uint MinBandwidthKbps => ctx.ReadDataUInt(64UL, 0U);
            public CapnpGen.NATType RequireNATType => (CapnpGen.NATType)ctx.ReadDataUShort(96UL, (ushort)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public float MinPacketLoss
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public uint MaxJitterMs
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public uint MinBandwidthKbps
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }

            public CapnpGen.NATType RequireNATType
            {
                get => (CapnpGen.NATType)this.ReadDataUShort(96UL, (ushort)0);
                set => this.WriteData(96UL, (ushort)value, (ushort)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb93fdd5870fe3ae2UL)]
    public enum NATType : ushort
    {
        any,
        open,
        moderate,
        strict
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb125b2c25fe59bcaUL)]
    public class RuleEvaluationContext : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb125b2c25fe59bcaUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Players = reader.Players?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PlayerContext>(_));
            GameMode = reader.GameMode;
            Region = reader.Region;
            TimeOfDay = reader.TimeOfDay;
            QueueDurationMs = reader.QueueDurationMs;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Players.Init(Players, (_s1, _v1) => _v1?.serialize(_s1));
            writer.GameMode.Init(GameMode);
            writer.Region = Region;
            writer.TimeOfDay = TimeOfDay;
            writer.QueueDurationMs = QueueDurationMs;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<CapnpGen.PlayerContext> Players
        {
            get;
            set;
        }

        public IReadOnlyList<byte> GameMode
        {
            get;
            set;
        }

        public CapnpGen.Region Region
        {
            get;
            set;
        }

        public ulong TimeOfDay
        {
            get;
            set;
        }

        public ulong QueueDurationMs
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
            public IReadOnlyList<CapnpGen.PlayerContext.READER> Players => ctx.ReadList(0).Cast(CapnpGen.PlayerContext.READER.create);
            public IReadOnlyList<byte> GameMode => ctx.ReadList(1).CastByte();
            public CapnpGen.Region Region => (CapnpGen.Region)ctx.ReadDataUShort(0UL, (ushort)0);
            public ulong TimeOfDay => ctx.ReadDataULong(64UL, 0UL);
            public ulong QueueDurationMs => ctx.ReadDataULong(128UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 2);
            }

            public ListOfStructsSerializer<CapnpGen.PlayerContext.WRITER> Players
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.PlayerContext.WRITER>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> GameMode
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public CapnpGen.Region Region
            {
                get => (CapnpGen.Region)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ulong TimeOfDay
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong QueueDurationMs
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xeae957fa861e0f45UL)]
    public class PlayerContext : ICapnpSerializable
    {
        public const UInt64 typeId = 0xeae957fa861e0f45UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            Mmr = reader.Mmr;
            Attributes = CapnpSerializable.Create<CapnpGen.PlayerAttributes>(reader.Attributes);
            InventoryScore = reader.InventoryScore;
            ConnectionQuality = CapnpSerializable.Create<CapnpGen.ConnectionQuality>(reader.ConnectionQuality);
            RecentMatches = reader.RecentMatches?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RecentMatch>(_));
            PartyId = reader.PartyId;
            IsPartyLeader = reader.IsPartyLeader;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId.Init(PlayerId);
            writer.Mmr = Mmr;
            Attributes?.serialize(writer.Attributes);
            writer.InventoryScore = InventoryScore;
            ConnectionQuality?.serialize(writer.ConnectionQuality);
            writer.RecentMatches.Init(RecentMatches, (_s1, _v1) => _v1?.serialize(_s1));
            writer.PartyId.Init(PartyId);
            writer.IsPartyLeader = IsPartyLeader;
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

        public float Mmr
        {
            get;
            set;
        }

        public CapnpGen.PlayerAttributes Attributes
        {
            get;
            set;
        }

        public uint InventoryScore
        {
            get;
            set;
        }

        public CapnpGen.ConnectionQuality ConnectionQuality
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.RecentMatch> RecentMatches
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PartyId
        {
            get;
            set;
        }

        public bool IsPartyLeader
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
            public float Mmr => ctx.ReadDataFloat(0UL, 0F);
            public CapnpGen.PlayerAttributes.READER Attributes => ctx.ReadStruct(1, CapnpGen.PlayerAttributes.READER.create);
            public uint InventoryScore => ctx.ReadDataUInt(32UL, 0U);
            public CapnpGen.ConnectionQuality.READER ConnectionQuality => ctx.ReadStruct(2, CapnpGen.ConnectionQuality.READER.create);
            public IReadOnlyList<CapnpGen.RecentMatch.READER> RecentMatches => ctx.ReadList(3).Cast(CapnpGen.RecentMatch.READER.create);
            public IReadOnlyList<byte> PartyId => ctx.ReadList(4).CastByte();
            public bool IsPartyLeader => ctx.ReadDataBool(64UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 5);
            }

            public ListOfPrimitivesSerializer<byte> PlayerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public float Mmr
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public CapnpGen.PlayerAttributes.WRITER Attributes
            {
                get => BuildPointer<CapnpGen.PlayerAttributes.WRITER>(1);
                set => Link(1, value);
            }

            public uint InventoryScore
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public CapnpGen.ConnectionQuality.WRITER ConnectionQuality
            {
                get => BuildPointer<CapnpGen.ConnectionQuality.WRITER>(2);
                set => Link(2, value);
            }

            public ListOfStructsSerializer<CapnpGen.RecentMatch.WRITER> RecentMatches
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.RecentMatch.WRITER>>(3);
                set => Link(3, value);
            }

            public ListOfPrimitivesSerializer<byte> PartyId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public bool IsPartyLeader
            {
                get => this.ReadDataBool(64UL, false);
                set => this.WriteData(64UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf19df43315782270UL)]
    public class ConnectionQuality : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf19df43315782270UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PingMs = reader.PingMs;
            PacketLoss = reader.PacketLoss;
            JitterMs = reader.JitterMs;
            NatType = reader.NatType;
            Region = reader.Region;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PingMs = PingMs;
            writer.PacketLoss = PacketLoss;
            writer.JitterMs = JitterMs;
            writer.NatType = NatType;
            writer.Region = Region;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint PingMs
        {
            get;
            set;
        }

        public float PacketLoss
        {
            get;
            set;
        }

        public uint JitterMs
        {
            get;
            set;
        }

        public CapnpGen.NATType NatType
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
            public uint PingMs => ctx.ReadDataUInt(0UL, 0U);
            public float PacketLoss => ctx.ReadDataFloat(32UL, 0F);
            public uint JitterMs => ctx.ReadDataUInt(64UL, 0U);
            public CapnpGen.NATType NatType => (CapnpGen.NATType)ctx.ReadDataUShort(96UL, (ushort)0);
            public CapnpGen.Region Region => (CapnpGen.Region)ctx.ReadDataUShort(112UL, (ushort)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public uint PingMs
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public float PacketLoss
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public uint JitterMs
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }

            public CapnpGen.NATType NatType
            {
                get => (CapnpGen.NATType)this.ReadDataUShort(96UL, (ushort)0);
                set => this.WriteData(96UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.Region Region
            {
                get => (CapnpGen.Region)this.ReadDataUShort(112UL, (ushort)0);
                set => this.WriteData(112UL, (ushort)value, (ushort)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfe6e7daffe31ed00UL)]
    public class RecentMatch : ICapnpSerializable
    {
        public const UInt64 typeId = 0xfe6e7daffe31ed00UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchId = reader.MatchId;
            OpponentIds = reader.OpponentIds;
            Outcome = reader.Outcome;
            Timestamp = reader.Timestamp;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchId.Init(MatchId);
            writer.OpponentIds.Init(OpponentIds, (_s1, _v1) => _s1.Init(_v1));
            writer.Outcome = Outcome;
            writer.Timestamp = Timestamp;
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

        public IReadOnlyList<IReadOnlyList<byte>> OpponentIds
        {
            get;
            set;
        }

        public CapnpGen.OutcomeType Outcome
        {
            get;
            set;
        }

        public ulong Timestamp
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
            public IReadOnlyList<IReadOnlyList<byte>> OpponentIds => ctx.ReadList(1).CastData();
            public CapnpGen.OutcomeType Outcome => (CapnpGen.OutcomeType)ctx.ReadDataUShort(0UL, (ushort)0);
            public ulong Timestamp => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 2);
            }

            public ListOfPrimitivesSerializer<byte> MatchId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> OpponentIds
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(1);
                set => Link(1, value);
            }

            public CapnpGen.OutcomeType Outcome
            {
                get => (CapnpGen.OutcomeType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ulong Timestamp
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x909fbdde423abffaUL)]
    public class RuleSet : ICapnpSerializable
    {
        public const UInt64 typeId = 0x909fbdde423abffaUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RuleSetId = reader.RuleSetId;
            Name = reader.Name;
            Description = reader.Description;
            GameId = reader.GameId;
            ModeId = reader.ModeId;
            Rules = reader.Rules?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.MatchmakingRule>(_));
            EvaluationOrder = reader.EvaluationOrder;
            TimeoutMs = reader.TimeoutMs;
            Backfill = CapnpSerializable.Create<CapnpGen.BackfillPolicy>(reader.Backfill);
            FallbackRules = reader.FallbackRules?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.MatchmakingRule>(_));
            Version = reader.Version;
            IsActive = reader.IsActive;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RuleSetId.Init(RuleSetId);
            writer.Name = Name;
            writer.Description = Description;
            writer.GameId.Init(GameId);
            writer.ModeId.Init(ModeId);
            writer.Rules.Init(Rules, (_s1, _v1) => _v1?.serialize(_s1));
            writer.EvaluationOrder.Init(EvaluationOrder);
            writer.TimeoutMs = TimeoutMs;
            Backfill?.serialize(writer.Backfill);
            writer.FallbackRules.Init(FallbackRules, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Version = Version;
            writer.IsActive = IsActive;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> RuleSetId
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

        public IReadOnlyList<CapnpGen.MatchmakingRule> Rules
        {
            get;
            set;
        }

        public IReadOnlyList<string> EvaluationOrder
        {
            get;
            set;
        }

        public ulong TimeoutMs
        {
            get;
            set;
        }

        public CapnpGen.BackfillPolicy Backfill
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.MatchmakingRule> FallbackRules
        {
            get;
            set;
        }

        public string Version
        {
            get;
            set;
        }

        public bool IsActive
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
            public IReadOnlyList<byte> RuleSetId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public string Description => ctx.ReadText(2, null);
            public IReadOnlyList<byte> GameId => ctx.ReadList(3).CastByte();
            public IReadOnlyList<byte> ModeId => ctx.ReadList(4).CastByte();
            public IReadOnlyList<CapnpGen.MatchmakingRule.READER> Rules => ctx.ReadList(5).Cast(CapnpGen.MatchmakingRule.READER.create);
            public IReadOnlyList<string> EvaluationOrder => ctx.ReadList(6).CastText2();
            public ulong TimeoutMs => ctx.ReadDataULong(0UL, 0UL);
            public CapnpGen.BackfillPolicy.READER Backfill => ctx.ReadStruct(7, CapnpGen.BackfillPolicy.READER.create);
            public IReadOnlyList<CapnpGen.MatchmakingRule.READER> FallbackRules => ctx.ReadList(8).Cast(CapnpGen.MatchmakingRule.READER.create);
            public string Version => ctx.ReadText(9, null);
            public bool IsActive => ctx.ReadDataBool(64UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 10);
            }

            public ListOfPrimitivesSerializer<byte> RuleSetId
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

            public ListOfStructsSerializer<CapnpGen.MatchmakingRule.WRITER> Rules
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.MatchmakingRule.WRITER>>(5);
                set => Link(5, value);
            }

            public ListOfTextSerializer EvaluationOrder
            {
                get => BuildPointer<ListOfTextSerializer>(6);
                set => Link(6, value);
            }

            public ulong TimeoutMs
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public CapnpGen.BackfillPolicy.WRITER Backfill
            {
                get => BuildPointer<CapnpGen.BackfillPolicy.WRITER>(7);
                set => Link(7, value);
            }

            public ListOfStructsSerializer<CapnpGen.MatchmakingRule.WRITER> FallbackRules
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.MatchmakingRule.WRITER>>(8);
                set => Link(8, value);
            }

            public string Version
            {
                get => this.ReadText(9, null);
                set => this.WriteText(9, value, null);
            }

            public bool IsActive
            {
                get => this.ReadDataBool(64UL, false);
                set => this.WriteData(64UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xec5614ec20ae7781UL)]
    public class BackfillPolicy : ICapnpSerializable
    {
        public const UInt64 typeId = 0xec5614ec20ae7781UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Enabled = reader.Enabled;
            MaxTimeMs = reader.MaxTimeMs;
            SkillTolerance = reader.SkillTolerance;
            PartialTeams = reader.PartialTeams;
            RoleFlexibility = reader.RoleFlexibility;
            ConnectionTolerance = reader.ConnectionTolerance;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Enabled = Enabled;
            writer.MaxTimeMs = MaxTimeMs;
            writer.SkillTolerance = SkillTolerance;
            writer.PartialTeams = PartialTeams;
            writer.RoleFlexibility = RoleFlexibility;
            writer.ConnectionTolerance = ConnectionTolerance;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool Enabled
        {
            get;
            set;
        }

        public ulong MaxTimeMs
        {
            get;
            set;
        }

        public float SkillTolerance
        {
            get;
            set;
        }

        public bool PartialTeams
        {
            get;
            set;
        }

        public bool RoleFlexibility
        {
            get;
            set;
        }

        public bool ConnectionTolerance
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
            public bool Enabled => ctx.ReadDataBool(0UL, false);
            public ulong MaxTimeMs => ctx.ReadDataULong(64UL, 0UL);
            public float SkillTolerance => ctx.ReadDataFloat(32UL, 0F);
            public bool PartialTeams => ctx.ReadDataBool(1UL, false);
            public bool RoleFlexibility => ctx.ReadDataBool(2UL, false);
            public bool ConnectionTolerance => ctx.ReadDataBool(3UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public bool Enabled
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public ulong MaxTimeMs
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public float SkillTolerance
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public bool PartialTeams
            {
                get => this.ReadDataBool(1UL, false);
                set => this.WriteData(1UL, value, false);
            }

            public bool RoleFlexibility
            {
                get => this.ReadDataBool(2UL, false);
                set => this.WriteData(2UL, value, false);
            }

            public bool ConnectionTolerance
            {
                get => this.ReadDataBool(3UL, false);
                set => this.WriteData(3UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd9121cb72e60ad3eUL)]
    public class MatchQuality : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd9121cb72e60ad3eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TotalScore = reader.TotalScore;
            SkillBalance = reader.SkillBalance;
            LatencyScore = reader.LatencyScore;
            RoleBalance = reader.RoleBalance;
            ConnectionScore = reader.ConnectionScore;
            PreferenceScore = reader.PreferenceScore;
            RuleScores = reader.RuleScores?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RuleScore>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TotalScore = TotalScore;
            writer.SkillBalance = SkillBalance;
            writer.LatencyScore = LatencyScore;
            writer.RoleBalance = RoleBalance;
            writer.ConnectionScore = ConnectionScore;
            writer.PreferenceScore = PreferenceScore;
            writer.RuleScores.Init(RuleScores, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float TotalScore
        {
            get;
            set;
        }

        public float SkillBalance
        {
            get;
            set;
        }

        public float LatencyScore
        {
            get;
            set;
        }

        public float RoleBalance
        {
            get;
            set;
        }

        public float ConnectionScore
        {
            get;
            set;
        }

        public float PreferenceScore
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.RuleScore> RuleScores
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
            public float TotalScore => ctx.ReadDataFloat(0UL, 0F);
            public float SkillBalance => ctx.ReadDataFloat(32UL, 0F);
            public float LatencyScore => ctx.ReadDataFloat(64UL, 0F);
            public float RoleBalance => ctx.ReadDataFloat(96UL, 0F);
            public float ConnectionScore => ctx.ReadDataFloat(128UL, 0F);
            public float PreferenceScore => ctx.ReadDataFloat(160UL, 0F);
            public IReadOnlyList<CapnpGen.RuleScore.READER> RuleScores => ctx.ReadList(0).Cast(CapnpGen.RuleScore.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 1);
            }

            public float TotalScore
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float SkillBalance
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float LatencyScore
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public float RoleBalance
            {
                get => this.ReadDataFloat(96UL, 0F);
                set => this.WriteData(96UL, value, 0F);
            }

            public float ConnectionScore
            {
                get => this.ReadDataFloat(128UL, 0F);
                set => this.WriteData(128UL, value, 0F);
            }

            public float PreferenceScore
            {
                get => this.ReadDataFloat(160UL, 0F);
                set => this.WriteData(160UL, value, 0F);
            }

            public ListOfStructsSerializer<CapnpGen.RuleScore.WRITER> RuleScores
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.RuleScore.WRITER>>(0);
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe0a2bba07a09a90cUL)]
    public class RuleScore : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe0a2bba07a09a90cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RuleId = reader.RuleId;
            Score = reader.Score;
            Passed = reader.Passed;
            Weight = reader.Weight;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RuleId = RuleId;
            writer.Score = Score;
            writer.Passed = Passed;
            writer.Weight = Weight;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string RuleId
        {
            get;
            set;
        }

        public float Score
        {
            get;
            set;
        }

        public bool Passed
        {
            get;
            set;
        }

        public float Weight
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
            public string RuleId => ctx.ReadText(0, null);
            public float Score => ctx.ReadDataFloat(0UL, 0F);
            public bool Passed => ctx.ReadDataBool(32UL, false);
            public float Weight => ctx.ReadDataFloat(64UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 1);
            }

            public string RuleId
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public float Score
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public bool Passed
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }

            public float Weight
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd472ceb592c48b2bUL), Proxy(typeof(MatchmakingRuleService_Proxy)), Skeleton(typeof(MatchmakingRuleService_Skeleton))]
    public interface IMatchmakingRuleService : IDisposable
    {
        Task<IReadOnlyList<byte>> CreateRuleSet(CapnpGen.RuleSet ruleSet, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task UpdateRuleSet(IReadOnlyList<byte> ruleSetId, CapnpGen.RuleSet ruleSet, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task DeleteRuleSet(IReadOnlyList<byte> ruleSetId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.RuleSet> GetRuleSet(IReadOnlyList<byte> ruleSetId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.RuleSet>> ListRuleSets(IReadOnlyList<byte> gameId, IReadOnlyList<byte> modeId, CancellationToken cancellationToken_ = default);
        Task<(CapnpGen.MatchQuality, bool)> EvaluateMatch(CapnpGen.RuleEvaluationContext context, IReadOnlyList<byte> ruleSetId, CancellationToken cancellationToken_ = default);
        Task<(IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>>, CapnpGen.MatchQuality)> FindBestMatch(IReadOnlyList<CapnpGen.PlayerContext> players, IReadOnlyList<byte> ruleSetId, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.RuleStats> GetRuleStatistics(IReadOnlyList<byte> ruleSetId, CapnpGen.TimeRange timeRange, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd472ceb592c48b2bUL)]
    public class MatchmakingRuleService_Proxy : Proxy, IMatchmakingRuleService
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd472ceb592c48b2bUL)]
    public class MatchmakingRuleService_Skeleton : Skeleton<IMatchmakingRuleService>
    {
        public MatchmakingRuleService_Skeleton()
        {
            SetMethodTable(CreateRuleSet, UpdateRuleSet, DeleteRuleSet, GetRuleSet, ListRuleSets, EvaluateMatch, FindBestMatch, GetRuleStatistics);
        }

        public override ulong InterfaceId => 15308525362632493867UL;
        Task<AnswerOrCounterquestion> CreateRuleSet(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Params_CreateRuleSet>(d_);
                return Impatient.MaybeTailCall(Impl.CreateRuleSet(in_.RuleSet, in_.Signature, cancellationToken_), ruleSetId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Result_CreateRuleSet.WRITER>();
                    var r_ = new CapnpGen.MatchmakingRuleService.Result_CreateRuleSet{RuleSetId = ruleSetId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> UpdateRuleSet(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Params_UpdateRuleSet>(d_);
                await Impl.UpdateRuleSet(in_.RuleSetId, in_.RuleSet, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Result_UpdateRuleSet.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> DeleteRuleSet(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Params_DeleteRuleSet>(d_);
                await Impl.DeleteRuleSet(in_.RuleSetId, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Result_DeleteRuleSet.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GetRuleSet(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Params_GetRuleSet>(d_);
                return Impatient.MaybeTailCall(Impl.GetRuleSet(in_.RuleSetId, cancellationToken_), ruleSet =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Result_GetRuleSet.WRITER>();
                    var r_ = new CapnpGen.MatchmakingRuleService.Result_GetRuleSet{RuleSet = ruleSet};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> ListRuleSets(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Params_ListRuleSets>(d_);
                return Impatient.MaybeTailCall(Impl.ListRuleSets(in_.GameId, in_.ModeId, cancellationToken_), ruleSets =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Result_ListRuleSets.WRITER>();
                    var r_ = new CapnpGen.MatchmakingRuleService.Result_ListRuleSets{RuleSets = ruleSets};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> EvaluateMatch(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Params_EvaluateMatch>(d_);
                return Impatient.MaybeTailCall(Impl.EvaluateMatch(in_.Context, in_.RuleSetId, cancellationToken_), (quality, passes) =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Result_EvaluateMatch.WRITER>();
                    var r_ = new CapnpGen.MatchmakingRuleService.Result_EvaluateMatch{Quality = quality, Passes = passes};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> FindBestMatch(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Params_FindBestMatch>(d_);
                return Impatient.MaybeTailCall(Impl.FindBestMatch(in_.Players, in_.RuleSetId, cancellationToken_), (teams, quality) =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Result_FindBestMatch.WRITER>();
                    var r_ = new CapnpGen.MatchmakingRuleService.Result_FindBestMatch{Teams = teams, Quality = quality};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetRuleStatistics(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.MatchmakingRuleService.Params_GetRuleStatistics>(d_);
                return Impatient.MaybeTailCall(Impl.GetRuleStatistics(in_.RuleSetId, in_.TimeRange, cancellationToken_), stats =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.MatchmakingRuleService.Result_GetRuleStatistics.WRITER>();
                    var r_ = new CapnpGen.MatchmakingRuleService.Result_GetRuleStatistics{Stats = stats};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class MatchmakingRuleService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xed84cffa0694a4aaUL)]
        public class Params_CreateRuleSet : ICapnpSerializable
        {
            public const UInt64 typeId = 0xed84cffa0694a4aaUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RuleSet = CapnpSerializable.Create<CapnpGen.RuleSet>(reader.RuleSet);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                RuleSet?.serialize(writer.RuleSet);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.RuleSet RuleSet
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
                public CapnpGen.RuleSet.READER RuleSet => ctx.ReadStruct(0, CapnpGen.RuleSet.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.RuleSet.WRITER RuleSet
                {
                    get => BuildPointer<CapnpGen.RuleSet.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd673cf78accc49bbUL)]
        public class Result_CreateRuleSet : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd673cf78accc49bbUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RuleSetId = reader.RuleSetId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RuleSetId.Init(RuleSetId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RuleSetId
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
                public IReadOnlyList<byte> RuleSetId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> RuleSetId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x94882b1d522cc85eUL)]
        public class Params_UpdateRuleSet : ICapnpSerializable
        {
            public const UInt64 typeId = 0x94882b1d522cc85eUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RuleSetId = reader.RuleSetId;
                RuleSet = CapnpSerializable.Create<CapnpGen.RuleSet>(reader.RuleSet);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RuleSetId.Init(RuleSetId);
                RuleSet?.serialize(writer.RuleSet);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RuleSetId
            {
                get;
                set;
            }

            public CapnpGen.RuleSet RuleSet
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
                public IReadOnlyList<byte> RuleSetId => ctx.ReadList(0).CastByte();
                public CapnpGen.RuleSet.READER RuleSet => ctx.ReadStruct(1, CapnpGen.RuleSet.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> RuleSetId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.RuleSet.WRITER RuleSet
                {
                    get => BuildPointer<CapnpGen.RuleSet.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8becf8f68da6fec9UL)]
        public class Result_UpdateRuleSet : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8becf8f68da6fec9UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9d6eca6b0e677014UL)]
        public class Params_DeleteRuleSet : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9d6eca6b0e677014UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RuleSetId = reader.RuleSetId;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RuleSetId.Init(RuleSetId);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RuleSetId
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
                public IReadOnlyList<byte> RuleSetId => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> RuleSetId
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa6c0d0c8dacfcbd6UL)]
        public class Result_DeleteRuleSet : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa6c0d0c8dacfcbd6UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfbb3101f3f260527UL)]
        public class Params_GetRuleSet : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfbb3101f3f260527UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RuleSetId = reader.RuleSetId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RuleSetId.Init(RuleSetId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RuleSetId
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
                public IReadOnlyList<byte> RuleSetId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> RuleSetId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8302dc7dc0e89b10UL)]
        public class Result_GetRuleSet : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8302dc7dc0e89b10UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RuleSet = CapnpSerializable.Create<CapnpGen.RuleSet>(reader.RuleSet);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                RuleSet?.serialize(writer.RuleSet);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.RuleSet RuleSet
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
                public CapnpGen.RuleSet.READER RuleSet => ctx.ReadStruct(0, CapnpGen.RuleSet.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.RuleSet.WRITER RuleSet
                {
                    get => BuildPointer<CapnpGen.RuleSet.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x94cc3f0d2aa3ee5aUL)]
        public class Params_ListRuleSets : ICapnpSerializable
        {
            public const UInt64 typeId = 0x94cc3f0d2aa3ee5aUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                GameId = reader.GameId;
                ModeId = reader.ModeId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.GameId.Init(GameId);
                writer.ModeId.Init(ModeId);
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

            public IReadOnlyList<byte> ModeId
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
                public IReadOnlyList<byte> ModeId => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> GameId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> ModeId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfdfbe3e00852ee5dUL)]
        public class Result_ListRuleSets : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfdfbe3e00852ee5dUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RuleSets = reader.RuleSets?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RuleSet>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RuleSets.Init(RuleSets, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.RuleSet> RuleSets
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
                public IReadOnlyList<CapnpGen.RuleSet.READER> RuleSets => ctx.ReadList(0).Cast(CapnpGen.RuleSet.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.RuleSet.WRITER> RuleSets
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.RuleSet.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf0cd0528416b1cc3UL)]
        public class Params_EvaluateMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf0cd0528416b1cc3UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Context = CapnpSerializable.Create<CapnpGen.RuleEvaluationContext>(reader.Context);
                RuleSetId = reader.RuleSetId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Context?.serialize(writer.Context);
                writer.RuleSetId.Init(RuleSetId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.RuleEvaluationContext Context
            {
                get;
                set;
            }

            public IReadOnlyList<byte> RuleSetId
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
                public CapnpGen.RuleEvaluationContext.READER Context => ctx.ReadStruct(0, CapnpGen.RuleEvaluationContext.READER.create);
                public IReadOnlyList<byte> RuleSetId => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.RuleEvaluationContext.WRITER Context
                {
                    get => BuildPointer<CapnpGen.RuleEvaluationContext.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> RuleSetId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc44abe108ea0321cUL)]
        public class Result_EvaluateMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc44abe108ea0321cUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Quality = CapnpSerializable.Create<CapnpGen.MatchQuality>(reader.Quality);
                Passes = reader.Passes;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Quality?.serialize(writer.Quality);
                writer.Passes = Passes;
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.MatchQuality Quality
            {
                get;
                set;
            }

            public bool Passes
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
                public CapnpGen.MatchQuality.READER Quality => ctx.ReadStruct(0, CapnpGen.MatchQuality.READER.create);
                public bool Passes => ctx.ReadDataBool(0UL, false);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 1);
                }

                public CapnpGen.MatchQuality.WRITER Quality
                {
                    get => BuildPointer<CapnpGen.MatchQuality.WRITER>(0);
                    set => Link(0, value);
                }

                public bool Passes
                {
                    get => this.ReadDataBool(0UL, false);
                    set => this.WriteData(0UL, value, false);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa67ac0bb2be9be16UL)]
        public class Params_FindBestMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa67ac0bb2be9be16UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Players = reader.Players?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PlayerContext>(_));
                RuleSetId = reader.RuleSetId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Players.Init(Players, (_s1, _v1) => _v1?.serialize(_s1));
                writer.RuleSetId.Init(RuleSetId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.PlayerContext> Players
            {
                get;
                set;
            }

            public IReadOnlyList<byte> RuleSetId
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
                public IReadOnlyList<CapnpGen.PlayerContext.READER> Players => ctx.ReadList(0).Cast(CapnpGen.PlayerContext.READER.create);
                public IReadOnlyList<byte> RuleSetId => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfStructsSerializer<CapnpGen.PlayerContext.WRITER> Players
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.PlayerContext.WRITER>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> RuleSetId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf4b9c18bf435199dUL)]
        public class Result_FindBestMatch : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf4b9c18bf435199dUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Teams = reader.Teams;
                Quality = CapnpSerializable.Create<CapnpGen.MatchQuality>(reader.Quality);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Teams.Init(Teams, (_s2, _v2) => _s2.Init(_v2, (_s1, _v1) => _s1.Init(_v1)));
                Quality?.serialize(writer.Quality);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>> Teams
            {
                get;
                set;
            }

            public CapnpGen.MatchQuality Quality
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
                public IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>> Teams => ctx.ReadList(0).Cast(_0 => _0.RequireList().CastData());
                public CapnpGen.MatchQuality.READER Quality => ctx.ReadStruct(1, CapnpGen.MatchQuality.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPointersSerializer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>> Teams
                {
                    get => BuildPointer<ListOfPointersSerializer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.MatchQuality.WRITER Quality
                {
                    get => BuildPointer<CapnpGen.MatchQuality.WRITER>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb4cbaa3ff03affe5UL)]
        public class Params_GetRuleStatistics : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb4cbaa3ff03affe5UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RuleSetId = reader.RuleSetId;
                TimeRange = CapnpSerializable.Create<CapnpGen.TimeRange>(reader.TimeRange);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RuleSetId.Init(RuleSetId);
                TimeRange?.serialize(writer.TimeRange);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> RuleSetId
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
                public IReadOnlyList<byte> RuleSetId => ctx.ReadList(0).CastByte();
                public CapnpGen.TimeRange.READER TimeRange => ctx.ReadStruct(1, CapnpGen.TimeRange.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> RuleSetId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.TimeRange.WRITER TimeRange
                {
                    get => BuildPointer<CapnpGen.TimeRange.WRITER>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x89222c3549f6c8e3UL)]
        public class Result_GetRuleStatistics : ICapnpSerializable
        {
            public const UInt64 typeId = 0x89222c3549f6c8e3UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Stats = CapnpSerializable.Create<CapnpGen.RuleStats>(reader.Stats);
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

            public CapnpGen.RuleStats Stats
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
                public CapnpGen.RuleStats.READER Stats => ctx.ReadStruct(0, CapnpGen.RuleStats.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.RuleStats.WRITER Stats
                {
                    get => BuildPointer<CapnpGen.RuleStats.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8d78f592906fc45fUL)]
    public class RuleStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0x8d78f592906fc45fUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RuleSetId = reader.RuleSetId;
            TotalEvaluations = reader.TotalEvaluations;
            SuccessfulMatches = reader.SuccessfulMatches;
            FailedMatches = reader.FailedMatches;
            AvgMatchQuality = reader.AvgMatchQuality;
            AvgQueueTimeMs = reader.AvgQueueTimeMs;
            CommonFailures = reader.CommonFailures?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RuleFailure>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RuleSetId.Init(RuleSetId);
            writer.TotalEvaluations = TotalEvaluations;
            writer.SuccessfulMatches = SuccessfulMatches;
            writer.FailedMatches = FailedMatches;
            writer.AvgMatchQuality = AvgMatchQuality;
            writer.AvgQueueTimeMs = AvgQueueTimeMs;
            writer.CommonFailures.Init(CommonFailures, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> RuleSetId
        {
            get;
            set;
        }

        public ulong TotalEvaluations
        {
            get;
            set;
        }

        public ulong SuccessfulMatches
        {
            get;
            set;
        }

        public ulong FailedMatches
        {
            get;
            set;
        }

        public float AvgMatchQuality
        {
            get;
            set;
        }

        public ulong AvgQueueTimeMs
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.RuleFailure> CommonFailures
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
            public IReadOnlyList<byte> RuleSetId => ctx.ReadList(0).CastByte();
            public ulong TotalEvaluations => ctx.ReadDataULong(0UL, 0UL);
            public ulong SuccessfulMatches => ctx.ReadDataULong(64UL, 0UL);
            public ulong FailedMatches => ctx.ReadDataULong(128UL, 0UL);
            public float AvgMatchQuality => ctx.ReadDataFloat(192UL, 0F);
            public ulong AvgQueueTimeMs => ctx.ReadDataULong(256UL, 0UL);
            public IReadOnlyList<CapnpGen.RuleFailure.READER> CommonFailures => ctx.ReadList(1).Cast(CapnpGen.RuleFailure.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(5, 2);
            }

            public ListOfPrimitivesSerializer<byte> RuleSetId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ulong TotalEvaluations
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ulong SuccessfulMatches
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong FailedMatches
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public float AvgMatchQuality
            {
                get => this.ReadDataFloat(192UL, 0F);
                set => this.WriteData(192UL, value, 0F);
            }

            public ulong AvgQueueTimeMs
            {
                get => this.ReadDataULong(256UL, 0UL);
                set => this.WriteData(256UL, value, 0UL);
            }

            public ListOfStructsSerializer<CapnpGen.RuleFailure.WRITER> CommonFailures
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.RuleFailure.WRITER>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xba1e63e2da0f5112UL)]
    public class RuleFailure : ICapnpSerializable
    {
        public const UInt64 typeId = 0xba1e63e2da0f5112UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RuleId = reader.RuleId;
            FailureCount = reader.FailureCount;
            FailureRate = reader.FailureRate;
            CommonReason = reader.CommonReason;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RuleId = RuleId;
            writer.FailureCount = FailureCount;
            writer.FailureRate = FailureRate;
            writer.CommonReason = CommonReason;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string RuleId
        {
            get;
            set;
        }

        public ulong FailureCount
        {
            get;
            set;
        }

        public float FailureRate
        {
            get;
            set;
        }

        public string CommonReason
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
            public string RuleId => ctx.ReadText(0, null);
            public ulong FailureCount => ctx.ReadDataULong(0UL, 0UL);
            public float FailureRate => ctx.ReadDataFloat(64UL, 0F);
            public string CommonReason => ctx.ReadText(1, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 2);
            }

            public string RuleId
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ulong FailureCount
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public float FailureRate
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public string CommonReason
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }
        }
    }
}