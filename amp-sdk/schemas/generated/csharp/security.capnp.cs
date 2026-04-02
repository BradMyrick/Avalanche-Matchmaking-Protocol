using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdc2029046698006cUL)]
    public enum FraudCategory : ushort
    {
        cheating,
        matchFixing,
        smurfing,
        boosting,
        collusion,
        disconnectAbuse,
        pingAbuse,
        accountSharing,
        botting,
        exploit,
        harassment,
        paymentFraud,
        identityFraud
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbef1027fb0d9e979UL)]
    public class SecurityIncident : ICapnpSerializable
    {
        public const UInt64 typeId = 0xbef1027fb0d9e979UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            IncidentId = reader.IncidentId;
            PlayerId = reader.PlayerId;
            Category = reader.Category;
            Description = reader.Description;
            Severity = reader.Severity;
            Confidence = reader.Confidence;
            Evidence = reader.Evidence?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Evidence>(_));
            MatchId = reader.MatchId;
            GameId = reader.GameId;
            DetectedAt = reader.DetectedAt;
            FirstOccurrence = reader.FirstOccurrence;
            LastOccurrence = reader.LastOccurrence;
            Status = reader.Status;
            AssignedTo = reader.AssignedTo;
            Resolution = CapnpSerializable.Create<CapnpGen.Resolution>(reader.Resolution);
            ImpactScore = reader.ImpactScore;
            PatternId = reader.PatternId;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.IncidentId.Init(IncidentId);
            writer.PlayerId.Init(PlayerId);
            writer.Category = Category;
            writer.Description = Description;
            writer.Severity = Severity;
            writer.Confidence = Confidence;
            writer.Evidence.Init(Evidence, (_s1, _v1) => _v1?.serialize(_s1));
            writer.MatchId.Init(MatchId);
            writer.GameId.Init(GameId);
            writer.DetectedAt = DetectedAt;
            writer.FirstOccurrence = FirstOccurrence;
            writer.LastOccurrence = LastOccurrence;
            writer.Status = Status;
            writer.AssignedTo.Init(AssignedTo);
            Resolution?.serialize(writer.Resolution);
            writer.ImpactScore = ImpactScore;
            writer.PatternId.Init(PatternId);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> IncidentId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PlayerId
        {
            get;
            set;
        }

        public CapnpGen.FraudCategory Category
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public CapnpGen.SeverityLevel Severity
        {
            get;
            set;
        }

        public float Confidence
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Evidence> Evidence
        {
            get;
            set;
        }

        public IReadOnlyList<byte> MatchId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> GameId
        {
            get;
            set;
        }

        public ulong DetectedAt
        {
            get;
            set;
        }

        public ulong FirstOccurrence
        {
            get;
            set;
        }

        public ulong LastOccurrence
        {
            get;
            set;
        }

        public CapnpGen.IncidentStatus Status
        {
            get;
            set;
        }

        public IReadOnlyList<byte> AssignedTo
        {
            get;
            set;
        }

        public CapnpGen.Resolution Resolution
        {
            get;
            set;
        }

        public float ImpactScore
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PatternId
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
            public IReadOnlyList<byte> IncidentId => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> PlayerId => ctx.ReadList(1).CastByte();
            public CapnpGen.FraudCategory Category => (CapnpGen.FraudCategory)ctx.ReadDataUShort(0UL, (ushort)0);
            public string Description => ctx.ReadText(2, null);
            public CapnpGen.SeverityLevel Severity => (CapnpGen.SeverityLevel)ctx.ReadDataUShort(16UL, (ushort)0);
            public float Confidence => ctx.ReadDataFloat(32UL, 0F);
            public IReadOnlyList<CapnpGen.Evidence.READER> Evidence => ctx.ReadList(3).Cast(CapnpGen.Evidence.READER.create);
            public IReadOnlyList<byte> MatchId => ctx.ReadList(4).CastByte();
            public IReadOnlyList<byte> GameId => ctx.ReadList(5).CastByte();
            public ulong DetectedAt => ctx.ReadDataULong(64UL, 0UL);
            public ulong FirstOccurrence => ctx.ReadDataULong(128UL, 0UL);
            public ulong LastOccurrence => ctx.ReadDataULong(192UL, 0UL);
            public CapnpGen.IncidentStatus Status => (CapnpGen.IncidentStatus)ctx.ReadDataUShort(256UL, (ushort)0);
            public IReadOnlyList<byte> AssignedTo => ctx.ReadList(6).CastByte();
            public CapnpGen.Resolution.READER Resolution => ctx.ReadStruct(7, CapnpGen.Resolution.READER.create);
            public float ImpactScore => ctx.ReadDataFloat(288UL, 0F);
            public IReadOnlyList<byte> PatternId => ctx.ReadList(8).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(5, 9);
            }

            public ListOfPrimitivesSerializer<byte> IncidentId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> PlayerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public CapnpGen.FraudCategory Category
            {
                get => (CapnpGen.FraudCategory)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public string Description
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }

            public CapnpGen.SeverityLevel Severity
            {
                get => (CapnpGen.SeverityLevel)this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, (ushort)value, (ushort)0);
            }

            public float Confidence
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public ListOfStructsSerializer<CapnpGen.Evidence.WRITER> Evidence
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.Evidence.WRITER>>(3);
                set => Link(3, value);
            }

            public ListOfPrimitivesSerializer<byte> MatchId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(4);
                set => Link(4, value);
            }

            public ListOfPrimitivesSerializer<byte> GameId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(5);
                set => Link(5, value);
            }

            public ulong DetectedAt
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong FirstOccurrence
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ulong LastOccurrence
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public CapnpGen.IncidentStatus Status
            {
                get => (CapnpGen.IncidentStatus)this.ReadDataUShort(256UL, (ushort)0);
                set => this.WriteData(256UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> AssignedTo
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(6);
                set => Link(6, value);
            }

            public CapnpGen.Resolution.WRITER Resolution
            {
                get => BuildPointer<CapnpGen.Resolution.WRITER>(7);
                set => Link(7, value);
            }

            public float ImpactScore
            {
                get => this.ReadDataFloat(288UL, 0F);
                set => this.WriteData(288UL, value, 0F);
            }

            public ListOfPrimitivesSerializer<byte> PatternId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(8);
                set => Link(8, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaf6f9220224fd55eUL)]
    public enum SeverityLevel : ushort
    {
        low,
        medium,
        high,
        critical
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8b42b31a7a3e0433UL)]
    public enum IncidentStatus : ushort
    {
        open,
        investigating,
        resolved,
        dismissed,
        escalated
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa995ee955c44bd2aUL)]
    public class Resolution : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa995ee955c44bd2aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Action = reader.Action;
            Duration = reader.Duration;
            Reason = reader.Reason;
            ResolvedBy = reader.ResolvedBy;
            ResolvedAt = reader.ResolvedAt;
            Notes = reader.Notes;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Action = Action;
            writer.Duration = Duration;
            writer.Reason = Reason;
            writer.ResolvedBy.Init(ResolvedBy);
            writer.ResolvedAt = ResolvedAt;
            writer.Notes = Notes;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.SecurityAction Action
        {
            get;
            set;
        }

        public ulong Duration
        {
            get;
            set;
        }

        public string Reason
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ResolvedBy
        {
            get;
            set;
        }

        public ulong ResolvedAt
        {
            get;
            set;
        }

        public string Notes
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
            public CapnpGen.SecurityAction Action => (CapnpGen.SecurityAction)ctx.ReadDataUShort(0UL, (ushort)0);
            public ulong Duration => ctx.ReadDataULong(64UL, 0UL);
            public string Reason => ctx.ReadText(0, null);
            public IReadOnlyList<byte> ResolvedBy => ctx.ReadList(1).CastByte();
            public ulong ResolvedAt => ctx.ReadDataULong(128UL, 0UL);
            public string Notes => ctx.ReadText(2, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 3);
            }

            public CapnpGen.SecurityAction Action
            {
                get => (CapnpGen.SecurityAction)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ulong Duration
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public string Reason
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ListOfPrimitivesSerializer<byte> ResolvedBy
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ulong ResolvedAt
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public string Notes
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xff2d0cd830dea5e1UL)]
    public class Evidence : ICapnpSerializable
    {
        public const UInt64 typeId = 0xff2d0cd830dea5e1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            EvidenceId = reader.EvidenceId;
            Type = reader.Type;
            Data = reader.Data;
            Hash = reader.Hash;
            SubmittedBy = reader.SubmittedBy;
            SubmittedAt = reader.SubmittedAt;
            Verified = reader.Verified;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.EvidenceId.Init(EvidenceId);
            writer.Type = Type;
            writer.Data.Init(Data);
            writer.Hash.Init(Hash);
            writer.SubmittedBy.Init(SubmittedBy);
            writer.SubmittedAt = SubmittedAt;
            writer.Verified = Verified;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> EvidenceId
        {
            get;
            set;
        }

        public CapnpGen.EvidenceType Type
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Data
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Hash
        {
            get;
            set;
        }

        public IReadOnlyList<byte> SubmittedBy
        {
            get;
            set;
        }

        public ulong SubmittedAt
        {
            get;
            set;
        }

        public bool Verified
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
            public IReadOnlyList<byte> EvidenceId => ctx.ReadList(0).CastByte();
            public CapnpGen.EvidenceType Type => (CapnpGen.EvidenceType)ctx.ReadDataUShort(0UL, (ushort)0);
            public IReadOnlyList<byte> Data => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> Hash => ctx.ReadList(2).CastByte();
            public IReadOnlyList<byte> SubmittedBy => ctx.ReadList(3).CastByte();
            public ulong SubmittedAt => ctx.ReadDataULong(64UL, 0UL);
            public bool Verified => ctx.ReadDataBool(16UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 4);
            }

            public ListOfPrimitivesSerializer<byte> EvidenceId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public CapnpGen.EvidenceType Type
            {
                get => (CapnpGen.EvidenceType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> Data
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> Hash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> SubmittedBy
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public ulong SubmittedAt
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public bool Verified
            {
                get => this.ReadDataBool(16UL, false);
                set => this.WriteData(16UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd1e7e014c89cbf64UL)]
    public enum EvidenceType : ushort
    {
        screenshot,
        video,
        logFile,
        replay,
        chatLog,
        performanceData,
        networkCapture,
        memoryDump
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb3c4429919daa3a5UL)]
    public enum SecurityAction : ushort
    {
        warning,
        tempBan,
        permBan,
        rankReset,
        rewardRemoval,
        itemRemoval,
        queueRestriction,
        chatRestriction,
        accountVerification,
        monitoring,
        noAction
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd8e8e21ca4ee9a46UL)]
    public class FraudPattern : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd8e8e21ca4ee9a46UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PatternId = reader.PatternId;
            Name = reader.Name;
            Description = reader.Description;
            DetectionRules = reader.DetectionRules;
            Indicators = reader.Indicators?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.FraudIndicator>(_));
            Threshold = reader.Threshold;
            DetectionCount = reader.DetectionCount;
            Accuracy = reader.Accuracy;
            FalsePositiveRate = reader.FalsePositiveRate;
            CreatedAt = reader.CreatedAt;
            LastUpdated = reader.LastUpdated;
            IsActive = reader.IsActive;
            Version = reader.Version;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PatternId.Init(PatternId);
            writer.Name = Name;
            writer.Description = Description;
            writer.DetectionRules.Init(DetectionRules);
            writer.Indicators.Init(Indicators, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Threshold = Threshold;
            writer.DetectionCount = DetectionCount;
            writer.Accuracy = Accuracy;
            writer.FalsePositiveRate = FalsePositiveRate;
            writer.CreatedAt = CreatedAt;
            writer.LastUpdated = LastUpdated;
            writer.IsActive = IsActive;
            writer.Version = Version;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> PatternId
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

        public IReadOnlyList<byte> DetectionRules
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.FraudIndicator> Indicators
        {
            get;
            set;
        }

        public float Threshold
        {
            get;
            set;
        }

        public ulong DetectionCount
        {
            get;
            set;
        }

        public float Accuracy
        {
            get;
            set;
        }

        public float FalsePositiveRate
        {
            get;
            set;
        }

        public ulong CreatedAt
        {
            get;
            set;
        }

        public ulong LastUpdated
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public string Version
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
            public IReadOnlyList<byte> PatternId => ctx.ReadList(0).CastByte();
            public string Name => ctx.ReadText(1, null);
            public string Description => ctx.ReadText(2, null);
            public IReadOnlyList<byte> DetectionRules => ctx.ReadList(3).CastByte();
            public IReadOnlyList<CapnpGen.FraudIndicator.READER> Indicators => ctx.ReadList(4).Cast(CapnpGen.FraudIndicator.READER.create);
            public float Threshold => ctx.ReadDataFloat(0UL, 0F);
            public ulong DetectionCount => ctx.ReadDataULong(64UL, 0UL);
            public float Accuracy => ctx.ReadDataFloat(32UL, 0F);
            public float FalsePositiveRate => ctx.ReadDataFloat(128UL, 0F);
            public ulong CreatedAt => ctx.ReadDataULong(192UL, 0UL);
            public ulong LastUpdated => ctx.ReadDataULong(256UL, 0UL);
            public bool IsActive => ctx.ReadDataBool(160UL, false);
            public string Version => ctx.ReadText(5, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(5, 6);
            }

            public ListOfPrimitivesSerializer<byte> PatternId
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

            public ListOfPrimitivesSerializer<byte> DetectionRules
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public ListOfStructsSerializer<CapnpGen.FraudIndicator.WRITER> Indicators
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.FraudIndicator.WRITER>>(4);
                set => Link(4, value);
            }

            public float Threshold
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public ulong DetectionCount
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public float Accuracy
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float FalsePositiveRate
            {
                get => this.ReadDataFloat(128UL, 0F);
                set => this.WriteData(128UL, value, 0F);
            }

            public ulong CreatedAt
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public ulong LastUpdated
            {
                get => this.ReadDataULong(256UL, 0UL);
                set => this.WriteData(256UL, value, 0UL);
            }

            public bool IsActive
            {
                get => this.ReadDataBool(160UL, false);
                set => this.WriteData(160UL, value, false);
            }

            public string Version
            {
                get => this.ReadText(5, null);
                set => this.WriteText(5, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa251190a0652fa19UL)]
    public class FraudIndicator : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa251190a0652fa19UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            IndicatorId = reader.IndicatorId;
            Name = reader.Name;
            Weight = reader.Weight;
            DataSource = reader.DataSource;
            Calculation = reader.Calculation;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.IndicatorId = IndicatorId;
            writer.Name = Name;
            writer.Weight = Weight;
            writer.DataSource = DataSource;
            writer.Calculation.Init(Calculation);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string IndicatorId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public float Weight
        {
            get;
            set;
        }

        public CapnpGen.DataSource DataSource
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Calculation
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
            public string IndicatorId => ctx.ReadText(0, null);
            public string Name => ctx.ReadText(1, null);
            public float Weight => ctx.ReadDataFloat(0UL, 0F);
            public CapnpGen.DataSource DataSource => (CapnpGen.DataSource)ctx.ReadDataUShort(32UL, (ushort)0);
            public IReadOnlyList<byte> Calculation => ctx.ReadList(2).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public string IndicatorId
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public float Weight
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public CapnpGen.DataSource DataSource
            {
                get => (CapnpGen.DataSource)this.ReadDataUShort(32UL, (ushort)0);
                set => this.WriteData(32UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> Calculation
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe7c72e28b6fec955UL)]
    public enum DataSource : ushort
    {
        gameplay,
        network,
        system,
        behavioral,
        economic,
        social
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x85bb9813e60d4416UL)]
    public class BehaviorProfile : ICapnpSerializable
    {
        public const UInt64 typeId = 0x85bb9813e60d4416UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            PlayPatterns = CapnpSerializable.Create<CapnpGen.PlayPatterns>(reader.PlayPatterns);
            SocialBehavior = CapnpSerializable.Create<CapnpGen.SocialBehavior>(reader.SocialBehavior);
            EconomicBehavior = CapnpSerializable.Create<CapnpGen.EconomicBehavior>(reader.EconomicBehavior);
            RiskScore = reader.RiskScore;
            RiskFactors = reader.RiskFactors?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.RiskFactor>(_));
            Confidence = reader.Confidence;
            Incidents = reader.Incidents?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.IncidentHistory>(_));
            Warnings = reader.Warnings;
            Suspensions = reader.Suspensions;
            CreatedAt = reader.CreatedAt;
            LastUpdated = reader.LastUpdated;
            MonitoringLevel = reader.MonitoringLevel;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId.Init(PlayerId);
            PlayPatterns?.serialize(writer.PlayPatterns);
            SocialBehavior?.serialize(writer.SocialBehavior);
            EconomicBehavior?.serialize(writer.EconomicBehavior);
            writer.RiskScore = RiskScore;
            writer.RiskFactors.Init(RiskFactors, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Confidence = Confidence;
            writer.Incidents.Init(Incidents, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Warnings = Warnings;
            writer.Suspensions = Suspensions;
            writer.CreatedAt = CreatedAt;
            writer.LastUpdated = LastUpdated;
            writer.MonitoringLevel = MonitoringLevel;
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

        public CapnpGen.PlayPatterns PlayPatterns
        {
            get;
            set;
        }

        public CapnpGen.SocialBehavior SocialBehavior
        {
            get;
            set;
        }

        public CapnpGen.EconomicBehavior EconomicBehavior
        {
            get;
            set;
        }

        public float RiskScore
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.RiskFactor> RiskFactors
        {
            get;
            set;
        }

        public float Confidence
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.IncidentHistory> Incidents
        {
            get;
            set;
        }

        public uint Warnings
        {
            get;
            set;
        }

        public uint Suspensions
        {
            get;
            set;
        }

        public ulong CreatedAt
        {
            get;
            set;
        }

        public ulong LastUpdated
        {
            get;
            set;
        }

        public CapnpGen.MonitoringLevel MonitoringLevel
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
            public CapnpGen.PlayPatterns.READER PlayPatterns => ctx.ReadStruct(1, CapnpGen.PlayPatterns.READER.create);
            public CapnpGen.SocialBehavior.READER SocialBehavior => ctx.ReadStruct(2, CapnpGen.SocialBehavior.READER.create);
            public CapnpGen.EconomicBehavior.READER EconomicBehavior => ctx.ReadStruct(3, CapnpGen.EconomicBehavior.READER.create);
            public float RiskScore => ctx.ReadDataFloat(0UL, 0F);
            public IReadOnlyList<CapnpGen.RiskFactor.READER> RiskFactors => ctx.ReadList(4).Cast(CapnpGen.RiskFactor.READER.create);
            public float Confidence => ctx.ReadDataFloat(32UL, 0F);
            public IReadOnlyList<CapnpGen.IncidentHistory.READER> Incidents => ctx.ReadList(5).Cast(CapnpGen.IncidentHistory.READER.create);
            public uint Warnings => ctx.ReadDataUInt(64UL, 0U);
            public uint Suspensions => ctx.ReadDataUInt(96UL, 0U);
            public ulong CreatedAt => ctx.ReadDataULong(128UL, 0UL);
            public ulong LastUpdated => ctx.ReadDataULong(192UL, 0UL);
            public CapnpGen.MonitoringLevel MonitoringLevel => (CapnpGen.MonitoringLevel)ctx.ReadDataUShort(256UL, (ushort)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(5, 6);
            }

            public ListOfPrimitivesSerializer<byte> PlayerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public CapnpGen.PlayPatterns.WRITER PlayPatterns
            {
                get => BuildPointer<CapnpGen.PlayPatterns.WRITER>(1);
                set => Link(1, value);
            }

            public CapnpGen.SocialBehavior.WRITER SocialBehavior
            {
                get => BuildPointer<CapnpGen.SocialBehavior.WRITER>(2);
                set => Link(2, value);
            }

            public CapnpGen.EconomicBehavior.WRITER EconomicBehavior
            {
                get => BuildPointer<CapnpGen.EconomicBehavior.WRITER>(3);
                set => Link(3, value);
            }

            public float RiskScore
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public ListOfStructsSerializer<CapnpGen.RiskFactor.WRITER> RiskFactors
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.RiskFactor.WRITER>>(4);
                set => Link(4, value);
            }

            public float Confidence
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public ListOfStructsSerializer<CapnpGen.IncidentHistory.WRITER> Incidents
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.IncidentHistory.WRITER>>(5);
                set => Link(5, value);
            }

            public uint Warnings
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }

            public uint Suspensions
            {
                get => this.ReadDataUInt(96UL, 0U);
                set => this.WriteData(96UL, value, 0U);
            }

            public ulong CreatedAt
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ulong LastUpdated
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public CapnpGen.MonitoringLevel MonitoringLevel
            {
                get => (CapnpGen.MonitoringLevel)this.ReadDataUShort(256UL, (ushort)0);
                set => this.WriteData(256UL, (ushort)value, (ushort)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8af7010bc20e8bc1UL)]
    public class PlayPatterns : ICapnpSerializable
    {
        public const UInt64 typeId = 0x8af7010bc20e8bc1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SessionLengthStats = CapnpSerializable.Create<CapnpGen.SessionStats>(reader.SessionLengthStats);
            TimeOfDayPattern = CapnpSerializable.Create<CapnpGen.TimePattern>(reader.TimeOfDayPattern);
            PlayFrequency = CapnpSerializable.Create<CapnpGen.FrequencyStats>(reader.PlayFrequency);
            PerformanceVariance = reader.PerformanceVariance;
            DisconnectRate = reader.DisconnectRate;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            SessionLengthStats?.serialize(writer.SessionLengthStats);
            TimeOfDayPattern?.serialize(writer.TimeOfDayPattern);
            PlayFrequency?.serialize(writer.PlayFrequency);
            writer.PerformanceVariance = PerformanceVariance;
            writer.DisconnectRate = DisconnectRate;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.SessionStats SessionLengthStats
        {
            get;
            set;
        }

        public CapnpGen.TimePattern TimeOfDayPattern
        {
            get;
            set;
        }

        public CapnpGen.FrequencyStats PlayFrequency
        {
            get;
            set;
        }

        public float PerformanceVariance
        {
            get;
            set;
        }

        public float DisconnectRate
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
            public CapnpGen.SessionStats.READER SessionLengthStats => ctx.ReadStruct(0, CapnpGen.SessionStats.READER.create);
            public CapnpGen.TimePattern.READER TimeOfDayPattern => ctx.ReadStruct(1, CapnpGen.TimePattern.READER.create);
            public CapnpGen.FrequencyStats.READER PlayFrequency => ctx.ReadStruct(2, CapnpGen.FrequencyStats.READER.create);
            public float PerformanceVariance => ctx.ReadDataFloat(0UL, 0F);
            public float DisconnectRate => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public CapnpGen.SessionStats.WRITER SessionLengthStats
            {
                get => BuildPointer<CapnpGen.SessionStats.WRITER>(0);
                set => Link(0, value);
            }

            public CapnpGen.TimePattern.WRITER TimeOfDayPattern
            {
                get => BuildPointer<CapnpGen.TimePattern.WRITER>(1);
                set => Link(1, value);
            }

            public CapnpGen.FrequencyStats.WRITER PlayFrequency
            {
                get => BuildPointer<CapnpGen.FrequencyStats.WRITER>(2);
                set => Link(2, value);
            }

            public float PerformanceVariance
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float DisconnectRate
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd7aa3b36c8fa17fdUL)]
    public class SessionStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd7aa3b36c8fa17fdUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            AvgDurationMs = reader.AvgDurationMs;
            StdDevDuration = reader.StdDevDuration;
            TotalSessions = reader.TotalSessions;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.AvgDurationMs = AvgDurationMs;
            writer.StdDevDuration = StdDevDuration;
            writer.TotalSessions = TotalSessions;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong AvgDurationMs
        {
            get;
            set;
        }

        public ulong StdDevDuration
        {
            get;
            set;
        }

        public ulong TotalSessions
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
            public ulong AvgDurationMs => ctx.ReadDataULong(0UL, 0UL);
            public ulong StdDevDuration => ctx.ReadDataULong(64UL, 0UL);
            public ulong TotalSessions => ctx.ReadDataULong(128UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 0);
            }

            public ulong AvgDurationMs
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ulong StdDevDuration
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong TotalSessions
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf70a87fcb396a6feUL)]
    public class TimePattern : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf70a87fcb396a6feUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            UsualStartHour = reader.UsualStartHour;
            UsualEndHour = reader.UsualEndHour;
            Variance = reader.Variance;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.UsualStartHour = UsualStartHour;
            writer.UsualEndHour = UsualEndHour;
            writer.Variance = Variance;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public byte UsualStartHour
        {
            get;
            set;
        }

        public byte UsualEndHour
        {
            get;
            set;
        }

        public float Variance
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
            public byte UsualStartHour => ctx.ReadDataByte(0UL, (byte)0);
            public byte UsualEndHour => ctx.ReadDataByte(8UL, (byte)0);
            public float Variance => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public byte UsualStartHour
            {
                get => this.ReadDataByte(0UL, (byte)0);
                set => this.WriteData(0UL, value, (byte)0);
            }

            public byte UsualEndHour
            {
                get => this.ReadDataByte(8UL, (byte)0);
                set => this.WriteData(8UL, value, (byte)0);
            }

            public float Variance
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xde8aa465c45b2bc9UL)]
    public class FrequencyStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0xde8aa465c45b2bc9UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchesPerDay = reader.MatchesPerDay;
            DaysSinceLast = reader.DaysSinceLast;
            Consistency = reader.Consistency;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchesPerDay = MatchesPerDay;
            writer.DaysSinceLast = DaysSinceLast;
            writer.Consistency = Consistency;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float MatchesPerDay
        {
            get;
            set;
        }

        public uint DaysSinceLast
        {
            get;
            set;
        }

        public float Consistency
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
            public float MatchesPerDay => ctx.ReadDataFloat(0UL, 0F);
            public uint DaysSinceLast => ctx.ReadDataUInt(32UL, 0U);
            public float Consistency => ctx.ReadDataFloat(64UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public float MatchesPerDay
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public uint DaysSinceLast
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public float Consistency
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe96b15758715ccf3UL)]
    public class SocialBehavior : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe96b15758715ccf3UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ChatFrequency = reader.ChatFrequency;
            ReportCount = reader.ReportCount;
            WasReportedCount = reader.WasReportedCount;
            FriendCount = reader.FriendCount;
            BlockedCount = reader.BlockedCount;
            ToxicityScore = reader.ToxicityScore;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ChatFrequency = ChatFrequency;
            writer.ReportCount = ReportCount;
            writer.WasReportedCount = WasReportedCount;
            writer.FriendCount = FriendCount;
            writer.BlockedCount = BlockedCount;
            writer.ToxicityScore = ToxicityScore;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float ChatFrequency
        {
            get;
            set;
        }

        public uint ReportCount
        {
            get;
            set;
        }

        public uint WasReportedCount
        {
            get;
            set;
        }

        public uint FriendCount
        {
            get;
            set;
        }

        public uint BlockedCount
        {
            get;
            set;
        }

        public float ToxicityScore
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
            public float ChatFrequency => ctx.ReadDataFloat(0UL, 0F);
            public uint ReportCount => ctx.ReadDataUInt(32UL, 0U);
            public uint WasReportedCount => ctx.ReadDataUInt(64UL, 0U);
            public uint FriendCount => ctx.ReadDataUInt(96UL, 0U);
            public uint BlockedCount => ctx.ReadDataUInt(128UL, 0U);
            public float ToxicityScore => ctx.ReadDataFloat(160UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 0);
            }

            public float ChatFrequency
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public uint ReportCount
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public uint WasReportedCount
            {
                get => this.ReadDataUInt(64UL, 0U);
                set => this.WriteData(64UL, value, 0U);
            }

            public uint FriendCount
            {
                get => this.ReadDataUInt(96UL, 0U);
                set => this.WriteData(96UL, value, 0U);
            }

            public uint BlockedCount
            {
                get => this.ReadDataUInt(128UL, 0U);
                set => this.WriteData(128UL, value, 0U);
            }

            public float ToxicityScore
            {
                get => this.ReadDataFloat(160UL, 0F);
                set => this.WriteData(160UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8374b80bd0977526UL)]
    public class EconomicBehavior : ICapnpSerializable
    {
        public const UInt64 typeId = 0x8374b80bd0977526UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TotalSpent = reader.TotalSpent;
            TransactionFrequency = reader.TransactionFrequency;
            ChargebackRate = reader.ChargebackRate;
            TradingActivity = CapnpSerializable.Create<CapnpGen.TradingStats>(reader.TradingActivity);
            CurrencyFlow = CapnpSerializable.Create<CapnpGen.CurrencyFlow>(reader.CurrencyFlow);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TotalSpent = TotalSpent;
            writer.TransactionFrequency = TransactionFrequency;
            writer.ChargebackRate = ChargebackRate;
            TradingActivity?.serialize(writer.TradingActivity);
            CurrencyFlow?.serialize(writer.CurrencyFlow);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong TotalSpent
        {
            get;
            set;
        }

        public float TransactionFrequency
        {
            get;
            set;
        }

        public float ChargebackRate
        {
            get;
            set;
        }

        public CapnpGen.TradingStats TradingActivity
        {
            get;
            set;
        }

        public CapnpGen.CurrencyFlow CurrencyFlow
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
            public ulong TotalSpent => ctx.ReadDataULong(0UL, 0UL);
            public float TransactionFrequency => ctx.ReadDataFloat(64UL, 0F);
            public float ChargebackRate => ctx.ReadDataFloat(96UL, 0F);
            public CapnpGen.TradingStats.READER TradingActivity => ctx.ReadStruct(0, CapnpGen.TradingStats.READER.create);
            public CapnpGen.CurrencyFlow.READER CurrencyFlow => ctx.ReadStruct(1, CapnpGen.CurrencyFlow.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 2);
            }

            public ulong TotalSpent
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public float TransactionFrequency
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public float ChargebackRate
            {
                get => this.ReadDataFloat(96UL, 0F);
                set => this.WriteData(96UL, value, 0F);
            }

            public CapnpGen.TradingStats.WRITER TradingActivity
            {
                get => BuildPointer<CapnpGen.TradingStats.WRITER>(0);
                set => Link(0, value);
            }

            public CapnpGen.CurrencyFlow.WRITER CurrencyFlow
            {
                get => BuildPointer<CapnpGen.CurrencyFlow.WRITER>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf11fcf13a26bdc6cUL)]
    public class TradingStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf11fcf13a26bdc6cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TotalTrades = reader.TotalTrades;
            SuspiciousTrades = reader.SuspiciousTrades;
            AvgTradeValue = reader.AvgTradeValue;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TotalTrades = TotalTrades;
            writer.SuspiciousTrades = SuspiciousTrades;
            writer.AvgTradeValue = AvgTradeValue;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint TotalTrades
        {
            get;
            set;
        }

        public uint SuspiciousTrades
        {
            get;
            set;
        }

        public ulong AvgTradeValue
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
            public uint TotalTrades => ctx.ReadDataUInt(0UL, 0U);
            public uint SuspiciousTrades => ctx.ReadDataUInt(32UL, 0U);
            public ulong AvgTradeValue => ctx.ReadDataULong(64UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public uint TotalTrades
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public uint SuspiciousTrades
            {
                get => this.ReadDataUInt(32UL, 0U);
                set => this.WriteData(32UL, value, 0U);
            }

            public ulong AvgTradeValue
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc9d6a1fbd8b2c245UL)]
    public class CurrencyFlow : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc9d6a1fbd8b2c245UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            InFlow = reader.InFlow;
            OutFlow = reader.OutFlow;
            NetFlow = reader.NetFlow;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.InFlow = InFlow;
            writer.OutFlow = OutFlow;
            writer.NetFlow = NetFlow;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong InFlow
        {
            get;
            set;
        }

        public ulong OutFlow
        {
            get;
            set;
        }

        public long NetFlow
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
            public ulong InFlow => ctx.ReadDataULong(0UL, 0UL);
            public ulong OutFlow => ctx.ReadDataULong(64UL, 0UL);
            public long NetFlow => ctx.ReadDataLong(128UL, 0L);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 0);
            }

            public ulong InFlow
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ulong OutFlow
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public long NetFlow
            {
                get => this.ReadDataLong(128UL, 0L);
                set => this.WriteData(128UL, value, 0L);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc07d7e381209e8c2UL)]
    public class RiskFactor : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc07d7e381209e8c2UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Factor = reader.Factor;
            Score = reader.Score;
            Evidence = reader.Evidence;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Factor = Factor;
            writer.Score = Score;
            writer.Evidence.Init(Evidence);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string Factor
        {
            get;
            set;
        }

        public float Score
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
            public string Factor => ctx.ReadText(0, null);
            public float Score => ctx.ReadDataFloat(0UL, 0F);
            public IReadOnlyList<byte> Evidence => ctx.ReadList(1).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public string Factor
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public float Score
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public ListOfPrimitivesSerializer<byte> Evidence
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xce1be80422bb552aUL)]
    public class IncidentHistory : ICapnpSerializable
    {
        public const UInt64 typeId = 0xce1be80422bb552aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            IncidentId = reader.IncidentId;
            Category = reader.Category;
            Severity = reader.Severity;
            ResolvedAt = reader.ResolvedAt;
            ActionTaken = reader.ActionTaken;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.IncidentId.Init(IncidentId);
            writer.Category = Category;
            writer.Severity = Severity;
            writer.ResolvedAt = ResolvedAt;
            writer.ActionTaken = ActionTaken;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> IncidentId
        {
            get;
            set;
        }

        public CapnpGen.FraudCategory Category
        {
            get;
            set;
        }

        public CapnpGen.SeverityLevel Severity
        {
            get;
            set;
        }

        public ulong ResolvedAt
        {
            get;
            set;
        }

        public CapnpGen.SecurityAction ActionTaken
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
            public IReadOnlyList<byte> IncidentId => ctx.ReadList(0).CastByte();
            public CapnpGen.FraudCategory Category => (CapnpGen.FraudCategory)ctx.ReadDataUShort(0UL, (ushort)0);
            public CapnpGen.SeverityLevel Severity => (CapnpGen.SeverityLevel)ctx.ReadDataUShort(16UL, (ushort)0);
            public ulong ResolvedAt => ctx.ReadDataULong(64UL, 0UL);
            public CapnpGen.SecurityAction ActionTaken => (CapnpGen.SecurityAction)ctx.ReadDataUShort(32UL, (ushort)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 1);
            }

            public ListOfPrimitivesSerializer<byte> IncidentId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public CapnpGen.FraudCategory Category
            {
                get => (CapnpGen.FraudCategory)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.SeverityLevel Severity
            {
                get => (CapnpGen.SeverityLevel)this.ReadDataUShort(16UL, (ushort)0);
                set => this.WriteData(16UL, (ushort)value, (ushort)0);
            }

            public ulong ResolvedAt
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public CapnpGen.SecurityAction ActionTaken
            {
                get => (CapnpGen.SecurityAction)this.ReadDataUShort(32UL, (ushort)0);
                set => this.WriteData(32UL, (ushort)value, (ushort)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc3f41356a435d0cfUL)]
    public enum MonitoringLevel : ushort
    {
        normal,
        elevated,
        high,
        maximum
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9dcf8caebb2c455cUL)]
    public class AntiCheatReport : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9dcf8caebb2c455cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ReportId = reader.ReportId;
            PlayerId = reader.PlayerId;
            MatchId = reader.MatchId;
            CheatSignatures = reader.CheatSignatures?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.CheatSignature>(_));
            SystemChecks = CapnpSerializable.Create<CapnpGen.SystemCheckResults>(reader.SystemChecks);
            BehaviorAnomalies = reader.BehaviorAnomalies?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.BehaviorAnomaly>(_));
            DetectionConfidence = reader.DetectionConfidence;
            CheatType = reader.CheatType;
            ClientVersion = reader.ClientVersion;
            HardwareHash = reader.HardwareHash;
            OsInfo = reader.OsInfo;
            Status = reader.Status;
            ReviewedBy = reader.ReviewedBy;
            ReviewNotes = reader.ReviewNotes;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ReportId.Init(ReportId);
            writer.PlayerId.Init(PlayerId);
            writer.MatchId.Init(MatchId);
            writer.CheatSignatures.Init(CheatSignatures, (_s1, _v1) => _v1?.serialize(_s1));
            SystemChecks?.serialize(writer.SystemChecks);
            writer.BehaviorAnomalies.Init(BehaviorAnomalies, (_s1, _v1) => _v1?.serialize(_s1));
            writer.DetectionConfidence = DetectionConfidence;
            writer.CheatType = CheatType;
            writer.ClientVersion = ClientVersion;
            writer.HardwareHash.Init(HardwareHash);
            writer.OsInfo = OsInfo;
            writer.Status = Status;
            writer.ReviewedBy.Init(ReviewedBy);
            writer.ReviewNotes = ReviewNotes;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> ReportId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> PlayerId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> MatchId
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.CheatSignature> CheatSignatures
        {
            get;
            set;
        }

        public CapnpGen.SystemCheckResults SystemChecks
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.BehaviorAnomaly> BehaviorAnomalies
        {
            get;
            set;
        }

        public float DetectionConfidence
        {
            get;
            set;
        }

        public CapnpGen.CheatType CheatType
        {
            get;
            set;
        }

        public string ClientVersion
        {
            get;
            set;
        }

        public IReadOnlyList<byte> HardwareHash
        {
            get;
            set;
        }

        public string OsInfo
        {
            get;
            set;
        }

        public CapnpGen.ReportStatus Status
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ReviewedBy
        {
            get;
            set;
        }

        public string ReviewNotes
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
            public IReadOnlyList<byte> ReportId => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> PlayerId => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> MatchId => ctx.ReadList(2).CastByte();
            public IReadOnlyList<CapnpGen.CheatSignature.READER> CheatSignatures => ctx.ReadList(3).Cast(CapnpGen.CheatSignature.READER.create);
            public CapnpGen.SystemCheckResults.READER SystemChecks => ctx.ReadStruct(4, CapnpGen.SystemCheckResults.READER.create);
            public IReadOnlyList<CapnpGen.BehaviorAnomaly.READER> BehaviorAnomalies => ctx.ReadList(5).Cast(CapnpGen.BehaviorAnomaly.READER.create);
            public float DetectionConfidence => ctx.ReadDataFloat(0UL, 0F);
            public CapnpGen.CheatType CheatType => (CapnpGen.CheatType)ctx.ReadDataUShort(32UL, (ushort)0);
            public string ClientVersion => ctx.ReadText(6, null);
            public IReadOnlyList<byte> HardwareHash => ctx.ReadList(7).CastByte();
            public string OsInfo => ctx.ReadText(8, null);
            public CapnpGen.ReportStatus Status => (CapnpGen.ReportStatus)ctx.ReadDataUShort(48UL, (ushort)0);
            public IReadOnlyList<byte> ReviewedBy => ctx.ReadList(9).CastByte();
            public string ReviewNotes => ctx.ReadText(10, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 11);
            }

            public ListOfPrimitivesSerializer<byte> ReportId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> PlayerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> MatchId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfStructsSerializer<CapnpGen.CheatSignature.WRITER> CheatSignatures
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.CheatSignature.WRITER>>(3);
                set => Link(3, value);
            }

            public CapnpGen.SystemCheckResults.WRITER SystemChecks
            {
                get => BuildPointer<CapnpGen.SystemCheckResults.WRITER>(4);
                set => Link(4, value);
            }

            public ListOfStructsSerializer<CapnpGen.BehaviorAnomaly.WRITER> BehaviorAnomalies
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.BehaviorAnomaly.WRITER>>(5);
                set => Link(5, value);
            }

            public float DetectionConfidence
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public CapnpGen.CheatType CheatType
            {
                get => (CapnpGen.CheatType)this.ReadDataUShort(32UL, (ushort)0);
                set => this.WriteData(32UL, (ushort)value, (ushort)0);
            }

            public string ClientVersion
            {
                get => this.ReadText(6, null);
                set => this.WriteText(6, value, null);
            }

            public ListOfPrimitivesSerializer<byte> HardwareHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(7);
                set => Link(7, value);
            }

            public string OsInfo
            {
                get => this.ReadText(8, null);
                set => this.WriteText(8, value, null);
            }

            public CapnpGen.ReportStatus Status
            {
                get => (CapnpGen.ReportStatus)this.ReadDataUShort(48UL, (ushort)0);
                set => this.WriteData(48UL, (ushort)value, (ushort)0);
            }

            public ListOfPrimitivesSerializer<byte> ReviewedBy
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(9);
                set => Link(9, value);
            }

            public string ReviewNotes
            {
                get => this.ReadText(10, null);
                set => this.WriteText(10, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb4965abca728b73bUL)]
    public class CheatSignature : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb4965abca728b73bUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SignatureId = reader.SignatureId;
            Name = reader.Name;
            MatchConfidence = reader.MatchConfidence;
            DetectedAt = reader.DetectedAt;
            Details = reader.Details;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.SignatureId = SignatureId;
            writer.Name = Name;
            writer.MatchConfidence = MatchConfidence;
            writer.DetectedAt = DetectedAt;
            writer.Details.Init(Details);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string SignatureId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public float MatchConfidence
        {
            get;
            set;
        }

        public ulong DetectedAt
        {
            get;
            set;
        }

        public IReadOnlyList<byte> Details
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
            public string SignatureId => ctx.ReadText(0, null);
            public string Name => ctx.ReadText(1, null);
            public float MatchConfidence => ctx.ReadDataFloat(0UL, 0F);
            public ulong DetectedAt => ctx.ReadDataULong(64UL, 0UL);
            public IReadOnlyList<byte> Details => ctx.ReadList(2).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 3);
            }

            public string SignatureId
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public string Name
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public float MatchConfidence
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public ulong DetectedAt
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ListOfPrimitivesSerializer<byte> Details
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc5a3cbcdd29d6a6eUL)]
    public class SystemCheckResults : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc5a3cbcdd29d6a6eUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MemoryScan = CapnpSerializable.Create<CapnpGen.MemoryScanResult>(reader.MemoryScan);
            ProcessScan = CapnpSerializable.Create<CapnpGen.ProcessScanResult>(reader.ProcessScan);
            FileScan = CapnpSerializable.Create<CapnpGen.FileScanResult>(reader.FileScan);
            DriverScan = CapnpSerializable.Create<CapnpGen.DriverScanResult>(reader.DriverScan);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            MemoryScan?.serialize(writer.MemoryScan);
            ProcessScan?.serialize(writer.ProcessScan);
            FileScan?.serialize(writer.FileScan);
            DriverScan?.serialize(writer.DriverScan);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.MemoryScanResult MemoryScan
        {
            get;
            set;
        }

        public CapnpGen.ProcessScanResult ProcessScan
        {
            get;
            set;
        }

        public CapnpGen.FileScanResult FileScan
        {
            get;
            set;
        }

        public CapnpGen.DriverScanResult DriverScan
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
            public CapnpGen.MemoryScanResult.READER MemoryScan => ctx.ReadStruct(0, CapnpGen.MemoryScanResult.READER.create);
            public CapnpGen.ProcessScanResult.READER ProcessScan => ctx.ReadStruct(1, CapnpGen.ProcessScanResult.READER.create);
            public CapnpGen.FileScanResult.READER FileScan => ctx.ReadStruct(2, CapnpGen.FileScanResult.READER.create);
            public CapnpGen.DriverScanResult.READER DriverScan => ctx.ReadStruct(3, CapnpGen.DriverScanResult.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 4);
            }

            public CapnpGen.MemoryScanResult.WRITER MemoryScan
            {
                get => BuildPointer<CapnpGen.MemoryScanResult.WRITER>(0);
                set => Link(0, value);
            }

            public CapnpGen.ProcessScanResult.WRITER ProcessScan
            {
                get => BuildPointer<CapnpGen.ProcessScanResult.WRITER>(1);
                set => Link(1, value);
            }

            public CapnpGen.FileScanResult.WRITER FileScan
            {
                get => BuildPointer<CapnpGen.FileScanResult.WRITER>(2);
                set => Link(2, value);
            }

            public CapnpGen.DriverScanResult.WRITER DriverScan
            {
                get => BuildPointer<CapnpGen.DriverScanResult.WRITER>(3);
                set => Link(3, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd48f23aa10385437UL)]
    public class MemoryScanResult : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd48f23aa10385437UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SuspiciousRegions = reader.SuspiciousRegions;
            InjectedCode = reader.InjectedCode;
            HooksDetected = reader.HooksDetected;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.SuspiciousRegions = SuspiciousRegions;
            writer.InjectedCode = InjectedCode;
            writer.HooksDetected = HooksDetected;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint SuspiciousRegions
        {
            get;
            set;
        }

        public bool InjectedCode
        {
            get;
            set;
        }

        public bool HooksDetected
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
            public uint SuspiciousRegions => ctx.ReadDataUInt(0UL, 0U);
            public bool InjectedCode => ctx.ReadDataBool(32UL, false);
            public bool HooksDetected => ctx.ReadDataBool(33UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public uint SuspiciousRegions
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public bool InjectedCode
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }

            public bool HooksDetected
            {
                get => this.ReadDataBool(33UL, false);
                set => this.WriteData(33UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbdb463b97ff50833UL)]
    public class ProcessScanResult : ICapnpSerializable
    {
        public const UInt64 typeId = 0xbdb463b97ff50833UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SuspiciousProcesses = reader.SuspiciousProcesses?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.SuspiciousProcess>(_));
            UnauthorizedInjects = reader.UnauthorizedInjects;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.SuspiciousProcesses.Init(SuspiciousProcesses, (_s1, _v1) => _v1?.serialize(_s1));
            writer.UnauthorizedInjects = UnauthorizedInjects;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<CapnpGen.SuspiciousProcess> SuspiciousProcesses
        {
            get;
            set;
        }

        public uint UnauthorizedInjects
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
            public IReadOnlyList<CapnpGen.SuspiciousProcess.READER> SuspiciousProcesses => ctx.ReadList(0).Cast(CapnpGen.SuspiciousProcess.READER.create);
            public uint UnauthorizedInjects => ctx.ReadDataUInt(0UL, 0U);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public ListOfStructsSerializer<CapnpGen.SuspiciousProcess.WRITER> SuspiciousProcesses
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.SuspiciousProcess.WRITER>>(0);
                set => Link(0, value);
            }

            public uint UnauthorizedInjects
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9ae17fb44ae6aedcUL)]
    public class SuspiciousProcess : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9ae17fb44ae6aedcUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ProcessName = reader.ProcessName;
            Confidence = reader.Confidence;
            Reason = reader.Reason;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ProcessName = ProcessName;
            writer.Confidence = Confidence;
            writer.Reason = Reason;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string ProcessName
        {
            get;
            set;
        }

        public float Confidence
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
            public string ProcessName => ctx.ReadText(0, null);
            public float Confidence => ctx.ReadDataFloat(0UL, 0F);
            public string Reason => ctx.ReadText(1, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 2);
            }

            public string ProcessName
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public float Confidence
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public string Reason
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe8d882a4c9b82111UL)]
    public class FileScanResult : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe8d882a4c9b82111UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ModifiedFiles = reader.ModifiedFiles?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.ModifiedFile>(_));
            CheatFiles = reader.CheatFiles?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.CheatFile>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ModifiedFiles.Init(ModifiedFiles, (_s1, _v1) => _v1?.serialize(_s1));
            writer.CheatFiles.Init(CheatFiles, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<CapnpGen.ModifiedFile> ModifiedFiles
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.CheatFile> CheatFiles
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
            public IReadOnlyList<CapnpGen.ModifiedFile.READER> ModifiedFiles => ctx.ReadList(0).Cast(CapnpGen.ModifiedFile.READER.create);
            public IReadOnlyList<CapnpGen.CheatFile.READER> CheatFiles => ctx.ReadList(1).Cast(CapnpGen.CheatFile.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 2);
            }

            public ListOfStructsSerializer<CapnpGen.ModifiedFile.WRITER> ModifiedFiles
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.ModifiedFile.WRITER>>(0);
                set => Link(0, value);
            }

            public ListOfStructsSerializer<CapnpGen.CheatFile.WRITER> CheatFiles
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.CheatFile.WRITER>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc50f983b1068f4b1UL)]
    public class ModifiedFile : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc50f983b1068f4b1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            FileName = reader.FileName;
            ExpectedHash = reader.ExpectedHash;
            ActualHash = reader.ActualHash;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.FileName = FileName;
            writer.ExpectedHash.Init(ExpectedHash);
            writer.ActualHash.Init(ActualHash);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string FileName
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ExpectedHash
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ActualHash
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
            public string FileName => ctx.ReadText(0, null);
            public IReadOnlyList<byte> ExpectedHash => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> ActualHash => ctx.ReadList(2).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 3);
            }

            public string FileName
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public ListOfPrimitivesSerializer<byte> ExpectedHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> ActualHash
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa2fbda3a851279d5UL)]
    public class CheatFile : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa2fbda3a851279d5UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            FileName = reader.FileName;
            CheatType = reader.CheatType;
            Confidence = reader.Confidence;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.FileName = FileName;
            writer.CheatType = CheatType;
            writer.Confidence = Confidence;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string FileName
        {
            get;
            set;
        }

        public CapnpGen.CheatType CheatType
        {
            get;
            set;
        }

        public float Confidence
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
            public string FileName => ctx.ReadText(0, null);
            public CapnpGen.CheatType CheatType => (CapnpGen.CheatType)ctx.ReadDataUShort(0UL, (ushort)0);
            public float Confidence => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public string FileName
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public CapnpGen.CheatType CheatType
            {
                get => (CapnpGen.CheatType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public float Confidence
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8c81319333877c9cUL)]
    public class DriverScanResult : ICapnpSerializable
    {
        public const UInt64 typeId = 0x8c81319333877c9cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            UnauthorizedDrivers = reader.UnauthorizedDrivers?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.UnauthorizedDriver>(_));
            KernelHooks = reader.KernelHooks;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.UnauthorizedDrivers.Init(UnauthorizedDrivers, (_s1, _v1) => _v1?.serialize(_s1));
            writer.KernelHooks = KernelHooks;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<CapnpGen.UnauthorizedDriver> UnauthorizedDrivers
        {
            get;
            set;
        }

        public bool KernelHooks
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
            public IReadOnlyList<CapnpGen.UnauthorizedDriver.READER> UnauthorizedDrivers => ctx.ReadList(0).Cast(CapnpGen.UnauthorizedDriver.READER.create);
            public bool KernelHooks => ctx.ReadDataBool(0UL, false);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public ListOfStructsSerializer<CapnpGen.UnauthorizedDriver.WRITER> UnauthorizedDrivers
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.UnauthorizedDriver.WRITER>>(0);
                set => Link(0, value);
            }

            public bool KernelHooks
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc31878f070a9dc9dUL)]
    public class UnauthorizedDriver : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc31878f070a9dc9dUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            DriverName = reader.DriverName;
            Description = reader.Description;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.DriverName = DriverName;
            writer.Description = Description;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string DriverName
        {
            get;
            set;
        }

        public string Description
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
            public string DriverName => ctx.ReadText(0, null);
            public string Description => ctx.ReadText(1, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 2);
            }

            public string DriverName
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public string Description
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xad6d3453800fb835UL)]
    public class BehaviorAnomaly : ICapnpSerializable
    {
        public const UInt64 typeId = 0xad6d3453800fb835UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            AnomalyType = reader.AnomalyType;
            Value = reader.Value;
            ExpectedRange = reader.ExpectedRange;
            Confidence = reader.Confidence;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.AnomalyType = AnomalyType;
            writer.Value = Value;
            writer.ExpectedRange.Init(ExpectedRange);
            writer.Confidence = Confidence;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.AnomalyType AnomalyType
        {
            get;
            set;
        }

        public float Value
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ExpectedRange
        {
            get;
            set;
        }

        public float Confidence
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
            public CapnpGen.AnomalyType AnomalyType => (CapnpGen.AnomalyType)ctx.ReadDataUShort(0UL, (ushort)0);
            public float Value => ctx.ReadDataFloat(32UL, 0F);
            public IReadOnlyList<byte> ExpectedRange => ctx.ReadList(0).CastByte();
            public float Confidence => ctx.ReadDataFloat(64UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 1);
            }

            public CapnpGen.AnomalyType AnomalyType
            {
                get => (CapnpGen.AnomalyType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public float Value
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public ListOfPrimitivesSerializer<byte> ExpectedRange
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public float Confidence
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa0fcbb594edf9203UL)]
    public enum AnomalyType : ushort
    {
        reactionTime,
        accuracy,
        movementPattern,
        decisionMaking,
        resourceUsage,
        networkPattern
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xeda3cd381d26c3f6UL)]
    public enum CheatType : ushort
    {
        aimAssist,
        wallhack,
        speedhack,
        teleport,
        esp,
        radar,
        triggerbot,
        bunnyhop,
        noRecoil,
        itemSpawn,
        godMode,
        lagSwitch
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbf1acedce3bf9406UL)]
    public enum ReportStatus : ushort
    {
        pending,
        reviewing,
        confirmed,
        falsePositive,
        inconclusive
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaeef123c9c18981fUL), Proxy(typeof(SecurityService_Proxy)), Skeleton(typeof(SecurityService_Skeleton))]
    public interface ISecurityService : IDisposable
    {
        Task<IReadOnlyList<byte>> ReportIncident(CapnpGen.SecurityIncident incident, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task UpdateIncident(IReadOnlyList<byte> incidentId, CapnpGen.SecurityIncident incident, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task ResolveIncident(IReadOnlyList<byte> incidentId, CapnpGen.Resolution resolution, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.BehaviorProfile> GetBehaviorProfile(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default);
        Task ApplySecurityAction(IReadOnlyList<byte> playerId, CapnpGen.SecurityAction action, ulong duration, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<bool> AppealAction(IReadOnlyList<byte> actionId, CapnpGen.Appeal appeal, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<(float, IReadOnlyList<CapnpGen.FraudIndicator>)> DetectFraud(IReadOnlyList<byte> matchId, CapnpGen.FraudDetectionData data, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<CapnpGen.FraudPattern>> GetFraudPatterns(IReadOnlyList<byte> gameId, CancellationToken cancellationToken_ = default);
        Task<IReadOnlyList<byte>> SubmitAntiCheatReport(CapnpGen.AntiCheatReport report, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task ReviewAntiCheatReport(IReadOnlyList<byte> reportId, CapnpGen.Review review, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default);
        Task<CapnpGen.SecurityStatistics> GetSecurityStats(CapnpGen.TimeRange timeRange, CancellationToken cancellationToken_ = default);
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaeef123c9c18981fUL)]
    public class SecurityService_Proxy : Proxy, ISecurityService
    {
        public async Task<IReadOnlyList<byte>> ReportIncident(CapnpGen.SecurityIncident incident, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_ReportIncident.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_ReportIncident()
            {Incident = incident, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 0, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_ReportIncident>(d_);
                return (r_.IncidentId);
            }
        }

        public async Task UpdateIncident(IReadOnlyList<byte> incidentId, CapnpGen.SecurityIncident incident, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_UpdateIncident.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_UpdateIncident()
            {IncidentId = incidentId, Incident = incident, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 1, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_UpdateIncident>(d_);
                return;
            }
        }

        public async Task ResolveIncident(IReadOnlyList<byte> incidentId, CapnpGen.Resolution resolution, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_ResolveIncident.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_ResolveIncident()
            {IncidentId = incidentId, Resolution = resolution, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 2, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_ResolveIncident>(d_);
                return;
            }
        }

        public async Task<CapnpGen.BehaviorProfile> GetBehaviorProfile(IReadOnlyList<byte> playerId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_GetBehaviorProfile.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_GetBehaviorProfile()
            {PlayerId = playerId};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 3, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_GetBehaviorProfile>(d_);
                return (r_.Profile);
            }
        }

        public async Task ApplySecurityAction(IReadOnlyList<byte> playerId, CapnpGen.SecurityAction action, ulong duration, string reason, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_ApplySecurityAction.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_ApplySecurityAction()
            {PlayerId = playerId, Action = action, Duration = duration, Reason = reason, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 4, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_ApplySecurityAction>(d_);
                return;
            }
        }

        public async Task<bool> AppealAction(IReadOnlyList<byte> actionId, CapnpGen.Appeal appeal, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_AppealAction.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_AppealAction()
            {ActionId = actionId, Appeal = appeal, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 5, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_AppealAction>(d_);
                return (r_.Accepted);
            }
        }

        public async Task<(float, IReadOnlyList<CapnpGen.FraudIndicator>)> DetectFraud(IReadOnlyList<byte> matchId, CapnpGen.FraudDetectionData data, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_DetectFraud.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_DetectFraud()
            {MatchId = matchId, Data = data};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 6, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_DetectFraud>(d_);
                return (r_.RiskScore, r_.Indicators);
            }
        }

        public async Task<IReadOnlyList<CapnpGen.FraudPattern>> GetFraudPatterns(IReadOnlyList<byte> gameId, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_GetFraudPatterns.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_GetFraudPatterns()
            {GameId = gameId};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 7, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_GetFraudPatterns>(d_);
                return (r_.Patterns);
            }
        }

        public async Task<IReadOnlyList<byte>> SubmitAntiCheatReport(CapnpGen.AntiCheatReport report, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_SubmitAntiCheatReport.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_SubmitAntiCheatReport()
            {Report = report, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 8, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_SubmitAntiCheatReport>(d_);
                return (r_.ReportId);
            }
        }

        public async Task ReviewAntiCheatReport(IReadOnlyList<byte> reportId, CapnpGen.Review review, IReadOnlyList<byte> signature, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_ReviewAntiCheatReport.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_ReviewAntiCheatReport()
            {ReportId = reportId, Review = review, Signature = signature};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 9, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_ReviewAntiCheatReport>(d_);
                return;
            }
        }

        public async Task<CapnpGen.SecurityStatistics> GetSecurityStats(CapnpGen.TimeRange timeRange, CancellationToken cancellationToken_ = default)
        {
            var in_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Params_GetSecurityStats.WRITER>();
            var arg_ = new CapnpGen.SecurityService.Params_GetSecurityStats()
            {TimeRange = timeRange};
            arg_?.serialize(in_);
            using (var d_ = await Call(12605313933559502879UL, 10, in_.Rewrap<DynamicSerializerState>(), false, cancellationToken_).WhenReturned)
            {
                var r_ = CapnpSerializable.Create<CapnpGen.SecurityService.Result_GetSecurityStats>(d_);
                return (r_.Stats);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaeef123c9c18981fUL)]
    public class SecurityService_Skeleton : Skeleton<ISecurityService>
    {
        public SecurityService_Skeleton()
        {
            SetMethodTable(ReportIncident, UpdateIncident, ResolveIncident, GetBehaviorProfile, ApplySecurityAction, AppealAction, DetectFraud, GetFraudPatterns, SubmitAntiCheatReport, ReviewAntiCheatReport, GetSecurityStats);
        }

        public override ulong InterfaceId => 12605313933559502879UL;
        Task<AnswerOrCounterquestion> ReportIncident(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_ReportIncident>(d_);
                return Impatient.MaybeTailCall(Impl.ReportIncident(in_.Incident, in_.Signature, cancellationToken_), incidentId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_ReportIncident.WRITER>();
                    var r_ = new CapnpGen.SecurityService.Result_ReportIncident{IncidentId = incidentId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> UpdateIncident(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_UpdateIncident>(d_);
                await Impl.UpdateIncident(in_.IncidentId, in_.Incident, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_UpdateIncident.WRITER>();
                return s_;
            }
        }

        async Task<AnswerOrCounterquestion> ResolveIncident(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_ResolveIncident>(d_);
                await Impl.ResolveIncident(in_.IncidentId, in_.Resolution, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_ResolveIncident.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GetBehaviorProfile(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_GetBehaviorProfile>(d_);
                return Impatient.MaybeTailCall(Impl.GetBehaviorProfile(in_.PlayerId, cancellationToken_), profile =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_GetBehaviorProfile.WRITER>();
                    var r_ = new CapnpGen.SecurityService.Result_GetBehaviorProfile{Profile = profile};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> ApplySecurityAction(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_ApplySecurityAction>(d_);
                await Impl.ApplySecurityAction(in_.PlayerId, in_.Action, in_.Duration, in_.Reason, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_ApplySecurityAction.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> AppealAction(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_AppealAction>(d_);
                return Impatient.MaybeTailCall(Impl.AppealAction(in_.ActionId, in_.Appeal, in_.Signature, cancellationToken_), accepted =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_AppealAction.WRITER>();
                    var r_ = new CapnpGen.SecurityService.Result_AppealAction{Accepted = accepted};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> DetectFraud(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_DetectFraud>(d_);
                return Impatient.MaybeTailCall(Impl.DetectFraud(in_.MatchId, in_.Data, cancellationToken_), (riskScore, indicators) =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_DetectFraud.WRITER>();
                    var r_ = new CapnpGen.SecurityService.Result_DetectFraud{RiskScore = riskScore, Indicators = indicators};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> GetFraudPatterns(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_GetFraudPatterns>(d_);
                return Impatient.MaybeTailCall(Impl.GetFraudPatterns(in_.GameId, cancellationToken_), patterns =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_GetFraudPatterns.WRITER>();
                    var r_ = new CapnpGen.SecurityService.Result_GetFraudPatterns{Patterns = patterns};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        Task<AnswerOrCounterquestion> SubmitAntiCheatReport(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_SubmitAntiCheatReport>(d_);
                return Impatient.MaybeTailCall(Impl.SubmitAntiCheatReport(in_.Report, in_.Signature, cancellationToken_), reportId =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_SubmitAntiCheatReport.WRITER>();
                    var r_ = new CapnpGen.SecurityService.Result_SubmitAntiCheatReport{ReportId = reportId};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }

        async Task<AnswerOrCounterquestion> ReviewAntiCheatReport(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_ReviewAntiCheatReport>(d_);
                await Impl.ReviewAntiCheatReport(in_.ReportId, in_.Review, in_.Signature, cancellationToken_);
                var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_ReviewAntiCheatReport.WRITER>();
                return s_;
            }
        }

        Task<AnswerOrCounterquestion> GetSecurityStats(DeserializerState d_, CancellationToken cancellationToken_)
        {
            using (d_)
            {
                var in_ = CapnpSerializable.Create<CapnpGen.SecurityService.Params_GetSecurityStats>(d_);
                return Impatient.MaybeTailCall(Impl.GetSecurityStats(in_.TimeRange, cancellationToken_), stats =>
                {
                    var s_ = SerializerState.CreateForRpc<CapnpGen.SecurityService.Result_GetSecurityStats.WRITER>();
                    var r_ = new CapnpGen.SecurityService.Result_GetSecurityStats{Stats = stats};
                    r_.serialize(s_);
                    return s_;
                }

                );
            }
        }
    }

    public static class SecurityService
    {
        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbc5ab0c5349ca653UL)]
        public class Params_ReportIncident : ICapnpSerializable
        {
            public const UInt64 typeId = 0xbc5ab0c5349ca653UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Incident = CapnpSerializable.Create<CapnpGen.SecurityIncident>(reader.Incident);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Incident?.serialize(writer.Incident);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.SecurityIncident Incident
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
                public CapnpGen.SecurityIncident.READER Incident => ctx.ReadStruct(0, CapnpGen.SecurityIncident.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.SecurityIncident.WRITER Incident
                {
                    get => BuildPointer<CapnpGen.SecurityIncident.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8ca890d18005686bUL)]
        public class Result_ReportIncident : ICapnpSerializable
        {
            public const UInt64 typeId = 0x8ca890d18005686bUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                IncidentId = reader.IncidentId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.IncidentId.Init(IncidentId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> IncidentId
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
                public IReadOnlyList<byte> IncidentId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> IncidentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb43b9532021f13dcUL)]
        public class Params_UpdateIncident : ICapnpSerializable
        {
            public const UInt64 typeId = 0xb43b9532021f13dcUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                IncidentId = reader.IncidentId;
                Incident = CapnpSerializable.Create<CapnpGen.SecurityIncident>(reader.Incident);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.IncidentId.Init(IncidentId);
                Incident?.serialize(writer.Incident);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> IncidentId
            {
                get;
                set;
            }

            public CapnpGen.SecurityIncident Incident
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
                public IReadOnlyList<byte> IncidentId => ctx.ReadList(0).CastByte();
                public CapnpGen.SecurityIncident.READER Incident => ctx.ReadStruct(1, CapnpGen.SecurityIncident.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> IncidentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.SecurityIncident.WRITER Incident
                {
                    get => BuildPointer<CapnpGen.SecurityIncident.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xfa92ac9780efcd74UL)]
        public class Result_UpdateIncident : ICapnpSerializable
        {
            public const UInt64 typeId = 0xfa92ac9780efcd74UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xeb1dfcf2e2cec8aeUL)]
        public class Params_ResolveIncident : ICapnpSerializable
        {
            public const UInt64 typeId = 0xeb1dfcf2e2cec8aeUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                IncidentId = reader.IncidentId;
                Resolution = CapnpSerializable.Create<CapnpGen.Resolution>(reader.Resolution);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.IncidentId.Init(IncidentId);
                Resolution?.serialize(writer.Resolution);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> IncidentId
            {
                get;
                set;
            }

            public CapnpGen.Resolution Resolution
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
                public IReadOnlyList<byte> IncidentId => ctx.ReadList(0).CastByte();
                public CapnpGen.Resolution.READER Resolution => ctx.ReadStruct(1, CapnpGen.Resolution.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> IncidentId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.Resolution.WRITER Resolution
                {
                    get => BuildPointer<CapnpGen.Resolution.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa1d5b4ab04525d13UL)]
        public class Result_ResolveIncident : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa1d5b4ab04525d13UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf29b0fbbfff3cb47UL)]
        public class Params_GetBehaviorProfile : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf29b0fbbfff3cb47UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf532d02f21e119b7UL)]
        public class Result_GetBehaviorProfile : ICapnpSerializable
        {
            public const UInt64 typeId = 0xf532d02f21e119b7UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Profile = CapnpSerializable.Create<CapnpGen.BehaviorProfile>(reader.Profile);
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

            public CapnpGen.BehaviorProfile Profile
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
                public CapnpGen.BehaviorProfile.READER Profile => ctx.ReadStruct(0, CapnpGen.BehaviorProfile.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.BehaviorProfile.WRITER Profile
                {
                    get => BuildPointer<CapnpGen.BehaviorProfile.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa4e47a11ff81c5e0UL)]
        public class Params_ApplySecurityAction : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa4e47a11ff81c5e0UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                PlayerId = reader.PlayerId;
                Action = reader.Action;
                Duration = reader.Duration;
                Reason = reader.Reason;
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.PlayerId.Init(PlayerId);
                writer.Action = Action;
                writer.Duration = Duration;
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

            public IReadOnlyList<byte> PlayerId
            {
                get;
                set;
            }

            public CapnpGen.SecurityAction Action
            {
                get;
                set;
            }

            public ulong Duration
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
                public IReadOnlyList<byte> PlayerId => ctx.ReadList(0).CastByte();
                public CapnpGen.SecurityAction Action => (CapnpGen.SecurityAction)ctx.ReadDataUShort(0UL, (ushort)0);
                public ulong Duration => ctx.ReadDataULong(64UL, 0UL);
                public string Reason => ctx.ReadText(1, null);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
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

                public CapnpGen.SecurityAction Action
                {
                    get => (CapnpGen.SecurityAction)this.ReadDataUShort(0UL, (ushort)0);
                    set => this.WriteData(0UL, (ushort)value, (ushort)0);
                }

                public ulong Duration
                {
                    get => this.ReadDataULong(64UL, 0UL);
                    set => this.WriteData(64UL, value, 0UL);
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x994476947c2f4b02UL)]
        public class Result_ApplySecurityAction : ICapnpSerializable
        {
            public const UInt64 typeId = 0x994476947c2f4b02UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd0f426f7e68cc9a9UL)]
        public class Params_AppealAction : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd0f426f7e68cc9a9UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                ActionId = reader.ActionId;
                Appeal = CapnpSerializable.Create<CapnpGen.Appeal>(reader.Appeal);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.ActionId.Init(ActionId);
                Appeal?.serialize(writer.Appeal);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> ActionId
            {
                get;
                set;
            }

            public CapnpGen.Appeal Appeal
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
                public IReadOnlyList<byte> ActionId => ctx.ReadList(0).CastByte();
                public CapnpGen.Appeal.READER Appeal => ctx.ReadStruct(1, CapnpGen.Appeal.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> ActionId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.Appeal.WRITER Appeal
                {
                    get => BuildPointer<CapnpGen.Appeal.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xaa8606c029c74d71UL)]
        public class Result_AppealAction : ICapnpSerializable
        {
            public const UInt64 typeId = 0xaa8606c029c74d71UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Accepted = reader.Accepted;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Accepted = Accepted;
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public bool Accepted
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
                public bool Accepted => ctx.ReadDataBool(0UL, false);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 0);
                }

                public bool Accepted
                {
                    get => this.ReadDataBool(0UL, false);
                    set => this.WriteData(0UL, value, false);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd22b4d2a798ca714UL)]
        public class Params_DetectFraud : ICapnpSerializable
        {
            public const UInt64 typeId = 0xd22b4d2a798ca714UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                MatchId = reader.MatchId;
                Data = CapnpSerializable.Create<CapnpGen.FraudDetectionData>(reader.Data);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.MatchId.Init(MatchId);
                Data?.serialize(writer.Data);
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

            public CapnpGen.FraudDetectionData Data
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
                public CapnpGen.FraudDetectionData.READER Data => ctx.ReadStruct(1, CapnpGen.FraudDetectionData.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public ListOfPrimitivesSerializer<byte> MatchId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.FraudDetectionData.WRITER Data
                {
                    get => BuildPointer<CapnpGen.FraudDetectionData.WRITER>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe0ad9152b95dc178UL)]
        public class Result_DetectFraud : ICapnpSerializable
        {
            public const UInt64 typeId = 0xe0ad9152b95dc178UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                RiskScore = reader.RiskScore;
                Indicators = reader.Indicators?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.FraudIndicator>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.RiskScore = RiskScore;
                writer.Indicators.Init(Indicators, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public float RiskScore
            {
                get;
                set;
            }

            public IReadOnlyList<CapnpGen.FraudIndicator> Indicators
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
                public float RiskScore => ctx.ReadDataFloat(0UL, 0F);
                public IReadOnlyList<CapnpGen.FraudIndicator.READER> Indicators => ctx.ReadList(0).Cast(CapnpGen.FraudIndicator.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(1, 1);
                }

                public float RiskScore
                {
                    get => this.ReadDataFloat(0UL, 0F);
                    set => this.WriteData(0UL, value, 0F);
                }

                public ListOfStructsSerializer<CapnpGen.FraudIndicator.WRITER> Indicators
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.FraudIndicator.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x98e4fd4a7d412197UL)]
        public class Params_GetFraudPatterns : ICapnpSerializable
        {
            public const UInt64 typeId = 0x98e4fd4a7d412197UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc563b82c2ce33e64UL)]
        public class Result_GetFraudPatterns : ICapnpSerializable
        {
            public const UInt64 typeId = 0xc563b82c2ce33e64UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Patterns = reader.Patterns?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.FraudPattern>(_));
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.Patterns.Init(Patterns, (_s1, _v1) => _v1?.serialize(_s1));
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<CapnpGen.FraudPattern> Patterns
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
                public IReadOnlyList<CapnpGen.FraudPattern.READER> Patterns => ctx.ReadList(0).Cast(CapnpGen.FraudPattern.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfStructsSerializer<CapnpGen.FraudPattern.WRITER> Patterns
                {
                    get => BuildPointer<ListOfStructsSerializer<CapnpGen.FraudPattern.WRITER>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa14b2b4280b4b8d3UL)]
        public class Params_SubmitAntiCheatReport : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa14b2b4280b4b8d3UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Report = CapnpSerializable.Create<CapnpGen.AntiCheatReport>(reader.Report);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                Report?.serialize(writer.Report);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public CapnpGen.AntiCheatReport Report
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
                public CapnpGen.AntiCheatReport.READER Report => ctx.ReadStruct(0, CapnpGen.AntiCheatReport.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(1).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 2);
                }

                public CapnpGen.AntiCheatReport.WRITER Report
                {
                    get => BuildPointer<CapnpGen.AntiCheatReport.WRITER>(0);
                    set => Link(0, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                    set => Link(1, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xae1df7e459d34462UL)]
        public class Result_SubmitAntiCheatReport : ICapnpSerializable
        {
            public const UInt64 typeId = 0xae1df7e459d34462UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                ReportId = reader.ReportId;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.ReportId.Init(ReportId);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> ReportId
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
                public IReadOnlyList<byte> ReportId => ctx.ReadList(0).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public ListOfPrimitivesSerializer<byte> ReportId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbaa9b2b0abe48e9dUL)]
        public class Params_ReviewAntiCheatReport : ICapnpSerializable
        {
            public const UInt64 typeId = 0xbaa9b2b0abe48e9dUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                ReportId = reader.ReportId;
                Review = CapnpSerializable.Create<CapnpGen.Review>(reader.Review);
                Signature = reader.Signature;
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                writer.ReportId.Init(ReportId);
                Review?.serialize(writer.Review);
                writer.Signature.Init(Signature);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
            }

            public IReadOnlyList<byte> ReportId
            {
                get;
                set;
            }

            public CapnpGen.Review Review
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
                public IReadOnlyList<byte> ReportId => ctx.ReadList(0).CastByte();
                public CapnpGen.Review.READER Review => ctx.ReadStruct(1, CapnpGen.Review.READER.create);
                public IReadOnlyList<byte> Signature => ctx.ReadList(2).CastByte();
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 3);
                }

                public ListOfPrimitivesSerializer<byte> ReportId
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                    set => Link(0, value);
                }

                public CapnpGen.Review.WRITER Review
                {
                    get => BuildPointer<CapnpGen.Review.WRITER>(1);
                    set => Link(1, value);
                }

                public ListOfPrimitivesSerializer<byte> Signature
                {
                    get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                    set => Link(2, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xceed1a2ab01933b0UL)]
        public class Result_ReviewAntiCheatReport : ICapnpSerializable
        {
            public const UInt64 typeId = 0xceed1a2ab01933b0UL;
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

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x838fff984e03497cUL)]
        public class Params_GetSecurityStats : ICapnpSerializable
        {
            public const UInt64 typeId = 0x838fff984e03497cUL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                TimeRange = CapnpSerializable.Create<CapnpGen.TimeRange>(reader.TimeRange);
                applyDefaults();
            }

            public void serialize(WRITER writer)
            {
                TimeRange?.serialize(writer.TimeRange);
            }

            void ICapnpSerializable.Serialize(SerializerState arg_)
            {
                serialize(arg_.Rewrap<WRITER>());
            }

            public void applyDefaults()
            {
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
                public CapnpGen.TimeRange.READER TimeRange => ctx.ReadStruct(0, CapnpGen.TimeRange.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.TimeRange.WRITER TimeRange
                {
                    get => BuildPointer<CapnpGen.TimeRange.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa5f2564d2f5f1532UL)]
        public class Result_GetSecurityStats : ICapnpSerializable
        {
            public const UInt64 typeId = 0xa5f2564d2f5f1532UL;
            void ICapnpSerializable.Deserialize(DeserializerState arg_)
            {
                var reader = READER.create(arg_);
                Stats = CapnpSerializable.Create<CapnpGen.SecurityStatistics>(reader.Stats);
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

            public CapnpGen.SecurityStatistics Stats
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
                public CapnpGen.SecurityStatistics.READER Stats => ctx.ReadStruct(0, CapnpGen.SecurityStatistics.READER.create);
            }

            public class WRITER : SerializerState
            {
                public WRITER()
                {
                    this.SetStruct(0, 1);
                }

                public CapnpGen.SecurityStatistics.WRITER Stats
                {
                    get => BuildPointer<CapnpGen.SecurityStatistics.WRITER>(0);
                    set => Link(0, value);
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa725d10fb760fa00UL)]
    public class FraudDetectionData : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa725d10fb760fa00UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            MatchData = reader.MatchData;
            PlayerData = reader.PlayerData?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.PlayerSecurityData>(_));
            NetworkData = CapnpSerializable.Create<CapnpGen.NetworkData>(reader.NetworkData);
            SystemData = CapnpSerializable.Create<CapnpGen.SystemData>(reader.SystemData);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.MatchData.Init(MatchData);
            writer.PlayerData.Init(PlayerData, (_s1, _v1) => _v1?.serialize(_s1));
            NetworkData?.serialize(writer.NetworkData);
            SystemData?.serialize(writer.SystemData);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> MatchData
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.PlayerSecurityData> PlayerData
        {
            get;
            set;
        }

        public CapnpGen.NetworkData NetworkData
        {
            get;
            set;
        }

        public CapnpGen.SystemData SystemData
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
            public IReadOnlyList<byte> MatchData => ctx.ReadList(0).CastByte();
            public IReadOnlyList<CapnpGen.PlayerSecurityData.READER> PlayerData => ctx.ReadList(1).Cast(CapnpGen.PlayerSecurityData.READER.create);
            public CapnpGen.NetworkData.READER NetworkData => ctx.ReadStruct(2, CapnpGen.NetworkData.READER.create);
            public CapnpGen.SystemData.READER SystemData => ctx.ReadStruct(3, CapnpGen.SystemData.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 4);
            }

            public ListOfPrimitivesSerializer<byte> MatchData
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfStructsSerializer<CapnpGen.PlayerSecurityData.WRITER> PlayerData
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.PlayerSecurityData.WRITER>>(1);
                set => Link(1, value);
            }

            public CapnpGen.NetworkData.WRITER NetworkData
            {
                get => BuildPointer<CapnpGen.NetworkData.WRITER>(2);
                set => Link(2, value);
            }

            public CapnpGen.SystemData.WRITER SystemData
            {
                get => BuildPointer<CapnpGen.SystemData.WRITER>(3);
                set => Link(3, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf87aa6d1c2d11885UL)]
    public class PlayerSecurityData : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf87aa6d1c2d11885UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            PlayerId = reader.PlayerId;
            GameplayMetrics = reader.GameplayMetrics;
            BehaviorMetrics = reader.BehaviorMetrics;
            EconomicMetrics = reader.EconomicMetrics;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.PlayerId.Init(PlayerId);
            writer.GameplayMetrics.Init(GameplayMetrics);
            writer.BehaviorMetrics.Init(BehaviorMetrics);
            writer.EconomicMetrics.Init(EconomicMetrics);
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

        public IReadOnlyList<byte> GameplayMetrics
        {
            get;
            set;
        }

        public IReadOnlyList<byte> BehaviorMetrics
        {
            get;
            set;
        }

        public IReadOnlyList<byte> EconomicMetrics
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
            public IReadOnlyList<byte> GameplayMetrics => ctx.ReadList(1).CastByte();
            public IReadOnlyList<byte> BehaviorMetrics => ctx.ReadList(2).CastByte();
            public IReadOnlyList<byte> EconomicMetrics => ctx.ReadList(3).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 4);
            }

            public ListOfPrimitivesSerializer<byte> PlayerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> GameplayMetrics
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<byte> BehaviorMetrics
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }

            public ListOfPrimitivesSerializer<byte> EconomicMetrics
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb3086fe3fb13b167UL)]
    public class NetworkData : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb3086fe3fb13b167UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            LatencyHistory = reader.LatencyHistory;
            PacketLoss = reader.PacketLoss;
            JitterHistory = reader.JitterHistory;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.LatencyHistory.Init(LatencyHistory);
            writer.PacketLoss.Init(PacketLoss);
            writer.JitterHistory.Init(JitterHistory);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<uint> LatencyHistory
        {
            get;
            set;
        }

        public IReadOnlyList<float> PacketLoss
        {
            get;
            set;
        }

        public IReadOnlyList<uint> JitterHistory
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
            public IReadOnlyList<uint> LatencyHistory => ctx.ReadList(0).CastUInt();
            public IReadOnlyList<float> PacketLoss => ctx.ReadList(1).CastFloat();
            public IReadOnlyList<uint> JitterHistory => ctx.ReadList(2).CastUInt();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 3);
            }

            public ListOfPrimitivesSerializer<uint> LatencyHistory
            {
                get => BuildPointer<ListOfPrimitivesSerializer<uint>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<float> PacketLoss
            {
                get => BuildPointer<ListOfPrimitivesSerializer<float>>(1);
                set => Link(1, value);
            }

            public ListOfPrimitivesSerializer<uint> JitterHistory
            {
                get => BuildPointer<ListOfPrimitivesSerializer<uint>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8214dbb4ccd5dc28UL)]
    public class SystemData : ICapnpSerializable
    {
        public const UInt64 typeId = 0x8214dbb4ccd5dc28UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            HardwareInfo = reader.HardwareInfo;
            RunningProcesses = reader.RunningProcesses;
            FileHashes = reader.FileHashes;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.HardwareInfo.Init(HardwareInfo);
            writer.RunningProcesses.Init(RunningProcesses);
            writer.FileHashes.Init(FileHashes, (_s1, _v1) => _s1.Init(_v1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> HardwareInfo
        {
            get;
            set;
        }

        public IReadOnlyList<string> RunningProcesses
        {
            get;
            set;
        }

        public IReadOnlyList<IReadOnlyList<byte>> FileHashes
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
            public IReadOnlyList<byte> HardwareInfo => ctx.ReadList(0).CastByte();
            public IReadOnlyList<string> RunningProcesses => ctx.ReadList(1).CastText2();
            public IReadOnlyList<IReadOnlyList<byte>> FileHashes => ctx.ReadList(2).CastData();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 3);
            }

            public ListOfPrimitivesSerializer<byte> HardwareInfo
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfTextSerializer RunningProcesses
            {
                get => BuildPointer<ListOfTextSerializer>(1);
                set => Link(1, value);
            }

            public ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>> FileHashes
            {
                get => BuildPointer<ListOfPointersSerializer<ListOfPrimitivesSerializer<byte>>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdd1c852cdc45039fUL)]
    public class Appeal : ICapnpSerializable
    {
        public const UInt64 typeId = 0xdd1c852cdc45039fUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            AppealId = reader.AppealId;
            ActionId = reader.ActionId;
            Reason = reader.Reason;
            Evidence = reader.Evidence;
            SubmittedAt = reader.SubmittedAt;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.AppealId.Init(AppealId);
            writer.ActionId.Init(ActionId);
            writer.Reason = Reason;
            writer.Evidence.Init(Evidence);
            writer.SubmittedAt = SubmittedAt;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> AppealId
        {
            get;
            set;
        }

        public IReadOnlyList<byte> ActionId
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

        public ulong SubmittedAt
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
            public IReadOnlyList<byte> AppealId => ctx.ReadList(0).CastByte();
            public IReadOnlyList<byte> ActionId => ctx.ReadList(1).CastByte();
            public string Reason => ctx.ReadText(2, null);
            public IReadOnlyList<byte> Evidence => ctx.ReadList(3).CastByte();
            public ulong SubmittedAt => ctx.ReadDataULong(0UL, 0UL);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 4);
            }

            public ListOfPrimitivesSerializer<byte> AppealId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public ListOfPrimitivesSerializer<byte> ActionId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(1);
                set => Link(1, value);
            }

            public string Reason
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }

            public ListOfPrimitivesSerializer<byte> Evidence
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(3);
                set => Link(3, value);
            }

            public ulong SubmittedAt
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd457205210c4bb8fUL)]
    public class Review : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd457205210c4bb8fUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            ReviewerId = reader.ReviewerId;
            Decision = reader.Decision;
            Notes = reader.Notes;
            Evidence = reader.Evidence;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.ReviewerId.Init(ReviewerId);
            writer.Decision = Decision;
            writer.Notes = Notes;
            writer.Evidence.Init(Evidence);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public IReadOnlyList<byte> ReviewerId
        {
            get;
            set;
        }

        public CapnpGen.ReviewDecision Decision
        {
            get;
            set;
        }

        public string Notes
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
            public IReadOnlyList<byte> ReviewerId => ctx.ReadList(0).CastByte();
            public CapnpGen.ReviewDecision Decision => (CapnpGen.ReviewDecision)ctx.ReadDataUShort(0UL, (ushort)0);
            public string Notes => ctx.ReadText(1, null);
            public IReadOnlyList<byte> Evidence => ctx.ReadList(2).CastByte();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public ListOfPrimitivesSerializer<byte> ReviewerId
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(0);
                set => Link(0, value);
            }

            public CapnpGen.ReviewDecision Decision
            {
                get => (CapnpGen.ReviewDecision)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public string Notes
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public ListOfPrimitivesSerializer<byte> Evidence
            {
                get => BuildPointer<ListOfPrimitivesSerializer<byte>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb8d8f6cd201bb926UL)]
    public enum ReviewDecision : ushort
    {
        uphold,
        overturn,
        reduce,
        escalate
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc988799eebdd6aaaUL)]
    public class SecurityStatistics : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc988799eebdd6aaaUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            TotalIncidents = reader.TotalIncidents;
            ResolvedIncidents = reader.ResolvedIncidents;
            FalsePositives = reader.FalsePositives;
            AvgResolutionTime = reader.AvgResolutionTime;
            CategoryBreakdown = reader.CategoryBreakdown?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.CategoryStats>(_));
            SeverityBreakdown = reader.SeverityBreakdown?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.SeverityStats>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.TotalIncidents = TotalIncidents;
            writer.ResolvedIncidents = ResolvedIncidents;
            writer.FalsePositives = FalsePositives;
            writer.AvgResolutionTime = AvgResolutionTime;
            writer.CategoryBreakdown.Init(CategoryBreakdown, (_s1, _v1) => _v1?.serialize(_s1));
            writer.SeverityBreakdown.Init(SeverityBreakdown, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ulong TotalIncidents
        {
            get;
            set;
        }

        public ulong ResolvedIncidents
        {
            get;
            set;
        }

        public ulong FalsePositives
        {
            get;
            set;
        }

        public ulong AvgResolutionTime
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.CategoryStats> CategoryBreakdown
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.SeverityStats> SeverityBreakdown
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
            public ulong TotalIncidents => ctx.ReadDataULong(0UL, 0UL);
            public ulong ResolvedIncidents => ctx.ReadDataULong(64UL, 0UL);
            public ulong FalsePositives => ctx.ReadDataULong(128UL, 0UL);
            public ulong AvgResolutionTime => ctx.ReadDataULong(192UL, 0UL);
            public IReadOnlyList<CapnpGen.CategoryStats.READER> CategoryBreakdown => ctx.ReadList(0).Cast(CapnpGen.CategoryStats.READER.create);
            public IReadOnlyList<CapnpGen.SeverityStats.READER> SeverityBreakdown => ctx.ReadList(1).Cast(CapnpGen.SeverityStats.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(4, 2);
            }

            public ulong TotalIncidents
            {
                get => this.ReadDataULong(0UL, 0UL);
                set => this.WriteData(0UL, value, 0UL);
            }

            public ulong ResolvedIncidents
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public ulong FalsePositives
            {
                get => this.ReadDataULong(128UL, 0UL);
                set => this.WriteData(128UL, value, 0UL);
            }

            public ulong AvgResolutionTime
            {
                get => this.ReadDataULong(192UL, 0UL);
                set => this.WriteData(192UL, value, 0UL);
            }

            public ListOfStructsSerializer<CapnpGen.CategoryStats.WRITER> CategoryBreakdown
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.CategoryStats.WRITER>>(0);
                set => Link(0, value);
            }

            public ListOfStructsSerializer<CapnpGen.SeverityStats.WRITER> SeverityBreakdown
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.SeverityStats.WRITER>>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8aaf09cc4a11c6e1UL)]
    public class CategoryStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0x8aaf09cc4a11c6e1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Category = reader.Category;
            Count = reader.Count;
            Percentage = reader.Percentage;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Category = Category;
            writer.Count = Count;
            writer.Percentage = Percentage;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.FraudCategory Category
        {
            get;
            set;
        }

        public ulong Count
        {
            get;
            set;
        }

        public float Percentage
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
            public CapnpGen.FraudCategory Category => (CapnpGen.FraudCategory)ctx.ReadDataUShort(0UL, (ushort)0);
            public ulong Count => ctx.ReadDataULong(64UL, 0UL);
            public float Percentage => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public CapnpGen.FraudCategory Category
            {
                get => (CapnpGen.FraudCategory)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ulong Count
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public float Percentage
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe3464696bc3950a1UL)]
    public class SeverityStats : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe3464696bc3950a1UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Severity = reader.Severity;
            Count = reader.Count;
            Percentage = reader.Percentage;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Severity = Severity;
            writer.Count = Count;
            writer.Percentage = Percentage;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.SeverityLevel Severity
        {
            get;
            set;
        }

        public ulong Count
        {
            get;
            set;
        }

        public float Percentage
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
            public CapnpGen.SeverityLevel Severity => (CapnpGen.SeverityLevel)ctx.ReadDataUShort(0UL, (ushort)0);
            public ulong Count => ctx.ReadDataULong(64UL, 0UL);
            public float Percentage => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public CapnpGen.SeverityLevel Severity
            {
                get => (CapnpGen.SeverityLevel)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public ulong Count
            {
                get => this.ReadDataULong(64UL, 0UL);
                set => this.WriteData(64UL, value, 0UL);
            }

            public float Percentage
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }
}