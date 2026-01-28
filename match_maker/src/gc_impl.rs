use crate::match_capnp::game_connector;
use capnp::capability::Promise;
pub struct GameConnectorImpl;

impl GameConnectorImpl {
    pub fn new() -> Self {
        Self {}
    }
}

impl game_connector::Server for GameConnectorImpl {
    fn request_game_service(
        &mut self,
        _: game_connector::RequestGameServiceParams,
        _: game_connector::RequestGameServiceResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        Promise::ok(())
    }
}
