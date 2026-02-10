use crate::service_capnp::match_session;
use capnp::capability::Promise;
use capnp_rpc::pry;

pub struct MatchSessionImpl {
    match_id: Vec<u8>,
    player_id: Vec<u8>,
}

impl MatchSessionImpl {
    pub fn new(match_id: &[u8], player_id: &[u8]) -> Self {
        Self {
            match_id: match_id.to_vec(),
            player_id: player_id.to_vec(),
        }
    }
}

impl match_session::Server for MatchSessionImpl {
    fn submit_outcome(
        &mut self,
        params: match_session::SubmitOutcomeParams,
        mut results: match_session::SubmitOutcomeResults,
    ) -> Promise<(), capnp::Error> {
        let submission = pry!(pry!(params.get()).get_submission());

        // Basic verification stub
        let _match_id = pry!(submission.get_match_id());

        println!(
            "Received outcome submission for match {:?} from player {:?}",
            self.match_id, self.player_id
        );

        results.get().set_accepted(true);
        Promise::ok(())
    }

    fn subscribe_to_events(
        &mut self,
        _params: match_session::SubscribeToEventsParams,
        _results: match_session::SubscribeToEventsResults,
    ) -> Promise<(), capnp::Error> {
        // Todo: Implement event subscription
        Promise::ok(())
    }
}
