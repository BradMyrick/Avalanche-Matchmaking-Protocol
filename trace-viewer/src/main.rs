pub mod rust_capnp {
    include!(concat!(env!("OUT_DIR"), "/rust_capnp.rs"));
}
pub mod game_types_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
pub mod game_core_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs"));
}
pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
pub mod amp_telemetry_capnp {
    include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs"));
}
pub mod service_capnp {
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}

struct TelemetryItem {
    match_id: String,
    event_type: String,
    timestamp: u64,
}

use crossterm::{
    event::{self, DisableMouseCapture, EnableMouseCapture, Event, KeyCode},
    execute,
    terminal::{disable_raw_mode, enable_raw_mode, EnterAlternateScreen, LeaveAlternateScreen},
};
use ratatui::{
    backend::{Backend, CrosstermBackend},
    layout::{Constraint, Direction, Layout},
    style::{Color, Modifier, Style},
    widgets::{Block, Borders, Cell, Paragraph, Row, Table, TableState},
    Frame, Terminal,
};
use std::{error::Error, io};
// use std::fs;
// use capnp::serialize_packed;

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

    // In a real scenario, this reads from parsed Cap'n proto binaries
    fn add_mock_data(&mut self) {
        self.items = vec![
            TelemetryItem {
                match_id: "m_1001".to_string(),
                event_type: "matchCreated".to_string(),
                timestamp: 1710000000,
            },
            TelemetryItem {
                match_id: "m_1001".to_string(),
                event_type: "matchJoined".to_string(),
                timestamp: 1710000010,
            },
            TelemetryItem {
                match_id: "m_1002".to_string(),
                event_type: "matchCreated".to_string(),
                timestamp: 1710000015,
            },
            TelemetryItem {
                match_id: "m_1001".to_string(),
                event_type: "settlementSubmitted".to_string(),
                timestamp: 1710000040,
            },
        ];
    }

    pub fn next(&mut self) {
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
    // setup terminal
    enable_raw_mode()?;
    let mut stdout = io::stdout();
    execute!(stdout, EnterAlternateScreen, EnableMouseCapture)?;
    let backend = CrosstermBackend::new(stdout);
    let mut terminal = Terminal::new(backend)?;

    let mut app = App::new();
    app.add_mock_data();

    // run app
    let res = run_app(&mut terminal, app);

    // restore terminal
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

fn run_app<B: Backend>(terminal: &mut Terminal<B>, mut app: App) -> io::Result<()> {
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

    let p =
        Paragraph::new(info_text).block(Block::default().title("Inspector").borders(Borders::ALL));
    f.render_widget(p, rects[1]);
}
