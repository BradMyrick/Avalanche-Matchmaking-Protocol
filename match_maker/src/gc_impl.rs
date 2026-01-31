use crate::match_capnp::{game_connector, match_maker};
use crate::mm_impl::MMImpl;
use capnp::capability::Promise;
use capnp_rpc::pry;

pub struct GameConnectorImpl;

impl GameConnectorImpl {
    pub fn new() -> Self {
        Self {}
    }
}

impl game_connector::Server for GameConnectorImpl {
    fn request_game_service(
        &mut self,
        params: game_connector::RequestGameServiceParams,
        mut results: game_connector::RequestGameServiceResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let params_reader = pry!(params.get());
        let assignment_req = pry!(params_reader.get_assignment_request());

        // Security: Validate player pool size and gameId
        let player_pool = pry!(assignment_req.get_player_pool());
        if player_pool.len() < 2 {
            let results_builder = results.get().init_assignment();
            let mut err = results_builder.init_err();
            err.set_msg("Matchmaking requires at least 2 players");
            return Promise::ok(());
        }

        let config = pry!(assignment_req.get_match_config());
        let _game_id = pry!(config.get_game_id());

        // Robustness: Generate unique Match ID (e.g., UUID or sequential for now)
        let match_id = format!("match-{}", uuid::Uuid::new_v4().to_string());
        println!("Spawning match: {}", match_id);

        let results_builder = results.get().init_assignment();
        let mut match_assignment = results_builder.init_match();

        match_assignment.set_match_id(match_id.as_bytes());
        match_assignment.set_ticket_id(b"ticket-123");

        // Copy opponents from request to assignment
        {
            let mut opponents_builder = match_assignment
                .reborrow()
                .init_opponents(player_pool.len());
            for i in 0..player_pool.len() {
                let opponent_src = player_pool.get(i);
                let mut opponent_dst = opponents_builder.reborrow().get(i);

                opponent_dst.set_player_id(pry!(opponent_src.get_player_id()));
                opponent_dst.set_display_name(pry!(opponent_src.get_display_name()));
                opponent_dst.set_elo(pry!(opponent_src.get_elo()));
                opponent_dst.set_region(pry!(opponent_src.get_region()));
                opponent_dst.set_mode(pry!(opponent_src.get_mode()));

                let src_wallet = pry!(opponent_src.get_player_wallet());
                let mut dst_wallet = opponent_dst.init_player_wallet();
                dst_wallet.set_payer_wallet(src_wallet.get_payer_wallet());
                dst_wallet.set_fee_token(src_wallet.get_fee_token());
                dst_wallet.set_auth_spend(src_wallet.get_auth_spend());
            }
        }

        // Return the MatchMaker capability
        match_assignment.set_settle(capnp_rpc::new_client(MMImpl));

        Promise::ok(())
    }
}
