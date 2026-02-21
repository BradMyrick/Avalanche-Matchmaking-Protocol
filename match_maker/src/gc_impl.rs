use crate::mm_impl::MatchSessionImpl;
use crate::service_capnp::{game_session_service, user_session};
use capnp::capability::Promise;
use capnp_rpc::pry;

// -- Game Session Service (The Entry Point) --
pub struct GameSessionServiceImpl;

impl GameSessionServiceImpl {
    pub fn new() -> Self {
        Self {}
    }
}

impl game_session_service::Server for GameSessionServiceImpl {
    fn login(
        &mut self,
        params: game_session_service::LoginParams,
        mut results: game_session_service::LoginResults,
    ) -> Promise<(), capnp::Error> {
        let _signature = pry!(pry!(params.get()).get_signed_challenge());
        // TODO: Validate signature

        let user_session_client = capnp_rpc::new_client(UserSessionImpl::new());
        results.get().set_session(user_session_client);

        Promise::ok(())
    }
}

// -- User Session (Authenticated Context) --
pub struct UserSessionImpl;

impl UserSessionImpl {
    pub fn new() -> Self {
        Self {}
    }
}

impl user_session::Server for UserSessionImpl {
    fn request_match(
        &mut self,
        params: user_session::RequestMatchParams,
        mut results: user_session::RequestMatchResults,
    ) -> Promise<(), capnp::Error> {
        let req = pry!(pry!(params.get()).get_req());
        let _game_id = pry!(req.get_game_id());

        // Stub: Immediately match with a dummy opponent
        let match_id = format!("match-{}", uuid::Uuid::new_v4().to_string());
        println!("Spawning match: {}", match_id);

        let mut results_builder = results.get();
        let mut assignment = results_builder.reborrow().init_assignment();

        assignment.set_match_id(match_id.as_bytes());
        // ...TODO Populate other assignment fields stub ...

        let session_client = capnp_rpc::new_client(MatchSessionImpl::new(
            match_id.as_bytes(),
            // TODO: Pass in player name
            b"player1",
        ));
        results_builder.set_session(session_client);

        Promise::ok(())
    }

    fn reconnect(
        &mut self,
        _params: user_session::ReconnectParams,
        _results: user_session::ReconnectResults,
    ) -> Promise<(), capnp::Error> {
        Promise::err(capnp::Error::failed("Not implemented".to_string()))
    }
}
