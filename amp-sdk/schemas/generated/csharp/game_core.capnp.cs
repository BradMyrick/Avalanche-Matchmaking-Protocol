using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xee648d69b5f2594cUL)]
    public class MatchConfig : ICapnpSerializable
    {
        public const UInt64 typeId = 0xee648d69b5f2594cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            GameId = reader.GameId;
            MaxPlayers = reader.MaxPlayers;
            TimeLimitMs = reader.TimeLimitMs;
            CustomRules = reader.CustomRules;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.GameId.Init(GameId);
            writer.MaxPlayers = MaxPlayers;
            writer.TimeLimitMs = TimeLimitMs;
            writer.CustomRules.Init(CustomRules);
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

        public byte MaxPlayers
        {
            get;
            set;
        }

        public ulong TimeLimitMs
        {
            get;
            set;
        }

        public IReadOnlyList<byte> CustomRules
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
            public byte MaxPlayers => ctx.ReadDataByte(0UL, (byte)0);
            public ulong TimeLimitMs => ctx.ReadDataULong(64UL, 0UL);
            public IReadOnlyList<byte> CustomRules => ctx.ReadList(1).CastByte();
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

            public byte MaxPlayers
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public ulong TimeLimitMs
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> CustomRules
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xecb2685cef27197bUL)]
    public class InputFrame : ICapnpSerializable
    {
        public const UInt64 typeId = 0xecb2685cef27197bUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            FrameId = reader.FrameId;
            PlayerId = reader.PlayerId;
            InputData = reader.InputData;
            Timestamp = reader.Timestamp;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.FrameId = FrameId;
            writer.PlayerId.Init(PlayerId);
            writer.InputData.Init(InputData);
            writer.Timestamp = Timestamp;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong FrameId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PlayerId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> InputData
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
            public ulong FrameId => ctx.ReadDataULong(0UL, 0UL);
            public IReadOnlyList<byte> PlayerId => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> InputData => ctx.ReadList(1).CastByte();
            public ulong Timestamp => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 2);
            }

            public ulong FrameId
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> PlayerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> InputData
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ulong Timestamp
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfbb0e0e0fa8b6c36UL)]
    public class GameEvent : ICapnpSerializable
    {
        public const UInt64 typeId = 0xfbb0e0e0fa8b6c36UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            EventId = reader.EventId;
            EventType = reader.EventType;
            EventData = reader.EventData;
            TriggeredBy = reader.TriggeredBy;
            Timestamp = reader.Timestamp;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.EventId = EventId;
            writer.EventType = EventType;
            writer.EventData.Init(EventData);
            writer.TriggeredBy.Init(TriggeredBy);
            writer.Timestamp = Timestamp;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong EventId
        {
            get;
            set;
        }

        public string EventType
        {
            get;
            set;
        }

        public IReadOnlyList<byte> EventData
        {
            get;
            set;
        }

        public IReadOnlyList<byte> TriggeredBy
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
            public ulong EventId => ctx.ReadDataULong(0UL, 0UL);
            public string EventType => ctx.ReadText(0, null);
            public IReadOnlyList<byte> EventData => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> TriggeredBy => ctx.ReadList(2).CastByte();
            public ulong Timestamp => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 3);
            }

            public ulong EventId
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public string EventType
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ListOfPrimitivesSerializer<byte> EventData
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> TriggeredBy
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ulong Timestamp
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }
}