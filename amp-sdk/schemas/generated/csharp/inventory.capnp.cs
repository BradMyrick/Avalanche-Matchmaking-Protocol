using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x81628603f8f4ce5cUL)]
    public enum Rarity : ushort
    {
        common,
        uncommon,
        rare,
        epic,
        legendary,
        mythic,
        exclusive
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9ea67da591489f95UL)]
    public enum ItemType : ushort
    {
        weapon,
        armor,
        consumable,
        cosmetic,
        ability,
        perk,
        currency,
        material,
        blueprint,
        key,
        container
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd46f9c074869105eUL)]
    public class ItemMetadata : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd46f9c074869105eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Name = reader.Name;
            Description = reader.Description;
            IconUrl = reader.IconUrl;
            ModelUrl = reader.ModelUrl;
            AnimationUrl = reader.AnimationUrl;
            SoundUrl = reader.SoundUrl;
            Rarity = reader.Rarity;
            ItemType = reader.ItemType;
            Slot = reader.Slot;
            LevelRequirement = reader.LevelRequirement;
            ClassRequirement = reader.ClassRequirement;
            FactionRequirement = reader.FactionRequirement;
            BaseStats = reader.BaseStats;
            ScalingStats = reader.ScalingStats;
            EnchantmentSlots = reader.EnchantmentSlots;
            MaxDurability = reader.MaxDurability;
            MaxStack = reader.MaxStack;
            IsConsumable = reader.IsConsumable;
            CooldownMs = reader.CooldownMs;
            IsTradable = reader.IsTradable;
            IsSoulbound = reader.IsSoulbound;
            IsDestroyable = reader.IsDestroyable;
            BaseValue = reader.BaseValue;
            TokenStandard = reader.TokenStandard;
            TokenId = reader.TokenId;
            ContractAddress = reader.ContractAddress;
            ChainId = reader.ChainId;
            Tags = reader.Tags;
            CreationDate = reader.CreationDate;
            LastUpdated = reader.LastUpdated;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Name = Name;
            writer.Description = Description;
            writer.IconUrl = IconUrl;
            writer.ModelUrl = ModelUrl;
            writer.AnimationUrl = AnimationUrl;
            writer.SoundUrl = SoundUrl;
            writer.Rarity = Rarity;
            writer.ItemType = ItemType;
            writer.Slot = Slot;
            writer.LevelRequirement = LevelRequirement;
            writer.ClassRequirement = ClassRequirement;
            writer.FactionRequirement = FactionRequirement;
            writer.BaseStats.Init(BaseStats);
            writer.ScalingStats.Init(ScalingStats);
            writer.EnchantmentSlots = EnchantmentSlots;
            writer.MaxDurability = MaxDurability;
            writer.MaxStack = MaxStack;
            writer.IsConsumable = IsConsumable;
            writer.CooldownMs = CooldownMs;
            writer.IsTradable = IsTradable;
            writer.IsSoulbound = IsSoulbound;
            writer.IsDestroyable = IsDestroyable;
            writer.BaseValue = BaseValue;
            writer.TokenStandard = TokenStandard;
            writer.TokenId.Init(TokenId);
            writer.ContractAddress.Init(ContractAddress);
            writer.ChainId = ChainId;
            writer.Tags.Init(Tags);
            writer.CreationDate = CreationDate;
            writer.LastUpdated = LastUpdated;
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

        public string IconUrl
        {
            get;
            set;
        }

        public string ModelUrl
        {
            get;
            set;
        }

        public string AnimationUrl
        {
            get;
            set;
        }

        public string SoundUrl
        {
            get;
            set;
        }

        public CapnpGen.Rarity Rarity
        {
            get;
            set;
        }

        public CapnpGen.ItemType ItemType
        {
            get;
            set;
        }

        public string Slot
        {
            get;
            set;
        }

        public uint LevelRequirement
        {
            get;
            set;
        }

        public string ClassRequirement
        {
            get;
            set;
        }

        public string FactionRequirement
        {
            get;
            set;
        }

        public IReadOnlyList<byte> BaseStats
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ScalingStats
        {
            get;
            set;
        }

        public byte EnchantmentSlots
        {
            get;
            set;
        }

        public uint MaxDurability
        {
            get;
            set;
        }

        public uint MaxStack
        {
            get;
            set;
        }

        public bool IsConsumable
        {
            get;
            set;
        }

        public ulong CooldownMs
        {
            get;
            set;
        }

        public bool IsTradable
        {
            get;
            set;
        }

        public bool IsSoulbound
        {
            get;
            set;
        }

        public bool IsDestroyable
        {
            get;
            set;
        }

        public ulong BaseValue
        {
            get;
            set;
        }

        public CapnpGen.TokenStandard TokenStandard
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TokenId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ContractAddress
        {
            get;
            set;
        }

        public ulong ChainId
        {
            get;
            set;
        }

        public IReadOnlyList<string> Tags
        {
            get;
            set;
        }

        public ulong CreationDate
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
            public string Name => ctx.ReadText(0, null);
            public string Description => ctx.ReadText(1, null);
            public string IconUrl => ctx.ReadText(2, null);
            public string ModelUrl => ctx.ReadText(3, null);
            public string AnimationUrl => ctx.ReadText(4, null);
            public string SoundUrl => ctx.ReadText(5, null);
            public CapnpGen.Rarity Rarity => (CapnpGen.Rarity)ctx.ReadDataUShort(0UL, (ushort)0);
            public CapnpGen.ItemType ItemType => (CapnpGen.ItemType)ctx.ReadDataUShort(16UL, (ushort)0);
            public string Slot => ctx.ReadText(6, null);
            public uint LevelRequirement => ctx.ReadDataUInt(32UL, 0U);
            public string ClassRequirement => ctx.ReadText(7, null);
            public string FactionRequirement => ctx.ReadText(8, null);
            public IReadOnlyList<byte> BaseStats => ctx.ReadList(9).CastByte();
            public IReadOnlyList<byte> ScalingStats => ctx.ReadList(10).CastByte();
            public byte EnchantmentSlots => ctx.ReadDataByte(64UL, (byte)0);
            public uint MaxDurability => ctx.ReadDataUInt(96UL, 0U);
            public uint MaxStack => ctx.ReadDataUInt(128UL, 0U);
            public bool IsConsumable => ctx.ReadDataBool(72UL, false);
            public ulong CooldownMs => ctx.ReadDataULong(192UL, 0UL);
            public bool IsTradable => ctx.ReadDataBool(73UL, false);
            public bool IsSoulbound => ctx.ReadDataBool(74UL, false);
            public bool IsDestroyable => ctx.ReadDataBool(75UL, false);
            public ulong BaseValue => ctx.ReadDataULong(256UL, 0UL);
            public CapnpGen.TokenStandard TokenStandard => (CapnpGen.TokenStandard)ctx.ReadDataUShort(80UL, (ushort)0);
            public IReadOnlyList<byte> TokenId => ctx.ReadList(11).CastByte();
            public IReadOnlyList<byte> ContractAddress => ctx.ReadList(12).CastByte();
            public ulong ChainId => ctx.ReadDataULong(320UL, 0UL);
            public IReadOnlyList<string> Tags => ctx.ReadList(13).CastText2();
            public ulong CreationDate => ctx.ReadDataULong(384UL, 0UL);
            public ulong LastUpdated => ctx.ReadDataULong(448UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(8, 14);
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

            public string IconUrl
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }

            public string ModelUrl
            {
                get => this.ReadText(3, null);
                set => this.WriteText(3, value, null);
            }

            public string AnimationUrl
            {
                get => this.ReadText(4, null);
                set => this.WriteText(4, value, null);
            }

            public string SoundUrl
            {
                get => this.ReadText(5, null);
                set => this.WriteText(5, value, null);
            }

            public CapnpGen.Rarity Rarity
            {
                get => (CapnpGen.Rarity)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.ItemType ItemType
            {
                get => (CapnpGen.ItemType)this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, (ushort)value, (ushort)0);
            }

            public string Slot
            {
                get => this.ReadText(6, null);
                set => this.WriteText(6, value, null);
            }

            public uint LevelRequirement
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public string ClassRequirement
            {
                get => this.ReadText(7, null);
                set => this.WriteText(7, value, null);
            }

            public string FactionRequirement
            {
                get => this.ReadText(8, null);
                set => this.WriteText(8, value, null);
            }

            public ListOfPrimitivesSerializer<byte> BaseStats
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(9);
                set => Link(9, value);
            }

            public ListOfPrimitivesSerializer<byte> ScalingStats
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(10);
                set => Link(10, value);
            }

            public byte EnchantmentSlots
            {
                get => this.ReadDataByte(64UL, (byte)0);
                set => this.WriteData(64UL, value, (byte)0);
            }

            public uint MaxDurability
            {
                get => this.ReadDataUInt(96UL, 0U);
                set => this.WriteData(96UL, value, 0U);
            }

            public uint MaxStack
            {
                get => this.ReadDataUInt(128UL, 0U);
                set => this.WriteData(128UL, value, 0U);
            }

            public bool IsConsumable
            {
                get => this.ReadDataBool(72UL, false);
                set => this.WriteData(72UL, value, false);
            }

            public ulong CooldownMs
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public bool IsTradable
            {
                get => this.ReadDataBool(73UL, false);
                set => this.WriteData(73UL, value, false);
            }

            public bool IsSoulbound
            {
                get => this.ReadDataBool(74UL, false);
                set => this.WriteData(74UL, value, false);
            }

            public bool IsDestroyable
            {
                get => this.ReadDataBool(75UL, false);
                set => this.WriteData(75UL, value, false);
            }

            public ulong BaseValue
            {
                get => this.ReadDataULong(256UL, 0UL);
                set => this.WriteData(256UL, value, 0UL);
            }

            public CapnpGen.TokenStandard TokenStandard
            {
                get => (CapnpGen.TokenStandard)this.ReadDataUShort(80UL, (ushort)0);
                set => this.WriteData(80UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> TokenId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(11);
                set => Link(11, value);
            }

            public ListOfPrimitivesSerializer<byte> ContractAddress
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(12);
                set => Link(12, value);
            }

            public ulong ChainId
            {
                get => this.ReadDataULong(320UL, 0UL);
                set => this.WriteData(320UL, value, 0UL);
            }

            public ListOfTextSerializer Tags
            {
                get => BuildPointer<ListOfTextSerializer>(13);
                set => Link(13, value);
            }

            public ulong CreationDate
            {
                get => this.ReadDataULong(384UL, 0UL);
                set => this.WriteData(384UL, value, 0UL);
            }

            public ulong LastUpdated
            {
                get => this.ReadDataULong(448UL, 0UL);
                set => this.WriteData(448UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x91c2063df0c5e5b0UL)]
    public enum TokenStandard : ushort
    {
        none,
        erc20,
        erc721,
        erc1155,
        avalancheNative
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xda2aa601159621d6UL)]
    public class InventoryItem : ICapnpSerializable
    {
        public const UInt64 typeId = 0xda2aa601159621d6UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ItemId = reader.ItemId;
            TemplateId = reader.TemplateId;
            OwnerId = reader.OwnerId;
            Quantity = reader.Quantity;
            Durability = reader.Durability;
            Level = reader.Level;
            Experience = reader.Experience;
            Enchantments = reader.Enchantments?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Enchantment>(_));
            Sockets = reader.Sockets?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Socket>(_));
            CustomName = reader.CustomName;
            IsEquipped = reader.IsEquipped;
            EquippedSlot = reader.EquippedSlot;
            IsBound = reader.IsBound;
            IsLocked = reader.IsLocked;
            AcquiredAt = reader.AcquiredAt;
            LastUsed = reader.LastUsed;
            TransactionHash = reader.TransactionHash;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ItemId.Init(ItemId);
            writer.TemplateId.Init(TemplateId);
            writer.OwnerId.Init(OwnerId);
            writer.Quantity = Quantity;
            writer.Durability = Durability;
            writer.Level = Level;
            writer.Experience = Experience;
            writer.Enchantments.Init(Enchantments, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Sockets.Init(Sockets, (_s1, _v1) => _v1?.serialize(_s1));
            writer.CustomName = CustomName;
            writer.IsEquipped = IsEquipped;
            writer.EquippedSlot = EquippedSlot;
            writer.IsBound = IsBound;
            writer.IsLocked = IsLocked;
            writer.AcquiredAt = AcquiredAt;
            writer.LastUsed = LastUsed;
            writer.TransactionHash.Init(TransactionHash);
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

        public IReadOnlyList<byte> TemplateId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> OwnerId
        {
            get;
            set;
        }

        public uint Quantity
        {
            get;
            set;
        }

        public uint Durability
        {
            get;
            set;
        }

        public uint Level
        {
            get;
            set;
        }

        public ulong Experience
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Enchantment> Enchantments
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Socket> Sockets
        {
            get;
            set;
        }

        public string CustomName
        {
            get;
            set;
        }

        public bool IsEquipped
        {
            get;
            set;
        }

        public string EquippedSlot
        {
            get;
            set;
        }

        public bool IsBound
        {
            get;
            set;
        }

        public bool IsLocked
        {
            get;
            set;
        }

        public ulong AcquiredAt
        {
            get;
            set;
        }

        public ulong LastUsed
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TransactionHash
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
            public IReadOnlyList<byte> TemplateId => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> OwnerId => ctx.ReadList(2).CastByte();
            public uint Quantity => ctx.ReadDataUInt(0UL, 0U);
            public uint Durability => ctx.ReadDataUInt(32UL, 0U);
            public uint Level => ctx.ReadDataUInt(64UL, 0U);
            public ulong Experience => ctx.ReadDataULong(128UL, 0UL);
            public IReadOnlyList<CapnpGen.Enchantment.READER> Enchantments => ctx.ReadList(3).Cast(CapnpGen.Enchantment.READER.create);
            public IReadOnlyList<CapnpGen.Socket.READER> Sockets => ctx.ReadList(4).Cast(CapnpGen.Socket.READER.create);
            public string CustomName => ctx.ReadText(5, null);
            public bool IsEquipped => ctx.ReadDataBool(96UL, false);
            public string EquippedSlot => ctx.ReadText(6, null);
            public bool IsBound => ctx.ReadDataBool(97UL, false);
            public bool IsLocked => ctx.ReadDataBool(98UL, false);
            public ulong AcquiredAt => ctx.ReadDataULong(192UL, 0UL);
            public ulong LastUsed => ctx.ReadDataULong(256UL, 0UL);
            public IReadOnlyList<byte> TransactionHash => ctx.ReadList(7).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(5, 8);
            }

            public ListOfPrimitivesSerializer<byte> ItemId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> TemplateId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> OwnerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public uint Quantity
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public uint Durability
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public uint Level
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }

            public ulong Experience
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ListOfStructsSerializer<CapnpGen.Enchantment.WRITER> Enchantments
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.Enchantment.WRITER>>(3);
                set => Link(3, value);
            }

            public ListOfStructsSerializer<CapnpGen.Socket.WRITER> Sockets
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.Socket.WRITER>>(4);
                set => Link(4, value);
            }

            public string CustomName
            {
                get => this.ReadText(5, null);
                set => this.WriteText(5, value, null);
            }

            public bool IsEquipped
            {
                get => this.ReadDataBool(96UL, false);
                set => this.WriteData(96UL, value, false);
            }

            public string EquippedSlot
            {
                get => this.ReadText(6, null);
                set => this.WriteText(6, value, null);
            }

            public bool IsBound
            {
                get => this.ReadDataBool(97UL, false);
                set => this.WriteData(97UL, value, false);
            }

            public bool IsLocked
            {
                get => this.ReadDataBool(98UL, false);
                set => this.WriteData(98UL, value, false);
            }

            public ulong AcquiredAt
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public ulong LastUsed
            {
                get => this.ReadDataULong(256UL, 0UL);
                set => this.WriteData(256UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> TransactionHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(7);
                set => Link(7, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xca58b107d3de755aUL)]
    public class Enchantment : ICapnpSerializable
    {
        public const UInt64 typeId = 0xca58b107d3de755aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            EnchantmentId = reader.EnchantmentId;
            Name = reader.Name;
            Effect = reader.Effect;
            Power = reader.Power;
            Duration = reader.Duration;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.EnchantmentId.Init(EnchantmentId);
            writer.Name = Name;
            writer.Effect.Init(Effect);
            writer.Power = Power;
            writer.Duration = Duration;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> EnchantmentId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Effect
        {
            get;
            set;
        }

        public uint Power
        {
            get;
            set;
        }

        public ulong Duration
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
            public IReadOnlyList<byte> EnchantmentId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public IReadOnlyList<byte> Effect => ctx.ReadList(2).CastByte();
            public uint Power => ctx.ReadDataUInt(0UL, 0U);
            public ulong Duration => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 3);
            }

            public ListOfPrimitivesSerializer<byte> EnchantmentId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public ListOfPrimitivesSerializer<byte> Effect
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public uint Power
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public ulong Duration
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9d8a3b72d6302919UL)]
    public class Socket : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9d8a3b72d6302919UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SocketType = reader.SocketType;
            GemId = reader.GemId;
            IsActive = reader.IsActive;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.SocketType = SocketType;
            writer.GemId.Init(GemId);
            writer.IsActive = IsActive;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string SocketType
        {
            get;
            set;
        }

        public IReadOnlyList<byte> GemId
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
            public string SocketType => ctx.ReadText(0, null);
            public IReadOnlyList<byte> GemId => ctx.ReadList(1).CastByte();
            public bool IsActive => ctx.ReadDataBool(0UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public string SocketType
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ListOfPrimitivesSerializer<byte> GemId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public bool IsActive
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xcbca5f878f3cfca0UL)]
    public class Loadout : ICapnpSerializable
    {
        public const UInt64 typeId = 0xcbca5f878f3cfca0UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            LoadoutId = reader.LoadoutId;
            Name = reader.Name;
            PlayerId = reader.PlayerId;
            SlotMappings = reader.SlotMappings?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.SlotMapping>(_));
            IsActive = reader.IsActive;
            IsDefault = reader.IsDefault;
            GameId = reader.GameId;
            ModeId = reader.ModeId;
            TotalStats = reader.TotalStats;
            Rating = reader.Rating;
            CreatedAt = reader.CreatedAt;
            LastUsed = reader.LastUsed;
            UsageCount = reader.UsageCount;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.LoadoutId.Init(LoadoutId);
            writer.Name = Name;
            writer.PlayerId.Init(PlayerId);
            writer.SlotMappings.Init(SlotMappings, (_s1, _v1) => _v1?.serialize(_s1));
            writer.IsActive = IsActive;
            writer.IsDefault = IsDefault;
            writer.GameId.Init(GameId);
            writer.ModeId.Init(ModeId);
            writer.TotalStats.Init(TotalStats);
            writer.Rating = Rating;
            writer.CreatedAt = CreatedAt;
            writer.LastUsed = LastUsed;
            writer.UsageCount = UsageCount;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> LoadoutId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PlayerId
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.SlotMapping> SlotMappings
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public bool IsDefault
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

        public IReadOnlyList<byte> TotalStats
        {
            get;
            set;
        }

        public uint Rating
        {
            get;
            set;
        }

        public ulong CreatedAt
        {
            get;
            set;
        }

        public ulong LastUsed
        {
            get;
            set;
        }

        public uint UsageCount
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
            public IReadOnlyList<byte> LoadoutId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public IReadOnlyList<byte> PlayerId => ctx.ReadList(2).CastByte();
            public IReadOnlyList<CapnpGen.SlotMapping.READER> SlotMappings => ctx.ReadList(3).Cast(CapnpGen.SlotMapping.READER.create);
            public bool IsActive => ctx.ReadDataBool(0UL, false);
            public bool IsDefault => ctx.ReadDataBool(1UL, false);
            public IReadOnlyList<byte> GameId => ctx.ReadList(4).CastByte();
            public IReadOnlyList<byte> ModeId => ctx.ReadList(5).CastByte();
            public IReadOnlyList<byte> TotalStats => ctx.ReadList(6).CastByte();
            public uint Rating => ctx.ReadDataUInt(32UL, 0U);
            public ulong CreatedAt => ctx.ReadDataULong(64UL, 0UL);
            public ulong LastUsed => ctx.ReadDataULong(128UL, 0UL);
            public uint UsageCount => ctx.ReadDataUInt(192UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 7);
            }

            public ListOfPrimitivesSerializer<byte> LoadoutId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public ListOfPrimitivesSerializer<byte> PlayerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfStructsSerializer<CapnpGen.SlotMapping.WRITER> SlotMappings
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.SlotMapping.WRITER>>(3);
                set => Link(3, value);
            }

            public bool IsActive
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public bool IsDefault
            {
                get => this.ReadDataBool(1UL, false);
                set => this.WriteData(1UL, value, false);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public ListOfPrimitivesSerializer<byte> ModeId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(5);
                set => Link(5, value);
            }

            public ListOfPrimitivesSerializer<byte> TotalStats
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(6);
                set => Link(6, value);
            }

            public uint Rating
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public ulong CreatedAt
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong LastUsed
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public uint UsageCount
            {
                get => this.ReadDataUInt(192UL, 0U);
                set => this.WriteData(192UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb640c49fc388622aUL)]
    public class SlotMapping : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb640c49fc388622aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SlotType = reader.SlotType;
            ItemId = reader.ItemId;
            Position = reader.Position;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.SlotType = SlotType;
            writer.ItemId.Init(ItemId);
            writer.Position = Position;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string SlotType
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ItemId
        {
            get;
            set;
        }

        public byte Position
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
            public string SlotType => ctx.ReadText(0, null);
            public IReadOnlyList<byte> ItemId => ctx.ReadList(1).CastByte();
            public byte Position => ctx.ReadDataByte(0UL, (byte)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public string SlotType
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ListOfPrimitivesSerializer<byte> ItemId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public byte Position
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe2a3e44b1bfb67ebUL)]
    public class Recipe : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe2a3e44b1bfb67ebUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RecipeId = reader.RecipeId;
            Name = reader.Name;
            Description = reader.Description;
            Ingredients = reader.Ingredients?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RecipeIngredient>(_));
            OutputItem = reader.OutputItem;
            OutputQuantity = reader.OutputQuantity;
            SuccessRate = reader.SuccessRate;
            SkillLevel = reader.SkillLevel;
            Workstation = reader.Workstation;
            CraftingTimeMs = reader.CraftingTimeMs;
            IsDiscoverable = reader.IsDiscoverable;
            IsTradable = reader.IsTradable;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RecipeId.Init(RecipeId);
            writer.Name = Name;
            writer.Description = Description;
            writer.Ingredients.Init(Ingredients, (_s1, _v1) => _v1?.serialize(_s1));
            writer.OutputItem.Init(OutputItem);
            writer.OutputQuantity = OutputQuantity;
            writer.SuccessRate = SuccessRate;
            writer.SkillLevel = SkillLevel;
            writer.Workstation = Workstation;
            writer.CraftingTimeMs = CraftingTimeMs;
            writer.IsDiscoverable = IsDiscoverable;
            writer.IsTradable = IsTradable;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> RecipeId
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

        public IReadOnlyList<CapnpGen.RecipeIngredient> Ingredients
        {
            get;
            set;
        }

        public IReadOnlyList<byte> OutputItem
        {
            get;
            set;
        }

        public uint OutputQuantity
        {
            get;
            set;
        }

        public float SuccessRate
        {
            get;
            set;
        }

        public uint SkillLevel
        {
            get;
            set;
        }

        public string Workstation
        {
            get;
            set;
        }

        public ulong CraftingTimeMs
        {
            get;
            set;
        }

        public bool IsDiscoverable
        {
            get;
            set;
        }

        public bool IsTradable
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
            public IReadOnlyList<byte> RecipeId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public string Description => ctx.ReadText(2, null);
            public IReadOnlyList<CapnpGen.RecipeIngredient.READER> Ingredients => ctx.ReadList(3).Cast(CapnpGen.RecipeIngredient.READER.create);
            public IReadOnlyList<byte> OutputItem => ctx.ReadList(4).CastByte();
            public uint OutputQuantity => ctx.ReadDataUInt(0UL, 0U);
            public float SuccessRate => ctx.ReadDataFloat(32UL, 0F);
            public uint SkillLevel => ctx.ReadDataUInt(64UL, 0U);
            public string Workstation => ctx.ReadText(5, null);
            public ulong CraftingTimeMs => ctx.ReadDataULong(128UL, 0UL);
            public bool IsDiscoverable => ctx.ReadDataBool(96UL, false);
            public bool IsTradable => ctx.ReadDataBool(97UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 6);
            }

            public ListOfPrimitivesSerializer<byte> RecipeId
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

            public ListOfStructsSerializer<CapnpGen.RecipeIngredient.WRITER> Ingredients
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.RecipeIngredient.WRITER>>(3);
                set => Link(3, value);
            }

            public ListOfPrimitivesSerializer<byte> OutputItem
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public uint OutputQuantity
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public float SuccessRate
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public uint SkillLevel
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }

            public string Workstation
            {
                get => this.ReadText(5, null);
                set => this.WriteText(5, value, null);
            }

            public ulong CraftingTimeMs
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public bool IsDiscoverable
            {
                get => this.ReadDataBool(96UL, false);
                set => this.WriteData(96UL, value, false);
            }

            public bool IsTradable
            {
                get => this.ReadDataBool(97UL, false);
                set => this.WriteData(97UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc12ad609a3b45429UL)]
    public class RecipeIngredient : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc12ad609a3b45429UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ItemId = reader.ItemId;
            Quantity = reader.Quantity;
            Quality = reader.Quality;
            IsConsumed = reader.IsConsumed;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ItemId.Init(ItemId);
            writer.Quantity = Quantity;
            writer.Quality = Quality;
            writer.IsConsumed = IsConsumed;
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

        public CapnpGen.Rarity Quality
        {
            get;
            set;
        }

        public bool IsConsumed
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
            public CapnpGen.Rarity Quality => (CapnpGen.Rarity)ctx.ReadDataUShort(32UL, (ushort)0);
            public bool IsConsumed => ctx.ReadDataBool(48UL, false);
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

            public CapnpGen.Rarity Quality
            {
                get => (CapnpGen.Rarity)this.ReadDataUShort(32UL, (ushort)0);
                set => this.WriteData(32UL, (ushort)value, (ushort)0);
            }

            public bool IsConsumed
            {
                get => this.ReadDataBool(48UL, false);
                set => this.WriteData(48UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbad1421abb0aa00aUL)]
    public class TradeOffer : ICapnpSerializable
    {
        public const UInt64 typeId = 0xbad1421abb0aa00aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            OfferId = reader.OfferId;
            SellerId = reader.SellerId;
            OfferedItems = reader.OfferedItems?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.TradeItem>(_));
            RequestedItems = reader.RequestedItems?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.TradeItem>(_));
            Price = reader.Price;
            CurrencyToken = reader.CurrencyToken;
            Expiration = reader.Expiration;
            IsActive = reader.IsActive;
            IsAuction = reader.IsAuction;
            MinimumBid = reader.MinimumBid;
            BuyoutPrice = reader.BuyoutPrice;
            EscrowAddress = reader.EscrowAddress;
            ChainId = reader.ChainId;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.OfferId.Init(OfferId);
            writer.SellerId.Init(SellerId);
            writer.OfferedItems.Init(OfferedItems, (_s1, _v1) => _v1?.serialize(_s1));
            writer.RequestedItems.Init(RequestedItems, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Price = Price;
            writer.CurrencyToken.Init(CurrencyToken);
            writer.Expiration = Expiration;
            writer.IsActive = IsActive;
            writer.IsAuction = IsAuction;
            writer.MinimumBid = MinimumBid;
            writer.BuyoutPrice = BuyoutPrice;
            writer.EscrowAddress.Init(EscrowAddress);
            writer.ChainId = ChainId;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> OfferId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> SellerId
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.TradeItem> OfferedItems
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.TradeItem> RequestedItems
        {
            get;
            set;
        }

        public ulong Price
        {
            get;
            set;
        }

        public IReadOnlyList<byte> CurrencyToken
        {
            get;
            set;
        }

        public ulong Expiration
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public bool IsAuction
        {
            get;
            set;
        }

        public ulong MinimumBid
        {
            get;
            set;
        }

        public ulong BuyoutPrice
        {
            get;
            set;
        }

        public IReadOnlyList<byte> EscrowAddress
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
            public IReadOnlyList<byte> OfferId => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> SellerId => ctx.ReadList(1).CastByte();
            public IReadOnlyList<CapnpGen.TradeItem.READER> OfferedItems => ctx.ReadList(2).Cast(CapnpGen.TradeItem.READER.create);
            public IReadOnlyList<CapnpGen.TradeItem.READER> RequestedItems => ctx.ReadList(3).Cast(CapnpGen.TradeItem.READER.create);
            public ulong Price => ctx.ReadDataULong(0UL, 0UL);
            public IReadOnlyList<byte> CurrencyToken => ctx.ReadList(4).CastByte();
            public ulong Expiration => ctx.ReadDataULong(64UL, 0UL);
            public bool IsActive => ctx.ReadDataBool(128UL, false);
            public bool IsAuction => ctx.ReadDataBool(129UL, false);
            public ulong MinimumBid => ctx.ReadDataULong(192UL, 0UL);
            public ulong BuyoutPrice => ctx.ReadDataULong(256UL, 0UL);
            public IReadOnlyList<byte> EscrowAddress => ctx.ReadList(5).CastByte();
            public ulong ChainId => ctx.ReadDataULong(320UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(6, 6);
            }

            public ListOfPrimitivesSerializer<byte> OfferId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> SellerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfStructsSerializer<CapnpGen.TradeItem.WRITER> OfferedItems
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.TradeItem.WRITER>>(2);
                set => Link(2, value);
            }

            public ListOfStructsSerializer<CapnpGen.TradeItem.WRITER> RequestedItems
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.TradeItem.WRITER>>(3);
                set => Link(3, value);
            }

            public ulong Price
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> CurrencyToken
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public ulong Expiration
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public bool IsActive
            {
                get => this.ReadDataBool(128UL, false);
                set => this.WriteData(128UL, value, false);
            }

            public bool IsAuction
            {
                get => this.ReadDataBool(129UL, false);
                set => this.WriteData(129UL, value, false);
            }

            public ulong MinimumBid
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public ulong BuyoutPrice
            {
                get => this.ReadDataULong(256UL, 0UL);
                set => this.WriteData(256UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> EscrowAddress
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(5);
                set => Link(5, value);
            }

            public ulong ChainId
            {
                get => this.ReadDataULong(320UL, 0UL);
                set => this.WriteData(320UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x88599918a2360c75UL)]
    public class TradeItem : ICapnpSerializable
    {
        public const UInt64 typeId = 0x88599918a2360c75UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ItemId = reader.ItemId;
            Quantity = reader.Quantity;
            InspectionHash = reader.InspectionHash;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ItemId.Init(ItemId);
            writer.Quantity = Quantity;
            writer.InspectionHash.Init(InspectionHash);
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

        public IReadOnlyList<byte> InspectionHash
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
            public IReadOnlyList<byte> InspectionHash => ctx.ReadList(1).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
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

            public ListOfPrimitivesSerializer<byte> InspectionHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe4ea99d1dc9b8809UL), Proxy(typeof(InventoryService_Proxy)), Skeleton(typeof(InventoryService_Skeleton))]
    public interface IInventoryService : IDisposable
    {
        Task<CapnpGen.ItemMetadata> GetItemMetadata(IReadOnlyList<byte> itemId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> CreateItem(CapnpGen.ItemMetadata metadata, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task UpdateItem(IReadOnlyList<byte> itemId, CapnpGen.ItemMetadata metadata, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.InventoryItem>> GetInventory(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> AddItem(IReadOnlyList<byte> playerId, IReadOnlyList<byte> templateId, uint quantity, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task RemoveItem(IReadOnlyList<byte> playerId, IReadOnlyList<byte> itemId, uint quantity, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task TransferItem(IReadOnlyList<byte> fromPlayer, IReadOnlyList<byte> toPlayer, IReadOnlyList<byte> itemId, uint quantity, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task EquipItem(IReadOnlyList<byte> playerId, IReadOnlyList<byte> itemId, string slot, CancellationToken cancellationToken_ = default);
        Task UnequipItem(IReadOnlyList<byte> playerId, string slot, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> CreateLoadout(CapnpGen.Loadout loadout, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task ActivateLoadout(IReadOnlyList<byte> playerId, IReadOnlyList<byte> loadoutId, CancellationToken cancellationToken_ = default);
        Task<(bool, IReadOnlyList<byte>)> CraftItem(IReadOnlyList<byte> playerId, IReadOnlyList<byte> recipeId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.Recipe>> GetRecipes(IReadOnlyList<byte> playerId, CapnpGen.RecipeFilter filter, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> CreateTradeOffer(CapnpGen.TradeOffer offer, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task AcceptTradeOffer(IReadOnlyList<byte> offerId, IReadOnlyList<byte> buyerId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task CancelTradeOffer(IReadOnlyList<byte> offerId, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<(ulong, IReadOnlyList<CapnpGen.AssetValue>)> GetInventoryValue(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe4ea99d1dc9b8809UL)]
    public class InventoryService_Proxy : Proxy, IInventoryService
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

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe4ea99d1dc9b8809UL)]
    public class InventoryService_Skeleton : Skeleton<IInventoryService>
    {
        public InventoryService_Skeleton()
        {
            SetMethodTable(GetItemMetadata, CreateItem, UpdateItem, GetInventory, AddItem, RemoveItem, TransferItem, EquipItem, UnequipItem, CreateLoadout, ActivateLoadout, CraftItem, GetRecipes, CreateTradeOffer, AcceptTradeOffer, CancelTradeOffer, GetInventoryValue);
        }

        public override ulong InterfaceId => 16495165711826257929UL;
        Task<AnswerOrCounterquestion> GetItemMetadata(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_GetItemMetadata>(d_);
                return Impatient.MaybeTailCall(Impl.GetItemMetadata(in_.ItemId, cancellationToken_), metadata =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_GetItemMetadata.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_GetItemMetadata{Metadata = metadata};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> CreateItem(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_CreateItem>(d_);
                return Impatient.MaybeTailCall(Impl.CreateItem(in_.Metadata, in_.Signature, cancellationToken_), itemId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_CreateItem.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_CreateItem{ItemId = itemId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> UpdateItem(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_UpdateItem>(d_);
                await Impl.UpdateItem(in_.ItemId, in_.Metadata, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_UpdateItem.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GetInventory(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_GetInventory>(d_);
                return Impatient.MaybeTailCall(Impl.GetInventory(in_.PlayerId, cancellationToken_), items =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_GetInventory.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_GetInventory{Items = items};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> AddItem(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_AddItem>(d_);
                return Impatient.MaybeTailCall(Impl.AddItem(in_.PlayerId, in_.TemplateId, in_.Quantity, in_.Signature, cancellationToken_), itemId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_AddItem.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_AddItem{ItemId = itemId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> RemoveItem(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_RemoveItem>(d_);
                await Impl.RemoveItem(in_.PlayerId, in_.ItemId, in_.Quantity, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_RemoveItem.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> TransferItem(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_TransferItem>(d_);
                await Impl.TransferItem(in_.FromPlayer, in_.ToPlayer, in_.ItemId, in_.Quantity, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_TransferItem.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> EquipItem(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_EquipItem>(d_);
                await Impl.EquipItem(in_.PlayerId, in_.ItemId, in_.Slot, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_EquipItem.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> UnequipItem(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_UnequipItem>(d_);
                await Impl.UnequipItem(in_.PlayerId, in_.Slot, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_UnequipItem.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> CreateLoadout(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_CreateLoadout>(d_);
                return Impatient.MaybeTailCall(Impl.CreateLoadout(in_.Loadout, in_.Signature, cancellationToken_), loadoutId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_CreateLoadout.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_CreateLoadout{LoadoutId = loadoutId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> ActivateLoadout(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_ActivateLoadout>(d_);
                await Impl.ActivateLoadout(in_.PlayerId, in_.LoadoutId, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_ActivateLoadout.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> CraftItem(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_CraftItem>(d_);
                return Impatient.MaybeTailCall(Impl.CraftItem(in_.PlayerId, in_.RecipeId, in_.Signature, cancellationToken_), (success, itemId) =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_CraftItem.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_CraftItem{Success = success, ItemId = itemId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetRecipes(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_GetRecipes>(d_);
                return Impatient.MaybeTailCall(Impl.GetRecipes(in_.PlayerId, in_.Filter, cancellationToken_), recipes =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_GetRecipes.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_GetRecipes{Recipes = recipes};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> CreateTradeOffer(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_CreateTradeOffer>(d_);
                return Impatient.MaybeTailCall(Impl.CreateTradeOffer(in_.Offer, in_.Signature, cancellationToken_), offerId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_CreateTradeOffer.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_CreateTradeOffer{OfferId = offerId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> AcceptTradeOffer(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_AcceptTradeOffer>(d_);
                await Impl.AcceptTradeOffer(in_.OfferId, in_.BuyerId, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_AcceptTradeOffer.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> CancelTradeOffer(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_CancelTradeOffer>(d_);
                await Impl.CancelTradeOffer(in_.OfferId, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_CancelTradeOffer.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GetInventoryValue(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.InventoryService.Params_GetInventoryValue>(d_);
                return Impatient.MaybeTailCall(Impl.GetInventoryValue(in_.PlayerId, cancellationToken_), (value, breakdown) =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.InventoryService.Result_GetInventoryValue.WRITER>();
                    var r_ = new CapnpGen.InventoryService.Result_GetInventoryValue{Value = value, Breakdown = breakdown};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class InventoryService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x93da11d0344976d1UL)]
        public class Params_GetItemMetadata : ICapnpSerializable
        {
            public const UInt64 typeId = 0x93da11d0344976d1UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                ItemId = reader.ItemId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.ItemId.Init(ItemId);
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
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> ItemId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa5761c6091061182UL)]
        public class Result_GetItemMetadata : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa5761c6091061182UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Metadata = CapnpSerializable.Create<CapnpGen.ItemMetadata>(reader.Metadata);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Metadata?.serialize(writer.Metadata);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.ItemMetadata Metadata
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
                public CapnpGen.ItemMetadata.READER Metadata => ctx.ReadStruct(0, CapnpGen.ItemMetadata.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.ItemMetadata.WRITER Metadata
                {
                    get => BuildPointer<CapnpGen.ItemMetadata.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdaeb169ac9574518UL)]
        public class Params_CreateItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xdaeb169ac9574518UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Metadata = CapnpSerializable.Create<CapnpGen.ItemMetadata>(reader.Metadata);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Metadata?.serialize(writer.Metadata);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.ItemMetadata Metadata
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
                public CapnpGen.ItemMetadata.READER Metadata => ctx.ReadStruct(0, CapnpGen.ItemMetadata.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.ItemMetadata.WRITER Metadata
                {
                    get => BuildPointer<CapnpGen.ItemMetadata.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8611ab9d387baf42UL)]
        public class Result_CreateItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8611ab9d387baf42UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                ItemId = reader.ItemId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.ItemId.Init(ItemId);
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
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> ItemId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xed12d48b6719d947UL)]
        public class Params_UpdateItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xed12d48b6719d947UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                ItemId = reader.ItemId;
                Metadata = CapnpSerializable.Create<CapnpGen.ItemMetadata>(reader.Metadata);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.ItemId.Init(ItemId);
                Metadata?.serialize(writer.Metadata);
                writer.Signature.Init(Signature);
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

            public CapnpGen.ItemMetadata Metadata
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
                public IReadOnlyList<byte> ItemId => ctx.ReadList(0).CastByte();
                public CapnpGen.ItemMetadata.READER Metadata => ctx.ReadStruct(1, CapnpGen.ItemMetadata.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> ItemId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.ItemMetadata.WRITER Metadata
                {
                    get => BuildPointer<CapnpGen.ItemMetadata.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd5566c8adea372ccUL)]
        public class Result_UpdateItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd5566c8adea372ccUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9c0c47c46e08b234UL)]
        public class Params_GetInventory : ICapnpSerializable
        {
            public const UInt64 typeId = 0x9c0c47c46e08b234UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8c124a600ea982a8UL)]
        public class Result_GetInventory : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8c124a600ea982a8UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Items = reader.Items?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.InventoryItem>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Items.Init(Items, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.InventoryItem> Items
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
                public IReadOnlyList<CapnpGen.InventoryItem.READER> Items => ctx.ReadList(0).Cast(CapnpGen.InventoryItem.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.InventoryItem.WRITER> Items
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.InventoryItem.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbd5d085bf32dc027UL)]
        public class Params_AddItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xbd5d085bf32dc027UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                TemplateId = reader.TemplateId;
                Quantity = reader.Quantity;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.TemplateId.Init(TemplateId);
                writer.Quantity = Quantity;
                writer.Signature.Init(Signature);
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

            public IReadOnlyList<byte> TemplateId
            {
                get;
                set;
            }

            public uint Quantity
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
                public IReadOnlyList<byte> PlayerId => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> TemplateId => ctx.ReadList(1).CastByte();
                public uint Quantity => ctx.ReadDataUInt(0UL, 0U);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
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

                public ListOfPrimitivesSerializer<byte> TemplateId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }

                public uint Quantity
                {
                    get => this.ReadDataUInt(0UL, 0U);
                    set => this.WriteData(0UL, value, 0U);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd713606623eba32eUL)]
        public class Result_AddItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd713606623eba32eUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                ItemId = reader.ItemId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.ItemId.Init(ItemId);
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
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> ItemId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe9fc83a2f4583317UL)]
        public class Params_RemoveItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe9fc83a2f4583317UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                ItemId = reader.ItemId;
                Quantity = reader.Quantity;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.ItemId.Init(ItemId);
                writer.Quantity = Quantity;
                writer.Signature.Init(Signature);
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
                public IReadOnlyList<byte> PlayerId => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> ItemId => ctx.ReadList(1).CastByte();
                public uint Quantity => ctx.ReadDataUInt(0UL, 0U);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
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

                public ListOfPrimitivesSerializer<byte> ItemId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }

                public uint Quantity
                {
                    get => this.ReadDataUInt(0UL, 0U);
                    set => this.WriteData(0UL, value, 0U);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf1b841ace4f37e64UL)]
        public class Result_RemoveItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf1b841ace4f37e64UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe0ec69e006154f63UL)]
        public class Params_TransferItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe0ec69e006154f63UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                FromPlayer = reader.FromPlayer;
                ToPlayer = reader.ToPlayer;
                ItemId = reader.ItemId;
                Quantity = reader.Quantity;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.FromPlayer.Init(FromPlayer);
                writer.ToPlayer.Init(ToPlayer);
                writer.ItemId.Init(ItemId);
                writer.Quantity = Quantity;
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> FromPlayer
            {
                get;
                set;
            }

            public IReadOnlyList<byte> ToPlayer
            {
                get;
                set;
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
                public IReadOnlyList<byte> FromPlayer => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> ToPlayer => ctx.ReadList(1).CastByte();
                public IReadOnlyList<byte> ItemId => ctx.ReadList(2).CastByte();
                public uint Quantity => ctx.ReadDataUInt(0UL, 0U);
                public IReadOnlyList<byte> Signature => ctx.ReadList(3).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 4);
                }

                public ListOfPrimitivesSerializer<byte> FromPlayer
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> ToPlayer
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> ItemId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }

                public uint Quantity
                {
                    get => this.ReadDataUInt(0UL, 0U);
                    set => this.WriteData(0UL, value, 0U);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                    set => Link(3, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xff68421ea80b39cfUL)]
        public class Result_TransferItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xff68421ea80b39cfUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf14313574a4e082bUL)]
        public class Params_EquipItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf14313574a4e082bUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                ItemId = reader.ItemId;
                Slot = reader.Slot;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.ItemId.Init(ItemId);
                writer.Slot = Slot;
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

            public IReadOnlyList<byte> ItemId
            {
                get;
                set;
            }

            public string Slot
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
                public IReadOnlyList<byte> ItemId => ctx.ReadList(1).CastByte();
                public string Slot => ctx.ReadText(2, null);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> ItemId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }

                public string Slot
                {
                    get => this.ReadText(2, null);
                    set => this.WriteText(2, value, null);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x987ae588f31c399cUL)]
        public class Result_EquipItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0x987ae588f31c399cUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf3ae7b9110250806UL)]
        public class Params_UnequipItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf3ae7b9110250806UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                Slot = reader.Slot;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.Slot = Slot;
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

            public string Slot
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
                public string Slot => ctx.ReadText(1, null);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public string Slot
                {
                    get => this.ReadText(1, null);
                    set => this.WriteText(1, value, null);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x80b1dec0210fb599UL)]
        public class Result_UnequipItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0x80b1dec0210fb599UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa024b44b830f425fUL)]
        public class Params_CreateLoadout : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa024b44b830f425fUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Loadout = CapnpSerializable.Create<CapnpGen.Loadout>(reader.Loadout);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Loadout?.serialize(writer.Loadout);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.Loadout Loadout
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
                public CapnpGen.Loadout.READER Loadout => ctx.ReadStruct(0, CapnpGen.Loadout.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.Loadout.WRITER Loadout
                {
                    get => BuildPointer<CapnpGen.Loadout.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc24b6b249de8f001UL)]
        public class Result_CreateLoadout : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc24b6b249de8f001UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                LoadoutId = reader.LoadoutId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.LoadoutId.Init(LoadoutId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> LoadoutId
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
                public IReadOnlyList<byte> LoadoutId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> LoadoutId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xeae41dd200138740UL)]
        public class Params_ActivateLoadout : ICapnpSerializable
        {
            public const UInt64 typeId = 0xeae41dd200138740UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                LoadoutId = reader.LoadoutId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.LoadoutId.Init(LoadoutId);
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

            public IReadOnlyList<byte> LoadoutId
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
                public IReadOnlyList<byte> LoadoutId => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> LoadoutId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd7985099945d6728UL)]
        public class Result_ActivateLoadout : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd7985099945d6728UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xeb56c8fc5f6c0fecUL)]
        public class Params_CraftItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xeb56c8fc5f6c0fecUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                RecipeId = reader.RecipeId;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.RecipeId.Init(RecipeId);
                writer.Signature.Init(Signature);
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

            public IReadOnlyList<byte> RecipeId
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
                public IReadOnlyList<byte> PlayerId => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> RecipeId => ctx.ReadList(1).CastByte();
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> RecipeId
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe44c3e85b6f4ae91UL)]
        public class Result_CraftItem : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe44c3e85b6f4ae91UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Success = reader.Success;
                ItemId = reader.ItemId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Success = Success;
                writer.ItemId.Init(ItemId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public bool Success
            {
                get;
                set;
            }

            public IReadOnlyList<byte> ItemId
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
                public bool Success => ctx.ReadDataBool(0UL, false);
                public IReadOnlyList<byte> ItemId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 1);
                }

                public bool Success
                {
                    get => this.ReadDataBool(0UL, false);
                    set => this.WriteData(0UL, value, false);
                }

                public ListOfPrimitivesSerializer<byte> ItemId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc178156965e8aa1dUL)]
        public class Params_GetRecipes : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc178156965e8aa1dUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                Filter = CapnpSerializable.Create<CapnpGen.RecipeFilter>(reader.Filter);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                Filter?.serialize(writer.Filter);
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

            public CapnpGen.RecipeFilter Filter
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
                public CapnpGen.RecipeFilter.READER Filter => ctx.ReadStruct(1, CapnpGen.RecipeFilter.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> PlayerId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.RecipeFilter.WRITER Filter
                {
                    get => BuildPointer<CapnpGen.RecipeFilter.WRITER>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc3518af77a825a31UL)]
        public class Result_GetRecipes : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc3518af77a825a31UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Recipes = reader.Recipes?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Recipe>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Recipes.Init(Recipes, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.Recipe> Recipes
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
                public IReadOnlyList<CapnpGen.Recipe.READER> Recipes => ctx.ReadList(0).Cast(CapnpGen.Recipe.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.Recipe.WRITER> Recipes
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.Recipe.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd86675c7c6b6f753UL)]
        public class Params_CreateTradeOffer : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd86675c7c6b6f753UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Offer = CapnpSerializable.Create<CapnpGen.TradeOffer>(reader.Offer);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Offer?.serialize(writer.Offer);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.TradeOffer Offer
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
                public CapnpGen.TradeOffer.READER Offer => ctx.ReadStruct(0, CapnpGen.TradeOffer.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.TradeOffer.WRITER Offer
                {
                    get => BuildPointer<CapnpGen.TradeOffer.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xef1a9eff0decb534UL)]
        public class Result_CreateTradeOffer : ICapnpSerializable
        {
            public const UInt64 typeId = 0xef1a9eff0decb534UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                OfferId = reader.OfferId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.OfferId.Init(OfferId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> OfferId
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
                public IReadOnlyList<byte> OfferId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> OfferId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbd4a526b554df34eUL)]
        public class Params_AcceptTradeOffer : ICapnpSerializable
        {
            public const UInt64 typeId = 0xbd4a526b554df34eUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                OfferId = reader.OfferId;
                BuyerId = reader.BuyerId;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.OfferId.Init(OfferId);
                writer.BuyerId.Init(BuyerId);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> OfferId
            {
                get;
                set;
            }

            public IReadOnlyList<byte> BuyerId
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
                public IReadOnlyList<byte> OfferId => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> BuyerId => ctx.ReadList(1).CastByte();
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> OfferId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> BuyerId
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xabb43160498f1f6cUL)]
        public class Result_AcceptTradeOffer : ICapnpSerializable
        {
            public const UInt64 typeId = 0xabb43160498f1f6cUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd28ac098fbb6dc65UL)]
        public class Params_CancelTradeOffer : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd28ac098fbb6dc65UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                OfferId = reader.OfferId;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.OfferId.Init(OfferId);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> OfferId
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
                public IReadOnlyList<byte> OfferId => ctx.ReadList(0).CastByte();
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> OfferId
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdf7f4ec33689cf6aUL)]
        public class Result_CancelTradeOffer : ICapnpSerializable
        {
            public const UInt64 typeId = 0xdf7f4ec33689cf6aUL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd439b8fba834baa7UL)]
        public class Params_GetInventoryValue : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd439b8fba834baa7UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xacdfb0f7e3e18f77UL)]
        public class Result_GetInventoryValue : ICapnpSerializable
        {
            public const UInt64 typeId = 0xacdfb0f7e3e18f77UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Value = reader.Value;
                Breakdown = reader.Breakdown?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.AssetValue>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Value = Value;
                writer.Breakdown.Init(Breakdown, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public ulong Value
            {
                get;
                set;
            }

            public IReadOnlyList<CapnpGen.AssetValue> Breakdown
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
                public ulong Value => ctx.ReadDataULong(0UL, 0UL);
                public IReadOnlyList<CapnpGen.AssetValue.READER> Breakdown => ctx.ReadList(0).Cast(CapnpGen.AssetValue.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 1);
                }

                public ulong Value
                {
                    get => this.ReadDataULong(0UL, 0UL);
                    set => this.WriteData(0UL, value, 0UL);
                }

                public ListOfStructsSerializer<CapnpGen.AssetValue.WRITER> Breakdown
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.AssetValue.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa5acf573be1a003cUL)]
    public class RecipeFilter : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa5acf573be1a003cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RequiredIngredients = reader.RequiredIngredients;
            MaxDifficulty = reader.MaxDifficulty;
            AvailableOnly = reader.AvailableOnly;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RequiredIngredients.Init(RequiredIngredients, (_s1, _v1) => _s1.Init(_v1));
            writer.MaxDifficulty = MaxDifficulty;
            writer.AvailableOnly = AvailableOnly;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<IReadOnlyList<byte>> RequiredIngredients
        {
            get;
            set;
        }

        public uint MaxDifficulty
        {
            get;
            set;
        }

        public bool AvailableOnly
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
            public IReadOnlyList<IReadOnlyList<byte>> RequiredIngredients => ctx.ReadList(0).CastData();
            public uint MaxDifficulty => ctx.ReadDataUInt(0UL, 0U);
            public bool AvailableOnly => ctx.ReadDataBool(32UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> RequiredIngredients
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(0);
                set => Link(0, value);
            }

            public uint MaxDifficulty
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public bool AvailableOnly
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa1cf76af6f70deaeUL)]
    public class AssetValue : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa1cf76af6f70deaeUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ItemId = reader.ItemId;
            Quantity = reader.Quantity;
            UnitValue = reader.UnitValue;
            TotalValue = reader.TotalValue;
            ValuationMethod = reader.ValuationMethod;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ItemId.Init(ItemId);
            writer.Quantity = Quantity;
            writer.UnitValue = UnitValue;
            writer.TotalValue = TotalValue;
            writer.ValuationMethod = ValuationMethod;
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

        public ulong UnitValue
        {
            get;
            set;
        }

        public ulong TotalValue
        {
            get;
            set;
        }

        public string ValuationMethod
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
            public ulong UnitValue => ctx.ReadDataULong(64UL, 0UL);
            public ulong TotalValue => ctx.ReadDataULong(128UL, 0UL);
            public string ValuationMethod => ctx.ReadText(1, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 2);
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

            public ulong UnitValue
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong TotalValue
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public string ValuationMethod
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }
        }
    }
}