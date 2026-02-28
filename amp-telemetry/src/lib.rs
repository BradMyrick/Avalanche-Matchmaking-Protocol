use once_cell::sync::OnceCell;
use opentelemetry::metrics::Meter;
use opentelemetry::trace::TracerProvider;
use opentelemetry::{KeyValue, global};
use opentelemetry_sdk::propagation::TraceContextPropagator;
use opentelemetry_sdk::resource::Resource;
use opentelemetry_sdk::trace::SdkTracerProvider;
use thiserror::Error;
use tracing::Level;
use tracing_opentelemetry::OpenTelemetryLayer;
use tracing_subscriber::Registry;
use tracing_subscriber::filter::EnvFilter;
use tracing_subscriber::layer::SubscriberExt;

/// Core config for telemetry initialization.
#[derive(Clone, Debug)]
pub struct TelemetryConfig {
    /// Logical service name, e.g. "amp-matchmaker", "amp-messaging-gateway".
    pub service_name: String,
    /// OTLP endpoint, e.g. "http://otel-collector:4317" or "http://localhost:4318".
    pub otlp_endpoint: String,
    /// Environment label, e.g. "dev", "staging", "prod".
    pub environment: String,
    /// Global sampling ratio, 0.0–1.0.
    pub trace_sample_ratio: f64,
}

/// Errors during telemetry init.
#[derive(Debug, Error)]
pub enum TelemetryError {
    #[error("failed to create trace pipeline: {0}")]
    TracePipeline(String),
    #[error("failed to create metrics pipeline: {0}")]
    MetricsPipeline(String),
}

struct GlobalMeters {
    pub meter: Meter,
}

static GLOBAL_METERS: OnceCell<GlobalMeters> = OnceCell::new();

pub fn init(config: TelemetryConfig) -> Result<(), TelemetryError> {
    let resource = Resource::builder_empty()
        .with_attributes([
            KeyValue::new("service.name", config.service_name.clone()),
            KeyValue::new("deployment.environment", config.environment.clone()),
            KeyValue::new("telemetry.sdk.language", "rust"),
            KeyValue::new("amp", "true"),
        ])
        .build();

    let span_exporter = opentelemetry_otlp::SpanExporter::builder()
        .with_tonic()
        .build()
        .expect("Failed to build span_exporter with tonic");

    let tracer_provider = SdkTracerProvider::builder()
        .with_resource(resource)
        .with_batch_exporter(span_exporter)
        .build();

    let tracer = tracer_provider.tracer("amp-tracer");

    // ---- Global registration ----
    let otel_layer = OpenTelemetryLayer::new(tracer.clone());

    let filter = EnvFilter::try_from_default_env()
        .unwrap_or_else(|_| EnvFilter::new("info,tower_http=info,h2=info"));

    let subscriber = Registry::default()
        .with(filter)
        .with(
            tracing_subscriber::fmt::layer()
                .with_target(false)
                .with_level(true),
        )
        .with(otel_layer);

    tracing::subscriber::set_global_default(subscriber).expect("setting default subscriber failed");

    //
    // TODO: add meters/metering
    //

    global::set_text_map_propagator(TraceContextPropagator::new());
    global::set_tracer_provider(tracer_provider);
    Ok(())
}
/// Shutdown telemetry pipelines gracefully. Call on shutdown.
pub fn shutdown() {
    //TODO: Flush/close providers.
    //TODO: shutdown isn't a function I can call
}

pub fn match_span(
    game_id: u32,
    match_id: u64,
    settlement_mode: &str,
    player_id: Option<&str>,
) -> tracing::Span {
    let span = tracing::span!(
        Level::INFO,
        "amp.match",
        amp.game_id = game_id,
        amp.match_id = match_id,
        amp.settlement_mode = settlement_mode,
        amp.player_id = player_id.unwrap_or(""),
    );
    span
}

/// Convenience: span for a Cap'n Proto RPC message.
pub fn messaging_span(
    game_id: u32,
    match_id: Option<u64>,
    message_type: &str,
    schema_id: &str,
) -> tracing::Span {
    let span = tracing::span!(
        Level::DEBUG,
        "amp.messaging",
        amp.game_id = game_id,
        amp.match_id = match_id.unwrap_or(0),
        amp.message_type = message_type,
        amp.schema_id = schema_id,
    );
    span
}

/// Counter: total received messages per service / game.
pub fn incr_message_counter(game_id: u32, message_type: &str) {
    if let Some(globals) = GLOBAL_METERS.get() {
        let counter = globals
            .meter
            .u64_counter("amp_messages_total")
            .with_description("Total messages processed by AMP components")
            .build();

        counter.add(
            1,
            &[
                KeyValue::new("amp.game_id", game_id.to_string()),
                KeyValue::new("amp.message_type", message_type.to_string()),
            ],
        );
    }
}

/// Gauge: set current connected players for a service.
/// Call this periodically (or on connect/disconnect).
pub fn set_connected_players(count: u64, game_id: Option<u32>) {
    if let Some(globals) = GLOBAL_METERS.get() {
        //TODO:
    }
}
