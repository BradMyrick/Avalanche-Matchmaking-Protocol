using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdfaeb084e9eecfa1UL)]
    public enum TelemetryEventType : ushort
    {
        matchCreated,
        matchJoined,
        settlementSubmitted,
        verifierResult,
        unknown
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf58f241bce5b1fe7UL)]
    public class AmpTelemetryEvent : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf58f241bce5b1fe7UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchId = reader.MatchId;
            GameId = reader.GameId;
            EventType = reader.EventType;
            Timestamp = reader.Timestamp;
            VerifierId = reader.VerifierId;
            EventData = reader.EventData;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchId.Init(MatchId);
            writer.GameId = GameId;
            writer.EventType = EventType;
            writer.Timestamp = Timestamp;
            writer.VerifierId.Init(VerifierId);
            writer.EventData.Init(EventData);
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

        public ulong GameId
        {
            get;
            set;
        }

        public CapnpGen.TelemetryEventType EventType
        {
            get;
            set;
        }

        public ulong Timestamp
        {
            get;
            set;
        }

        public IReadOnlyList<byte> VerifierId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> EventData
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
            public ulong GameId => ctx.ReadDataULong(0UL, 0UL);
            public CapnpGen.TelemetryEventType EventType => (CapnpGen.TelemetryEventType)ctx.ReadDataUShort(64UL, (ushort)0);
            public ulong Timestamp => ctx.ReadDataULong(128UL, 0UL);
            public IReadOnlyList<byte> VerifierId => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> EventData => ctx.ReadList(2).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 3);
            }

            public ListOfPrimitivesSerializer<byte> MatchId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ulong GameId
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public CapnpGen.TelemetryEventType EventType
            {
                get => (CapnpGen.TelemetryEventType)this.ReadDataUShort(64UL, (ushort)0);
                set => this.WriteData(64UL, (ushort)value, (ushort)0);
            }

            public ulong Timestamp
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> VerifierId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> EventData
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8f01d0e46ee0ed81UL), Proxy(typeof(TelemetryReceiver_Proxy)), Skeleton(typeof(TelemetryReceiver_Skeleton))]
    public interface ITelemetryReceiver : IDisposable
    {
        Task LogEvent(CapnpGen.AmpTelemetryEvent @event, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8f01d0e46ee0ed81UL)]
    public class TelemetryReceiver_Proxy : Proxy, ITelemetryReceiver
    {
        public async Task LogEvent(CapnpGen.AmpTelemetryEvent @event, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.TelemetryReceiver.Params_LogEvent.WRITER>();
            var arg_ = new CapnpGen.TelemetryReceiver.Params_LogEvent()
            {Event = @event};
            arg_?.serialize(in_);
            using (var d_ = await Call(10304747101931761025UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.TelemetryReceiver.Result_LogEvent>(d_);
                return;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8f01d0e46ee0ed81UL)]
    public class TelemetryReceiver_Skeleton : Skeleton<ITelemetryReceiver>
    {
        public TelemetryReceiver_Skeleton()
        {
            SetMethodTable(LogEvent);
        }

        public override ulong InterfaceId => 10304747101931761025UL;
        async Task<AnswerOrCounterquestion> LogEvent(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.TelemetryReceiver.Params_LogEvent>(d_);
                await Impl.LogEvent(in_.Event, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.TelemetryReceiver.Result_LogEvent.WRITER>();
                return s_;
            }
        }
    }

    public static class TelemetryReceiver
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfc9f261140896255UL)]
        public class Params_LogEvent : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfc9f261140896255UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe76e3c41da02b24dUL)]
        public class Result_LogEvent : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe76e3c41da02b24dUL;
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