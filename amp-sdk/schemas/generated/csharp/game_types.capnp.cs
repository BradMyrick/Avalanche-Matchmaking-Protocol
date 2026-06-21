using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xafbd78655c3a90e8UL)]
    public class TurnBasedReplay : ICapnpSerializable
    {
        public const UInt64 typeId = 0xafbd78655c3a90e8UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            InitialState = reader.InitialState;
            Moves = reader.Moves?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.SignedMove>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.InitialState.Init(InitialState);
            writer.Moves.Init(Moves, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> InitialState
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.SignedMove> Moves
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
            public IReadOnlyList<byte> InitialState => ctx.ReadList(0).CastByte();
            public IReadOnlyList<CapnpGen.SignedMove.READER> Moves => ctx.ReadList(1).Cast(CapnpGen.SignedMove.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 2);
            }

            public ListOfPrimitivesSerializer<byte> InitialState
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfStructsSerializer<CapnpGen.SignedMove.WRITER> Moves
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.SignedMove.WRITER>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbd46ee4aa75c4d82UL)]
    public class SignedMove : ICapnpSerializable
    {
        public const UInt64 typeId = 0xbd46ee4aa75c4d82UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            MoveData = reader.MoveData;
            Timestamp = reader.Timestamp;
            Signature = reader.Signature;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId = PlayerId;
            writer.MoveData.Init(MoveData);
            writer.Timestamp = Timestamp;
            writer.Signature.Init(Signature);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public byte PlayerId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> MoveData
        {
            get;
            set;
        }

        public ulong Timestamp
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
            public byte PlayerId => ctx.ReadDataByte(0UL, (byte)0);
            public IReadOnlyList<byte> MoveData => ctx.ReadList(0).CastByte();
            public ulong Timestamp => ctx.ReadDataULong(64UL, 0UL);
            public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 2);
            }

            public byte PlayerId
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public ListOfPrimitivesSerializer<byte> MoveData
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ulong Timestamp
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> Signature
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf56dd6de663ff17cUL)]
    public class RealtimeTranscript : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf56dd6de663ff17cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Snapshots = reader.Snapshots?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.StateSnapshot>(_));
            Inputs = reader.Inputs;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Snapshots.Init(Snapshots, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Inputs.Init(Inputs);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<CapnpGen.StateSnapshot> Snapshots
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Inputs
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
            public IReadOnlyList<CapnpGen.StateSnapshot.READER> Snapshots => ctx.ReadList(0).Cast(CapnpGen.StateSnapshot.READER.create);
            public IReadOnlyList<byte> Inputs => ctx.ReadList(1).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 2);
            }

            public ListOfStructsSerializer<CapnpGen.StateSnapshot.WRITER> Snapshots
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.StateSnapshot.WRITER>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> Inputs
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc76b6d06e89a9d6fUL)]
    public class StateSnapshot : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc76b6d06e89a9d6fUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            FrameId = reader.FrameId;
            StateHash = reader.StateHash;
            Signatures = reader.Signatures;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.FrameId = FrameId;
            writer.StateHash.Init(StateHash);
            writer.Signatures.Init(Signatures, (_s1, _v1) => _s1.Init(_v1));
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

        public IReadOnlyList<byte> StateHash
        {
            get;
            set;
        }

        public IReadOnlyList<IReadOnlyList<byte>> Signatures
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
            public IReadOnlyList<byte> StateHash => ctx.ReadList(0).CastByte();
            public IReadOnlyList<IReadOnlyList<byte>> Signatures => ctx.ReadList(1).CastData();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public ulong FrameId
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> StateHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> Signatures
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc1fe1aeb7e84017eUL)]
    public class OracleResult : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc1fe1aeb7e84017eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Source = reader.Source;
            QueryId = reader.QueryId;
            Result = reader.Result;
            Proof = reader.Proof;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Source = Source;
            writer.QueryId.Init(QueryId);
            writer.Result.Init(Result);
            writer.Proof.Init(Proof);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string Source
        {
            get;
            set;
        }

        public IReadOnlyList<byte> QueryId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Result
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
            public string Source => ctx.ReadText(0, null);
            public IReadOnlyList<byte> QueryId => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> Result => ctx.ReadList(2).CastByte();
            public IReadOnlyList<byte> Proof => ctx.ReadList(3).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 4);
            }

            public string Source
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ListOfPrimitivesSerializer<byte> QueryId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> Result
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> Proof
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }
        }
    }
}