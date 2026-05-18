#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod rust_capnp {
    include!(concat!(env!("OUT_DIR"), "/rust_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_types_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_core_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod amp_telemetry_capnp {
    include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod service_capnp {
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod player_profile_capnp {
    include!(concat!(env!("OUT_DIR"), "/player_profile_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod matchmaking_rules_capnp {
    include!(concat!(env!("OUT_DIR"), "/matchmaking_rules_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_registry_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_registry_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod inventory_capnp {
    include!(concat!(env!("OUT_DIR"), "/inventory_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod tournament_capnp {
    include!(concat!(env!("OUT_DIR"), "/tournament_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod security_capnp {
    include!(concat!(env!("OUT_DIR"), "/security_capnp.rs"));
}

struct TelemetryItem {
    match_id: String,
    event_type: String,
    timestamp: u64,
}

use capnp::message::ReaderOptions;
use capnp::serialize_packed;
use crossterm::{
    event::{self, DisableMouseCapture, EnableMouseCapture, Event, KeyCode},
    execute,
    terminal::{
        EnterAlternateScreen, LeaveAlternateScreen, disable_raw_mode,
        enable_raw_mode,
    },
};
use ratatui::{
    Frame, Terminal,
    backend::{Backend, CrosstermBackend},
    layout::{Constraint, Direction, Layout},
    style::{Color, Modifier, Style},
    widgets::{Block, Borders, Cell, Paragraph, Row, Table, TableState},
};
use std::{error::Error, io::Read};

struct App {
    state: TableState,
    items: Vec<TelemetryItem>,
}

impl App {
    fn new() -> App {
        App {
            state: TableState::default(),
            items: vec![],
        }
    }

    fn load_from_file(&mut self, path: &str) {
        let mut file = match std::fs::File::open(path) {
            Ok(f) => f,
            Err(_) => return,
        };

        loop {
            let mut len_buf = [0u8; 4];
            match file.read_exact(&mut len_buf) {
                Ok(()) => {}
                Err(_) => break,
            }
            let len = u32::from_le_bytes(len_buf) as usize;

            let mut msg_buf = vec![0u8; len];
            if file.read_exact(&mut msg_buf).is_err() {
                break;
            }

            let reader = match serialize_packed::read_message(
                &mut msg_buf.as_slice(),
                ReaderOptions::new(),
            ) {
                Ok(r) => r,
                Err(_) => continue,
            };

            let event = match reader
                .get_root::<amp_telemetry_capnp::amp_telemetry_event::Reader<
                '_,
            >>() {
                Ok(e) => e,
                Err(_) => continue,
            };

            let match_id = match event.get_match_id() {
                Ok(d) => hex::encode(d),
                Err(_) => "unknown".to_string(),
            };

            let event_type = match event.get_event_type() {
                Ok(t) => format!("{:?}", t),
                Err(_) => "unknown".to_string(),
            };

            let timestamp = event.get_timestamp();

            self.items.push(TelemetryItem {
                match_id,
                event_type,
                timestamp,
            });
        }

        if !self.items.is_empty() {
            self.state.select(Some(0));
        }
    }

    pub fn next(&mut self) {
        if self.items.is_empty() {
            return;
        }
        let i = match self.state.selected() {
            Some(i) => {
                if i >= self.items.len() - 1 {
                    0
                } else {
                    i + 1
                }
            }
            None => 0,
        };
        self.state.select(Some(i));
    }

    pub fn previous(&mut self) {
        let i = match self.state.selected() {
            Some(i) => {
                if i == 0 {
                    self.items.len() - 1
                } else {
                    i - 1
                }
            }
            None => 0,
        };
        self.state.select(Some(i));
    }
}

fn main() -> Result<(), Box<dyn Error>> {
    enable_raw_mode()?;
    let mut stdout = std::io::stdout();
    execute!(stdout, EnterAlternateScreen, EnableMouseCapture)?;
    let backend = CrosstermBackend::new(stdout);
    let mut terminal = Terminal::new(backend)?;

    let mut app = App::new();

    let path = std::env::args()
        .nth(1)
        .unwrap_or_else(|| "telemetry.bin".to_string());
    app.load_from_file(&path);

    let res = run_app(&mut terminal, app);

    disable_raw_mode()?;
    execute!(
        terminal.backend_mut(),
        LeaveAlternateScreen,
        DisableMouseCapture
    )?;
    terminal.show_cursor()?;

    if let Err(err) = res {
        println!("{:?}", err)
    }

    Ok(())
}

fn run_app<B: Backend>(
    terminal: &mut Terminal<B>,
    mut app: App,
) -> std::io::Result<()> {
    loop {
        terminal.draw(|f| ui(f, &mut app))?;

        if let Event::Key(key) = event::read()? {
            match key.code {
                KeyCode::Char('q') => return Ok(()),
                KeyCode::Down | KeyCode::Char('j') => app.next(),
                KeyCode::Up | KeyCode::Char('k') => app.previous(),
                _ => {}
            }
        }
    }
}

fn ui(f: &mut Frame, app: &mut App) {
    let rects = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Percentage(80), Constraint::Percentage(20)])
        .split(f.area());

    let selected_style = Style::default()
        .add_modifier(Modifier::REVERSED)
        .fg(Color::Cyan);
    let normal_style = Style::default().bg(Color::Black);

    let header_cells = vec!["Match ID", "Event Type", "Timestamp"]
        .into_iter()
        .map(|h| Cell::from(h).style(Style::default().fg(Color::Yellow)));

    let header = Row::new(header_cells)
        .style(normal_style)
        .height(1)
        .bottom_margin(1);

    let rows: Vec<Row> = app
        .items
        .iter()
        .map(|item| {
            let cells = vec![
                Cell::from(item.match_id.clone()),
                Cell::from(item.event_type.clone()),
                Cell::from(item.timestamp.to_string()),
            ];
            Row::new(cells).height(1).bottom_margin(0)
        })
        .collect();

    let widths = [
        Constraint::Percentage(33),
        Constraint::Percentage(33),
        Constraint::Percentage(33),
    ];

    let t = Table::new(rows, widths)
        .header(header)
        .block(
            Block::default()
                .borders(Borders::ALL)
                .title("AMP Telemetry Trace Viewer (Press q to quit)"),
        )
        .highlight_style(selected_style)
        .highlight_symbol(">> ");

    f.render_stateful_widget(t, rects[0], &mut app.state);

    let info_text = if let Some(idx) = app.state.selected() {
        if let Some(item) = app.items.get(idx) {
            format!(
                "Details for Select Event:\nMatch ID: {}\nEvent: {}\nTime: {}",
                item.match_id, item.event_type, item.timestamp
            )
        } else {
            "No selection".to_string()
        }
    } else {
        "Select an event to view details.".to_string()
    };

    let p = Paragraph::new(info_text)
        .block(Block::default().title("Inspector").borders(Borders::ALL));
    f.render_widget(p, rects[1]);
}
